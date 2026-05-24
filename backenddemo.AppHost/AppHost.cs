var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database
var database = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("demodb");

// Add Redis cache
var cache = builder.AddRedis("cache");

// Add ASP.NET Core API Service with PostgreSQL
var apiService = builder.AddProject<Projects.backenddemo_ApiService>("apiservice")
    .WithHttpEndpoint()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache);

// Add React Frontend (Vite app)
var frontend = builder.AddNpmApp("frontend", "../my-app")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithEnvironment("VITE_API_BASE_URL", apiService.GetEndpoint("http"));

// Wait for frontend to be ready
frontend.WaitFor(apiService);

builder.Build().Run();
