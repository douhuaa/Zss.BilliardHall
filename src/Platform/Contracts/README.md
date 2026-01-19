# Contracts（数据契约）

## 目的

Contracts 是模块间通信的**数据载体**，仅用于传递只读数据，不包含业务逻辑或业务意图。

## 使用规则

### ✅ 允许的使用场景

1. **Query Handler** - 只读查询可以返回契约
2. **Endpoint / API** - 用于请求和响应数据传输
3. **Projection / ReadModel** - 视图模型可以使用契约
4. **Domain Events** - 事件载荷可以包含契约数据

### ❌ 禁止的使用场景

1. **Command Handler** - 不允许依赖其他模块的查询契约来做业务决策
2. **Domain Model** - 领域模型不应依赖契约
3. **Platform / Building Blocks** - 基础设施层不应依赖业务契约

## 设计原则

### 1. 契约应该是稳定的
- 一旦发布，避免破坏性变更
- 使用版本化策略处理演进

### 2. 契约应该是只读的
- 仅用于数据传递，不包含业务方法
- 属性应该是只读的（init 或 get-only）

### 3. 契约应该是简单的
- 避免复杂的嵌套结构
- 使用原始类型和标准库类型
- 不包含业务规则或验证逻辑

## 示例

### ✅ 正确的契约定义

```csharp
namespace Zss.BilliardHall.Platform.Contracts;

// 简单的数据传输对象
public record MemberDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public DateTime CreatedAt { get; init; }
}

// 查询接口（可以返回契约）
public interface IMemberQueries
{
    Task<MemberDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<MemberDto>> GetActiveMembers();
}
```

### ❌ 错误的使用方式

```csharp
// ❌ 错误：Command Handler 依赖其他模块的查询
public class CreateOrderHandler
{
    private readonly IMemberQueries _memberQueries;
    
    public CreateOrderHandler(IMemberQueries memberQueries)
    {
        _memberQueries = memberQueries; // 违规！
    }
    
    public async Task Handle(CreateOrder command)
    {
        // 使用契约做业务决策
        var member = await _memberQueries.GetByIdAsync(command.MemberId);
        if (member?.Status == "Active") // 违规！基于契约做业务判断
        {
            // ...
        }
    }
}

// ✅ 正确：通过领域事件或直接调用模块公开的命令
public class CreateOrderHandler
{
    public async Task Handle(CreateOrder command)
    {
        // 方案1: 发布命令让 Members 模块验证
        // await _mediator.Send(new ValidateMemberCommand(command.MemberId));
        
        // 方案2: 在 Order 模块内维护必要的会员状态副本
        // 通过订阅 MemberActivated/MemberDeactivated 事件保持同步
    }
}
```

## 架构测试保障

以下架构测试确保契约使用规则被遵守：

1. `CommandHandlers_Should_Not_Depend_On_IQuery_Interfaces` - 防止 Command Handler 依赖契约查询
2. `Platform_Should_Not_Depend_On_Module_Contracts` - 防止 Platform 依赖业务契约
3. 允许 Query Handler、Endpoint、ReadModel 使用契约

## 参考

- [ADR-0001: 模块化单体与垂直切片架构决策](/docs/adr/ADR-0001-modular-monolith-vertical-slice-architecture.md)
