using Wolverine.Http;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// Web Host 层 Bootstrapper
/// 负责配置 ASP.NET Core 管道和端点映射
/// </summary>
public static class WebHostBootstrapper
{
    /// <summary>
    /// 配置 Web 应用程序管道
    /// </summary>
    public static void Configure(WebApplication app)
    {
        // 映射 Wolverine HTTP 端点
        app.MapWolverineEndpoints();
    }
}

