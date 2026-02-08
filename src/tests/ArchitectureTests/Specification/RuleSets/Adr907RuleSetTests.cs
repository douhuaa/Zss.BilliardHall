using Xunit;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets;

/// <summary>
/// 验证 ADR-907 规则集的 DSL 定义正确性
/// </summary>
public class Adr907RuleSetTests
{
    [Fact(DisplayName = "ADR-907 规则集应正确初始化")]
    public void Adr907RuleSet_Should_Initialize_Correctly()
    {
        // Arrange & Act
        var ruleSet = Adr907RuleSet.Instance;

        // Assert
        Assert.Equal("ADR-907", ruleSet.AdrNumber);
        Assert.Equal("ArchitectureTests 执法治理体系", ruleSet.Title);
        Assert.NotNull(ruleSet.Description);
        Assert.NotEmpty(ruleSet.Rules);
    }

    [Fact(DisplayName = "ADR-907 应包含 4 个主要规则")]
    public void Adr907RuleSet_Should_Have_Four_Main_Rules()
    {
        // Arrange
        var ruleSet = Adr907RuleSet.Instance;

        // Act & Assert
        Assert.Equal(4, ruleSet.Rules.Count);
        Assert.True(ruleSet.Rules.ContainsKey("ADR-907_1"));
        Assert.True(ruleSet.Rules.ContainsKey("ADR-907_2"));
        Assert.True(ruleSet.Rules.ContainsKey("ADR-907_3"));
        Assert.True(ruleSet.Rules.ContainsKey("ADR-907_4"));
    }

    [Fact(DisplayName = "ADR-907_1 应包含 3 个条款")]
    public void Adr907_Rule1_Should_Have_Three_Clauses()
    {
        // Arrange
        var ruleSet = Adr907RuleSet.Instance;

        // Act
        var rule1 = ruleSet.Rules["ADR-907_1"];

        // Assert
        Assert.Equal("ADR-907_1", rule1.RuleId);
        Assert.Equal("ArchitectureTests 的法律地位", rule1.Title);
        Assert.Equal(3, rule1.Clauses.Count);
        Assert.True(rule1.Clauses.ContainsKey("ADR-907_1_1"));
        Assert.True(rule1.Clauses.ContainsKey("ADR-907_1_2"));
        Assert.True(rule1.Clauses.ContainsKey("ADR-907_1_3"));
    }

    [Fact(DisplayName = "ADR-907_2 应包含 8 个条款")]
    public void Adr907_Rule2_Should_Have_Eight_Clauses()
    {
        // Arrange
        var ruleSet = Adr907RuleSet.Instance;

        // Act
        var rule2 = ruleSet.Rules["ADR-907_2"];

        // Assert
        Assert.Equal("ADR-907_2", rule2.RuleId);
        Assert.Equal("命名与组织规范", rule2.Title);
        Assert.Equal(8, rule2.Clauses.Count);
    }

    [Fact(DisplayName = "ADR-907_3 应包含 4 个条款")]
    public void Adr907_Rule3_Should_Have_Four_Clauses()
    {
        // Arrange
        var ruleSet = Adr907RuleSet.Instance;

        // Act
        var rule3 = ruleSet.Rules["ADR-907_3"];

        // Assert
        Assert.Equal("ADR-907_3", rule3.RuleId);
        Assert.Equal("最小断言语义规范", rule3.Title);
        Assert.Equal(4, rule3.Clauses.Count);
    }

    [Fact(DisplayName = "ADR-907_4 应包含 6 个条款")]
    public void Adr907_Rule4_Should_Have_Six_Clauses()
    {
        // Arrange
        var ruleSet = Adr907RuleSet.Instance;

        // Act
        var rule4 = ruleSet.Rules["ADR-907_4"];

        // Assert
        Assert.Equal("ADR-907_4", rule4.RuleId);
        Assert.Equal("Analyzer / CI Gate 映射协议", rule4.Title);
        Assert.Equal(6, rule4.Clauses.Count);
    }

    [Fact(DisplayName = "ADR-907 应总计包含 21 个条款")]
    public void Adr907RuleSet_Should_Have_TwentyOne_Total_Clauses()
    {
        // Arrange
        var ruleSet = Adr907RuleSet.Instance;

        // Act
        var totalClauses = ruleSet.Rules.Values
            .Sum(rule => rule.Clauses.Count);

        // Assert - 根据 ADR-907 文档，应有 21 个条款 (3 + 8 + 4 + 6)
        Assert.Equal(21, totalClauses);
    }

    [Fact(DisplayName = "所有条款应具有执法级别")]
    public void All_Clauses_Should_Have_Enforcement_Level()
    {
        // Arrange
        var ruleSet = Adr907RuleSet.Instance;

        // Act & Assert
        foreach (var rule in ruleSet.Rules.Values)
        {
            foreach (var clause in rule.Clauses.Values)
            {
                Assert.NotNull(clause.Enforcement);
                Assert.NotEmpty(clause.Enforcement);
                Assert.True(clause.Enforcement == "L1" || clause.Enforcement == "L2",
                    $"条款 {clause.ClauseId} 的执法级别应为 L1 或 L2");
            }
        }
    }

    [Fact(DisplayName = "所有条款应具有执行类型")]
    public void All_Clauses_Should_Have_Execution_Type()
    {
        // Arrange
        var ruleSet = Adr907RuleSet.Instance;
        var validExecutionTypes = new[] { "Convention", "Static", "Documentation", "ManualReview" };

        // Act & Assert
        foreach (var rule in ruleSet.Rules.Values)
        {
            foreach (var clause in rule.Clauses.Values)
            {
                Assert.NotNull(clause.ExecutionType);
                Assert.NotEmpty(clause.ExecutionType);
                Assert.Contains(clause.ExecutionType, validExecutionTypes);
            }
        }
    }

    [Fact(DisplayName = "所有条款应具有条件描述")]
    public void All_Clauses_Should_Have_Condition_Description()
    {
        // Arrange
        var ruleSet = Adr907RuleSet.Instance;

        // Act & Assert
        foreach (var rule in ruleSet.Rules.Values)
        {
            foreach (var clause in rule.Clauses.Values)
            {
                Assert.NotNull(clause.Condition);
                Assert.NotEmpty(clause.Condition);
            }
        }
    }

    [Fact(DisplayName = "条款 ID 格式应正确")]
    public void Clause_Ids_Should_Follow_Correct_Format()
    {
        // Arrange
        var ruleSet = Adr907RuleSet.Instance;

        // Act & Assert
        foreach (var rule in ruleSet.Rules.Values)
        {
            foreach (var clause in rule.Clauses.Values)
            {
                // 格式应为 ADR-907_<Rule>_<Clause>
                Assert.Matches(@"^ADR-907_\d+_\d+$", clause.ClauseId);
                Assert.StartsWith(rule.RuleId, clause.ClauseId);
            }
        }
    }
}
