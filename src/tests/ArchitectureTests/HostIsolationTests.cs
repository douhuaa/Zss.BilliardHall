using NetArchTest.Rules;
using System.Reflection;
using Xunit;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Xml;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

public class HostIsolationTests
{
    // 主机程序集来源
    public static IReadOnlyList<Assembly> HostAssemblies => HostAssemblyData.HostAssemblies;

    [Theory]
    [ClassData(typeof(HostAssemblyData))]
    public void Host_Should_Not_Have_Dependencies_On_Modules(Assembly hostAssembly)
    {
        var forbidden = ModuleAssemblyData.ModuleNames.Select(m => $"Zss.BilliardHall.Modules.{m}").ToArray();
        if (forbidden.Length == 0)
        {
            Assert.Fail("❌ 未找到任何 Modules 程序集，架构测试无效。请先运行 `dotnet build`。");
        }

        var result = Types.InAssembly(hostAssembly)
            .ShouldNot().HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"主机程序集 {hostAssembly.GetName().Name} 不应依赖任何 Modules 程序集。违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? System.Array.Empty<string>())}。\n" +
            "修复建议：将业务契约移出 Modules（到 Platform/BuildingBlocks），或使用消息/事件进行解耦。");
    }

    [Theory]
    [ClassData(typeof(HostAssemblyData))]
    public void Host_Should_Not_Contain_Module_Namespaces(Assembly hostAssembly)
    {
        var typesArr = Types.InAssembly(hostAssembly)
            .That().ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
            .GetTypes()
            .ToArray();

        Assert.Empty(typesArr);
    }

    [Theory]
    [ClassData(typeof(HostAssemblyData))]
    public void Host_Should_Not_Contain_Business_Types(Assembly hostAssembly)
    {
        var typesArr = Types.InAssembly(hostAssembly)
            .That().ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
            .GetTypes()
            .ToArray();

        Assert.Empty(typesArr);
    }

    [Fact]
    public void Host_Csproj_Should_Not_Reference_Modules()
    {
        var root = ModuleIsolationTests.GetSolutionRoot();
        var hostDir = Path.Combine(root, "src", "Host");
        var csprojs = Directory.GetFiles(hostDir, "*.csproj", SearchOption.AllDirectories);

        var modulesDir = Path.Combine(root, "src", "Modules");
        var moduleProjFiles = Directory.Exists(modulesDir)
            ? Directory.GetFiles(modulesDir, "*.csproj", SearchOption.AllDirectories).Select(Path.GetFullPath).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var csproj in csprojs)
        {
            var doc = new XmlDocument();
            doc.Load(csproj);
            var mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("msb", doc.DocumentElement!.NamespaceURI);
            var references = doc.SelectNodes("//msb:ProjectReference", mgr);
            if (references == null) continue;
            foreach (XmlNode reference in references)
            {
                var include = reference?.Attributes?["Include"]?.Value;
                if (string.IsNullOrEmpty(include)) continue;
                var refPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(csproj)!, include));
                if (moduleProjFiles.Contains(refPath))
                {
                    Assert.Fail($"Host 项目 {Path.GetFileName(csproj)} 不应直接引用 Modules 下的项目: {Path.GetFileName(refPath)}（{csproj} -> {include}）。\n修复建议：将共享契约移到 Platform/BuildingBlocks，或改为消息通信。");
                }
            }
        }
    }
}

public class HostAssemblyData : IEnumerable<object[]>
{
    public static List<Assembly> HostAssemblies { get; } = new();

    static HostAssemblyData()
    {
        var root = ModuleIsolationTests.GetSolutionRoot();
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
                prioritized.Add(Path.Combine(projectDir, "bin", configuration, t, $"{projectName}.dll"));
                prioritized.Add(Path.Combine(projectDir, "obj", configuration, t, $"{projectName}.dll"));
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

            var candidates = prioritized.Concat(fallback)
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(Path.GetFullPath)
                .Distinct()
                .ToList();

            var ordered = candidates.Where(File.Exists).ToList();
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
