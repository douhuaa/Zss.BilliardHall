using Wolverine.Http;
using Zss.BilliardHall.BuildingBlocks.Exceptions;

namespace Zss.BilliardHall.Wolverine.Bootstrapper;

/// <summary>
/// Host 层决定：
/// 这是一个什么宿主（HTTP / Worker / Processor）
/// 暴露哪些 Endpoint
/// 使用哪些 Middleware
/// 这是唯一接触 ASP.NET Pipeline 的地方。
/// </summary>
public static class HttpHost
{
    public static WebApplication Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        builder.AddPlatformDefaults();
        builder.AddApplicationModules();


        var app = builder.Build();


        app.MapDefaultEndpoints();
        app.UseMiddleware<DomainExceptionMiddleware>();
        app.MapWolverineEndpoints();

        app.MapGet("/", () => Results.Ok());

        return app;
    }
}
