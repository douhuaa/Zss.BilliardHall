using NetArchTest.Rules;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0002: Platform / Application / Host 三层启动体系
/// 验证三层依赖约束和启动职责分工
/// </summary>
public sealed class ADR_0002_Architecture_Tests
{
    #region 1. Platform 层约束

    [Fact(DisplayName = "ADR-0002.1: Platform 不应依赖 Application")]
    public void Platform_Should_Not_Depend_On_Application()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var result = Types.InAssembly(platformAssembly)
            .ShouldNot().HaveDependencyOn("Zss.BilliardHall.Application")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0002 违规: Platform 层不应依赖 Application 层。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
            $"修复建议：依赖关系应该是 Application -> Platform，而不是反向。");
    }

    [Fact(DisplayName = "ADR-0002.2: Platform 不应依赖 Host")]
    public void Platform_Should_Not_Depend_On_Host()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var result = Types.InAssembly(platformAssembly)
            .ShouldNot().HaveDependencyOnAny(
                "Zss.BilliardHall.Host",
                "Zss.BilliardHall.Host.Web",
                "Zss.BilliardHall.Host.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0002 违规: Platform 层不应依赖 Host 层。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
            $"修复建议：依赖关系应该是 Host -> Platform，而不是反向。");
    }

    [Fact(DisplayName = "ADR-0002.3: Platform 不应依赖任何 Modules")]
    public void Platform_Should_Not_Depend_On_Modules()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var result = Types.InAssembly(platformAssembly)
            .ShouldNot().HaveDependencyOnAny(
                ModuleAssemblyData.ModuleNames.Select(m => $"Zss.BilliardHall.Modules.{m}").ToArray())
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0002 违规: Platform 层不应依赖任何 Modules。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
            $"修复建议：Platform 是基础层，应该被模块依赖，而不是依赖模块。");
    }

    [Fact(DisplayName = "ADR-0002.4: Platform 应有唯一的 PlatformBootstrapper 入口")]
    public void Platform_Should_Have_Single_Bootstrapper_Entry_Point()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var bootstrappers = Types.InAssembly(platformAssembly)
            .That().HaveNameEndingWith("Bootstrapper")
            .And().AreClasses()
            .GetTypes()
            .ToList();

        Assert.True(bootstrappers.Count > 0,
            $"❌ ADR-0002 违规: Platform 层必须包含 Bootstrapper 入口点。\n" +
            $"修复建议：创建 PlatformBootstrapper 类作为 Platform 层的唯一入口。");

        // 验证有 Configure 方法
        var platformBootstrapper = bootstrappers.FirstOrDefault(t => t.Name == "PlatformBootstrapper");
        Assert.NotNull(platformBootstrapper);

        var configureMethods = platformBootstrapper.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Configure")
            .ToList();

        Assert.True(configureMethods.Count > 0,
            $"❌ ADR-0002 违规: PlatformBootstrapper 必须包含 Configure 方法。\n" +
            $"修复建议：在 PlatformBootstrapper 中添加 public static void Configure() 方法。");
    }

    #endregion

    #region 2. Application 层约束

    [Fact(DisplayName = "ADR-0002.5: Application 不应依赖 Host")]
    public void Application_Should_Not_Depend_On_Host()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var result = Types.InAssembly(applicationAssembly)
            .ShouldNot().HaveDependencyOnAny(
                "Zss.BilliardHall.Host",
                "Zss.BilliardHall.Host.Web",
                "Zss.BilliardHall.Host.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0002 违规: Application 层不应依赖 Host 层。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
            $"修复建议：依赖关系应该是 Host -> Application，而不是反向。");
    }

    [Fact(DisplayName = "ADR-0002.6: Application 不应依赖任何 Modules")]
    public void Application_Should_Not_Depend_On_Modules()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var result = Types.InAssembly(applicationAssembly)
            .ShouldNot().HaveDependencyOnAny(
                ModuleAssemblyData.ModuleNames.Select(m => $"Zss.BilliardHall.Modules.{m}").ToArray())
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0002 违规: Application 层不应依赖任何 Modules。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
            $"修复建议：Application 层用于横切关注点，不应耦合具体业务模块。");
    }

    [Fact(DisplayName = "ADR-0002.7: Application 应有唯一的 ApplicationBootstrapper 入口")]
    public void Application_Should_Have_Single_Bootstrapper_Entry_Point()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var bootstrappers = Types.InAssembly(applicationAssembly)
            .That().HaveNameEndingWith("Bootstrapper")
            .And().AreClasses()
            .GetTypes()
            .ToList();

        Assert.True(bootstrappers.Count > 0,
            $"❌ ADR-0002 违规: Application 层必须包含 Bootstrapper 入口点。\n" +
            $"修复建议：创建 ApplicationBootstrapper 类作为 Application 层的唯一入口。");

        var applicationBootstrapper = bootstrappers.FirstOrDefault(t => t.Name == "ApplicationBootstrapper");
        Assert.NotNull(applicationBootstrapper);

        var configureMethods = applicationBootstrapper.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Configure")
            .ToList();

        Assert.True(configureMethods.Count > 0,
            $"❌ ADR-0002 违规: ApplicationBootstrapper 必须包含 Configure 方法。\n" +
            $"修复建议：在 ApplicationBootstrapper 中添加 public static void Configure() 方法。");
    }

    [Fact(DisplayName = "ADR-0002.8: Application 不应包含 HttpContext 等 Host 专属类型")]
    public void Application_Should_Not_Use_HttpContext()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var result = Types.InAssembly(applicationAssembly)
            .ShouldNot().HaveDependencyOn("Microsoft.AspNetCore.Http.HttpContext")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0002 违规: Application 层不应使用 HttpContext 等 Host 专属类型。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
            $"修复建议：HttpContext 等类型应该只存在于 Host 层。");
    }

    #endregion

    #region 3. Host 层约束

    [Theory(DisplayName = "ADR-0002.9: Host 不应依赖任何 Modules")]
    [ClassData(typeof(HostAssemblyData))]
    public void Host_Should_Not_Depend_On_Modules(Assembly hostAssembly)
    {
        var forbidden = ModuleAssemblyData.ModuleNames.Select(m => $"Zss.BilliardHall.Modules.{m}").ToArray();
        if (forbidden.Length == 0)
        {
            // 如果没有模块，跳过测试
            return;
        }

        var result = Types.InAssembly(hostAssembly)
            .ShouldNot().HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0002 违规: Host {hostAssembly.GetName().Name} 不应依赖任何 Modules。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
            $"修复建议：将业务契约移出 Modules（到 Platform/BuildingBlocks），或使用消息/事件进行解耦。");
    }

    [Theory(DisplayName = "ADR-0002.10: Host 不应包含业务类型")]
    [ClassData(typeof(HostAssemblyData))]
    public void Host_Should_Not_Contain_Business_Types(Assembly hostAssembly)
    {
        var businessTypes = Types.InAssembly(hostAssembly)
            .That().ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
            .GetTypes()
            .ToArray();

        Assert.Empty(businessTypes);
    }

    [Theory(DisplayName = "ADR-0002.11: Host 项目文件不应引用 Modules")]
    [MemberData(nameof(GetHostProjectFiles))]
    public void Host_Csproj_Should_Not_Reference_Modules(string csprojPath)
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var modulesDir = Path.Combine(root, "src", "Modules");
        var moduleProjFiles = Directory.Exists(modulesDir)
            ? Directory.GetFiles(modulesDir, "*.csproj", SearchOption.AllDirectories).Select(Path.GetFullPath).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var doc = new System.Xml.XmlDocument();
        doc.Load(csprojPath);
        var mgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
        mgr.AddNamespace("msb", doc.DocumentElement!.NamespaceURI);
        var references = doc.SelectNodes("//msb:ProjectReference", mgr);
        if (references == null) return;

        foreach (System.Xml.XmlNode reference in references)
        {
            var include = reference?.Attributes?["Include"]?.Value;
            if (string.IsNullOrEmpty(include)) continue;
            var refPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(csprojPath)!, include));
            if (moduleProjFiles.Contains(refPath))
            {
                Assert.Fail(
                    $"❌ ADR-0002 违规: Host 项目 {Path.GetFileName(csprojPath)} 不应直接引用 Modules 下的项目: {Path.GetFileName(refPath)}。\n" +
                    $"项目路径: {csprojPath}\n" +
                    $"引用路径: {include}\n" +
                    $"修复建议：将共享契约移到 Platform/BuildingBlocks，或改为消息通信。");
            }
        }
    }

    public static IEnumerable<object[]> GetHostProjectFiles()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var hostDir = Path.Combine(root, "src", "Host");
        if (!Directory.Exists(hostDir)) yield break;

        var csprojs = Directory.GetFiles(hostDir, "*.csproj", SearchOption.AllDirectories);
        foreach (var csproj in csprojs)
        {
            yield return new object[] { csproj };
        }
    }

    [Fact(DisplayName = "ADR-0002.12: Program.cs 应该保持简洁（建议 ≤ 50 行）")]
    public void Program_Cs_Should_Be_Concise()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var hostDir = Path.Combine(root, "src", "Host");
        if (!Directory.Exists(hostDir)) return;

        var programFiles = Directory.GetFiles(hostDir, "Program.cs", SearchOption.AllDirectories);

        foreach (var programFile in programFiles)
        {
            var lines = File.ReadAllLines(programFile)
                .Where(line => !string.IsNullOrWhiteSpace(line)) // 排除空行
                .Where(line => !line.TrimStart().StartsWith("//")) // 排除注释行
                .ToList();

            Assert.True(lines.Count <= 50,
                $"❌ ADR-0002 建议: Program.cs 建议保持在 50 行以内，当前 {lines.Count} 行。\n" +
                $"文件路径: {programFile}\n" +
                $"修复建议：将启动逻辑移到 PlatformBootstrapper 和 ApplicationBootstrapper。");
        }
    }

    #endregion

    #region 4. 三层依赖方向验证

    [Fact(DisplayName = "ADR-0002.13: 验证完整的三层依赖方向 (Host -> Application -> Platform)")]
    public void Verify_Complete_Three_Layer_Dependency_Direction()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;

        // Platform 不应依赖 Application 或 Host
        var platformResult = Types.InAssembly(platformAssembly)
            .ShouldNot().HaveDependencyOnAny("Zss.BilliardHall.Application", "Zss.BilliardHall.Host")
            .GetResult();

        Assert.True(platformResult.IsSuccessful,
            $"❌ ADR-0002 违规: Platform 不应依赖 Application 或 Host。\n" +
            $"违规类型: {string.Join(", ", platformResult.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。");

        // Application 不应依赖 Host
        var applicationResult = Types.InAssembly(applicationAssembly)
            .ShouldNot().HaveDependencyOn("Zss.BilliardHall.Host")
            .GetResult();

        Assert.True(applicationResult.IsSuccessful,
            $"❌ ADR-0002 违规: Application 不应依赖 Host。\n" +
            $"违规类型: {string.Join(", ", applicationResult.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。");
    }

    #endregion
}
