using Zss.BilliardHall.Specification.Rules;

namespace Zss.BilliardHall.Generators;

/// <summary>
/// Agent 指令生成器接口
///
/// 本接口定义从 RuleSet 生成 Agent Instructions YAML 的契约。
/// 生成的指令符合 .github/INSTRUCTIONS-SCHEMA.md 规范。
/// </summary>
public interface IAgentInstructionGenerator
{
    /// <summary>
    /// 从 RuleSet 生成 YAML 格式的 Agent Instructions
    /// </summary>
    /// <param name="ruleSet">架构规则集</param>
    /// <returns>YAML 格式的 Agent Instructions 内容</returns>
    /// <exception cref="ArgumentNullException">当 ruleSet 为 null 时抛出</exception>
    string GenerateInstructions(ArchitectureRuleSet ruleSet);

    /// <summary>
    /// 从 RuleSet 生成 YAML 格式的 Agent Instructions（带选项）
    /// </summary>
    /// <param name="ruleSet">架构规则集</param>
    /// <param name="options">生成选项</param>
    /// <returns>YAML 格式的 Agent Instructions 内容</returns>
    /// <exception cref="ArgumentNullException">当 ruleSet 或 options 为 null 时抛出</exception>
    string GenerateInstructions(ArchitectureRuleSet ruleSet, InstructionGenerationOptions options);
}
