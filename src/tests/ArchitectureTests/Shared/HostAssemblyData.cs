namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// Host 程序集数据提供器
/// 用于支持 ADR 测试的参数化测试
///
/// 优化说明：
/// - 使用 Lazy<T> 延迟加载避免重复初始化
/// </summary>
public sealed class HostAssemblyData : IEnumerable<object[]>
{
    private static readonly Lazy<List<Assembly>> _hostAssemblies =
        new(LoadHostAssemblies, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 获取已加载的 Host 程序集（延迟加载，线程安全）
    /// </summary>
    public static IReadOnlyList<Assembly> HostAssemblies => _hostAssemblies.Value;

    private static List<Assembly> LoadHostAssemblies()
    {
        var assemblies = new List<Assembly>();
        var root = TestEnvironment.RepositoryRoot;
        var hostDir = TestEnvironment.HostPath;
        if (!Directory.Exists(hostDir))
            return assemblies;

        var configuration = TestConstants.BuildConfiguration;
        var tfms = TestConstants.SupportedTargetFrameworks;

        foreach (var projectDir in Directory.GetDirectories(hostDir))
        {
            var projectName = Path.GetFileName(projectDir);

            var prioritized = new List<string>();
            foreach (var t in tfms)
            {
                prioritized.Add(Path.Combine(projectDir,
                "bin",
                configuration,
                t,
                $"{projectName}.dll"));
                prioritized.Add(Path.Combine(projectDir,
                "obj",
                configuration,
                t,
                $"{projectName}.dll"));
            }

            var fallback = new List<string>();
            try
            {
                fallback.AddRange(Directory.GetFiles(projectDir, "*.dll", SearchOption.AllDirectories));
                var bin = Path.Combine(projectDir, "bin");
                if (Directory.Exists(bin))
                    fallback.AddRange(Directory.GetFiles(bin, "*.dll", SearchOption.AllDirectories));
                var obj = Path.Combine(projectDir, "obj");
                if (Directory.Exists(obj))
                    fallback.AddRange(Directory.GetFiles(obj, "*.dll", SearchOption.AllDirectories));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ArchitectureTests] 搜索 Host DLL 时出错: {ex}");
            }

            var candidates = prioritized
                .Concat(fallback)
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(Path.GetFullPath)
                .Distinct()
                .ToList();

            var ordered = candidates
                .Where(File.Exists)
                .ToList();
            if (!ordered.Any())
            {
                Debug.WriteLine($"[ArchitectureTests] 未找到 Host 输出 DLL: {projectName} at {projectDir}。请确保已构建 Host 项目（dotnet build）。");
                continue;
            }

            var selected = ordered.First();
            try
            {
                var asm = Assembly.LoadFrom(selected);
                Debug.WriteLine($"[ArchitectureTests] Loaded Host assembly: {selected}, AssemblyName={asm.GetName().Name}");
                assemblies.Add(asm);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ArchitectureTests] 无法加载 Host 程序集 {selected}: {ex}");
            }
        }

        return assemblies;
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        Debug.WriteLine($"[ArchitectureTests] Loaded host assemblies count={HostAssemblies.Count}");
        if (HostAssemblies.Count == 0)
        {
            true.Should().BeFalse("❌ 未加载任何 Host 程序集，架构测试失效。请先运行 `dotnet build` 或检查 Host 输出路径/命名约定。");
        }
        foreach (var asm in HostAssemblies)
        {
            yield return new object[] { asm };
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
