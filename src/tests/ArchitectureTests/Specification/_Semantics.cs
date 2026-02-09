namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

public static partial class ArchitectureTestSpecification
{
    public static partial class Semantics
    {
        /// <summary>
        /// 裁决性关键词列表
        /// 这些词不应该出现在非裁决性文档（如 Onboarding）中作为新规则定义
        /// </summary>
        [Obsolete("请使用 ArchitectureTestSpecification.DecisionLanguage 代替。" + "DecisionKeywords 已被升级为更强大的 DecisionLanguage 语义模型，" + "支持裁决级别（Must/MustNot/Should）和三态输出。")]
        public static IReadOnlyList<string> DecisionKeywords { get; } = [
            "必须", "禁止", "不得", "强制", "不允许"
        ];

        /// <summary>
        /// 关键语义块标题（必须是 ## 级别且唯一）
        /// ADR 文档中的核心结构性标题
        /// </summary>
        public static IReadOnlyList<string> RequiredHeadings { get; } = [
            "Relationships",
            "Decision",
            "Enforcement",
            "Glossary"
        ];
    }
}
