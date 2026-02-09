using Xunit;
using Xunit.Abstractions;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Common;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// 规则集执行器测试
/// 使用 Theory 驱动，批量执行所有已注册的架构规则
/// 
/// 执行策略：
/// - Governance/Enforcement (L1/L2)：失败将阻断 CI
/// - Heuristics (L3)：仅输出警告，不阻断
/// </summary>
public sealed class RuleSetRunnerTests
{
    private readonly ITestOutputHelper _output;

    public RuleSetRunnerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// 提供所有规则作为测试数据
    /// </summary>
    public static IEnumerable<object[]> RuleData()
    {
        var options = new ArchitectureRulesOptions();
        var rules = Runner.CentralizedRuleSetRegistry.All(options).ToArray();
        
        foreach (var rule in rules)
        {
            yield return new object[] { rule };
        }
    }

    /// <summary>
    /// 架构规范规则验证测试
    /// 每个规则作为一个独立的测试用例执行
    /// </summary>
    /// <param name="rule">要验证的规则</param>
    [Theory(DisplayName = "架构规范规则验证")]
    [MemberData(nameof(RuleData))]
    public void Should_Conform_To_Rules(RuleDefinition rule)
    {
        // 获取当前加载的所有程序集
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // 执行规则验证
        var result = rule.Evaluate(assemblies);

        // 输出规则信息
        _output.WriteLine($"规则: {rule.Id}");
        _output.WriteLine($"标题: {rule.Title}");
        _output.WriteLine($"层级: {rule.Layer}");
        _output.WriteLine($"严重程度: {rule.Severity}");
        _output.WriteLine($"结果: {(result.Passed ? "✓ 通过" : "✗ 失败")}");
        
        if (!string.IsNullOrWhiteSpace(result.Message))
        {
            _output.WriteLine($"消息: {result.Message}");
        }

        // Heuristics 层规则：仅警告，不阻断
        if (rule.Layer == RuleLayer.Heuristics)
        {
            // Heuristics 规则总是通过测试，只输出警告信息
            if (result.Warnings is { Length: > 0 })
            {
                _output.WriteLine($"⚠ 警告信息:");
                foreach (var warning in result.Warnings)
                {
                    _output.WriteLine($"  - {warning}");
                }
            }
            return;
        }

        // Governance/Enforcement 层规则：失败将阻断测试
        Assert.True(result.Passed,
            $"❌ 规则 {rule.Id} 验证失败\n" +
            $"标题: {rule.Title}\n" +
            $"层级: {rule.Layer}\n" +
            $"严重程度: {rule.Severity}\n" +
            $"详细信息:\n{result.Message}");
    }

    /// <summary>
    /// 测试：验证规则集统计信息
    /// </summary>
    [Fact(DisplayName = "规则集统计信息应正确")]
    public void RuleSet_Statistics_Should_Be_Correct()
    {
        var options = new ArchitectureRulesOptions();
        var stats = Runner.CentralizedRuleSetRegistry.GetStatistics(options);

        _output.WriteLine($"规则总数: {stats.Total}");
        _output.WriteLine($"治理层 (Governance): {stats.GovernanceCount}");
        _output.WriteLine($"执行层 (Enforcement): {stats.EnforcementCount}");
        _output.WriteLine($"启发层 (Heuristics): {stats.HeuristicsCount}");
        _output.WriteLine($"L1 级别: {stats.L1Count}");
        _output.WriteLine($"L2 级别: {stats.L2Count}");
        _output.WriteLine($"L3 级别: {stats.L3Count}");

        // 验证规则集不为空
        Assert.True(stats.Total > 0, "规则集不应为空");
        
        // 验证计数一致性
        Assert.Equal(stats.Total, 
            stats.GovernanceCount + stats.EnforcementCount + stats.HeuristicsCount);
        Assert.Equal(stats.Total, 
            stats.L1Count + stats.L2Count + stats.L3Count);
    }

    /// <summary>
    /// 测试：验证所有规则都有有效的 ID
    /// </summary>
    [Fact(DisplayName = "所有规则都应有有效的标识符")]
    public void All_Rules_Should_Have_Valid_Ids()
    {
        var options = new ArchitectureRulesOptions();
        var rules = Runner.CentralizedRuleSetRegistry.All(options).ToArray();

        foreach (var rule in rules)
        {
            Assert.NotNull(rule.Id);
            Assert.False(string.IsNullOrWhiteSpace(rule.Id.NewCode), 
                $"规则 {rule.Title} 缺少新编码");
            Assert.False(string.IsNullOrWhiteSpace(rule.Id.Adr), 
                $"规则 {rule.Title} 缺少 ADR 映射");
            Assert.False(string.IsNullOrWhiteSpace(rule.Id.Section), 
                $"规则 {rule.Title} 缺少章节信息");
            
            _output.WriteLine($"✓ {rule.Id}");
        }
    }

    /// <summary>
    /// 测试：验证规则编号的唯一性
    /// </summary>
    [Fact(DisplayName = "规则编号应唯一")]
    public void Rule_Codes_Should_Be_Unique()
    {
        var options = new ArchitectureRulesOptions();
        var rules = Runner.CentralizedRuleSetRegistry.All(options).ToArray();

        var duplicates = rules
            .GroupBy(r => r.Id.NewCode)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        Assert.Empty(duplicates);
    }

    /// <summary>
    /// 测试：验证按层级查询功能
    /// </summary>
    [Theory(DisplayName = "按层级查询规则应正确")]
    [InlineData(RuleLayer.Governance)]
    [InlineData(RuleLayer.Enforcement)]
    [InlineData(RuleLayer.Heuristics)]
    public void Query_Rules_By_Layer_Should_Work(RuleLayer layer)
    {
        var options = new ArchitectureRulesOptions();
        var rules = Runner.CentralizedRuleSetRegistry.GetByLayer(layer, options).ToArray();

        _output.WriteLine($"{layer} 层规则数量: {rules.Length}");
        
        foreach (var rule in rules)
        {
            Assert.Equal(layer, rule.Layer);
            _output.WriteLine($"  - {rule.Id}: {rule.Title}");
        }
    }

    /// <summary>
    /// 测试：验证按严重程度查询功能
    /// </summary>
    [Theory(DisplayName = "按严重程度查询规则应正确")]
    [InlineData(SeverityLevel.L1)]
    [InlineData(SeverityLevel.L2)]
    [InlineData(SeverityLevel.L3)]
    public void Query_Rules_By_Severity_Should_Work(SeverityLevel severity)
    {
        var options = new ArchitectureRulesOptions();
        var rules = Runner.CentralizedRuleSetRegistry.GetBySeverity(severity, options).ToArray();

        _output.WriteLine($"{severity} 级别规则数量: {rules.Length}");
        
        foreach (var rule in rules)
        {
            Assert.Equal(severity, rule.Severity);
            _output.WriteLine($"  - {rule.Id}: {rule.Title}");
        }
    }
}
