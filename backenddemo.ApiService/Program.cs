using backenddemo.ApiService.Data;
using backenddemo.ApiService.Middleware;
using backenddemo.ApiService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Configuration.AddEnvironmentVariables();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings(
    builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing"),
    builder.Configuration["Jwt:Issuer"] ?? "backenddemo.ApiService",
    builder.Configuration["Jwt:Audience"] ?? "backenddemo.Web",
    int.TryParse(builder.Configuration["Jwt:ExpireMinutes"], out var minutes) ? minutes : 60
);

builder.Services.AddSingleton(jwtSettings);

var dbConnectionString = builder.Configuration.GetConnectionString("demodb") ?? string.Empty;
Console.WriteLine($"DB Connection string: {dbConnectionString}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(dbConnectionString);
});

var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = secretKey,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new { error = "Too many requests. Please try again later." }, cancellationToken: token);
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            var host = new Uri(origin).Host;
            return host == "localhost"
                || host == "127.0.0.1"
                || host.EndsWith(".localhost"); // Aspire dev proxy (e.g. frontend-backenddemo.dev.localhost)
        })
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Run EF Core migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    for (var attempt = 1; attempt <= 5; attempt++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex) when (attempt < 5)
        {
            Console.Error.WriteLine($"[Startup] Migration attempt {attempt}/5 failed: {ex.Message}. Retrying in 3 s...");
            Thread.Sleep(3000);
        }
    }

    // Seed products
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new Product(1, "Keyboard", 50),
            new Product(2, "Mouse", 30),
            new Product(3, "Monitor 27\"", 299),
            new Product(4, "USB-C Hub", 45)
        );
        db.SaveChanges();
    }
    
    // Seed test user
    if (!db.Users.Any())
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("krit");
        db.Users.Add(new User
        {
            Username = "krit",
            Email = "krit@demo.com",
            FullName = "Krit Chaiyabud",
            PasswordHash = hashedPassword,
            IsActive = true
        });
        db.SaveChanges();
    }
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Internal Server Error",
            status = 500
        });
    });
});

app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Route not found",
            status = 404
        });
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowFrontend");
app.UseSecurityHeaders();
app.UseRateLimiter();
app.UseRequestLogging();
app.UseRequestTiming();
app.UseHeaderValidation();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Json(new { message = "Backend Running" }));

app.MapGet("/health", async (ApplicationDbContext db) =>
{
    var userCount = await db.Users.CountAsync();
    var productCount = await db.Products.CountAsync();
    return Results.Json(new ApiHealthDto(
        Status: "Healthy",
        Version: "1.0.0",
        Timestamp: DateTime.UtcNow,
        Database: "PostgreSQL"
    ));
});

// ============ AUTHENTICATION ENDPOINTS ============

app.MapPost("/auth/register", async (UserRegisterRequest request, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest(new { error = "Username, email, and password are required." });
    }

    var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);
    if (existingUser != null)
    {
        return Results.BadRequest(new { error = "Username or email already exists." });
    }

    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
    var newUser = new User
    {
        Username = request.Username,
        Email = request.Email,
        FullName = request.FullName,
        PasswordHash = hashedPassword,
        IsActive = true
    };

    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    var userDto = new UserDto(newUser.Id, newUser.Username, newUser.Email, newUser.FullName, newUser.CreatedAt);
    return Results.Created($"/users/{newUser.Id}", userDto);
});

app.MapPost("/auth/login", async (UserLogin login, ApplicationDbContext db) =>
{
    if (login is null || string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
    {
        return Results.BadRequest(new { error = "Username and password are required." });
    }

    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == login.Username);
    if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    if (!user.IsActive)
    {
        return Results.Json(new { error = "User account is inactive." }, statusCode: StatusCodes.Status401Unauthorized);
    }

    var token = CreateJwtToken(user.Username, user.Id.ToString(), jwtSettings);
    return Results.Ok(new { token, username = user.Username, userId = user.Id, email = user.Email });
});

// ============ USER ENDPOINTS ============

app.MapGet("/users/{id}", [Authorize] async (int id, ApplicationDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user == null)
    {
        return Results.NotFound(new { error = "User not found" });
    }

    var userDto = new UserDto(user.Id, user.Username, user.Email, user.FullName, user.CreatedAt);
    return Results.Json(userDto);
});

app.MapGet("/users", [Authorize] async (ApplicationDbContext db) =>
{
    var users = await db.Users
        .Where(u => u.IsActive)
        .Select(u => new UserDto(u.Id, u.Username, u.Email, u.FullName, u.CreatedAt))
        .ToListAsync();
    return Results.Json(users);
});

app.MapPut("/users/{id}", [Authorize] async (int id, UserProfileUpdateRequest request, ApplicationDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user == null)
    {
        return Results.NotFound(new { error = "User not found" });
    }

    user.Email = request.Email ?? user.Email;
    user.FullName = request.FullName ?? user.FullName;
    user.UpdatedAt = DateTime.UtcNow;

    db.Users.Update(user);
    await db.SaveChangesAsync();

    var userDto = new UserDto(user.Id, user.Username, user.Email, user.FullName, user.CreatedAt);
    return Results.Json(userDto);
});

// ============ PRODUCT ENDPOINTS ============

app.MapGet("/products", async (ApplicationDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    return Results.Json(products);
});

app.MapGet("/products/{id}", async (int id, ApplicationDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    return product is null
        ? Results.NotFound(new { message = "Product not found" })
        : Results.Json(product);
});

app.MapPost("/products", [Authorize] async (Product product, ApplicationDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id}", [Authorize] async (int id, Product updatedProduct, ApplicationDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null)
    {
        return Results.NotFound(new { message = "Product not found" });
    }

    db.Entry(product).CurrentValues.SetValues(updatedProduct);
    await db.SaveChangesAsync();
    return Results.Ok(updatedProduct);
});

app.MapDelete("/products/{id}", [Authorize] async (int id, ApplicationDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null)
    {
        return Results.NotFound(new { message = "Product not found" });
    }

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Product deleted" });
});

// ============ DASHBOARD ENDPOINTS ============

app.MapGet("/dashboard/stats", [Authorize] async (ApplicationDbContext db) =>
{
    var totalUsers = await db.Users.CountAsync();
    var activeUsers = await db.Users.Where(u => u.IsActive).CountAsync();
    var totalProducts = await db.Products.CountAsync();

    return Results.Json(new DashboardStatsDto(
        TotalUsers: totalUsers,
        TotalProducts: totalProducts,
        ActiveUsers: activeUsers,
        LastUpdated: DateTime.UtcNow
    ));
});

// ============ PRODUCT SEARCH ENDPOINT ============

app.MapGet("/products/search", async (string? q, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(q))
        return Results.Json(Array.Empty<object>());

    var results = await db.Products
        .Where(p => p.Name.ToLower().Contains(q.ToLower()))
        .ToListAsync();
    return Results.Json(results);
});

// ============ PRODUCT STATS ENDPOINT ============

app.MapGet("/products/stats", [Authorize] async (ApplicationDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    if (!products.Any())
        return Results.Json(new ProductStatsDto(0, 0m, 0m, 0m));

    return Results.Json(new ProductStatsDto(
        TotalCount: products.Count,
        AveragePrice: Math.Round(products.Average(p => p.Price), 2),
        MinPrice: products.Min(p => p.Price),
        MaxPrice: products.Max(p => p.Price)
    ));
});

// ============ CURRENT USER ENDPOINT ============

app.MapGet("/users/me", [Authorize] async (ClaimsPrincipal claims, ApplicationDbContext db) =>
{
    var username = claims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (username == null)
        return Results.Unauthorized();

    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
    if (user == null)
        return Results.NotFound(new { error = "User not found" });

    return Results.Json(new UserDto(user.Id, user.Username, user.Email, user.FullName, user.CreatedAt));
});

// ============ CHANGE PASSWORD ENDPOINT ============

app.MapPost("/auth/change-password", [Authorize] async (ChangePasswordRequest request, ClaimsPrincipal claims, ApplicationDbContext db) =>
{
    var username = claims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (username == null)
        return Results.Unauthorized();

    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
    if (user == null)
        return Results.NotFound(new { error = "User not found" });

    if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        return Results.BadRequest(new { error = "Current password is incorrect" });

    if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 4)
        return Results.BadRequest(new { error = "New password must be at least 4 characters" });

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
    user.UpdatedAt = DateTime.UtcNow;
    db.Users.Update(user);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Password changed successfully" });
});

// ============ DEACTIVATE USER ENDPOINT ============

app.MapDelete("/users/{id}", [Authorize] async (int id, ApplicationDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user == null)
        return Results.NotFound(new { error = "User not found" });

    user.IsActive = false;
    user.UpdatedAt = DateTime.UtcNow;
    db.Users.Update(user);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "User deactivated successfully" });
});

// ============ TEST/SECURE ENDPOINTS ============

app.MapGet("/hello", () => Results.Json(new { message = "Hello Interns" }));

app.MapGet("/secure", [Authorize] () => Results.Ok(new { message = "Protected Route Accessed" }));

app.MapFallback(() => Results.NotFound(new { error = "Route not found" }));

app.MapDefaultEndpoints();

app.Run();

static string CreateJwtToken(string username, string userId, JwtSettings jwtSettings)
{
    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim("UserId", userId),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpireMinutes);

    var token = new JwtSecurityToken(
        issuer: jwtSettings.Issuer,
        audience: jwtSettings.Audience,
        claims: claims,
        expires: expires,
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}





