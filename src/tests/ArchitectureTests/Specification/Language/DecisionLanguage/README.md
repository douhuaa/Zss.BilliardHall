# DecisionLanguage - 裁决语言模型

## 概述

DecisionLanguage 是 ArchitectureTests 的"裁决语法层"，从简单的"关键词匹配"升级到"规范语言模型"，支持三态输出（✅ Allowed / ⚠️ Blocked / ❓ Warning）。

**最新增强**：v2.0 引入了词边界识别和语义上下文分析，显著降低误判率。

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

## 增强功能（v2.0）

### 词边界识别

系统现在能够识别复合词，避免误判：

- ❌ "需要性分析" 中的"需要"不会被匹配
- ❌ "禁止者" 中的"禁止"不会被匹配  
- ❌ "可需要性" 中的"需要"不会被匹配
- ✅ "需要遵循规范" 中的"需要"会被正确匹配

**实现机制**：检查关键词后是否跟随特定后缀（如"性"、"者"、"度"），这些后缀通常表示复合词。

### 否定上下文分析

系统现在能够识别否定上下文，避免将否定表述误判为裁决语言：

#### 前置否定词
- ❌ "不应该使用反射" → 不匹配（"不"否定了"应该"）
- ❌ "不需要额外配置" → 不匹配
- ✅ "应该使用依赖注入" → 匹配 Should 级别

#### 后置否定词
- ❌ "应该避免使用全局变量" → 不匹配（"避免"否定了"应该"）
- ❌ "应该不要直接访问" → 不匹配
- ✅ "应该优先使用构造函数注入" → 匹配 Should 级别

#### 复杂场景处理
- "不应该使用反射，但应该使用依赖注入" → 匹配第二个"应该"（Should 级别）

### 误判对比

| 场景 | v1.0 (简单匹配) | v2.0 (增强解析) |
|-----|----------------|----------------|
| "应该避免使用全局变量" | ✅ Should (误判) | ❌ None (正确) |
| "不应该使用反射" | ✅ Should (误判) | ❌ None (正确) |
| "需要性分析" | ✅ Must (误判) | ❌ None (正确) |
| "禁止者" | ✅ MustNot (误判) | ❌ None (正确) |
| "应该使用依赖注入" | ✅ Should (正确) | ✅ Should (正确) |

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
5. **词边界识别**：避免复合词误判（v2.0 新增）
6. **否定上下文排除**：识别并排除否定表述（v2.0 新增）

## 实现细节（v2.0）

### 匹配算法流程

```
1. 基本检查：sentence 是否包含 keyword
   ↓ 否 → 返回 false
   ↓ 是
2. 词边界检查：keyword 是否构成复合词？
   ↓ 是（如"需要性"）→ 跳过此位置，继续查找
   ↓ 否
3. 否定上下文检查：keyword 是否处于否定上下文？
   ↓ 是（如"不应该"）→ 跳过此位置，继续查找
   ↓ 否
4. 返回 true（找到有效匹配）
```

### 词边界规则

检查关键词后是否跟随复合词后缀：
- "性"：需要性、可能性、必要性
- "者"：禁止者、建议者
- "度"：强制度、必要度

### 否定上下文规则

**前置否定词**：
- "不" + 关键词：不应该、不需要、不必须

**后置否定词**：
- 关键词 + "避免"：应该避免、建议避免
- 关键词 + "不要"：应该不要、建议不要

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

- **57 个单元测试**覆盖所有场景（v2.0 新增 21 个测试）
- **14 个集成测试**验证实际使用场景
- 测试文件：`Tests/DecisionLanguageTests.cs`、`Tests/DecisionLanguageIntegrationTests.cs`
- 测试覆盖率：100%

### 新增测试场景（v2.0）
- 否定上下文识别：5 个测试
- 词边界识别：3 个测试
- 正向裁决语言验证：4 个测试
- 多关键词处理：1 个测试
- 边界情况：6 个测试
- 典型误判场景：3 个测试

## 未来扩展

1. **DecisionLanguage → ADR-905 Markdown 自动生成**
2. **DecisionResult → 三态输出标准化**
3. **RuleId × DecisionLanguage × Clause 强类型绑定**
4. **支持更多语言（英文等）**
5. **自定义规则注册机制**
6. **更复杂的语义分析（如基于机器学习的上下文理解）**

## 版本历史

### v2.0 (2026-02-06)
- ✨ 新增词边界识别功能
- ✨ 新增否定上下文分析功能
- ✨ 显著降低误判率
- ✅ 新增 21 个测试用例
- 📚 更新文档说明

### v1.0 (2026-02-05)
- 🎉 初始版本：基于关键词的简单匹配
- ✅ 支持三态输出（Must/MustNot/Should）
- ✅ 36 个单元测试
