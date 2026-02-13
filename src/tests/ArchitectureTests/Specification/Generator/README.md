# Generator 测试

## 📋 概述

本目录包含所有与 ADR 文档生成器相关的测试，验证自动生成 ADR Decision 节和 Agent Instructions 的功能。

## 🎯 职责范围

- **ADR Decision 生成**：从 RuleSet 自动生成 ADR 文档的 Decision 节
- **Agent Instruction 生成**：从 RuleSet 自动生成 Agent Instructions YAML 文件
- **文档合并**：将生成的内容合并到现有 ADR 文档
- **YAML 安全**：防止 YAML 注入和格式错误

## 📁 测试文件

### ADR Decision Generator 测试

| 测试文件 | 职责 | 测试数量 |
|---------|------|---------|
| `AdrDecisionGenerator_Tests.cs` | 核心生成功能单元测试 | ~40 |
| `AdrDecisionGenerator_IntegrationTests.cs` | 端到端集成测试 | ~15 |
| `AdrDecisionGenerator_RefactoredMethodsTests.cs` | 重构方法专项测试 | ~30 |
| `AdrDecisionGenerator_SafetyTests.cs` | 安全边界和参数验证 | ~18 |
| `AdrDecisionGenerator_GoldenTests.cs` | Golden 文件对比测试 | ~3 |

### Agent Instruction Generator 测试

| 测试文件 | 职责 | 测试数量 |
|---------|------|---------|
| `AgentInstructionGenerator_Tests.cs` | 核心生成功能单元测试 | ~30 |
| `AgentInstructionGenerator_IntegrationTests.cs` | 端到端集成测试 | ~12 |
| `AgentInstructionGenerator_SecurityTests.cs` | YAML 注入防护测试 | ~34 |
| `AgentInstructionGenerator_YamlEscaping_Tests.cs` | YAML 转义专项测试 | ~15 |
| `AgentInstructionGenerator_GoldenTests.cs` | Golden 文件对比测试 | ~3 |

### 其他测试

| 测试文件 | 职责 | 测试数量 |
|---------|------|---------|
| `AdrDocumentMerger_Tests.cs` | 文档合并逻辑测试 | ~20 |
| `InstructionGenerationOptions_Tests.cs` | 生成选项验证测试 | ~10 |

## 🔍 关键概念

### ADR Decision Generator

`AdrDecisionGenerator` 负责从 `ArchitectureRuleSet` 生成 Markdown 格式的 Decision 节：

```markdown
## Decision（裁决）

⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**

### Rule 3: 最小断言语义规范

**决策级别**: Must  
**严重性**: Governance  
**作用域**: Test

#### Clause 3.1: 断言必须验证单一职责
**执行方式**: Automated

每个断言应专注于验证一个明确的行为或属性...
```

### Agent Instruction Generator

`AgentInstructionGenerator` 负责从 `ArchitectureRuleSet` 生成 YAML 格式的 Agent Instructions：

```yaml
agent: architecture-guardian
guidelines:
  - "ADR-907_3_1: 断言必须验证单一职责"
  - "ADR-907_3_2: 使用描述性断言方法"
```

### YAML 安全机制

生成器实现了全面的 YAML 注入防护：
- 转义特殊字符：`\`, `"`, `\n`, `` ` ``, `$`
- 防止命令注入和代码执行
- 确保生成的 YAML 结构完整性

## 🧪 测试示例

```csharp
// ADR Decision 生成测试
[Fact]
public void GenerateDecisionSection_Should_Return_Valid_Markdown()
{
    var ruleSet = CreateSampleRuleSet();
    var generator = new AdrDecisionGenerator();
    
    var result = generator.GenerateDecisionSection(ruleSet);
    
    result.Should().Contain("## Decision（裁决）");
    result.Should().Contain("### Rule 1:");
    result.Should().Contain("#### Clause 1.1:");
}

// YAML 注入防护测试
[Theory]
[InlineData("$(whoami)", "\\$(whoami)")]
[InlineData("'; rm -rf /", "\\'; rm -rf /")]
public void EscapeYamlString_Should_Prevent_Injection(
    string malicious, string expected)
{
    var escaped = AgentInstructionGenerator.EscapeYamlString(malicious);
    escaped.Should().Be(expected);
}

// Golden 测试
[Fact]
public void GenerateDecisionSection_Should_Match_Golden_Sample()
{
    var ruleSet = LoadAdr907RuleSet();
    var generated = _generator.GenerateDecisionSection(ruleSet);
    var golden = File.ReadAllText(_goldenFilePath);
    
    generated.Should().Be(golden);
}
```

## 📂 Golden 样本文件

`golden/` 目录包含标准样本文件，用于回归测试：
- `adr907_sample.md`：ADR-907 的 Decision 节标准样本
- `agent_instructions_sample.yaml`：Agent Instructions 标准样本

## 🔧 生成选项

### DecisionGenerationOptions

```csharp
var options = new DecisionGenerationOptions
{
    IncludeWarningNote = true,      // 包含警告注释
    HeaderLevelOffset = 0,          // 标题级别偏移（0-2）
    IncludeMetadata = true,         // 包含决策级别等元数据
    IncludeExecutionType = true     // 包含执行方式
};
```

### InstructionGenerationOptions

```csharp
var options = new InstructionGenerationOptions
{
    IncludeRuleId = true,          // 包含 RuleId 前缀
    MaxGuidelinesPerAgent = 50,    // 每个 Agent 最大指导数量
    SortByRuleNumber = true        // 按规则编号排序
};
```

## 📚 相关资源

- **ADR-902**：定义了 ADR 文档生成规范
- **ADR-907**：定义了 ArchitectureTests 执法治理体系
- **ADR-940**：定义了文档生成和维护规范
- **生产代码位置**：`src/tools/Generators/`（独立项目）
- **测试适配器**：`Specification/Generator/Adapters/`（向后兼容）

## 🎯 设计原则

1. **单一职责**：每个生成器专注于一种输出格式
2. **可测试性**：通过接口和 Golden 测试确保质量
3. **安全性**：防止注入攻击和格式错误
4. **向后兼容**：通过适配器支持旧测试代码
