var builder = DistributedApplication.CreateBuilder(args);

// Fixed SA password so mssql extension can connect with known credentials
var sqlPassword = builder.AddParameter("sql-password", "YourStrong!Pass123", secret: true);

// SQL Server on fixed host port 1434 (avoids conflict with any local SQL Server on 1433)
var database = builder.AddSqlServer("sqlserver", password: sqlPassword, port: 1434)
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("demodb");

// Redis cache
var cache = builder.AddRedis("cache");

// ASP.NET Core API Service
var apiService = builder.AddProject<Projects.backenddemo_ApiService>("apiservice")
    .WithHttpEndpoint()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache);

// React Frontend (Vite)
var frontend = builder.AddViteApp("frontend", "../frontend")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithEnvironment("VITE_API_BASE_URL", apiService.GetEndpoint("http"));

frontend.WaitFor(apiService);

builder.Build().Run();
