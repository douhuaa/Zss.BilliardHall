using Zss.BilliardHall.Application;
using Zss.BilliardHall.Platform;

var builder = WebApplication.CreateBuilder(args);

PlatformBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);
ApplicationBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();

// 配置中间件管道
PlatformBootstrapper.ConfigureMiddleware(app, app.Environment);
ApplicationBootstrapper.ConfigureMiddleware(app);

// 映射端点
ApplicationBootstrapper.MapEndpoints(app);

app.Run();
