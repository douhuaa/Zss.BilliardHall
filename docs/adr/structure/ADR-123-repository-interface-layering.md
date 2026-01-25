# ADR-123：Repository 接口与分层命名规范

**状态**：✅ Accepted  
**级别**：结构层  
**影响范围**：所有 Repository 实现  
**生效时间**：待审批通过后

---

## 规则本体（Rule）

> **这是本 ADR 唯一具有裁决力的部分。**

### ADR-123.1：Repository 接口必须位于 Domain 层

Repository 接口**必须**定义在 Domain 层，不得定义在其他层。

**位置要求**：
```
✅ src/Modules/{Module}/Domain/Repositories/I{Aggregate}Repository.cs
❌ src/Modules/{Module}/Infrastructure/...  （禁止）
❌ src/Modules/{Module}/Application/...     （禁止）
```

**命名空间**：
```csharp
✅ namespace Zss.BilliardHall.Modules.Orders.Domain.Repositories;
❌ namespace Zss.BilliardHall.Modules.Orders.Infrastructure.Repositories;
```

### ADR-123.2：Repository 实现必须位于 Infrastructure 层

Repository 具体实现**必须**位于 Infrastructure 层。

**位置要求**：
```
✅ src/Modules/{Module}/Infrastructure/Repositories/{Aggregate}Repository.cs
❌ src/Modules/{Module}/Domain/...  （禁止）
```

### ADR-123.3：Repository 接口命名必须遵循 I{Aggregate}Repository 模式

Repository 接口名称**必须**为 `I` + 聚合根名 + `Repository`。

**命名规则**：
- ✅ `IOrderRepository`（Order 聚合根）
- ✅ `IMemberRepository`（Member 聚合根）
- ❌ `OrderRepository`（缺少 I 前缀）
- ❌ `IOrderRepo`（缩写不规范）
- ❌ `IOrderDataAccess`（非 Repository 后缀）

### ADR-123.4：Repository 实现命名禁止使用 Impl 后缀

Repository 实现类名称**必须**直接使用聚合根名 + `Repository`，不得添加 Impl 等后缀。

**命名规则**：
- ✅ `OrderRepository` implements `IOrderRepository`
- ❌ `OrderRepositoryImpl`
- ❌ `OrderRepositoryImplementation`
- ❌ `SqlOrderRepository`（除非有多种实现）

**多实现例外**：
如果确实有多种实现（如 SQL 和 NoSQL），允许：
- ✅ `SqlOrderRepository`
- ✅ `MongoOrderRepository`

### ADR-123.5：Repository 方法必须表达领域意图

Repository 方法名**必须**表达领域意图，不得暴露技术细节。

**命名规则**：
```csharp
// ✅ 正确：表达领域意图
Task<Order?> GetByIdAsync(Guid orderId);
Task<IReadOnlyList<Order>> GetActiveOrdersAsync();
Task SaveAsync(Order order);

// ❌ 错误：暴露技术细节
Task<Order?> SelectByIdAsync(Guid orderId);    // Select 是 SQL 术语
Task<Order?> FindByPrimaryKeyAsync(Guid id);   // PrimaryKey 是数据库概念
Task InsertOrUpdateAsync(Order order);          // Insert/Update 是 CRUD 术语
```

**推荐动词**：
- Get/Find（查询单个或多个）
- Save（新增或更新）
- Delete/Remove（删除）
- Exists（存在性检查）

**永久黑名单（严禁使用）**：
- ❌ Select/SelectAll/SelectWhere（SQL 术语）
- ❌ Insert/Update/Upsert（CRUD 术语）
- ❌ Query/Execute/ExecuteSql（数据库操作暴露）
- ❌ FindByPrimaryKey/FindByForeignKey（数据库概念）
- ❌ Load/Fetch（ORM 实现细节）

**违规处理**：
- 代码审查必须拒绝黑名单方法名
- 建议使用 Roslyn Analyzer 自动检测黑名单

---

## 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 测试映射

| 规则编号 | 执行级 | 测试/手段 |
|---------|--------|----------|
| ADR-123.1 | L1 | `Repository_Interfaces_Must_Be_In_Domain` |
| ADR-123.2 | L1 | `Repository_Implementations_Must_Be_In_Infrastructure` |
| ADR-123.3 | L1 | `Repository_Interfaces_Must_Follow_Naming` |
| ADR-123.4 | L1 | `Repository_Implementations_Must_Not_Have_Impl_Suffix` |
| ADR-123.5 | L2 | Code Review |

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例**仅在以下情况允许**：

1. **多种实现并存**：同时支持 SQL 和 NoSQL
2. **遗留代码迁移**：大规模重构的过渡期
3. **第三方框架约束**：框架强制要求特定命名

### 破例要求（不可省略）

每个破例**必须**：

- 记录在 `docs/summaries/arch-violations.md`
- 说明特殊情况和技术原因
- 提供迁移计划（如适用）

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

* **结构层 ADR**
  * 修改需 Tech Lead 审批
  * 需评估对现有 Repository 的影响

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

- ✗ Repository 的具体实现技术（EF Core/Dapper）
- ✗ Repository 方法的具体实现逻辑
- ✗ 数据库查询优化
- ✗ 缓存策略

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 相关 ADR
- ADR-0001：模块化单体与垂直切片架构
- ADR-0002：Platform/Application/Host 三层启动体系

### Domain-Driven Design
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [DDD Aggregates](https://www.dddcommunity.org/library/vernon_2011/)

### 实践指导
- Repository 实现示例参见 `docs/copilot/adr-0123.prompts.md`

---

## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 |
|-----|------|---------|--------|
| 1.0 Draft | 2026-01-24 | 初始版本 | GitHub Copilot |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
