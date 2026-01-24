using Zss.BilliardHall.Application;
using Zss.BilliardHall.Host.Worker;
using Zss.BilliardHall.Platform;

var builder = Host.CreateApplicationBuilder(args);

PlatformBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);
ApplicationBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
