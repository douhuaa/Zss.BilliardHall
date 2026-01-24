# ADR-0005：应用内交互模型

**状态**：Final  
**级别**：宪法层  
**影响范围**：Application / Modules / Host  
**生效时间**：即刻

---

## 规则本体（Rule）

> **这是本 ADR 唯一具有裁决力的部分。**

### R1 Use Case 与 Handler 唯一性

每个业务用例**必须**：
- 有且仅有一个 Handler
- Handler 拥有该用例的全部业务决策权
- Handler 不得持有跨调用生命周期的业务状态

### R2 Endpoint 职责限制

Endpoint/Controller **必须**：
- 仅做请求适配和 Handler 调用
- 不包含任何业务规则或决策逻辑

Endpoint **禁止**：
- 包含业务逻辑
- 直接访问数据库或仓储
- 承载业务状态

### R3 Handler 边界约束

Handler **禁止**：
- 作为同步跨模块"粘合层"
- 返回或暴露领域实体（Entity/Aggregate/VO）作为出参
- 依赖 ASP.NET 专属类型（如 HttpContext）

### R4 模块通信规则

模块间通信**必须**：
- 默认使用异步通信（领域事件/集成事件）
- 仅通过契约 DTO/事件通信
- 不直接引用/传递 Entity/Aggregate/VO

**禁止**：
- 未经审批的跨模块同步调用
- 共享领域模型

### R5 契约约束

契约（Contract）**必须**：
- 仅用于数据传递
- 只读且单向
- 不包含业务决策/行为方法

### R6 CQRS 分离

Command Handler **必须**：
- 仅执行业务逻辑
- 返回 void 或唯一标识（Guid/int/string）
- 不返回业务数据

Query Handler **必须**：
- 仅返回只读 DTO/投影
- 不修改状态
- 不发布事件

Command/Query Handler **禁止**：
- 职责混用或合并

---

## 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 执行级别

| 级别 | 名称      | 执法方式               | 后果    |
| ---- | ------- | ------------------ | ----- |
| L1   | 静态可执行   | 自动化测试（NetArchTest） | CI 阻断 |
| L2   | 语义半自动   | Roslyn Analyzer / 启发式 | 人工复核  |
| L3   | 人工 Gate | Code Review / Checklist | 架构裁决  |

### 测试映射

| 规则编号 | 执行级 | 测试 / 手段                        |
| ------- | --- | ------------------------------ |
| R5.1    | L1/L3 | `Handlers_Should_Be_In_UseCases_Namespace` |
| R5.1    | L1  | `Handlers_Should_Not_Have_State` |
| R5.2    | L2/L3 | `Endpoints_Should_Not_Contain_Business_Logic` |
| R5.3    | L1  | `Handlers_Should_Not_Depend_On_HttpContext` |
| R5.3    | L2  | `Handlers_Should_Not_Return_Domain_Entities` |
| R5.4    | L2  | `Modules_Should_Not_Have_Synchronous_Cross_Module_Calls` |
| R5.4    | L1  | `Modules_Should_Not_Share_Domain_Models` |
| R5.5    | L2/L3 | `Contracts_Should_Not_Contain_Business_Logic` |
| R5.6    | L1  | `Command_Handlers_Should_Not_Return_Business_Data` |
| R5.6    | L1  | `Query_Handlers_Should_Not_Modify_State` |
| R5.6    | L1  | `Handlers_Should_Follow_CQRS_Pattern` |

### 测试位置

所有架构测试位于：`src/tests/ArchitectureTests/ADR/ADR_0005_Architecture_Tests.cs`

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例 **仅在以下情况允许**：

* 迁移期遗留代码（必须在 6 个月内归还）
* 性能关键路径的特殊优化（需架构委员会审批）
* 第三方集成的技术限制（需架构委员会审批）

### 破例要求（不可省略）

每个破例 **必须**：

* 记录在 `docs/summaries/ARCH-VIOLATIONS.md`
* 指明 ADR-0005 + 规则编号（如 R5.4）
* 指定失效日期（不超过 6 个月）
* 给出归还计划（具体到季度）

**未记录的破例 = 未授权架构违规。**

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

* **宪法层 ADR**（ADR-0001~0005）

  * 修改 = 架构修宪
  * 需要架构委员会 100% 同意
  * 需要 2 周公示期
  * 需要全量回归测试

### 失效与替代

* Superseded ADR **必须**：
  - 状态标记为 "Superseded by ADR-YYYY"
  - 指向替代 ADR
  - 保留在仓库中（不删除）
  - 移除或更新对应测试

* 不允许"隐性废弃"（偷偷删除或不标记状态）

### 同步更新

ADR 变更时 **必须** 同步更新：

* 架构测试代码
* Copilot prompts 文件（`docs/copilot/adr-0005.prompts.md`）
* 映射脚本
* README 导航

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

* 具体使用哪个消息框架 → ADR-300+ 技术层
* Saga/补偿模式的实现 → 业务实践
* 失败处理和重试策略 → 工程实践
* 性能优化的具体方案 → 性能团队
* 教学示例和最佳实践 → `docs/copilot/adr-0005.prompts.md`

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 术语表

| 术语            | 定义说明 |
|----------------|--------------------------------------|
| Use Case       | 端到端业务用例 |
| Handler        | 业务用例的唯一决策实现 |
| Command/Query  | 分别代表写/读单一职责 |
| CQRS           | 命令-查询职责分离 |
| 契约（Contract） | 模块间只读通信对象 |
| 领域实体        | 业务内聚的复杂类型，不跨模块 |
| 模块间通信      | 只允许事件、契约、原始类型 |

### 相关 ADR

- [ADR-0000：架构测试与 CI 治理](../governance/ADR-0000-architecture-tests.md)
- [ADR-0001：模块化单体与垂直切片架构](ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0002：Platform/Application/Host 启动体系](ADR-0002-platform-application-host-bootstrap.md)

### 辅导材料

- `docs/copilot/adr-0005.prompts.md` - 示例代码和常见问题
- `docs/copilot/backend-development.instructions.md` - 后端开发指导

### 代码示例

**合规的 Command Handler**：

```csharp
public class CreateOrderHandler : ICommandHandler<CreateOrder>
{
    public async Task<Guid> Handle(CreateOrder command)
    {
        // ✅ 加载/创建聚合
        var order = new Order(command.MemberId, command.Items);
        
        // ✅ 执行业务逻辑（在领域模型中）
        order.Calculate();
        
        // ✅ 保存
        await _repository.SaveAsync(order);
        
        // ✅ 发布事件（可选）
        await _eventBus.Publish(new OrderCreated(order.Id));
        
        return order.Id; // ✅ 仅返回 ID
    }
}
```

**合规的 Query Handler**：

```csharp
public class GetOrderByIdHandler : IQueryHandler<GetOrderById, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderById query)
    {
        // ✅ 只读查询
        var order = await _repository.GetByIdAsync(query.OrderId);
        
        // ✅ 返回 DTO
        return new OrderDto(order.Id, order.Total);
    }
}
```

**违规示例**：

```csharp
// ❌ Command Handler 返回业务数据
public async Task<OrderDto> Handle(CreateOrder command) { ... }

// ❌ Query Handler 修改状态
public async Task<OrderDto> Handle(GetOrderById query)
{
    await _repository.UpdateLastAccessedAsync(query.OrderId); // 修改状态
    return dto;
}

// ❌ Endpoint 包含业务逻辑
builder.MapPost("/orders", async (CreateOrderRequest request) =>
{
    if (request.Items.Count > 10) // 业务规则
        return Results.BadRequest();
    // ...
});

// ❌ Handler 返回领域实体
public async Task<Order> Handle(GetOrderById query) // 返回 Entity
{
    return await _repository.GetByIdAsync(query.OrderId);
}
```

### 检查清单

开发时自检：

- [ ] 每个用例唯一 Handler 归属？
- [ ] Endpoint 禁止写业务条件/分支/存储？
- [ ] Handler/Endpoint 不依赖 ASP.NET 类型？
- [ ] Handler 无持久状态？
- [ ] 跨模块所有调用为异步事件/契约？
- [ ] Contract 不含业务判断、行为方法？
- [ ] Query/Command Handler 职责完全分离？

### 版本历史

| 版本 | 日期 | 变更摘要 |
|------|------|---------|
| 4.0 | 2026-01-24 | 采用终极模板，明确规则与执法分离 |
| 3.0 | 2026-01-23 | 精简为极简判裁版，仅保裁决性规则 |
| 2.0 | 2026-01-20 | 作为"经验/治理+裁决"混合版 |
| 1.0 | 2026-01-20 | 初始发布 |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
