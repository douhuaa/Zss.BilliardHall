namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// 输出规范定义
/// 定义 Agent 和测试的标准输出格式，特别是三态输出模型
/// </summary>
public sealed class _Output
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static readonly _Output Instance = new();

    /// <summary>
    /// 三态输出规范（通过实例访问）
    /// Agent 响应和测试结果必须使用这些标识之一
    /// </summary>
    public StatesSpec States => StatesSpec.Instance;

    private _Output() { }

    /// <summary>
    /// 三态输出模型
    /// 定义 Allowed、Blocked、Uncertain 三种判定状态
    /// </summary>
    public sealed class StatesSpec
    {
        public static readonly StatesSpec Instance = new();
        private StatesSpec() { }

        /// <summary>
        /// 允许状态：ADR 正文明确允许且经测试验证
        /// </summary>
        public string Allowed => "✅ Allowed";

        /// <summary>
        /// 阻止状态：ADR 正文明确禁止或导致测试失败
        /// </summary>
        public string Blocked => "⚠️ Blocked";

        /// <summary>
        /// 不确定状态：ADR 正文未明确，默认禁止
        /// </summary>
        public string Uncertain => "❓ Uncertain";

        /// <summary>
        /// 三态输出的完整标识列表
        /// </summary>
        public IReadOnlyList<string> FullIndicators =>
        [
            Allowed, Blocked, Uncertain
        ];

        /// <summary>
        /// 三态输出的简写形式
        /// </summary>
        public IReadOnlyList<string> ShortForms =>
        [
            "Allowed", "Blocked", "Uncertain"
        ];

        /// <summary>
        /// 三态输出的 Emoji 标识
        /// </summary>
        public IReadOnlyList<string> Emojis =>
        [
            "✅", "⚠️", "❓"
        ];
    }
}
