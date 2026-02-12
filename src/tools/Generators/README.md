# Zss.BilliardHall.Generators

架构治理生成器库 - 将 RuleSet 转换为不同格式的可执行产物。

## 概述

本库提供四个核心生成器：

1. **AdrDecisionGenerator** - 将 RuleSet 生成为 ADR 文档的 Decision 章节（Markdown 格式）
2. **AdrDocumentMerger** - 将生成的 Decision 章节与现有 ADR 文档合并
3. **ArchitectureTestGenerator** - 将 RuleSet 生成为 xUnit 架构测试代码
4. **AgentInstructionGenerator** - 将 RuleSet 生成为 Agent Instructions（YAML 格式）

## 使用示例

### ADR Decision 生成

```csharp
var generator = new AdrDecisionGenerator();
var ruleSet = // ... 构建 RuleSet
var markdown = generator.GenerateDecisionSection(ruleSet);
```

### ADR 文档合并

```csharp
var merger = new AdrDocumentMerger(new AdrDecisionGenerator());
var existingAdr = File.ReadAllText("docs/adr/ADR-001.md");
var ruleSet = // ... 构建 RuleSet

// 合并新的 Decision 章节到现有文档
var updatedAdr = merger.MergeDecisionSection(existingAdr, ruleSet);
File.WriteAllText("docs/adr/ADR-001.md", updatedAdr);
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

## 依赖注入支持

本库提供 ServiceCollection 扩展方法，便于在 ASP.NET Core 或其他支持 DI 的应用中使用：

```csharp
// 注册所有生成器
services.AddGenerators();

// 或单独注册
services.AddAdrDecisionGenerator();
services.AddAdrDocumentMerger();
services.AddAgentInstructionGenerator();
services.AddArchitectureTestGenerator();
```

## YAML 注入防护（AgentInstructionGenerator）

**修复日期**：2026-02-12

AgentInstructionGenerator 使用 **YamlDotNet** 进行安全的 YAML 序列化，确保：

### 防护特性

1. **自动转义特殊字符**：
   - 换行符 `\n` → `\\n`
   - 双引号 `"` → `\"`
   - Shell 特殊字符 `$`, `` ` `` → 转义为 `\$`, `` \` ``
   - YAML 控制字符（冒号、管道、大括号等）通过引号包裹

2. **结构化序列化**：
   - 使用 YamlDotNet 的 SerializerBuilder
   - CamelCase 命名约定
   - 自动处理 null 值
   - 多行文本自动转换为单行转义格式

3. **防止 YAML 注入**：
   - 用户输入中的恶意 YAML 结构会被转义为纯文本
   - 无法通过输入注入新的 YAML 键值对或列表
   - 验证通过反序列化测试确保结构完整性

### 测试覆盖

- 34 个安全测试用例验证各种注入场景
- 测试覆盖：单行、多行、特殊字符、Shell 命令、YAML 控制字符
- 所有测试采用结构比对（反序列化验证）而非文本匹配

## 架构原则

- **不可变性**：所有 Options 类型使用 `record` 或 `init-only` 属性
- **依赖注入**：支持构造函数注入（带默认实现）
- **参数校验**：使用 `ArgumentNullException.ThrowIfNull` 和 Options.Validate()
- **早期返回**：减少嵌套，提高可读性
- **小方法拆分**：遵循单一职责原则
- **稳定输出**：同样的输入产生同样的输出（决定性生成）
- **安全优先**：所有用户输入都经过转义和验证

## 依赖

- .NET 10.0
- Markdig（Markdown 解析与生成）
- YamlDotNet（YAML 序列化）
- Microsoft.Extensions.DependencyInjection.Abstractions（DI 支持）

## 命名空间

`Zss.BilliardHall.Generators`

所有公共类型位于此命名空间下。

## 迁移说明

### AdrDocumentMerger 迁移（2026-02-12）

`AdrDocumentMerger` 已从测试项目 (`src/tests/ArchitectureTests/Specification/Generator/`) 迁移到生产代码库 (`src/tools/Generators/`)。

**变更内容**：
- ✅ 接口 `IAdrDocumentMerger` 已添加
- ✅ 实现类 `AdrDocumentMerger` 已迁移
- ✅ 命名空间更新为 `Zss.BilliardHall.Generators`
- ✅ 添加 DI 注册扩展 `AddAdrDocumentMerger()`
- ✅ 所有 11 个单元测试已迁移并通过

**旧实现位置**（已禁用）：
- `src/tests/ArchitectureTests/Specification/Generator/Tests/AdrDocumentMerger_Tests.cs.disabled`

**新实现位置**：
- 接口：`src/tools/Generators/IAdrDocumentMerger.cs`
- 实现：`src/tools/Generators/AdrDocumentMerger.cs`
- 测试：`src/tests/ArchitectureTests/Specification/Generator/Tests/AdrDocumentMerger_Tests.cs`

**迁移原因**：
- 支持生产环境中的 ADR 文档自动化更新
- 与其他生成器保持架构一致性
- 启用依赖注入以提高可测试性
