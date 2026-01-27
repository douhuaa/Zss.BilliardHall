---
adr: ADR-124
title: "Endpoint 命名及参数约束规范"
status: Accepted
level: Structure
deciders: "Architecture Board"
date: 2026-01-26
version: "1.0"
maintainer: "Architecture Board"
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---

# ADR-124：Endpoint 命名及参数约束规范

---
> ⚖️ **本 ADR 定义 HTTP Endpoint 命名、职责边界和单一调用约束的唯一裁决规则。**

**影响范围**：所有 HTTP Endpoint  

---

## 聚焦内容（Focus）

- Endpoint 类命名必须遵循 {UseCase}Endpoint 模式
- Request/Response DTO 命名规范
- Endpoint 禁止包含业务逻辑
- 一个 Endpoint 只能调用一个 Command 或 Query
- 复杂流程必须使用 Saga/Workflow 显式建模

---

## 术语表（Glossary）

| 术语         | 定义                            | 英文对照                |
|------------|-------------------------------|------------------------|
| Endpoint   | HTTP/gRPC 等协议的入口点，仅做请求适配      | Endpoint               |
| Request DTO | 请求数据传输对象，用于接收客户端请求          | Request DTO            |
| Response DTO | 响应数据传输对象，用于返回客户端响应         | Response DTO           |
| Saga       | 长事务协调器，用于管理跨多个聚合的业务流程      | Saga                   |
| L1 测试      | 静态可执行自动化测试                   | Level 1 Test           |
| L2 测试      | 语义半自动化测试或人工审查               | Level 2 Test           |

---

## 决策（Decision）

### ADR-124.1：Endpoint 类命名必须遵循 {UseCase}Endpoint 模式

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

### ADR-124.2：请求 DTO 命名必须遵循 {UseCase}Request 模式

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

### ADR-124.3：响应 DTO 命名必须遵循 {UseCase}Response 模式

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

### ADR-124.4：Endpoint 禁止包含业务逻辑

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

### ADR-124.5：一个 Endpoint 只能调用一个 Command 或 Query

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

## 快速参考表

| 约束编号       | 约束描述                   | 测试方式       | 测试用例                                      | 必须遵守 |
|------------|------------------------|------------|--------------------------------------------|------|
| ADR-124.1  | Endpoint 类命名规范        | L1 - 自动化测试 | Endpoints_Must_Follow_Naming_Convention      | ✅    |
| ADR-124.2  | Request DTO 命名规范      | L1 - 自动化测试 | Request_DTOs_Must_End_With_Request           | ✅    |
| ADR-124.3  | Response DTO 命名规范     | L1 - 自动化测试 | Response_DTOs_Must_End_With_Response         | ✅    |
| ADR-124.4  | Endpoint 禁止包含业务逻辑    | L2 - Code Review + Roslyn | Endpoints_Must_Not_Contain_Business_Logic  | ✅    |
| ADR-124.5  | 一个 Endpoint 只能调用一个操作 | L2 - Code Review | Endpoints_Must_Call_Single_Operation       | ✅    |

> **级别说明**：L1=静态自动化（脚本检查），L2=语义半自动或人工审查

---

## 必测/必拦架构测试（Enforcement）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_124_Architecture_Tests.cs` 强制验证：

- Endpoint 类命名是否符合 `{UseCase}Endpoint` 模式
- Request DTO 命名是否以 `Request` 结尾
- Response DTO 命名是否以 `Response` 结尾

**L2 测试**：
- 通过 Code Review 检查 Endpoint 是否包含业务逻辑
- 通过 Code Review 检查 Endpoint 是否只调用一个 Command/Query
- 建议使用 Roslyn Analyzer 检测业务逻辑特征

**有一项违规视为架构违规，CI 自动阻断。**

---

## 检查清单

- [ ] Endpoint 类名是否遵循 {UseCase}Endpoint 模式？
- [ ] Request DTO 是否以 Request 结尾？
- [ ] Response DTO 是否以 Response 结尾？
- [ ] Endpoint 是否只做 HTTP 映射，无业务逻辑？
- [ ] Endpoint 是否只调用一个 Command 或 Query？
- [ ] 复杂流程是否使用 Saga/Workflow？

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
- 指定失效日期（不超过 3 个月）

---

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0005：应用内交互模型与执行边界](../constitutional/ADR-0005-Application-Interaction-Model-Final.md) - Endpoint 约束基于 Handler 模式
- [ADR-121：契约 DTO 命名与组织](./ADR-121-contract-dto-naming-organization.md) - Endpoint 使用契约遵循命名规范
- [ADR-0006：术语与编号宪法](../constitutional/ADR-0006-terminology-numbering-constitution.md) - Endpoint 命名遵循术语规范

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

非裁决性参考（详细示例、Endpoint 实现最佳实践、REST API 设计）请查阅：
- `docs/copilot/adr-0124.prompts.md`
- [REST API Guidelines](https://restfulapi.net/)
- [Microsoft REST API Guidelines](https://github.com/microsoft/api-guidelines)
