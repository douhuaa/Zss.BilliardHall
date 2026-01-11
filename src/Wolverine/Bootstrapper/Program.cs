var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddMartenDefaults(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
