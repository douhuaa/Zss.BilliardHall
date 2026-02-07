namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// 条款执行类型
/// 定义条款的执行策略，用于确定如何验证和执行规则
///
/// 这是将条款从"纯文本描述"向"可执行规范"演进的关键步骤
/// 为未来的自动化测试生成和执行策略提供结构化基础
/// </summary>
public enum ClauseExecutionType
{
    /// <summary>
    /// 静态分析
    /// 通过编译时分析验证（如 Roslyn Analyzer）
    /// 适用于代码结构、命名规范等可在编译时检查的规则
    /// </summary>
    StaticAnalysis,

    /// <summary>
    /// 约定检查
    /// 通过架构测试验证约定和规范
    /// 适用于需要反射、类型检查等运行时验证的规则
    /// </summary>
    Convention,

    /// <summary>
    /// 运行时验证
    /// 通过运行时行为验证
    /// 适用于需要实际执行才能验证的规则
    /// </summary>
    Runtime,

    /// <summary>
    /// 文档验证
    /// 通过文档格式、内容、链接等验证
    /// 适用于文档质量、ADR 格式等规则
    /// </summary>
    Documentation,

    /// <summary>
    /// 手工审查
    /// 需要人工判断的规则
    /// 适用于主观性强、难以自动化的规则
    /// </summary>
    ManualReview
}
