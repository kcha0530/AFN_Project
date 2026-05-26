var builder = DistributedApplication.CreateBuilder(args);

// SQL Server database
var database = builder.AddSqlServer("sqlserver")
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
