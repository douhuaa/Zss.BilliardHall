---
adr: ADR-122
title: "ArchitectureTests 命名与组织规范（ARD）"
status: Superseded
level: Governance
version: "1.0"
deciders: "Architecture Board"
date: 2026-01-28
maintainer: "Architecture Board"
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: ADR-903
---

# ADR-122：ArchitectureTests 命名与组织规范（ARD）

**版本**：1.0

> ⚖️ 本 ADR 的全部裁决已被 ADR-903 吸收并强化，不再具有独立裁决力。

---

## Focus（聚焦内容）

统一 ArchitectureTests 的命名、组织结构与 ADR 映射方式，确保：
- 架构规则可追溯到具体 ADR
- 测试失败信息直接指向被违反的 ADR
- CI 能进行自动治理裁决

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| ArchitectureTests | 用于验证架构约束、依赖关系、分层规则的测试集合 | ArchitectureTests |
| ARD | ADR 在测试层的可执行映射形式 | Architecture Rule Definition |
| RuleId | 与 ADR 条目一一对应的规则编号（如 ADR-240.1） | RuleId |

---

## Decision（裁决）

> ⚠️ **本节是唯一裁决来源，其他章节不得产生新规则。**

### ADR-122.1:L1 ArchitectureTests 必须按 ADR 编号分组

- ArchitectureTests 必须以 ADR 编号作为最小组织单元
- 每个 ADR 至少对应一个测试类或测试文件

**结构示例**：

```text
Tests/
└── ArchitectureTests/
    ├── ADR_240_HandlerRules/
    │   ├── ADR_240_1_HandlerExceptionRuleTests.cs
    │   └── ADR_240_2_HandlerDependencyRuleTests.cs
    └── ADR_340_LoggingRules/
        └── ADR_340_1_LoggingScopeRuleTests.cs
```

---

### ADR-122.2:L1 测试类命名必须显式包含 ADR 编号与条目号

**规则**：
- 测试类命名格式必须为：ADR_<ADR编号>_<条目号>_<RuleName>Tests
- 禁止使用模糊名称或隐藏 ADR 编号

```csharp
public class ADR_240_1_HandlerMustNotCatchExceptionTests
```

禁止：
- 使用模糊名称（如 `HandlerRulesTests`）
- 隐藏 ADR 编号

---

### ADR-122.3:L1 每个测试类必须声明对应 ADR 条目

- 每个 ArchitectureTest 必须在注释或常量中声明其 ADR 对应关系

```csharp
/// <summary>
/// Enforces ADR-240.1: Handler 禁止捕获异常
/// </summary>
public class ADR_240_1_HandlerMustNotCatchExceptionTests
{
}
```

或：

```csharp
private const string RuleId = "ADR-240.1";
```

---

### ADR-122.4:L1 一个测试类只允许验证一个 ADR 条目

**规则**：
- 禁止一个测试类覆盖多个 ADR 条目
- 禁止在同一测试中混合不同 ADR 的规则

> ADR 是治理裁决单元，测试必须与裁决单元一一对应。

---

### ADR-122.5:L1 测试失败信息必须可反向定位 ADR

**规则**：
- 测试断言失败信息必须包含 ADR 编号

**示例**：
```csharp
.Should().BeTrue("违反 ADR-240.1：Handler 不允许捕获异常");
```

---

### ADR-122.6:L1 被废弃 ADR 的测试必须同步处理

**规则**：
- ADR 状态变为 Superseded / Deprecated 时，对应 ArchitectureTests 必须删除或标记为 [Obsolete] 并指向替代 ADR
- 禁止出现“孤儿测试”或无 ADR 来源的架构测试

---

## Enforcement（执法模型）

> 本节为唯一架构执法映射表，所有必测/必拦规则均需在此列明。

| 规则编号 | 执行级 | 执法方式 | 关键测试用例/流程 | 必须遵守 |
|---------|------|----------------|--------------------------|------|
| ADR-122.1 | L1   | 目录结构自动化校验 | Test_Directory_Structure_Must_Mirror_ADR | ✅    |
| ADR-122.2 | L1   | 类名自动化校验 | Test_Class_Naming_Must_Contain_ADR | ✅    |
| ADR-122.3 | L1   | 注释/常量自动化校验 | Test_Class_Must_Declare_RuleId | ✅    |
| ADR-122.4 | L1   | 单一 ADR 校验 | Test_Class_Must_Only_Validate_One_ADR | ✅    |
| ADR-122.5 | L1   | 断言消息自动化校验 | Test_Assertion_Message_Must_Contain_ADR | ✅    |
| ADR-122.6 | L1   | ADR 状态变更自动化校验 | Test_Obsolete_Or_Delete_On_ADR_Superseded | ✅    |

---

## Non-Goals（明确不管什么）

- 单元测试 / 集成测试命名
- 具体断言库选型
- 架构规则本身的内容设计

---

## Prohibited（禁止行为）

- 无 ADR 编号的 ArchitectureTests
- 一个测试类对应多个 ADR
- ADR 已删除但测试仍存在
- 使用自然语言描述而无 RuleId

---

## Relationships（关系声明）

**上位裁决（Supersedes）**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md)

**Depends On**：
- [ADR-902：ADR 结构与章节规范](../governance/ADR-902-adr-template-structure-contract.md)

**Depended By**：
- 所有包含 Enforcement 章节的 ADR

---

## References（非裁决性参考）

- NetArchTest.Rules
- xUnit / NUnit Architecture Test Practices

---

## History（版本历史）

| 版本  | 日期         | 变更说明       | 修订人 |
|-----|------------|------------|-----|
| 1.0 | 2026-01-28 | 首次发布（Final） | GitHub Copilot |
