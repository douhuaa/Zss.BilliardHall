using System.Reflection;
using Marten;
using Microsoft.AspNetCore.Builder;
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
        => LoadModuleAssemblies(ReadModuleAssemblyNames(configuration, environment));

    private static string[] ReadModuleAssemblyNames(IConfiguration configuration, IHostEnvironment environment)
    {
        // 推荐最终只保留数组格式；此处兼容 string/array 两种写法
        var section = configuration.GetSection("Modules:Assemblies");
        var namesFromArray = section.Get<string[]>();

        if (namesFromArray is { Length: > 0 })
            return NormalizeAssemblyNames(namesFromArray);

        var raw = configuration["Modules:Assemblies"];
        if (!string.IsNullOrWhiteSpace(raw))
            return NormalizeAssemblyNames(raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

        throw new InvalidOperationException(
            "缺少 Modules:Assemblies。模块加载必须显式声明。\n" +
            $"Environment={environment.EnvironmentName}\n" +
            $"BaseDirectory={AppContext.BaseDirectory}");
    }

    private static string[] NormalizeAssemblyNames(IEnumerable<string> names)
        => names
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

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
