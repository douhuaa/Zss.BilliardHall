namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// Onboarding 内容规范定义
/// 定义 Onboarding 文档允许和禁止的内容类型及核心原则
/// </summary>
public sealed class _Onboarding
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static readonly _Onboarding Instance = new();

    /// <summary>
    /// Onboarding 文档禁止的内容类型（核心类型）
    /// </summary>
    public IReadOnlyList<string> ProhibitedContent =>
    [
        "架构约束定义"
    ];

    /// <summary>
    /// Onboarding 文档允许的内容类型
    /// </summary>
    public IReadOnlyList<string> AllowedContent =>
    [
        "导航指引",
        "学习路径",
        "示例代码",
        "快速入门"
    ];

    /// <summary>
    /// Onboarding 文档的三个核心问题
    /// </summary>
    public IReadOnlyList<string> CoreQuestions =>
    [
        "我是谁",
        "我先看什么",
        "我下一步去哪"
    ];

    private _Onboarding() { }
}
