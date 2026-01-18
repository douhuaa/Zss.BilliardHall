using Zss.BilliardHall.Application;
using Zss.BilliardHall.Host.Worker;
using Zss.BilliardHall.Platform;

var builder = Host.CreateApplicationBuilder(args);

PlatformDefaults.Configure(builder);
Application.Configure(builder.Services, builder.Configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
