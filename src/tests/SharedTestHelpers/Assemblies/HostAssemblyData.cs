namespace Zss.BilliardHall.Tests.SharedTestHelpers.Assemblies;

/// <summary>
/// Host 程序集数据提供器
/// 用于支持 ADR 测试的参数化测试
///
/// 优化说明：
/// - 使用 Lazy<T> 延迟加载避免重复初始化
/// - 继承 AssemblyLoaderBase 消除与 ModuleAssemblyData 的代码重复
/// 
/// 重构说明（2026-02-09）：
/// - 提取公共加载逻辑到 AssemblyLoaderBase
/// - 减少代码重复 ~70%
/// </summary>
public sealed class HostAssemblyData : AssemblyLoaderBase, IEnumerable<object[]>
{
    private static readonly Lazy<List<Assembly>> _hostAssemblies =
        new(LoadHostAssemblies, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 获取已加载的 Host 程序集（延迟加载，线程安全）
    /// </summary>
    public static IReadOnlyList<Assembly> HostAssemblies => _hostAssemblies.Value;

    private static List<Assembly> LoadHostAssemblies()
    {
        var hostDir = TestEnvironment.HostPath;
        if (!Directory.Exists(hostDir))
        {
            return new List<Assembly>();
        }

        var configuration = TestEnvironment.BuildConfiguration;
        var tfms = TestEnvironment.SupportedTargetFrameworks;
        var hostDirectories = Directory.GetDirectories(hostDir);

        // 使用基类的统一加载逻辑
        var assemblies = LoadAssembliesFromDirectories(
            hostDirectories,
            configuration,
            tfms,
            nameValidator: null); // Host 程序集无需特殊名称验证

        return assemblies;
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        Debug.WriteLine($"[ArchitectureTests] Loaded host assemblies count={HostAssemblies.Count}");
        ValidateAssembliesNotEmpty(HostAssemblies, "Host");
        
        foreach (var asm in HostAssemblies)
        {
            yield return new object[] { asm };
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
