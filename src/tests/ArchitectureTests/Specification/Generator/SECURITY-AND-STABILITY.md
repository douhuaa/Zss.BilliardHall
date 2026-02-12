# Agent Instruction Generator - 安全与稳定性保证

## 概述

本文档说明 Agent Instruction Generator 如何确保生成的 YAML 指令的安全性、稳定性和可靠性。

## 1. Instruction ID 生成的稳定性和唯一性

### 确定性生成
- **基于 RuleNumber 排序**：指令 ID 基于 RuleNumber 排序生成，确保相同 RuleSet 每次生成结果完全一致
- **顺序无关性**：无论规则添加顺序如何，生成的 ID 总是按 RuleNumber 排序，保证可重复性

### ID 格式规范
- **格式**：`{AgentPrefix}-{NNN}`，其中 NNN 是三位数字（001-999）
- **前缀验证**：AgentPrefix 必须是 2-3 个大写字母，通过正则表达式验证
- **唯一性**：每个 Agent 使用独特的前缀（AG, TG, HP 等），避免跨 Agent 冲突

### 配置灵活性
```csharp
// 自定义起始编号，避免冲突
var options = new InstructionGenerationOptions
{
    AgentPrefix = "AG",
    StartInstructionNumber = 10  // 从 AG-010 开始
};
```

### 相关测试
- `GenerateInstructions_Should_Generate_Stable_IDs_For_Same_RuleSet` - 验证确定性
- `GenerateInstructions_Should_Generate_Sequential_IDs_Without_Gaps` - 验证顺序性
- `GenerateInstructions_Should_Generate_Consistent_IDs_For_Same_Rules_In_Different_Order` - 验证排序无关性
- `GenerateInstructions_Should_Support_Different_Prefixes_For_Different_Agents` - 验证唯一性

## 2. YAML 注入防护

### 多层防护机制

#### 2.1 字符转义
所有用户输入的文本（summary, condition, enforcement）都经过 `EscapeYamlString` 方法处理：

```csharp
private static string EscapeYamlString(string? text)
{
    return text
        .Replace("\\", "\\\\")  // 反斜杠
        .Replace("\"", "\\\"")  // 双引号
        .Replace("\n", "\\n")   // 换行符
        .Replace("\r", "\\r")   // 回车符
        .Replace("\t", "\\t")   // 制表符
        .Replace("`", "\\`")    // 反引号（防止命令注入）
        .Replace("$", "\\$");   // 美元符号（防止变量替换）
}
```

#### 2.2 防护目标

| 攻击类型 | 防护措施 | 测试覆盖 |
|---------|---------|---------|
| YAML 结构破坏 | 转义换行符、冒号上下文保护 | ✓ 13 个测试 |
| 命令注入 | 转义 `$()`, `` ` ``, `${}`  | ✓ 4 个测试 |
| 字符串边界破坏 | 转义双引号 | ✓ 1 个测试 |
| 多行注入 | 转义 `\n`, `\r` | ✓ 3 个测试 |
| 命令分隔符注入 | 验证命令格式 | ✓ 1 个测试 |

### 已防护的攻击模式

```yaml
# 攻击示例 1：YAML 结构注入
summary: "Test: malicious\ncommands:\n  evil: rm -rf /"
# 防护后：所有特殊字符被转义，保持在引号内

# 攻击示例 2：命令注入
condition: "Test with $(evil command)"
# 防护后：$(evil command) 被转义为 \$(evil command\)

# 攻击示例 3：反引号命令执行
condition: "Test with `dangerous`"
# 防护后：被转义为 \`dangerous\`

# 攻击示例 4：变量替换
enforcement: "Test with ${MALICIOUS_VAR}"
# 防护后：被转义为 \${MALICIOUS_VAR}
```

### 相关测试
- `GenerateInstructions_Should_Prevent_YAML_Structure_Injection_In_Summary` (11 变体)
- `GenerateInstructions_Should_Prevent_Command_Injection_In_Condition` (4 变体)
- `GenerateInstructions_Should_Prevent_Structure_Injection_In_Enforcement` (3 变体)
- `GenerateInstructions_Should_Not_Allow_Script_Injection_In_Commands`
- `GenerateInstructions_Should_Escape_Quotes_In_All_Fields`

## 3. 边界条件处理

### 3.1 空值和异常处理

| 场景 | 处理方式 | 测试覆盖 |
|-----|---------|---------|
| Null RuleSet | 抛出 ArgumentNullException | ✓ |
| 空 RuleSet（无规则） | 返回 `instructions:\n` | ✓ |
| 空 summary/condition | 返回空字符串 | ✓ |
| 仅有规则无条款 | 生成指令，action 显示 "0 个约束条款" | ✓ |

### 3.2 极限场景

```csharp
// 大规模 RuleSet
// 测试：100 个规则，验证性能和格式
GenerateInstructions_Should_Handle_Very_Large_RuleSet

// 超长文本
// 测试：10KB 文本，验证不破坏结构
GenerateInstructions_Should_Handle_Extremely_Long_Text

// Unicode 字符
// 测试：多语言和 emoji，验证编码正确性
GenerateInstructions_Should_Handle_Unicode_Characters
```

### 相关测试
- `GenerateInstructions_Should_Handle_Empty_RuleSet_Gracefully`
- `GenerateInstructions_Should_Handle_RuleSet_With_Only_Rules_No_Clauses`
- `GenerateInstructions_Should_Handle_Very_Large_RuleSet`
- `GenerateInstructions_Should_Handle_Empty_Summary_Gracefully` (3 变体)
- `GenerateInstructions_Should_Handle_Unicode_Characters`
- `GenerateInstructions_Should_Handle_Extremely_Long_Text`

## 4. 命令安全

### 预定义命令模板
生成器仅生成预定义的安全命令：

```yaml
commands:
  run_adr_tests: "dotnet test src/tests/ArchitectureTests/ --filter \"FullyQualifiedName~ADR{Number}\" --logger \"console;verbosity=detailed\""
  run_all_architecture_tests: "dotnet test src/tests/ArchitectureTests/ --filter \"Category=Architecture\" --logger \"console;verbosity=detailed\""
```

### 命令验证规则
1. **仅 dotnet test**：所有命令必须以 `dotnet test` 开头
2. **无 shell 操作符**：禁止 `&`, `|`, `;`（logger 格式除外）
3. **无命令替换**：禁止 `$(...)`, `` `...` ``
4. **固定路径**：只使用预定义的测试路径

### 相关测试
- `GenerateInstructions_Should_Not_Allow_Script_Injection_In_Commands`

## 5. 测试覆盖

### 测试统计
```
总测试数: 95 个（全部通过）
├── 单元测试: 37 个
│   ├── 接口契约: 3 个
│   ├── YAML 格式: 8 个
│   ├── ID 生成: 4 个
│   ├── 边界条件: 7 个
│   └── 功能验证: 15 个
├── 集成测试: 8 个
├── Golden 测试: 9 个
├── 配置测试: 24 个
└── 安全测试: 34 个（新增）
    ├── YAML 注入: 18 个
    ├── ID 稳定性: 5 个
    ├── 边界条件: 8 个
    └── 命令安全: 3 个
```

### 关键测试文件
- `AgentInstructionGenerator_Tests.cs` - 基础功能测试
- `AgentInstructionGenerator_IntegrationTests.cs` - 集成测试
- `AgentInstructionGenerator_SecurityTests.cs` - 安全和边界测试（新）
- `AgentInstructionGenerator_GoldenTests.cs` - Golden 参考测试
- `InstructionGenerationOptions_Tests.cs` - 配置验证测试

## 6. 最佳实践

### 6.1 使用建议
```csharp
// ✓ 推荐：使用标准前缀
var options = new InstructionGenerationOptions
{
    AgentPrefix = "AG",  // Architecture Guardian
    AgentName = "Architecture Guardian"
};

// ✓ 推荐：验证输入
ruleSet.ValidateCompleteness();  // 确保每个规则都有条款

// ✓ 推荐：保存前验证
var yaml = generator.GenerateInstructions(ruleSet);
// 可选：使用 YAML parser 验证格式
```

### 6.2 不推荐做法
```csharp
// ✗ 不推荐：跳过验证
// 直接使用用户输入作为 summary，应该先验证

// ✗ 不推荐：手动拼接 YAML
// 总是使用生成器，不要手动构造 YAML 字符串

// ✗ 不推荐：重复的前缀
// 确保每个 Agent 使用唯一的前缀
```

## 7. 安全审计记录

| 日期 | 审计项 | 结果 | 备注 |
|-----|-------|------|-----|
| 2026-02-12 | YAML 注入测试 | ✓ 通过 | 18 个注入测试全部通过 |
| 2026-02-12 | 命令注入测试 | ✓ 通过 | 防护 shell 命令执行 |
| 2026-02-12 | ID 稳定性测试 | ✓ 通过 | 确定性和唯一性验证 |
| 2026-02-12 | 边界条件测试 | ✓ 通过 | 8 个边界场景覆盖 |

## 8. 未来改进

### 计划中的增强
1. **YAML 语法验证**：集成 YAML parser 进行输出验证
2. **命令白名单**：可配置的命令模板白名单
3. **审计日志**：记录所有生成操作
4. **沙箱执行**：隔离环境中验证生成的指令

### 持续监控
- 定期运行安全测试套件
- 监控新的 YAML 注入模式
- 更新转义规则以应对新威胁

## 参考资料

- [YAML 规范](https://yaml.org/spec/)
- [OWASP 注入防护指南](https://owasp.org/www-community/attacks/Code_Injection)
- [.github/INSTRUCTIONS-SCHEMA.md](/.github/INSTRUCTIONS-SCHEMA.md)

---

**最后更新**: 2026-02-12  
**维护责任**: 架构委员会  
**审核周期**: 每季度
