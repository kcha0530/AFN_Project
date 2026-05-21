using backenddemo.ApiService.Middleware;
using backenddemo.ApiService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings(
    builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing"),
    builder.Configuration["Jwt:Issuer"] ?? "backenddemo.ApiService",
    builder.Configuration["Jwt:Audience"] ?? "backenddemo.Web",
    int.TryParse(builder.Configuration["Jwt:ExpireMinutes"], out var minutes) ? minutes : 60
);

builder.Services.AddSingleton(jwtSettings);

var dbConnectionString = builder.Configuration.GetConnectionString("Default") ?? string.Empty;
Console.WriteLine($"DB Connection string: {dbConnectionString}");

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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowFrontend");
app.UseSecurityHeaders();
app.UseRequestLogging();
app.UseRequestTiming();
app.UseHeaderValidation();
app.UseAuthentication();
app.UseAuthorization();

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

var products = new List<Product>
{
    new Product(1, "Keyboard", 50),
    new Product(2, "Mouse", 30)
};

app.MapGet("/", () => Results.Json(new { message = "Backend Running" }));

app.MapGet("/hello", () => Results.Json(new { message = "Hello Interns" }));

app.MapGet("/products", () => Results.Json(products));

app.MapGet("/products/{id}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is null
        ? Results.NotFound(new { message = "Product not found" })
        : Results.Json(product);
});

app.MapPost("/products", (Product product) =>
{
    products.Add(product);
    return Results.Created($"/products/{product.Id}", product);
});

app.MapDelete("/products/{id}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null)
    {
        return Results.NotFound(new { message = "Product not found" });
    }

    products.Remove(product);
    return Results.Ok(new { message = "Product deleted" });
});

app.MapPut("/products/{id}", (int id, Product updatedProduct) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null)
    {
        return Results.NotFound(new { message = "Product not found" });
    }

    products.Remove(product);
    products.Add(updatedProduct);
    return Results.Ok(updatedProduct);
});

app.MapPost("/login", (UserLogin login) =>
{
    if (login is null || string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
    {
        return Results.BadRequest(new { error = "Username and password are required." });
    }

    if (login.Username != "admin" || login.Password != "P@ssw0rd")
    {
        return Results.Unauthorized();
    }

    var token = CreateJwtToken(login.Username, jwtSettings);
    return Results.Ok(new { token });
});

app.MapGet("/secure", SecureRoute);

app.MapFallback(() => Results.NotFound(new { error = "Route not found" }));

app.Run();

[Authorize]
static IResult SecureRoute() => Results.Ok(new { message = "Protected Route Accessed" });

static string CreateJwtToken(string username, JwtSettings jwtSettings)
{
    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, username),
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





