using NetArchTest.Rules;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0002: Platform / Application / Host 三层启动体系
/// 验证三层依赖约束和启动职责分工
/// 
/// 测试映射：
/// - Platform_Should_Not_Depend_On_Application → ADR-0002.1 (Platform 不应依赖 Application)
/// - Platform_Should_Not_Depend_On_Host → ADR-0002.2 (Platform 不应依赖 Host)
/// - Platform_Should_Not_Depend_On_Modules → ADR-0002.3 (Platform 不应依赖任何 Modules)
/// - Platform_Should_Have_Single_Bootstrapper_Entry_Point → ADR-0002.4 (Platform 应有唯一的 PlatformBootstrapper 入口)
/// - Application_Should_Not_Depend_On_Host → ADR-0002.5 (Application 不应依赖 Host)
/// - Application_Should_Not_Depend_On_Modules → ADR-0002.6 (Application 不应依赖任何 Modules)
/// - Application_Should_Have_Single_Bootstrapper_Entry_Point → ADR-0002.7 (Application 应有唯一的 ApplicationBootstrapper 入口)
/// - Application_Should_Not_Use_HttpContext → ADR-0002.8 (Application 不应包含 HttpContext 等 Host 专属类型)
/// - Host_Should_Not_Depend_On_Modules → ADR-0002.9 (Host 不应依赖任何 Modules)
/// - Host_Should_Not_Contain_Business_Types → ADR-0002.10 (Host 不应包含业务类型)
/// - Host_Csproj_Should_Not_Reference_Modules → ADR-0002.11 (Host 项目文件不应引用 Modules)
/// - Program_Cs_Should_Be_Concise → ADR-0002.12 (Program.cs 应保持简洁)
/// - Program_Cs_Should_Only_Call_Bootstrapper → ADR-0002.13 (Program.cs 只应调用 Bootstrapper)
/// - Verify_Complete_Three_Layer_Dependency_Direction → ADR-0002.14 (验证完整的三层依赖方向)
/// </summary>
public sealed class ADR_0002_Architecture_Tests
{

    #region 1. Platform 层约束

    [Fact(DisplayName = "ADR-0002.1: Platform 不应依赖 Application")]
    public void Platform_Should_Not_Depend_On_Application()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var result = Types
            .InAssembly(platformAssembly)
            .ShouldNot()
            .HaveDependencyOn("Zss.BilliardHall.Application")
            .GetResult();

        Assert.True(result.IsSuccessful,
        $"❌ ADR-0002.1 违规: Platform 层不应依赖 Application 层\n\n" +
        $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 移除 Platform 对 Application 的引用\n" +
        $"2. 将共享的技术抽象提取到 Platform 层\n" +
        $"3. 确保依赖方向正确: Host → Application → Platform\n\n" +
        $"参考: docs/copilot/adr-0002.prompts.md (场景 2)");
    }

    [Fact(DisplayName = "ADR-0002.2: Platform 不应依赖 Host")]
    public void Platform_Should_Not_Depend_On_Host()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var result = Types
            .InAssembly(platformAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Zss.BilliardHall.Host", "Zss.BilliardHall.Host.Web", "Zss.BilliardHall.Host.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful,
        $"❌ ADR-0002.2 违规: Platform 层不应依赖 Host 层\n\n" +
        $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 移除 Platform 对 Host 的引用\n" +
        $"2. Platform 只提供技术基座能力（日志、追踪、异常处理）\n" +
        $"3. 不感知运行形态（Web/Worker）\n\n" +
        $"参考: docs/copilot/adr-0002.prompts.md (场景 2)");
    }

    [Fact(DisplayName = "ADR-0002.3: Platform 不应依赖任何 Modules")]
    public void Platform_Should_Not_Depend_On_Modules()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var result = Types
            .InAssembly(platformAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ModuleAssemblyData
                .ModuleNames
                .Select(m => $"Zss.BilliardHall.Modules.{m}")
                .ToArray())
            .GetResult();

        Assert.True(result.IsSuccessful,
        $"❌ ADR-0002.3 违规: Platform 层不应依赖任何 Modules\n\n" +
        $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 移除 Platform 对 Modules 的引用\n" +
        $"2. Platform 是技术基座，不感知业务模块\n" +
        $"3. 将业务无关的通用逻辑提取到 BuildingBlocks\n\n" +
        $"参考: docs/copilot/adr-0002.prompts.md (场景 2)");
    }

    [Fact(DisplayName = "ADR-0002.4: Platform 应有唯一的 PlatformBootstrapper 入口")]
    public void Platform_Should_Have_Single_Bootstrapper_Entry_Point()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var bootstrappers = Types
            .InAssembly(platformAssembly)
            .That()
            .HaveNameEndingWith("Bootstrapper")
            .And()
            .AreClasses()
            .GetTypes()
            .ToList();

        Assert.True(bootstrappers.Count > 0,
        $"❌ ADR-0002.4 违规: Platform 层必须包含 Bootstrapper 入口点\n\n" + $"修复建议:\n" + $"1. 创建 PlatformBootstrapper 类作为 Platform 层的唯一入口\n" + $"2. 在 PlatformBootstrapper 中封装所有技术配置\n" + $"3. 提供 public static void Configure() 方法\n\n" + $"参考: docs/copilot/adr-0002.prompts.md (场景 2)");

        // 验证有 Configure 方法
        var platformBootstrapper = bootstrappers.FirstOrDefault(t => t.Name == "PlatformBootstrapper");
        Assert.NotNull(platformBootstrapper);

        var configureMethods = platformBootstrapper
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Configure")
            .ToList();

        Assert.True(configureMethods.Count > 0,
        $"❌ ADR-0002.4 违规: PlatformBootstrapper 必须包含 Configure 方法\n\n" +
        $"修复建议:\n" +
        $"1. 在 PlatformBootstrapper 中添加 public static void Configure() 方法\n" +
        $"2. 方法签名应接受 IServiceCollection, IConfiguration, IHostEnvironment\n" +
        $"3. 在此方法中注册所有 Platform 层服务\n\n" +
        $"参考: docs/copilot/adr-0002.prompts.md (场景 2)");
    }

    #endregion

    #region 2. Application 层约束

    [Fact(DisplayName = "ADR-0002.5: Application 不应依赖 Host")]
    public void Application_Should_Not_Depend_On_Host()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var result = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Zss.BilliardHall.Host", "Zss.BilliardHall.Host.Web", "Zss.BilliardHall.Host.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful,
        $"❌ ADR-0002.5 违规: Application 层不应依赖 Host 层\n\n" +
        $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 移除 Application 对 Host 的引用\n" +
        $"2. Application 定义\"系统是什么\"，不应感知运行形态\n" +
        $"3. 使用抽象替代具体的 Host 类型（如 ICurrentUserProvider 替代 HttpContext）\n\n" +
        $"参考: docs/copilot/adr-0002.prompts.md (场景 3)");
    }

    [Fact(DisplayName = "ADR-0002.6: Application 不应依赖任何 Modules")]
    public void Application_Should_Not_Depend_On_Modules()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var result = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ModuleAssemblyData
                .ModuleNames
                .Select(m => $"Zss.BilliardHall.Modules.{m}")
                .ToArray())
            .GetResult();

        Assert.True(result.IsSuccessful,
        $"❌ ADR-0002.6 违规: Application 层不应依赖任何 Modules\n\n" +
        $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 移除 Application 对 Modules 的引用\n" +
        $"2. Application 通过扫描和反射加载模块，而非直接引用\n" +
        $"3. 使用 ApplicationBootstrapper 自动发现并注册模块\n\n" +
        $"参考: docs/copilot/adr-0002.prompts.md (场景 3)");
    }

    [Fact(DisplayName = "ADR-0002.7: Application 应有唯一的 ApplicationBootstrapper 入口")]
    public void Application_Should_Have_Single_Bootstrapper_Entry_Point()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var bootstrappers = Types
            .InAssembly(applicationAssembly)
            .That()
            .HaveNameEndingWith("Bootstrapper")
            .And()
            .AreClasses()
            .GetTypes()
            .ToList();

        Assert.True(bootstrappers.Count > 0,
        $"❌ ADR-0002.7 违规: Application 层必须包含 Bootstrapper 入口点\n\n" + $"修复建议:\n" + $"1. 创建 ApplicationBootstrapper 类作为 Application 层的唯一入口\n" + $"2. 在 ApplicationBootstrapper 中封装模块扫描和业务装配\n" + $"3. 提供 public static void Configure() 方法\n\n" + $"参考: docs/copilot/adr-0002.prompts.md (场景 3)");

        var applicationBootstrapper = bootstrappers.FirstOrDefault(t => t.Name == "ApplicationBootstrapper");
        Assert.NotNull(applicationBootstrapper);

        var configureMethods = applicationBootstrapper
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Configure")
            .ToList();

        Assert.True(configureMethods.Count > 0,
        $"❌ ADR-0002.7 违规: ApplicationBootstrapper 必须包含 Configure 方法\n\n" +
        $"修复建议:\n" +
        $"1. 在 ApplicationBootstrapper 中添加 public static void Configure() 方法\n" +
        $"2. 方法签名应接受 IServiceCollection, IConfiguration\n" +
        $"3. 在此方法中注册所有 Application 层服务和模块\n\n" +
        $"参考: docs/copilot/adr-0002.prompts.md (场景 3)");
    }

    [Fact(DisplayName = "ADR-0002.8: Application 不应包含 HttpContext 等 Host 专属类型")]
    public void Application_Should_Not_Use_HttpContext()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var result = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore.Http.HttpContext")
            .GetResult();

        Assert.True(result.IsSuccessful,
        $"❌ ADR-0002.8 违规: Application 层不应使用 HttpContext 等 Host 专属类型\n\n" +
        $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 移除对 HttpContext 的依赖\n" +
        $"2. 创建业务抽象（如 ICurrentUserProvider）\n" +
        $"3. 在 Host 层实现抽象，Application 层只依赖接口\n\n" +
        $"参考: docs/copilot/adr-0002.prompts.md (场景 3, 反模式 3)");
    }

    #endregion

    #region 3. Host 层约束

    [Theory(DisplayName = "ADR-0002.9: Host 不应依赖任何 Modules")]
    [ClassData(typeof(HostAssemblyData))]
    public void Host_Should_Not_Depend_On_Modules(Assembly hostAssembly)
    {
        var forbidden = ModuleAssemblyData
            .ModuleNames
            .Select(m => $"Zss.BilliardHall.Modules.{m}")
            .ToArray();
        if (forbidden.Length == 0)
        {
            // 如果没有模块，跳过测试
            return;
        }

        var result = Types
            .InAssembly(hostAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful,
        $"❌ ADR-0002.9 违规: Host {hostAssembly.GetName().Name} 不应依赖任何 Modules\n\n" +
        $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 移除 Host 对 Modules 的项目引用\n" +
        $"2. Host 通过 Application 间接引入模块\n" +
        $"3. 将共享契约移到 Platform/BuildingBlocks\n\n" +
        $"参考: docs/copilot/adr-0002.prompts.md (场景 1, 反模式 4)");
    }

    [Theory(DisplayName = "ADR-0002.10: Host 不应包含业务类型")]
    [ClassData(typeof(HostAssemblyData))]
    public void Host_Should_Not_Contain_Business_Types(Assembly hostAssembly)
    {
        var businessTypes = Types
            .InAssembly(hostAssembly)
            .That()
            .ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
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
            ? Directory
                .GetFiles(modulesDir, "*.csproj", SearchOption.AllDirectories)
                .Select(Path.GetFullPath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
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
                Assert.Fail($"❌ ADR-0002.11 违规: Host 项目不应直接引用 Modules\n\n" +
                            $"Host 项目: {Path.GetFileName(csprojPath)}\n" +
                            $"违规引用: {Path.GetFileName(refPath)}\n" +
                            $"引用路径: {include}\n\n" +
                            $"修复建议:\n" +
                            $"1. 从 Host.csproj 中移除对 Module 项目的引用\n" +
                            $"2. Host 只应引用 Application 和 Platform\n" +
                            $"3. 将共享契约移到 Platform/BuildingBlocks\n\n" +
                            $"参考: docs/copilot/adr-0002.prompts.md (场景 1, 反模式 4)");
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
            var lines = File
                .ReadAllLines(programFile)
                .Where(line => !string.IsNullOrWhiteSpace(line)) // 排除空行
                .Where(line => !line
                    .TrimStart()
                    .StartsWith("//")) // 排除注释行
                .ToList();

            Assert.True(lines.Count <= 50,
            $"❌ ADR-0002.12 违规: Program.cs 应保持简洁（≤ 50 行）\n\n" +
            $"当前行数: {lines.Count}\n" +
            $"文件路径: {programFile}\n\n" +
            $"修复建议:\n" +
            $"1. 将技术配置逻辑移到 PlatformBootstrapper\n" +
            $"2. 将业务装配逻辑移到 ApplicationBootstrapper\n" +
            $"3. Program.cs 只保留核心调用（builder.AddPlatform, builder.AddApplication, app.Run）\n\n" +
            $"参考: docs/copilot/adr-0002.prompts.md (场景 4)");
        }
    }

    // Compiled regex patterns for performance
    private static readonly System.Text.RegularExpressions.Regex AddModulePattern = new System.Text.RegularExpressions.Regex(@"\.Add\w+Module\(", System.Text.RegularExpressions.RegexOptions.Compiled);
    private static readonly System.Text.RegularExpressions.Regex AddFeaturePattern = new System.Text.RegularExpressions.Regex(@"\.Add\w+Feature\(", System.Text.RegularExpressions.RegexOptions.Compiled);
    private static readonly System.Text.RegularExpressions.Regex UsePattern = new System.Text.RegularExpressions.Regex(@"\.Use\w+\(", System.Text.RegularExpressions.RegexOptions.Compiled);
    private static readonly System.Text.RegularExpressions.Regex ServiceRegistrationPattern = new System.Text.RegularExpressions.Regex(@"services\.(AddScoped|AddSingleton|AddTransient)", System.Text.RegularExpressions.RegexOptions.Compiled);
    private static readonly System.Text.RegularExpressions.Regex MapEndpointPattern = new System.Text.RegularExpressions.Regex(@"app\.Map\w+\(", System.Text.RegularExpressions.RegexOptions.Compiled);
    private static readonly System.Text.RegularExpressions.Regex IfPattern = new System.Text.RegularExpressions.Regex(@"if\s*\(", System.Text.RegularExpressions.RegexOptions.Compiled);
    private static readonly System.Text.RegularExpressions.Regex SwitchPattern = new System.Text.RegularExpressions.Regex(@"switch\s*\(", System.Text.RegularExpressions.RegexOptions.Compiled);
    private static readonly System.Text.RegularExpressions.Regex TernaryPattern = new System.Text.RegularExpressions.Regex(@"\?\s*.*\s*:", System.Text.RegularExpressions.RegexOptions.Compiled);

    [Fact(DisplayName = "ADR-0002.13: Program.cs 只应调用 Bootstrapper（语义检查）")]
    public void Program_Cs_Should_Only_Call_Bootstrapper()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var hostDir = Path.Combine(root, "src", "Host");
        if (!Directory.Exists(hostDir)) return;

        var programFiles = Directory.GetFiles(hostDir, "Program.cs", SearchOption.AllDirectories);
        var violations = new List<string>();

        var suspiciousPatterns = new[] {
            (AddModulePattern, @"\.Add\w+Module\("),
            (AddFeaturePattern, @"\.Add\w+Feature\("),
            (UsePattern, @"\.Use\w+\("),
            (ServiceRegistrationPattern, @"services\.Add(Scoped|Singleton|Transient)"),
            (MapEndpointPattern, @"app\.Map\w+\("),
        };

        var conditionalPatterns = new[] {
            (IfPattern, "if"),
            (SwitchPattern, "switch"),
            (TernaryPattern, "ternary operator"),
        };

        var allowedPatterns = new[] {
            "Bootstrapper",
            "WebApplication",
            "builder.Build()",
            "app.Run()",
        };

        foreach (var programFile in programFiles)
        {
            var lines = File.ReadAllLines(programFile);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i]
                    .Trim();

                // 跳过注释和空行
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                foreach (var (regex, patternName) in suspiciousPatterns)
                {
                    if (regex.IsMatch(line))
                    {
                        // 检查是否在允许的上下文中
                        var isAllowed = allowedPatterns.Any(allowed => line.Contains(allowed));
                        if (!isAllowed)
                        {
                            violations.Add($"❌ {Path.GetFileName(programFile)} (第 {i + 1} 行): 检测到非 Bootstrapper 扩展调用\n" + $"   代码: {line}\n" + $"   匹配模式: {patternName}");
                        }
                    }
                }

                foreach (var (regex, patternName) in conditionalPatterns)
                {
                    if (regex.IsMatch(line))
                    {
                        violations.Add($"⚠️ {Path.GetFileName(programFile)} (第 {i + 1} 行): 检测到条件分支逻辑 ({patternName})\n" + $"   代码: {line}\n" + $"   建议: 环境判断应在 Bootstrapper 内部处理，而非 Program.cs");
                    }
                }
            }
        }

        // 这是建议性检查，记录但不阻断（当前 Program.cs 简单，未来可能需要）
        if (violations.Any())
        {
            var message = "⚠️ ADR-0002 语义检查建议：\n" +
                          string.Join("\n\n", violations) +
                          "\n\n建议：\n" +
                          "1. Program.cs 应该只调用 Bootstrapper.Configure() 等入口方法\n" +
                          "2. 所有 AddXxxModule/AddXxxFeature 调用应封装在 Bootstrapper 中\n" +
                          "3. 环境判断（Development/Production）应在 Bootstrapper 内部处理\n" +
                          "4. 直接的服务注册和端点配置应移到对应的 Bootstrapper";

            // 记录为建议，不阻断构建
            System.Diagnostics.Trace.WriteLine(message);
        }
    }

    #endregion

    #region 4. 三层依赖方向验证

    [Fact(DisplayName = "ADR-0002.14: 验证完整的三层依赖方向 (Host -> Application -> Platform)")]
    public void Verify_Complete_Three_Layer_Dependency_Direction()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;

        // Platform 不应依赖 Application 或 Host
        var platformResult = Types
            .InAssembly(platformAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Zss.BilliardHall.Application", "Zss.BilliardHall.Host")
            .GetResult();

        Assert.True(platformResult.IsSuccessful,
        $"❌ ADR-0002.14 违规: Platform 不应依赖 Application 或 Host\n\n" +
        $"违规类型:\n{string.Join("\n", platformResult.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 确保三层依赖方向: Host → Application → Platform\n" +
        $"2. Platform 是最底层，不依赖任何上层\n" +
        $"3. 移除违规的依赖引用\n\n" +
        $"参考: docs/copilot/adr-0002.prompts.md");

        // Application 不应依赖 Host
        var applicationResult = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Zss.BilliardHall.Host")
            .GetResult();

        Assert.True(applicationResult.IsSuccessful,
        $"❌ ADR-0002.14 违规: Application 不应依赖 Host\n\n" +
        $"违规类型:\n{string.Join("\n", applicationResult.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 确保三层依赖方向: Host → Application → Platform\n" +
        $"2. Application 不感知运行形态\n" +
        $"3. 移除对 Host 的依赖\n\n" +
        $"参考: docs/copilot/adr-0002.prompts.md");
    }

    #endregion

}
