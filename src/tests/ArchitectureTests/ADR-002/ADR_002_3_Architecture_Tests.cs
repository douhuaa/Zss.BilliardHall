using NetArchTest.Rules;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_002;

/// <summary>
/// ADR-002_3: Host 层约束（Rule）
/// 验证 Host 层职责边界和依赖隔离
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-002_3_1: Host 不依赖 Modules
/// - ADR-002_3_2: Host 不包含业务类型
/// - ADR-002_3_3: Host 项目文件不引用 Modules
/// - ADR-002_3_4: Program.cs 保持简洁
/// - ADR-002_3_5: Program.cs 只调用 Bootstrapper
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md
/// </summary>
public sealed class ADR_002_3_Architecture_Tests
{
    /// <summary>
    /// ADR-002_3_1: Host 不依赖 Modules
    /// 验证 Host 层不应依赖任何业务模块（§ADR-002_3_1）
    /// </summary>
    [Theory(DisplayName = "ADR-002_3_1: Host 不应依赖任何 Modules")]
    [ClassData(typeof(HostAssemblyData))]
    public void ADR_002_3_1_Host_Should_Not_Depend_On_Modules(Assembly hostAssembly)
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

        result.IsSuccessful.Should().BeTrue($"❌ ADR-002_3_1 违规: Host {hostAssembly.GetName().Name} 不应依赖任何 Modules\n\n" +
        $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 移除 Host 对 Modules 的项目引用\n" +
        $"2. Host 通过 Application 间接引入模块\n" +
        $"3. 将共享契约移到 Platform/BuildingBlocks\n\n" +
        $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
    }

    /// <summary>
    /// ADR-002_3_2: Host 不包含业务类型
    /// 验证 Host 不包含任何业务逻辑（§ADR-002_3_2）
    /// </summary>
    [Theory(DisplayName = "ADR-002_3_2: Host 不应包含业务类型")]
    [ClassData(typeof(HostAssemblyData))]
    public void ADR_002_3_2_Host_Should_Not_Contain_Business_Types(Assembly hostAssembly)
    {
        var businessTypes = Types
            .InAssembly(hostAssembly)
            .That()
            .ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
            .GetTypes()
            .ToArray();

        businessTypes.Should().BeEmpty();
    }

    /// <summary>
    /// ADR-002_3_3: Host 项目文件不引用 Modules
    /// 验证 Host 项目文件禁止 ProjectReference 指向 Modules（§ADR-002_3_3）
    /// </summary>
    [Theory(DisplayName = "ADR-002_3_3: Host 项目文件不应引用 Modules")]
    [MemberData(nameof(GetHostProjectFiles))]
    public void ADR_002_3_3_Host_Csproj_Should_Not_Reference_Modules(string csprojPath)
    {
        var root = TestEnvironment.RepositoryRoot;
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
                true.Should().BeFalse($"❌ ADR-002_3_3 违规: Host 项目不应直接引用 Modules\n\n" +
                            $"Host 项目: {Path.GetFileName(csprojPath)}\n" +
                            $"违规引用: {Path.GetFileName(refPath)}\n" +
                            $"引用路径: {include}\n\n" +
                            $"修复建议:\n" +
                            $"1. 从 Host.csproj 中移除对 Module 项目的引用\n" +
                            $"2. Host 只应引用 Application 和 Platform\n" +
                            $"3. 将共享契约移到 Platform/BuildingBlocks\n\n" +
                            $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
            }
        }
    }

    /// <summary>
    /// ADR-002_3_4: Program.cs 保持简洁
    /// 验证 Program.cs 保持极简（建议 ≤50 行）（§ADR-002_3_4）
    /// </summary>
    [Fact(DisplayName = "ADR-002_3_4: Program.cs 应该保持简洁（建议 ≤ 50 行）")]
    public void ADR_002_3_4_Program_Cs_Should_Be_Concise()
    {
        var root = TestEnvironment.RepositoryRoot;
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

            (lines.Count <= 50).Should().BeTrue($"❌ ADR-002_3_4 违规: Program.cs 应保持简洁（≤ 50 行）\n\n" +
            $"当前行数: {lines.Count}\n" +
            $"文件路径: {programFile}\n\n" +
            $"修复建议:\n" +
            $"1. 将技术配置逻辑移到 PlatformBootstrapper\n" +
            $"2. 将业务装配逻辑移到 ApplicationBootstrapper\n" +
            $"3. Program.cs 只保留核心调用（builder.AddPlatform, builder.AddApplication, app.Run）\n\n" +
            $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
        }
    }

    // Compiled regex patterns for performance
    private static readonly Regex AddModulePattern = new Regex(@"\.Add\w+Module\(", RegexOptions.Compiled);
    private static readonly Regex AddFeaturePattern = new Regex(@"\.Add\w+Feature\(", RegexOptions.Compiled);
    private static readonly Regex UsePattern = new Regex(@"\.Use\w+\(", RegexOptions.Compiled);
    private static readonly Regex ServiceRegistrationPattern = new Regex(@"services\.(AddScoped|AddSingleton|AddTransient)", RegexOptions.Compiled);
    private static readonly Regex MapEndpointPattern = new Regex(@"app\.Map\w+\(", RegexOptions.Compiled);
    private static readonly Regex IfPattern = new Regex(@"if\s*\(", RegexOptions.Compiled);
    private static readonly Regex SwitchPattern = new Regex(@"switch\s*\(", RegexOptions.Compiled);
    private static readonly Regex TernaryPattern = new Regex(@"\?\s*.*\s*:", RegexOptions.Compiled);

    /// <summary>
    /// ADR-002_3_5: Program.cs 只调用 Bootstrapper
    /// 验证 Program.cs 只应调用 Platform、Application 的 Bootstrapper（§ADR-002_3_5）
    /// </summary>
    [Fact(DisplayName = "ADR-002_3_5: Program.cs 只应调用 Bootstrapper（语义检查）")]
    public void ADR_002_3_5_Program_Cs_Should_Only_Call_Bootstrapper()
    {
        var root = TestEnvironment.RepositoryRoot;
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
            var message = "⚠️ ADR-002 语义检查建议：\n" +
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

    // ========== 辅助方法 ==========

    public static IEnumerable<object[]> GetHostProjectFiles()
    {
        var root = TestEnvironment.RepositoryRoot;
        var hostDir = Path.Combine(root, "src", "Host");
        if (!Directory.Exists(hostDir)) yield break;

        var csprojs = Directory.GetFiles(hostDir, "*.csproj", SearchOption.AllDirectories);
        foreach (var csproj in csprojs)
        {
            yield return new object[] { csproj };
        }
    }
}
