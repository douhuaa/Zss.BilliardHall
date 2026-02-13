# RuleId 测试

## 📋 概述

本目录包含所有与 `ArchitectureRuleId` 核心功能相关的测试，验证 RuleId 的解析、表示、排序和身份不变量。

## 🎯 职责范围

- **RuleId 解析**：验证从字符串格式（如 `ADR-907_3_1`）到结构化对象的转换
- **RuleId 表示**：验证 RuleId 的字符串表示形式符合规范
- **RuleId 排序**：验证 RuleId 的自然排序逻辑（按 ADR → Rule → Clause）
- **身份不变量**：验证 RuleId（规则级别）和 ClauseId（条款级别）的级别标识

## 📁 测试文件

| 测试文件 | 职责 | 测试数量 |
|---------|------|---------|
| `RuleIdParser_Tests.cs` | RuleId 字符串解析功能 | ~30 |
| `ArchitectureRuleIdParsingInvariants_Tests.cs` | 解析不变量和边界条件 | ~15 |
| `ArchitectureRuleIdRepresentationInvariants_Tests.cs` | 字符串表示不变量 | ~20 |
| `ArchitectureRuleIdOrderingInvariants_Tests.cs` | 排序逻辑不变量 | ~15 |
| `ArchitectureRuleIdIdentityInvariants_Tests.cs` | 身份级别不变量 | ~16 |

## 🔍 关键概念

### RuleId 格式规范

RuleId 使用 `ADR-XXX_Y_Z` 格式：
- `XXX`：ADR 编号（如 907）
- `Y`：规则编号（Rule Number）
- `Z`：条款编号（Clause Number，可选）

**示例**：
- `ADR-907_3`：ADR-907 的规则 3（规则级别）
- `ADR-907_3_1`：ADR-907 的规则 3 的条款 1（条款级别）

### 级别标识

- **Rule Level**（规则级别）：`ClauseNumber == null`
- **Clause Level**（条款级别）：`ClauseNumber != null`

## 🧪 测试示例

```csharp
// 解析测试
[Theory]
[InlineData("ADR-907_3_1", 907, 3, 1)]
public void Parse_ValidClauseId_ReturnsCorrectComponents(
    string input, int expectedAdr, int expectedRule, int expectedClause)
{
    var ruleId = RuleIdParser.Parse(input);
    ruleId.AdrNumber.Should().Be(expectedAdr);
    ruleId.RuleNumber.Should().Be(expectedRule);
    ruleId.ClauseNumber.Should().Be(expectedClause);
}

// 不变量测试
[Fact]
public void RuleId_Should_Always_Be_Rule_Level()
{
    var id = ArchitectureRuleId.Rule(907, 3);
    RuleIdAssertions.AssertIsRule(id, context: "RuleId(907, 3)");
}
```

## 📚 相关资源

- **ADR-907**：定义了 RuleId 格式规范和编号系统
- **ADR-907-A**：定义了 RuleId 对齐执行标准
- **Shared/Testing/RuleIdAssertions.cs**：RuleId 断言辅助类
