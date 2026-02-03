---
adr: ADR-124
title: "Endpoint 命名及参数约束规范"
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


# ADR-124：Endpoint 命名及参数约束规范

> ⚖️ **本 ADR 定义 HTTP Endpoint 命名、职责边界和单一调用约束的唯一裁决规则。**

**影响范围**：所有 HTTP Endpoint  
## Focus（聚焦内容）

- Endpoint 类命名必须遵循 {UseCase}Endpoint 模式
- Request/Response DTO 命名规范
- Endpoint 禁止包含业务逻辑
- 一个 Endpoint 只能调用一个 Command 或 Query
- 复杂流程必须使用 Saga/Workflow 显式建模

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------------|-------------------------------|------------------------|
| Endpoint   | HTTP/gRPC 等协议的入口点，仅做请求适配      | Endpoint               |
| Request DTO | 请求数据传输对象，用于接收客户端请求          | Request DTO            |
| Response DTO | 响应数据传输对象，用于返回客户端响应         | Response DTO           |
| Saga       | 长事务协调器，用于管理跨多个聚合的业务流程      | Saga                   |
| L1 测试      | 静态可执行自动化测试                   | Level 1 Test           |
| L2 测试      | 语义半自动化测试或人工审查               | Level 2 Test           |

---

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-124 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-124_<Rule>_<Clause>
> ```

---

### ADR-124_1：Endpoint 命名规范（Rule）

#### ADR-124_1_1 Endpoint 类命名必须遵循 {UseCase}Endpoint 模式

**规则**：
- Endpoint 类名**必须**为用例名 + `Endpoint` 后缀
- 禁止使用 `Controller`、`Api` 等其他后缀
- 禁止过于泛化的命名（如 `OrderEndpoint`）

**判定**：
- ✅ `CreateOrderEndpoint`
- ✅ `GetOrderByIdEndpoint`
- ❌ `OrderEndpoint`（过于泛化）
- ❌ `CreateOrderController`（非 Endpoint 后缀）
- ❌ `CreateOrderApi`（非标准后缀）

#### ADR-124_1_2 请求 DTO 命名必须遵循 {UseCase}Request 模式

**规则**：
- 请求 DTO 名称**必须**为用例名 + `Request` 后缀
- 必须包含用例动词，不可省略
- 禁止使用 `Input`、`Dto` 等其他后缀

**判定**：
- ✅ `CreateOrderRequest`
- ✅ `UpdateMemberProfileRequest`
- ❌ `CreateOrderDto`（不明确是请求还是响应）
- ❌ `OrderRequest`（缺少用例动词）
- ❌ `CreateOrderInput`（非标准后缀）

#### ADR-124_1_3 响应 DTO 命名必须遵循 {UseCase}Response 模式

**规则**：
- 响应 DTO 名称**必须**为用例名 + `Response` 后缀
- 简单响应（仅返回 ID）可直接返回原始类型
- 禁止使用 `Result`、`Dto` 等其他后缀

**判定**：
- ✅ `CreateOrderResponse`
- ✅ `GetOrderByIdResponse`
- ✅ `Guid`（仅返回 ID 的简单情况）
- ❌ `OrderDto`（不明确用途）
- ❌ `CreateOrderResult`（非标准后缀）

---

### ADR-124_2：Endpoint 职责边界（Rule）

#### ADR-124_2_1 Endpoint 禁止包含业务逻辑

**规则**：
- Endpoint **禁止**包含任何业务逻辑
- Endpoint **仅允许**：HTTP 请求映射、响应映射、状态码设置
- Endpoint **禁止**：业务验证、数据转换逻辑、直接访问数据库

**判定**：

**✅ 允许的职责**：
```csharp
public class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/orders", async (
            CreateOrderRequest request,
            IMessageBus bus) =>
        {
            // ✅ HTTP 请求映射到 Command
            var command = new CreateOrder(request.MemberId, request.Items);
            
            // ✅ 委托给 Handler
            var orderId = await bus.InvokeAsync(command);
            
            // ✅ 返回 HTTP 响应
            return Results.Created($"/orders/{orderId}", orderId);
        });
    }
}
```

**❌ 禁止的行为**：
```csharp
// ❌ 业务验证
if (request.Items.Count == 0)
    return Results.BadRequest("订单必须包含至少一个商品");

// ❌ 数据转换逻辑
var total = request.Items.Sum(i => i.Price * i.Quantity);

// ❌ 直接访问数据库
var order = await _dbContext.Orders.FindAsync(id);
```

#### ADR-124_2_2 一个 Endpoint 只能调用一个 Command 或 Query

**规则**：
- 每个 Endpoint 方法**必须**只调用一个 Command 或一个 Query
- 禁止在 Endpoint 中串联多个 Command/Query
- 复杂流程**必须**通过 Saga/Workflow 显式建模

**判定**：

**✅ 正确模式**：
```csharp
// ✅ 只调用一个 Command
var orderId = await bus.InvokeAsync(new CreateOrder(...));
return Results.Created($"/orders/{orderId}", orderId);
```

**❌ 禁止模式**：
```csharp
// ❌ 调用多个 Command/Query
var member = await bus.InvokeAsync(new GetMember(memberId));
var order = await bus.InvokeAsync(new CreateOrder(...));
await bus.InvokeAsync(new SendNotification(...));
```

**✅ 复杂流程使用 Saga**：
```csharp
// ✅ 复杂流程使用 Saga
public class OrderFulfillmentSaga
{
    public async Task Execute(FulfillOrder command)
    {
        await _bus.Send(new ValidateInventory(...));
        await _bus.Send(new ReserveFunds(...));
        await _bus.Send(new CreateShipment(...));
    }
}

// ✅ Endpoint 只调用 Saga
builder.MapPost("/orders/fulfill", async (request, bus) =>
{
    await bus.Send(new FulfillOrder(...));
});
```

---

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-124 各条款（Clause）的执法方式及执行级别。
>
> 所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_124_Architecture_Tests.cs` 强制验证。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-124_1_1** | L1 | ArchitectureTests 验证 Endpoint 类命名模式 | §ADR-124_1_1 |
| **ADR-124_1_2** | L1 | ArchitectureTests 验证 Request DTO 命名 | §ADR-124_1_2 |
| **ADR-124_1_3** | L1 | ArchitectureTests 验证 Response DTO 命名 | §ADR-124_1_3 |
| **ADR-124_2_1** | L2 | Code Review + Roslyn Analyzer 检测业务逻辑 | §ADR-124_2_1 |
| **ADR-124_2_2** | L2 | Code Review 检查单一调用约束 | §ADR-124_2_2 |

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
- [ADR-005：应用内交互模型与执行边界](../constitutional/ADR-005-Application-Interaction-Model-Final.md) - Endpoint 约束基于 Handler 模式
- [ADR-121：契约 DTO 命名与组织](./ADR-121-contract-dto-naming-organization.md) - Endpoint 使用契约遵循命名规范
- [ADR-006：术语与编号宪法](../constitutional/ADR-006-terminology-numbering-constitution.md) - Endpoint 命名遵循术语规范

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
