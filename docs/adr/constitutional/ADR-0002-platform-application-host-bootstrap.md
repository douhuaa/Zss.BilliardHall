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

---

## Enforcement（执法模型）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0002_Architecture_Tests.cs` 强制验证。

**有一项违规视为架构违规，CI 自动阻断。**

---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **具体框架选型**：不约束使用 ASP.NET Core、Wolverine 还是其他特定框架（仅约束分层边界）
- **依赖注入容器选择**：不约束使用哪个 DI 容器（仅约束注册在哪一层）
- **配置来源**：不约束配置来自 appsettings.json、环境变量还是其他来源
- **日志实现**：不约束使用 Serilog、NLog 还是其他日志库（仅约束在 Platform 层）
- **启动性能优化**：不涉及启动速度、懒加载等性能优化策略
- **多进程模型**：不涉及是否运行多个 Host 实例或进程间通信
- **Bootstrapper 内部实现**：不约束 Bootstrapper 的具体实现方式（仅约束其唯一性和职责）
- **测试环境配置**：不约束测试环境如何模拟或替换 Bootstrapper

---

## Prohibited（禁止行为）


以下行为明确禁止：

### Platform 层违规
- ❌ **Platform 依赖 Application/Host/Modules**：禁止 Platform 项目引用业务层或宿主层
- ❌ **Platform 包含业务逻辑**：禁止在 Platform 中实现任何业务规则或领域逻辑
- ❌ **Platform 多个 Bootstrapper**：每个 Platform 项目只允许一个 Bootstrapper 入口
- ❌ **Platform 直接访问数据库**：禁止 Platform 层直接实现数据访问逻辑

### Application 层违规
- ❌ **Application 依赖 Host**：禁止 Application 项目引用任何 Host 项目
- ❌ **Application 使用 HttpContext**：禁止直接依赖 ASP.NET Core 的 HttpContext 或其他 Host 专属类型
- ❌ **Application 多个 Bootstrapper**：每个 Application 项目只允许一个 Bootstrapper 入口
- ❌ **Application 包含进程相关代码**：禁止包含中间件、路由配置等进程特定逻辑

### Host 层违规
- ❌ **Host 依赖 Modules**：Host 项目文件禁止 `<ProjectReference>` 指向 Modules
- ❌ **Host 包含业务逻辑**：禁止在 Program.cs 或 Host 项目中实现业务规则
- ❌ **Program.cs 臃肿**：Program.cs 超过 30 行视为违规（除注释和空行）
- ❌ **Host 直接注册服务**：禁止在 Host 中直接调用 `services.AddScoped<T>()` 等（应委托给 Bootstrapper）
- ❌ **Host 多个 Bootstrapper 调用点**：禁止在多处调用 Bootstrapper（必须集中在 Program.cs）

### 反向依赖违规
- ❌ **Application 回调 Host**：禁止 Application 通过接口、委托等方式回调 Host 层
- ❌ **Platform 访问 Application 配置**：禁止 Platform 依赖 Application 的配置或状态
- ❌ **跨层直接访问**：禁止通过 ServiceLocator 模式或静态访问器绕过依赖方向


---

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

---

## References（非裁决性参考）


**相关外部资源**：
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - 分层架构理论基础
- [Hexagonal Architecture (Ports and Adapters)](https://alistair.cockburn.us/hexagonal-architecture/) - 六边形架构参考
- [ASP.NET Core Startup Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/startup) - Microsoft 官方指导

**相关内部文档**：
- [ADR-0001：模块化单体与垂直切片架构](./ADR-0001-modular-monolith-vertical-slice-architecture.md) - 模块隔离与垂直切片
- [ADR-0003：命名空间与项目结构规范](./ADR-0003-namespace-rules.md) - 三层命名空间规范
- [ADR-0004：中央包管理与层级依赖规则](./ADR-0004-Cpm-Final.md) - 层级包依赖规则
- [ADR-0005：应用内交互模型与执行边界](./ADR-0005-Application-Interaction-Model-Final.md) - 三层运行时交互


---

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                         |
|-----|------------|----------------------------------------------|
| 2.0 | 2026-01-29 | 同步 ADR-902/940/0006 标准：添加 Front Matter、术语表英文对照 |
| 1.0 | 2026-01-26 | 裁决型重构，移除冗余                                   |

---
