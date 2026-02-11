# ADR Decision 生成器

## 概述

ADR Decision 生成器用于将 `ArchitectureRuleSet` 转换为 Markdown 格式的 Decision 章节，实现 ADR 文档的自动化生成和维护。

## 核心组件

### 1. IAdrDecisionGenerator

Decision 生成器接口，定义了从 RuleSet 生成 Markdown 格式 Decision 章节的契约。

**主要方法**：
- `GenerateDecisionSection(ArchitectureRuleSet ruleSet)` - 使用默认选项生成 Decision 章节
- `GenerateDecisionSection(ArchitectureRuleSet ruleSet, DecisionGenerationOptions options)` - 使用自定义选项生成 Decision 章节

### 2. AdrDecisionGenerator

Decision 生成器的默认实现，负责：
- 将 RuleSet 中的 Rules 和 Clauses 转换为标准的 Markdown 格式
- 支持 RuleId 标准格式（ADR-XXX_Y_Z）
- 自动排序和层次化输出
- Markdown 特殊字符转义

### 3. IAdrDocumentMerger

文档合并器接口，用于将生成的 Decision 章节与现有 ADR 文档合并。

**主要方法**：
- `MergeDecisionSection(string existingAdrContent, ArchitectureRuleSet ruleSet, DecisionGenerationOptions? options)` - 合并 RuleSet 到现有文档
- `MergeDecisionSection(string existingAdrContent, string newDecisionContent)` - 合并 Decision 内容到现有文档

### 4. AdrDocumentMerger

文档合并器的实现，功能包括：
- 使用 Markdig 解析 Markdown 文档
- 保留 Front Matter（YAML 头部）
- 保留 Context、Consequences 等章节
- 替换 Decision 章节
- 维护正确的章节顺序

### 5. DecisionGenerationOptions

生成选项配置类，支持：
- `IncludeSectionHeader` - 是否包含 "## Decision（裁决）" 标题
- `IncludeWarningNote` - 是否包含警告说明
- `HeaderLevelOffset` - 标题层级偏移量（0-4）
- `AddBlankLinesBetweenClauses` - 是否在 Clause 之间添加空行
- `EscapeMarkdown` - 是否转义 Markdown 特殊字符

## 使用示例

### 基本用法

```csharp
// 创建生成器实例
var generator = new AdrDecisionGenerator();

// 从 RuleSet 生成 Decision 章节
var ruleSet = Adr907_ArchitectureTests_RuleSet.Create();
var decisionMarkdown = generator.GenerateDecisionSection(ruleSet);
```

### 自定义选项

```csharp
var options = new DecisionGenerationOptions
{
    IncludeSectionHeader = true,
    IncludeWarningNote = true,
    HeaderLevelOffset = 0,
    AddBlankLinesBetweenClauses = false,
    EscapeMarkdown = true
};

var decisionMarkdown = generator.GenerateDecisionSection(ruleSet, options);
```

### 合并到现有文档

```csharp
// 创建合并器实例
var merger = new AdrDocumentMerger(new AdrDecisionGenerator());

// 读取现有 ADR 文档
var existingAdr = File.ReadAllText("docs/adr/ADR-907.md");

// 合并新的 Decision 章节
var updatedAdr = merger.MergeDecisionSection(existingAdr, ruleSet);

// 保存更新后的文档
File.WriteAllText("docs/adr/ADR-907.md", updatedAdr);
```

## 输出格式

生成的 Decision 章节遵循以下层次结构：

```markdown
## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**

### ADR-XXX_Y：规则摘要（Rule）

#### ADR-XXX_Y_Z 条款条件
- 条款执行说明
```

## 设计原则

1. **统一格式**：所有 ADR Decision 章节使用统一的 Markdown 格式
2. **可追溯性**：每个 Rule 和 Clause 都有唯一的 RuleId
3. **保留性**：合并时保留现有文档的 Context、Consequences 等章节
4. **安全性**：自动转义 Markdown 特殊字符，防止破坏文档结构
5. **可配置性**：通过 Options 支持多种生成场景

## 测试覆盖

- **单元测试**：16 个测试验证核心生成逻辑
- **集成测试**：12 个测试验证与真实 RuleSet 的集成
- **Golden 测试**：验证生成内容与标准样本的一致性
- **安全测试**：验证边界条件和异常处理

总计 **49 个测试**，覆盖率 > 80%

## 相关文档

- [ADR-907: ArchitectureTests 执法治理体系](../../RuleSets/ADR907/ADR-907.md)
- [ADR-907-A: ADR-907 对齐执行标准](../../RuleSets/ADR907A/ADR-907-A.md)
- [Specification Language 规范](../Language/README.md)

## 维护指南

### 添加新的生成选项

1. 在 `DecisionGenerationOptions` 中添加新属性
2. 在 `AdrDecisionGenerator` 中实现相应逻辑
3. 添加单元测试验证新选项
4. 更新本 README 和相关文档

### 修改输出格式

1. 修改 `AdrDecisionGenerator` 中的相应方法
2. 更新 Golden 测试样本文件
3. 运行所有测试确保兼容性
4. 更新文档说明新格式

## 版本历史

- **2026-02-11**: 初始实现，支持 RuleSet → Markdown 转换和文档合并
- **2026-02-11**: 重构，统一使用 Shared/Adr/FrontMatterParser 处理 Front Matter
