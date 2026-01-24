using Serilog;
using Zss.BilliardHall.Application;
using Zss.BilliardHall.Platform;

var builder = WebApplication.CreateBuilder(args);

// 使用 Serilog 替换默认日志
builder.Host.UseSerilog();

PlatformBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);
ApplicationBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();

app.Run();
