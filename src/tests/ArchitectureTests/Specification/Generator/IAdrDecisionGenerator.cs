namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator;

/// <summary>
/// ADR Decision 生成器接口
///
/// 本接口定义 ADR 裁决内容的**权威输出语义**。
/// 不限定具体呈现格式（Markdown 仅为当前实现）。
/// </summary>
public interface IAdrDecisionGenerator
{
    /// <summary>
    /// 从 RuleSet 生成 Markdown 格式的 Decision 章节
    /// </summary>
    /// <param name="ruleSet">架构规则集</param>
    /// <returns>Markdown 格式的 Decision 章节内容</returns>
    /// <exception cref="ArgumentNullException">当 ruleSet 为 null 时抛出</exception>
    string GenerateDecisionSection(ArchitectureRuleSet ruleSet);

    /// <summary>
    /// 从 RuleSet 生成 Markdown 格式的 Decision 章节（带选项）
    /// </summary>
    /// <param name="ruleSet">架构规则集</param>
    /// <param name="options">生成选项</param>
    /// <returns>Markdown 格式的 Decision 章节内容</returns>
    /// <exception cref="ArgumentNullException">当 ruleSet 或 options 为 null 时抛出</exception>
    string GenerateDecisionSection(ArchitectureRuleSet ruleSet, DecisionGenerationOptions options);
}
