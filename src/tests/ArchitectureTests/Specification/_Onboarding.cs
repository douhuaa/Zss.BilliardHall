namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

public static partial class ArchitectureTestSpecification
{
    public static partial class Onboarding
    {
        /// <summary>
        /// Onboarding 文档禁止的内容类型（核心类型）
        /// </summary>
        public static IReadOnlyList<string> ProhibitedContent => [
            "架构约束定义"
        ];

        /// <summary>
        /// Onboarding 文档允许的内容类型
        /// </summary>
        public static IReadOnlyList<string> AllowedContent => [
            "导航指引",
            "学习路径",
            "示例代码",
            "快速入门"
        ];

        /// <summary>
        /// Onboarding 文档的三个核心问题
        /// </summary>
        public static IReadOnlyList<string> CoreQuestions => [
            "我是谁",
            "我先看什么",
            "我下一步去哪"
        ];
    }
}
