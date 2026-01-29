---
adr: ADR-904
title: "ArchitectureTests 最小断言语义规范"
status: Superseded
level: Governance
version: "1.0"
deciders: "Architecture Board"
date: 2026-01-27
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: ADR-903-906
---


# ADR-904：ArchitectureTests 最小断言语义规范

> 本 ADR 的全部裁决已被 [ADR-903-906](ADR-903-906.md) 吸收并强化，不再具有独立裁决力。
> ⚖️ **本 ADR 规范 ArchitectureTests 的最小断言数量与语义要求，确保每条测试具备实际裁决能力，防止空测试或弱断言。**

---

## Focus（聚焦内容）

- 防止 ArchitectureTests 空洞或仅做形式验证
- 确保每条 ADR 的测试断言**真实反映规则内容**
- 定义最小有效断言数量和语义校验要求
- 支持 CI / Analyzer 自动化检测与反作弊

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 最小断言 | ArchitectureTests 中每个测试类必须包含的有效断言数量 | Minimum Assertions |
| 有效断言 | 断言必须验证架构约束或 ADR 条款，而非空断言或形式化断言 | Effective Assertion |
| 反作弊 | 自动检测空断言、弱断言或跳过逻辑，确保测试有效性 | Anti-Cheating |
| ADR 镜像 | 测试结构与 ADR 条目一一对应 | ADR Mirror |

---

## Decision（裁决）

> ⚠️ **本节是唯一裁决来源。**

### ADR-904.1:L1 每个 ArchitectureTest 类必须至少包含 1 个有效断言

**规则**：
- 每个测试类必须至少包含 **一个有效断言**，验证具体 ADR 条目
- 禁止空测试类、无 Assert、Try/Catch 吞异常形式

**示例**：
```csharp
[Fact]
public void HandlerMustNotCatchException()
{
    var handlerType = typeof(CreateOrderHandler);
    Assert.False(handlerType.GetMethods()
        .Any(m => m.CatchAllExceptions()), "ADR-240.1: Handlers must not catch exceptions");
}
````

**禁止**：
- 空测试类
- `Assert.True(true)` / `Assert.Pass()`
- `try { ... } catch {}` 吞异常

---

### ADR-904.2:L1 每个测试方法必须映射单个 ADR 子规则

**规则**：
- 一个测试方法 **只能映射一个 ADR 子规则**
- 禁止在同一方法中覆盖多个 ADR 条目

**示例**：
```csharp
ADR_240_1_HandlerMustNotCatchExceptionTests.Handle_ValidCommand_DoesNotCatchExceptions
```

---

### ADR-904.3:L2 测试断言必须语义清晰可反向溯源

**规则**：
- 所有断言失败信息必须包含 ADR 编号与规则描述
- CI 日志必须可以直接定位到 ADR 条目

**示例**：

```csharp
.Should().BeTrue("违反 ADR-240.1：Handler 不允许捕获异常");
```

**目的**：
- 失败即裁决，无需人工解释
- 支持破例机制和偿还计划

---

### ADR-904.4:L2 禁止弱断言或形式化断言

**规则**：
- 禁止仅验证 `true/false` 或类型存在
- 必须验证结构、依赖关系、约束内容

**示例**：
- ✅ 验证 Handler 是否为无状态
- ✅ 验证事件处理链是否符合 ADR-210.x
- ❌ 仅写 `Assert.True(true)` 或 `Assert.NotNull(someObject)`

---

## Enforcement（执法模型）

|规则编号|执行级|执法方式|
|---|---|---|
|ADR-904.1|L1|CI 自动扫描测试类断言数量|
|ADR-904.2|L1|测试方法命名映射 ADR 子规则|
|ADR-904.3|L2|断言消息内容扫描 + Code Review|
|ADR-904.4|L2|弱断言检测 + 半自动语义审查|

**执行时机**：
- CI 阶段自动阻断空或弱断言
- Code Review 发现语义不清断言
- 审计阶段确保 ADR 测试覆盖完整性

---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **单元测试和集成测试的编写规范**：不涉及业务逻辑测试的具体实现方式和测试方法论
- **测试框架的具体API使用**：不规定如何使用xUnit、NUnit等框架的具体API
- **测试覆盖率指标**：不设定代码覆盖率、分支覆盖率等具体数值目标
- **测试数据生成策略**：不涉及测试数据的创建、Mock对象的使用等具体技术
- **测试运行性能优化**：不涉及测试并行化、测试缓存等性能优化手段
- **测试报告的格式**：不规定测试结果报告的视觉呈现和输出格式
- **测试用例的命名规范**：不约束测试方法、测试类的具体命名风格
- **测试代码的组织结构**：不涉及测试项目的目录结构和文件组织方式

---

## Prohibited（禁止行为）

- 空测试类或空方法
- 弱断言（如 `Assert.True(true)`）
- 单方法覆盖多个 ADR
- 断言信息不包含 ADR 编号
- 弃用或删除的 ADR 测试未同步处理

---

## Relationships（关系声明）

**Depends On**：

- [ADR-903：ArchitectureTests 命名与组织规范](../governance/ADR-903-architecture-tests-naming-organization.MD)

**Depended By**：
- [ADR-906：Analyzer 与 CI Gate 映射协议](../governance/ADR-906-analyzer-ci-gate-mapping-protocol.md) - CI Gate 映射依赖断言语义规范
- 所有【必须架构测试覆盖】ADR
- CI / Analyzer 校验规则

**Related**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md)
- [ADR-122：测试代码组织与命名规范（ARD）](../structure/ADR-122-test-organization-naming.md)

---

## References（非裁决性参考）

- NetArchTest.Rules
- xUnit / NUnit 架构测试实践
- FluentAssertions 断言语义规范

---

## History（版本历史）

|版本|日期|说明|修订人|
|---|---|---|---|
|1.0|2026-01-28|初始正式版本|Architecture Board|
