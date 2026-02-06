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
        /// 2. 使用词边界识别和上下文分析进行精确匹配
        /// 3. 排除否定上下文（如"应该避免"、"不应该"等）
        /// 4. 一旦匹配到关键词，立即返回对应的 DecisionResult
        /// 5. 如果未匹配到任何关键词，返回 DecisionResult.None
        /// 
        /// 设计原则：
        /// - 未识别 ≠ Must（防止误伤）
        /// - 默认为非裁决、不可阻断
        /// - 词边界识别避免误判（如"需要性"不匹配"需要"）
        /// - 否定上下文排除避免误判（如"应该避免"不匹配"应该"）
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
                if (rule.Keywords.Any(keyword => IsKeywordMatch(sentence, keyword)))
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
        /// 检查关键词是否在句子中匹配（带词边界和上下文分析）
        /// </summary>
        /// <param name="sentence">要检查的句子</param>
        /// <param name="keyword">关键词</param>
        /// <returns>true 如果关键词在有效上下文中出现</returns>
        private static bool IsKeywordMatch(string sentence, string keyword)
        {
            // 找到关键词的所有位置
            var index = 0;
            while ((index = sentence.IndexOf(keyword, index, StringComparison.Ordinal)) != -1)
            {
                // 检查词边界
                if (!IsWordBoundary(sentence, index, keyword.Length))
                {
                    index += keyword.Length;
                    continue;
                }

                // 检查否定上下文
                if (IsNegativeContext(sentence, index, keyword))
                {
                    index += keyword.Length;
                    continue;
                }

                // 找到有效匹配
                return true;
            }

            // 没有找到有效匹配
            return false;
        }

        /// <summary>
        /// 检查关键词位置是否符合词边界规则
        /// 对于中文，词边界的定义相对宽松，主要排除明显的复合词情况
        /// </summary>
        /// <param name="sentence">句子</param>
        /// <param name="startIndex">关键词起始位置</param>
        /// <param name="keywordLength">关键词长度</param>
        /// <returns>true 如果符合词边界规则</returns>
        private static bool IsWordBoundary(string sentence, int startIndex, int keywordLength)
        {
            var endIndex = startIndex + keywordLength;

            // 检查后面是否跟着明显的复合词后缀
            // 例如："需要性"、"禁止者" 中的 "需要"、"禁止" 应该被排除
            if (endIndex < sentence.Length)
            {
                var nextChar = sentence[endIndex];
                // "性"、"者"、"度"等后缀通常表示复合词
                if (nextChar is '性' or '者' or '度')
                {
                    return false;
                }
            }

            // 检查前面是否有特定前缀构成复合词
            // 例如："可需要性" 中的 "需要" 应该被排除
            if (startIndex > 0)
            {
                var prevChar = sentence[startIndex - 1];
                // "可"字前缀通常表示修饰关系，也可能构成复合词
                if (prevChar == '可')
                {
                    // 如果前面是"可"，且后面跟着"性"等后缀，则确定是复合词
                    if (endIndex < sentence.Length && sentence[endIndex] == '性')
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 检查关键词是否处于否定上下文中
        /// 否定上下文包括：
        /// - "不" + 关键词（如"不应该"）
        /// - 关键词 + "避免"（如"应该避免"）
        /// - 关键词 + "不要"（如"应该不要"）
        /// </summary>
        /// <param name="sentence">句子</param>
        /// <param name="keywordIndex">关键词起始位置</param>
        /// <param name="keyword">关键词</param>
        /// <returns>true 如果关键词处于否定上下文</returns>
        private static bool IsNegativeContext(string sentence, int keywordIndex, string keyword)
        {
            // 检查前面是否有否定词
            if (keywordIndex > 0)
            {
                var prevChar = sentence[keywordIndex - 1];
                if (prevChar == '不')
                {
                    // "不应该"、"不需要" 等都是否定上下文
                    return true;
                }
            }

            // 检查后面是否跟着否定词
            var keywordEnd = keywordIndex + keyword.Length;
            if (keywordEnd >= sentence.Length)
            {
                return false;
            }

            var remainingSpan = sentence.AsSpan(keywordEnd);

            // 检查是否跟着"避免"、"不要"等否定词（使用 Ordinal 比较，避免多余分配）
            if (remainingSpan.StartsWith("避免".AsSpan(), StringComparison.Ordinal) ||
                remainingSpan.StartsWith("不要".AsSpan(), StringComparison.Ordinal))
            {
                return true;
            }

            return false;
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
