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

        ConfigureMarten(services, configuration);
        ConfigureWolverine(services, enableHttp);

        var moduleAssemblies = LoadModuleAssembliesFromConfig(configuration);
        ConfigureWolverineDiscovery(services, moduleAssemblies);
        ConfigureModules(services, configuration, environment, moduleAssemblies);
    }

    private static void ConfigureMarten(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("缺少 ConnectionStrings:Postgres（请用 User Secrets/KeyVault 注入，禁止硬编码）。");

        services.AddMarten(opts => opts.Connection(connectionString))
            .UseLightweightSessions();
    }

    private static void ConfigureWolverine(IServiceCollection services, bool enableHttp)
    {
        services.AddWolverine(_ => { });

        if (enableHttp)
            services.AddWolverineHttp();
    }

    private static Assembly[] LoadModuleAssembliesFromConfig(IConfiguration configuration)
    {
        var raw = configuration["Modules:Assemblies"];
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException(
                "缺少 Modules:Assemblies。模块加载必须显式声明（禁止目录扫描兜底）。");
        }

        var names = raw
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var assemblies = new List<Assembly>(names.Length);

        foreach (var name in names)
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
        Assembly[] moduleAssemblies)
    {
        foreach (var moduleType in DiscoverModuleTypes(moduleAssemblies))
        {
            var module = CreateModuleInstance(moduleType);
            module.ConfigureServices(services, configuration, environment);
        }
    }

    private static IEnumerable<Type> DiscoverModuleTypes(Assembly[] moduleAssemblies)
        => moduleAssemblies
            .SelectMany(SafeGetTypes)
            .Where(static t =>
                t is { IsAbstract: false, IsInterface: false } &&
                typeof(IModule).IsAssignableFrom(t));

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(static t => t is not null)!;
        }
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
