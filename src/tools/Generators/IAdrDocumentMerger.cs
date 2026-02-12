using Zss.BilliardHall.Specification.Rules;

namespace Zss.BilliardHall.Generators;

/// <summary>
/// ADR 文档合并器接口
/// 负责将生成的 Decision 章节与现有 ADR 文档的其他章节合并
/// </summary>
public interface IAdrDocumentMerger
{
    /// <summary>
    /// 合并生成的 Decision 章节与现有 ADR 文档
    /// 保留 Front Matter、Context、Consequences 等章节
    /// </summary>
    /// <param name="existingAdrContent">现有 ADR 文档内容</param>
    /// <param name="ruleSet">架构规则集</param>
    /// <param name="options">生成选项</param>
    /// <returns>合并后的完整 ADR 文档内容</returns>
    string MergeDecisionSection(string existingAdrContent, ArchitectureRuleSet ruleSet, DecisionGenerationOptions? options = null);

    /// <summary>
    /// 合并生成的 Decision 章节与现有 ADR 文档
    /// </summary>
    /// <param name="existingAdrContent">现有 ADR 文档内容</param>
    /// <param name="newDecisionContent">新生成的 Decision 章节内容</param>
    /// <returns>合并后的完整 ADR 文档内容</returns>
    string MergeDecisionSection(string existingAdrContent, string newDecisionContent);
}
