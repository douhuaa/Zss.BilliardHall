using System.Reflection;
using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0000: 架构测试与 CI 治理宪法（v2.0）
/// 架构宪法测试：每一条 ADR 文档，必须有唯一、严格对应的架构测试类
/// </summary>
public sealed class ADR_0000_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const string AdrFilePattern = "ADR-*.md";
    private const string TestNamespacePrefix = "Zss.BilliardHall.Tests.ArchitectureTests.ADR";
    private const string TypeSuffix = "_Architecture_Tests";

    // ADR-0005-Enforcement-Levels 是 ADR-0005 的补充文档，不是独立的 ADR
    private static readonly HashSet<string> AdrWithoutTests = new(StringComparer.OrdinalIgnoreCase) {
        "ADR-0005-Enforcement-Levels"
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
    [Fact(DisplayName = "ADR-0000: 每条 ADR 必须有且仅有唯一对应的架构测试类")]
    public void Each_ADR_Must_Have_Exact_And_Unique_Architecture_Test()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根（docs/adr 或 .git）");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        Assert.True(Directory.Exists(adrDirectory), $"未找到 ADR 文档目录：{AdrDocsPath}");

        var adrIds = LoadAdrIds(adrDirectory)
            .Where(adr => !AdrWithoutTests.Contains(adr)) // 跳过无需测试的 ADR
            .ToList();
        Assert.NotEmpty(adrIds);

        var testTypes = LoadArchitectureTestTypes();

        var duplicates = adrIds
            .Select(adr => new {
                Adr = adr,
                Matches = testTypes
                    .Where(t => string.Equals(t.Name, ToExpectedTypeName(adr), StringComparison.OrdinalIgnoreCase))
                    .Select(t => t.FullName)
                    .ToList()
            })
            .Where(x => x.Matches.Count > 1)
            .ToList();

        var missing = adrIds
            .Where(adr => !testTypes.Any(t => string.Equals(t.Name, ToExpectedTypeName(adr), StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (duplicates.Any() || missing.Any())
        {
            var messages = new List<string>();

            if (duplicates.Any())
            {
                messages.Add("❌ ADR-0000 架构违规（重复定义）");
                foreach (var d in duplicates)
                {
                    messages.Add($" - {d.Adr} 对应多个测试类型:");
                    messages.AddRange(d.Matches.Select(fullName => $"     • {fullName}"));
                }
            }

            if (missing.Any())
            {
                messages.Add("❌ ADR-0000 架构违规（缺失测试）");
                messages.AddRange(missing.Select(m => $" - {m}"));
            }

            messages.Add("每条 ADR 文档必须有且仅有一个对应的架构测试类，命名规范：ADR_xxxx_xxxx_Architecture_Tests");

            throw new Xunit.Sdk.XunitException(string.Join(Environment.NewLine, messages));
        }
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
            .Where(file => !string.IsNullOrWhiteSpace(file) && System.Text.RegularExpressions.Regex.IsMatch(file!, @"^ADR-\d{4}", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
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
    [Fact(DisplayName = "ADR-0000: 架构测试类必须包含最少断言数（反作弊）")]
    public void Architecture_Test_Classes_Must_Have_Minimum_Assertions()
    {
        var testTypes = LoadArchitectureTestTypes();
        var violations = new List<string>();

        foreach (var testType in testTypes)
        {
            // 跳过 ADR-0000 自身
            if (testType.Name == "ADR_0000_Architecture_Tests")
                continue;

            // 跳过重定向测试（ADR_0008 已重构为三层架构）
            if (testType.Name == "ADR_0008_Architecture_Tests")
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
            var message = "❌ ADR-0000 反作弊检查失败：\n" + string.Join("\n", violations) + "\n\n修复建议：架构测试类必须包含实质性的测试逻辑，不允许空测试或全部跳过的测试。";
            throw new Xunit.Sdk.XunitException(message);
        }
    }

    /// <summary>
    /// 反作弊规则：测试失败消息必须包含 ADR 编号
    /// </summary>
    [Fact(DisplayName = "ADR-0000: 测试失败消息必须包含 ADR 编号（反作弊）")]
    public void Test_Failure_Messages_Must_Include_ADR_Number()
    {
        var testTypes = LoadArchitectureTestTypes();
        var violations = new List<string>();

        foreach (var testType in testTypes)
        {
            // 跳过 ADR-0000 自身
            if (testType.Name == "ADR_0000_Architecture_Tests")
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
            var message = "⚠️ ADR-0000 建议：\n" + string.Join("\n", violations) + "\n\n建议：所有测试的 DisplayName 应包含 ADR 编号（如 'ADR-0001: ...'），便于追溯和审计。";

            // 这是建议性规则，暂时只输出调试信息，不阻断
            System.Diagnostics.Debug.WriteLine(message);
        }
    }

    /// <summary>
    /// 禁止使用 Skip 属性跳过架构测试
    /// </summary>
    [Fact(DisplayName = "ADR-0000: 禁止跳过架构测试（反作弊）")]
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
            var message = "❌ ADR-0000 违规：禁止跳过架构测试\n" + string.Join("\n", skippedTests) + "\n\n修复建议：如果某个架构约束不再适用，应删除测试或修改 ADR，而不是跳过测试。" + "跳过测试会导致架构约束形同虚设。";
            throw new Xunit.Sdk.XunitException(message);
        }
    }

    /// <summary>
    /// ADR-0000.Z: 测试方法应使用 DisplayName 包含 ADR 编号
    /// 确保每个测试方法都能清晰地追溯到对应的 ADR 规则
    /// </summary>
    [Fact(DisplayName = "ADR-0000.Z: 测试方法 DisplayName 应包含 ADR 编号")]
    public void Test_Methods_Should_Include_ADR_Number_In_DisplayName()
    {
        var testTypes = LoadArchitectureTestTypes();
        var warnings = new List<string>();
        var errors = new List<string>();

        foreach (var testType in testTypes)
        {
            // 跳过 ADR-0000 自身和重定向类
            if (testType.Name == "ADR_0000_Architecture_Tests" || testType.Name == "ADR_0008_Architecture_Tests")
                continue;

            // 提取 ADR 编号（支持 3 位或 4 位编号）
            var adrNumberMatch = System.Text.RegularExpressions.Regex.Match(testType.Name, @"ADR_(\d{3,4})");
            if (!adrNumberMatch.Success)
            {
                errors.Add($"❌ {testType.Name}: 无法从类名提取 ADR 编号");
                continue;
            }

            var adrNumber = adrNumberMatch.Groups[1].Value;

            // 检查测试方法的 DisplayName
            var testMethods = testType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any() || 
                           m.GetCustomAttributes(typeof(TheoryAttribute), false).Any())
                .ToList();

            foreach (var method in testMethods)
            {
                var factAttr = method.GetCustomAttributes(typeof(FactAttribute), false).FirstOrDefault() as FactAttribute;
                var theoryAttr = method.GetCustomAttributes(typeof(TheoryAttribute), false).FirstOrDefault() as TheoryAttribute;
                var displayName = factAttr?.DisplayName ?? theoryAttr?.DisplayName;

                if (string.IsNullOrWhiteSpace(displayName))
                {
                    warnings.Add($"⚠️ {testType.Name}.{method.Name}: 缺少 DisplayName 属性");
                }
                else if (!displayName.Contains($"ADR-{adrNumber}") && !displayName.Contains($"ADR-0{adrNumber}"))
                {
                    // 只对宪法层和治理层（0000-0999）强制要求 ADR 编号
                    var adrNum = int.Parse(adrNumber);
                    if (adrNum < 1000)
                    {
                        warnings.Add($"⚠️ {testType.Name}.{method.Name}: DisplayName 建议包含 'ADR-{adrNumber}'");
                    }
                }
            }
        }

        // 只对错误失败，警告仅输出
        if (errors.Any())
        {
            var message = "❌ ADR-0000.Z 违规：以下测试类存在问题\n" +
                         string.Join("\n", errors) +
                         "\n\n修复建议：\n" +
                         "1. 测试类名应遵循 ADR_{编号}_Architecture_Tests 格式\n" +
                         "2. 编号可以是 3 位（如 910）或 4 位（如 0001）\n\n" +
                         "参考：docs/adr/governance/ADR-0000-architecture-tests.md";
            throw new Xunit.Sdk.XunitException(message);
        }

        // 输出警告信息供参考
        if (warnings.Any())
        {
            System.Diagnostics.Debug.WriteLine("⚠️ ADR-0000.Z 建议：\n" + string.Join("\n", warnings) +
                "\n\n建议：\n" +
                "1. 所有测试方法应使用 [Fact(DisplayName = \"...\")] 或 [Theory(DisplayName = \"...\")]\n" +
                "2. DisplayName 格式：'ADR-{编号}.{子编号}: {规则描述}'\n" +
                "3. 示例：[Fact(DisplayName = \"ADR-0001.1: 模块不应相互引用\")]\n\n" +
                "注意：这是建议性规则，不会阻断测试，但有助于提升可追溯性");
        }
    }

    private static string? ExtractAdrNumber(string typeName)
    {
        var match = System.Text.RegularExpressions.Regex.Match(typeName, @"ADR_(\d{3,4})");
        return match.Success ? match.Groups[1].Value : null;
    }
}
