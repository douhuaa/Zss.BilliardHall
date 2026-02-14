using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;
using Zss.BilliardHall.Platform.Contracts;

namespace Zss.BilliardHall.Application;

/// <summary>
/// Application 层 Bootstrapper
/// 职责：装配 Wolverine、Marten、DI 容器
/// 输入：显式的 IModule[] 实例（由 Host 层提供）
/// 不依赖反射，完全显式
/// 冻结规范：应仅被 HostBootstrapper 调用，不支持扩展
/// </summary>
public static class ApplicationBootstrapper
{
    /// <summary>
    /// 配置应用程序服务
    /// </summary>
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        IModule[] modules)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(modules);

        var enableHttp = configuration.GetValue("Wolverine:Http:Enabled", true);

        ConfigureMarten(services, configuration, modules);
        ConfigureWolverine(services, enableHttp, modules);
        ConfigureModules(services, configuration, environment, modules);
    }

    /// <summary>
    /// 配置 Marten（EF Core 替代品）+ Wolverine 集成
    /// 调用所有模块的 IMartenModule.ConfigureMarten()
    /// </summary>
    private static void ConfigureMarten(IServiceCollection services, IConfiguration configuration, IModule[] modules)
    {
        var connectionString = GetRequiredConnectionString(configuration, "Postgres");

        services
            .AddMarten(opts =>
            {
                opts.Connection(connectionString);

                // 调用所有 IMartenModule 扩展 Schema
                foreach (var module in modules.OfType<IMartenModule>())
                    module.ConfigureMarten(opts);
            })
            .UseLightweightSessions()
            .IntegrateWithWolverine(); // 自动将 Marten 事务集成到 Wolverine 管道
    }

    /// <summary>
    /// 配置 Wolverine 消息总线
    /// 收集所有模块的 Handlers（通过程序集扫描）
    /// </summary>
    private static void ConfigureWolverine(IServiceCollection services, bool enableHttp, IModule[] modules)
    {
        services.AddWolverine(w =>
        {
            // 从所有模块的程序集扫描 Handlers
            var scannedAssemblies = new HashSet<System.Reflection.Assembly>();
            foreach (var module in modules)
            {
                var moduleType = module.GetType();
                var assembly = moduleType.Assembly;
                if (scannedAssemblies.Add(assembly))
                {
                    w.Discovery.IncludeAssembly(assembly);
                }
            }

            // 自动事务：所有 Handler 都在 Marten 事务上下文运行
            w.Policies.AutoApplyTransactions();
            // 使用持久化本地队列（如果消息未能处理，将自动重试）
            w.Policies.UseDurableLocalQueues();
        });

        if (enableHttp)
            services.AddWolverineHttp();
    }

    /// <summary>
    /// 调用所有模块的 ConfigureServices
    /// </summary>
    private static void ConfigureModules(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        IModule[] modules)
    {
        foreach (var module in modules)
            module.ConfigureServices(services, configuration, environment);
    }

    private static string GetRequiredConnectionString(IConfiguration configuration, string name)
    {
        var value = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"缺少 ConnectionStrings:{name}（请用 User Secrets/KeyVault 注入，禁止硬编码）。");

        return value;
    }
}
