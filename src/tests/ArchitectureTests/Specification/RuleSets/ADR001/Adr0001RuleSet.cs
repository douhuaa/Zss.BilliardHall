namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR001;

using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// ADR-001：模块化单体与垂直切片架构
/// 定义模块物理隔离、依赖方向、通信机制等核心规则
/// </summary>
public static class Adr0001RuleSet
{
    /// <summary>
    /// ADR 编号
    /// </summary>
    public const int AdrNumber = 1;

    /// <summary>
    /// 获取完整的规则集定义
    /// </summary>
    public static ArchitectureRuleSet RuleSet => LazyRuleSet.Value;

    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(AdrNumber);

        // Rule 1: 模块物理隔离
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "模块物理隔离",
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "模块按业务能力独立划分",
            enforcement: "通过 NetArchTest 验证模块不相互引用");

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "项目文件禁止引用其他模块",
            enforcement: "解析 .csproj 文件验证无 ProjectReference 指向其他模块");

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 3,
            condition: "命名空间匹配模块边界",
            enforcement: "验证类型命名空间与模块名称一致");

        // Rule 2: 垂直切片组织
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "垂直切片组织",
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "每个模块包含完整的垂直切片",
            enforcement: "验证模块包含 Domain、Application、Infrastructure 层次");

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 2,
            condition: "禁止跨模块水平分层",
            enforcement: "验证无跨模块的 Domain/Application 层依赖");

        // Rule 3: 模块通信机制
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "模块通信机制",
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Module);

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "模块间仅通过领域事件异步通信",
            enforcement: "验证无直接方法调用，仅事件发布/订阅");

        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 2,
            condition: "模块间查询仅通过数据契约",
            enforcement: "验证查询使用只读 DTO，无领域对象传递");

        return ruleSet;
    });
}
