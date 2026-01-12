using Microsoft.Extensions.Hosting;
using Wolverine;
using Zss.BilliardHall.Modules.Members;

namespace Zss.BilliardHall.Wolverine.Bootstrapper;

/// <summary>
/// Wolverine 配置扩展
/// Wolverine configuration extensions
/// </summary>
public static class WolverineConfigurationExtensions
{
    /// <summary>
    /// 配置 Wolverine 扫描所有模块程序集
    /// Configure Wolverine to scan all module assemblies
    /// </summary>
    public static IHostApplicationBuilder ConfigureWolverineModuleDiscovery(this IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureWolverine(opts =>
        {
            // 扫描 Members 模块程序集
            // Scan Members module assembly
            opts.Discovery.IncludeAssembly(typeof(Member).Assembly);
            
            // 未来添加其他模块时，在此处添加：
            // Add other modules here as they are implemented:
            // opts.Discovery.IncludeAssembly(typeof(SomeOtherModule).Assembly);
        });

        return builder;
    }
}
