using JasperFx.Core.IoC;
using Wolverine.Http;
using Zss.BilliardHall.Application;
using Zss.BilliardHall.Composition;
using Zss.BilliardHall.Platform;
using Zss.BilliardHall.Platform.Contracts;
using Zss.BilliardHall.Platform.Errors;

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

        // 4. Host 层服务（异常映射器等）
        builder.Services.AddSingleton<IExceptionProblemDetailsMapper, ExceptionProblemDetailsMapper>();

        // 注册异常转换器链（将技术层异常转换为领域异常）
        builder.Services.AddSingleton<IExceptionTranslator, FluentValidationExceptionTranslator>();

        // 5. 自动注册错误模块 (IErrorModule)
        builder.Services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(c => c.AssignableTo<IErrorModule>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());
    }

    /// <summary>
    /// 配置应用程序管道
    /// </summary>
    public static void ConfigureApplication(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // 初始化错误注册 (ErrorRegistry)
        using (var scope = app.Services.CreateScope())
        {
            var modules = scope.ServiceProvider.GetServices<IErrorModule>();
            foreach (var module in modules)
            {
                module.Register();
            }
            ErrorRegistry.Freeze();
        }

        // 全局异常处理中间件（统一将所有未处理异常转换为 ProblemDetails）
        // 这个中间件是后备，处理其他可能漏掉的异常
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // 映射 Wolverine HTTP 端点
        app.MapWolverineEndpoints();

        app.Logger.LogInformation("Host 管道配置完成");
    }
}
