namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// ADR-907 Rule 4：Analyzer / CI Gate 映射协议
/// 验证 ArchitectureTests 与 CI/Analyzer 的集成和映射
/// </summary>
public sealed class ADR_907_4_Architecture_Tests
{
    private const string AdrDocumentPath = "docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md";

    [Fact(DisplayName = "ADR-907_4_1：所有ArchitectureTests必须被自动发现并注册")]
    public void ADR_907_4_1_All_ArchitectureTests_Must_Be_Auto_Discovered_And_Registered()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(4, 1);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification"))
            .ToList();

        var testsNotDiscoverable = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileNameWithoutExtension(testFile);

            // 检查类是否为 public
            var hasPublicClass = Regex.IsMatch(content, @"public\s+(sealed\s+)?class\s+\w+");
            
            // 检查是否有测试方法（带 [Fact] 或 [Theory]）
            var hasTestMethods = Regex.IsMatch(content, @"\[(?:Fact|Theory)");

            // 检查是否缺少 public 修饰符或测试特性
            if (hasTestMethods && !hasPublicClass)
            {
                testsNotDiscoverable.Add($"{fileName}.cs (类不是 public)");
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_4_1 必须在 RuleSet 中定义");

        testsNotDiscoverable.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_4_1",
                summary: "所有ArchitectureTests必须被自动发现并注册",
                currentState: $"发现 {testsNotDiscoverable.Count} 个测试类可能无法被自动发现：\n  - " +
                             string.Join("\n  - ", testsNotDiscoverable),
                remediationSteps: new[]
                {
                    "确保所有测试类都是 public",
                    "确保测试类包含在 ArchitectureTests.csproj 中",
                    "验证测试方法带有 [Fact] 或 [Theory] 特性"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_4_2：测试失败必须精确映射至ADR子规则（RuleId）")]
    public void ADR_907_4_2_Test_Failures_Must_Map_Precisely_To_Adr_Sub_Rules()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(4, 2);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification"))
            .ToList();

        var testsWithoutRuleIdMapping = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileNameWithoutExtension(testFile);

            // 查找测试方法
            var methodPattern = @"\[(?:Fact|Theory).*?DisplayName\s*=\s*""([^""]*)""\]\s*public\s+\w+\s+(\w+)";
            var methodMatches = Regex.Matches(content, methodPattern, RegexOptions.Singleline);

            foreach (Match match in methodMatches)
            {
                var displayName = match.Groups[1].Value;
                var methodName = match.Groups[2].Value;

                // 检查 DisplayName 是否包含 RuleId（格式：ADR-XXX_Y_Z）
                var hasRuleIdInDisplayName = Regex.IsMatch(displayName, @"ADR-\d+_\d+_\d+");

                // 检查方法名是否包含 RuleId
                var hasRuleIdInMethodName = Regex.IsMatch(methodName, @"ADR_\d+_\d+_\d+");

                if (!hasRuleIdInDisplayName || !hasRuleIdInMethodName)
                {
                    testsWithoutRuleIdMapping.Add($"{methodName} (在 {fileName}.cs 中)");
                }
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_4_2 必须在 RuleSet 中定义");

        testsWithoutRuleIdMapping.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_4_2",
                summary: "测试失败必须精确映射至ADR子规则（RuleId）",
                currentState: $"发现 {testsWithoutRuleIdMapping.Count} 个测试缺少 RuleId 映射：\n  - " +
                             string.Join("\n  - ", testsWithoutRuleIdMapping.Take(10)) +
                             (testsWithoutRuleIdMapping.Count > 10 ? $"\n  ... 以及其他 {testsWithoutRuleIdMapping.Count - 10} 个" : ""),
                remediationSteps: new[]
                {
                    "在测试方法的 DisplayName 中包含完整的 RuleId（格式：ADR-XXX_Y_Z）",
                    "确保方法名也遵循命名规范：ADR_XXX_Y_Z_{行为描述}",
                    "使用 AssertionMessageBuilder 确保失败消息包含 RuleId"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_4_3：支持执行级别分类（L1阻断 / L2告警）")]
    public void ADR_907_4_3_Must_Support_Enforcement_Level_Classification()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(4, 3);

        // Act - 验证 ADR-907 的所有规则都标记为 L1（阻断级）
        var allClauses = ruleSet.Clauses.ToList();
        var clausesWithoutEnforcement = new List<string>();

        foreach (var c in allClauses)
        {
            // 检查 Clause 是否有明确的执行类型
            if (c.ExecutionType == ClauseExecutionType.ManualReview)
            {
                clausesWithoutEnforcement.Add($"{c.Id} (执行类型为 ManualReview，应该是自动化的)");
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_4_3 必须在 RuleSet 中定义");

        clausesWithoutEnforcement.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_4_3",
                summary: "支持执行级别分类（L1阻断 / L2告警）",
                currentState: $"发现 {clausesWithoutEnforcement.Count} 个条款缺少明确的执行级别：\n  - " +
                             string.Join("\n  - ", clausesWithoutEnforcement),
                remediationSteps: new[]
                {
                    "在 RuleSet 定义中为每个 Clause 指定 ExecutionType",
                    "L1 级别的规则应使用 StaticAnalysis 或 Convention",
                    "L2 级别的规则应使用 RuntimeCheck 或标记为 Warning"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));

        // 验证 ADR-907 本身的执行级别
        var adr907Document = Path.Combine(TestEnvironment.AdrPath, "governance", "ADR-907-architecture-tests-enforcement-governance.md");
        if (File.Exists(adr907Document))
        {
            var content = File.ReadAllText(adr907Document);
            content.Should().Contain("primary_enforcement: L1",
                "ADR-907 应该声明为 L1 执行级别");
        }
    }

    [Fact(DisplayName = "ADR-907_4_4：破例机制必须自动记录")]
    public void ADR_907_4_4_Exception_Mechanism_Must_Be_Automatically_Recorded()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(4, 4);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act - 检查是否有使用 Skip 但未记录破例的测试
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification"))
            .ToList();

        var skippedWithoutExceptionRecord = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileNameWithoutExtension(testFile);

            // 查找 Skip 属性
            var skipPattern = @"\[(?:Fact|Theory).*?Skip\s*=\s*""([^""]*)""\]";
            var skipMatches = Regex.Matches(content, skipPattern);

            foreach (Match match in skipMatches)
            {
                var skipReason = match.Groups[1].Value;
                
                // 检查是否包含破例记录的必要信息
                var hasExceptionInfo = skipReason.Contains("Issue") ||
                                      skipReason.Contains("Debt") ||
                                      skipReason.Contains("到期") ||
                                      skipReason.Contains("偿还");

                if (!hasExceptionInfo)
                {
                    skippedWithoutExceptionRecord.Add($"{fileName}.cs (Skip 原因缺少破例记录: {skipReason})");
                }
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_4_4 必须在 RuleSet 中定义");

        skippedWithoutExceptionRecord.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_4_4",
                summary: "破例机制必须自动记录",
                currentState: $"发现 {skippedWithoutExceptionRecord.Count} 个跳过的测试缺少破例记录：\n  - " +
                             string.Join("\n  - ", skippedWithoutExceptionRecord),
                remediationSteps: new[]
                {
                    "在 Skip 原因中包含 Issue 编号或 Debt 跟踪信息",
                    "记录破例的到期时间和偿还计划",
                    "或者移除 Skip 属性，使测试正常执行"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_4_5：Analyzer必须具备检测空测试/弱断言/跨ADR的能力")]
    public void ADR_907_4_5_Analyzer_Must_Detect_Empty_Weak_And_Cross_Adr_Tests()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(4, 5);

        // Act - 验证 RuleSet 中是否定义了相应的检测规则
        var requiredClauses = new[]
        {
            (2, 7), // 禁止空弱断言
            (2, 3), // 禁止跨 ADR 混合测试
            (3, 4)  // 禁止形式化断言
        };

        var missingClauses = new List<string>();

        foreach (var (ruleNum, clauseNum) in requiredClauses)
        {
            var c = ruleSet.GetClause(ruleNum, clauseNum);
            if (c == null)
            {
                missingClauses.Add($"ADR-907_{ruleNum}_{clauseNum}");
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_4_5 必须在 RuleSet 中定义");

        missingClauses.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_4_5",
                summary: "Analyzer必须具备检测空测试/弱断言/跨ADR的能力",
                currentState: $"RuleSet 中缺少 {missingClauses.Count} 个必要的检测规则：\n  - " +
                             string.Join("\n  - ", missingClauses),
                remediationSteps: new[]
                {
                    "在 Adr907RuleSet.cs 中添加缺少的 Clause 定义",
                    "确保包含：空测试检测、弱断言检测、跨ADR混合检测、形式化断言检测",
                    "为每个检测能力创建对应的测试方法"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));

        // 验证这些检测规则对应的测试是否存在
        var testFilesPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests", "ADR-907");
        if (Directory.Exists(testFilesPath))
        {
            var testFiles = Directory.GetFiles(testFilesPath, "*.cs");
            var allContent = string.Join("\n", testFiles.Select(File.ReadAllText));

            allContent.Should().Contain("ADR_907_2_7_", "应该有检测空弱断言的测试");
            allContent.Should().Contain("ADR_907_2_3_", "应该有检测跨ADR混合的测试");
            allContent.Should().Contain("ADR_907_3_4_", "应该有检测形式化断言的测试");
        }
    }

    [Fact(DisplayName = "ADR-907_4_6：ADR生命周期变更必须同步")]
    public void ADR_907_4_6_Adr_Lifecycle_Changes_Must_Be_Synchronized()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(4, 6);
        var adrPath = TestEnvironment.AdrPath;
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act - 检查已 Superseded 或 Obsolete 的 ADR 是否仍有活跃测试
        var adrFiles = Directory.GetFiles(adrPath, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("README") && !f.Contains("proposals"))
            .ToList();

        var supersededAdrsWithActiveTests = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 检查是否为 Superseded 或 Obsolete 状态
            var isSuperseded = content.Contains("status: Superseded", StringComparison.OrdinalIgnoreCase) ||
                              content.Contains("status: Obsolete", StringComparison.OrdinalIgnoreCase);

            if (!isSuperseded) continue;

            // 提取 ADR 编号
            var match = Regex.Match(fileName, @"ADR-(\d+)", RegexOptions.IgnoreCase);
            if (!match.Success) continue;

            var adrNumber = match.Groups[1].Value;

            // 检查是否仍有测试目录
            var testDirectory = Path.Combine(architectureTestsPath, $"ADR-{adrNumber}");
            if (Directory.Exists(testDirectory))
            {
                var testFiles = Directory.GetFiles(testDirectory, "*.cs");
                var hasActiveTests = testFiles.Any(f =>
                {
                    var testContent = File.ReadAllText(f);
                    return !testContent.Contains("[Obsolete]") && !testContent.Contains("Skip");
                });

                if (hasActiveTests)
                {
                    supersededAdrsWithActiveTests.Add($"ADR-{adrNumber} ({fileName})");
                }
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_4_6 必须在 RuleSet 中定义");

        supersededAdrsWithActiveTests.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_4_6",
                summary: "ADR生命周期变更必须同步",
                currentState: $"发现 {supersededAdrsWithActiveTests.Count} 个已废弃的 ADR 仍有活跃测试：\n  - " +
                             string.Join("\n  - ", supersededAdrsWithActiveTests),
                remediationSteps: new[]
                {
                    "将已 Superseded 的 ADR 对应测试标记为 [Obsolete]",
                    "或将测试移动到 archive/ 目录",
                    "更新测试文档，说明该测试已被哪个新测试取代"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }
}
