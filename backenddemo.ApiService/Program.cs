using backenddemo.ApiService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"];

var connectionString =
    builder.Configuration["ConnectionStrings:Default"];
    
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
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
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request Path: {context.Request.Path}");

    await next();
});

app.Use(async (context, next) =>
{
    var start = DateTime.Now;

    await next();

    var end = DateTime.Now;

    Console.WriteLine($"Request Time: {(end - start).TotalMilliseconds} ms");
});

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path == "/" || path == "/hello" || path.StartsWithSegments("/swagger") || path.StartsWithSegments("/swagger-ui"))
    {
        await next();
        return;
    }

    if (!context.Request.Headers.ContainsKey("x-api-key"))
    {
        context.Response.StatusCode = 400;

        await context.Response.WriteAsJsonAsync(new
        {
            error = "API Key Missing"
        });

        return;
    }

    await next();
});

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;

        await context.Response.WriteAsJsonAsync(new
        {
            error = "Something went wrong"
        });
    });
});


var products = new List<Product>
{
    new Product(1, "Keyboard", 50),
    new Product(2, "Mouse", 30)
};

app.MapGet("/", () =>
{
    return Results.Json(new
    {
        message = "Backend Running"
    });
});

app.MapGet("/hello", () =>
{
    return Results.Json(new
    {
        message = "Hello Interns"
    });
});

app.MapGet("/products", () =>
{
    return Results.Json(products);
});

app.MapGet("/products/{id}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);

    if (product == null)
    {
        return Results.NotFound(new
        {
            message = "Product not found"
        });
    }

    return Results.Json(product);
});

app.MapPost("/products", (Product product) =>
{
    products.Add(product);

    return Results.Created($"/products/{product.Id}", product);
});

app.MapDelete("/products/{id}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);

    if (product == null)
    {
        return Results.NotFound(new
        {
            message = "Product not found"
        });
    }

    products.Remove(product);

    return Results.Ok(new
    {
        message = "Product deleted"
    });
});

app.MapPut("/products/{id}", (int id, Product updatedProduct) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);

    if (product == null)
    {
        return Results.NotFound(new
        {
            message = "Product not found"
        });
    }

    products.Remove(product);
    products.Add(updatedProduct);

    return Results.Ok(updatedProduct);
});

app.MapPost("/login", () =>
{
    return Results.Ok(new
    {
        token = "fake-jwt-token"
    });
});

app.MapGet("/secure", () =>
{
    return Results.Ok(new
    {
        message = "Protected Route Accessed"
    });
}).RequireAuthorization();

app.MapFallback(() =>
{
    return Results.NotFound(new
    {
        error = "Route not found"
    });
});

app.Run();





