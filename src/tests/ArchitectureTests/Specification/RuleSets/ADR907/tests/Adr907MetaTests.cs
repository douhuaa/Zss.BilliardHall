namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.tests;

/// <summary>
/// ADR-907 元测试
/// 验证所有条款都有对应的测试实现
/// </summary>
public sealed class Adr907MetaTests
{
    [Fact(DisplayName = "ADR-907: 验证所有条款都有对应的测试方法")]
    public void ADR_907_All_Clauses_Should_Have_Test_Methods()
    {
        // 获取所有定义的条款
        var allClauses = new List<(int RuleId, int ClauseId, string Name)>();
        foreach (var rule in Adr907Definitions.AllRules)
        {
            foreach (var clause in rule.Clauses)
            {
                allClauses.Add((rule.RuleId, clause.ClauseId, clause.Name));
            }
        }

        // 获取所有测试方法
        var testAssembly = typeof(Adr907MetaTests).Assembly;
        var testTypes = new[]
        {
            typeof(Adr907Rule1_Tests),
            typeof(Adr907Rule2_Tests),
            typeof(Adr907Rule3_Tests),
            typeof(Adr907Rule4_Tests)
        };
        
        var testMethods = testTypes
            .SelectMany(t => Adr907TestHelpers.GetTestMethods(t))
            .Select(m => m.Name)
            .ToList();

        // 验证每个条款都有对应的测试方法
        var missingTests = new List<string>();
        foreach (var (ruleId, clauseId, name) in allClauses)
        {
            var expectedPrefix = $"ADR_907_{ruleId}_{clauseId}_";
            var hasTest = testMethods.Any(m => m.StartsWith(expectedPrefix));
            if (!hasTest)
            {
                missingTests.Add($"ADR-907_{ruleId}_{clauseId}: {name}");
            }
        }
        
        Adr907TestHelpers.AssertNoViolations(
            "ADR-907",
            "所有条款都必须有对应的测试方法",
            missingTests,
            new[] { 
                "为每个条款创建对应的测试方法",
                "测试方法命名格式：ADR_907_Rule_Clause_描述"
            });
    }
}
