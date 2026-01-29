---
adr: ADR-0004
title: "中央包管理（CPM）规范"
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

# ADR-0004：中央包管理（CPM）规范

> ⚖️ **本 ADR 是架构宪法的核心，定义中央包管理的唯一裁决源。**

---

## Focus（聚焦内容）

仅定义适用于全生命周期自动化裁决/阻断的**包管理约束**：

- 所有依赖包通过 Directory.Packages.props 集中管理
- 层级依赖规则细化：Platform、Application、Modules、Host 各自边界
- 禁止项目文件手动指定包版本
- 所有规则必须架构测试覆盖

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|--------------------------|----------------------------------|-----------------------|
| CPM                      | Central Package Management，中央包管理 | CPM                   |
| Directory.Packages.props | NuGet 配置文件，集中定义全局依赖包版本           | Directory.Packages.props |
| 传递依赖固定                   | 通过 CPM 禁止传递依赖的漂移                 | Transitive Dependency Lock |
| 层级依赖                     | 不同层项目允许的包类型约束                    | Layered Dependencies  |
| 包分组                      | 依赖包按技术栈、场景分隔分组                   | Package Grouping      |

---

## Decision（裁决）

### 包集中管理与防御（ADR-0004.1, 0004.2, 0004.3, 0004.9）

**规则**：
- 必须启用 CPM
- Directory.Packages.props 必须存在
- 禁止项目文件手动指定 Version
- 禁止私自覆盖中央包版本

**判定**：
- ❌ 未启用 CPM
- ❌ 缺少 Directory.Packages.props
- ❌ 项目文件包含 Version 属性
- ❌ 项目覆盖中央包版本
- ✅ 所有包版本集中管理

### 层级依赖规则（ADR-0004.4, 0004.8）

**规则**：
- Platform：仅技术底座包（Logging、OpenTelemetry、基础异常处理）
- Application：装配与 Pipeline 包（Wolverine、Marten）
- Modules：业务依赖、DTO、协议、契约
- Host：仅调用 Bootstrapper，不依赖业务包
- Tests：被测模块 + Platform/Application

**判定**：
- ❌ Platform 依赖业务包
- ❌ 层级依赖不符合规范
- ✅ 所有层级依赖正确

### 包分组与统一（ADR-0004.5, 0004.6, 0004.7）

**规则**：
- Directory.Packages.props 必须包含包分组
- 所有测试项目使用相同测试框架版本
- 所有使用的包必须集中声明

**判定**：
- ❌ Directory.Packages.props 缺少分组
- ❌ 测试框架版本不一致
- ❌ 存在未声明的包依赖
- ✅ 包分组完整、版本统一

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-0000
- [ADR-0002：平台、应用与主机启动器架构](./ADR-0002-platform-application-host-bootstrap.md) - 包管理规范基于层级装配边界
- [ADR-0003：命名空间与项目结构规范](./ADR-0003-namespace-rules.md) - 包管理依赖命名空间结构

**被依赖（Depended By）**：
- [ADR-0005：应用内交互模型与执行边界](./ADR-0005-Application-Interaction-Model-Final.md) - 运行时依赖语义基于包管理规则

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- 无

---

## 快速参考表

| 约束编号       | 约束描述                        | 测试方式           | 测试用例                                                  | 必须遵守 |
|------------|-----------------------------|-----------------|---------------------------------------------------------|------|
| ADR-0004.1 | 必须启用 CPM                    | L1 - 文件扫描        | CPM_Should_Be_Enabled                                   | ✅    |
| ADR-0004.2 | Directory.Packages.props 强制存在 | L1 - 文件扫描        | Repository_Should_Have_Directory_Packages_Props         | ✅    |
| ADR-0004.3 | 不允许项目文件手动指定版本               | L1 - 文件扫描        | Projects_Should_Not_Specify_Package_Versions            | ✅    |
| ADR-0004.4 | 层级依赖必须严格遵守                  | L1 - 语义检查        | Layer_Package_Dependencies_Should_Be_Valid              | ✅    |
| ADR-0004.5 | 包分组规范                       | L1 - 文件扫描        | Directory_Packages_Props_Should_Contain_Package_Groups  | ✅    |
| ADR-0004.6 | 测试项目使用相同测试框架版本              | L1 - 文件扫描        | All_Test_Projects_Should_Use_Same_Test_Framework_Versions | ✅    |
| ADR-0004.7 | 包依赖集中声明                     | L1 - 文件扫描        | Directory_Packages_Props_Should_Define_All_Used_Packages | ✅    |
| ADR-0004.8 | Platform 不得依赖业务包            | L1 - 文件扫描        | Platform_Projects_Should_Not_Reference_Business_Packages | ✅    |
| ADR-0004.9 | 禁止私自覆盖中央包版本                 | L1 - 文件扫描        | Projects_Should_Not_Override_Central_Package_Versions   | ✅    |

> **级别说明**：L1=静态自动化（ArchitectureTests）

---

## Enforcement（执法模型）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0004_Architecture_Tests.cs` 强制验证。

**有一项违规视为架构违规，CI 自动阻断。**

---

## 检查清单

- [ ] 是否使用 Directory.Packages.props 管理所有包？
- [ ] 项目文件无手动 Version？
- [ ] 各层引入的依赖包完全符合层级规则？
- [ ] 所有项目依赖包都在集中声明？
- [ ] 相关架构测试和 CI 校验被正确拦截？

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

非裁决性参考（包分组策略、版本管理建议）请查阅：
- [ADR-0004 Copilot Prompts](../../copilot/adr-0004.prompts.md)
- 工程指南（如有）
