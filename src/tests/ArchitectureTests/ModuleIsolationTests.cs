using NetArchTest.Rules;
using System.Reflection;
using Xunit;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace Zss.BilliardHall.Tests.ArchitectureTests;


public class ModuleIsolationTests
{
    // 模块程序集和模块名清单唯一来源
    public static IReadOnlyList<Assembly> ModuleAssemblies => ModuleAssemblyData.ModuleAssemblies;
    public static IReadOnlyList<string> ModuleNames => ModuleAssemblyData.ModuleNames;

    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Reference_Other_Modules(Assembly moduleAssembly)
    {
        var moduleName = moduleAssembly.GetName().Name!.Split('.').Last();
        foreach (var other in ModuleNames.Where(m => m != moduleName))
        {
            var result = Types.InAssembly(moduleAssembly)
                .ShouldNot()
                .HaveDependencyOn($"Zss.BilliardHall.Modules.{other}")
                .GetResult();
            Assert.True(result.IsSuccessful, $"模块 {moduleName} 不应依赖模块 {other}。修复建议：将共享逻辑移至 Platform/BuildingBlocks，或改为消息通信（Publish/Invoke），或由 Bootstrapper/Coordinator 做模块级协调。");
        }
    }

    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Contain_Traditional_Layering_Namespaces(Assembly moduleAssembly)
    {
        var forbidden = new[] { ".Application", ".Domain", ".Infrastructure", ".Repository", ".Service", ".Shared", ".Common" };
        foreach (var ns in forbidden)
        {
            var result = Types.InAssembly(moduleAssembly)
                .That().ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
                .ShouldNot().ResideInNamespaceContaining(ns)
                .GetResult();
            Assert.True(result.IsSuccessful, $"模块 {moduleAssembly.GetName().Name} 禁止出现命名空间: {ns}。修复建议：将相关代码移回模块切片内部或抽象到 Platform（仅当满足 BuildingBlocks 准入规则）。");
        }
    }

    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Contain_Repository_Or_Service_Semantics(Assembly moduleAssembly)
    {
        var forbidden = new[] { "Repository", "Service", "Manager", "Store" };
        foreach (var word in forbidden)
        {
            var result = Types.InAssembly(moduleAssembly)
                .That().ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
                .ShouldNot().HaveNameMatching($".*{word}.*")
                .GetResult();
            Assert.True(result.IsSuccessful, $"模块 {moduleAssembly.GetName().Name} 禁止出现类型名包含: {word}。修复建议：把实现放到模块内的领域/领域服务或改为消息交互。");
        }
    }

    // 新增：Modules 只能依赖 Platform，不得依赖 Application/Host
    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Only_Depend_On_Platform(Assembly moduleAssembly)
    {
        var result = Types.InAssembly(moduleAssembly)
            .ShouldNot().HaveDependencyOnAny(
                "Zss.BilliardHall.Application",
                "Zss.BilliardHall.Host")
            .GetResult();
        Assert.True(result.IsSuccessful, $"模块 {moduleAssembly.GetName().Name} 不应依赖 Application 或 Host。修复建议：将需要共享的契约放到 Platform/BuildingBlocks，或通过消息通信解耦。");
    }

    [Fact]
    public void Modules_Csproj_Should_Not_Reference_Other_Modules()
    {
        var root = GetSolutionRoot();
        var modulesDir = Path.Combine(root, "src", "Modules");
        var csprojs = Directory.GetFiles(modulesDir, "*.csproj", SearchOption.AllDirectories);
        // 白名单项目（按需扩展）
        var allowedProjectNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Zss.BilliardHall.Platform",
            "Zss.BilliardHall.BuildingBlocks",
        };
        foreach (var csproj in csprojs)
        {
            var doc = new XmlDocument();
            doc.Load(csproj);
            var mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("msb", doc.DocumentElement!.NamespaceURI);
            var projectName = Path.GetFileNameWithoutExtension(csproj);
            var references = doc.SelectNodes("//msb:ProjectReference", mgr);
            if (references == null) continue;
            foreach (XmlNode reference in references)
            {
                var include = reference?.Attributes?["Include"]?.Value;
                if (string.IsNullOrEmpty(include)) continue;
                var refPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(csproj)!, include));
                var refName = Path.GetFileNameWithoutExtension(refPath);
                if (string.Equals(refName, projectName, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (allowedProjectNames.Contains(refName))
                    continue;
                Assert.True(false, $"模块 {projectName} 不应引用其他模块或非白名单项目: {refName}（{csproj} -> {include}）。修复建议：将共享代码移至 Platform/BuildingBlocks，或改用消息通信（Publish/Invoke）。");
            }
        }
    }

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
}

public class ModuleAssemblyData : IEnumerable<object[]>
{
    // 唯一模块程序集和模块名清单
    public static List<Assembly> ModuleAssemblies { get; } = new();
    public static List<string> ModuleNames { get; } = new();

    static ModuleAssemblyData()
    {
        var root = ModuleIsolationTests.GetSolutionRoot();
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

            // 1) 明确主候选：bin/{Configuration}/{TFM}/{ModuleName}.dll（�� tfms 顺序）
            var prioritizedCandidates = new List<string>();
            foreach (var t in tfms)
            {
                prioritizedCandidates.Add(Path.Combine(moduleDir, "bin", configuration, t, $"{moduleName}.dll"));
                prioritizedCandidates.Add(Path.Combine(moduleDir, "obj", configuration, t, $"{moduleName}.dll"));
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
            var ordered = orderedCandidates.Where(File.Exists).ToList();

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
                if (asm.GetName().Name == moduleName || asm.GetName().Name == $"Zss.BilliardHall.Modules.{moduleName}")
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

    public ModuleAssemblyData() { /* nothing, static ctor does all */ }

    public IEnumerator<object[]> GetEnumerator()
    {
        Debug.WriteLine($"[ArchitectureTests] Loaded module assemblies count={ModuleAssemblies.Count}");
        if (ModuleAssemblies.Count == 0)
        {
            Assert.True(false, "❌ 未加载任何 Modules 程序集，架构测试失效。请先运行 `dotnet build` 或检查模块输出路径/命名约定。");
        }
        foreach (var module in ModuleAssemblies)
        {
            yield return new object[] { module };
        }
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}