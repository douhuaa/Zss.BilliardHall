# Registry 测试

## 📋 概述

本目录包含所有与规则注册表（RuleSetRegistry）和核心规则定义相关的测试，验证架构规则集的完整性和正确性。

## 🎯 职责范围

- **规则集注册**：验证 RuleSetRegistry 的查询和注册功能
- **规则集结构**：验证每个 ADR 对应的 RuleSet 结构完整性
- **规则定义验证**：验证核心业务规则定义与 ADR 规范的匹配性

## 📁 测试文件

| 测试文件 | 职责 | 测试数量 |
|---------|------|---------|
| `RuleSetRegistry_Tests.cs` | 注册表查询和验证功能 | ~40 |
| `ArchitectureRules_Tests.cs` | 核心规则定义验证 | ~15 |
| `ArchitectureRuleSetInvariants_Tests.cs` | RuleSet 结构不变量 | ~11 |

## 🔍 关键概念

### RuleSetRegistry

`RuleSetRegistry` 是所有架构规则集的中央注册表，提供：

1. **查询功能**：
   - `Get(int adrNumber)`：宽容模式，返回 null 如果不存在
   - `GetStrict(int adrNumber)`：严格模式，抛出异常如果不存在
   - `GetAllAdrNumbers()`：获取所有已注册的 ADR 编号

2. **验证功能**：
   - `Exists(int adrNumber)`：检查 ADR 是否存在
   - 确保每个 ADR 都有唯一对应的 RuleSet

### ArchitectureRuleSet

每个 `ArchitectureRuleSet` 包含：
- **ADR 编号**：标识来源 ADR 文档
- **规则列表**：Rule 和 Clause 的集合
- **元数据**：决策级别、严重性、作用域等

## 🧪 测试示例

```csharp
// 注册表查询测试
[Theory]
[InlineData(1)]
[InlineData(907)]
public void Get_Int_Should_Return_RuleSet_When_Exists(int adrNumber)
{
    var ruleSet = RuleSetRegistry.Get(adrNumber);
    AssertRuleSetExists(ruleSet, adrNumber);
}

// 规则结构完整性测试
[Theory]
[MemberData(nameof(AllAdrNumbers))]
public void RuleSet_Should_Maintain_Structural_Integrity(int adrNumber)
{
    var ruleSet = RuleSetRegistry.GetStrict(adrNumber);
    RuleSetValidator.ValidateFull(ruleSet, adrNumber);
}

// 核心规则定义测试
[Theory]
[InlineData(1, 1, "模块物理隔离", RuleSeverity.Constitutional)]
[InlineData(900, 1, "架构裁决权威性", RuleSeverity.Governance)]
public void Core_Rules_Should_Match_Specification(
    int adr, int ruleNum, string summary, RuleSeverity? severity)
{
    var ruleSet = RuleSetRegistry.GetStrict(adr);
    var rule = ruleSet.GetRule(ruleNum);
    
    rule.Summary.Should().Contain(summary);
    if (severity.HasValue)
        rule.Severity.Should().Be(severity.Value);
}
```

## 📊 已注册的 ADR

当前已注册的核心 ADR 包括：
- **ADR-001**：模块化单体与垂直切片架构
- **ADR-002**：Platform/Application/Host 三层启动体系
- **ADR-003**：命名空间与项目边界规范
- **ADR-004**：中央包管理 (CPM) 规范
- **ADR-005**：应用内交互模型与执行边界
- **ADR-900**：架构测试与 CI 治理元规则
- **ADR-907**：ArchitectureTests 执法治理体系

## 📚 相关资源

- **ADR-900**：定义了架构测试元规则和 RuleSet 体系
- **ADR-907**：定义了 ArchitectureTests 的执法治理体系
- **Shared/Testing/RuleSetValidator.cs**：RuleSet 验证辅助类
