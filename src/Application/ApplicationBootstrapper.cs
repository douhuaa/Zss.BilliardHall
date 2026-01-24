namespace Zss.BilliardHall.Application;

public static class ApplicationBootstrapper
{
    public static void Configure(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration, Microsoft.Extensions.Hosting.IHostEnvironment environment)
    {
        // 注册 Wolverine/Marten/模块扫描/通用 Pipeline
        // 示例：
        // services.AddWolverine(...);
        // services.AddMarten(...).IntegrateWithWolverine();
        // 模块扫描可在此处根据 Module Marker 自动注册
    }
}
