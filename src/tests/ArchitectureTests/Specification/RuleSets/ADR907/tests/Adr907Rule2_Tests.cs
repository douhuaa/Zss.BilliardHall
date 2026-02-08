namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.tests;

/// <summary>
/// ADR-907 Rule 2: 命名与组织规范
/// 验证测试项目的命名、组织结构和编码规范
/// </summary>
public sealed class Adr907Rule2_Tests
{
    private readonly Adr907RuleSet _ruleSet = new();
    private ArchitectureRuleSet _adr907RuleSet => _ruleSet.Define();

    [Fact(DisplayName = "ADR-907_2_1: ArchitectureTests 必须集中于独立测试项目")]
    public void ADR_907_2_1_独立测试项目()
    {
        var clause = _adr907RuleSet.GetClause(2, 1);
        clause.Should().NotBeNull("Clause 2.1 必须存在");
        
        // 验证项目目录存在
        Directory.Exists(Adr907TestHelpers.ArchTestProjectRoot).Should().BeTrue(
            AssertionMessageBuilder.BuildDirectoryNotFoundMessage(
                "ADR-907_2_1",
                Adr907TestHelpers.ArchTestProjectRoot,
                "ArchitectureTests 项目目录",
                new[] { "创建独立的 ArchitectureTests 测试项目" },
                "docs/adr/ADR-907.md"));
        
        // 验证项目文件存在
        var csprojPath = Path.Combine(Adr907TestHelpers.ArchTestProjectRoot, "ArchitectureTests.csproj");
        File.Exists(csprojPath).Should().BeTrue(
            AssertionMessageBuilder.BuildFileNotFoundMessage(
                "ADR-907_2_1",
                csprojPath,
                "ArchitectureTests.csproj 项目文件",
                new[] { "确保存在独立的 ArchitectureTests.csproj 项目文件" },
                "docs/adr/ADR-907.md"));
    }

    [Fact(DisplayName = "ADR-907_2_2: 测试目录必须按 ADR 编号分组")]
    public void ADR_907_2_2_按ADR分组()
    {
        var clause = _adr907RuleSet.GetClause(2, 2);
        clause.Should().NotBeNull("Clause 2.2 必须存在");
        
        // 验证 RuleSets 目录存在
        Directory.Exists(Adr907TestHelpers.AdrTestsRoot).Should().BeTrue(
            $"ADR-907_2_2: RuleSets 目录必须存在于 {Adr907TestHelpers.AdrTestsRoot}");
        
        // 验证 ADR 目录
        var adrDirectories = Adr907TestHelpers.GetAdrDirectories(Adr907TestHelpers.AdrTestsRoot);
        adrDirectories.Should().NotBeEmpty("ADR-907_2_2: 必须存在按 ADR-XXX 格式命名的测试目录");
        
        // 验证当前测试文件位置
        VerifyCurrentTestFileLocation();
        
        // 验证目录命名格式
        var violations = ValidateAdrDirectoryNaming(adrDirectories);
        Adr907TestHelpers.AssertNoViolations(
            "ADR-907_2_2",
            "目录名称必须符合 ADR-XXX 格式",
            violations,
            new[] { "重命名目录为 ADR-XXX 格式（XXX 为三位数字）" });
    }

    [Fact(DisplayName = "ADR-907_2_3: 单个测试类或文件仅允许覆盖一个 ADR")]
    public void ADR_907_2_3_一对一映射()
    {
        var clause = _adr907RuleSet.GetClause(2, 3);
        clause.Should().NotBeNull("Clause 2.3 必须存在");
        
        var testAssembly = typeof(Adr907Rule2_Tests).Assembly;
        var allTestTypes = Adr907TestHelpers.GetAdrTestTypes(testAssembly);
        
        var violations = FindTypesWithMultipleAdrNumbers(allTestTypes);
        
        Adr907TestHelpers.AssertNoViolations(
            "ADR-907_2_3",
            "单个测试类只能覆盖一个 ADR",
            violations,
            new[] { 
                "拆分混合多个 ADR 的测试类",
                "每个测试类只测试一个 ADR 的规则"
            });
    }

    [Fact(DisplayName = "ADR-907_2_4: 测试类命名必须显式绑定 ADR")]
    public void ADR_907_2_4_显式绑定命名()
    {
        var clause = _adr907RuleSet.GetClause(2, 4);
        clause.Should().NotBeNull("Clause 2.4 必须存在");
        
        var testAssembly = typeof(Adr907Rule2_Tests).Assembly;
        var adrTestTypes = Adr907TestHelpers.GetAdrTestTypes(testAssembly);
        
        var violations = ValidateTestClassNaming(adrTestTypes);
        
        Adr907TestHelpers.AssertNoViolations(
            "ADR-907_2_4",
            "测试类命名必须显式绑定 ADR 编号",
            violations,
            new[] { 
                "重命名测试类为 AdrXXX_* 或 ADRXXX_* 格式",
                "确保类名中包含三位 ADR 编号"
            });
    }

    [Fact(DisplayName = "ADR-907_2_5: 测试方法必须映射 ADR 子规则")]
    public void ADR_907_2_5_方法映射子规则()
    {
        var clause = _adr907RuleSet.GetClause(2, 5);
        clause.Should().NotBeNull("Clause 2.5 必须存在");
        
        // 使用 Rule1 作为示例（避免循环依赖）
        var testType = typeof(Adr907Rule1_Tests);
        var testMethods = Adr907TestHelpers.GetTestMethods(testType);
        
        var violations = ValidateTestMethodNaming(testMethods);
        
        Adr907TestHelpers.AssertNoViolations(
            "ADR-907_2_5",
            "测试方法必须映射到具体的 ADR 子规则",
            violations,
            new[] { 
                "重命名测试方法为 ADR_XXX_Rule_Clause_描述 格式",
                "确保每个方法明确映射到一个子规则"
            });
    }

    [Fact(DisplayName = "ADR-907_2_6: 测试失败信息必须包含 ADR 编号与子规则")]
    public void ADR_907_2_6_失败信息溯源()
    {
        var clause = _adr907RuleSet.GetClause(2, 6);
        clause.Should().NotBeNull("Clause 2.6 必须存在");
        
        // 检查测试类是否使用了 AssertionMessageBuilder
        var currentFile = Adr907TestHelpers.GetCurrentTestFilePath();
        
        Adr907TestHelpers.FileContainsPattern(currentFile, "AssertionMessageBuilder")
            .Should().BeTrue("ADR-907_2_6: 测试必须使用 AssertionMessageBuilder 构建失败消息");
        
        Adr907TestHelpers.FileMatchesRegex(currentFile, @"ADR-907_")
            .Should().BeTrue("ADR-907_2_6: 测试失败消息必须包含 RuleId");
    }

    [Fact(DisplayName = "ADR-907_2_7: ArchitectureTests 不得为空、占位或弱断言")]
    public void ADR_907_2_7_禁止弱断言()
    {
        var clause = _adr907RuleSet.GetClause(2, 7);
        clause.Should().NotBeNull("Clause 2.7 必须存在");
        
        var testType = typeof(Adr907Rule1_Tests);
        var testMethods = Adr907TestHelpers.GetTestMethods(testType);
        
        var violations = FindEmptyTestMethods(testMethods);
        
        Adr907TestHelpers.AssertNoViolations(
            "ADR-907_2_7",
            "测试不得为空、占位或弱断言",
            violations,
            new[] { 
                "为每个测试添加有意义的断言",
                "确保测试真正验证架构规则"
            });
    }

    [Fact(DisplayName = "ADR-907_2_8: 不得 Skip 或条件禁用测试")]
    public void ADR_907_2_8_禁止跳过测试()
    {
        var clause = _adr907RuleSet.GetClause(2, 8);
        clause.Should().NotBeNull("Clause 2.8 必须存在");
        
        var testType = typeof(Adr907Rule1_Tests);
        var testMethods = Adr907TestHelpers.GetTestMethods(testType);
        
        var violations = FindSkippedTests(testMethods);
        
        Adr907TestHelpers.AssertNoViolations(
            "ADR-907_2_8",
            "不得使用 Skip 跳过测试",
            violations,
            new[] { 
                "移除 Skip 属性",
                "修复测试使其能够正常运行",
                "如需临时禁用，通过破例机制正式记录"
            });
    }

    #region 私有辅助方法

    private void VerifyCurrentTestFileLocation()
    {
        var currentTestDir = Path.Combine(Adr907TestHelpers.AdrTestsRoot, "ADR907");
        Directory.Exists(currentTestDir).Should().BeTrue(
            AssertionMessageBuilder.BuildDirectoryNotFoundMessage(
                "ADR-907_2_2",
                currentTestDir,
                "ADR907 测试目录",
                new[] { 
                    "创建 ADR907 目录",
                    "将所有 ADR-907 相关测试移至该目录"
                },
                "docs/adr/ADR-907.md"));
        
        var currentTestFile = Adr907TestHelpers.GetCurrentTestFilePath();
        if (currentTestFile != null)
        {
            var normalizedTestFile = Path.GetFullPath(currentTestFile);
            var normalizedExpectedDir = Path.GetFullPath(currentTestDir);
            
            normalizedTestFile.Should().StartWith(normalizedExpectedDir,
                AssertionMessageBuilder.Build(
                    "ADR-907_2_2",
                    "测试文件必须位于 ADR-XXX 格式的目录中",
                    $"当前文件：{currentTestFile}\n预期目录：{currentTestDir}",
                    new[] { 
                        $"将测试文件移动到 {currentTestDir} 目录",
                        "确保目录名称匹配 ADR-XXX 格式（XXX 为三位数字）"
                    },
                    "docs/adr/ADR-907.md"));
        }
    }

    private List<string> ValidateAdrDirectoryNaming(List<string> directories)
    {
        var violations = new List<string>();
        foreach (var dir in directories)
        {
            var dirName = Path.GetFileName(dir);
            if (!Regex.IsMatch(dirName, @"^ADR\d{3}$"))
            {
                violations.Add($"{dirName}: 目录名称必须符合 ADR-XXX 格式（XXX 为三位数字）");
            }
        }
        return violations;
    }

    private List<string> FindTypesWithMultipleAdrNumbers(List<Type> testTypes)
    {
        var violations = new List<string>();
        
        foreach (var testType in testTypes)
        {
            if (testType.Namespace == null) continue;
            
            var adrMatches = Regex.Matches(testType.Namespace, @"ADR\d{3}");
            if (adrMatches.Count > 1)
            {
                var adrNumbers = string.Join(", ", adrMatches.Cast<Match>().Select(m => m.Value));
                violations.Add($"{testType.FullName} 包含多个 ADR 编号：{adrNumbers}");
            }
        }
        
        return violations;
    }

    private List<string> ValidateTestClassNaming(List<Type> testTypes)
    {
        var violations = new List<string>();
        
        foreach (var testType in testTypes)
        {
            if (!testType.Name.StartsWith("Adr", StringComparison.OrdinalIgnoreCase))
            {
                violations.Add($"{testType.FullName}: 类名必须以 Adr 或 ADR 开头");
                continue;
            }
            
            if (!Regex.IsMatch(testType.Name, @"Adr\d{3}", RegexOptions.IgnoreCase))
            {
                violations.Add($"{testType.FullName}: 类名必须包含三位 ADR 编号（如 Adr907）");
            }
        }
        
        return violations;
    }

    private List<string> ValidateTestMethodNaming(List<MethodInfo> testMethods)
    {
        var violations = new List<string>();
        
        var ruleClauseTests = testMethods
            .Where(m => m.Name.StartsWith("ADR_"))
            .Where(m => !m.Name.Contains("All_Clauses"))
            .ToList();
        
        foreach (var method in ruleClauseTests)
        {
            if (!Regex.IsMatch(method.Name, @"^ADR_\d{3}_\d+_\d+_"))
            {
                violations.Add($"{method.Name}: 方法名必须包含 ADR_XXX_Rule_Clause 格式的编号");
            }
        }
        
        return violations;
    }

    private List<string> FindEmptyTestMethods(List<MethodInfo> testMethods)
    {
        var violations = new List<string>();
        
        foreach (var method in testMethods)
        {
            if (Adr907TestHelpers.IsEmptyOrTooSmall(method))
            {
                violations.Add($"{method.Name}: 方法体过小，可能是空测试或占位符");
            }
        }
        
        return violations;
    }

    private List<string> FindSkippedTests(List<MethodInfo> testMethods)
    {
        var violations = new List<string>();
        
        foreach (var method in testMethods)
        {
            var factAttr = method.GetCustomAttribute<FactAttribute>();
            if (factAttr?.Skip != null && !string.IsNullOrEmpty(factAttr.Skip))
            {
                violations.Add($"{method.Name}: 使用了 Skip = \"{factAttr.Skip}\"");
            }
            
            var theoryAttr = method.GetCustomAttribute<TheoryAttribute>();
            if (theoryAttr?.Skip != null && !string.IsNullOrEmpty(theoryAttr.Skip))
            {
                violations.Add($"{method.Name}: 使用了 Skip = \"{theoryAttr.Skip}\"");
            }
        }
        
        return violations;
    }

    #endregion
}
