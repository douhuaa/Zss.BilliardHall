# ADR-124：Endpoint 命名及参数约束规范

**状态**：Draft  
**级别**：结构层  
**影响范围**：所有 HTTP Endpoint  
**生效时间**：待审批通过后

---

## 规则本体（Rule）

> **这是本 ADR 唯一具有裁决力的部分。**

### ADR-124.1：Endpoint 类命名必须遵循 {UseCase}Endpoint 模式

Endpoint 类名**必须**为用例名 + `Endpoint` 后缀。

**命名规则**：
- ✅ `CreateOrderEndpoint`
- ✅ `GetOrderByIdEndpoint`
- ❌ `OrderEndpoint`（过于泛化）
- ❌ `CreateOrderController`（非 Endpoint 后缀）
- ❌ `CreateOrderApi`（非标准后缀）

### ADR-124.2：请求 DTO 命名必须遵循 {UseCase}Request 模式

请求 DTO 名称**必须**为用例名 + `Request` 后缀。

**命名规则**：
- ✅ `CreateOrderRequest`
- ✅ `UpdateMemberProfileRequest`
- ❌ `CreateOrderDto`（不明确是请求还是响应）
- ❌ `OrderRequest`（缺少用例动词）
- ❌ `CreateOrderInput`（非标准后缀）

### ADR-124.3：响应 DTO 命名必须遵循 {UseCase}Response 模式

响应 DTO 名称**必须**为用例名 + `Response` 后缀。

**命名规则**：
- ✅ `CreateOrderResponse`
- ✅ `GetOrderByIdResponse`
- ❌ `OrderDto`（不明确用途）
- ❌ `CreateOrderResult`（非标准后缀）

**简单响应例外**：
- 如仅返回 ID，可直接返回 `Guid` 而非创建 Response DTO

### ADR-124.4：Endpoint 禁止包含业务逻辑

Endpoint **禁止**包含任何业务逻辑，仅负责 HTTP 映射。

**允许的职责**：
- ✅ HTTP 请求映射到 Command/Query
- ✅ Command/Query 结果映射到 HTTP 响应
- ✅ HTTP 状态码设置
- ❌ 业务验证
- ❌ 数据转换逻辑
- ❌ 直接访问数据库

**正确示例**：
```csharp
public class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/orders", async (
            CreateOrderRequest request,
            IMessageBus bus) =>
        {
            var command = new CreateOrder(
                request.MemberId,
                request.Items);
            
            var orderId = await bus.InvokeAsync(command);
            
            return Results.Created($"/orders/{orderId}", orderId);
        });
    }
}
```

### ADR-124.5：一个 Endpoint 只能调用一个 Command 或 Query

每个 Endpoint 方法**必须**只调用一个 Command 或一个 Query。

**禁止模式**：
```csharp
// ❌ 错误：调用多个 Command/Query
var member = await bus.InvokeAsync(new GetMember(memberId));
var order = await bus.InvokeAsync(new CreateOrder(...));
await bus.InvokeAsync(new SendNotification(...));
```

**正确模式**：
```csharp
// ✅ 正确：只调用一个 Command
var orderId = await bus.InvokeAsync(new CreateOrder(...));
return Results.Created($"/orders/{orderId}", orderId);
```

**复杂流程处理**：
- 如需多步骤，应在 Handler 或 Saga 中编排，而非在 Endpoint 中
- 复杂业务流程**必须**通过 Saga / Workflow 显式建模
- **禁止**在 Handler 中隐式串联多个领域动作

**Saga 示例**：
```csharp
// ✅ 正确：复杂流程使用 Saga
public class OrderFulfillmentSaga
{
    public async Task Execute(FulfillOrder command)
    {
        await _bus.Send(new ValidateInventory(...));
        await _bus.Send(new ReserveFunds(...));
        await _bus.Send(new CreateShipment(...));
    }
}

// ❌ 错误：在 Endpoint 中串联多个操作
builder.MapPost("/orders/fulfill", async (request, bus) =>
{
    await bus.Send(new ValidateInventory(...));  // 多个调用
    await bus.Send(new ReserveFunds(...));
    await bus.Send(new CreateShipment(...));
});
```

---

## 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 测试映射

| 规则编号 | 执行级 | 测试/手段 |
|---------|--------|----------|
| ADR-124.1 | L1 | `Endpoints_Must_Follow_Naming_Convention` |
| ADR-124.2 | L1 | `Request_DTOs_Must_End_With_Request` |
| ADR-124.3 | L1 | `Response_DTOs_Must_End_With_Response` |
| ADR-124.4 | L2 | Code Review + Roslyn Analyzer |
| ADR-124.5 | L2 | Code Review |

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例**仅在以下情况允许**：

1. **遗留 API 兼容**：保持向后兼容性
2. **框架约束**：第三方框架强制要求
3. **批量操作端点**：合理的批量处理场景

### 破例要求（不可省略）

每个破例**必须**：

- 记录在 `docs/summaries/arch-violations.md`
- 说明兼容性或技术原因
- 提供迁移计划（如适用）

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

* **结构层 ADR**
  * 修改需 Tech Lead 审批
  * 需评估对现有 API 的影响

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

- ✗ HTTP 方法选择（GET/POST/PUT/DELETE）
- ✗ API 版本化策略
- ✗ 认证授权机制
- ✗ API 文档生成
- ✗ REST 与 GraphQL 的选择

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 相关 ADR
- ADR-0005：应用内交互模型与执行边界
- ADR-121：契约（Contract）与 DTO 命名组织规范

### REST API 设计
- [REST API Guidelines](https://restfulapi.net/)
- [Microsoft REST API Guidelines](https://github.com/microsoft/api-guidelines)

### 实践指导
- Endpoint 实现示例参见 `docs/copilot/adr-0124.prompts.md`

---

## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 |
|-----|------|---------|--------|
| 1.0 Draft | 2026-01-24 | 初始版本 | GitHub Copilot |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
