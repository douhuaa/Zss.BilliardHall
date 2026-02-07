namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// 验证 RuleSetRegistry 和规则集定义的正确性
/// 确保从 ADR 文档定义的规则集可以正常工作
/// </summary>
public sealed class ArchitectureRulesTests
{
    [Fact(DisplayName = "所有 RuleSet 应该包含至少一个规则")]
    public void All_RuleSets_Should_Have_At_Least_One_Rule()
    {
        // Arrange & Act
        var ruleSets = RuleSetRegistry.GetAllRuleSets().ToList();

        // Assert
        ruleSets.Should().NotBeEmpty("应该至少定义一个 RuleSet");

        foreach (var ruleSet in ruleSets)
        {
            ruleSet.RuleCount.Should().BeGreaterThan(0,
                $"ADR-{ruleSet.AdrNumber:0000} 应该至少包含一个规则");
        }
    }

    [Theory(DisplayName = "RuleSetRegistry.Get 应该能够通过 ADR 编号获取规则集")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(120)]
    [InlineData(201)]
    [InlineData(900)]
    [InlineData(907)]
    public void RuleSetRegistry_Get_Should_Return_RuleSet_For_Valid_Adr_Number(int adrNumber)
    {
        // Act
        var ruleSet = RuleSetRegistry.Get(adrNumber);

        // Assert
        ruleSet.Should().NotBeNull($"应该能够获取 ADR-{adrNumber:0000} 的规则集");
        ruleSet!.AdrNumber.Should().Be(adrNumber);
    }

    [Fact(DisplayName = "RuleSetRegistry.Get 对于未定义的 ADR 应该返回 null")]
    public void RuleSetRegistry_Get_Should_Return_Null_For_Undefined_Adr()
    {
        // Act
        var ruleSet = RuleSetRegistry.Get(999);

        // Assert
        ruleSet.Should().BeNull("未定义的 ADR 应该返回 null");
    }

    [Fact(DisplayName = "ADR-001 应该包含预期的规则")]
    public void Adr001_Should_Have_Expected_Rules()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(1);

        // Assert
        ruleSet.AdrNumber.Should().Be(1);
        ruleSet.RuleCount.Should().BeGreaterThanOrEqualTo(3, "ADR-001 至少应该有 3 个规则");

        // 验证 Rule 1: 模块物理隔离
        ruleSet.HasRule(1).Should().BeTrue("应该包含 Rule 1");
        var rule1 = ruleSet.GetRule(1);
        rule1.Should().NotBeNull();
        rule1!.Summary.Should().Be("模块物理隔离");
        rule1.Severity.Should().Be(RuleSeverity.Constitutional);

        // 验证 Clause 1.1
        ruleSet.HasClause(1, 1).Should().BeTrue("应该包含 Clause 1.1");
        var clause11 = ruleSet.GetClause(1, 1);
        clause11.Should().NotBeNull();
        clause11!.Condition.Should().Contain("模块按业务能力独立划分");
    }

    [Fact(DisplayName = "ADR-900 应该包含预期的规则")]
    public void Adr900_Should_Have_Expected_Rules()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(900);

        // Assert
        ruleSet.AdrNumber.Should().Be(900);
        ruleSet.RuleCount.Should().BeGreaterThanOrEqualTo(4, "ADR-900 至少应该有 4 个规则");

        // 验证 Rule 1: 架构裁决权威性
        ruleSet.HasRule(1).Should().BeTrue("应该包含 Rule 1");
        var rule1 = ruleSet.GetRule(1);
        rule1.Should().NotBeNull();
        rule1!.Summary.Should().Be("架构裁决权威性");
        rule1.Severity.Should().Be(RuleSeverity.Governance);

        // 验证 Clause 1.1
        ruleSet.HasClause(1, 1).Should().BeTrue("应该包含 Clause 1.1");
        var clause11 = ruleSet.GetClause(1, 1);
        clause11.Should().NotBeNull();
        clause11!.Condition.Should().Contain("ADR 正文是唯一裁决依据");
    }

    [Fact(DisplayName = "ADR-907 应该包含预期的规则")]
    public void Adr907_Should_Have_Expected_Rules()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(907);

        // Assert
        ruleSet.AdrNumber.Should().Be(907);
        ruleSet.RuleCount.Should().BeGreaterThanOrEqualTo(4, "ADR-907 至少应该有 4 个规则");

        // 验证 Rule 3: 最小断言语义规范
        ruleSet.HasRule(3).Should().BeTrue("应该包含 Rule 3");
        var rule3 = ruleSet.GetRule(3);
        rule3.Should().NotBeNull();
        rule3!.Summary.Should().Be("最小断言语义规范");

        // 验证 Clause 3.4: 禁止形式化断言
        ruleSet.HasClause(3, 4).Should().BeTrue("应该包含 Clause 3.4");
        var clause34 = ruleSet.GetClause(3, 4);
        clause34.Should().NotBeNull();
        clause34!.Enforcement.Should().Contain("Assert.True(true)");
    }

    [Fact(DisplayName = "ADR-120 应该包含预期的规则")]
    public void Adr120_Should_Have_Expected_Rules()
    {
        // Arrange
        var ruleSet = RuleSetRegistry.GetStrict(120);

        // Assert
        ruleSet.AdrNumber.Should().Be(120);
        ruleSet.RuleCount.Should().BeGreaterThanOrEqualTo(3, "ADR-120 至少应该有 3 个规则");

        // 验证 Rule 1: 事件类型命名规范
        ruleSet.HasRule(1).Should().BeTrue("应该包含 Rule 1");
        var rule1 = ruleSet.GetRule(1);
        rule1.Should().NotBeNull();
        rule1!.Summary.Should().Be("事件类型命名规范");

        // 验证 Clause 1.1: 事件命名模式
        ruleSet.HasClause(1, 1).Should().BeTrue("应该包含 Clause 1.1");
        var clause11 = ruleSet.GetClause(1, 1);
        clause11.Should().NotBeNull();
        clause11!.Enforcement.Should().Contain("Event 后缀");
    }

    [Fact(DisplayName = "所有 RuleSet 的条款数应该大于规则数")]
    public void All_RuleSets_Should_Have_More_Clauses_Than_Rules()
    {
        // Arrange & Act
        var ruleSets = RuleSetRegistry.GetAllRuleSets().ToList();

        // Assert
        foreach (var ruleSet in ruleSets)
        {
            ruleSet.ClauseCount.Should().BeGreaterThanOrEqualTo(ruleSet.RuleCount,
                $"ADR-{ruleSet.AdrNumber:0000} 的条款数 ({ruleSet.ClauseCount}) 应该至少等于规则数 ({ruleSet.RuleCount})");
        }
    }

    [Fact(DisplayName = "所有规则应该有有效的 RuleId")]
    public void All_Rules_Should_Have_Valid_RuleIds()
    {
        // Arrange & Act
        var ruleSets = RuleSetRegistry.GetAllRuleSets().ToList();

        // Assert
        foreach (var ruleSet in ruleSets)
        {
            foreach (var rule in ruleSet.Rules)
            {
                rule.Id.Level.Should().Be(RuleLevel.Rule,
                    $"规则 {rule.Id} 的级别应该是 Rule");

                rule.Id.AdrNumber.Should().Be(ruleSet.AdrNumber,
                    $"规则 {rule.Id} 的 ADR 编号应该与 RuleSet 一致");

                rule.Summary.Should().NotBeNullOrWhiteSpace(
                    $"规则 {rule.Id} 的摘要不应为空");
            }
        }
    }

    [Fact(DisplayName = "所有条款应该有有效的 RuleId")]
    public void All_Clauses_Should_Have_Valid_RuleIds()
    {
        // Arrange & Act
        var ruleSets = RuleSetRegistry.GetAllRuleSets().ToList();

        // Assert
        foreach (var ruleSet in ruleSets)
        {
            foreach (var clause in ruleSet.Clauses)
            {
                clause.Id.Level.Should().Be(RuleLevel.Clause,
                    $"条款 {clause.Id} 的级别应该是 Clause");

                clause.Id.AdrNumber.Should().Be(ruleSet.AdrNumber,
                    $"条款 {clause.Id} 的 ADR 编号应该与 RuleSet 一致");

                clause.Condition.Should().NotBeNullOrWhiteSpace(
                    $"条款 {clause.Id} 的条件描述不应为空");

                clause.Enforcement.Should().NotBeNullOrWhiteSpace(
                    $"条款 {clause.Id} 的执行要求不应为空");
            }
        }
    }

    [Fact(DisplayName = "GetAllAdrNumbers 应该返回所有已定义的 ADR 编号")]
    public void GetAllAdrNumbers_Should_Return_All_Defined_Adr_Numbers()
    {
        // Act
        var adrNumbers = RuleSetRegistry.GetAllAdrNumbers().ToList();

        // Assert
        adrNumbers.Should().NotBeEmpty("应该至少定义一个 ADR");
        adrNumbers.Should().Contain(1, "应该包含 ADR-001");
        adrNumbers.Should().Contain(900, "应该包含 ADR-900");
        adrNumbers.Should().Contain(907, "应该包含 ADR-907");

        // 验证每个 ADR 编号都能获取到对应的 RuleSet
        foreach (var adrNumber in adrNumbers)
        {
            var ruleSet = RuleSetRegistry.Get(adrNumber);
            ruleSet.Should().NotBeNull($"ADR-{adrNumber:0000} 应该有对应的 RuleSet");
        }
    }

    [Fact(DisplayName = "惰性初始化应该工作正常")]
    public void Lazy_Initialization_Should_Work_Correctly()
    {
        // Act - 通过 Registry 多次获取同一个 RuleSet
        var ruleSet1 = RuleSetRegistry.GetStrict(1);
        var ruleSet2 = RuleSetRegistry.GetStrict(1);

        // Assert - Registry 应该返回相同的实例
        ruleSet1.Should().BeSameAs(ruleSet2, "多次访问应该返回同一个实例（惰性初始化）");
    }
}
