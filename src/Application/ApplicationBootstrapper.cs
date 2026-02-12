using System.Reflection;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Http;
using Zss.BilliardHall.Platform.Contracts;

namespace Zss.BilliardHall.Application;

public static class ApplicationBootstrapper
{
    public static void Configure(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var enableHttp = configuration.GetValue("Wolverine:Http:Enabled", true);

        var moduleAssemblies = LoadModuleAssemblies(ReadModuleAssemblyNames(configuration));

        // 关键：先实例化模块（按配置顺序 + 每程序集唯一 IModule）
        var modules = CreateModulesInOrder(moduleAssemblies);

        ConfigureMarten(services, configuration, modules);
        ConfigureWolverine(services, enableHttp);
        ConfigureWolverineDiscovery(services, moduleAssemblies);

        ConfigureModules(services, configuration, environment, modules);
    }

    private static void ConfigureMarten(IServiceCollection services, IConfiguration configuration, IReadOnlyList<IModule> modules)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("缺少 ConnectionStrings:Postgres（请用 User Secrets/KeyVault 注入，禁止硬编码）。");

        services.AddMarten(opts =>
            {
                opts.Connection(connectionString);

                foreach (var module in modules.OfType<IMartenModule>())
                    module.ConfigureMarten(opts);
            })
            .UseLightweightSessions();
    }

    private static void ConfigureWolverine(IServiceCollection services, bool enableHttp)
    {
        services.AddWolverine(_ => { });

        if (enableHttp)
            services.AddWolverineHttp();
    }

    private static void ConfigureWolverineDiscovery(IServiceCollection services, Assembly[] moduleAssemblies)
    {
        services.Configure<WolverineOptions>(w =>
        {
            foreach (var assembly in moduleAssemblies)
                w.Discovery.IncludeAssembly(assembly);

            w.Policies.AutoApplyTransactions();
        });
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

    private static string[] ReadModuleAssemblyNames(IConfiguration configuration)
    {
        var raw = configuration["Modules:Assemblies"];
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("缺少 Modules:Assemblies。模块加载必须显式声明。");

        return raw
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static Assembly[] LoadModuleAssemblies(string[] assemblyNames)
    {
        var assemblies = new List<Assembly>(assemblyNames.Length);

        foreach (var name in assemblyNames)
        {
            try
            {
                assemblies.Add(Assembly.Load(new AssemblyName(name)));
            }
            catch (FileNotFoundException ex)
            {
                throw new InvalidOperationException($"模块程序集未找到：{name}。请确认已构建并可被加载。", ex);
            }
            catch (FileLoadException ex)
            {
                throw new InvalidOperationException($"模块程序集加载失败：{name}（文件存在但无法加载）。", ex);
            }
            catch (BadImageFormatException ex)
            {
                throw new InvalidOperationException($"模块程序集格式错误：{name}（可能是目标框架/架构不匹配）。", ex);
            }
        }

        return assemblies.ToArray();
    }

    private static Type GetSingleModuleTypeOrThrow(Assembly assembly)
    {
        var moduleTypes = SafeGetTypes(assembly)
            .Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(IModule).IsAssignableFrom(t))
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
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!; }
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
}
