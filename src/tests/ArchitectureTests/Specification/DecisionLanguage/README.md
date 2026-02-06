# DecisionLanguage - 裁决语言模型

## 概述

DecisionLanguage 是 ArchitectureTests 的"裁决语法层"，从简单的"关键词匹配"升级到"规范语言模型"，支持三态输出（✅ Allowed / ⚠️ Blocked / ❓ Warning）。

## 核心概念

### 1. DecisionLevel（裁决级别）

与 ADR-905 对齐的三个裁决级别：

- **Must**：强制性要求，违反将阻断构建/CI
- **MustNot**：明确禁止，违反将阻断构建/CI  
- **Should**：推荐建议，违反仅产生警告不阻断

### 2. DecisionRule（裁决规则）

定义裁决语言的识别规则：

```csharp
public sealed record DecisionRule(
    DecisionLevel Level,           // 裁决级别
    IReadOnlyList<string> Keywords, // 触发关键词
    bool IsBlocking                // 是否阻断
);
```

### 3. DecisionResult（解析结果）

Parse 方法的返回值：

```csharp
public sealed record DecisionResult(
    DecisionLevel? Level,  // 级别（null 表示 None）
    bool IsBlocking       // 是否阻断
)
{
    public static readonly DecisionResult None;
    public bool IsDecision => Level.HasValue;
}
```

## API 使用

### 基本解析

```csharp
var result = ArchitectureTestSpecification.DecisionLanguage.Parse("必须遵循模块边界");

if (result.Level == DecisionLevel.Must && result.IsBlocking)
{
    // 处理强制性规则
}
```

### 三态判定

```csharp
var decision = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

if (decision.IsBlocking)
{
    // ❌ Blocked: 阻断级别，测试失败
}
else if (decision.IsDecision)
{
    // ⚠️ Warning: 警告级别，记录但不阻断
}
else
{
    // ✅ Allowed: 无裁决语言或符合规范
}
```

### 辅助方法

```csharp
// 判断是否为阻断级别
bool isBlocking = ArchitectureTestSpecification.DecisionLanguage.IsBlockingDecision(text);

// 判断是否包含任何裁决语言
bool hasDecision = ArchitectureTestSpecification.DecisionLanguage.HasDecisionLanguage(text);
```

## 规则配置

当前配置的规则集：

| 级别 | 关键词 | 阻断 |
|------|--------|------|
| Must | 必须, 强制, 需要 | ✅ |
| MustNot | 禁止, 不得, 不允许 | ✅ |
| Should | 应该, 建议, 推荐 | ❌ |

访问规则配置：

```csharp
var rules = ArchitectureTestSpecification.DecisionLanguage.Rules;
```

## 设计原则

1. **未识别 ≠ Must**：防止误伤，未识别的文本返回 `DecisionResult.None`
2. **None 的 Level 为 null**：明确区分 None 和真实的 Should 级别
3. **按序匹配**：Rules 按顺序匹配，一旦匹配立即返回
4. **默认非阻断**：None 的 IsBlocking 为 false

## 向后兼容

旧的 `Semantics.DecisionKeywords` 仍然可用，但已标记为 Obsolete：

```csharp
[Obsolete("请使用 ArchitectureTestSpecification.DecisionLanguage 代替")]
public static IReadOnlyList<string> DecisionKeywords { get; }
```

## 与其他组件的关系

DecisionLanguage 是架构治理的基础设施，可被以下组件使用：

- **ArchitectureTests**：L1 级别的阻断式验证
- **Roslyn Analyzers**：L2 级别的编译时警告
- **GitHub Copilot Agents**：智能代码审查和建议

## 示例代码

完整的使用示例请参考：`Examples/DecisionLanguageUsageExamples.cs`

## 测试覆盖

- 36 个单元测试覆盖所有场景
- 测试文件：`Tests/DecisionLanguageTests.cs`
- 测试覆盖率：100%

## 未来扩展

1. **DecisionLanguage → ADR-905 Markdown 自动生成**
2. **DecisionResult → 三态输出标准化**
3. **RuleId × DecisionLanguage × Clause 强类型绑定**
4. **支持更多语言（英文等）**
5. **自定义规则注册机制**
