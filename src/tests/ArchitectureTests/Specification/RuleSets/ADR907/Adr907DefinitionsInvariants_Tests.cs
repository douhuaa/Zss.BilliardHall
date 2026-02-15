namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907;

/// <summary>
/// ADR-907 规则定义的不变量清单式测试
/// 验证规则定义的基本契约，不依赖具体规则数量
/// </summary>
public sealed class Adr907DefinitionsInvariants_Tests
{
    #region 基础完整性

    [Fact(DisplayName = "不变量：AllRules 不为空")]
    public void AllRules_Should_Not_Be_Null()
    {
        Adr907Definitions.AllRules.Should().NotBeNull("规则集合必须存在");
    }

    [Fact(DisplayName = "不变量：每个规则至少有一个条款")]
    public void Each_Rule_Should_Have_At_Least_One_Clause()
    {
        foreach (var rule in Adr907Definitions.AllRules)
        {
            rule.Clauses.Should().NotBeEmpty(
            $"规则 {rule.RuleId} 必须至少包含一个条款");
        }
    }

    [Fact(DisplayName = "不变量：规则ID必须唯一")]
    public void RuleIds_Should_Be_Unique()
    {
        var ruleIds = Adr907Definitions.AllRules.Select(r => r.RuleId);
        ruleIds.Should().OnlyHaveUniqueItems();
    }

    [Fact(DisplayName = "不变量：条款组合 (RuleId, ClauseId) 必须唯一")]
    public void Clause_Combinations_Should_Be_Unique()
    {
        var combinations = Adr907Definitions.AllRules
            .SelectMany(r => r.Clauses)
            .Select(c => (c.RuleId, c.ClauseId));

        combinations.Should().OnlyHaveUniqueItems();
    }

    #endregion

    #region 条款属性不变量

    [Fact(DisplayName = "不变量：所有条款的 Name/Description/ValidationHint 非空")]
    public void Clause_Properties_Should_Not_Be_Empty()
    {
        foreach (var clause in Adr907Definitions.AllRules.SelectMany(r => r.Clauses))
        {
            clause.Name.Should().NotBeNullOrWhiteSpace("条款 Name 不能为空");
            clause.Description.Should().NotBeNullOrWhiteSpace("条款 Description 不能为空");
            clause.ValidationHint.Should().NotBeNullOrWhiteSpace("条款 ValidationHint 不能为空");
        }
    }

    [Fact(DisplayName = "不变量：条款 RuleId 必与父规则匹配")]
    public void Clause_RuleId_Should_Match_Parent_Rule()
    {
        foreach (var rule in Adr907Definitions.AllRules)
            foreach (var clause in rule.Clauses)
            {
                clause.RuleId.Should().Be(rule.RuleId, "条款必须属于其父规则");
            }
    }

    #endregion

    #region ExecutionType 支持

    [Fact(DisplayName = "不变量：所有条款的 ExecutionType 必须被 Resolver 支持")]
    public void Clause_ExecutionType_Should_Be_Supported()
    {
        var executionTypes = Adr907Definitions.AllRules
            .SelectMany(r => r.Clauses)
            .Select(c => c.ExecutionType)
            .Distinct();

        foreach (var type in executionTypes)
        {
            ClauseRegistrationStrategyResolver.IsSupported(type)
                .Should().BeTrue($"Resolver 必须支持 ExecutionType: {type}");
        }
    }

    #endregion

    #region RuleSet 构建契约

    [Fact(DisplayName = "不变量：Adr907RuleSet 能正确创建")]
    public void RuleSet_Should_Build_Correctly()
    {
        var ruleSetDefinition = new Adr907RuleSet();
        var ruleSet = ruleSetDefinition.Define();

        ruleSet.Should().NotBeNull("规则集定义必须返回有效对象");
        ruleSet.AdrNumber.Should().Be(907, "ADR 编号应为 907");

        // 所有规则和条款都被注册
        ruleSet.RuleCount.Should().Be(Adr907Definitions.AllRules.Count);
        var expectedClauseCount = Adr907Definitions.AllRules.Sum(r => r.Clauses.Count);
        ruleSet.ClauseCount.Should().Be(expectedClauseCount);
    }

    #endregion

    #region ClauseSpec 验证契约

    [Fact(DisplayName = "不变量：有效 ClauseSpec 验证不抛异常")]
    public void Valid_ClauseSpec_Should_Validate()
    {
        var spec = new ClauseSpec(
        RuleId: 1,
        ClauseId: 1,
        Name: "测试",
        Description: "测试描述",
        ExecutionType: ClauseExecutionType.Convention,
        ValidationHint: "提示"
        );

        spec.Invoking(s => s.Validate()).Should().NotThrow();
    }

    [Theory(DisplayName = "不变量：ClauseSpec 拒绝无效 RuleId/ClauseId")]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void ClauseSpec_Should_Reject_Invalid_Ids(int ruleId, int clauseId)
    {
        var spec = new ClauseSpec(
        RuleId: ruleId,
        ClauseId: clauseId,
        Name: "测试",
        Description: "测试描述",
        ExecutionType: ClauseExecutionType.Convention,
        ValidationHint: "提示"
        );

        spec.Invoking(s => s.Validate())
            .Should().Throw<ArgumentException>();
    }

    #endregion
}
