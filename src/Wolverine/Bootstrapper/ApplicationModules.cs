
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
    extension<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        /// <summary>
        /// Register application module markers. Does not load or reference application assemblies.
        /// </summary>
        public TBuilder AddApplicationModules()
        {
            // Marker registration: application projects can consume ApplicationOptions from DI
            builder.Services.AddSingleton(new ApplicationOptions { IsWorker = false });
            return builder;
        }

        /// <summary>
        /// Configure the host as an HTTP application.
        /// This method does not map endpoints or reference endpoint types (Platform constraint).
        /// It only registers a marker indicating HTTP mode.
        /// </summary>
        public TBuilder AddHttpApplication()
        {
            builder.Services.AddSingleton(new ApplicationOptions { IsWorker = false });
            return builder;
        }

        /// <summary>
        /// Configure the host as a Worker (background) application.
        /// This method sets a marker indicating worker mode. No endpoints or middleware are added here.
        /// </summary>
        public TBuilder AddWorkerApplication()
        {
            builder.Services.AddSingleton(new ApplicationOptions { IsWorker = true });
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
