using Zss.BilliardHall.Wolverine.ServiceDefaults;

namespace Zss.BilliardHall.Wolverine.Bootstrapper;

/// <summary>
/// HTTP 应用主机构建器（轻量标记）
/// </summary>
public static class HttpHost
{
    /// <summary>
    /// 构建并运行 HTTP 应用
    /// </summary>
    public static WebApplication Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. 平台层配置（基础设施、日志、健康检查等）
        builder.AddPlatformDefaults();

        // 2. 应用模块注册（业务模块、Wolverine、Marten 等）
        builder.AddApplicationModules();

        var app = builder.Build();

        // 3. HTTP 管道配置
        app.MapDefaultEndpoints();

        return app;
    }
}
