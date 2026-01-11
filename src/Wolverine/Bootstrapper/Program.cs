var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (service discovery, resilience, health checks, OpenTelemetry)
builder.AddServiceDefaults();

var app = builder.Build();

// Map default endpoints (health checks, aliveness checks)
app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
