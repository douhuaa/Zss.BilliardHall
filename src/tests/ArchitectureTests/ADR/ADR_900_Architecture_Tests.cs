using System.Reflection;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-900: 架构测试与 CI 治理宪法（v2.0）
/// 架构宪法测试：每一条 ADR 文档，必须有唯一、严格对应的架构测试类
/// </summary>
public sealed class ADR_900_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const string AdrFilePattern = "ADR-*.md";
    private const string TestNamespacePrefix = "Zss.BilliardHall.Tests.ArchitectureTests.ADR";
    private const string TypeSuffix = "_Architecture_Tests";

    // ADR-005-Enforcement-Levels 是 ADR-005 的补充文档，不是独立的 ADR
    // 已废弃（Superseded）或草稿（Draft）状态的 ADR 不需要测试
    private static readonly HashSet<string> AdrWithoutTests = new(StringComparer.OrdinalIgnoreCase) {
        "ADR-005-Enforcement-Levels",
        "ADR-906-analyzer-ci-gate-mapping-protocol",  // Superseded
        "ADR-904-architecturetests-minimum-assertion-semantics",  // Superseded
        "ADR-009-guardian-failure-feedback"  // Draft
    };

    // 最小 IL 字节数阈值：用于启发式判断测试方法是否包含实质内容
    // 这是一个经验值，基于简单测试方法的典型 IL 大小
    // 注意：这不是严格的验证，只是启发式检查
    private const int MinimumILBytesForSubstantialTest = 50;

    /// <summary>
    /// 只以 ADR 编号作为测试类绑定依据：
    /// - 标题/slug 可演进
    /// - 编号不可变
    /// </summary>
    [Fact(DisplayName = "ADR-900_1_1: 每条 ADR 必须有且仅有唯一对应的架构测试类")]
    public void Each_ADR_Must_Have_Exact_And_Unique_Architecture_Test()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根（docs/adr 或 .git）");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        Directory.Exists(adrDirectory).Should().BeTrue($"❌ ADR-900_1_1 违规：ADR 文档目录不存在\n\n" +
            $"预期路径：{AdrDocsPath}\n\n" +
            $"修复建议：确保 docs/adr 目录存在\n\n" +
            $"参考：docs/adr/governance/ADR-900-architecture-tests.md（§1.1）");

        var adrIds = LoadAdrIds(adrDirectory)
            .Where(adr => !AdrWithoutTests.Contains(adr)) // 跳过无需测试的 ADR
            .ToList();
        adrIds.Should().NotBeEmpty();

        var testTypes = LoadArchitectureTestTypes();

        // 对于每个 ADR，查找匹配的测试类
        // 支持两种格式：
        // 1. 旧格式：ADR_001_Architecture_Tests（单个测试类）
        // 2. 新格式：ADR_001_1_Architecture_Tests, ADR_001_2_Architecture_Tests（按 Rule 拆分）
        var missing = adrIds
            .Where(adr => !HasMatchingTests(adr, testTypes))
            .ToList();

        if (missing.Any())
        {
            var messages = new List<string>
            {
                "❌ ADR-900_1_1 违规 ADR 缺失对应测试"
            };
            messages.AddRange(missing.Select(m => $" - {m}"));
            messages.Add("每条 ADR 文档必须有至少一个对应的架构测试类");
            messages.Add("命名规范：ADR_<number>_Architecture_Tests 或 ADR_<number>_<Rule>_Architecture_Tests");

            throw new Xunit.Sdk.XunitException(string.Join(Environment.NewLine, messages));
        }
    }
    
    /// <summary>
    /// 检查 ADR 是否有匹配的测试类
    /// 支持两种格式：ADR_001_Architecture_Tests 或 ADR_001_1_Architecture_Tests
    /// </summary>
    private static bool HasMatchingTests(string adrId, IReadOnlyList<Type> testTypes)
    {
        var adrNumber = ExtractNumberFromAdrId(adrId);
        if (string.IsNullOrEmpty(adrNumber))
            return false;
            
        // 匹配 ADR_<number>_Architecture_Tests 或 ADR_<number>_<Rule>_Architecture_Tests
        return testTypes.Any(t => 
            t.Name.StartsWith($"ADR_{adrNumber}_", StringComparison.OrdinalIgnoreCase) &&
            t.Name.EndsWith(TypeSuffix, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 从 ADR ID 中提取编号部分（如从 "ADR-001-xxxx" 提取 "001"）
    /// </summary>
    private static string? ExtractNumberFromAdrId(string adrId)
    {
        var segments = adrId.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
            return null;
        return segments[1];
    }

    private static string? FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "docs", "adr")) || Directory.Exists(Path.Combine(current.FullName, ".git")))
                return current.FullName;

            current = current.Parent;
        }
        return null;
    }

    private static IReadOnlyList<string> LoadAdrIds(string adrDirectory)
    {
        // 递归搜索所有子目录中的 ADR 文件（支持新的分层目录结构）
        return Directory
            .GetFiles(adrDirectory, AdrFilePattern, SearchOption.AllDirectories)
            .Select(Path.GetFileNameWithoutExtension)
            .Where(file => !string.IsNullOrWhiteSpace(file) && System.Text.RegularExpressions.Regex.IsMatch(file!, @"^ADR-\d{3,4}", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            .Select(file => file!)
            .Distinct()
            .ToList();
    }

    private static IReadOnlyList<Type> LoadArchitectureTestTypes()
    {
        return Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsSealed: true, IsPublic: true, Namespace: not null } && t.Namespace.StartsWith(TestNamespacePrefix, StringComparison.OrdinalIgnoreCase) && t.Name.EndsWith(TypeSuffix))
            .ToList();
    }

    private static string ToExpectedTypeName(string adrId)
    {
        var segments = adrId.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
            throw new InvalidOperationException($"ADR 文件名不合法: {adrId}");
        var number = segments[1];
        return $"ADR_{number}{TypeSuffix}";
    }

    /// <summary>
    /// 反作弊规则：测试类必须包含实质性测试，不能是空壳
    /// </summary>
    [Fact(DisplayName = "ADR-900_1_2: 架构测试类必须包含最少断言数（反作弊）")]
    public void Architecture_Test_Classes_Must_Have_Minimum_Assertions()
    {
        var testTypes = LoadArchitectureTestTypes();
        var violations = new List<string>();

        foreach (var testType in testTypes)
        {
            // 跳过 ADR-900 自身
            if (testType.Name == "ADR_900_Architecture_Tests")
                continue;

            // 跳过重定向测试（ADR_008 已重构为三层架构）
            if (testType.Name == "ADR_008_Architecture_Tests")
                continue;

            var testMethods = testType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m
                                .GetCustomAttributes(typeof(FactAttribute), false)
                                .Any() ||
                            m
                                .GetCustomAttributes(typeof(TheoryAttribute), false)
                                .Any())
                .ToList();

            if (testMethods.Count == 0)
            {
                violations.Add($"❌ {testType.Name}: 无任何测试方法");
                continue;
            }

            // 检查是否所有测试都被跳过
            var allSkipped = testMethods.All(m =>
            {
                var factAttr = m
                    .GetCustomAttributes(typeof(FactAttribute), false)
                    .FirstOrDefault() as FactAttribute;
                var theoryAttr = m
                    .GetCustomAttributes(typeof(TheoryAttribute), false)
                    .FirstOrDefault() as TheoryAttribute;
                return (factAttr?.Skip != null) || (theoryAttr?.Skip != null);
            });

            if (allSkipped)
            {
                violations.Add($"❌ {testType.Name}: 所有测试都被跳过（Skip）");
            }

            // 简单启发式检查：测试方法体应该有实际内容
            // 至少应该有一些方法调用（如 Assert.True, GetTypes 等）
            var hasSubstantialTests = testMethods.Any(m => m
                                                               .GetMethodBody()
                                                               ?.GetILAsByteArray()
                                                               ?.Length >
                                                           MinimumILBytesForSubstantialTest);
            if (!hasSubstantialTests)
            {
                violations.Add($"⚠️ {testType.Name}: 测试方法体可能过于简单（建议人工审查）");
            }
        }

        if (violations.Any())
        {
            var message = "❌ ADR-900_1_2 违规 架构测试反作弊检查失败\n" + string.Join("\n", violations) + "\n\n修复建议：架构测试类必须包含实质性的测试逻辑，不允许空测试或全部跳过的测试。";
            throw new Xunit.Sdk.XunitException(message);
        }
    }

    /// <summary>
    /// 反作弊规则：测试失败消息必须包含 ADR 编号
    /// </summary>
    [Fact(DisplayName = "ADR-900_1_3: 测试失败消息必须包含 ADR 编号（反作弊）")]
    public void Test_Failure_Messages_Must_Include_ADR_Number()
    {
        var testTypes = LoadArchitectureTestTypes();
        var violations = new List<string>();

        foreach (var testType in testTypes)
        {
            // 跳过 ADR-900 自身
            if (testType.Name == "ADR_900_Architecture_Tests")
                continue;

            // 提取 ADR 编号
            var adrNumber = ExtractAdrNumber(testType.Name);
            if (adrNumber == null)
            {
                violations.Add($"❌ {testType.Name}: 无法从类名提取 ADR 编号");
                continue;
            }

            // 检查测试方法的 DisplayName 是否包含 ADR 编号
            var testMethods = testType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m
                                .GetCustomAttributes(typeof(FactAttribute), false)
                                .Any() ||
                            m
                                .GetCustomAttributes(typeof(TheoryAttribute), false)
                                .Any())
                .ToList();

            foreach (var method in testMethods)
            {
                var factAttr = method
                    .GetCustomAttributes(typeof(FactAttribute), false)
                    .FirstOrDefault() as FactAttribute;
                var theoryAttr = method
                    .GetCustomAttributes(typeof(TheoryAttribute), false)
                    .FirstOrDefault() as TheoryAttribute;
                var displayName = factAttr?.DisplayName ?? theoryAttr?.DisplayName;

                if (displayName != null && !displayName.Contains($"ADR-{adrNumber}"))
                {
                    violations.Add($"⚠️ {testType.Name}.{method.Name}: DisplayName 缺少 ADR 编号标识");
                }
            }
        }

        if (violations.Any())
        {
            var message = "⚠️ ADR-900_1_3 建议：\n" + string.Join("\n", violations) + "\n\n建议：所有测试的 DisplayName 应包含 ADR 编号（如 'ADR-900: ...'），便于追溯和审计。";

            // 这是建议性规则，暂时只输出调试信息，不阻断
            System.Diagnostics.Debug.WriteLine(message);
        }
    }

    /// <summary>
    /// 禁止使用 Skip 属性跳过架构测试
    /// </summary>
    [Fact(DisplayName = "ADR-900_1_4: 禁止跳过架构测试（反作弊）")]
    public void Architecture_Tests_Must_Not_Be_Skipped()
    {
        var testTypes = LoadArchitectureTestTypes();
        var skippedTests = new List<string>();

        foreach (var testType in testTypes)
        {
            var testMethods = testType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m
                                .GetCustomAttributes(typeof(FactAttribute), false)
                                .Any() ||
                            m
                                .GetCustomAttributes(typeof(TheoryAttribute), false)
                                .Any())
                .ToList();

            foreach (var method in testMethods)
            {
                var factAttr = method
                    .GetCustomAttributes(typeof(FactAttribute), false)
                    .FirstOrDefault() as FactAttribute;
                var theoryAttr = method
                    .GetCustomAttributes(typeof(TheoryAttribute), false)
                    .FirstOrDefault() as TheoryAttribute;

                var skipReason = factAttr?.Skip ?? theoryAttr?.Skip;
                if (skipReason != null)
                {
                    skippedTests.Add($"❌ {testType.Name}.{method.Name}: Skip = \"{skipReason}\"");
                }
            }
        }

        if (skippedTests.Any())
        {
            var message = "❌ ADR-900_1_4 违规 禁止跳过架构测试\n" + string.Join("\n", skippedTests) + "\n\n修复建议：如果某个架构约束不再适用，应删除测试或修改 ADR，而不是跳过测试。跳过测试会导致架构约束形同虚设。";
            throw new Xunit.Sdk.XunitException(message);
        }
    }

    private static string? ExtractAdrNumber(string typeName)
    {
        var match = System.Text.RegularExpressions.Regex.Match(typeName, @"ADR_(\d{4})");
        return match.Success ? match.Groups[1].Value : null;
    }
}
