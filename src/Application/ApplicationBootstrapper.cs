using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Zss.BilliardHall.Application;

public static class ApplicationBootstrapper
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        // 注册 Wolverine/Marten/模块扫描/通用 Pipeline
        // 示例：
        // services.AddWolverine(...);
        // services.AddMarten(...).IntegrateWithWolverine();
        // 模块扫描可在此处根据 Module Marker 自动注册
    }

    /// <summary>
    /// 配置应用层中间件和端点映射
    /// </summary>
    public static void ConfigureMiddleware(
        IApplicationBuilder app
    )
    {
        // 应用层中间件
        // 例如：认证、授权
        // app.UseAuthentication();
        // app.UseAuthorization();
    }

    /// <summary>
    /// 映射应用端点
    /// </summary>
    public static void MapEndpoints(
        IEndpointRouteBuilder endpoints
    )
    {
        // 自动发现并注册模块端点
        // 示例：通过约定扫描所有实现 IEndpoint 的类型
        // 或使用 Wolverine.HTTP 自动发现
        
        // 临时示例端点
        endpoints.MapGet("/", () => new
        {
            Application = "Zss.BilliardHall",
            Status = "Running",
            Timestamp = DateTime.UtcNow
        });
    }
}
