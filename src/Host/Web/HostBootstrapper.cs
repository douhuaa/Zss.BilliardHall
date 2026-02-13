using Wolverine.Http;
using Zss.BilliardHall.Application;
using Zss.BilliardHall.Platform;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// Host 层统一 Bootstrapper
/// 封装所有初始化逻辑，简化 Program.cs
/// </summary>
public static class HostBootstrapper
{
    /// <summary>
    /// 配置服务容器
    /// </summary>
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // 1. 配置 Platform 层（基础设施）
        PlatformBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);

        // 2. Host 层决定加载哪些模块（类型安全）
        var moduleAssemblies = ModuleRegistry.GetEnabledAssemblies(builder.Configuration);

        // 3. 配置 Application 层（业务装配）
        ApplicationBootstrapper.Configure(
            builder.Services,
            builder.Configuration,
            builder.Environment,
            moduleAssemblies);
    }

    /// <summary>
    /// 配置应用程序管道
    /// </summary>
    public static void ConfigureApplication(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // 映射 Wolverine HTTP 端点
        app.MapWolverineEndpoints();

        app.Logger.LogInformation("Host 管道配置完成");
    }
}
