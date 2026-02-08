# Architecture RuleSet DSL

## 概述

Architecture RuleSet DSL 是一个声明式的 API，用于定义架构规则和条款。它提供了一种简洁、可读的方式来组织和管理架构测试规则，减少样板代码，提高可维护性。

## 目录结构

```
src/tests/ArchitectureTests/Specification/RuleSets/
├── ArchitectureRuleSetDslExtensions.cs  # DSL 扩展方法和基础类
├── Adr907RuleSetTests.cs                # 验证测试
└── ADR907/
    └── Adr907RuleSet.cs                 # ADR-907 规则集实现
```

## 核心概念

### ArchitectureRuleSet
抽象基类，提供规则和条款的管理功能。每个 ADR 的规则集应继承此类。

### RuleBuilder
规则构建器，支持链式 API 来添加条款。通过 `.Rule()` 方法创建，支持以下条款类型：

- **ConventionClause**: 基于约定的架构约束验证
- **StaticClause**: 基于静态代码分析的约束验证
- **DocumentationClause**: 基于文档结构和内容的约束验证
- **ManualReviewClause**: 需要人工判断的约束验证

### ArchitectureRule & ArchitectureClause
规则和条款的数据模型，包含：
- RuleId: 唯一标识符（如 "ADR-907_1"）
- Title: 规则/条款标题
- Condition: 条款条件描述
- Enforcement: 执法级别（L1, L2）
- ExecutionType: 执行类型（Convention, Static, Documentation, ManualReview）

## 使用示例

### 定义规则集

```csharp
public class Adr907RuleSet : ArchitectureRuleSet
{
    private static readonly Lazy<Adr907RuleSet> _instance = new(() => new Adr907RuleSet());
    public static Adr907RuleSet Instance => _instance.Value;

    private Adr907RuleSet()
    {
        AdrNumber = "ADR-907";
        Title = "ArchitectureTests 执法治理体系";
        Description = "整合 ArchitectureTests 的命名、组织、最小断言及 CI/Analyzer 映射规则";
        EnsureInitialized();
    }

    protected override void DefineRules()
    {
        // 定义规则 1
        this.Rule("ADR-907_1", "ArchitectureTests 的法律地位",
                "定义 ArchitectureTests 在架构治理中的权威地位")
            .ConventionClause("1", "唯一自动化执法形式",
                "ArchitectureTests 是 ADR 的唯一自动化执法形式", "L1")
            .ConventionClause("2", "可执法性要求",
                "任何具备裁决力的 ADR 必须有对应的 ArchitectureTests", "L1");

        // 定义规则 2
        this.Rule("ADR-907_2", "命名与组织规范",
                "定义 ArchitectureTests 的项目结构和命名约定")
            .ConventionClause("1", "独立测试项目要求",
                "ArchitectureTests 必须集中于独立测试项目", "L1")
            .StaticClause("2", "ADR 编号目录分组",
                "测试目录必须按 ADR 编号分组", "L1");
    }
}
```

### 访问规则集

```csharp
// 获取规则集实例
var ruleSet = Adr907RuleSet.Instance;

// 访问规则
var rule1 = ruleSet.Rules["ADR-907_1"];
Console.WriteLine($"Rule: {rule1.Title}");

// 访问条款
foreach (var clause in rule1.Clauses.Values)
{
    Console.WriteLine($"  Clause {clause.ClauseId}: {clause.Title}");
    Console.WriteLine($"    Enforcement: {clause.Enforcement}");
    Console.WriteLine($"    Type: {clause.ExecutionType}");
}
```

## DSL API 参考

### Rule() 扩展方法

```csharp
public static RuleBuilder Rule(
    this ArchitectureRuleSet ruleSet,
    string ruleId,
    string title,
    string? description = null)
```

**参数**:
- `ruleId`: 规则唯一标识符（如 "ADR-907_1"）
- `title`: 规则标题
- `description`: 规则描述（可选）

**返回**: RuleBuilder 实例，支持链式调用

### Clause 方法

#### ConventionClause()

```csharp
public RuleBuilder ConventionClause(
    string clauseId,
    string title,
    string condition,
    string enforcement = "L1")
```

用于基于约定的架构约束验证。

#### StaticClause()

```csharp
public RuleBuilder StaticClause(
    string clauseId,
    string title,
    string condition,
    string enforcement = "L1")
```

用于基于静态代码分析的约束验证。

#### DocumentationClause()

```csharp
public RuleBuilder DocumentationClause(
    string clauseId,
    string title,
    string condition,
    string enforcement = "L1")
```

用于基于文档结构和内容的约束验证。

#### ManualReviewClause()

```csharp
public RuleBuilder ManualReviewClause(
    string clauseId,
    string title,
    string condition,
    string enforcement = "L2")
```

用于需要人工判断的约束验证。默认执法级别为 L2。

## 传统方式 vs DSL 方式

### 传统方式（假设）

```csharp
public Adr907RuleSet()
{
    AddRule("ADR-907_1", "ArchitectureTests 的法律地位", "...");
    AddClause("ADR-907_1", "1", "唯一自动化执法形式", "...", "L1", "Convention");
    AddClause("ADR-907_1", "2", "可执法性要求", "...", "L1", "Convention");
    AddClause("ADR-907_1", "3", "禁止文档约束例外", "...", "L1", "Convention");
    
    AddRule("ADR-907_2", "命名与组织规范", "...");
    AddClause("ADR-907_2", "1", "独立测试项目要求", "...", "L1", "Convention");
    AddClause("ADR-907_2", "2", "ADR 编号目录分组", "...", "L1", "Convention");
    // ... 重复多次
}
```

**问题**:
- 大量重复的 AddRule/AddClause 调用
- 规则和条款之间的关系不清晰
- 难以快速理解规则结构
- 容易出错（如忘记 AddRule 或参数顺序错误）

### DSL 方式

```csharp
protected override void DefineRules()
{
    this.Rule("ADR-907_1", "ArchitectureTests 的法律地位", "...")
        .ConventionClause("1", "唯一自动化执法形式", "...", "L1")
        .ConventionClause("2", "可执法性要求", "...", "L1")
        .ConventionClause("3", "禁止文档约束例外", "...", "L1");

    this.Rule("ADR-907_2", "命名与组织规范", "...")
        .ConventionClause("1", "独立测试项目要求", "...", "L1")
        .ConventionClause("2", "ADR 编号目录分组", "...", "L1");
}
```

**优势**:
- ✅ 声明式语法，清晰表达规则层级结构
- ✅ 链式调用，减少样板代码
- ✅ 类型安全的 Clause 方法（如 ConventionClause, StaticClause）
- ✅ 自动关联规则和条款
- ✅ 更易于阅读和维护

## 设计模式

### 单例模式
规则集使用 Lazy 单例模式，确保只初始化一次：

```csharp
private static readonly Lazy<Adr907RuleSet> _instance = new(() => new Adr907RuleSet());
public static Adr907RuleSet Instance => _instance.Value;
```

### 构建器模式
RuleBuilder 使用构建器模式，支持链式 API：

```csharp
this.Rule("...", "...")
    .Clause("1", "...", "...")
    .Clause("2", "...", "...")
    .Clause("3", "...", "...");
```

### 扩展方法模式
使用 C# 扩展方法为 ArchitectureRuleSet 添加 DSL 能力，不破坏原有类结构。

## 最佳实践

1. **单一职责**: 每个规则集只对应一个 ADR
2. **延迟初始化**: 使用 Lazy<T> 确保规则只定义一次
3. **清晰命名**: RuleId 和 ClauseId 遵循 ADR-XXX_Rule_Clause 格式
4. **文档同步**: 规则集定义应与 ADR 文档保持同步
5. **类型选择**: 根据验证方式选择合适的 Clause 类型

## 验证测试

每个规则集应包含验证测试，确保：
- 规则数量正确
- 条款数量正确
- 所有条款有执法级别
- 所有条款有执行类型
- ClauseId 格式正确

示例：
```csharp
[Fact(DisplayName = "ADR-907 应包含 4 个主要规则")]
public void Adr907RuleSet_Should_Have_Four_Main_Rules()
{
    var ruleSet = Adr907RuleSet.Instance;
    Assert.Equal(4, ruleSet.Rules.Count);
}
```

## 扩展和自定义

### 添加新的 Clause 类型

如果需要新的条款类型，可以扩展 RuleBuilder：

```csharp
public static class CustomRuleBuilderExtensions
{
    public static RuleBuilder PerformanceClause(
        this RuleBuilder builder,
        string clauseId,
        string title,
        string condition,
        string enforcement = "L2")
    {
        return builder.Clause(clauseId, title, condition, enforcement, "Performance");
    }
}
```

### 添加自定义验证

可以在 ArchitectureRuleSet 中添加自定义验证逻辑：

```csharp
public abstract class ArchitectureRuleSet
{
    public IEnumerable<string> Validate()
    {
        foreach (var rule in Rules.Values)
        {
            if (rule.Clauses.Count == 0)
                yield return $"Rule {rule.RuleId} has no clauses";
        }
    }
}
```

## 参考

- ADR-907: ArchitectureTests 执法治理体系
- ADR-0000: 架构测试与 CI 治理宪法
- [xUnit 文档](https://xunit.net/)
- [Fluent API 设计模式](https://en.wikipedia.org/wiki/Fluent_interface)
