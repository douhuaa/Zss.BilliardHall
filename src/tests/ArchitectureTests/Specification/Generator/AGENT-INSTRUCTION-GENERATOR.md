# Agent Instruction Generator

## 概述

Agent Instruction Generator 用于将 `ArchitectureRuleSet` 自动转换为 YAML 格式的 Agent Instructions，实现 Agent 指令的自动化生成和维护。

## 核心组件

### 1. IAgentInstructionGenerator

Agent 指令生成器接口，定义了从 RuleSet 生成 YAML 格式 Agent Instructions 的契约。

**主要方法**：
- `GenerateInstructions(ArchitectureRuleSet ruleSet)` - 使用默认选项生成指令
- `GenerateInstructions(ArchitectureRuleSet ruleSet, InstructionGenerationOptions options)` - 使用自定义选项生成指令

### 2. AgentInstructionGenerator

Agent 指令生成器的默认实现，负责：
- 将 RuleSet 中的 Rules 和 Clauses 转换为 YAML 格式
- 支持 RuleId 标准格式（ADR-XXX_Y_Z）
- 自动排序和层次化输出
- YAML 特殊字符转义
- 生成 RuleSet API 查询示例
- 生成约束检查逻辑
- 生成测试命令

### 3. InstructionGenerationOptions

生成选项配置类，支持：
- `AgentPrefix` - Agent 前缀（例如：AG, TG, HP）
- `AgentName` - Agent 名称
- `StartInstructionNumber` - 起始指令编号
- `IncludeApiExamples` - 是否包含 RuleSet API 查询示例
- `IncludeConstraintChecks` - 是否包含约束检查逻辑
- `IncludeTestCommands` - 是否包含测试命令
- `IncludeGuidelines` - 是否包含指导原则
- `IndentSpaces` - YAML 缩进空格数

## 使用示例

### 基本用法

```csharp
// 创建生成器实例
var generator = new AgentInstructionGenerator();

// 从 RuleSet 生成指令
var ruleSet = new Adr907RuleSet().Define();
var instructionsYaml = generator.GenerateInstructions(ruleSet);

// 保存到文件
File.WriteAllText(".github/instructions/adr907-agent.instructions.yaml", instructionsYaml);
```

### 自定义选项

```csharp
var options = new InstructionGenerationOptions
{
    AgentPrefix = "AG",
    AgentName = "Architecture Guardian",
    StartInstructionNumber = 1,
    IncludeApiExamples = true,
    IncludeConstraintChecks = true,
    IncludeTestCommands = true,
    IncludeGuidelines = true,
    IndentSpaces = 2
};

var instructionsYaml = generator.GenerateInstructions(ruleSet, options);
```

### 批量生成多个 RuleSets

```csharp
var generator = new AgentInstructionGenerator();
var ruleSetDefinitions = new IArchitectureRuleSetDefinition[]
{
    new Adr907RuleSet(),
    new Adr900RuleSet(),
    new Adr001RuleSet()
};

foreach (var ruleSetDef in ruleSetDefinitions)
{
    var ruleSet = ruleSetDef.Define();
    var options = new InstructionGenerationOptions
    {
        AgentPrefix = $"ADR{ruleSet.AdrNumber:D3}".Substring(0, 3),
        AgentName = $"ADR-{ruleSet.AdrNumber} Agent"
    };
    
    var yaml = generator.GenerateInstructions(ruleSet, options);
    var fileName = $".github/instructions/adr{ruleSet.AdrNumber:D3}-agent.instructions.yaml";
    File.WriteAllText(fileName, yaml);
}
```

## 输出格式

生成的 Agent Instructions 遵循 `.github/INSTRUCTIONS-SCHEMA.md` 规范：

```yaml
instructions:
  - id: GEN-001
    description: "规则摘要"
    action: "验证 ADR-XXX_Y 的 N 个约束条款"
    conditions:
      - "PullRequest"
      - "CI pipeline"
    output: "Allowed / Blocked / Uncertain"
    tools:
      - "RuleSet API"
      - "ArchitectureTests"
      - "ADR-XXX RuleSet"
    feedback:
      - "生成 FailureObject（如违反约束）"
      - "阻断 CI 管道（Constitutional 级别）"
      - "记录违规到日志"
    guidelines:
      - "RuleSet API 查询示例："
        - "ruleSet.GetClause(1, 1) → 条件描述"
      - "约束检查逻辑："
        - "ADR-XXX_Y_Z - 执行类型: 执行要求"
    commands:
      run_adr_tests: "dotnet test src/tests/ArchitectureTests/ ..."
      run_all_architecture_tests: "dotnet test src/tests/ArchitectureTests/ ..."
```

## 特性

### 1. RuleSet API 查询示例

自动生成查询示例，帮助 Agent 理解如何访问 RuleSet：

```yaml
guidelines:
  - "RuleSet API 查询示例："
    - "ruleSet.GetClause(1, 1) → 测试类使用 ADR-XXX_Y_Z_Tests 格式"
    - "ruleSet.GetClause(1, 2) → 测试方法使用 Should_描述预期行为 格式"
```

### 2. 约束检查逻辑

自动生成约束检查逻辑，描述如何验证每个条款：

```yaml
guidelines:
  - "约束检查逻辑："
    - "ADR-907_1_1 - 使用静态分析验证: 文件名必须匹配 'ADR-{Number}_{RuleNumber}_{ClauseNumber}_Tests.cs'"
    - "ADR-907_1_2 - 使用静态分析验证: 方法名必须以 'Should_' 开头"
```

### 3. 三态输出

所有指令输出遵循三态判定规则：`Allowed / Blocked / Uncertain`

### 4. 严重程度反馈

根据规则的严重程度自动生成相应的反馈机制：
- **Constitutional** - 阻断 CI 管道
- **Governance** - 阻止 PR 合并
- **Technical** - 生成架构警告

### 5. 作用域条件

根据规则的作用域自动生成触发条件：
- **Solution** - CI pipeline
- **Module** - Code Modified
- **Document** - Documentation Updated
- **Test** - Test Modified
- **Agent** - Agent Instruction Updated

## 验证

生成器包含完整的测试套件：

### 单元测试（37 个）
- 接口契约测试
- YAML 格式验证
- API 查询示例测试
- 边界条件测试
- 特殊字符转义测试

### 集成测试（8 个）
- 与实际 RuleSet 的集成
- 复杂场景测试
- YAML 结构验证
- 确定性输出测试

### Golden 测试（8 个）
- 与标准样本对比
- 格式一致性验证
- 结构完整性检查

## 最佳实践

### 1. 使用标准前缀

遵循 `.github/INSTRUCTIONS-SCHEMA.md` 中定义的 Agent 前缀映射：
- AG - Architecture Guardian
- AR - ADR Reviewer
- DM - Documentation Maintainer
- DE - Expert Dotnet Engineer
- HP - Handler Pattern Enforcer
- MB - Module Boundary Checker
- TG - Test Generator

### 2. 保持指令编号连续

同一 Agent 的指令编号应该连续，避免跳号。

### 3. 验证生成的 YAML

生成后使用 YAML 验证工具确保格式正确：

```bash
# 使用 yamllint 验证
yamllint .github/instructions/*.yaml

# 或使用 yq 验证
yq eval '.instructions[].id' your-file.yaml
```

### 4. 定期更新

当 RuleSet 发生变化时，重新生成 Agent Instructions 以保持同步。

## 相关文档

- [Instructions 文件规范](.github/INSTRUCTIONS-SCHEMA.md)
- [ADR Decision Generator](./README.md)
- [ArchitectureRuleSet](../Rules/README.md)

## 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|---------|
| 1.0 | 2026-02-12 | 初始实现：完整的 RuleSet → YAML 转换功能 |

## 维护责任

**责任人**：架构委员会  
**审核周期**：每季度  
**状态**：✅ Active
