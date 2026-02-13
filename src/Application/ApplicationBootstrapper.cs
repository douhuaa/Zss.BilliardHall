using System.Reflection;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Http;
using Zss.BilliardHall.Platform.Contracts;

namespace Zss.BilliardHall.Application;

/// <summary>
/// Application 层 Bootstrapper
/// 负责装配应用程序的核心服务和中间件
/// 符合 ADR-002：Application 不依赖 Modules，不包含进程相关代码
/// </summary>

public static class ApplicationBootstrapper
{
    /// <summary>
    /// 配置应用程序服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="environment">环境</param>
    /// <param name="moduleAssemblies">模块程序集列表（由 Host 层提供）</param>
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        Assembly[] moduleAssemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(moduleAssemblies);

        var enableHttp = configuration.GetValue("Wolverine:Http:Enabled", true);

        var modules = CreateModulesInOrder(moduleAssemblies);

        ConfigureMarten(services, configuration, modules);
        ConfigureWolverine(services, enableHttp, moduleAssemblies);
        ConfigureModules(services, configuration, environment, modules);
    }

    private static void ConfigureMarten(IServiceCollection services, IConfiguration configuration, IReadOnlyList<IModule> modules)
    {
        var connectionString = GetRequiredConnectionString(configuration, "Postgres");

        services.AddMarten(opts =>
            {
                opts.Connection(connectionString);

                foreach (var module in modules.OfType<IMartenModule>())
                    module.ConfigureMarten(opts);
            })
            .UseLightweightSessions();
    }

    private static void ConfigureWolverine(IServiceCollection services, bool enableHttp, Assembly[] moduleAssemblies)
    {
        services.AddWolverine(w =>
        {
            foreach (var assembly in moduleAssemblies)
                w.Discovery.IncludeAssembly(assembly);

            w.Policies.AutoApplyTransactions();

            // 🚀 根据环境切换 CodeGen 模式
            // Development: Dynamic (快速开发，支持热重载)
            // Production: Auto (Wolverine 自动选择最佳模式)
            // 注意：Wolverine 5.x 会根据环境自动优化，无需显式配置
            // 如需强制 Static 模式，可设置环境变量 WOLVERINE_CODEGEN_MODE=Static
        });

        if (enableHttp)
            services.AddWolverineHttp();
    }

    private static void ConfigureModules(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        IReadOnlyList<IModule> modules)
    {
        foreach (var module in modules)
            module.ConfigureServices(services, configuration, environment);
    }


    private static IReadOnlyList<IModule> CreateModulesInOrder(Assembly[] moduleAssemblies)
    {
        var modules = new List<IModule>(moduleAssemblies.Length);

        foreach (var assembly in moduleAssemblies)
        {
            var moduleType = GetSingleModuleTypeOrThrow(assembly);
            modules.Add(CreateModuleInstance(moduleType));
        }

        return modules;
    }

    private static Type GetSingleModuleTypeOrThrow(Assembly assembly)
    {
        var moduleTypes = SafeGetTypes(assembly)
            .Where(static t => t is { IsAbstract: false, IsInterface: false } && typeof(IModule).IsAssignableFrom(t))
            .ToArray();

        return moduleTypes.Length switch
        {
            0 => throw new InvalidOperationException($"模块程序集未声明 IModule：{assembly.GetName().Name}（必须且只能有一个）。"),
            1 => moduleTypes[0],
            _ => throw new InvalidOperationException(
                $"模块程序集包含多个 IModule（必须且只能有一个）：{assembly.GetName().Name}\n" +
                string.Join('\n', moduleTypes.Select(t => $" - {t.FullName}")))
        };
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(static t => t is not null)!; }
    }

    private static IModule CreateModuleInstance(Type moduleType)
    {
        try
        {
            return (IModule)Activator.CreateInstance(moduleType)!;
        }
        catch (MissingMethodException ex)
        {
            throw new InvalidOperationException($"模块必须提供 public 无参构造函数：{moduleType.FullName}", ex);
        }
    }

    private static string GetRequiredConnectionString(IConfiguration configuration, string name)
    {
        var value = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"缺少 ConnectionStrings:{name}（请用 User Secrets/KeyVault 注入，禁止硬编码）。");

        return value;
    }
}
