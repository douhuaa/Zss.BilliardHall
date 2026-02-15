# RuleSet API 使用指南

**版本**: 1.0  
**状态**: ✅ Active  
**最后更新**: 2026-02-15

---

## 概述

本指南介绍如何使用 `RuleSetRegistry` API 查询和访问架构规则。RuleSet API 是架构治理的唯一真相源（Single Source of Truth），所有 Agent 和 Skills 都应使用此 API 而非硬编码或解析 Markdown 文档。

---

## 核心概念

### RuleSet（规则集）

每个 ADR 对应一个 RuleSet，包含该 ADR 中定义的所有 Rule 和 Clause。

```csharp
public sealed class ArchitectureRuleSet
{
    public int AdrNumber { get; }
    public IReadOnlyCollection<ArchitectureRuleDefinition> Rules { get; }
    public IReadOnlyCollection<ArchitectureClauseDefinition> Clauses { get; }
}
```

### Rule（规则）

规则是架构约束的高层次描述。

```csharp
public sealed class ArchitectureRuleDefinition
{
    public ArchitectureRuleId Id { get; }       // 如 "ADR-001_1"
    public string Summary { get; }               // 规则摘要
    public DecisionLevel Decision { get; }       // MUST/MUST_NOT/SHOULD
    public RuleSeverity Severity { get; }        // Constitutional/Governance/Technical
    public RuleScope Scope { get; }              // Module/Solution/Test/Document
}
```

### Clause（条款）

条款是规则的具体执行要求。

```csharp
public sealed class ArchitectureClauseDefinition
{
    public ArchitectureRuleId Id { get; }        // 如 "ADR-001_1_1"
    public string Condition { get; }             // 条件描述
    public string Enforcement { get; }           // 执行要求
    public ClauseExecutionType ExecutionType { get; } // Convention/StaticAnalysis/Runtime/Documentation/ManualReview
}
```

---

## 基本用法

### 1. 获取规则集

#### 宽容模式（探索性查询）

```csharp
// 返回 null 如果规则集不存在
var ruleSet = RuleSetRegistry.Get(1);  // ADR-001
if (ruleSet != null)
{
    // 处理规则集
}
```

#### 严格模式（架构验证，推荐）

```csharp
// 抛出异常如果规则集不存在
var ruleSet = RuleSetRegistry.GetStrict(1);  // ADR-001

// 也支持字符串格式
var ruleSet = RuleSetRegistry.GetStrict("ADR-001");  // 等价于上面
```

### 2. 查询规则和条款

```csharp
// 获取规则集
var ruleSet = RuleSetRegistry.GetStrict(1);

// 获取特定规则
var rule1 = ruleSet.GetRule(1);
// rule1.Summary: "模块物理隔离"
// rule1.Decision: DecisionLevel.MustNot
// rule1.Severity: RuleSeverity.Constitutional

// 获取特定条款
var clause1_1 = ruleSet.GetClause(1, 1);
// clause1_1.Condition: "模块按业务能力独立划分"
// clause1_1.Enforcement: "通过 NetArchTest 验证模块不相互引用"
// clause1_1.ExecutionType: ClauseExecutionType.Convention

// 检查规则/条款是否存在
bool hasRule = ruleSet.HasRule(1);
bool hasClause = ruleSet.HasClause(1, 1);
```

### 3. 遍历规则和条款

```csharp
var ruleSet = RuleSetRegistry.GetStrict(5);  // ADR-005

// 遍历所有规则
foreach (var rule in ruleSet.Rules)
{
    Console.WriteLine($"Rule {rule.Id.RuleNumber}: {rule.Summary}");
    Console.WriteLine($"  Decision: {rule.Decision}");
    Console.WriteLine($"  Severity: {rule.Severity}");
    
    // 获取该规则的所有条款
    var clauses = ruleSet.Clauses
        .Where(c => c.Id.RuleNumber == rule.Id.RuleNumber);
    
    foreach (var clause in clauses)
    {
        Console.WriteLine($"  Clause {clause.Id.ClauseNumber}:");
        Console.WriteLine($"    Condition: {clause.Condition}");
        Console.WriteLine($"    Enforcement: {clause.Enforcement}");
    }
}
```

---

## 高级查询

### 按严重程度查询

```csharp
// 查询所有宪法层规则（最高优先级）
var constitutionalRuleSets = RuleSetRegistry.GetBySeverity(RuleSeverity.Constitutional);

// 查询治理层规则
var governanceRuleSets = RuleSetRegistry.GetBySeverity(RuleSeverity.Governance);

// 查询技术层规则
var technicalRuleSets = RuleSetRegistry.GetBySeverity(RuleSeverity.Technical);
```

### 按作用域查询

```csharp
// 查询模块级规则
var moduleRuleSets = RuleSetRegistry.GetByScope(RuleScope.Module);

// 查询解决方案级规则
var solutionRuleSets = RuleSetRegistry.GetByScope(RuleScope.Solution);

// 查询测试相关规则
var testRuleSets = RuleSetRegistry.GetByScope(RuleScope.Test);
```

### 按 ADR 层级快速查询

```csharp
// 获取宪法层 ADR (001-008)
var constitutionalRuleSets = RuleSetRegistry.GetConstitutionalRuleSets();

// 获取治理层 ADR (900-999)
var governanceRuleSets = RuleSetRegistry.GetGovernanceRuleSets();

// 获取运行时 ADR (201-240)
var runtimeRuleSets = RuleSetRegistry.GetRuntimeRuleSets();

// 获取结构层 ADR (120-124)
var structureRuleSets = RuleSetRegistry.GetStructureRuleSets();
```

### 获取所有规则集

```csharp
// 获取所有已注册的 ADR 编号
var adrNumbers = RuleSetRegistry.GetAllAdrNumbers();  // 返回 IEnumerable<int>

// 获取所有规则集
var allRuleSets = RuleSetRegistry.GetAllRuleSets();  // 返回 IEnumerable<ArchitectureRuleSet>

// 检查规则集是否存在
bool exists = RuleSetRegistry.Contains(1);  // ADR-001
```

---

## 常见使用场景

### 场景 1: 代码生成器验证规则

```csharp
public void GenerateHandler(string handlerType, string returnType)
{
    // 获取 Handler 规则集
    var handlerRules = RuleSetRegistry.GetStrict(5);  // ADR-005
    
    // 查询 CQRS 分离约束
    var clause5_1 = handlerRules.GetClause(5, 1);
    
    // 验证返回类型
    if (handlerType == "Command" && !IsSimpleReturnType(returnType))
    {
        throw new ValidationException(
            $"违反规则 ADR-005_5_1: {clause5_1.Condition}\n" +
            $"Enforcement: {clause5_1.Enforcement}");
    }
    
    // 生成代码...
}
```

### 场景 2: 架构测试生成

```csharp
public void GenerateTestsForAdr(int adrNumber)
{
    var ruleSet = RuleSetRegistry.GetStrict(adrNumber);
    
    foreach (var rule in ruleSet.Rules)
    {
        var clauses = ruleSet.Clauses
            .Where(c => c.Id.RuleNumber == rule.Id.RuleNumber);
        
        foreach (var clause in clauses)
        {
            // 生成测试类名
            var testClassName = $"ADR_{adrNumber:D3}_{clause.Id.RuleNumber}_{clause.Id.ClauseNumber}_Tests";
            
            // 根据 ExecutionType 选择测试模板
            var template = clause.ExecutionType switch
            {
                ClauseExecutionType.Convention => GenerateNetArchTest(clause),
                ClauseExecutionType.StaticAnalysis => GenerateFileSystemTest(clause),
                ClauseExecutionType.Runtime => GenerateRuntimeTest(clause),
                _ => throw new NotSupportedException()
            };
            
            GenerateTestFile(testClassName, template, clause);
        }
    }
}
```

### 场景 3: 代码审查和验证

```csharp
public ReviewResult ReviewHandler(Type handlerType)
{
    // 获取相关规则集
    var adr005 = RuleSetRegistry.GetStrict(5);   // Handler 模式
    var adr240 = RuleSetRegistry.GetStrict(240); // Handler 异常约束
    
    // 检查命名
    var clause1_1 = adr005.GetClause(1, 1);
    if (!handlerType.Name.EndsWith("Handler"))
    {
        return new ReviewResult
        {
            Decision = "Blocked",
            RuleId = "ADR-005_1_1",
            Evidence = ["Handler 命名不符合规范"],
            RuleDetails = new
            {
                Condition = clause1_1.Condition,
                Enforcement = clause1_1.Enforcement
            }
        };
    }
    
    // 检查可变字段
    var clause2_1 = adr005.GetClause(2, 1);
    var mutableFields = handlerType.GetFields()
        .Where(f => !f.IsInitOnly);
        
    if (mutableFields.Any())
    {
        return new ReviewResult
        {
            Decision = "Blocked",
            RuleId = "ADR-005_2_1",
            Evidence = [$"发现可变字段: {string.Join(", ", mutableFields.Select(f => f.Name))}"],
            RuleDetails = new
            {
                Condition = clause2_1.Condition,
                Enforcement = clause2_1.Enforcement
            }
        };
    }
    
    return new ReviewResult { Decision = "Allowed" };
}
```

### 场景 4: 文档同步

```csharp
public string GenerateAdrDecisionSection(int adrNumber)
{
    var ruleSet = RuleSetRegistry.GetStrict(adrNumber);
    var sb = new StringBuilder();
    
    sb.AppendLine("## Decision");
    sb.AppendLine();
    
    foreach (var rule in ruleSet.Rules.OrderBy(r => r.Id.RuleNumber))
    {
        sb.AppendLine($"### Rule {rule.Id.RuleNumber}: {rule.Summary}");
        sb.AppendLine();
        sb.AppendLine($"**RuleId**: `ADR-{adrNumber:D3}_{rule.Id.RuleNumber}`");
        sb.AppendLine($"**Decision Level**: {rule.Decision}");
        sb.AppendLine($"**Severity**: {rule.Severity}");
        sb.AppendLine();
        
        var clauses = ruleSet.Clauses
            .Where(c => c.Id.RuleNumber == rule.Id.RuleNumber)
            .OrderBy(c => c.Id.ClauseNumber);
        
        foreach (var clause in clauses)
        {
            sb.AppendLine($"#### Clause {clause.Id.RuleNumber}.{clause.Id.ClauseNumber}");
            sb.AppendLine();
            sb.AppendLine($"**ClauseId**: `ADR-{adrNumber:D3}_{clause.Id.RuleNumber}_{clause.Id.ClauseNumber}`");
            sb.AppendLine($"**Condition**: {clause.Condition}");
            sb.AppendLine($"**Enforcement**: {clause.Enforcement}");
            sb.AppendLine();
        }
    }
    
    return sb.ToString();
}
```

### 场景 5: 违规分类和报告

```csharp
public Dictionary<RuleSeverity, List<Violation>> ClassifyViolations(List<Violation> violations)
{
    var classified = new Dictionary<RuleSeverity, List<Violation>>();
    
    foreach (var violation in violations)
    {
        // 从 RuleId 解析 ADR 编号
        var match = Regex.Match(violation.RuleId, @"ADR-(\d+)_(\d+)");
        if (!match.Success) continue;
        
        var adrNumber = int.Parse(match.Groups[1].Value);
        var ruleNumber = int.Parse(match.Groups[2].Value);
        
        var ruleSet = RuleSetRegistry.Get(adrNumber);
        if (ruleSet == null) continue;
        
        var rule = ruleSet.GetRule(ruleNumber);
        if (rule == null) continue;
        
        if (!classified.ContainsKey(rule.Severity))
        {
            classified[rule.Severity] = new List<Violation>();
        }
        classified[rule.Severity].Add(violation);
    }
    
    return classified;
}
```

---

## RuleId 格式规范

所有 RuleId 必须使用以下标准格式：

```
ADR-XXX_Y_Z
```

其中：
- `XXX`: ADR 编号（3位数字，如 001, 005, 240）
- `Y`: Rule 编号（1位或多位数字）
- `Z`: Clause 编号（1位或多位数字）

**示例**：
- ✅ `ADR-001_1_1` - ADR-001, Rule 1, Clause 1
- ✅ `ADR-005_2_3` - ADR-005, Rule 2, Clause 3
- ✅ `ADR-240_1_1` - ADR-240, Rule 1, Clause 1
- ❌ `ADR-0001_1_1` - 错误：4位数字
- ❌ `ADR-001.1.1` - 错误：使用点分隔符
- ❌ `ADR-001:1:1` - 错误：使用冒号分隔符

---

## 最佳实践

### 1. 使用严格模式进行架构验证

在架构验证、代码生成、测试等关键场景中，始终使用 `GetStrict()` 方法：

```csharp
// ✅ 推荐：严格模式
var ruleSet = RuleSetRegistry.GetStrict(1);

// ❌ 不推荐：宽容模式（除非是探索性查询）
var ruleSet = RuleSetRegistry.Get(1);
if (ruleSet == null) { /* ... */ }
```

### 2. 包含完整的 RuleDetails

在输出决策、报告或错误消息时，始终包含完整的 RuleDetails：

```csharp
return new Decision
{
    Result = "Blocked",
    RuleId = "ADR-005_1_1",
    Evidence = ["具体违规证据"],
    RuleDetails = new
    {
        Condition = clause.Condition,      // ✅ 包含条件
        Enforcement = clause.Enforcement   // ✅ 包含执行要求
    }
};
```

### 3. 按严重程度分类和处理

根据规则的严重程度采取不同的行动：

```csharp
var ruleSet = RuleSetRegistry.GetStrict(adrNumber);
var rule = ruleSet.GetRule(ruleNumber);

switch (rule.Severity)
{
    case RuleSeverity.Constitutional:
        // 宪法级违规 - 必须阻断
        return BlockAction();
        
    case RuleSeverity.Governance:
        // 治理级违规 - 需要审批
        return RequireApproval();
        
    case RuleSeverity.Technical:
        // 技术级违规 - 建议修复
        return RecommendFix();
}
```

### 4. 验证规则集完整性

在处理 RuleSet 之前，验证其完整性：

```csharp
var ruleSet = RuleSetRegistry.GetStrict(adrNumber);

try
{
    ruleSet.ValidateCompleteness();
}
catch (InvalidOperationException ex)
{
    // 存在没有 Clause 的 Rule
    Logger.Error($"RuleSet {adrNumber} 不完整: {ex.Message}");
    throw;
}
```

### 5. 缓存规则集查询结果

如果需要多次访问同一个 RuleSet，考虑缓存：

```csharp
private static readonly Lazy<ArchitectureRuleSet> LazyHandlerRules = 
    new(() => RuleSetRegistry.GetStrict(5));

public ArchitectureRuleSet HandlerRules => LazyHandlerRules.Value;
```

---

## 错误处理

### GetStrict() 方法可能抛出的异常

```csharp
try
{
    var ruleSet = RuleSetRegistry.GetStrict("ADR-999");
}
catch (InvalidOperationException ex)
{
    // 规则集不存在
    Console.WriteLine($"Error: {ex.Message}");
    // "无效的 ADR 编号：999。该 ADR 规则集不存在或尚未注册。可用的 ADR 编号：..."
}

try
{
    var ruleSet = RuleSetRegistry.GetStrict("ADR-0001");
}
catch (ArgumentException ex)
{
    // 格式错误
    Console.WriteLine($"Error: {ex.Message}");
    // "无效的 ADR 编号格式：'ADR-0001'。支持的格式：'ADR-001', 'ADR-1', '001', '1', 'ADR001'..."
}
```

---

## 相关文档

- [ADR-907：ArchitectureTests 执法治理体系](../adr/governance/ADR-907-architecture-testing-enforcement-governance-system.md)
- [ADR-907-A：ADR-907 对齐执行标准](../adr/governance/ADR-907-A-adr-907-alignment-execution-standards.md)
- [Epic: RuleSet-as-Source-of-Truth](../governance/EPIC-RuleSet-as-Source-of-Truth.md)
- [RuleSetRegistry 源代码](../../src/tools/Specification/Index/RuleSetRegistry.cs)

---

**维护者**: 架构委员会  
**版本**: 1.0  
**最后更新**: 2026-02-15
