namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

using Zss.BilliardHall.Tests.ArchitectureTests.Specification.DecisionLanguage;

/// <summary>
/// DecisionLanguage 定义
/// 这是架构治理的"裁决语法层"，定义可被机器理解的裁决语言模型
/// 
/// 设计目标：
/// 1. 从"关键词匹配"升级到"规范语言模型"
/// 2. 支持三态输出（Allowed / Blocked / Warning）
/// 3. 与 ADR-905 执行级别分类对齐
/// 4. 为 ArchitectureTests、Analyzer、Agent 提供统一的裁决语义解析
/// </summary>
public static partial class ArchitectureTestSpecification
{
    /// <summary>
    /// 裁决语言模型
    /// 定义可被机器理解和执行的架构裁决规则
    /// </summary>
    public static class DecisionLanguage
    {
        /// <summary>
        /// 裁决规则集
        /// 定义所有支持的裁决语言及其语义
        /// </summary>
        public static IReadOnlyList<DecisionRule> Rules { get; } =
        [
            // MUST: 强制性要求，阻断级别
            new DecisionRule(
                Level: DecisionLevel.Must,
                Keywords: ["必须", "强制", "需要"],
                IsBlocking: true
            ),

            // MUST NOT: 明确禁止，阻断级别
            new DecisionRule(
                Level: DecisionLevel.MustNot,
                Keywords: ["禁止", "不得", "不允许"],
                IsBlocking: true
            ),

            // SHOULD: 推荐建议，警告级别
            new DecisionRule(
                Level: DecisionLevel.Should,
                Keywords: ["应该", "建议", "推荐"],
                IsBlocking: false
            )
        ];

        /// <summary>
        /// 从自然语言文本中解析裁决语义
        /// 
        /// 解析规则：
        /// 1. 按照 Rules 定义的顺序依次匹配
        /// 2. 一旦匹配到关键词，立即返回对应的 DecisionResult
        /// 3. 如果未匹配到任何关键词，返回 DecisionResult.None
        /// 
        /// 设计原则：
        /// - 未识别 ≠ Must（防止误伤）
        /// - 默认为非裁决、不可阻断
        /// </summary>
        /// <param name="sentence">要解析的文本</param>
        /// <returns>解析出的裁决结果</returns>
        public static DecisionResult Parse(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
            {
                return DecisionResult.None;
            }

            foreach (var rule in Rules)
            {
                if (rule.Keywords.Any(keyword => sentence.Contains(keyword)))
                {
                    return new DecisionResult(
                        rule.Level,
                        rule.IsBlocking
                    );
                }
            }

            return DecisionResult.None;
        }

        /// <summary>
        /// 判断给定文本是否包含阻断级别的裁决语言
        /// </summary>
        /// <param name="sentence">要检查的文本</param>
        /// <returns>true 如果包含阻断级别的裁决语言</returns>
        public static bool IsBlockingDecision(string sentence)
        {
            var result = Parse(sentence);
            return result.IsDecision && result.IsBlocking;
        }

        /// <summary>
        /// 判断给定文本是否包含任何裁决语言（包括非阻断级别）
        /// </summary>
        /// <param name="sentence">要检查的文本</param>
        /// <returns>true 如果包含任何裁决语言</returns>
        public static bool HasDecisionLanguage(string sentence)
        {
            return Parse(sentence).IsDecision;
        }
    }
}
