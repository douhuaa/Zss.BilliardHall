namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator;

/// <summary>
/// ADR Decision 生成器（默认实现）
/// 使用 Zss.BilliardHall.Generators 项目中的生成器
/// </summary>
public sealed class AdrDecisionGenerator : IAdrDecisionGenerator
{
    private readonly Adapters.AdrDecisionGeneratorAdapter _adapter = new();

    /// <summary>
    /// 从 RuleSet 生成 Markdown 格式的 Decision 章节
    /// </summary>
    public string GenerateDecisionSection(ArchitectureRuleSet ruleSet)
    {
        return _adapter.GenerateDecisionSection(ruleSet);
    }

    /// <summary>
    /// 从 RuleSet 生成 Markdown 格式的 Decision 章节（带选项）
    /// </summary>
    public string GenerateDecisionSection(ArchitectureRuleSet ruleSet, DecisionGenerationOptions options)
    {
        return _adapter.GenerateDecisionSection(ruleSet, options);
    }
}
