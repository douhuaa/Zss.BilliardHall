using System.Reflection;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Http;
using Zss.BilliardHall.Modules.Members;
using Zss.BilliardHall.Modules.Orders;
using Zss.BilliardHall.Platform.Contracts;

namespace Zss.BilliardHall.Application;

/// <summary>
/// 模块注册表 - 类型安全的模块声明
/// 优势：
/// 1. IDE 可跟踪引用（F12 导航、重构支持）
/// 2. 编译时检查（模块缺失立即发现）
/// 3. 明确的依赖关系（通过 ProjectReference 强制存在）
/// </summary>
public static class ModuleRegistry
{
    /// <summary>
    /// 所有可用模块的类型列表
    /// 注意：顺序决定了模块的初始化顺序
    /// </summary>
    private static readonly Type[] AllModuleTypes =
    [
        typeof(MemberModule),  // Members 模块
        typeof(OrderModule)    // Orders 模块
    ];

    /// <summary>
    /// 获取启用的模块程序集
    /// 支持通过配置控制启用/禁用（可选）
    /// </summary>
    public static Assembly[] GetEnabledModuleAssemblies(IConfiguration configuration)
    {
        var enabledModuleNames = GetEnabledModuleNames(configuration);

        // 如果配置为空，默认启用所有模块
        if (enabledModuleNames.Length == 0)
            return AllModuleTypes.Select(t => t.Assembly).Distinct().ToArray();

        // 根据配置筛选启用的模块
        var enabledSet = new HashSet<string>(enabledModuleNames, StringComparer.OrdinalIgnoreCase);
        var enabledModules = AllModuleTypes
            .Where(t => enabledSet.Contains(t.Assembly.GetName().Name!))
            .Select(t => t.Assembly)
            .Distinct()
            .ToArray();

        // 验证配置的模块是否存在
        var foundNames = new HashSet<string>(enabledModules.Select(a => a.GetName().Name!), StringComparer.OrdinalIgnoreCase);
        var missingModules = enabledSet.Except(foundNames).ToArray();
        if (missingModules.Length > 0)
            throw new InvalidOperationException(
                $"配置中指定的模块不存在：{string.Join(", ", missingModules)}\n" +
                $"可用模块：{string.Join(", ", AllModuleTypes.Select(t => t.Assembly.GetName().Name))}");

        return enabledModules;
    }

    private static string[] GetEnabledModuleNames(IConfiguration configuration)
    {
        // 支持两种配置方式：
        // 1. Modules:Enabled 数组（推荐）- 明确控制启用的模块
        // 2. Modules:Assemblies 数组（兼容旧格式）
        var section = configuration.GetSection("Modules:Enabled");
        if (section.Exists())
        {
            var names = section.Get<string[]>();
            if (names is { Length: > 0 })
                return NormalizeModuleNames(names);
        }

        // 兼容旧格式
        section = configuration.GetSection("Modules:Assemblies");
        if (section.Exists())
        {
            var names = section.Get<string[]>();
            if (names is { Length: > 0 })
                return NormalizeModuleNames(names);
        }

        var raw = configuration["Modules:Assemblies"];
        if (!string.IsNullOrWhiteSpace(raw))
            return NormalizeModuleNames(raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

        // 默认启用所有模块
        return [];
    }

    private static string[] NormalizeModuleNames(IEnumerable<string> names)
        => names
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}

public static class ApplicationBootstrapper
{
    public static void Configure(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var enableHttp = configuration.GetValue("Wolverine:Http:Enabled", true);

        var moduleAssemblies = GetModuleAssemblies(configuration, environment);
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

    private static Assembly[] GetModuleAssemblies(IConfiguration configuration, IHostEnvironment environment)
    {
        // 🚀 使用类型安全的模块注册表
        // 优势：编译时检查、IDE 跟踪、重构支持
        var assemblies = ModuleRegistry.GetEnabledModuleAssemblies(configuration);

        if (assemblies.Length == 0)
            throw new InvalidOperationException(
                "未找到任何启用的模块。\n" +
                $"Environment={environment.EnvironmentName}\n" +
                $"请在 ModuleRegistry 中注册模块类型。");

        return assemblies;
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

    public static void UseApplication(this WebApplication app)
    {
        app.MapWolverineEndpoints();
    }
}
