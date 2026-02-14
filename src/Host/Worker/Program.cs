using Serilog;
using Zss.BilliardHall.Host.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog();

HostBootstrapper.ConfigureServices(builder);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
