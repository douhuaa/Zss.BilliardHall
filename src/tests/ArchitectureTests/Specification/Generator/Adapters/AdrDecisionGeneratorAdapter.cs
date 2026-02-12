using Zss.BilliardHall.Generators;
using GenOptions = Zss.BilliardHall.Generators.DecisionGenerationOptions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Adapters;

/// <summary>
/// ADR Decision 生成器适配器
/// 将测试中的 ArchitectureRuleSet 适配到新生成器的 IRuleSet 接口
/// 
/// 此适配器保持与现有测试的兼容性，使测试能够透明地使用新的生成器实现
/// </summary>
public sealed class AdrDecisionGeneratorAdapter : IAdrDecisionGenerator
{
    private readonly Generators.AdrDecisionGenerator _generator = new();

    /// <summary>
    /// 从 RuleSet 生成 Markdown 格式的 Decision 章节
    /// </summary>
    public string GenerateDecisionSection(ArchitectureRuleSet ruleSet)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        var adapted = new RuleSetAdapter(ruleSet);
        var result = _generator.GenerateDecisionSection(adapted);
        return result.Content;
    }

    /// <summary>
    /// 从 RuleSet 生成 Markdown 格式的 Decision 章节（带选项）
    /// </summary>
    public string GenerateDecisionSection(ArchitectureRuleSet ruleSet, DecisionGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        ArgumentNullException.ThrowIfNull(options);
        
        var adapted = new RuleSetAdapter(ruleSet);
        
        // 转换选项对象
        var genOptions = new GenOptions
        {
            IncludeSectionHeader = options.IncludeSectionHeader,
            IncludeWarningNote = options.IncludeWarningNote,
            HeaderLevelOffset = options.HeaderLevelOffset,
            AddBlankLinesBetweenClauses = options.AddBlankLinesBetweenClauses,
            EscapeMarkdown = options.EscapeMarkdown
        };
        
        var result = _generator.GenerateDecisionSection(adapted, genOptions);
        return result.Content;
    }
}
