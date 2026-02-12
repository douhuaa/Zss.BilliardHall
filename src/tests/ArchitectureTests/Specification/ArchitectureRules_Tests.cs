namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// 验证 RuleSetRegistry 和规则集定义的正确性
/// 确保从 ADR 文档定义的规则集可以正常工作
///
/// 重构说明：
/// - 使用 RuleSetValidator 辅助类替代重复的验证逻辑
/// - 保持 Theory + InlineData/MemberData 的数据驱动测试模式
/// - 提取的辅助方法移到 RuleSetValidator 工具类
/// </summary>
public sealed class ArchitectureRules_Tests
{
    #region 测试数据源

    /// <summary>
    /// 所有已定义的 ADR 编号（物化避免延迟枚举问题）
    /// </summary>
    public static IEnumerable<object[]> AllAdrNumbers =>
        RuleSetRegistry.GetAllAdrNumbers()
            .Select(n => new object[] { n })
            .ToList();

    #endregion

    #region RuleSet 结构完整性测试

    [Theory(DisplayName = "RuleSet 结构与标识符应符合全局规范")]
    [MemberData(nameof(AllAdrNumbers))]
    public void RuleSet_Should_Maintain_Structural_Integrity(int adrNumber)
    {
        var ruleSet = RuleSetRegistry.GetStrict(adrNumber);

        // 使用统一的验证器执行完整验证
        RuleSetValidator.ValidateFull(ruleSet, adrNumber);
    }

    #endregion

    #region 核心规则和条款验证测试

    [Theory(DisplayName = "核心业务规则定义应匹配 ADR 规范")]
    [InlineData(1, 1, "模块物理隔离", RuleSeverity.Constitutional)]
    [InlineData(900, 1, "架构裁决权威性", RuleSeverity.Governance)]
    [InlineData(907, 3, "最小断言语义规范", null)]
    [InlineData(120, 1, "事件类型命名规范", null)]
    public void Core_Rules_Should_Match_Specification(
        int adr,
        int ruleNum,
        string summary,
        RuleSeverity? severity)
    {
        var rule = RuleSetRegistry.GetStrict(adr).GetRule(ruleNum);

        rule.Should().NotBeNull($"ADR-{adr:000} Rule {ruleNum} 应存在");
        rule!.Summary.Should().Be(summary, $"ADR-{adr:000} Rule {ruleNum} 摘要应为预期值");

        if (severity.HasValue)
        {
            rule.Severity.Should().Be(severity.Value,
                $"ADR-{adr:000} Rule {ruleNum} 严重程度应为 {severity.Value}");
        }
    }

    [Theory(DisplayName = "关键条款约束应匹配 ADR 规范内容")]
    [InlineData(1, 1, 1, "Condition", "模块按业务能力独立划分")]
    [InlineData(900, 1, 1, "Condition", "ADR 正文是唯一裁决依据")]
    [InlineData(907, 3, 4, "Enforcement", "Assert.True(true)")]
    [InlineData(120, 1, 1, "Enforcement", "Event 后缀")]
    public void Core_Clauses_Should_Match_Specification(
        int adr,
        int ruleNum,
        int clauseNum,
        string type,
        string expected)
    {
        var clause = RuleSetRegistry.GetStrict(adr).GetClause(ruleNum, clauseNum);

        clause.Should().NotBeNull($"ADR-{adr:000} Clause {ruleNum}.{clauseNum} 应存在");

        var content = type.Equals("Condition", StringComparison.OrdinalIgnoreCase)
            ? clause!.Condition
            : clause!.Enforcement;

        content.Should().Contain(expected,
            $"ADR-{adr:000} Clause {ruleNum}.{clauseNum} 的 {type} 应包含 '{expected}'");
    }

    #endregion

    #region Registry 行为测试

    [Fact(DisplayName = "Registry 应支持常规获取与错误处理")]
    public void Registry_Should_Handle_Lookup_Correctly()
    {
        // 测试宽容模式：未定义的 ADR 应返回 null
        RuleSetRegistry.Get(999).Should().BeNull("未定义的 ADR 应返回 null");

        // 验证关键 ADR 存在
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

    #endregion
}
