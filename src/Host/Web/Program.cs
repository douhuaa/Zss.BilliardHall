using Serilog;
using Wolverine.Http;
using Zss.BilliardHall.Application;
using Zss.BilliardHall.Platform;

var builder = WebApplication.CreateBuilder(args);

// 使用 Serilog 替换默认日志
builder.Host.UseSerilog();

PlatformBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);

// 显式提供模块程序集清单
var moduleAssemblies = new[]
{
    typeof(Zss.BilliardHall.Modules.Members.ModuleMarker).Assembly,
};

ApplicationBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment, moduleAssemblies);

var app = builder.Build();

// 映射 Wolverine HTTP 端点
app.MapWolverineEndpoints();

// 添加健康检查端点
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
