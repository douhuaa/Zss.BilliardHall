# Zss.BilliardHall.Generators

这是一个可复用的代码生成器库，用于从架构规则集（RuleSet）生成各种格式的文档和代码。

## 概述

本项目提供了一组生成器接口和实现，用于将架构决策记录（ADR）中定义的规则和条款转换为可执行的文档和测试代码。

## 核心组件

### 1. IAdrDecisionGenerator

ADR Decision 生成器接口，用于从 RuleSet 生成 Markdown 格式的 Decision 章节。

**使用示例**：

```csharp
// 创建生成器实例
var generator = new AdrDecisionGenerator();

// 从 RuleSet 生成 Decision 章节
var result = generator.GenerateDecisionSection(ruleSet);
Console.WriteLine(result.Content);

// 使用自定义选项
var options = new DecisionGenerationOptions
{
    IncludeSectionHeader = true,
    IncludeWarningNote = true,
    HeaderLevelOffset = 0,
    AddBlankLinesBetweenClauses = false,
    EscapeMarkdown = true
};

var result = generator.GenerateDecisionSection(ruleSet, options);
```

### 2. DecisionGenerationOptions

生成选项配置类，支持：
- `IncludeSectionHeader` - 是否包含 "## Decision（裁决）" 标题
- `IncludeWarningNote` - 是否包含警告说明
- `HeaderLevelOffset` - 标题层级偏移量（0-2）
- `AddBlankLinesBetweenClauses` - 是否在 Clause 之间添加空行
- `EscapeMarkdown` - 是否转义 Markdown 特殊字符

### 3. DecisionGenerationResult

封装生成结果的类，包含：
- `Content` - 生成的 Markdown 格式文本

支持隐式转换为字符串，便于向后兼容。

## 架构设计

### 接口抽象

为了保持生成器的独立性和可复用性，我们定义了以下接口：

- `IRuleSet` - 规则集接口
- `IRule` - 规则接口
- `IClause` - 条款接口
- `IRuleId` - 规则 ID 接口

这些接口允许生成器与具体的规则数据结构解耦，使其可以被不同的项目和上下文使用。

### 性能优化

- 使用 `StringBuilder` 进行字符串拼接，预估容量以减少内存分配
- 稳定排序：按 `Rule.Id`, `Clause.Number` 使用 Ordinal comparer
- 条款检索优化：从 O(N*M) 降低到 O(M log M + N)

### 确定性输出

- 所有排序操作使用稳定的比较器
- 行尾统一为 LF，避免跨平台差异
- 参数校验：`ArgumentNullException.ThrowIfNull` 确保输入有效性

## 使用要求

- .NET 10.0 或更高版本
- C# 14

## 后续计划

此 PR 是第一步重构，后续计划：
- 逐步迁移其他生成器（AgentInstructionGenerator、ArchitectureTestGenerator 等）
- 添加更多生成器实现
- 完善文档和示例
- 添加单元测试（当前测试在 ArchitectureTests 项目中）

## 贡献

请遵循仓库的编码约定和 ADR 规范。
