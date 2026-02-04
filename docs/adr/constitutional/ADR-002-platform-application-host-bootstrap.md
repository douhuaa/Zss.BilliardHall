---
adr: ADR-002
title: "Platform / Application / Host 三层启动体系"
status: Final
level: Constitutional
deciders: "Architecture Board"
date: 2026-02-04
version: "3.0"
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---


# ADR-002：Platform / Application / Host 三层启动体系

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

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-002 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-002_<Rule>_<Clause>
> ```

---

### ADR-002_1：Platform 层约束（Rule）

#### ADR-002_1_1 Platform 仅提供通用技术能力

- Platform 只提供通用技术能力（日志、追踪、异常、序列化）
- 不感知任何业务领域概念

**判定**：
- ❌ Platform 包含业务逻辑或领域类型
- ✅ 仅包含技术基础设施

#### ADR-002_1_2 Platform 不依赖上层

- Platform 不可访问 Application、Host、Modules
- 确保技术基座的独立性和可复用性

**判定**：
- ❌ Platform 依赖 Application/Host/Modules
- ✅ Platform 完全独立

#### ADR-002_1_3 Platform 唯一 Bootstrapper 入口

- Platform 必须有唯一入口 `PlatformBootstrapper.Configure`
- 所有 Platform 服务注册集中在 Bootstrapper

**判定**：
- ❌ Platform 缺少唯一 Bootstrapper 入口
- ❌ Platform 存在多个 Bootstrapper
- ✅ 唯一 PlatformBootstrapper.Configure 入口

---

### ADR-002_2：Application 层约束（Rule）

#### ADR-002_2_1 Application 负责系统能力装配

- Application 负责系统能力的装配和集成
- 定义"系统是什么"，聚合模块和用例

**判定**：
- ❌ Application 包含具体业务逻辑
- ✅ 仅做模块装配和集成

#### ADR-002_2_2 Application 禁止依赖 Host

- Application 禁止依赖 Host 层
- 不感知运行形态（Web/Worker/Test）

**判定**：
- ❌ Application 依赖 Host
- ✅ Application 独立于运行环境

#### ADR-002_2_3 Application 禁止依赖 Modules

- Application 禁止直接引用 Modules
- 通过扫描和反射加载模块

**判定**：
- ❌ Application 直接引用 Modules
- ✅ Application 通过扫描发现模块

#### ADR-002_2_4 Application 不包含 Host 专属类型

- Application 不包含 HttpContext 等 Host 专属类型
- 使用抽象替代具体的 Host 类型

**判定**：
- ❌ Application 使用 HttpContext
- ✅ Application 只依赖抽象接口

#### ADR-002_2_5 Application 唯一 Bootstrapper 入口

- Application 必须有唯一入口 `ApplicationBootstrapper.Configure`
- 所有 Application 服务注册集中在 Bootstrapper

**判定**：
- ❌ Application 缺少唯一 Bootstrapper 入口
- ❌ Application 存在多个 Bootstrapper
- ✅ 唯一 ApplicationBootstrapper.Configure 入口

---

### ADR-002_3：Host 层约束（Rule）

#### ADR-002_3_1 Host 唯一职责为调用 Bootstrapper

- Host 唯一职责：调用 Platform、Application 的 Bootstrapper
- 决定进程模型，不包含任何业务逻辑

**判定**：
- ❌ Host 包含业务逻辑或技术配置
- ✅ 仅调用两个 Bootstrapper

#### ADR-002_3_2 Host 决定进程模型

- Host 决定进程模型（Web/Worker/Test）
- 项目命名为 `Zss.BilliardHall.Host.*`

**判定**：
- ❌ Host 项目命名不规范
- ✅ Host 项目命名符合规范

#### ADR-002_3_3 Host 不依赖 Modules

- Host 不应依赖任何业务模块
- Host 通过 Application 间接引入模块

**判定**：
- ❌ Host 依赖 Modules
- ❌ Host 项目文件引用 Modules
- ✅ Host 完全独立于业务模块

#### ADR-002_3_4 Program.cs 极简化

- Program.cs 保持极简（建议 ≤30 行）
- 只保留核心调用

**判定**：
- ❌ Program.cs 超过 30 行
- ✅ Program.cs 简洁明了

#### ADR-002_3_5 Program.cs 只调用 Bootstrapper

- Program.cs 只应调用 Bootstrapper
- 不包含具体的服务注册、配置逻辑

**判定**：
- ❌ Program.cs 包含具体配置
- ❌ Program.cs 做了 Bootstrapper 以外的事
- ✅ Program.cs 只调用 Bootstrapper

---

### ADR-002_4：三层依赖方向验证（Rule）

#### ADR-002_4_1 完整的单向依赖链

- 完整的单向依赖链：Host → Application → Platform
- 任何反向依赖都是违规

**判定**：
- ❌ 存在反向依赖
- ✅ 严格的单向依赖流

---

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-002 各条款（Clause）的执法方式及执行级别。
>
> 所有规则通过 `src/tests/ArchitectureTests/ADR-002/` 目录下的测试强制验证。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-002_1_1** | L1 | ArchitectureTests 验证 Platform 不依赖 Application | §ADR-002_1_1 |
| **ADR-002_1_2** | L1 | ArchitectureTests 验证 Platform 不依赖 Host/Modules | §ADR-002_1_2 |
| **ADR-002_1_3** | L1 | ArchitectureTests 验证 PlatformBootstrapper 存在 | §ADR-002_1_3 |
| **ADR-002_2_1** | L1 | ArchitectureTests 验证 Application 职责边界 | §ADR-002_2_1 |
| **ADR-002_2_2** | L1 | ArchitectureTests 验证 Application 不依赖 Host | §ADR-002_2_2 |
| **ADR-002_2_3** | L1 | ArchitectureTests 验证 Application 不依赖 Modules | §ADR-002_2_3 |
| **ADR-002_2_4** | L1 | ArchitectureTests 验证 Application 不使用 HttpContext | §ADR-002_2_4 |
| **ADR-002_2_5** | L1 | ArchitectureTests 验证 ApplicationBootstrapper 存在 | §ADR-002_2_5 |
| **ADR-002_3_1** | L1 | ArchitectureTests 验证 Host 不依赖 Modules | §ADR-002_3_1 |
| **ADR-002_3_2** | L1 | ArchitectureTests 验证 Host 项目命名规范 | §ADR-002_3_2 |
| **ADR-002_3_3** | L1 | ArchitectureTests 验证 Host 项目文件不引用 Modules | §ADR-002_3_3 |
| **ADR-002_3_4** | L2 | ArchitectureTests 验证 Program.cs 行数限制 | §ADR-002_3_4 |
| **ADR-002_3_5** | L2 | ArchitectureTests 语义检查 Program.cs 内容 | §ADR-002_3_5 |
| **ADR-002_4_1** | L1 | ArchitectureTests 验证三层依赖方向 | §ADR-002_4_1 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决

**有一项 L1 违规视为架构违规，CI 自动阻断。**

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
- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-900
- [ADR-001：模块化单体与垂直切片架构](./ADR-001-modular-monolith-vertical-slice-architecture.md) - 三层体系与模块组织配合定义系统结构

**被依赖（Depended By）**：
- [ADR-003：命名空间与项目结构规范](./ADR-003-namespace-rules.md) - 命名空间规范基于三层体系
- [ADR-004：中央包管理与层级依赖规则](./ADR-004-Cpm-Final.md) - 包依赖规则基于三层依赖方向
- [ADR-005：应用内交互模型与执行边界](./ADR-005-Application-Interaction-Model-Final.md) - 运行时交互基于三层装配
- [ADR-123：Repository 接口与分层命名规范](../structure/ADR-123-repository-interface-layering.md)
- [ADR-340：结构化日志与监控约束](../technical/ADR-340-structured-logging-monitoring-constraints.md)
- [ADR-920：示例代码治理规范](../governance/ADR-920-examples-governance-constitution.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-006：术语与编号宪法](./ADR-006-terminology-numbering-constitution.md) - 层级命名规范

---

---

## References（非裁决性参考）


**相关外部资源**：
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - 分层架构理论基础
- [Hexagonal Architecture (Ports and Adapters)](https://alistair.cockburn.us/hexagonal-architecture/) - 六边形架构参考
- [ASP.NET Core Startup Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/startup) - Microsoft 官方指导

**相关内部文档**：
- [ADR-001：模块化单体与垂直切片架构](./ADR-001-modular-monolith-vertical-slice-architecture.md) - 模块隔离与垂直切片
- [ADR-003：命名空间与项目结构规范](./ADR-003-namespace-rules.md) - 三层命名空间规范
- [ADR-004：中央包管理与层级依赖规则](./ADR-004-Cpm-Final.md) - 层级包依赖规则
- [ADR-005：应用内交互模型与执行边界](./ADR-005-Application-Interaction-Model-Final.md) - 三层运行时交互


---

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                         | 修订人 |
|-----|------------|----------------------------------------------|----|
| 3.0 | 2026-02-04 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
| 2.0 | 2026-01-29 | 同步 ADR-902/940/0006 标准：添加 Front Matter、术语表英文对照 | Architecture Board |
| 1.0 | 2026-01-26 | 裁决型重构，移除冗余                                   | Architecture Board |

---
