namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// 验证 RuleSetRegistry 和规则集定义的正确性
/// 确保从 ADR 文档定义的规则集可以正常工作
/// </summary>
public sealed class ArchitectureRulesTests
{
    // 物化 MemberData：所有已定义的 ADR 编号（避免延迟枚举问题）
    public static IEnumerable<object[]> AllAdrNumbers =>
        RuleSetRegistry.GetAllAdrNumbers()
            .Select(n => new object[] { n })
            .ToList();

    [Theory(DisplayName = "RuleSet 结构与标识符应符合全局规范")]
    [MemberData(nameof(AllAdrNumbers))]
    public void RuleSet_Should_Maintain_Structural_Integrity(int adrNumber)
    {
        var ruleSet = RuleSetRegistry.GetStrict(adrNumber);

        VerifyRuleStructure(ruleSet, adrNumber);
        VerifyClauseStructure(ruleSet, adrNumber);
    }

    [Theory(DisplayName = "核心业务规则定义应匹配 ADR 规范")]
    [InlineData(1, 1, "模块物理隔离", RuleSeverity.Constitutional)]
    [InlineData(900, 1, "架构裁决权威性", RuleSeverity.Governance)]
    [InlineData(907, 3, "最小断言语义规范", null)]
    [InlineData(120, 1, "事件类型命名规范", null)]
    public void Core_Rules_Should_Match_Specification(int adr, int ruleNum, string summary, RuleSeverity? severity)
    {
        var rule = RuleSetRegistry.GetStrict(adr).GetRule(ruleNum);
        rule.Should().NotBeNull($"ADR-{adr:0000} Rule {ruleNum} 应存在");
        rule!.Summary.Should().Be(summary, $"ADR-{adr:0000} Rule {ruleNum} 摘要应为预期值");
        if (severity.HasValue) rule.Severity.Should().Be(severity.Value);
    }

    [Theory(DisplayName = "关键条款约束应匹配 ADR 规范内容")]
    [InlineData(1, 1, 1, "Condition", "模块按业务能力独立划分")]
    [InlineData(900, 1, 1, "Condition", "ADR 正文是唯一裁决依据")]
    [InlineData(907, 3, 4, "Enforcement", "Assert.True(true)")]
    [InlineData(120, 1, 1, "Enforcement", "Event 后缀")]
    public void Core_Clauses_Should_Match_Specification(int adr, int ruleNum, int clauseNum, string type, string expected)
    {
        var clause = RuleSetRegistry.GetStrict(adr).GetClause(ruleNum, clauseNum);
        clause.Should().NotBeNull($"ADR-{adr:0000} Clause {ruleNum}.{clauseNum} 应存在");

        var content = type.Equals("Condition", StringComparison.OrdinalIgnoreCase)
            ? clause!.Condition
            : clause!.Enforcement;

        content.Should().Contain(expected, $"ADR-{adr:0000} Clause {ruleNum}.{clauseNum} 应包含期望文本");
    }

    [Fact(DisplayName = "Registry 应支持常规获取与错误处理")]
    public void Registry_Should_Handle_Lookup_Correctly()
    {
        RuleSetRegistry.Get(999).Should().BeNull("未定义的 ADR 应返回 null");

        var adrList = RuleSetRegistry.GetAllAdrNumbers();
        adrList.Should().Contain(new[] { 1, 900, 907 }, "应包含关键 ADR 编号");
    }

    [Fact(DisplayName = "RuleSet 实例应实现单例惰性加载")]
    public void RuleSet_Instances_Should_Be_Singletons()
    {
        var first = RuleSetRegistry.GetStrict(1);
        var second = RuleSetRegistry.GetStrict(1);
        first.Should().BeSameAs(second, "多次访问应返回同一内存实例");
    }

    #region 辅助方法

    private static void VerifyRuleStructure(dynamic ruleSet, int adrNumber)
    {
        // 基本计数检查
        ((int)ruleSet.RuleCount).Should().BeGreaterThan(0, $"ADR-{adrNumber:0000} 必须包含规则");

        // 逐条检查以便失败定位（每项断言都包含具体 ID）
        foreach (var rule in ruleSet.Rules)
        {
            var id = rule.Id.ToString();
            ((RuleLevel)rule.Id.Level).Should().Be(RuleLevel.Rule, $"规则 {id} 的级别应为 Rule");
            ((int)rule.Id.AdrNumber).Should().Be(adrNumber, $"规则 {id} 的 ADR 应为 ADR-{adrNumber:0000}");
            ((string)rule.Summary).Should().NotBeNullOrWhiteSpace($"规则 {id} 的摘要不应为空");
        }
    }

    private static void VerifyClauseStructure(dynamic ruleSet, int adrNumber)
    {
        ((int)ruleSet.ClauseCount).Should().BeGreaterThanOrEqualTo(
            (int)ruleSet.RuleCount,
            $"ADR-{adrNumber:0000} 的条款数 ({ruleSet.ClauseCount}) 应至少等于规则数 ({ruleSet.RuleCount})");

        foreach (var clause in ruleSet.Clauses)
        {
            var id = clause.Id.ToString();
            ((RuleLevel)clause.Id.Level).Should().Be(RuleLevel.Clause, $"条款 {id} 的级别应为 Clause");
            ((int)clause.Id.AdrNumber).Should().Be(adrNumber, $"条款 {id} 的 ADR 应为 ADR-{adrNumber:0000}");
            ((string)clause.Condition).Should().NotBeNullOrWhiteSpace($"条款 {id} 的 Condition 不应为空");
            ((string)clause.Enforcement).Should().NotBeNullOrWhiteSpace($"条款 {id} 的 Enforcement 不应为空");
        }
    }

    #endregion
}
