namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// ADR-907 Rule 3：最小断言语义规范
/// 验证 ArchitectureTests 的断言质量和语义要求
/// </summary>
public sealed class ADR_907_3_Architecture_Tests
{
    private const string AdrDocumentPath = "docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md";

    [Fact(DisplayName = "ADR-907_3_1：每个测试类至少包含1个有效断言")]
    public void ADR_907_3_1_Each_Test_Class_Must_Have_At_Least_One_Effective_Assertion()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(3, 1);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification"))
            .ToList();

        var classesWithoutAssertions = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileNameWithoutExtension(testFile);

            // 检查类是否包含有效断言
            var hasAssertion = content.Contains("Should().") ||
                              content.Contains("Should().Not") ||
                              content.Contains("Assert.") ||
                              content.Contains(".Verify(");

            // 检查是否有测试方法
            var hasTestMethods = Regex.IsMatch(content, @"\[(?:Fact|Theory)\]");

            if (hasTestMethods && !hasAssertion)
            {
                classesWithoutAssertions.Add($"{fileName}.cs");
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_3_1 必须在 RuleSet 中定义");

        classesWithoutAssertions.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_3_1",
                summary: "每个测试类至少包含1个有效断言",
                currentState: $"发现 {classesWithoutAssertions.Count} 个测试类缺少有效断言：\n  - " +
                             string.Join("\n  - ", classesWithoutAssertions),
                remediationSteps: new[]
                {
                    "为每个测试方法添加至少一个有效的断言",
                    "使用 FluentAssertions（Should()）或 xUnit（Assert.）",
                    "确保断言验证架构约束，而不是形式化断言"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_3_2：每个测试方法只能映射一个ADR子规则")]
    public void ADR_907_3_2_Each_Test_Method_Must_Map_To_Single_Sub_Rule()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(3, 2);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification"))
            .ToList();

        var methodsWithMultipleRules = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileNameWithoutExtension(testFile);

            // 查找测试方法
            var methodPattern = @"public\s+\w+\s+(ADR_\d+_\d+_\d+_\w+)\s*\([^)]*\)\s*\{([^}]*(?:\{[^}]*\}[^}]*)*)\}";
            var methodMatches = Regex.Matches(content, methodPattern, RegexOptions.Singleline);

            foreach (Match match in methodMatches)
            {
                var methodName = match.Groups[1].Value;
                var methodBody = match.Groups[2].Value;

                // 从方法名提取 RuleId
                var ruleIdMatch = Regex.Match(methodName, @"ADR_(\d+)_(\d+)_(\d+)");
                if (!ruleIdMatch.Success) continue;

                var expectedRuleId = $"ADR-{ruleIdMatch.Groups[1].Value}_{ruleIdMatch.Groups[2].Value}_{ruleIdMatch.Groups[3].Value}";

                // 检查方法体中是否引用了其他 RuleId
                var referencedRuleIds = Regex.Matches(methodBody, @"ADR-(\d+)_(\d+)_(\d+)")
                    .Select(m => m.Value)
                    .Distinct()
                    .Where(id => id != expectedRuleId)
                    .ToList();

                if (referencedRuleIds.Any())
                {
                    methodsWithMultipleRules.Add(
                        $"{methodName} (在 {fileName}.cs 中，还引用了: {string.Join(", ", referencedRuleIds)})");
                }
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_3_2 必须在 RuleSet 中定义");

        methodsWithMultipleRules.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_3_2",
                summary: "每个测试方法只能映射一个ADR子规则",
                currentState: $"发现 {methodsWithMultipleRules.Count} 个测试方法映射了多个子规则：\n  - " +
                             string.Join("\n  - ", methodsWithMultipleRules),
                remediationSteps: new[]
                {
                    "拆分测试方法，使每个方法只验证一个 ADR 子规则",
                    "如果需要验证多个相关约束，创建多个独立的测试方法",
                    "保持测试的单一职责原则"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_3_3：所有断言失败信息必须可反向溯源到ADR")]
    public void ADR_907_3_3_All_Assertion_Failures_Must_Be_Traceable_To_Adr()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(3, 3);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification"))
            .ToList();

        var assertionsWithoutTraceability = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileNameWithoutExtension(testFile);

            // 查找测试方法
            var methodPattern = @"public\s+\w+\s+(ADR_\d+_\d+_\d+_\w+)\s*\([^)]*\)\s*\{([^}]*(?:\{[^}]*\}[^}]*)*)\}";
            var methodMatches = Regex.Matches(content, methodPattern, RegexOptions.Singleline);

            foreach (Match match in methodMatches)
            {
                var methodName = match.Groups[1].Value;
                var methodBody = match.Groups[2].Value;

                // 检查是否包含断言
                var hasAssertion = methodBody.Contains("Should()") || methodBody.Contains("Assert.");
                if (!hasAssertion) continue;

                // 检查失败信息是否包含必要元素
                var hasRuleId = Regex.IsMatch(methodBody, @"ADR-\d+_\d+_\d+");
                var hasViolationMarker = methodBody.Contains("违规") || methodBody.Contains("❌");
                var hasRemediation = methodBody.Contains("修复建议") || methodBody.Contains("remediationSteps");
                var hasReference = methodBody.Contains("参考") || methodBody.Contains("adrReference");
                var usesBuildMethod = methodBody.Contains("Build(");

                // 如果使用了 Build 方法，认为是规范的
                if (usesBuildMethod) continue;

                // 否则检查是否包含所有必要元素
                var isTraceable = hasRuleId && (hasViolationMarker || hasRemediation || hasReference);

                if (!isTraceable)
                {
                    assertionsWithoutTraceability.Add($"{methodName} (在 {fileName}.cs 中)");
                }
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_3_3 必须在 RuleSet 中定义");

        assertionsWithoutTraceability.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_3_3",
                summary: "所有断言失败信息必须可反向溯源到ADR",
                currentState: $"发现 {assertionsWithoutTraceability.Count} 个断言缺少完整的溯源信息：\n  - " +
                             string.Join("\n  - ", assertionsWithoutTraceability.Take(10)) +
                             (assertionsWithoutTraceability.Count > 10 ? $"\n  ... 以及其他 {assertionsWithoutTraceability.Count - 10} 个" : ""),
                remediationSteps: new[]
                {
                    "使用 AssertionMessageBuilder.Build() 方法构建标准失败信息",
                    "确保失败信息包含：RuleId、违规标记、修复建议、文档引用",
                    "参考 ARCHITECTURE-TEST-GUIDELINES.md 中的标准格式"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_3_4：禁止形式化断言")]
    public void ADR_907_3_4_Formal_Assertions_Are_Prohibited()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(3, 4);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification"))
            .ToList();

        var formalAssertions = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileNameWithoutExtension(testFile);

            // 查找测试方法
            var methodPattern = @"public\s+\w+\s+(ADR_\d+_\d+_\d+_\w+)\s*\([^)]*\)\s*\{([^}]*(?:\{[^}]*\}[^}]*)*)\}";
            var methodMatches = Regex.Matches(content, methodPattern, RegexOptions.Singleline);

            foreach (Match match in methodMatches)
            {
                var methodName = match.Groups[1].Value;
                var methodBody = match.Groups[2].Value;

                // 检查形式化断言模式
                var hasFormalAssertion =
                    Regex.IsMatch(methodBody, @"Assert\.True\s*\(\s*true\s*\)") ||
                    Regex.IsMatch(methodBody, @"Assert\.False\s*\(\s*false\s*\)") ||
                    Regex.IsMatch(methodBody, @"\.Should\(\)\.BeTrue\s*\(\s*\).*true\.Should\(\)") ||
                    Regex.IsMatch(methodBody, @"1\.Should\(\)\.Be\s*\(\s*1\s*\)") ||
                    Regex.IsMatch(methodBody, @"Assert\.Equal\s*\(\s*1\s*,\s*1\s*\)");

                if (hasFormalAssertion)
                {
                    formalAssertions.Add($"{methodName} (在 {fileName}.cs 中)");
                }
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_3_4 必须在 RuleSet 中定义");

        formalAssertions.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_3_4",
                summary: "禁止形式化断言",
                currentState: $"发现 {formalAssertions.Count} 个测试包含形式化断言：\n  - " +
                             string.Join("\n  - ", formalAssertions),
                remediationSteps: new[]
                {
                    "移除 Assert.True(true)、Assert.False(false) 等无意义断言",
                    "用实际的架构约束验证替换形式化断言",
                    "确保每个断言都验证真实的结构或行为约束"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }
}
