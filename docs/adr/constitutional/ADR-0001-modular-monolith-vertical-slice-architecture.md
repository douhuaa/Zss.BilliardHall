---
adr: ADR-0001
title: "模块化单体与垂直切片架构"
status: Final
level: Constitutional
deciders: "Architecture Board"
date: 2026-01-29
version: "5.0"
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---

# ADR-0001：模块化单体与垂直切片架构

> ⚖️ **本 ADR 是架构宪法的核心，定义模块隔离与垂直切片的唯一裁决源。**

---

## Focus（聚焦内容）

仅定义适用于全生命周期自动化裁决/阻断的**模块隔离约束**：

- 模块按业务能力独立划分，物理隔离
- 垂直切片为最小业务组织单元
- 模块间禁止直接依赖与共享领域对象
- 模块通信仅通过领域事件、契约、原始类型
- 所有规则必须架构测试覆盖

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|---------------|--------------------------------|-------------------|
| 模块化单体         | 单进程，按业务能力独立模块，物理分离             | Modular Monolith  |
| 垂直切片          | 以单个业务用例为最小组织单元，端到端实现           | Vertical Slice    |
| 契约（Contract）  | 只读数据 DTO，仅用于模块间信息传递            | Contract          |
| 领域事件          | 描述业务事实，供其他模块异步订阅               | Domain Event      |
| 横向分层          | Controller/Service/Repository 抽象 | Horizontal Layers |
| 依赖隔离          | 模块内部不可被外部引用、反射或直接调用            | Dependency Isolation |

---

## Decision（裁决）

> ⚠️ **本节是唯一裁决来源，其他章节不得产生新规则。**

### ADR-0001.1:L1 模块按业务能力独立划分

- 模块按业务能力独立划分（如 Members/Orders）
- 每模块 = 独立程序集 = 清晰目录边界
- 模块间禁止直接引用代码、类型、资源

**判定**：
- ❌ 模块引用其他模块类型
- ✅ 仅通过契约、事件、原始类型通信

### ADR-0001.2:L1 项目文件禁止引用其他模块

- 项目文件（.csproj）不得包含对其他模块的 ProjectReference
- 确保模块在编译时物理隔离

**判定**：
- ❌ 项目文件引用其他模块
- ✅ 模块编译时独立

### ADR-0001.3:L2 垂直切片以用例为最小单元

- 用例（Use Case）为最小组织单元
- 每用例包含 Endpoint → Command/Query → Handler → 领域逻辑
- Handler 必须在 UseCases 命名空间

**判定**：
- ❌ Handler 不在 UseCases 命名空间
- ✅ 每用例自包含完整切片

### ADR-0001.4:L1 禁止横向 Service 抽象

- 禁止使用 Service/Manager/Helper 类承载业务逻辑
- 业务逻辑应在 Handler 或领域模型中

**判定**：
- ❌ 存在 *Service 类
- ✅ 业务逻辑在正确位置

### ADR-0001.5:L2 模块间通信仅允许事件/契约/原始类型

- 模块间仅允许：领域事件、契约 DTO、原始类型（Guid/string/int）
- 禁止直接依赖其他模块的 Entity/Aggregate/VO

**判定**：
- ❌ 直接依赖 Entity/Aggregate/VO
- ✅ 仅传递只读数据

### ADR-0001.6:L2 契约不含业务决策字段

- 契约 DTO 不含业务判断字段（如 CanRefund）
- 契约不含行为方法
- 契约仅用于数据传递

**判定**：
- ❌ 契约包含业务判断字段
- ✅ 契约仅传递数据

### ADR-0001.7:L1 命名空间匹配模块边界

- 命名空间必须与模块边界一致
- 目录结构必须反映模块隔离
- 确保命名空间不跨模块

**判定**：
- ❌ 命名空间不匹配模块边界
- ✅ 命名空间清晰隔离

---

## 快速参考表

| 约束编号       | 约束描述          | 测试方式                                         | 测试用例                                           | 必须遵守 |
|------------|---------------|----------------------------------------------|------------------------------------------------|------|
| ADR-0001.1 | 模块不可相互引用      | L1 - NetArchTest                             | Modules_Should_Not_Reference_Other_Modules     | ✅    |
| ADR-0001.2 | 项目文件禁止引用其他模块  | L1 - 项目文件扫描                                  | Module_Csproj_Should_Not_Reference_Other_Modules | ✅    |
| ADR-0001.3 | 垂直切片/用例为最小单元  | L2 - NetArchTest                             | Handlers_Should_Be_In_UseCases_Namespace       | ✅    |
| ADR-0001.4 | 禁止横向 Service 抽象 | L1 - NetArchTest                             | Modules_Should_Not_Contain_Service_Classes     | ✅    |
| ADR-0001.5 | 只允许事件/契约/原始类型通信 | L2 - 语义检查                                    | Contract_Rules_Semantic_Check                  | ✅    |
| ADR-0001.6 | Contract 不含业务判断字段 | L2/L3 - Roslyn分析 + 人工                        | Contract_Business_Field_Analyzer               | ✅    |
| ADR-0001.7 | 命名空间/目录强制隔离    | L1 - NetArchTest                             | Namespace_Should_Match_Module_Boundaries       | ✅    |

> **级别说明**：L1=静态自动化（ArchitectureTests），L2=语义半自动（Roslyn/启发式），L3=人工Gate

---

## Enforcement（执法模型）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0001_Architecture_Tests.cs` 强制验证：

- 模块不应相互引用（程序集级别）
- 模块项目文件不应引用其他模块
- 命名空间应匹配模块边界
- Handler 必须在 UseCases 命名空间
- 模块不应包含 Service 类
- 契约语义规则检查
- 契约业务字段分析

**有一项违规视为架构违规，CI 自动阻断。**

---

## 检查清单

- [ ] 模块按业务能力划分且物理完全隔离？
- [ ] 垂直切片以用例（UseCase）为唯一最小组织单元？
- [ ] 杜绝了横向 Service、领域模型共享、同步耦合？
- [ ] 模块间通信仅允许事件、契约和原始类型？
- [ ] 合约和事件定义无业务决策字段？
- [ ] 所有隔离规则有自动化测试覆盖？

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 测试执行机制

**被依赖（Depended By）**：
- [ADR-0002：平台、应用与主机启动器架构](./ADR-0002-platform-application-host-bootstrap.md) - 基于模块隔离规则
- [ADR-0003：命名空间与项目结构规范](./ADR-0003-namespace-rules.md) - 基于模块边界定义
- [ADR-0005：应用内交互模型与执行边界](./ADR-0005-Application-Interaction-Model-Final.md) - 基于模块通信约束
- [ADR-120：领域事件命名约定](../structure/ADR-120-domain-event-naming-convention.md) - 基于模块通信机制
- [ADR-121：契约 DTO 命名与组织](../structure/ADR-121-contract-dto-naming-organization.md) - 基于模块通信机制
- [ADR-122：测试代码组织与命名规范](../structure/ADR-122-test-organization-naming.md)
- [ADR-123：Repository 接口与分层命名规范](../structure/ADR-123-repository-interface-layering.md)
- [ADR-920：示例代码治理宪法](../governance/ADR-920-examples-governance-constitution.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0004：中央包管理与层级依赖规则](./ADR-0004-Cpm-Final.md) - 依赖管理补充
- [ADR-0006：术语与编号宪法](./ADR-0006-terminology-numbering-constitution.md) - 术语规范
- [ADR-0008：文档编写与维护宪法](./ADR-0008-documentation-governance-constitution.md) - 文档治理

---

## References（非裁决性参考）

非裁决性参考（建议、最佳实践、详细示例）请查阅：
- [ADR-0001 Copilot Prompts](../../copilot/adr-0001.prompts.md) - Copilot 场景化指导
- [模块化单体架构案例](../../cases/README.md) - 实际案例参考

**相关外部资源**：
- Simon Brown - Modular Monoliths
- Kamil Grzybek - Modular Monolith Architecture

---


---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充


---

## Non-Goals（明确不管什么）


本 ADR 明确不涉及以下内容：

- 待补充


## History（版本历史）

| 版本  | 日期         | 变更说明                                              |
|-----|------------|---------------------------------------------------|
| 5.0 | 2026-01-29 | 对齐 ADR-902 标准：添加 primary_enforcement、标准章节、规则独立编号 |
| 4.0 | 2026-01-26 | 裁决型重构，移除冗余                                        |
| 3.2 | 2026-01-23 | 术语&执行等级补充                                         |
| 3.0 | 2026-01-20 | 分层体系与目录规则固化                                       |
| 1.0 | 初始         | 初版发布                                              |
