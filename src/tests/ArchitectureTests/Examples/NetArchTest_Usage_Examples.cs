namespace Zss.BilliardHall.Tests.ArchitectureTests.Examples;

/// <summary>
/// NetArchTest 使用示例
/// 展示如何使用 NetArchTest 和 NetArchTestHelper 编写架构测试
///
/// 这个文件是一个教学示例，展示了：
/// 1. 如何使用 NetArchTest 的流畅 API
/// 2. 如何集成 RuleSetRegistry 获取规则元数据
/// 3. 如何使用 NetArchTestHelper 简化测试编写
/// 4. 如何使用 AssertionMessageBuilder 生成标准化错误消息
///
/// 参考：
/// - docs/guidelines/NETARCHTEST-USAGE-GUIDE.md
/// - src/tests/ArchitectureTests/Specification/RuleSets/ADR001/tests/Adr001_Module_Isolation_Tests.cs
/// </summary>
public sealed class NetArchTest_Usage_Examples
{
    #region 示例: 从 RuleSetRegistry 获取规则元数据

    /// <summary>
    /// 示例: 从 RuleSetRegistry 获取规则元数据
    ///
    /// 优势：
    /// - 规则信息统一管理
    /// - 避免硬编码
    /// - 便于维护
    /// </summary>
    [Fact(DisplayName = "示例: 使用 RuleSetRegistry")]
    public void Example_Using_RuleSetRegistry()
    {
        // 从 Registry 获取规则集（ADR-001）
        var ruleSet = RuleSetRegistry.GetStrict(1);

        // 验证规则集存在
        ruleSet.Should().NotBeNull();
        ruleSet.AdrNumber.Should().Be(1);

        // 获取具体规则和条款
        var rule = ruleSet.GetRule(1);
        rule.Should().NotBeNull();
        rule.Summary.Should().Be("模块物理隔离");

        var clause = ruleSet.GetClause(1, 1);
        clause.Should().NotBeNull();
        clause.Id.ToString().Should().Be("ADR-001_1_1");

        // 在实际测试中使用规则元数据
        var ruleId = clause.Id.ToString();
        var condition = clause.Condition;
        var enforcement = clause.Enforcement;

        // 输出信息（用于演示）
        Console.WriteLine($"规则 ID: {ruleId}");
        Console.WriteLine($"条件: {condition}");
        Console.WriteLine($"执行: {enforcement}");
    }

    #endregion
}
