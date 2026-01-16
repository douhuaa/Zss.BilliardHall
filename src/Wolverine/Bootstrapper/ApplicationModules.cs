
using Wolverine;
using Zss.BilliardHall.Modules.Members;

namespace Zss.BilliardHall.Wolverine.Bootstrapper;

/// <summary>
/// Application module helpers for the bootstrapper.
/// These methods intentionally do NOT reference any Application projects and
/// only register lightweight markers/options so that higher-level modules can
/// detect the intended application mode (HTTP vs Worker) at startup.
/// 业务模块（Vertical Slice）注册
/// Wolverine Handler / Endpoint 扫描
/// 领域异常、业务策略
/// Platform 层禁止引用 Application 项目，因此这些方法保持为轻量标记。
/// </summary>
public static class ApplicationModules
{
    // .net 10+ 支持在静态类中定义扩展方法
    extension<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        /// <summary>
        /// Register application module markers. Does not load or reference application assemblies.
        /// </summary>
        public TBuilder AddApplicationModules()
        {
            // Marker registration: application projects can consume ApplicationOptions from DI
            builder.Services.AddSingleton(new ApplicationOptions { IsWorker = false });
            builder.AddWolverineModuleDiscovery();
            return builder;
        }

        private TBuilder AddWolverineModuleDiscovery()
        {
            builder.Services.ConfigureWolverine(opts =>
            {
                // 扫描 Members 模块程序集
                opts.Discovery.IncludeAssembly(typeof(Member).Assembly);

                // 未来添加其他模块时，在此处添加：
                // opts.Discovery.IncludeAssembly(typeof(SomeOtherModule).Assembly);
            });
            return builder;
        }
    }
}

/// <summary>
/// Lightweight options/marker used by application projects to detect the intended host mode.
/// Stored in DI so Application layer can read it without Platform referencing Application.
/// </summary>
public sealed class ApplicationOptions
{
    /// <summary>
    /// True when the application should run as a worker/background service (no HTTP endpoints).
    /// False for HTTP applications.
    /// </summary>
    public bool IsWorker { get; init; }
}
