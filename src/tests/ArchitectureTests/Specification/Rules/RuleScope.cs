namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// 规则作用域
/// 定义规则适用的范围，用于确定检查边界
/// </summary>
public enum RuleScope
{
    /// <summary>
    /// 解决方案级别
    /// 规则适用于整个解决方案
    /// 例如：全局命名规范、中央包管理
    /// </summary>
    Solution,

    /// <summary>
    /// 模块级别
    /// 规则适用于单个模块内部
    /// 例如：模块内部的命名约定、依赖规则
    /// </summary>
    Module,

    /// <summary>
    /// 文档级别
    /// 规则适用于文档系统
    /// 例如：ADR 格式要求、文档质量标准
    /// </summary>
    Document,

    /// <summary>
    /// 测试级别
    /// 规则适用于测试代码
    /// 例如：测试命名规范、测试组织结构
    /// </summary>
    Test,

    /// <summary>
    /// Agent 级别
    /// 规则适用于 Copilot Agent 行为
    /// 例如：Agent 权限约束、输出格式要求
    /// </summary>
    Agent
}
