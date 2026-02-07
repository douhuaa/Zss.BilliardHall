namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// ADR-907 Rule 2：命名与组织规范
/// 验证 ArchitectureTests 的命名和组织结构符合规范
/// </summary>
public sealed class ADR_907_2_Architecture_Tests
{
    private const string AdrDocumentPath = "docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md";

    [Fact(DisplayName = "ADR-907_2_1：ArchitectureTests 必须集中于独立测试项目")]
    public void ADR_907_2_1_ArchitectureTests_Must_Be_In_Independent_Project()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(2, 1);
        var testsPath = Path.Combine(TestEnvironment.SourceRoot, "tests");

        // Act
        var architectureTestsProject = Path.Combine(testsPath, "ArchitectureTests", "ArchitectureTests.csproj");
        var projectExists = File.Exists(architectureTestsProject);

        // Assert
        clause.Should().NotBeNull("ADR-907_2_1 必须在 RuleSet 中定义");

        projectExists.Should().BeTrue(
            Build(
                ruleId: "ADR-907_2_1",
                summary: "ArchitectureTests 必须集中于独立测试项目",
                currentState: $"ArchitectureTests.csproj 不存在于预期位置：{architectureTestsProject}",
                remediationSteps: new[]
                {
                    "在 src/tests/ArchitectureTests/ 目录下创建 ArchitectureTests.csproj 项目文件",
                    "确保项目命名格式为：<SolutionName>.Tests.Architecture",
                    "将所有架构测试文件迁移到该独立项目中"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_2_2：测试目录必须按 ADR 编号分组")]
    public void ADR_907_2_2_Test_Directories_Must_Be_Grouped_By_Adr_Number()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(2, 2);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification")) // 排除 Specification 目录
            .ToList();

        var incorrectlyOrganizedTests = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(testFile);
            var match = Regex.Match(fileName, @"ADR_(\d+)_\d+_Architecture_Tests");

            if (match.Success)
            {
                var adrNumber = match.Groups[1].Value;
                var expectedDirectory = $"ADR-{adrNumber}";
                var actualDirectory = Path.GetFileName(Path.GetDirectoryName(testFile));

                if (actualDirectory != expectedDirectory)
                {
                    incorrectlyOrganizedTests.Add(
                        $"{fileName}.cs (在 {actualDirectory}/ 目录下，应该在 {expectedDirectory}/ 目录下)");
                }
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_2_2 必须在 RuleSet 中定义");

        incorrectlyOrganizedTests.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_2_2",
                summary: "测试目录必须按 ADR 编号分组",
                currentState: $"发现 {incorrectlyOrganizedTests.Count} 个测试文件未按 ADR 编号组织：\n  - " +
                             string.Join("\n  - ", incorrectlyOrganizedTests),
                remediationSteps: new[]
                {
                    "将测试文件移动到对应的 ADR 编号目录下",
                    "目录格式：src/tests/ArchitectureTests/ADR-{编号}/",
                    "确保每个 ADR 的测试都在其专属目录中"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_2_3：单个测试类仅允许覆盖一个 ADR")]
    public void ADR_907_2_3_Single_Test_Class_Must_Cover_Only_One_Adr()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(2, 3);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification"))
            .ToList();

        var mixedAdrTests = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileNameWithoutExtension(testFile);

            // 从文件名提取主 ADR 编号
            var fileNameMatch = Regex.Match(fileName, @"ADR_(\d+)");
            if (!fileNameMatch.Success) continue;

            var primaryAdr = fileNameMatch.Groups[1].Value;

            // 检查测试方法名是否引用了其他 ADR
            var methodMatches = Regex.Matches(content, @"public\s+\w+\s+ADR_(\d+)_");
            var referencedAdrs = methodMatches
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .ToList();

            if (referencedAdrs.Count > 1 || (referencedAdrs.Count == 1 && referencedAdrs[0] != primaryAdr))
            {
                mixedAdrTests.Add($"{fileName}.cs (引用了多个 ADR: {string.Join(", ", referencedAdrs.Select(a => $"ADR-{a}"))})");
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_2_3 必须在 RuleSet 中定义");

        mixedAdrTests.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_2_3",
                summary: "单个测试类仅允许覆盖一个 ADR",
                currentState: $"发现 {mixedAdrTests.Count} 个测试类混合了多个 ADR：\n  - " +
                             string.Join("\n  - ", mixedAdrTests),
                remediationSteps: new[]
                {
                    "将不同 ADR 的测试方法拆分到各自的测试类中",
                    "每个测试类专注于单一 ADR 的约束验证",
                    "确保测试类命名与包含的测试方法 ADR 编号一致"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_2_4：测试类命名必须显式绑定 ADR")]
    public void ADR_907_2_4_Test_Class_Name_Must_Explicitly_Bind_Adr()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(2, 4);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification") && !f.Contains("Shared") && !f.Contains("Performance"))
            .ToList();

        var invalidlyNamedClasses = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileNameWithoutExtension(testFile);

            // 查找类定义
            var classMatches = Regex.Matches(content, @"public\s+sealed\s+class\s+(\w+)");
            
            foreach (Match match in classMatches)
            {
                var className = match.Groups[1].Value;

                // 检查类名是否符合格式：ADR_{编号}_{Rule}_Architecture_Tests
                if (!Regex.IsMatch(className, @"^ADR_\d+_\d+_Architecture_Tests$"))
                {
                    // 如果包含 "Architecture" 或 "Tests"，但格式不对，则认为是架构测试类但命名不规范
                    if (className.Contains("Architecture") || className.Contains("Test"))
                    {
                        invalidlyNamedClasses.Add($"{className} (在文件 {fileName}.cs 中)");
                    }
                }
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_2_4 必须在 RuleSet 中定义");

        invalidlyNamedClasses.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_2_4",
                summary: "测试类命名必须显式绑定 ADR",
                currentState: $"发现 {invalidlyNamedClasses.Count} 个测试类命名不符合规范：\n  - " +
                             string.Join("\n  - ", invalidlyNamedClasses),
                remediationSteps: new[]
                {
                    "重命名测试类为：ADR_{编号}_{Rule}_Architecture_Tests",
                    "例如：ADR_907_2_Architecture_Tests",
                    "确保文件名与类名一致"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_2_5：测试方法必须映射 ADR 子规则")]
    public void ADR_907_2_5_Test_Method_Must_Map_To_Adr_Sub_Rule()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(2, 5);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification"))
            .ToList();

        var invalidMethodNames = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileNameWithoutExtension(testFile);

            // 查找测试方法（带 [Fact] 或 [Theory] 特性）
            var methodPattern = @"\[(?:Fact|Theory).*?\]\s*public\s+\w+\s+(\w+)\s*\(";
            var methodMatches = Regex.Matches(content, methodPattern, RegexOptions.Singleline);

            foreach (Match match in methodMatches)
            {
                var methodName = match.Groups[1].Value;

                // 检查方法名是否符合格式：ADR_{编号}_{Rule}_{Clause}_{行为描述}
                if (!Regex.IsMatch(methodName, @"^ADR_\d+_\d+_\d+_\w+"))
                {
                    invalidMethodNames.Add($"{methodName} (在 {fileName}.cs 中)");
                }
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_2_5 必须在 RuleSet 中定义");

        invalidMethodNames.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_2_5",
                summary: "测试方法必须映射 ADR 子规则",
                currentState: $"发现 {invalidMethodNames.Count} 个测试方法命名不符合规范：\n  - " +
                             string.Join("\n  - ", invalidMethodNames.Take(10)) +
                             (invalidMethodNames.Count > 10 ? $"\n  ... 以及其他 {invalidMethodNames.Count - 10} 个" : ""),
                remediationSteps: new[]
                {
                    "重命名测试方法为：ADR_{编号}_{Rule}_{Clause}_{行为描述}",
                    "例如：ADR_907_2_5_Test_Method_Must_Map_To_Adr_Sub_Rule",
                    "确保每个测试方法明确映射到一个具体的 ADR 条款"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_2_6：测试失败信息必须包含 ADR 编号与子规则")]
    public void ADR_907_2_6_Test_Failure_Message_Must_Include_Adr_And_Sub_Rule()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(2, 6);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification"))
            .ToList();

        var testsWithoutProperMessages = new List<string>();

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

                // 检查是否使用了 Build 方法或包含 ADR-XXX_Y_Z 格式的字符串
                var hasProperMessage = methodBody.Contains("Build(") ||
                                      Regex.IsMatch(methodBody, @"ADR-\d+_\d+_\d+") ||
                                      methodBody.Contains("AssertionMessageBuilder");

                if (!hasProperMessage && methodBody.Contains("Should()."))
                {
                    testsWithoutProperMessages.Add($"{methodName} (在 {fileName}.cs 中)");
                }
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_2_6 必须在 RuleSet 中定义");

        testsWithoutProperMessages.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_2_6",
                summary: "测试失败信息必须包含 ADR 编号与子规则",
                currentState: $"发现 {testsWithoutProperMessages.Count} 个测试方法可能缺少规范的失败信息：\n  - " +
                             string.Join("\n  - ", testsWithoutProperMessages.Take(10)) +
                             (testsWithoutProperMessages.Count > 10 ? $"\n  ... 以及其他 {testsWithoutProperMessages.Count - 10} 个" : ""),
                remediationSteps: new[]
                {
                    "使用 AssertionMessageBuilder.Build() 方法构建标准失败信息",
                    "确保失败信息包含 RuleId（格式：ADR-XXX_Y_Z）",
                    "参考 ARCHITECTURE-TEST-GUIDELINES.md 中的断言消息格式"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_2_7：ArchitectureTests 不得为空或弱断言")]
    public void ADR_907_2_7_ArchitectureTests_Must_Not_Be_Empty_Or_Weak()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(2, 7);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification"))
            .ToList();

        var emptyOrWeakTests = new List<string>();

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

                // 检查是否为空测试或弱断言
                var isEmpty = methodBody.Trim().Length < 50; // 过短的方法体
                var hasNoAssertion = !methodBody.Contains("Should()") &&
                                    !methodBody.Contains("Assert.") &&
                                    !methodBody.Contains("Verify");

                if (isEmpty || hasNoAssertion)
                {
                    emptyOrWeakTests.Add($"{methodName} (在 {fileName}.cs 中)");
                }
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_2_7 必须在 RuleSet 中定义");

        emptyOrWeakTests.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_2_7",
                summary: "ArchitectureTests 不得为空、占位或弱断言",
                currentState: $"发现 {emptyOrWeakTests.Count} 个可能为空或弱断言的测试：\n  - " +
                             string.Join("\n  - ", emptyOrWeakTests.Take(10)) +
                             (emptyOrWeakTests.Count > 10 ? $"\n  ... 以及其他 {emptyOrWeakTests.Count - 10} 个" : ""),
                remediationSteps: new[]
                {
                    "为每个测试添加有效的架构约束验证",
                    "确保测试包含至少一个有意义的断言",
                    "移除占位符测试或补充完整的测试逻辑"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }

    [Fact(DisplayName = "ADR-907_2_8：不得 Skip 或条件禁用测试")]
    public void ADR_907_2_8_Must_Not_Skip_Or_Conditionally_Disable_Tests()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);
        var clause = ruleSet.GetClause(2, 8);
        var architectureTestsPath = Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");

        // Act
        var testFiles = Directory.GetFiles(architectureTestsPath, "*Architecture_Tests.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Specification"))
            .ToList();

        var skippedTests = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileNameWithoutExtension(testFile);

            // 查找 Skip 属性或条件禁用
            var skipPattern = @"\[(?:Fact|Theory).*?Skip\s*=\s*""([^""]*)""\]";
            var skipMatches = Regex.Matches(content, skipPattern);

            foreach (Match match in skipMatches)
            {
                var skipReason = match.Groups[1].Value;
                skippedTests.Add($"{fileName}.cs (Skip 原因: {skipReason})");
            }

            // 查找条件禁用 (如 #if DEBUG)
            if (content.Contains("#if DEBUG") || content.Contains("#if RELEASE"))
            {
                skippedTests.Add($"{fileName}.cs (使用条件编译指令)");
            }
        }

        // Assert
        clause.Should().NotBeNull("ADR-907_2_8 必须在 RuleSet 中定义");

        skippedTests.Should().BeEmpty(
            Build(
                ruleId: "ADR-907_2_8",
                summary: "不得 Skip、条件禁用测试（除非走破例机制）",
                currentState: $"发现 {skippedTests.Count} 个被跳过或条件禁用的测试：\n  - " +
                             string.Join("\n  - ", skippedTests),
                remediationSteps: new[]
                {
                    "移除测试的 Skip 属性，使测试正常执行",
                    "移除条件编译指令（#if DEBUG 等）",
                    "如果确实需要临时禁用测试，必须通过破例机制并记录原因和偿还计划"
                },
                adrReference: AdrDocumentPath,
                includeClauseReference: true));
    }
}
