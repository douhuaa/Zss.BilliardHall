---
name: "Generate Endpoint"
description: "生成符合规范的 HTTP Endpoint 代码"
version: "1.0"
risk_level: "高"
category: "代码生成"
required_agent: "architecture-guardian"
dependencies:
  - "verify-module-structure"  # 前置：验证模块结构存在
  - "generate-handler"  # 可选：如果 Handler 不存在可能需要先生成
post_execution:
  - "run-architecture-tests"  # 建议：生成后运行架构测试验证
---

# Generate Endpoint Skill

**类别**：代码生成  
**风险等级**：高  
**版本**：1.0

---

## 功能定义

### 用途

生成符合规范的薄 HTTP Endpoint 适配器，确保 Endpoint 只做请求/响应映射，不包含业务逻辑。

### 输入参数

- `module`：字符串，模块名称
- `useCase`：字符串，用例名称
- `httpMethod`：字符串枚举，"GET" / "POST" / "PUT" / "DELETE" / "PATCH"
- `route`：字符串，路由路径（如 "/orders"）
- `requestType`：字符串，请求类型名称
- `responseType`：字符串，响应类型名称

### 输出结果

```json
{
  "generated": true,
  "files": [
    {
      "path": "src/Modules/Orders/UseCases/CreateOrder/CreateOrderEndpoint.cs",
      "content": "...",
      "type": "Endpoint"
    }
  ],
  "summary": {
    "httpMethod": "POST",
    "route": "/orders",
    "isThin": true,
    "businessLogicDetected": false
  }
}
```

---

## 前置条件

### 必须满足的条件

- [ ] Handler 已存在
- [ ] Request/Response 类型已定义
- [ ] 模块已注册 Endpoint

### 必须的 Agent 授权

- **需要**：`architecture-guardian`
- **理由**：Endpoint 是外部接口，必须严格遵守架构约束

---

## 执行步骤

1. **验证输入参数**
  - 检查 Handler 存在
  - 验证 HTTP 方法有效
  - 验证路由格式

2. **生成 Endpoint 代码**
  - 使用薄适配器模板
  - 只做请求映射
  - 委托给 Handler
  - 返回标准 HTTP 响应

3. **验证生成结果**
  - 检查无业务逻辑
  - 验证符合规范

4. **记录日志**

---

## Endpoint 模板

### Command Endpoint 模板（POST/PUT/DELETE）

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zss.BilliardHall.BuildingBlocks.Application.Messaging;

namespace Zss.BilliardHall.Modules.{Module}.UseCases.{UseCase};

public class {UseCase}Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.Map{HttpMethod}("{Route}", async (
            {RequestType} request,
            IMessageBus bus) =>
        {
            // ✅ 映射到命令
            var command = new {UseCase}(
                request.Property1,
                request.Property2
            );
            
            // ✅ 委托给 Handler
            var {resultId} = await bus.InvokeAsync(command);
            
            // ✅ 返回 HTTP 响应
            return Results.Created($"{Route}/{{{resultId}}}", {resultId});
        })
        .WithTags("{Module}")
        .WithName("{UseCase}")
        .Produces<{ResponseType}>({StatusCode});
    }
}
```

### Query Endpoint 模板（GET）

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zss.BilliardHall.BuildingBlocks.Application.Messaging;

namespace Zss.BilliardHall.Modules.{Module}.UseCases.{UseCase};

public class {UseCase}Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("{Route}", async (
            {QueryParams},
            IMessageBus bus) =>
        {
            // ✅ 映射到查询
            var query = new {UseCase}({QueryParams});
            
            // ✅ 委托给 Handler
            var result = await bus.InvokeAsync(query);
            
            // ✅ 返回 HTTP 响应
            return Results.Ok(result);
        })
        .WithTags("{Module}")
        .WithName("{UseCase}")
        .Produces<{ResponseType}>(200);
    }
}
```

---

## 验证规则

### Endpoint 必须是薄适配器

**✅ 允许**：
- 请求映射到 Command/Query
- 委托给 Handler
- 返回标准 HTTP 响应
- 添加元数据（Tags, Name, Produces）

**❌ 禁止**：
- 业务逻辑
- 数据验证（应在 Command/Query 中）
- 直接访问数据库
- 直接操作领域模型

### HTTP 状态码规范

| 操作 | 成功状态码 | 示例 |
|-----|----------|------|
| POST（创建） | 201 Created | `Results.Created()` |
| GET（查询） | 200 OK | `Results.Ok()` |
| PUT（更新） | 200 OK | `Results.Ok()` |
| DELETE（删除） | 204 No Content | `Results.NoContent()` |
| PATCH（部分更新） | 200 OK | `Results.Ok()` |

---

## 回滚机制

### 如何回滚

1. 删除生成的 Endpoint 文件
2. 从模块注册中移除 Endpoint

### 回滚验证

- [ ] Endpoint 文件已删除
- [ ] 应用可启动
- [ ] 路由表正确

---

## 危险信号

🚨 **必须阻止**：
- Endpoint 包含业务逻辑
- Endpoint 直接访问数据库
- Endpoint 包含验证逻辑
- Endpoint 直接操作领域模型

---

## 使用示例

### 示例 1：生成 POST Endpoint

**输入**：
```json
{
  "module": "Orders",
  "useCase": "CreateOrder",
  "httpMethod": "POST",
  "route": "/orders",
  "requestType": "CreateOrderRequest",
  "responseType": "Guid"
}
```

**输出**：
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
                request.Items
            );
            
            var orderId = await bus.InvokeAsync(command);
            
            return Results.Created($"/orders/{orderId}", orderId);
        })
        .WithTags("Orders")
        .WithName("CreateOrder")
        .Produces<Guid>(201);
    }
}
```

### 示例 2：生成 GET Endpoint

**输入**：
```json
{
  "module": "Orders",
  "useCase": "GetOrderById",
  "httpMethod": "GET",
  "route": "/orders/{id}",
  "responseType": "OrderDto"
}
```

**输出**：
```csharp
public class GetOrderByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/orders/{id}", async (
            Guid id,
            IMessageBus bus) =>
        {
            var query = new GetOrderById(id);
            var result = await bus.InvokeAsync(query);
            
            return Results.Ok(result);
        })
        .WithTags("Orders")
        .WithName("GetOrderById")
        .Produces<OrderDto>(200);
    }
}
```

---

## RuleSet API 集成

### 使用 RuleSetRegistry 查询 Endpoint 规范

生成 Endpoint 代码时，应从 RuleSetRegistry 获取薄适配器约束：

```csharp
using Zss.BilliardHall.Specification.Index;

// 获取 ADR-005 规则集（应用内交互模型 - Endpoint 规范）
var adr005 = RuleSetRegistry.GetStrict(5);
Console.WriteLine($"📋 基于 ADR-{adr005.AdrNumber:D3} 生成 Endpoint");

// 检查 Endpoint 薄适配器规则
var endpointRule = adr005.GetRule(4); // Endpoint 规则（假设）
if (endpointRule != null)
{
    Console.WriteLine($"   规则: {endpointRule.Title}");
}
```

### Endpoint 规则验证

根据 RuleSet 验证 Endpoint 是否为薄适配器：

```csharp
// 验证 Endpoint 不包含业务逻辑（基于 ADR-005）
var thinAdapterClause = adr005.GetClause(4, 1); // Endpoint 薄适配器条款
if (endpointContainsBusinessLogic)
{
    var ruleId = new ArchitectureRuleId(5, 4, 1);
    Console.WriteLine($"❌ 违反 {ruleId}：Endpoint 不能包含业务逻辑");
}

// 验证 HTTP 状态码规范
var statusCodeClause = adr005.GetClause(4, 2); // HTTP 状态码条款
if (httpMethod == "POST" && statusCode != 201)
{
    var ruleId = new ArchitectureRuleId(5, 4, 2);
    Console.WriteLine($"⚠️  建议使用 201 Created（{ruleId}）");
}
```

### 参考实现

当前没有专用的 GenerateEndpointCommandHandler，但可以参考：
- `GenerateTestCommandHandler.cs` - 使用 RuleSetRegistry 的模式
- `RunArchitectureTestsCommandHandler.cs` - RuleId 验证模式

---

## 参考资料

- **RuleSetRegistry API**：`src/tools/Specification/Index/RuleSetRegistry.cs`
- **IRuleSetQueryService**：`src/tools/Specification/Services/IRuleSetQueryService.cs`
- [ADR-005：应用内交互模型](../../../docs/adr/constitutional/ADR-005-Application-Interaction-Model-Final.md)
- [后端开发指令](../../instructions/backend.instructions.md)

---

**维护者**：架构委员会  
**状态**：✅ Active  
**版本**：1.1  
**最后更新**：2026-02-20  
**变更**：集成 RuleSetRegistry API，移除硬编码规则引用
