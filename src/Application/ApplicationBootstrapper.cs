using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Marten;
using Zss.BilliardHall.Application.Infrastructure;
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
        IReadOnlyList<IModule> modules)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(modules);

        var enableHttp = configuration.GetValue("Wolverine:Http:Enabled", true);

        // 配置 Marten 选项（使用 IOptions 模式）
        services.Configure<MartenOptions>(configuration.GetSection(MartenOptions.SectionName));
        services.AddOptions<MartenOptions>()
            .Validate(opts =>
            {
                try
                {
                    opts.Validate();
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }, "Marten 配置无效：请配置 Marten:ConnectionString（建议使用 User Secrets、KeyVault 或环境变量）")
            .ValidateOnStart();

        ConfigureMarten(services, configuration, modules);
        ConfigureWolverine(services, enableHttp, modules);
        ConfigureModules(services, configuration, environment, modules);
    }

    /// <summary>
    /// 配置 Marten（EF Core 替代品）+ Wolverine 集成
    /// 调用所有模块的 IMartenModule.ConfigureMarten()
    /// 使用 IOptions 模式获取配置
    /// </summary>
    private static void ConfigureMarten(IServiceCollection services, IConfiguration configuration, IReadOnlyList<IModule> modules)
    {
        services
            .AddMarten(sp =>
            {
                var martenOptions = sp.GetRequiredService<IOptions<MartenOptions>>().Value;
                var opts = new StoreOptions();
                opts.Connection(martenOptions.ConnectionString);

                // 调用所有 IMartenModule 扩展 Schema
                foreach (var module in modules.OfType<IMartenModule>())
                    module.ConfigureMarten(opts);

                return opts;
            })
            .UseLightweightSessions()
            .IntegrateWithWolverine(); // 自动将 Marten 事务集成到 Wolverine 管道
    }

    /// <summary>
    /// 配置 Wolverine 消息总线
    /// 收集所有模块的 Handlers（通过程序集扫描）
    /// 注册验证中间件
    /// </summary>
    private static void ConfigureWolverine(IServiceCollection services, bool enableHttp, IReadOnlyList<IModule> modules)
    {
        services.AddWolverine(w =>
        {
            var assemblies = GetDistinctModuleAssemblies(modules);
            foreach (var assembly in assemblies)
            {
                w.Discovery.IncludeAssembly(assembly);
            }

            // 官方推荐：使用 Wolverine.FluentValidation 提供的验证集成
            w.UseFluentValidation();

            // 异常翻译：将技术异常（如 PostgresException）翻译为领域异常（DomainException）
            // 在 handler pipeline 中运行，Web/Worker 层只处理结构化异常
            w.Policies.AddMiddleware<ExceptionTranslationMiddleware>();

            // 自动事务：所有 Handler 都在 Marten 事务上下文运行
            w.Policies.AutoApplyTransactions();

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
        IReadOnlyList<IModule> modules)
    {
        foreach (var module in modules)
            module.ConfigureServices(services, configuration, environment);
    }

    private static IReadOnlyList<System.Reflection.Assembly> GetDistinctModuleAssemblies(IReadOnlyList<IModule> modules)
    {
        var set = new HashSet<System.Reflection.Assembly>();
        foreach (var module in modules)
            set.Add(module.GetType().Assembly);

        return set.ToArray();
    }
}
