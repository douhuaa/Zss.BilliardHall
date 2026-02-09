namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// 输出规范定义
/// 定义 Agent 和测试的标准输出格式，特别是三态输出模型
/// </summary>
public static partial class ArchitectureTestSpecification
{
    public static partial class Output
    {
        /// <summary>
        /// 三态输出模型
        /// 定义 Allowed、Blocked、Uncertain 三种判定状态
        /// </summary>
        public static class States
        {
            /// <summary>
            /// 允许状态：ADR 正文明确允许且经测试验证
            /// </summary>
            public static string Allowed => "✅ Allowed";

            /// <summary>
            /// 阻止状态：ADR 正文明确禁止或导致测试失败
            /// </summary>
            public static string Blocked => "⚠️ Blocked";

            /// <summary>
            /// 不确定状态：ADR 正文未明确，默认禁止
            /// </summary>
            public static string Uncertain => "❓ Uncertain";

            /// <summary>
            /// 三态输出的完整标识列表
            /// </summary>
            public static IReadOnlyList<string> FullIndicators =>
            [
                Allowed, Blocked, Uncertain
            ];

            /// <summary>
            /// 三态输出的简写形式
            /// </summary>
            public static IReadOnlyList<string> ShortForms =>
            [
                "Allowed", "Blocked", "Uncertain"
            ];

            /// <summary>
            /// 三态输出的 Emoji 标识
            /// </summary>
            public static IReadOnlyList<string> Emojis =>
            [
                "✅", "⚠️", "❓"
            ];
        }
    }
}
