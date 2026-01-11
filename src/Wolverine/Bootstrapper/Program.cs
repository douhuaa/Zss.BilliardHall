var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMartenDefaults();
builder.AddWolverineDefaults();


var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
