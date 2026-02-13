using Serilog;
using Zss.BilliardHall.Application;
using Zss.BilliardHall.Host.Web;
using Zss.BilliardHall.Platform;

var builder = WebApplication.CreateBuilder(args);

// 使用 Serilog 替换默认日志
builder.Host.UseSerilog();

PlatformBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);

// Host 层决定加载哪些模块（类型安全）
var moduleAssemblies = ModuleRegistry.GetEnabledAssemblies(builder.Configuration);
ApplicationBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment, moduleAssemblies);

var app = builder.Build();

WebHostBootstrapper.Configure(app);
app.Run();
