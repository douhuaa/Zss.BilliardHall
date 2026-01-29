---
adr: ADR-0002
title: "Platform / Application / Host 三层启动体系"
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

# ADR-0002：Platform / Application / Host 三层启动体系

> ⚖️ **本 ADR 是架构宪法的核心，定义三层启动体系的唯一裁决源。**

---

## Focus（聚焦内容）

仅定义适用于全生命周期自动化裁决/阻断的**三层装配约束**：

- Platform / Application / Host 职责分明
- 层级依赖方向：唯一单向依赖（Host → Application → Platform）
- 每层必须有唯一 Bootstrapper 入口
- Program.cs 极简化（≤30行）
- 所有规则必须架构测试覆盖

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|--------------|--------------------------------|------------------|
| Platform     | 技术基座，仅提供技术能力，不感知业务             | Platform Layer   |
| Application  | 应用装配层，定义"系统是什么"，聚合模块和用例        | Application Layer |
| Host         | 进程外壳，决定"怎么跑"，如 Web/Worker/Test | Host Layer       |
| Bootstrapper | 唯一的装配入口，负责注册服务和配置              | Bootstrapper     |
| 单向依赖         | Host → Application → Platform  | Unidirectional Dependency |

---

## Decision（裁决）

### Platform 层约束（ADR-0002.1, 0002.2, 0002.3, 0002.4）

**规则**：
- 只提供通用技术能力（日志、追踪、异常、序列化）
- 不感知任何业务（不可访问 Application、Host、Modules）
- 必须有唯一入口 `PlatformBootstrapper.Configure`

**判定**：
- ❌ Platform 依赖 Application/Host/Modules
- ❌ Platform 缺少唯一 Bootstrapper 入口
- ✅ 仅包含技术基础设施

### Application 层约束（ADR-0002.5, 0002.6, 0002.7, 0002.8）

**规则**：
- 负责系统能力的装配和集成
- 禁止依赖 Host
- 必须有唯一入口 `ApplicationBootstrapper.Configure`
- 不包含 HttpContext 等 Host 专属类型

**判定**：
- ❌ Application 依赖 Host/Modules
- ❌ Application 使用 HttpContext
- ❌ Application 缺少唯一 Bootstrapper 入口
- ✅ 仅做模块装配和集成

### Host 层约束（ADR-0002.9, 0002.10, 0002.11, 0002.12, 0002.13）

**规则**：
- 唯一职责：调用 Platform、Application 的 Bootstrapper
- 决定进程模型，不包含任何业务逻辑
- Program.cs 保持极简（建议 ≤30 行）
- 项目命名为 `Zss.BilliardHall.Host.*`

**判定**：
- ❌ Host 依赖 Modules
- ❌ Host 包含业务类型
- ❌ Host 项目文件引用 Modules
- ❌ Program.cs 超过 30 行
- ❌ Program.cs 做了 Bootstrapper 以外的事
- ✅ 仅调用两个 Bootstrapper

### 三层依赖方向验证（ADR-0002.14）

**规则**：
- 完整的单向依赖链：Host → Application → Platform

**判定**：
- ❌ 任何反向依赖
- ✅ 严格的单向依赖流

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-0000
- [ADR-0001：模块化单体与垂直切片架构](./ADR-0001-modular-monolith-vertical-slice-architecture.md) - 三层体系与模块组织配合定义系统结构

**被依赖（Depended By）**：
- [ADR-0003：命名空间与项目结构规范](./ADR-0003-namespace-rules.md) - 命名空间规范基于三层体系
- [ADR-0004：中央包管理与层级依赖规则](./ADR-0004-Cpm-Final.md) - 包依赖规则基于三层依赖方向
- [ADR-0005：应用内交互模型与执行边界](./ADR-0005-Application-Interaction-Model-Final.md) - 运行时交互基于三层装配
- [ADR-123：Repository 接口与分层命名规范](../structure/ADR-123-repository-interface-layering.md)
- [ADR-340：结构化日志与监控约束](../technical/ADR-340-structured-logging-monitoring-constraints.md)
- [ADR-920：示例代码治理宪法](../governance/ADR-920-examples-governance-constitution.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0006：术语与编号宪法](./ADR-0006-terminology-numbering-constitution.md) - 层级命名规范

---

## 快速参考表

| 约束编号        | 约束描述                            | 测试方式           | 测试用例                                                    | 必须遵守 |
|-------------|-------------------------------- |-----------------|---------------------------------------------------------|------|
| ADR-0002.1  | Platform 不应依赖 Application        | L1 - NetArchTest | Platform_Should_Not_Depend_On_Application               | ✅    |
| ADR-0002.2  | Platform 不应依赖 Host               | L1 - NetArchTest | Platform_Should_Not_Depend_On_Host                      | ✅    |
| ADR-0002.3  | Platform 不应依赖任何 Modules          | L1 - NetArchTest | Platform_Should_Not_Depend_On_Modules                   | ✅    |
| ADR-0002.4  | Platform 应有唯一 Bootstrapper 入口   | L1 - NetArchTest | Platform_Should_Have_Single_Bootstrapper_Entry_Point    | ✅    |
| ADR-0002.5  | Application 不依赖 Host             | L1 - NetArchTest | Application_Should_Not_Depend_On_Host                   | ✅    |
| ADR-0002.6  | Application 不依赖 Modules          | L1 - NetArchTest | Application_Should_Not_Depend_On_Modules                | ✅    |
| ADR-0002.7  | Application 应有唯一 Bootstrapper 入口 | L1 - NetArchTest | Application_Should_Have_Single_Bootstrapper_Entry_Point | ✅    |
| ADR-0002.8  | Application 不含 Host 专属类型         | L1 - NetArchTest | Application_Should_Not_Use_HttpContext                  | ✅    |
| ADR-0002.9  | Host 不依赖 Modules                 | L1 - NetArchTest | Host_Should_Not_Depend_On_Modules                       | ✅    |
| ADR-0002.10 | Host 不含业务类型                      | L1 - NetArchTest | Host_Should_Not_Contain_Business_Types                  | ✅    |
| ADR-0002.11 | Host 项目文件不应引用 Modules            | L1 - 项目文件扫描      | Host_Csproj_Should_Not_Reference_Modules                | ✅    |
| ADR-0002.12 | Program.cs 建议 ≤30行               | L1 - 文件扫描        | Program_Cs_Should_Be_Concise                            | ✅    |
| ADR-0002.13 | Program.cs 只应调用 Bootstrapper     | L1 - 语义检查        | Program_Cs_Should_Only_Call_Bootstrapper                | ✅    |
| ADR-0002.14 | 三层唯一依赖方向验证                       | L1 - NetArchTest | Verify_Complete_Three_Layer_Dependency_Direction        | ✅    |

> **级别说明**：L1=静态自动化（ArchitectureTests）

---

## Enforcement（执法模型）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0002_Architecture_Tests.cs` 强制验证。

**有一项违规视为架构违规，CI 自动阻断。**

---

## 检查清单

- [ ] Platform 是否只提供技术能力，不依赖业务？
- [ ] Application 只做装配，无 Host/业务依赖？
- [ ] Host 仅负责装配和启动，无业务逻辑、无服务注册？
- [ ] Program.cs 是否极简、只调用 Bootstrapper？
- [ ] 每一层均有唯一 Bootstrapper 入口定义？
- [ ] 所有依赖方向只允许单向流动？

---


---

## References（非裁决性参考）


- 待补充


---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充


---

## Non-Goals（明确不管什么）


本 ADR 明确不涉及以下内容：

- 待补充


## History（版本历史）

| 版本  | 日期         | 变更说明                                         |
|-----|------------|----------------------------------------------|
| 2.0 | 2026-01-29 | 同步 ADR-902/940/0006 标准：添加 Front Matter、术语表英文对照 |
| 1.0 | 2026-01-26 | 裁决型重构，移除冗余                                   |

---

## 附注

本文件禁止添加示例/建议/FAQ/背景说明，仅维护自动化可判定的架构红线。

非裁决性参考（目录结构、命名规范、详细示例）请查阅：
- [ADR-0002 Copilot Prompts](../../copilot/adr-0002.prompts.md)
- 工程指南（如有）
