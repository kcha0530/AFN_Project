using backenddemo.ApiService.Data;
using backenddemo.ApiService.Middleware;
using backenddemo.ApiService.Models;
using backenddemo.ApiService.Repositories;
using backenddemo.ApiService.Services;
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

// ── Database ─────────────────────────────────────────────────────────────────
var dbConnectionString = builder.Configuration.GetConnectionString("demodb") ?? string.Empty;
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dbConnectionString));

// ── JWT ───────────────────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings(
    builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing"),
    builder.Configuration["Jwt:Issuer"] ?? "backenddemo.ApiService",
    builder.Configuration["Jwt:Audience"] ?? "backenddemo.Web",
    int.TryParse(builder.Configuration["Jwt:ExpireMinutes"], out var minutes) ? minutes : 60
);
builder.Services.AddSingleton(jwtSettings);

var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidIssuer              = jwtSettings.Issuer,
        ValidateAudience         = true,
        ValidAudience            = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = secretKey,
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.FromMinutes(1)
    };
});
builder.Services.AddAuthorization();

// ── Flight Architecture (Repository → Service) ────────────────────────────────
builder.Services.AddScoped<IFlightRepository, FlightRepository>();
builder.Services.AddScoped<IFlightService, FlightService>();

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window      = TimeSpan.FromMinutes(1),
            QueueLimit  = 0
        });
    });
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { success = false, message = "Too many requests. Please try again later." },
            cancellationToken: token);
    };
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            var host = new Uri(origin).Host;
            return host == "localhost"
                || host == "127.0.0.1"
                || host.EndsWith(".localhost"); // Aspire dev proxy
        })
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// ── Controllers + Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Startup: migrate + seed ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    for (var attempt = 1; attempt <= 5; attempt++)
    {
        try { db.Database.Migrate(); break; }
        catch (Exception ex) when (attempt < 5)
        {
            Console.Error.WriteLine($"[Startup] Migration attempt {attempt}/5 failed: {ex.Message}. Retrying in 3 s...");
            Thread.Sleep(3000);
        }
    }

    // Seed admin user
    if (!db.Users.Any())
    {
        db.Users.Add(new User
        {
            Username     = "krit",
            Email        = "krit@demo.com",
            FullName     = "Krit Chaiyabud",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("krit"),
            IsActive     = true
        });
        db.SaveChanges();
    }

    // Seed sample flights
    if (!db.Flights.Any())
    {
        var now = DateTime.UtcNow.Date;
        db.Flights.AddRange(
            new Flight { AirlineName="Qantas",             FlightNumber="QF401",  AircraftType="Boeing 787",  FromCity="Melbourne",  ToCity="Bangkok",    DepartureTime=now.AddDays(1).AddHours(10), ArrivalTime=now.AddDays(1).AddHours(16), Price=750,  Currency="AUD", TotalSeats=180, AvailableSeats=35,  Status="Scheduled", Terminal="2", Gate="G14", IsRefundable=true,  CabinClass="Economy",  CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow },
            new Flight { AirlineName="Singapore Airlines", FlightNumber="SQ210",  AircraftType="Airbus A380", FromCity="Melbourne",  ToCity="Singapore",  DepartureTime=now.AddDays(1).AddHours(8),  ArrivalTime=now.AddDays(1).AddHours(14), Price=920,  Currency="AUD", TotalSeats=400, AvailableSeats=120, Status="Scheduled", Terminal="2", Gate="G22", IsRefundable=true,  CabinClass="Economy",  CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow },
            new Flight { AirlineName="Qantas",             FlightNumber="QF8",    AircraftType="Boeing 787",  FromCity="Melbourne",  ToCity="London",     DepartureTime=now.AddDays(2).AddHours(21), ArrivalTime=now.AddDays(3).AddHours(5),  Price=2400, Currency="AUD", TotalSeats=200, AvailableSeats=48,  Status="Scheduled", Terminal="2", Gate="G7",  IsRefundable=true,  CabinClass="Business", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow },
            new Flight { AirlineName="Jetstar",            FlightNumber="JQ511",  AircraftType="Airbus A320", FromCity="Sydney",     ToCity="Melbourne",  DepartureTime=now.AddDays(1).AddHours(7),  ArrivalTime=now.AddDays(1).AddHours(9),  Price=149,  Currency="AUD", TotalSeats=160, AvailableSeats=60,  Status="Scheduled", Terminal="1", Gate="C5",  IsRefundable=false, CabinClass="Economy",  CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow },
            new Flight { AirlineName="Virgin Australia",   FlightNumber="VA112",  AircraftType="Boeing 737",  FromCity="Brisbane",   ToCity="Sydney",     DepartureTime=now.AddDays(1).AddHours(9),  ArrivalTime=now.AddDays(1).AddHours(10), Price=189,  Currency="AUD", TotalSeats=160, AvailableSeats=80,  Status="Scheduled", Terminal="1", Gate="B3",  IsRefundable=true,  CabinClass="Economy",  CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow },
            new Flight { AirlineName="Emirates",           FlightNumber="EK403",  AircraftType="Airbus A380", FromCity="Sydney",     ToCity="Dubai",      DepartureTime=now.AddDays(1).AddHours(22), ArrivalTime=now.AddDays(2).AddHours(6),  Price=1850, Currency="AUD", TotalSeats=500, AvailableSeats=200, Status="Scheduled", Terminal="1", Gate="A10", IsRefundable=true,  CabinClass="Economy",  CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow },
            new Flight { AirlineName="Thai Airways",       FlightNumber="TG461",  AircraftType="Airbus A350", FromCity="Bangkok",    ToCity="Melbourne",  DepartureTime=now.AddDays(2).AddHours(23), ArrivalTime=now.AddDays(3).AddHours(10), Price=880,  Currency="AUD", TotalSeats=300, AvailableSeats=0,   Status="Scheduled", Terminal="1", Gate="D2",  IsRefundable=true,  CabinClass="Economy",  CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow },
            new Flight { AirlineName="Air Asia",           FlightNumber="AK124",  AircraftType="Airbus A320", FromCity="Kuala Lumpur",ToCity="Bangkok",    DepartureTime=now.AddDays(1).AddHours(6),  ArrivalTime=now.AddDays(1).AddHours(7),  Price=95,   Currency="MYR", TotalSeats=180, AvailableSeats=90,  Status="Scheduled", Terminal="2", Gate="H8",  IsRefundable=false, CabinClass="Economy",  CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow }
        );
        foreach (var f in db.Flights.Local) f.RecalculateDuration();
        db.SaveChanges();
    }
}

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode  = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { success = false, message = "Internal Server Error", status = 500 });
    });
});

app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new { success = false, message = "Route not found", status = 404 });
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Flight Management API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Flight Management API";
});

app.UseCors("AllowFrontend");
app.UseSecurityHeaders();
app.UseRateLimiter();
app.UseRequestLogging();
app.UseRequestTiming();
app.UseHeaderValidation();
app.UseAuthentication();
app.UseAuthorization();

// ── Minimal API: health + auth + users + dashboard ───────────────────────────

app.MapGet("/", () => Results.Json(new { success = true, message = "Flight Management API Running", version = "1.0" }));

app.MapGet("/health", async (ApplicationDbContext db) =>
{
    var userCount   = await db.Users.CountAsync();
    var flightCount = await db.Flights.CountAsync(f => !f.IsDeleted);
    return Results.Json(new ApiHealthDto("Healthy", "1.0.0", DateTime.UtcNow, "PostgreSQL"));
});

// Auth
app.MapPost("/auth/login", async (UserLogin login, ApplicationDbContext db) =>
{
    if (login is null || string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
        return Results.BadRequest(new { success = false, message = "Username and password are required." });

    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == login.Username);
    if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
        return Results.Unauthorized();

    if (!user.IsActive)
        return Results.Json(new { success = false, message = "Account is inactive." }, statusCode: 401);

    var token = CreateJwtToken(user.Username, user.Id.ToString(), jwtSettings);
    return Results.Ok(new { success = true, token, username = user.Username, userId = user.Id, email = user.Email });
});

app.MapPost("/auth/register", async (UserRegisterRequest request, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Email))
        return Results.BadRequest(new { success = false, message = "Username, email, and password are required." });

    var exists = await db.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email);
    if (exists)
        return Results.BadRequest(new { success = false, message = "Username or email already exists." });

    var newUser = new User
    {
        Username     = request.Username,
        Email        = request.Email,
        FullName     = request.FullName ?? string.Empty,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        IsActive     = true
    };
    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{newUser.Id}", new { success = true, message = "User registered.", data = new UserDto(newUser.Id, newUser.Username, newUser.Email, newUser.FullName, newUser.CreatedAt) });
});

app.MapPost("/auth/change-password", [Authorize] async (ChangePasswordRequest request, ClaimsPrincipal claims, ApplicationDbContext db) =>
{
    var username = claims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    if (username == null) return Results.Unauthorized();

    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
    if (user == null) return Results.NotFound(new { success = false, message = "User not found" });
    if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        return Results.BadRequest(new { success = false, message = "Current password is incorrect" });
    if (request.NewPassword.Length < 4)
        return Results.BadRequest(new { success = false, message = "New password must be at least 4 characters" });

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
    user.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true, message = "Password changed successfully" });
});

// Users
app.MapGet("/users", [Authorize] async (ApplicationDbContext db) =>
{
    var users = await db.Users
        .Where(u => u.IsActive)
        .Select(u => new UserDto(u.Id, u.Username, u.Email, u.FullName, u.CreatedAt))
        .ToListAsync();
    return Results.Json(new { success = true, data = users });
});

app.MapGet("/users/me", [Authorize] async (ClaimsPrincipal claims, ApplicationDbContext db) =>
{
    var username = claims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    if (username == null) return Results.Unauthorized();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
    if (user == null) return Results.NotFound(new { success = false, message = "User not found" });
    return Results.Json(new { success = true, data = new UserDto(user.Id, user.Username, user.Email, user.FullName, user.CreatedAt) });
});

// Dashboard
app.MapGet("/dashboard/stats", [Authorize] async (ApplicationDbContext db) =>
{
    var totalUsers     = await db.Users.CountAsync();
    var activeUsers    = await db.Users.CountAsync(u => u.IsActive);
    var totalFlights   = await db.Flights.CountAsync(f => !f.IsDeleted);
    var activeFlights  = await db.Flights.CountAsync(f => !f.IsDeleted && f.Status != "Cancelled");
    var availFlights   = await db.Flights.CountAsync(f => !f.IsDeleted && f.AvailableSeats > 0);

    return Results.Json(new { success = true, data = new DashboardStatsDto(totalUsers, totalFlights, activeFlights, availFlights, DateTime.UtcNow) });
});

// Controllers (flights)
app.MapControllers();

app.MapDefaultEndpoints();
app.MapFallback(() => Results.NotFound(new { success = false, message = "Route not found" }));

app.Run();

static string CreateJwtToken(string username, string userId, JwtSettings jwtSettings)
{
    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, username),
        new("UserId", userId),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
    var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
    var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token  = new JwtSecurityToken(
        issuer:            jwtSettings.Issuer,
        audience:          jwtSettings.Audience,
        claims:            claims,
        expires:           DateTime.UtcNow.AddMinutes(jwtSettings.ExpireMinutes),
        signingCredentials: creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
}
