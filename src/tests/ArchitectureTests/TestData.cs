using System.Diagnostics;
using System.Reflection;
using System.Xml;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// 模块程序集数据提供器
/// 用于支持 ADR 测试的参数化测试
/// </summary>
public class ModuleAssemblyData : IEnumerable<object[]>
{
    // 唯一模块程序集和模块名清单
    public static List<Assembly> ModuleAssemblies { get; } = new();
    public static List<string> ModuleNames { get; } = new();

    static ModuleAssemblyData()
    {
        var root = GetSolutionRoot();
        var modulesDir = Path.Combine(root, "src", "Modules");
        if (!Directory.Exists(modulesDir))
            return;

        // 支持通过环境变量切换配置（CI 可设置为 Release）
        var configuration = Environment.GetEnvironmentVariable("Configuration") ?? "Debug";

        // 常见 TFM 列表（优先按此顺序尝试）
        var tfms = new[] { "net10.0", "net8.0", "net7.0", "net6.0", "net5.0" };

        foreach (var moduleDir in Directory.GetDirectories(modulesDir))
        {
            var moduleName = Path.GetFileName(moduleDir);
            // 记录目录名（后续会去重排序）
            ModuleNames.Add(moduleName);

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
                    ModuleAssemblies.Add(asm);
                }
                else
                {
                    Debug.WriteLine($"[ArchitectureTests] 警告: 加载的程序集名称为 {asm.GetName().Name}，与模块目录名 {moduleName} 不完全匹配。请保证一致的命名空间约定（推荐 Zss.BilliardHall.Modules.{moduleName}）。");
                    ModuleAssemblies.Add(asm);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ArchitectureTests] 无法加载程序集 {selected}: {ex}");
            }
        }

        // 微调 1：ModuleNames 去重并排序（原地替换，保持 List API）
        var orderedNames = ModuleNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
        ModuleNames.Clear();
        ModuleNames.AddRange(orderedNames);
    }

    public ModuleAssemblyData()
    {
        /* nothing, static ctor does all */
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        Debug.WriteLine($"[ArchitectureTests] Loaded module assemblies count={ModuleAssemblies.Count}, names={string.Join(",", ModuleNames)}");
        if (ModuleAssemblies.Count == 0)
        {
            Assert.Fail("❌ 未加载任何模块程序集，架构测试失效。请先运行 `dotnet build` 或检查模块输出路径/命名约定。");
        }
        foreach (var asm in ModuleAssemblies)
        {
            yield return new object[] { asm };
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public static string GetSolutionRoot()
    {
        var dir = new DirectoryInfo(Environment.CurrentDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Zss.BilliardHall.slnx")))
        {
            dir = dir.Parent;
        }
        if (dir == null)
            throw new DirectoryNotFoundException("未找到解决方案根目录（Zss.BilliardHall.slnx）");
        return dir.FullName;
    }

    public static IEnumerable<object[]> GetModuleProjectFiles()
    {
        var root = GetSolutionRoot();
        var modulesDir = Path.Combine(root, "src", "Modules");
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
/// </summary>
public class HostAssemblyData : IEnumerable<object[]>
{
    public static List<Assembly> HostAssemblies { get; } = new();

    static HostAssemblyData()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var hostDir = Path.Combine(root, "src", "Host");
        if (!Directory.Exists(hostDir))
            return;

        var configuration = Environment.GetEnvironmentVariable("Configuration") ?? "Debug";
        var tfms = new[] { "net10.0", "net8.0", "net7.0", "net6.0" };

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
                HostAssemblies.Add(asm);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ArchitectureTests] 无法加载 Host 程序集 {selected}: {ex}");
            }
        }
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        Debug.WriteLine($"[ArchitectureTests] Loaded host assemblies count={HostAssemblies.Count}");
        if (HostAssemblies.Count == 0)
        {
            Assert.Fail("❌ 未加载任何 Host 程序集，架构测试失效。请先运行 `dotnet build` 或检查 Host 输出路径/命名约定。");
        }
        foreach (var asm in HostAssemblies)
        {
            yield return new object[] { asm };
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
