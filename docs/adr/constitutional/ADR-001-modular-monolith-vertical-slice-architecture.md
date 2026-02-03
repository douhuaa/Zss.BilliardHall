---
adr: ADR-001
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


# ADR-001：模块化单体与垂直切片架构

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

---

## Decision（裁决）

> ⚠️ **本节是唯一裁决来源，其他章节不得产生新规则。**

### ADR-001.1:L1 模块按业务能力独立划分

- 模块按业务能力独立划分（如 Members/Orders）
- 每模块 = 独立程序集 = 清晰目录边界
- 模块间禁止直接引用代码、类型、资源

**判定**：
- ❌ 模块引用其他模块类型
- ✅ 仅通过契约、事件、原始类型通信

### ADR-001.2:L1 项目文件禁止引用其他模块

- 项目文件（.csproj）不得包含对其他模块的 ProjectReference
- 确保模块在编译时物理隔离

**判定**：
- ❌ 项目文件引用其他模块
- ✅ 模块编译时独立

### ADR-001.3:L2 垂直切片以用例为最小单元

- 用例（Use Case）为最小组织单元
- 每用例包含 Endpoint → Command/Query → Handler → 领域逻辑
- Handler 必须在 UseCases 命名空间

**判定**：
- ❌ Handler 不在 UseCases 命名空间
- ✅ 每用例自包含完整切片

### ADR-001.4:L1 禁止横向 Service 抽象

- 禁止使用 Service/Manager/Helper 类承载业务逻辑
- 业务逻辑应在 Handler 或领域模型中

**判定**：
- ❌ 存在 *Service 类
- ✅ 业务逻辑在正确位置

### ADR-001.5:L2 模块间通信仅允许事件/契约/原始类型

- 模块间仅允许：领域事件、契约 DTO、原始类型（Guid/string/int）
- 禁止直接依赖其他模块的 Entity/Aggregate/VO

**判定**：
- ❌ 直接依赖 Entity/Aggregate/VO
- ✅ 仅传递只读数据

### ADR-001.6:L2 契约不含业务决策字段

- 契约 DTO 不含业务判断字段（如 CanRefund）
- 契约不含行为方法
- 契约仅用于数据传递

**判定**：
- ❌ 契约包含业务判断字段
- ✅ 契约仅传递数据

### ADR-001.7:L1 命名空间匹配模块边界

- 命名空间必须与模块边界一致
- 目录结构必须反映模块隔离
- 确保命名空间不跨模块

**判定**：
- ❌ 命名空间不匹配模块边界
- ✅ 命名空间清晰隔离

---

---

## Enforcement（执法模型）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_001_Architecture_Tests.cs` 强制验证：

- 模块不应相互引用（程序集级别）
- 模块项目文件不应引用其他模块
- 命名空间应匹配模块边界
- Handler 必须在 UseCases 命名空间
- 模块不应包含 Service 类
- 契约语义规则检查
- 契约业务字段分析

**有一项违规视为架构违规，CI 自动阻断。**

---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **微服务拆分策略**：不涉及何时将模块化单体拆分为微服务的决策标准
- **模块内部实现细节**：不约束模块内部的具体业务逻辑实现方式
- **数据库物理分离**：不强制要求模块使用独立数据库（Database per Module）
- **团队组织结构**：不涉及团队如何组织或分配模块开发职责
- **性能优化策略**：不涉及缓存、并发、异步等具体性能优化手段
- **特定技术栈选型**：不约束特定的 ORM、消息队列或其他技术选型（除非违反隔离原则）
- **模块粒度判定**：不提供如何判断"什么应该成为独立模块"的业务标准
- **部署模式**：不涉及容器化、云原生或其他部署形式（仅确保单体可部署）

---

## Prohibited（禁止行为）


以下行为明确禁止：

### 模块依赖违规
- ❌ **直接引用其他模块的类型**：禁止通过 `using` 引用其他模块的命名空间
- ❌ **项目文件跨模块引用**：`.csproj` 中禁止 `<ProjectReference>` 指向其他模块
- ❌ **通过反射访问其他模块**：禁止使用反射、动态加载等方式绕过编译时隔离
- ❌ **共享领域对象**：禁止直接传递 Entity、Aggregate、ValueObject 到其他模块

### 通信机制违规
- ❌ **同步调用其他模块**：禁止直接方法调用、同步仓储查询等同步通信方式（除非经架构委员会审批）
- ❌ **在契约中包含业务逻辑**：Contract DTO 中禁止包含计算属性、业务判断方法或行为
- ❌ **事件携带领域对象**：领域事件禁止直接携带 Entity/Aggregate/VO，只允许原始类型和 DTO

### 架构模式违规
- ❌ **使用横向 Service 抽象**：禁止创建 `*Service`、`*Manager`、`*Helper` 等横向抽象类承载业务逻辑
- ❌ **Handler 不在 UseCases 命名空间**：所有 Handler 必须在 `*.UseCases.*` 命名空间下
- ❌ **模块命名空间混乱**：命名空间必须与目录结构和模块边界严格对应

### 测试绕过
- ❌ **修改 InternalsVisibleTo**：禁止为绕过模块隔离而暴露 internal 类型给其他模块
- ❌ **添加 public 访问修饰符**：禁止仅为其他模块访问而将原本应该 internal 的类型标记为 public


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md) - 测试执行机制

**被依赖（Depended By）**：
- [ADR-002：平台、应用与主机启动器架构](./ADR-002-platform-application-host-bootstrap.md) - 基于模块隔离规则
- [ADR-003：命名空间与项目结构规范](./ADR-003-namespace-rules.md) - 基于模块边界定义
- [ADR-005：应用内交互模型与执行边界](./ADR-005-Application-Interaction-Model-Final.md) - 基于模块通信约束
- [ADR-120：领域事件命名约定](../structure/ADR-120-domain-event-naming-convention.md) - 基于模块通信机制
- [ADR-121：契约 DTO 命名与组织](../structure/ADR-121-contract-dto-naming-organization.md) - 基于模块通信机制
- [ADR-122：测试代码组织与命名规范](../structure/ADR-122-test-organization-naming.md)
- [ADR-123：Repository 接口与分层命名规范](../structure/ADR-123-repository-interface-layering.md)
- [ADR-920：示例代码治理规范](../governance/ADR-920-examples-governance-constitution.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-004：中央包管理与层级依赖规则](./ADR-004-Cpm-Final.md) - 依赖管理补充
- [ADR-006：术语与编号宪法](./ADR-006-terminology-numbering-constitution.md) - 术语规范
- [ADR-008：文档编写与维护宪法](./ADR-008-documentation-governance-constitution.md) - 文档治理

---

---

## References（非裁决性参考）

**相关外部资源**：
- [Simon Brown - Modular Monoliths](https://www.codingthearchitecture.com/presentations/sa2015-modular-monoliths) - 模块化单体理论基础
- [Kamil Grzybek - Modular Monolith Architecture](https://github.com/kgrzybek/modular-monolith-with-ddd) - 参考实现
- [Vertical Slice Architecture by Jimmy Bogard](https://www.youtube.com/watch?v=SUiWfhAhgQw) - 垂直切片架构讲解
- [Mark Seemann - Dependency Rejection](https://blog.ploeh.dk/2017/02/02/dependency-rejection/) - 模块隔离原则

**相关内部文档**：
- [ADR-002：平台、应用与主机启动器架构](./ADR-002-platform-application-host-bootstrap.md) - 三层架构体系
- [ADR-003：命名空间与项目结构规范](./ADR-003-namespace-rules.md) - 模块命名空间规范
- [ADR-004：中央包管理与层级依赖规则](./ADR-004-Cpm-Final.md) - 模块依赖管理
- [ADR-005：应用内交互模型与执行边界](./ADR-005-Application-Interaction-Model-Final.md) - 模块间通信

---


---

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                              |
|-----|------------|---------------------------------------------------|
| 5.0 | 2026-01-29 | 对齐 ADR-902 标准：添加 primary_enforcement、标准章节、规则独立编号 |
| 4.0 | 2026-01-26 | 裁决型重构，移除冗余                                        |
| 3.2 | 2026-01-23 | 术语&执行等级补充                                         |
| 3.0 | 2026-01-20 | 分层体系与目录规则固化                                       |
| 1.0 | 初始         | 初版发布                                              |
