// *******************************************************
// Platform layer constraints
// - Platform 层禁止引用任何 Application 项目
//   (Platform must not reference any Application projects)
// - Platform 层不允许出现 Endpoint / Middleware
//   (Platform must not contain Endpoint or Middleware types)
//
// 目的：确保 Platform 只负责启动/基础配置，不包含业务用例或 HTTP 端点实现。
// Reason: Platform should only host bootstrapping and infra-level defaults.
// *******************************************************

using Serilog;

namespace Zss.BilliardHall.Wolverine.Bootstrapper;

public static class PlatformDefaults
{
    public static void AddPlatformDefaults(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());


        builder.AddServiceDefaults();
        builder.AddMartenDefaults();
        builder.AddWolverineDefaults();
        builder.ConfigureWolverineModuleDiscovery();
    }
}
