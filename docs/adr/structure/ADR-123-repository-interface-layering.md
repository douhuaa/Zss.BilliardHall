---
adr: ADR-123
title: "Repository 接口与分层命名规范"
status: Final
level: Structure
version: "2.0"
deciders: "Architecture Board"
date: 2026-01-26
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---

# ADR-123：Repository 接口与分层命名规范

> ⚖️ **本 ADR 定义 Repository 接口与实现的分层位置和命名的唯一裁决规则。**

**影响范围**：所有 Repository 实现  
**生效时间**：即刻

---

## Focus（聚焦内容）

- Repository 接口必须在 Domain 层
- Repository 实现必须在 Infrastructure 层
- Repository 接口与实现命名规范
- Repository 方法命名必须表达领域意图
- 禁止暴露技术细节的方法名

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------------|------------------------------|----------------------|
| Repository | 领域对象持久化抽象接口，隔离技术实现        | Repository           |
| 聚合根        | 聚合的根实体，是 Repository 操作的基本单位 | Aggregate Root       |
| 领域意图       | 从业务角度表达操作语义，隐藏技术细节        | Domain Intent        |
| L1 测试      | 静态可执行自动化测试                 | Level 1 Test         |
| L2 测试      | 语义半自动化测试或人工审查             | Level 2 Test         |

---

## Decision（裁决）

### ADR-123.1：Repository 接口必须位于 Domain 层

**规则**：
- Repository 接口**必须**定义在 Domain 层
- 禁止在 Infrastructure 或 Application 层定义接口
- 接口命名空间**必须**为 `{Root}.Domain.Repositories`

**判定**：
- ✅ `src/Modules/{Module}/Domain/Repositories/I{Aggregate}Repository.cs`
- ✅ `namespace Zss.BilliardHall.Modules.Orders.Domain.Repositories;`
- ❌ `src/Modules/{Module}/Infrastructure/...`（禁止）
- ❌ `src/Modules/{Module}/Application/...`（禁止）

### ADR-123.2：Repository 实现必须位于 Infrastructure 层

**规则**：
- Repository 具体实现**必须**位于 Infrastructure 层
- 禁止在 Domain 层实现 Repository
- 实现命名空间**必须**为 `{Root}.Infrastructure.Repositories`

**判定**：
- ✅ `src/Modules/{Module}/Infrastructure/Repositories/{Aggregate}Repository.cs`
- ✅ `namespace Zss.BilliardHall.Modules.Orders.Infrastructure.Repositories;`
- ❌ `src/Modules/{Module}/Domain/...`（禁止）

### ADR-123.3：Repository 接口命名必须遵循 I{Aggregate}Repository 模式

**规则**：
- Repository 接口名称**必须**为 `I` + 聚合根名 + `Repository`
- 禁止省略 `I` 前缀
- 禁止使用缩写或其他后缀

**判定**：
- ✅ `IOrderRepository`（Order 聚合根）
- ✅ `IMemberRepository`（Member 聚合根）
- ❌ `OrderRepository`（缺少 I 前缀）
- ❌ `IOrderRepo`（缩写不规范）
- ❌ `IOrderDataAccess`（非 Repository 后缀）

### ADR-123.4：Repository 实现命名禁止使用 Impl 后缀

**规则**：
- Repository 实现类名称**必须**直接使用聚合根名 + `Repository`
- 禁止添加 `Impl` 等后缀
- 多实现场景允许技术前缀（如 `Sql`、`Mongo`）

**判定**：
- ✅ `OrderRepository` implements `IOrderRepository`
- ✅ `SqlOrderRepository`（多实现场景）
- ✅ `MongoOrderRepository`（多实现场景）
- ❌ `OrderRepositoryImpl`
- ❌ `OrderRepositoryImplementation`

### ADR-123.5：Repository 方法必须表达领域意图

**规则**：
- Repository 方法名**必须**表达领域意图
- 禁止暴露技术细节（SQL、数据库概念）
- 禁止使用 CRUD 术语

**判定**：

**✅ 允许的方法名**：
```csharp
Task<Order?> GetByIdAsync(Guid orderId);
Task<IReadOnlyList<Order>> GetActiveOrdersAsync();
Task SaveAsync(Order order);
Task<bool> ExistsAsync(Guid orderId);
```

**❌ 永久黑名单**：
```csharp
Task<Order?> SelectByIdAsync(Guid orderId);    // Select 是 SQL 术语
Task<Order?> FindByPrimaryKeyAsync(Guid id);   // PrimaryKey 是数据库概念
Task InsertOrUpdateAsync(Order order);          // Insert/Update 是 CRUD 术语
Task<Order> QueryByIdAsync(Guid id);            // Query 暴露数据库操作
Task ExecuteSqlAsync(string sql);               // 直接暴露 SQL
```

**推荐动词**：
- Get/Find（查询）
- Save（新增或更新）
- Delete/Remove（删除）
- Exists（存在性检查）

---

## 快速参考表

| 约束编号       | 约束描述                   | 测试方式       | 测试用例                                      | 必须遵守 |
|------------|------------------------|------------|--------------------------------------------|------|
| ADR-123.1  | Repository 接口必须在 Domain | L1 - 自动化测试 | Repository_Interfaces_Must_Be_In_Domain      | ✅    |
| ADR-123.2  | Repository 实现必须在 Infrastructure | L1 - 自动化测试 | Repository_Implementations_Must_Be_In_Infrastructure | ✅    |
| ADR-123.3  | Repository 接口命名规范     | L1 - 自动化测试 | Repository_Interfaces_Must_Follow_Naming     | ✅    |
| ADR-123.4  | Repository 实现禁止 Impl 后缀 | L1 - 自动化测试 | Repository_Implementations_Must_Not_Have_Impl_Suffix | ✅    |
| ADR-123.5  | Repository 方法表达领域意图   | L2 - Code Review | Repository_Methods_Must_Express_Domain_Intent | ✅    |

> **级别说明**：L1=静态自动化（脚本检查），L2=语义半自动或人工审查

---

## Enforcement（执法模型）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_123_Architecture_Tests.cs` 强制验证：

- Repository 接口必须在 Domain 层检查
- Repository 实现必须在 Infrastructure 层检查
- Repository 接口命名是否符合 `I{Aggregate}Repository` 模式
- Repository 实现命名是否避免 `Impl` 后缀
- 代码审查检查方法名是否在黑名单中

**L2 测试**：
- 通过 Code Review 检查方法名是否表达领域意图
- 建议使用 Roslyn Analyzer 自动检测黑名单方法名

**有一项违规视为架构违规，CI 自动阻断。**

---

## 检查清单

- [ ] Repository 接口是否在 Domain 层？
- [ ] Repository 实现是否在 Infrastructure 层？
- [ ] Repository 接口命名是否符合 `I{Aggregate}Repository`？
- [ ] Repository 实现是否避免 Impl 后缀？
- [ ] Repository 方法名是否表达领域意图，避免技术术语？

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例**仅在以下情况允许**：

1. **多种实现并存**：同时支持 SQL 和 NoSQL，需技术前缀区分
2. **遗留代码迁移**：大规模重构的过渡期
3. **第三方框架约束**：框架强制要求特定命名

### 破例要求（不可省略）

每个破例**必须**：

- 记录在 `docs/summaries/arch-violations.md`
- 说明特殊情况和技术原因
- 提供迁移计划（如适用）
- 指定失效日期（不超过 3 个月）

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) - Repository 分层基于模块结构
- [ADR-0002：平台、应用与主机启动器架构](../constitutional/ADR-0002-platform-application-host-bootstrap.md) - Repository 遵循三层体系

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- 无

---

## 版本历史

| 版本  | 日期         | 变更说明       | 修订人 |
|-----|------------|------------|-----|
| 2.0 | 2026-01-26 | 裁决型重构，添加决策章节 | GitHub Copilot |
| 1.0 | 2026-01-24 | 初始版本       | GitHub Copilot |

---

## 附注

本文件禁止添加示例/建议/FAQ/背景说明，仅维护自动化可判定的架构红线。

非裁决性参考（详细示例、Repository 实现最佳实践、DDD Repository Pattern）请查阅：
- `docs/copilot/adr-0123.prompts.md`
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
