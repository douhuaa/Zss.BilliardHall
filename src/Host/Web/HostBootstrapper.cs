using Wolverine.Http;
using Zss.BilliardHall.Application;
using Zss.BilliardHall.Composition;
using Zss.BilliardHall.Platform;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// Host 层统一 Bootstrapper
/// 职责：编排 Platform → Application → Modules 的初始化
/// 冻结：永不修改
/// </summary>
public static class HostBootstrapper
{
    /// <summary>
    /// 配置服务容器
    /// </summary>
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // 1. Platform 层（日志、遥测等基础设施）
        PlatformBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);

        // 2. 通过 Composition Root 获取启用的模块
        var modules = ModuleComposition.GetEnabledModules(builder.Configuration);

        // 3. Application 层（Wolverine、Marten、DI 装配）
        ApplicationBootstrapper.Configure(
            builder.Services,
            builder.Configuration,
            builder.Environment,
            modules);
    }

    /// <summary>
    /// 配置应用程序管道
    /// </summary>
    public static void ConfigureApplication(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // 添加验证异常处理中间件（将 FluentValidation 错误转换为 ProblemDetails）
        app.UseMiddleware<ValidationExceptionMiddleware>();

        // 映射 Wolverine HTTP 端点
        app.MapWolverineEndpoints();

        app.Logger.LogInformation("Host 管道配置完成");
    }
}
