# Zss.BilliardHall.Generators

架构治理生成器库 - 将 RuleSet 转换为不同格式的可执行产物。

## 概述

本库提供三个核心生成器：

1. **AdrDecisionGenerator** - 将 RuleSet 生成为 ADR 文档的 Decision 章节（Markdown 格式）
2. **ArchitectureTestGenerator** - 将 RuleSet 生成为 xUnit 架构测试代码
3. **AgentInstructionGenerator** - 将 RuleSet 生成为 Agent Instructions（YAML 格式）

## 使用示例

### ADR Decision 生成

```csharp
var generator = new AdrDecisionGenerator();
var ruleSet = // ... 构建 RuleSet
var markdown = generator.GenerateDecisionSection(ruleSet);
```

### 架构测试生成

```csharp
var generator = new ArchitectureTestGenerator();
var ruleSet = // ... 构建 RuleSet
var testCode = generator.Generate(ruleSet);
```

### Agent 指令生成

```csharp
var generator = new AgentInstructionGenerator();
var ruleSet = // ... 构建 RuleSet
var yaml = generator.GenerateInstructions(ruleSet);
```

## 架构原则

- **不可变性**：所有 Options 类型使用 `record` 或 `init-only` 属性
- **依赖注入**：支持构造函数注入（带默认实现）
- **参数校验**：使用 `ArgumentNullException.ThrowIfNull` 和 Options.Validate()
- **早期返回**：减少嵌套，提高可读性
- **小方法拆分**：遵循单一职责原则
- **稳定输出**：同样的输入产生同样的输出（决定性生成）

## 依赖

- .NET 10.0
- Markdig（Markdown 解析与生成）
- YamlDotNet（YAML 序列化）

## 命名空间

`Zss.BilliardHall.Generators`

所有公共类型位于此命名空间下。
