# DecisionLanguage - 裁决语言模型

## 概述

DecisionLanguage 是 ArchitectureTests 的"裁决语法层"，定义架构规则的裁决级别和执行策略。它是整个治理体系的语义基础，用于区分强制性要求（Must/MustNot）和推荐建议（Should）。

## 核心概念

### 1. DecisionLevel（裁决级别）

与 ADR-905 对齐的三个裁决级别：

```csharp
public enum DecisionLevel
{
    /// <summary>
    /// 必须（MUST）
    /// 表示强制性要求，违反将阻断构建/CI
    /// 对应 ADR-905 的 L1 级别执行
    /// </summary>
    Must,

    /// <summary>
    /// 禁止（MUST NOT）
    /// 表示明确禁止的行为，违反将阻断构建/CI
    /// 对应 ADR-905 的 L1 级别执行
    /// </summary>
    MustNot,

    /// <summary>
    /// 应该（SHOULD）
    /// 表示推荐性建议，违反仅产生警告不阻断
    /// 对应 ADR-905 的 L2 级别执行
    /// </summary>
    Should
}
```

### 2. DecisionRule（裁决规则）

定义裁决语言的识别规则和执行策略：

```csharp
public sealed record DecisionRule(
    DecisionLevel Level,           // 裁决级别
    IReadOnlyList<string> Keywords // 触发关键词
)
{
    /// <summary>
    /// 是否具备阻断权力（CI失败）
    /// 根据 ADR-905 规范自动计算：
    /// - Must / MustNot → 阻断（true）
    /// - Should → 非阻断（false）
    /// </summary>
    public bool IsBlocking => Level switch
    {
        DecisionLevel.Must => true,
        DecisionLevel.MustNot => true,
        DecisionLevel.Should => false,
        _ => false
    };
}
```

### 3. DecisionParseResult（解析结果）

表示裁决解析的结果，专注于"解析"职责：

```csharp
public sealed record DecisionParseResult(
    DecisionLevel? Level  // 级别（null 表示未识别到裁决语言）
)
{
    public static readonly DecisionParseResult None;
    public bool HasDecision => Level.HasValue;
}
```

### 4. DecisionExecutionResult（执行结果）

表示裁决的执行策略和行为，专注于"执行"职责：

```csharp
public sealed record DecisionExecutionResult(
    DecisionLevel Level,  // 裁决级别
    bool IsBlocking      // 是否阻断构建/CI
);
```

## 设计原则

1. **类型安全**：使用强类型枚举而非字符串，在编译时防止错误
2. **语义清晰**：Must/MustNot/Should 明确表达约束强度
3. **职责分离**：解析结果和执行策略分离，保持单一职责
4. **ADR 对齐**：完全对齐 ADR-905 执行级别分类标准
5. **自动推导**：IsBlocking 基于 DecisionLevel 自动计算，避免不一致

## 使用场景

### 在 ArchitectureRuleDefinition 中使用

```csharp
var rule = new ArchitectureRuleDefinition(
    Id: ArchitectureRuleId.Rule(907, 3),
    Summary: "最小断言语义规范",
    Decision: DecisionLevel.Must,  // 使用 DecisionLevel
    Severity: RuleSeverity.Governance,
    Scope: RuleScope.Test
);

// 判断是否阻断
bool isBlocking = rule.Decision switch
{
    DecisionLevel.Must => true,
    DecisionLevel.MustNot => true,
    DecisionLevel.Should => false,
    _ => false
};
```

### 在规则集定义中使用

```csharp
public sealed class Adr900RuleSet : IArchitectureRuleSetDefinition
{
    public ArchitectureRuleSet Define()
    {
        var ruleSet = new ArchitectureRuleSet(900);
        
        // 强制性规则 - 阻断 CI
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "架构裁决权威性",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Test);
        
        // 推荐性规则 - 仅警告
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "代码审查建议",
            decision: DecisionLevel.Should,
            severity: RuleSeverity.Technical,
            scope: RuleScope.Module);
            
        return ruleSet;
    }
}
```

### 三态判定逻辑

```csharp
public enum TestResult
{
    /// <summary>
    /// ✅ Allowed - 测试通过或无适用规则
    /// </summary>
    Allowed,
    
    /// <summary>
    /// ⚠️ Warning - 违反 Should 级别规则（不阻断）
    /// </summary>
    Warning,
    
    /// <summary>
    /// ❌ Blocked - 违反 Must/MustNot 级别规则（阻断 CI）
    /// </summary>
    Blocked
}

public TestResult EvaluateRule(ArchitectureRuleDefinition rule, bool isViolated)
{
    if (!isViolated)
        return TestResult.Allowed;
    
    return rule.Decision switch
    {
        DecisionLevel.Must => TestResult.Blocked,
        DecisionLevel.MustNot => TestResult.Blocked,
        DecisionLevel.Should => TestResult.Warning,
        _ => TestResult.Allowed
    };
}
```

## 与其他组件的关系

DecisionLanguage 是架构治理的基础设施，被以下组件使用：

- **ArchitectureRuleDefinition**：定义规则的裁决级别
- **ArchitectureRuleSet**：组织和管理带裁决级别的规则
- **ArchitectureTests**：基于裁决级别决定测试失败的严重性
- **CI/CD**：基于裁决级别决定是否阻断构建

## 参考文档

- [ADR-905: 执行级别分类](../../../../../docs/adr/governance/ADR-905-enforcement-level-classification.md)
- [ADR-907: ArchitectureTests 执法治理体系](../../../../../docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md)
- [ArchitectureRuleDefinition](../../Rules/ArchitectureRuleDefinition.cs)

## 未来扩展

基于当前的语义基础，可以扩展以下功能：

1. **自然语言解析**：从 ADR 文档文本中自动识别裁决级别
2. **多语言支持**：支持英文等其他语言的裁决关键词
3. **语义验证**：验证规则定义与 ADR 文档的一致性
4. **自动分类**：基于规则内容自动推荐裁决级别
5. **Roslyn Analyzer 集成**：将裁决级别映射到 DiagnosticSeverity
