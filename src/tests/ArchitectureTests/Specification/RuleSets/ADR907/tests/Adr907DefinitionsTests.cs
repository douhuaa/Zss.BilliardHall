namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.Tests;

/// <summary>
/// Adr907Definitions 的单元测试
/// 验证 ADR-907 规则定义的正确性和完整性
/// </summary>
public sealed class Adr907DefinitionsTests
{
    [Fact(DisplayName = "应该定义4个规则")]
    public void Should_Define_Four_Rules()
    {
        // Arrange & Act
        var allRules = Adr907Definitions.AllRules;

        // Assert
        allRules.Should().NotBeNull();
        allRules.Should().HaveCount(4, "ADR-907 定义了4个规则");
    }

    [Fact(DisplayName = "所有规则ID应该唯一")]
    public void All_Rule_Ids_Should_Be_Unique()
    {
        // Arrange & Act
        var ruleIds = Adr907Definitions.AllRules.Select(r => r.RuleId).ToList();

        // Assert
        ruleIds.Should().OnlyHaveUniqueItems("每个规则必须有唯一的 RuleId");
    }

    [Fact(DisplayName = "所有条款 (RuleId, ClauseId) 组合应该唯一")]
    public void All_Clause_Combinations_Should_Be_Unique()
    {
        // Arrange & Act
        var clauseCombinations = Adr907Definitions.AllRules
            .SelectMany(r => r.Clauses)
            .Select(c => (c.RuleId, c.ClauseId))
            .ToList();

        // Assert
        clauseCombinations.Should().OnlyHaveUniqueItems(
            "每个条款必须有唯一的 (RuleId, ClauseId) 组合");
    }

    [Fact(DisplayName = "所有规则都应该有至少一个条款")]
    public void All_Rules_Should_Have_At_Least_One_Clause()
    {
        // Arrange & Act
        var rulesWithoutClauses = Adr907Definitions.AllRules
            .Where(r => r.Clauses.Count == 0)
            .ToList();

        // Assert
        rulesWithoutClauses.Should().BeEmpty(
            "每个规则至少应该有一个条款来定义如何执行");
    }

    [Fact(DisplayName = "所有条款的 Description 应该非空")]
    public void All_Clauses_Should_Have_Non_Empty_Description()
    {
        // Arrange & Act
        var clausesWithEmptyDescription = Adr907Definitions.AllRules
            .SelectMany(r => r.Clauses)
            .Where(c => string.IsNullOrWhiteSpace(c.Description))
            .ToList();

        // Assert
        clausesWithEmptyDescription.Should().BeEmpty(
            "所有条款都必须有非空的 Description");
    }

    [Fact(DisplayName = "所有条款的 ValidationHint 应该非空")]
    public void All_Clauses_Should_Have_Non_Empty_ValidationHint()
    {
        // Arrange & Act
        var clausesWithEmptyHint = Adr907Definitions.AllRules
            .SelectMany(r => r.Clauses)
            .Where(c => string.IsNullOrWhiteSpace(c.ValidationHint))
            .ToList();

        // Assert
        clausesWithEmptyHint.Should().BeEmpty(
            "所有条款都必须有非空的 ValidationHint");
    }

    [Fact(DisplayName = "所有条款的 Name 应该非空")]
    public void All_Clauses_Should_Have_Non_Empty_Name()
    {
        // Arrange & Act
        var clausesWithEmptyName = Adr907Definitions.AllRules
            .SelectMany(r => r.Clauses)
            .Where(c => string.IsNullOrWhiteSpace(c.Name))
            .ToList();

        // Assert
        clausesWithEmptyName.Should().BeEmpty(
            "所有条款都必须有非空的 Name");
    }

    [Fact(DisplayName = "所有条款的 RuleId 应该与其父规则匹配")]
    public void All_Clauses_RuleId_Should_Match_Parent_Rule()
    {
        // Arrange & Act
        var mismatchedClauses = Adr907Definitions.AllRules
            .SelectMany(r => r.Clauses.Select(c => new { Rule = r, Clause = c }))
            .Where(x => x.Clause.RuleId != x.Rule.RuleId)
            .ToList();

        // Assert
        mismatchedClauses.Should().BeEmpty(
            "所有条款的 RuleId 必须与其所属规则的 RuleId 匹配");
    }

    [Fact(DisplayName = "ClauseRegistrationStrategyResolver 应该支持所有使用的 ExecutionType")]
    public void Resolver_Should_Support_All_Used_Execution_Types()
    {
        // Arrange
        var usedExecutionTypes = Adr907Definitions.AllRules
            .SelectMany(r => r.Clauses)
            .Select(c => c.ExecutionType)
            .Distinct()
            .ToList();

        // Act & Assert
        foreach (var executionType in usedExecutionTypes)
        {
            ClauseRegistrationStrategyResolver.IsSupported(executionType)
                .Should().BeTrue(
                    $"Resolver 必须支持 ExecutionType: {executionType}");
        }
    }

    [Fact(DisplayName = "应该能成功创建 Adr907RuleSet")]
    public void Should_Successfully_Create_Adr907_RuleSet()
    {
        // Arrange & Act
        var ruleSetDefinition = new Adr907RuleSet();
        var ruleSet = ruleSetDefinition.Define();

        // Assert
        ruleSet.Should().NotBeNull();
        ruleSet.AdrNumber.Should().Be(907);
        ruleSet.RuleCount.Should().Be(4, "应该包含4个规则");
        
        // 计算预期的条款总数
        var expectedClauseCount = Adr907Definitions.AllRules.Sum(r => r.Clauses.Count);
        ruleSet.ClauseCount.Should().Be(expectedClauseCount, "所有条款都应该被注册");
    }

    [Theory(DisplayName = "所有条款 RuleId 应该在有效范围内")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void All_Clause_RuleIds_Should_Be_In_Valid_Range(int ruleId)
    {
        // Arrange & Act
        var clausesForRule = Adr907Definitions.AllRules
            .Where(r => r.RuleId == ruleId)
            .SelectMany(r => r.Clauses)
            .ToList();

        // Assert
        clausesForRule.Should().NotBeEmpty($"规则 {ruleId} 应该有条款");
        clausesForRule.Should().AllSatisfy(c => 
            c.ClauseId.Should().BeGreaterThan(0, "ClauseId 必须大于 0"));
    }

    [Fact(DisplayName = "Adr907ExecutionBindings 应该只包含有效的 (RuleId, ClauseId) 组合")]
    public void Execution_Bindings_Should_Only_Reference_Valid_Clauses()
    {
        // Arrange
        var validCombinations = Adr907Definitions.AllRules
            .SelectMany(r => r.Clauses)
            .Select(c => (c.RuleId, c.ClauseId))
            .ToHashSet();

        // Act
        var invalidBindings = Adr907ExecutionBindings.All
            .Where(b => !validCombinations.Contains((b.RuleId, b.ClauseId)))
            .ToList();

        // Assert
        invalidBindings.Should().BeEmpty(
            "所有执行绑定必须引用存在的 (RuleId, ClauseId) 组合");
    }

    [Fact(DisplayName = "ClauseSpec 应该能正确验证")]
    public void ClauseSpec_Should_Validate_Correctly()
    {
        // Arrange
        var validSpec = new ClauseSpec(
            RuleId: 1,
            ClauseId: 1,
            Name: "测试名称",
            Description: "测试描述",
            ExecutionType: ClauseExecutionType.Convention,
            ValidationHint: "验证提示");

        // Act & Assert
        validSpec.Invoking(s => s.Validate()).Should().NotThrow();
    }

    [Theory(DisplayName = "ClauseSpec 应该拒绝无效的 RuleId")]
    [InlineData(0)]
    [InlineData(-1)]
    public void ClauseSpec_Should_Reject_Invalid_RuleId(int invalidRuleId)
    {
        // Arrange
        var invalidSpec = new ClauseSpec(
            RuleId: invalidRuleId,
            ClauseId: 1,
            Name: "测试",
            Description: "描述",
            ExecutionType: ClauseExecutionType.Convention,
            ValidationHint: "提示");

        // Act & Assert
        invalidSpec.Invoking(s => s.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*RuleId*");
    }

    [Theory(DisplayName = "ClauseSpec 应该拒绝无效的 ClauseId")]
    [InlineData(0)]
    [InlineData(-1)]
    public void ClauseSpec_Should_Reject_Invalid_ClauseId(int invalidClauseId)
    {
        // Arrange
        var invalidSpec = new ClauseSpec(
            RuleId: 1,
            ClauseId: invalidClauseId,
            Name: "测试",
            Description: "描述",
            ExecutionType: ClauseExecutionType.Convention,
            ValidationHint: "提示");

        // Act & Assert
        invalidSpec.Invoking(s => s.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*ClauseId*");
    }
}
