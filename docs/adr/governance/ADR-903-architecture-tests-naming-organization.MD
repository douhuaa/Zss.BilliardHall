---
adr: ADR-903
title: "ArchitectureTests 命名与组织规范"
status: Superseded
level: Governance
version: "1.0"
deciders: "Architecture Board"
date: 2026-01-28
maintainer: "Architecture Board"
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: ADR-903-906
---

# ADR-903：ArchitectureTests 命名与组织规范

> 本 ADR 的全部裁决已被 [ADR-903-906](ADR-903-906.md) 吸收并强化，不再具有独立裁决力。
> ⚖️ **这是架构治理的“执行侧接口定义”。本 ADR 只裁定架构测试是否“长得合法”，是否具备执法资格。**

---

## Focus（聚焦内容）

本 ADR 聚焦解决以下问题：
- ArchitectureTests **无法反向定位 ADR**
- 测试命名随意，**CI 无法自动审计**
- 多条 ADR 混在一个测试里，**裁决不可追责**
- 架构测试“存在但不可置信”（反作弊失效）

**本 ADR 的唯一目标**：
> **让 ArchitectureTests 成为 ADR 的“可执行镜像”，而不是装饰品。**

---

## Glossary（术语表）

| 术语                | 定义                 |
| ----------------- | ------------------ |
| ArchitectureTests | 用于验证 ADR 架构约束的测试集合 |
| ADR 镜像            | 测试结构与 ADR 结构一一对应   |
| 裁决可溯源             | 测试失败可明确定位到 ADR.X   |
| 反作弊               | 防止空断言、合并规则、跳过测试    |

---

## Decision（裁决）

> ⚠️ **本节是唯一裁决来源。**
>
> 每条规则必须为独立三级标题：
>
> `### ADR-903.X:L? <规则标题>`

---

### ADR-903.1:L1 架构测试必须集中在独立项目

**规则**：
所有 ArchitectureTests **必须**位于独立测试项目中。

**强制命名格式**：
```
<SolutionName>.Tests.Architecture
```

**示例**：
```
Zss.BilliardHall.Tests.Architecture
```

**禁止**：
- ❌ 与 Unit / Integration Tests 混合
- ❌ 多个 ArchitectureTests 项目
- ❌ 使用 Shared/Common 测试项目

---

### ADR-903.2:L1 测试目录必须以 ADR 编号为一级单位

**规则**：
测试目录结构 **必须**以 ADR 编号为一级目录。
```
/ADR-XXXX/
```

**示例**：
```
/ADR-0000/
/ADR-0005/
/ADR-0902/
```

**约束**：
- 一个 ADR **必须且只能**对应一个目录
- 不存在 ADR 的目录 **非法**

---

### ADR-903.3:L1 禁止跨 ADR 混合测试

**规则**：
单个测试类、测试文件 **不得**覆盖多个 ADR。

**禁止示例**：
```
ADR-0005-And-0006_Tests.cs
CQRS_Rules_Tests.cs
CommonArchitectureTests.cs
```

**理由**：
- 混合测试 = 裁决责任不清
- CI 无法精确失败定位
- 无法执行破例与偿还机制

---

### ADR-903.4:L1 测试类命名必须显式绑定 ADR

**规则**：
测试类名 **必须**包含 ADR 编号。

**强制格式**：
```
ADR_<编号>_Architecture_Tests
```

**示例**：
```csharp
ADR_0005_Architecture_Tests
ADR_0902_Architecture_Tests
```

**禁止**：
- ❌ 省略 ADR 编号
- ❌ 使用业务语义代替 ADR

---

### ADR-903.5:L1 测试方法必须映射 ADR 子规则

**规则**：
每个测试方法 **必须**映射到具体 `ADR-XXX.X`。

**命名格式**：
```
ADR_<编号>_<子规则>_<行为描述>
```

**示例**：

```csharp
ADR_0005_1_Handlers_Must_Be_Stateless
ADR_0000_2_Architecture_Tests_Must_Not_Be_Skipped
```

**禁止**：

- ❌ 一个测试方法覆盖多个子规则
- ❌ 模糊命名（ValidateAll / CheckRules）

---

### ADR-903.6:L1 测试失败信息必须包含 ADR 编号

**规则**：
所有断言失败消息 **必须**显式包含 ADR 编号。

**示例**：
```csharp
.Should().BeTrue("ADR-0005.1: Handlers must be stateless");
```

**目的**：
- CI 日志即裁决书
- 失败即定位，无需人工解释

---

### ADR-903.7:L1 架构测试不得为空或弱断言

**规则**：
每个 ArchitectureTests 类 **必须**满足最小断言数量（≥1 有效结构断言）。

**禁止**：
- ❌ 空测试类
- ❌ Assert.True(true)
- ❌ try/catch 吞异常

---

### ADR-903.8:L2 架构测试不得被跳过或条件禁用

**规则**：
- 禁止 `[Skip]`
- 禁止运行时条件禁用
- 禁止环境变量绕过

**例外**：
- 仅允许通过 ADR-0000 定义的 **破例机制**

---

## Enforcement（执法模型）

> 本节为唯一架构执法映射表，所有必测/必拦规则均需在此列明。

| 规则        | 执行级 | 执法方式         |
| --------- | --- | ------------ |
| ADR-903.1 | L1  | CI 校验项目命名    |
| ADR-903.2 | L1  | 目录结构扫描       |
| ADR-903.3 | L1  | 测试类-ADR 映射校验 |
| ADR-903.4 | L1  | 测试类命名正则      |
| ADR-903.5 | L1  | 测试方法命名解析     |
| ADR-903.6 | L1  | 失败消息扫描       |
| ADR-903.7 | L2  | 断言数量/语义审计    |
| ADR-903.8 | L1  | Skip/条件检测    |

---

## Non-Goals（明确不管什么）

- 架构规则内容本身
- 业务正确性
- 测试框架选型
- 测试性能优化
- 代码风格与美学

---

## Prohibited（禁止行为）

- ArchitectureTests 作为“示意性存在”
- 通过命名技巧规避 ADR 绑定
- 合并多条 ADR 到一个测试
- 跳过或弱化测试以“先过 CI”

---

## Relationships（关系声明）

**Depends On**：

- [ADR-0000：架构测试与 CI 治理宪法](./ADR-0000-architecture-tests.md)
- [ADR-902：ADR 模板与结构契约](./ADR-902-adr-template-structure-contract.md)

**Depended By**：

- 所有【必须架构测试覆盖】的 ADR

**Related**：

- [ADR-905：执行级别分类](./ADR-905-enforcement-level-classification.md)

---

## History（版本历史）

| 版本  | 日期         | 说明     |
| --- | ---------- | ------ |
| 1.0 | 2026-01-28 | 初始正式版本 |
