# ADR-905：执行级别分类（Enforcement Level Classification）

> ⚖️ **本 ADR 定义 ADR-0005 规则的执行级别分类，是架构测试与人工审查的分工边界。**

**状态**：✅ Accepted  
**版本**：1.0  
**级别**：治理层 / 架构元规则  
**影响范围**：ADR-0005 所有规则的验证和执行  
**生效时间**：即刻

---

## 聚焦内容（Focus）

- ADR-0005 规则的三级执行分类
- 静态测试、语义分析、人工审查的边界
- 测试映射与工具选型

---

## 术语表（Glossary）

| 术语 | 定义 |
|------|------|
| Level 1 (L1) | 静态可执行，NetArchTest 完全覆盖 |
| Level 2 (L2) | 语义半自动，需 Roslyn Analyzer |
| Level 3 (L3) | 人工 Gate，需 PR 审查 |

---

## 规则本体（Rule）

> **这是本 ADR 唯一具有裁决力的部分。**

### ADR-905.1：三级执行分类强制要求

ADR-0005 的所有规则 **必须** 按以下三级分类：

#### Level 1 (L1)：静态可执行

**定义**：基于类型、命名空间、依赖关系的静态约束，通过 NetArchTest 完全自动化检查。

**执行要求**：
- ✅ 测试失败 = 绝对违规
- ✅ CI 阻断，不可破例
- ✅ 工具：ArchitectureTests（NetArchTest）

---

#### Level 2 (L2)：语义半自动

**定义**：需要语义分析的规则（如方法调用链、业务逻辑复杂度），通过 Roslyn Analyzer 检查。

**执行要求**：
- ⚠️ 测试失败 = 需要人工审查
- ⚠️ 可协商破例，需充分理由
- ⚠️ 工具：Roslyn Analyzer（自定义分析器）

---

#### Level 3 (L3)：人工 Gate

**定义**：涉及业务语义、设计意图、架构权衡的规则，需要人工审查和决策。

**执行要求**：
- ❓ 测试无法覆盖，依赖人工判断
- ❓ 预期破例，需记录在 `ARCH-VIOLATIONS.md`
- ❓ 工具：PR Checklist + 架构师 Code Review

---

---

## 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 测试映射

| 规则编号 | 执行级 | 测试/手段 | ADR-0005 规则 |
|---------|--------|-----------|--------------|
| ADR-0005.1 | L1 | `Handlers_Should_Have_Clear_Naming_Convention` | Handler 命名约定 |
| ADR-0005.3 | L1 | `Handlers_Should_Not_Depend_On_AspNet` | Handler 不依赖 ASP.NET |
| ADR-0005.4 | L1 | `Handlers_Should_Be_Stateless` | Handler 无状态 |
| ADR-0005.5 | L1 | `Modules_Should_Not_Have_Synchronous_Cross_Module_Calls` | 模块间同步调用 |
| ADR-0005.6 | L1 | `Async_Methods_Should_Follow_Naming_Convention` | 异步方法命名 |
| ADR-0005.7 | L1 | `Modules_Should_Not_Share_Domain_Entities` | 模块不共享领域实体 |
| ADR-0005.9 | L1 | `Command_And_Query_Handlers_Should_Be_Separated` | CQRS 分离 |
| ADR-0005.12 | L1 | `All_Handlers_Should_Be_In_Module_Assemblies` | Handler 位置 |
| ADR-0005.2 | L2 | Roslyn Analyzer（ADR0005_02） | Endpoint 业务逻辑 |
| ADR-0005.5 | L2 | Roslyn Analyzer（ADR0005_05） | 跨模块调用语义 |
| ADR-0005.10 | L2 | Roslyn Analyzer | Command Handler 返回值 |
| ADR-0005.11 | L2 | Roslyn Analyzer（ADR0005_11） | 异常使用规范 |
| ADR-0005.5 | L3 | PR Review + ARCH-VIOLATION | 模块间同步调用破例 |
| ADR-0005.8 | L3 | PR Review | Query Handler 返回 Contracts |

### 人工 Gate 流程

**Level 3 规则** 的破例流程：

1. **PR 提交时**：开发者在 PR 模板中声明架构破例
2. **CI 检查**：如果 ArchitectureTests 失败但 PR 声明"无违规" → 自动拒绝
3. **架构审查**：如果 PR 声明"有破例" → 架构师审查并决定是否批准
4. **记录归档**：所有破例必须记录在 [`ARCH-VIOLATIONS.md`](../summaries/arch-violations.md) 中

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例 **仅在以下情况允许**：

- Level 2 规则：需充分理由和人工审查
- Level 3 规则：预期破例，需记录权衡过程
- Level 1 规则：❌ **不允许破例**，CI 强制阻断

### 破例要求（不可省略）

每个破例 **必须**：

- 记录在 `ARCH-VIOLATIONS.md`
- 指明 ADR 编号 + 规则编号
- 指定失效日期
- 给出归还计划

**未记录的破例 = 未授权架构违规。**

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

- **ADR-905**（治理层 ADR）
  - 修改需 Tech Lead 审批
  - 修改时需同步更新 ADR-0005 的相关章节
  - 需要更新对应的架构测试和 Roslyn Analyzer
  - 不得削弱或推翻 ADR-0005 的规则

### 失效与替代

- 本 ADR 是 ADR-0005 的补充说明，定义执行级别分类
- 如需废弃，必须指向替代 ADR
- 不允许"隐性废弃"

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

- 具体的测试实现代码
- Roslyn Analyzer 的实现细节
- 测试工具的选型和配置
- 测试的编写教学
- 破例的具体审批流程细节（参见 ADR-0000）

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

**依赖（Depends On）**：
- [ADR-0005：应用内交互模型与执行边界](../constitutional/ADR-0005-Application-Interaction-Model-Final.md) - 本文档是 ADR-0005 的执行级别补充
- [ADR-0000：架构测试与 CI 治理宪法](./ADR-0000-architecture-tests.md) - 执行级别基于测试和 CI 治理机制

**相关（Related）**：
- [ADR-124：Endpoint 命名及参数约束规范](../structure/ADR-124-endpoint-naming-constraints.md)
- [ADR-120：领域事件命名规范](../structure/ADR-120-domain-event-naming-convention.md)
- [ADR-121：契约（Contract）与 DTO 命名组织规范](../structure/ADR-121-contract-dto-naming-organization.md)
- [ADR-210：领域事件版本化与兼容性](../runtime/ADR-210-event-versioning-compatibility.md)
- [ADR-240：Handler 异常约束](../runtime/ADR-240-handler-exception-constraints.md)
- [ADR-201：Handler 生命周期管理](../runtime/ADR-201-handler-lifecycle-management.md)
- [ADR-220：事件总线集成规范](../runtime/ADR-220-event-bus-integration.md)
- [ADR-920：示例代码治理宪法](./ADR-920-examples-governance-constitution.md)

---
