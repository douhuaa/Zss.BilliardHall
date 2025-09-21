using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using BilliardHall.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Port=3306;Database=billiard_hall;User=billiard;Password=billiard123;";

builder.Services.AddDbContext<BilliardHallDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"))
    .AddDbContextCheck<BilliardHallDbContext>("database")
    .AddCheck("redis", () => 
    {
        // Simplified Redis health check for now
        try 
        {
            // TODO: Implement actual Redis connection check
            return HealthCheckResult.Healthy("Redis connection simulated");
        }
        catch
        {
            return HealthCheckResult.Unhealthy("Redis connection failed");
        }
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// Basic API endpoints for Sprint 0
app.MapGet("/", () => new { 
    service = "Billiard Hall Management System",
    version = "v0.1-sprint0",
    status = "running",
    timestamp = DateTime.UtcNow
})
.WithName("GetServiceInfo")
.WithOpenApi();

app.MapGet("/api/stores", async (BilliardHallDbContext db) =>
{
    var stores = await db.Stores.Where(s => s.IsDeleted == 0).ToListAsync();
    return Results.Ok(stores);
})
.WithName("GetStores")
.WithOpenApi();

app.MapGet("/api/tables", async (BilliardHallDbContext db, long? storeId = null) =>
{
    var query = db.BilliardTables.Where(t => t.IsDeleted == 0);
    if (storeId.HasValue)
        query = query.Where(t => t.StoreId == storeId.Value);
    
    var tables = await query.ToListAsync();
    return Results.Ok(tables);
})
.WithName("GetTables")
.WithOpenApi();

// Event tracking endpoint (basic implementation)
app.MapPost("/api/track", async (BilliardHallDbContext db, TrackingRequest request) =>
{
    foreach (var eventData in request.Events)
    {
        var eventLog = new BilliardHall.Domain.EventLog
        {
            EventType = eventData.EventType,
            BizId = eventData.BizId,
            UserId = eventData.UserId,
            SessionId = eventData.SessionId,
            Payload = System.Text.Json.JsonSerializer.Serialize(eventData.Payload),
            OccurredAt = eventData.OccurredAt ?? DateTime.UtcNow
        };
        
        db.EventLogs.Add(eventLog);
    }
    
    await db.SaveChangesAsync();
    
    return Results.Ok(new { 
        success = true, 
        processed_count = request.Events.Count,
        failed_events = new object[0]
    });
})
.WithName("TrackEvents")
.WithOpenApi();

app.Run();

// DTOs for API
public record TrackingRequest(List<EventData> Events);

public record EventData(
    string EventType,
    string? BizId = null,
    long? UserId = null,
    long? SessionId = null,
    object? Payload = null,
    DateTime? OccurredAt = null
);