namespace Zss.BilliardHall.Tests.SharedTestHelpers.Assemblies;

/// <summary>
/// 模块程序集数据提供器
/// 用于支持 ADR 测试的参数化测试
/// 
/// 优化说明：
/// - 使用 Lazy<T> 延迟加载避免重复初始化
/// - 使用 TestEnvironment 消除重复代码
/// - 继承 AssemblyLoaderBase 消除与 HostAssemblyData 的代码重复
/// 
/// 重构说明（2026-02-09）：
/// - 提取公共加载逻辑到 AssemblyLoaderBase
/// - 减少代码重复 ~70%
/// </summary>
public sealed class ModuleAssemblyData : AssemblyLoaderBase, IEnumerable<object[]>
{
    private static readonly Lazy<List<Assembly>> _moduleAssemblies =
        new(LoadModuleAssemblies, LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<List<string>> _moduleNames =
        new(LoadModuleNames, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 获取已加载的模块程序集（延迟加载，线程安全）
    /// </summary>
    public static IReadOnlyList<Assembly> ModuleAssemblies => _moduleAssemblies.Value;

    /// <summary>
    /// 获取已加载的模块名称列表（延迟加载，线程安全）
    /// </summary>
    public static IReadOnlyList<string> ModuleNames => _moduleNames.Value;

    private static List<Assembly> LoadModuleAssemblies()
    {
        var modulesDir = TestEnvironment.ModulesPath;
        if (!Directory.Exists(modulesDir))
        {
            return new List<Assembly>();
        }

        var configuration = TestEnvironment.BuildConfiguration;
        var tfms = TestEnvironment.SupportedTargetFrameworks;
        var moduleDirectories = Directory.GetDirectories(modulesDir);

        // 使用基类的统一加载逻辑
        var assemblies = LoadAssembliesFromDirectories(
            moduleDirectories,
            configuration,
            tfms,
            nameValidator: (assemblyName, moduleName) =>
            {
                // 允许 AssemblyName 为 "Zss.BilliardHall.Modules.{模块名}" 或 "{模块名}"
                return assemblyName == moduleName ||
                       assemblyName == $"Zss.BilliardHall.Modules.{moduleName}";
            });

        return assemblies;
    }

    private static List<string> LoadModuleNames()
    {
        var names = new List<string>();
        var modulesDir = TestEnvironment.ModulesPath;
        if (!Directory.Exists(modulesDir))
            return names;

        foreach (var moduleDir in Directory.GetDirectories(modulesDir))
        {
            var moduleName = Path.GetFileName(moduleDir);
            names.Add(moduleName);
        }

        // 去重并排序
        return names
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public ModuleAssemblyData()
    {
        /* 所有初始化在 Lazy<T> 中完成 */
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        Debug.WriteLine($"[ArchitectureTests] Loaded module assemblies count={ModuleAssemblies.Count}, names={string.Join(",", ModuleNames)}");
        ValidateAssembliesNotEmpty(ModuleAssemblies, "模块");

        foreach (var asm in ModuleAssemblies)
        {
            yield return new object[] { asm };
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public static IEnumerable<object[]> GetModuleProjectFiles()
    {
        var modulesDir = TestEnvironment.ModulesPath;
        if (!Directory.Exists(modulesDir))
        {
            yield break;
        }

        var csprojs = Directory.GetFiles(modulesDir, "*.csproj", SearchOption.AllDirectories);
        foreach (var csproj in csprojs)
        {
            yield return new object[] { csproj };
        }
    }
}
