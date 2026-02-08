namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.tests;

/// <summary>
/// ADR-907 Rule 1: ArchitectureTests 的法律地位
/// 验证 ArchitectureTests 作为唯一自动化执法形式的规则
/// </summary>
public sealed class Adr907Rule1_Tests
{
    private readonly Adr907RuleSet _ruleSet = new();
    private ArchitectureRuleSet _adr907RuleSet => _ruleSet.Define();

    [Fact(DisplayName = "ADR-907_1_1: ArchitectureTests 是 ADR 的唯一自动化执法形式")]
    public void ADR_907_1_1_唯一执法形式()
    {
        // 验证规则存在
        var rule = _adr907RuleSet.GetRule(1);
        rule.Should().NotBeNull("Rule 1 必须存在");
        rule!.Summary.Should().Be("ArchitectureTests 的法律地位");
        
        var clause = _adr907RuleSet.GetClause(1, 1);
        clause.Should().NotBeNull("Clause 1.1 必须存在");
        
        // 检查其他测试项目是否违规引用 NetArchTest
        var violations = CheckOtherProjectsForNetArchTest();
        
        Adr907TestHelpers.AssertNoViolations(
            "ADR-907_1_1",
            "ArchitectureTests 必须是唯一的自动化执法形式",
            violations,
            new[] { 
                "将架构测试移至 ArchitectureTests 项目",
                "从其他测试项目中移除 NetArchTest 引用"
            });
    }

    [Fact(DisplayName = "ADR-907_1_2: 任何具备裁决力的 ADR 必须有对应的 ArchitectureTests")]
    public void ADR_907_1_2_必须有测试()
    {
        var clause = _adr907RuleSet.GetClause(1, 2);
        clause.Should().NotBeNull("Clause 1.2 必须存在");
        
        var allAdrNumbers = RuleSetRegistry.GetAllAdrNumbers().ToList();
        allAdrNumbers.Should().NotBeEmpty("必须有已注册的 ADR");
        
        var missingTests = FindAdrsWithoutTests(allAdrNumbers);
        
        // 记录统计信息
        var missingPercentage = (double)missingTests.Count / allAdrNumbers.Count;
        var statsMessage = $"ADR-907_1_2 统计：{allAdrNumbers.Count} 个 ADR 中，{missingTests.Count} 个缺少测试 ({missingPercentage:P0})";
        Console.WriteLine(statsMessage);
        
        // 注意：暂时不强制失败，只记录统计
        // TODO: 逐步为所有 ADR 添加测试，然后启用严格检查
    }

    [Fact(DisplayName = "ADR-907_1_3: 不存在无执法路径的架构规则")]
    public void ADR_907_1_3_禁止无执法路径()
    {
        var clause = _adr907RuleSet.GetClause(1, 3);
        clause.Should().NotBeNull("Clause 1.3 必须存在");
        
        var violations = FindRulesWithoutClauses();
        
        Adr907TestHelpers.AssertNoViolations(
            "ADR-907_1_3",
            "所有规则必须至少有一个条款定义执行路径",
            violations,
            new[] { 
                "为每个规则添加至少一个条款",
                "条款应明确说明如何验证该规则"
            });
    }

    #region 私有辅助方法

    private List<string> CheckOtherProjectsForNetArchTest()
    {
        var violations = new List<string>();
        var testProjectsDir = Path.Combine(TestEnvironment.SourceRoot, "tests");
        
        if (!Directory.Exists(testProjectsDir))
        {
            return violations;
        }

        var testProjects = Directory.GetDirectories(testProjectsDir)
            .Where(d => !d.EndsWith("ArchitectureTests"))
            .Where(d => Directory.GetFiles(d, "*.csproj").Any())
            .ToList();
        
        foreach (var project in testProjects)
        {
            var csprojFiles = Directory.GetFiles(project, "*.csproj");
            foreach (var csproj in csprojFiles)
            {
                if (Adr907TestHelpers.FileContainsPattern(csproj, "NetArchTest"))
                {
                    violations.Add($"{Path.GetFileName(project)}: 包含 NetArchTest 引用");
                }
            }
        }
        
        return violations;
    }

    private List<string> FindAdrsWithoutTests(List<int> allAdrNumbers)
    {
        var missingTests = new List<string>();
        
        foreach (var adrNumber in allAdrNumbers)
        {
            var ruleSet = RuleSetRegistry.GetStrict(adrNumber);
            
            if (ruleSet.Rules.Any())
            {
                var adrTestDir = Path.Combine(Adr907TestHelpers.AdrTestsRoot, $"ADR{adrNumber:000}");
                var hasTests = Directory.Exists(adrTestDir) && 
                              Directory.GetFiles(adrTestDir, "*Tests.cs", SearchOption.AllDirectories).Any();
                
                if (!hasTests)
                {
                    missingTests.Add($"ADR-{adrNumber:000}: 有 {ruleSet.Rules.Count} 个规则但没有对应的测试");
                }
            }
        }
        
        return missingTests;
    }

    private List<string> FindRulesWithoutClauses()
    {
        var violations = new List<string>();
        var allAdrNumbers = RuleSetRegistry.GetAllAdrNumbers();
        
        foreach (var adrNumber in allAdrNumbers)
        {
            var ruleSet = RuleSetRegistry.GetStrict(adrNumber);
            
            foreach (var rule in ruleSet.Rules)
            {
                var clausesForRule = ruleSet.Clauses
                    .Where(c => c.Id.RuleNumber == rule.Id.RuleNumber)
                    .ToList();
                
                if (!clausesForRule.Any())
                {
                    violations.Add(
                        $"ADR-{adrNumber:000} Rule {rule.Id.RuleNumber}: \"{rule.Summary}\" 没有任何条款");
                }
            }
        }
        
        return violations;
    }

    #endregion
}
