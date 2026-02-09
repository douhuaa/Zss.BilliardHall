---
adr: ADR-123
title: "Repository 接口与分层命名规范"
status: Final
level: Structure
version: "3.0"
deciders: "Architecture Board"
date: 2026-02-03
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---


# ADR-123：Repository 接口与分层命名规范

> ⚖️ **本 ADR 定义 Repository 接口与实现的分层位置和命名的唯一裁决规则。**

**影响范围**：所有 Repository 实现  
## Focus（聚焦内容）

- Repository 接口必须在 Domain 层
- Repository 实现必须在 Infrastructure 层
- Repository 接口与实现命名规范
- Repository 方法命名必须表达领域意图
- 禁止暴露技术细节的方法名

---

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

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-123 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-123_<Rule>_<Clause>
> ```

---

### ADR-123_1：Repository 分层约束（Rule）

#### ADR-123_1_1 Repository 接口必须位于 Domain 层

**规则**：
- Repository 接口**必须**定义在 Domain 层
- 禁止在 Infrastructure 或 Application 层定义接口
- 接口命名空间**必须**为 `{Root}.Domain.Repositories`

**判定**：
- ✅ `src/Modules/{Module}/Domain/Repositories/I{Aggregate}Repository.cs`
- ✅ `namespace Zss.BilliardHall.Modules.Orders.Domain.Repositories;`
- ❌ `src/Modules/{Module}/Infrastructure/...`（禁止）
- ❌ `src/Modules/{Module}/Application/...`（禁止）

#### ADR-123_1_2 Repository 实现必须位于 Infrastructure 层

**规则**：
- Repository 具体实现**必须**位于 Infrastructure 层
- 禁止在 Domain 层实现 Repository
- 实现命名空间**必须**为 `{Root}.Infrastructure.Repositories`

**判定**：
- ✅ `src/Modules/{Module}/Infrastructure/Repositories/{Aggregate}Repository.cs`
- ✅ `namespace Zss.BilliardHall.Modules.Orders.Infrastructure.Repositories;`
- ❌ `src/Modules/{Module}/Domain/...`（禁止）

---

### ADR-123_2：Repository 命名规范（Rule）

#### ADR-123_2_1 Repository 接口命名必须遵循 I{Aggregate}Repository 模式

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

#### ADR-123_2_2 Repository 实现命名禁止使用 Impl 后缀

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

#### ADR-123_2_3 Repository 方法必须表达领域意图

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

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-123 各条款（Clause）的执法方式及执行级别。
>
> 所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_123_Architecture_Tests.cs` 强制验证。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-123_1_1** | L1 | ArchitectureTests 验证接口在 Domain 层 | §ADR-123_1_1 |
| **ADR-123_1_2** | L1 | ArchitectureTests 验证实现在 Infrastructure 层 | §ADR-123_1_2 |
| **ADR-123_2_1** | L1 | ArchitectureTests 验证接口命名模式 | §ADR-123_2_1 |
| **ADR-123_2_2** | L1 | ArchitectureTests 检测 Impl 后缀 | §ADR-123_2_2 |
| **ADR-123_2_3** | L2 | Code Review + Roslyn Analyzer 检测黑名单方法名 | §ADR-123_2_3 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决

**有一项 L1 违规视为架构违规，CI 自动阻断。**

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
- [ADR-001：模块化单体与垂直切片架构](../constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md) - Repository 分层基于模块结构
- [ADR-002：平台、应用与主机启动器架构](../constitutional/ADR-002-platform-application-host-bootstrap.md) - Repository 遵循三层体系

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- 无

---

---

## References（非裁决性参考）


- 待补充


---

---

## History（版本历史）

| 版本  | 日期         | 变更说明   | 修订人 |
|-----|------------|--------|-------|
| 3.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
| 2.0 | 2026-01-26 | 更新版本 | Architecture Board |
| 1.0 | 2026-01-29 | 初始版本 | Architecture Board |
