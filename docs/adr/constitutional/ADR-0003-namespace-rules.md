---
adr: ADR-0003
title: "命名空间与项目边界规范"
status: Final
level: Constitutional
deciders: "Architecture Board"
date: 2026-01-29
version: "2.0"
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---


# ADR-0003：命名空间与项目边界规范

> ⚖️ **本 ADR 是架构宪法的核心，定义命名空间与项目边界的唯一裁决源。**

---

## Focus（聚焦内容）

仅定义适用于全生命周期自动化裁决/阻断的**命名空间约束**：

- BaseNamespace 固定与统一定义
- 目录结构与 RootNamespace 自动映射
- 项目命名与命名空间边界原则
- 所有规则必须架构测试覆盖

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|-----------------------|--------------------------------------|----------------------|
| BaseNamespace         | 公司+产品根命名空间（`Zss.BilliardHall`）       | Base Namespace       |
| RootNamespace         | 项目根命名空间，由BaseNamespace及目录自动推导        | Root Namespace       |
| Directory.Build.props | MSBuild全局配置文件，统一定义BaseNamespace      | Directory.Build.props |
| MSBuild 推导            | 通过 MSBuild 条件和目录映射自动赋值 RootNamespace | MSBuild Inference    |

---

---

## Decision（裁决）

### 命名空间自动推导与一致性（ADR-0003.1, 0003.2, 0003.3, 0003.4, 0003.5）

**规则**：
- 所有项目必须通过 Directory.Build.props 定义 BaseNamespace
- 目录结构直接推导 RootNamespace
- 类型命名空间必须与项目层级匹配

**判定**：
- ❌ 类型命名空间不以 BaseNamespace 开头
- ❌ Platform 类型不以 `Zss.BilliardHall.Platform` 为前缀
- ❌ Application 类型不以 `Zss.BilliardHall.Application` 为前缀
- ❌ Module 类型不对应 `Zss.BilliardHall.Modules.{Name}`
- ❌ Host 类型不对应 `Zss.BilliardHall.Host.{Name}`
- ✅ 所有命名空间自动映射正确

### 防御规则（ADR-0003.6, 0003.7, 0003.8, 0003.9）

**规则**：
- Directory.Build.props 必须存在于仓库根目录
- Directory.Build.props 必须定义 BaseNamespace
- 项目命名需遵循命名空间映射
- 不得出现不规范命名空间（Common、Shared、Utils）

**判定**：
- ❌ 缺少 Directory.Build.props
- ❌ Directory.Build.props 未定义 BaseNamespace
- ❌ 项目命名不符合规范
- ❌ 存在不规范命名空间
- ✅ 所有配置和命名正确

---

---

## Enforcement（执法模型）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0003_Architecture_Tests.cs` 强制验证。

**有一项违规视为架构违规，CI 自动阻断。**

---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- 待补充

---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-0000
- [ADR-0002：平台、应用与主机启动器架构](./ADR-0002-platform-application-host-bootstrap.md) - 命名空间规范基于三层体系
- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)

**被依赖（Depended By）**：
- [ADR-0004：中央包管理与层级依赖规则](./ADR-0004-Cpm-Final.md) - 包管理依赖命名空间规范
- [ADR-0005：应用内交互模型与执行边界](./ADR-0005-Application-Interaction-Model-Final.md) - 运行时注册依赖命名空间映射
- [ADR-122：测试代码组织与命名规范](../structure/ADR-122-test-organization-naming.md)
- [ADR-121：契约（Contract）与 DTO 命名组织规范](../structure/ADR-121-contract-dto-naming-organization.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0006：术语与编号宪法](./ADR-0006-terminology-numbering-constitution.md) - 命名规范

---

---

## References（非裁决性参考）


- 待补充


---

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                         |
|-----|------------|----------------------------------------------|
| 2.0 | 2026-01-29 | 同步 ADR-902/940/0006 标准：添加 Front Matter、术语表英文对照 |
| 1.0 | 2026-01-26 | 裁决型重构，移除冗余                                   |

---
