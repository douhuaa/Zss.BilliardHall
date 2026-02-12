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

## YAML 多行字符串序列化（AgentInstructionGenerator）

**最后更新**：2026-02-12

### 实现策略

AgentInstructionGenerator 使用 **YamlDotNet 12.0.2** 配合自定义 **MultilineEventEmitter** 实现 YAML 序列化：

#### 核心机制

1. **MultilineEventEmitter**（`src/tools/Generators/Utils/MultilineEventEmitter.cs`）
   - 继承自 `YamlDotNet.Serialization.EventEmitters.ChainedEventEmitter`
   - 自动检测包含换行符的字符串
   - 对多行字符串使用 **ScalarStyle.Literal**（`|` 格式）
   - 对包含特殊字符的单行字符串使用 **ScalarStyle.DoubleQuoted**

2. **特殊字符处理**
   - 以冒号开头：`: starts with colon` → 使用双引号
   - 包含 `: ` 模式：`key: value` → 使用双引号
   - 以空格开头或结尾 → 使用双引号
   - 包含 `` ` `` 或 `$` → 使用双引号
   - 包含换行符 `\n` → 使用 literal block（`|`）

3. **序列化配置**
   ```csharp
   new SerializerBuilder()
       .WithNamingConvention(CamelCaseNamingConvention.Instance)
       .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
       .WithEventEmitter(next => new MultilineEventEmitter(next))
       .Build()
   ```

### YAML 安全性

1. **防止 YAML 注入**：
   - 用户输入中的恶意 YAML 结构（如 `commands:\n  evil: rm -rf /`）会被自动转义
   - 无法通过输入注入新的 YAML 键值对或列表
   - 验证通过反序列化测试确保结构完整性

2. **测试覆盖**（`AgentInstructionGenerator_YamlEscaping_Tests.cs`）：
   - 17 个测试用例覆盖各种边界情况
   - 单行文本、多行文本、特殊字符组合
   - 冒号、引号、反引号、美元符号
   - 所有测试采用结构比对（反序列化验证）

### 已知限制

1. **尾随空格处理**：YamlDotNet 可能会修剪字符串尾随空格，这是 YAML 规范的预期行为
2. **极端边界情况**：超长字符串或极其复杂的嵌套结构未经详尽测试
3. **性能考量**：MultilineEventEmitter 对每个字符串值都会进行模式检查，对于超大规模序列化可能有轻微性能影响

### 后续优化建议

1. **缓存正则表达式**：如果 `NeedsQuoting` 方法使用正则表达式，可以预编译提高性能
2. **更细粒度的样式控制**：根据字符串长度、复杂度选择不同的 ScalarStyle
3. **性能基准测试**：建立性能基准，监控大规模序列化场景

## AdrDocumentMerger 迁移与重构

**迁移日期**：2026-02-12

### 迁移说明

AdrDocumentMerger 已从测试目录迁移到生产代码（`src/tools/Generators/AdrDocumentMerger.cs`），并进行了重构以提高可读性和可测试性。

### 架构设计

#### 接口定义

```csharp
public interface IAdrDocumentMerger
{
    string MergeDecisionSection(string existingAdrContent, ArchitectureRuleSet ruleSet, DecisionGenerationOptions? options = null);
    string MergeDecisionSection(string existingAdrContent, string newDecisionContent);
}
```

#### 方法职责拆分

1. **ExtractRawFrontMatter**：提取 YAML Front Matter（包括 `---` 分隔符）
2. **ExtractSections**：使用 Markdig 解析 Markdown，提取所有章节（H2 标题）
3. **ExtractSectionName**：从标题行提取章节名称（去除中英文标题）
4. **NormalizeNewlines**：统一换行符为 LF，避免跨平台差异
5. **MergeDecisionSection**：协调所有步骤，构建最终文档

### 核心功能

1. **Front Matter 保留**：正确保留 YAML front matter（`---` 包裹）
2. **章节顺序管理**：按固定顺序重组章节
   ```
   Front Matter → Focus → Glossary → Decision → Context → Consequences → References
   ```
3. **Decision 章节替换**：用新生成的 Decision 内容替换旧内容
4. **非标准章节保留**：保留不在标准顺序中的自定义章节

### 测试覆盖

**测试文件**：`src/tests/ArchitectureTests/Specification/Generator/Tests/AdrDocumentMerger_Tests.cs`

- 10 个测试用例覆盖各种合并场景
- Front Matter 保留、Decision 区块替换、章节顺序维护
- 空文档、无 Decision 区块的回退策略
- 参数验证（null 检查）

### 使用示例

```csharp
var generator = new AdrDecisionGenerator();
var merger = new AdrDocumentMerger(generator);
var existingAdr = File.ReadAllText("docs/adr/ADR-001.md");
var ruleSet = /* ... 构建 RuleSet ... */;

// 合并新的 Decision 章节到现有文档
var updatedAdr = merger.MergeDecisionSection(existingAdr, ruleSet);
File.WriteAllText("docs/adr/ADR-001.md", updatedAdr);
```

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
