# ADR-0003：命名空间与项目边界规范

**状态**：✅ Final（裁决型ADR）  
**版本**：1.0
**级别**：架构约束（Architectural Contract）  
**适用范围**：所有 Platform / Application / Modules / Host / Tests 项目  
**生效时间**：即刻

---

## 本章聚焦内容（Focus）

仅定义适用于全生命周期自动化裁决/阻断的**命名空间约束**：

- BaseNamespace 固定与统一定义
- 目录结构与 RootNamespace 自动映射
- 项目命名与命名空间边界原则
- 所有规则必须架构测试覆盖

---

## 术语表（Glossary）

| 术语                    | 定义                                   |
|-----------------------|--------------------------------------|
| BaseNamespace         | 公司+产品根命名空间（`Zss.BilliardHall`）       |
| RootNamespace         | 项目根命名空间，由BaseNamespace及目录自动推导        |
| Directory.Build.props | MSBuild全局配置文件，统一定义BaseNamespace      |
| MSBuild 推导            | 通过 MSBuild 条件和目录映射自动赋值 RootNamespace |

---

## 决策（Decision）

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

## 关系声明（Relationships）

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

## 快速参考表

| 约束编号       | 约束描述                            | 测试方式           | 测试用例                                                | 必须遵守 |
|------------|--------------------------------- |----------------|----------------------------------------------------|------|
| ADR-0003.1 | 所有类型命名空间以 BaseNamespace 开头       | L1 - NetArchTest | All_Types_Should_Start_With_Base_Namespace          | ✅    |
| ADR-0003.2 | Platform 类型以 Platform 命名空间为前缀    | L1 - NetArchTest | Platform_Types_Should_Have_Platform_Namespace       | ✅    |
| ADR-0003.3 | Application 类型以 Application 命名空间为前缀 | L1 - NetArchTest | Application_Types_Should_Have_Application_Namespace | ✅    |
| ADR-0003.4 | Modules 类型对应 Modules.{Name} 命名空间  | L1 - NetArchTest | Module_Types_Should_Have_Module_Namespace           | ✅    |
| ADR-0003.5 | Host 类型对应 Host.{Name} 命名空间        | L1 - NetArchTest | Host_Types_Should_Have_Host_Namespace               | ✅    |
| ADR-0003.6 | Directory.Build.props 必须位于仓库根目录   | L1 - 文件扫描        | Directory_Build_Props_Should_Exist_At_Repository_Root | ✅    |
| ADR-0003.7 | Directory.Build.props 定义 BaseNamespace | L1 - 文件扫描        | Directory_Build_Props_Should_Define_Base_Namespace  | ✅    |
| ADR-0003.8 | 项目命名需遵循命名空间映射                    | L1 - 文件扫描        | All_Projects_Should_Follow_Namespace_Convention     | ✅    |
| ADR-0003.9 | 不得出现不规范命名空间                      | L1 - NetArchTest | Modules_Should_Not_Contain_Irregular_Namespace_Patterns | ✅    |

> **级别说明**：L1=静态自动化（ArchitectureTests）

---

## 必测/必拦架构测试（Enforcement）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0003_Architecture_Tests.cs` 强制验证。

**有一项违规视为架构违规，CI 自动阻断。**

---

## 检查清单

- [ ] 是否用 Directory.Build.props 统一 BaseNamespace？
- [ ] 根命名空间是否由目录自动推导？
- [ ] 项目名与目录/二级命名空间是否严格一致？
- [ ] 全局无 Common、Shared、Utils 等命名空间？
- [ ] CI 与架构测试是否已自动检验命名空间合规？

---

## 版本历史

| 版本  | 日期         | 变更说明       |
|-----|------------|------------|
| 4.0 | 2026-01-26 | 裁决型重构，移除冗余 |
| 3.0 | 2026-01-22 | 完全去编号、结构升级 |
| 2.0 | 2026-01-20 | 机制细化       |
| 1.0 | 初版         | 初始发布       |

---

## 附注

本文件禁止添加示例/建议/FAQ/背景说明，仅维护自动化可判定的架构红线。

非裁决性参考（MSBuild 推导逻辑、项目配置示例）请查阅：
- [ADR-0003 Copilot Prompts](../../copilot/adr-0003.prompts.md)
- 工程指南（如有）
