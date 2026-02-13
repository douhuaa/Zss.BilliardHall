namespace Zss.BilliardHall.Tests.SharedTestHelpers.Testing;

/// <summary>
/// RuleId 断言辅助类
/// 提供统一的、可重用的 RuleId 验证和断言逻辑
/// 
/// 设计原则：
/// - 单一职责：专注于 RuleId 的验证和断言
/// - DRY：避免测试代码中的重复断言逻辑
/// - 清晰的失败消息：每个断言都提供明确的上下文信息
/// </summary>
public static class RuleIdAssertions
{
    /// <summary>
    /// 断言解析结果的各个字段是否符合预期
    /// </summary>
    /// <param name="result">实际解析结果</param>
    /// <param name="expectedAdr">预期的 ADR 编号</param>
    /// <param name="expectedRule">预期的 Rule 编号</param>
    /// <param name="expectedClause">预期的 Clause 编号（可选）</param>
    /// <param name="context">上下文信息，用于生成更清晰的错误消息</param>
    public static void AssertParsedRuleId(
        ArchitectureRuleId result,
        int expectedAdr,
        int expectedRule,
        int? expectedClause,
        string? context = null)
    {
        var prefix = string.IsNullOrEmpty(context) ? "" : $"{context}: ";

        result.AdrNumber.Should().Be(expectedAdr, 
            $"{prefix}ADR 编号应为 {expectedAdr}");
        
        result.RuleNumber.Should().Be(expectedRule, 
            $"{prefix}Rule 编号应为 {expectedRule}");
        
        result.ClauseNumber.Should().Be(expectedClause, 
            $"{prefix}Clause 编号应为 {expectedClause?.ToString() ?? "null"}");
        
        var expectedIsRule = expectedClause is null;
        var expectedIsClause = expectedClause is not null;
        
        result.IsRule.Should().Be(expectedIsRule, 
            $"{prefix}IsRule 应为 {expectedIsRule}");
        
        result.IsClause.Should().Be(expectedIsClause, 
            $"{prefix}IsClause 应为 {expectedIsClause}");
    }

    /// <summary>
    /// 断言 TryParse 成功并返回预期结果
    /// </summary>
    public static void AssertTryParseSuccess(
        string input,
        int expectedAdr,
        int expectedRule,
        int? expectedClause)
    {
        var success = RuleIdParser.TryParse(input, out var result);
        
        success.Should().BeTrue($"TryParse 应成功解析 '{input}'");
        
        AssertParsedRuleId(result, expectedAdr, expectedRule, expectedClause, 
            context: $"解析 '{input}'");
    }

    /// <summary>
    /// 断言 TryParse 失败
    /// </summary>
    public static void AssertTryParseFailed(string? input)
    {
        var success = RuleIdParser.TryParse(input!, out var result);
        
        success.Should().BeFalse($"TryParse 应该失败解析 '{input ?? "null"}'");
        result.Should().Be(default(ArchitectureRuleId), 
            "解析失败时应返回默认值");
    }

    /// <summary>
    /// 断言两个 RuleId 相等
    /// </summary>
    public static void AssertRuleIdEquals(
        ArchitectureRuleId actual,
        ArchitectureRuleId expected,
        string? context = null)
    {
        var prefix = string.IsNullOrEmpty(context) ? "" : $"{context}: ";

        actual.Should().Be(expected, $"{prefix}RuleId 应相等");
        actual.AdrNumber.Should().Be(expected.AdrNumber, $"{prefix}ADR 编号应相等");
        actual.RuleNumber.Should().Be(expected.RuleNumber, $"{prefix}Rule 编号应相等");
        actual.ClauseNumber.Should().Be(expected.ClauseNumber, $"{prefix}Clause 编号应相等");
    }

    /// <summary>
    /// 断言 RuleId 是 Rule 级别
    /// </summary>
    public static void AssertIsRule(ArchitectureRuleId ruleId, string? context = null)
    {
        var prefix = string.IsNullOrEmpty(context) ? "" : $"{context}: ";

        ruleId.IsRule.Should().BeTrue($"{prefix}{ruleId} 应是 Rule 级别");
        ruleId.IsClause.Should().BeFalse($"{prefix}{ruleId} 不应是 Clause 级别");
        ruleId.ClauseNumber.Should().BeNull($"{prefix}{ruleId} 的 ClauseNumber 应为 null");
        ruleId.Level.Should().Be(RuleLevel.Rule, $"{prefix}{ruleId} 的 Level 应为 Rule");
    }

    /// <summary>
    /// 断言 RuleId 是 Clause 级别
    /// </summary>
    public static void AssertIsClause(ArchitectureRuleId ruleId, string? context = null)
    {
        var prefix = string.IsNullOrEmpty(context) ? "" : $"{context}: ";

        ruleId.IsClause.Should().BeTrue($"{prefix}{ruleId} 应是 Clause 级别");
        ruleId.IsRule.Should().BeFalse($"{prefix}{ruleId} 不应是 Rule 级别");
        ruleId.ClauseNumber.Should().NotBeNull($"{prefix}{ruleId} 的 ClauseNumber 不应为 null");
        ruleId.Level.Should().Be(RuleLevel.Clause, $"{prefix}{ruleId} 的 Level 应为 Clause");
    }
}
