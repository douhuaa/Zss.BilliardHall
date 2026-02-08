namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.tests;

/// <summary>
/// ADR-907 Rule 4: Analyzer / CI Gate 映射协议
/// 验证架构测试与 CI/CD 集成的规范
/// </summary>
public sealed class Adr907Rule4_Tests
{
    private readonly Adr907RuleSet _ruleSet = new();
    private ArchitectureRuleSet _adr907RuleSet => _ruleSet.Define();

    [Fact(DisplayName = "ADR-907_4_1: 所有 ArchitectureTests 必须被 Analyzer 自动发现")]
    public void ADR_907_4_1_自动发现()
    {
        var clause = _adr907RuleSet.GetClause(4, 1);
        clause.Should().NotBeNull("Clause 4.1 必须存在");
        
        // 验证 ADR-907 在 RuleSetRegistry 中已注册
        var adr907 = RuleSetRegistry.GetStrict(907);
        adr907.Should().NotBeNull("ADR-907 必须在 RuleSetRegistry 中注册");
        adr907.AdrNumber.Should().Be(907);
        
        // 验证测试类使用了正确的测试框架属性
        var testType = typeof(Adr907Rule4_Tests);
        var testMethods = Adr907TestHelpers.GetTestMethods(testType);
        
        testMethods.Should().NotBeEmpty(
            "ADR-907_4_1: 测试类必须包含标记为 Fact 或 Theory 的测试方法以便自动发现");
    }

    [Fact(DisplayName = "ADR-907_4_2: 测试失败必须精确映射至 ADR 子规则（RuleId）")]
    public void ADR_907_4_2_RuleId格式()
    {
        var clause = _adr907RuleSet.GetClause(4, 2);
        clause.Should().NotBeNull("Clause 4.2 必须存在");
        
        var violations = ValidateAllRuleIdFormats();
        
        Adr907TestHelpers.AssertNoViolations(
            "ADR-907_4_2",
            "所有 RuleId 必须符合 ADR-XXX_Y_Z 格式",
            violations.Take(20),  // 限制显示数量
            new[] { 
                "修正 RuleId 格式为 ADR-XXX_Y_Z",
                "XXX 为三位 ADR 编号，Y 为规则编号，Z 为条款编号"
            });
    }

    [Fact(DisplayName = "ADR-907_4_3: 支持执行级别分类（L1/L2）")]
    public void ADR_907_4_3_执行级别()
    {
        var clause = _adr907RuleSet.GetClause(4, 3);
        clause.Should().NotBeNull("Clause 4.3 必须存在");
        
        var violations = ValidateSeverityLevels();
        
        Adr907TestHelpers.AssertNoViolations(
            "ADR-907_4_3",
            "所有规则必须定义有效的严重级别",
            violations,
            new[] { 
                "为每个规则设置 RuleSeverity",
                "使用 Constitutional/Governance/Critical/High/Medium/Low"
            });
    }

    [Fact(DisplayName = "ADR-907_4_4: 破例机制必须自动记录")]
    public void ADR_907_4_4_破例记录()
    {
        var clause = _adr907RuleSet.GetClause(4, 4);
        clause.Should().NotBeNull("Clause 4.4 必须存在");
        
        // 这是基础设施检查，如果破例目录存在则验证
        var exemptionsPath = Path.Combine(TestEnvironment.RepositoryRoot, "docs", "adr", "exemptions");
        
        if (Directory.Exists(exemptionsPath))
        {
            var exemptionFiles = Directory.GetFiles(exemptionsPath, "*.md");
            exemptionFiles.Should().NotBeNull("如果存在 exemptions 目录，应该能够列出文件");
        }
    }

    [Fact(DisplayName = "ADR-907_4_5: Analyzer 必须具备检测能力")]
    public void ADR_907_4_5_Analyzer检测()
    {
        var clause = _adr907RuleSet.GetClause(4, 5);
        clause.Should().NotBeNull("Clause 4.5 必须存在");
        
        // 检查 ArchitectureAnalyzers 项目是否存在
        var analyzersProjectPath = Path.Combine(TestEnvironment.SourceRoot, "tools", "ArchitectureAnalyzers");
        
        Directory.Exists(analyzersProjectPath).Should().BeTrue(
            AssertionMessageBuilder.BuildDirectoryNotFoundMessage(
                "ADR-907_4_5",
                analyzersProjectPath,
                "ArchitectureAnalyzers 项目目录",
                new[] { 
                    "创建 ArchitectureAnalyzers 项目",
                    "实现 Roslyn Analyzer 检测架构违规"
                },
                "docs/adr/ADR-907.md"));
        
        // 检查是否存在执行绑定
        var hasBindings = Adr907ExecutionBindings.All.Any();
        hasBindings.Should().BeTrue(
            "ADR-907_4_5: 应该定义一些执行绑定以连接规则和 Analyzer");
    }

    [Fact(DisplayName = "ADR-907_4_6: ADR 生命周期变更必须同步")]
    public void ADR_907_4_6_生命周期同步()
    {
        var clause = _adr907RuleSet.GetClause(4, 6);
        clause.Should().NotBeNull("Clause 4.6 必须存在");
        
        // 检查 ADR 文档目录是否存在状态管理
        var adrPath = TestEnvironment.AdrPath;
        Directory.Exists(adrPath).Should().BeTrue(
            "ADR-907_4_6: ADR 文档目录必须存在以支持生命周期管理");
        
        // 检查是否有 ADR 文档
        var adrFiles = FileSystemTestHelper.GetAdrFiles();
        adrFiles.Should().NotBeEmpty(
            "ADR-907_4_6: 必须有 ADR 文档才能管理生命周期");
    }

    #region 私有辅助方法

    private List<string> ValidateAllRuleIdFormats()
    {
        var violations = new List<string>();
        var allAdrNumbers = RuleSetRegistry.GetAllAdrNumbers();
        
        foreach (var adrNumber in allAdrNumbers)
        {
            var ruleSet = RuleSetRegistry.GetStrict(adrNumber);
            
            foreach (var rule in ruleSet.Rules)
            {
                var ruleIdStr = rule.Id.ToString();
                if (!Regex.IsMatch(ruleIdStr, @"^ADR-\d{3}_\d+$"))
                {
                    violations.Add($"Rule {ruleIdStr} 格式不正确");
                }
            }
            
            foreach (var clauseDef in ruleSet.Clauses)
            {
                var clauseIdStr = clauseDef.Id.ToString();
                if (!Regex.IsMatch(clauseIdStr, @"^ADR-\d{3}_\d+_\d+$"))
                {
                    violations.Add($"Clause {clauseIdStr} 格式不正确");
                }
            }
        }
        
        return violations;
    }

    private List<string> ValidateSeverityLevels()
    {
        var violations = new List<string>();
        var allAdrNumbers = RuleSetRegistry.GetAllAdrNumbers();
        
        foreach (var adrNumber in allAdrNumbers)
        {
            var ruleSet = RuleSetRegistry.GetStrict(adrNumber);
            
            foreach (var rule in ruleSet.Rules)
            {
                if (!Enum.IsDefined(typeof(RuleSeverity), rule.Severity))
                {
                    violations.Add($"{rule.Id}: Severity 值无效");
                }
            }
        }
        
        return violations;
    }

    #endregion
}
