# 强类型架构规则 ID 系统

## 概述

这个目录包含了强类型的架构规则ID系统，用于消除基于字符串的规则引用带来的不确定性。

## 为什么需要强类型？

### 旧方式的问题（字符串）

```csharp
// ❌ 问题1: ADR-907.3 到底是 Rule 还是 Clause？
var ruleId = "ADR-907.3";

// ❌ 问题2: 907.3 和 907.03 是否等价？
var ruleId1 = "ADR-907.3";
var ruleId2 = "ADR-907.03";

// ❌ 问题3: 907.3 是否真的存在？
var ruleId = "ADR-907.99";  // 编译通过，运行时才发现问题
```

### 新方式的优势（强类型）

```csharp
// ✅ 明确是 Rule 级别
var rule = ArchitectureRuleId.Rule(907, 3);

// ✅ 明确是 Clause 级别
var clause = ArchitectureRuleId.Clause(907, 3, 2);

// ✅ 不可能构造出不合法的格式
// ✅ ToString() 保证规范格式: "ADR-0907.3" 或 "ADR-0907.3.2"
// ✅ 可比较、可排序
// ✅ 类型安全
```

## 核心类型

### 1. `ArchitectureRuleId`

强类型的规则ID，是整个治理体系的最小不可再分单元。

```csharp
// 创建 Rule 级别的ID
var rule = ArchitectureRuleId.Rule(907, 3);
rule.ToString(); // "ADR-0907.3"

// 创建 Clause 级别的ID
var clause = ArchitectureRuleId.Clause(907, 3, 2);
clause.ToString(); // "ADR-0907.3.2"

// 获取层级
rule.Level;    // RuleLevel.Rule
clause.Level;  // RuleLevel.Clause

// 排序
var sorted = new[] { clause, rule }.OrderBy(x => x);
// 结果: [rule, clause] - Rule 总是在同编号的 Clause 之前
```

### 2. `RuleLevel`

规则层级枚举，显式定义裁决粒度。

```csharp
public enum RuleLevel
{
    Rule,    // ADR-XXXX.Y 格式
    Clause   // ADR-XXXX.Y.Z 格式
}
```

### 3. `RuleSeverity`

规则严重程度，决定违规的处理策略。

```csharp
public enum RuleSeverity
{
    Constitutional,  // 宪法级 - 违反即阻断
    Governance,      // 治理级 - PR 阻断
    Technical        // 技术级 - 架构警告
}
```

### 4. `RuleScope`

规则作用域，定义规则适用的范围。

```csharp
public enum RuleScope
{
    Solution,  // 解决方案级别
    Module,    // 模块级别
    Document,  // 文档级别
    Test,      // 测试级别
    Agent      // Agent 级别
}
```

### 5. `DecisionLevel`（裁决级别）

定义规则的裁决强度和执行方式，与 ADR-905 对齐。

```csharp
public enum DecisionLevel
{
    Must,     // 必须 - 强制性要求，违反将阻断 CI
    MustNot,  // 禁止 - 明确禁止，违反将阻断 CI
    Should    // 应该 - 推荐建议，违反仅产生警告
}
```

### 6. `ClauseExecutionType`（执行类型）

定义条款的执行策略，用于确定如何验证和执行规则。

```csharp
public enum ClauseExecutionType
{
    StaticAnalysis,  // 静态分析（通过 Roslyn Analyzer）
    Convention,      // 约定检查（通过架构测试）
    Runtime,         // 运行时验证
    Documentation,   // 文档验证
    ManualReview     // 手工审查
}
```

### 7. `ArchitectureRuleDefinition`

规则定义（Rule 级别），包含裁决语义。

```csharp
var rule = new ArchitectureRuleDefinition(
    Id: ArchitectureRuleId.Rule(907, 3),
    Summary: "最小断言语义规范",
    Decision: DecisionLevel.Must,        // 裁决级别
    Severity: RuleSeverity.Governance,
    Scope: RuleScope.Test
);
```

### 8. `ArchitectureClauseDefinition`

条款定义（Clause 级别），包含执行策略。

```csharp
var clause = new ArchitectureClauseDefinition(
    Id: ArchitectureRuleId.Clause(907, 3, 1),
    Condition: "每个测试类至少包含1个有效断言",
    Enforcement: "通过静态分析验证断言数量",
    ExecutionType: ClauseExecutionType.StaticAnalysis  // 执行类型
);
```

### 9. `ArchitectureRuleSet`

规则集管理类，将 ADR 转换为可执行规范。

```csharp
var ruleSet = new ArchitectureRuleSet(907);

// 添加规则（包含裁决级别）
ruleSet.AddRule(
    ruleNumber: 3,
    summary: "最小断言语义规范", 
    decision: DecisionLevel.Must,       // 裁决级别
    severity: RuleSeverity.Governance,
    scope: RuleScope.Test);

// 添加条款（包含执行类型）
ruleSet.AddClause(
    ruleNumber: 3,
    clauseNumber: 1, 
    condition: "每个测试类至少包含1个有效断言",
    enforcement: "通过静态分析验证",
    executionType: ClauseExecutionType.StaticAnalysis);  // 执行类型

// 验证完整性（确保每个规则都有条款）
ruleSet.ValidateCompleteness();

// 查询
var rule = ruleSet.GetRule(3);
var clause = ruleSet.GetClause(3, 1);
var hasRule = ruleSet.HasRule(3);
```

## 使用示例

查看 `ArchitectureRulesExample.cs` 文件获取完整的使用示例。

### 在测试中使用

```csharp
[Fact(DisplayName = "ADR-907_3_1: 测试类必须包含至少一个有效断言")]
public void ADR_907_3_1_Test_Classes_Must_Have_Minimum_Assertions()
{
    // 创建强类型的规则ID
    var ruleId = ArchitectureRuleId.Clause(907, 3, 1);
    
    // ... 执行验证逻辑
    
    // 使用规则ID生成清晰的失败消息
    violations.Should().BeEmpty(
        $"❌ {ruleId} 违规：测试类必须包含至少一个有效断言\n\n" +
        $"修复建议：\n" +
        $"  1. 每个测试类必须包含至少 1 个有效断言\n" +
        $"  2. 断言必须验证架构约束，不是形式化断言\n\n" +
        $"参考：docs/adr/governance/ADR-907-...");
}
```

## 未来扩展

基于这个强类型系统，可以实现：

1. **自动测试生成**: 基于 RuleSet 自动生成测试方法骨架
2. **文档同步**: RuleSet ↔ Markdown 双向同步
3. **强类型 DSL**: `ArchitectureRules.Adr907.Rule3.Clause1`
4. **规则验证**: 编译时验证规则引用的有效性
5. **JSON 序列化**: 支持规则的结构化存储和传输

## 测试

运行单元测试：

```bash
dotnet test --filter "FullyQualifiedName~ArchitectureRuleIdTests|FullyQualifiedName~ArchitectureRuleSetTests"
```

所有测试位于 `../Tests/` 目录下。

## 设计原则

1. **类型安全**: 不可能构造出不合法的规则ID
2. **明确性**: Rule 和 Clause 在类型层面就能区分
3. **可验证**: 编译时就能发现格式错误
4. **可扩展**: 为未来的 DSL 和工具支持做好准备
5. **零重复**: ToString() 是唯一的规范格式来源

## 参考

- 问题陈述: 见任务描述
- ADR-907: `docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md`
