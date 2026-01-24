using Serilog;
using Zss.BilliardHall.Application;
using Zss.BilliardHall.Host.Worker;
using Zss.BilliardHall.Platform;

var builder = Host.CreateApplicationBuilder(args);

// 使用 Serilog 替换默认日志
builder.Services.AddSerilog();

PlatformBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);
ApplicationBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
