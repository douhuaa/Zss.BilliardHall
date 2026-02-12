namespace Zss.BilliardHall.Specification;

public static partial class ArchitectureTestSpecification
{
    public static partial class Semantics
    {
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
