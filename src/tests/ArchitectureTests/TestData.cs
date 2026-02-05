using System.Diagnostics;
using System.Reflection;
using System.Xml;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// 模块程序集数据提供器
/// 用于支持 ADR 测试的参数化测试
/// 
/// 优化说明：
/// - 使用 Lazy<T> 延迟加载避免重复初始化
/// - 使用 TestConstants 和 TestEnvironment 消除重复代码
/// </summary>
public sealed class ModuleAssemblyData : IEnumerable<object[]>
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
        var assemblies = new List<Assembly>();
        var root = TestEnvironment.RepositoryRoot;
        var modulesDir = TestEnvironment.ModulesPath;
        if (!Directory.Exists(modulesDir))
            return assemblies;

        // 支持通过环境变量切换配置（CI 可设置为 Release）
        var configuration = TestConstants.BuildConfiguration;

        // 常见 TFM 列表（优先按此顺序尝试）
        var tfms = TestConstants.SupportedTargetFrameworks;

        foreach (var moduleDir in Directory.GetDirectories(modulesDir))
        {
            var moduleName = Path.GetFileName(moduleDir);

            // 1) 明确主候选：bin/{Configuration}/{TFM}/{ModuleName}.dll（按 tfms 顺序）
            var prioritizedCandidates = new List<string>();
            foreach (var t in tfms)
            {
                prioritizedCandidates.Add(Path.Combine(moduleDir,
                "bin",
                configuration,
                t,
                $"{moduleName}.dll"));
                prioritizedCandidates.Add(Path.Combine(moduleDir,
                "obj",
                configuration,
                t,
                $"{moduleName}.dll"));
            }

            // 2) 其它 fallback 候选
            var fallback = new List<string>();
            // 全仓库搜索与常见输出目录
            try
            {
                fallback.AddRange(Directory.GetFiles(moduleDir, $"{moduleName}.dll", SearchOption.AllDirectories));
                fallback.AddRange(Directory.GetFiles(Path.Combine(moduleDir, "bin"), $"{moduleName}.dll", SearchOption.AllDirectories));
                fallback.AddRange(Directory.GetFiles(Path.Combine(moduleDir, "obj"), $"{moduleName}.dll", SearchOption.AllDirectories));
                // 最后任何包含模块名的 dll（谨慎）
                fallback.AddRange(Directory.GetFiles(moduleDir, $"*{moduleName}*.dll", SearchOption.AllDirectories));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ArchitectureTests] 搜索 DLL 时出错: {ex}");
            }

            // 合并并保留存在的路径，优先级：prioritizedCandidates -> fallback
            var orderedCandidates = prioritizedCandidates
                .Concat(fallback)
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(Path.GetFullPath)
                .Distinct()
                .ToList();

            // 只保留真实存在的文件（但保持优先顺序）
            var ordered = orderedCandidates
                .Where(File.Exists)
                .ToList();

            if (!ordered.Any())
            {
                Debug.WriteLine($"[ArchitectureTests] 未找到模块输出 DLL: {moduleName}，路径={moduleDir}。请确保已构建模块（dotnet build）或调整测试配置（Configuration env）。");
                continue;
            }

            var selected = ordered.First();
            try
            {
                var asm = Assembly.LoadFrom(selected);
                Debug.WriteLine($"[ArchitectureTests] Loaded: {selected}, AssemblyName={asm.GetName().Name}");
                // 允许 AssemblyName 为 "Zss.BilliardHall.Modules.{模块名}" 或 "{模块名}"
                if (asm.GetName()
                        .Name ==
                    moduleName ||
                    asm.GetName()
                        .Name ==
                    $"Zss.BilliardHall.Modules.{moduleName}")
                {
                    assemblies.Add(asm);
                }
                else
                {
                    Debug.WriteLine($"[ArchitectureTests] 警告: 加载的程序集名称为 {asm.GetName().Name}，与模块目录名 {moduleName} 不完全匹配。请保证一致的命名空间约定（推荐 Zss.BilliardHall.Modules.{moduleName}）。");
                    assemblies.Add(asm);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ArchitectureTests] 无法加载程序集 {selected}: {ex}");
            }
        }

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
        if (ModuleAssemblies.Count == 0)
        {
            true.Should().BeFalse("❌ 未加载任何模块程序集，架构测试失效。请先运行 `dotnet build` 或检查模块输出路径/命名约定。");
        }
        foreach (var asm in ModuleAssemblies)
        {
            yield return new object[] { asm };
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public static IEnumerable<object[]> GetModuleProjectFiles()
    {
        var root = TestEnvironment.RepositoryRoot;
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
