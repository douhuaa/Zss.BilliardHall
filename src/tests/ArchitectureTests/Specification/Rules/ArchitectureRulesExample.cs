using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.DecisionLanguage;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// 使用示例：如何使用强类型 ArchitectureRuleId 系统
/// 
/// 这个类展示了如何在实际测试中使用新的强类型规则系统
/// </summary>
public static class ArchitectureRulesExample
{
    /// <summary>
    /// 示例1：构建 ADR-907 的规则集
    /// 这展示了如何将 ADR 文档转换为可执行的规则定义
    /// </summary>
    public static ArchitectureRuleSet BuildAdr907Rules()
    {
        var ruleSet = new ArchitectureRuleSet(907);

        // 添加 Rule 3: 最小断言语义规范
        ruleSet.AddRule(
            ruleNumber: 3,
            summary: "最小断言语义规范",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Test
        );

        // 添加 Clause 3.1: 最小断言数量要求
        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 1,
            condition: "每个测试类至少包含1个有效断言",
            enforcement: "通过静态分析验证断言数量",
            executionType: ClauseExecutionType.StaticAnalysis
        );

        // 添加 Clause 3.2: 单一子规则映射
        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 2,
            condition: "每个测试方法只能映射一个ADR子规则",
            enforcement: "通过命名模式检查验证",
            executionType: ClauseExecutionType.Convention
        );

        // 添加 Clause 3.3: 失败信息可溯源性
        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 3,
            condition: "所有断言失败信息必须可反向溯源到ADR",
            enforcement: "验证失败消息包含ADR引用、违规标记、修复建议和文档引用",
            executionType: ClauseExecutionType.Convention
        );

        // 添加 Clause 3.4: 禁止形式化断言
        ruleSet.AddClause(
            ruleNumber: 3,
            clauseNumber: 4,
            condition: "明确禁止Assert.True(true)等形式化断言",
            enforcement: "通过模式匹配检测形式化断言",
            executionType: ClauseExecutionType.StaticAnalysis
        );

        return ruleSet;
    }

    /// <summary>
    /// 示例2：在测试中使用强类型 RuleId
    /// </summary>
    public static class UsageInTests
    {
        // 使用强类型方式
        public static void StronglyTyped()
        {
            // 明确是 Rule 级别
            var rule = ArchitectureRuleId.Rule(907, 3);
            
            // 明确是 Clause 级别
            var clause = ArchitectureRuleId.Clause(907, 3, 2);
            
            // 规范的字符串格式
            var ruleStr = rule.ToString();  // "ADR-0907.3"
            var clauseStr = clause.ToString();  // "ADR-0907.3.2"
            
            // 类型安全的比较
            var isRule = rule.Level == RuleLevel.Rule;
            var isClause = clause.Level == RuleLevel.Clause;
        }
    }

    /// <summary>
    /// 示例3：规则查询和验证
    /// </summary>
    public static void QueryAndValidation()
    {
        var ruleSet = BuildAdr907Rules();

        // 检查规则是否存在
        if (ruleSet.HasRule(3))
        {
            var rule = ruleSet.GetRule(3);
            // rule.Id.ToString() => "ADR-0907.3"
            // rule.Summary => "最小断言语义规范"
            // rule.Severity => RuleSeverity.Governance
        }

        // 检查条款是否存在
        if (ruleSet.HasClause(3, 1))
        {
            var clause = ruleSet.GetClause(3, 1);
            // clause.Id.ToString() => "ADR-0907.3.1"
            // clause.Condition => "每个测试类至少包含1个有效断言"
        }

        // 获取所有规则和条款
        var allRules = ruleSet.Rules;
        var allClauses = ruleSet.Clauses;
        
        // 排序（按 ADR、Rule、Clause 顺序）
        var sortedIds = allClauses
            .Select(c => c.Id)
            .OrderBy(id => id)
            .ToList();
    }

    /// <summary>
    /// 示例4：测试断言中的使用
    /// </summary>
    public static class AssertionUsage
    {
        public static void NewAssertion()
        {
            var ruleId = ArchitectureRuleId.Clause(907, 3, 1);
            var violations = new List<string>();
            // ... 收集违规
            
            // 清晰的规则引用
            // violations.Should().BeEmpty(
            //     $"❌ {ruleId} 违规：测试类必须包含至少一个有效断言\n\n" +
            //     $"修复建议：...");
        }
    }

    /// <summary>
    /// 示例5：未来的扩展可能性
    /// </summary>
    public static class FutureExtensions
    {
        // 可以基于 RuleId 进行的未来扩展：
        
        // 1. 自动生成测试方法
        // public static void GenerateTest(ArchitectureRuleId ruleId) { }
        
        // 2. RuleSet -> Markdown 文档生成
        // public static string ToMarkdown(ArchitectureRuleSet ruleSet) { }
        
        // 3. RuleSet -> JSON 序列化
        // public static string ToJson(ArchitectureRuleSet ruleSet) { }
        
        // 4. 强类型的 DSL 入口
        // public static class ArchitectureRules
        // {
        //     public static class Adr907
        //     {
        //         public static class Rule3
        //         {
        //             public static ArchitectureRuleId Id => ArchitectureRuleId.Rule(907, 3);
        //             public static class Clause1 { ... }
        //             public static class Clause2 { ... }
        //         }
        //     }
        // }
    }
}
