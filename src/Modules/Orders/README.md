# Orders 模块

## 模块职责

管理台球馆订单的生命周期，包括：

- 订单创建
- 订单状态管理
- 订单计费

## 架构组织

本模块采用 **垂直切片架构**（Vertical Slice Architecture），按功能/用例组织代码：

```
Orders/
├── Features/
│   ├── CreateOrder/            # 创建订单切片
│   │   ├── CreateOrderCommand.cs
│   │   ├── CreateOrderCommandHandler.cs
│   │   └── CreateOrderEndpoint.cs
│   ├── CompleteOrder/          # 完成订单切片
│   │   └── ...
│   └── GetOrderById/           # 查询订单切片
│       └── ...
└── Orders.csproj
```

## 跨模块协作示例

### 场景：创建订单需要验证会员状态

**❌ 错误做法**（违反架构约束）：

```csharp
public class CreateOrderCommandHandler
{
    private readonly IMemberQueries _memberQueries; // ❌ 不允许
    
    public async Task Handle(CreateOrderCommand command)
    {
        var member = await _memberQueries.GetByIdAsync(command.MemberId);
        if (member?.Status != "Active") // ❌ 基于契约做业务决策
        {
            throw new Exception("会员未激活");
        }
        // ...
    }
}
```

**✅ 正确做法 1**：维护本地副本

```csharp
// Orders 模块维护会员状态的本地副本
public class CreateOrderCommandHandler
{
    private readonly IDocumentSession _session;
    
    public async Task Handle(CreateOrderCommand command)
    {
        // 从本地副本查询会员状态
        var memberStatus = await _session
            .Query<MemberStatusProjection>()
            .FirstOrDefaultAsync(m => m.MemberId == command.MemberId);
            
        if (memberStatus?.IsActive != true)
        {
            throw new Exception("会员未激活");
        }
        // ...
    }
}

// 通过订阅 Members 模块的领域事件保持同步
public class MemberActivatedEventHandler
{
    public async Task Handle(MemberActivated @event)
    {
        // 更新本地投影
    }
}
```

**✅ 正确做法 2**：发布验证命令

```csharp
public class CreateOrderCommandHandler
{
    private readonly IMessageBus _bus;
    
    public async Task Handle(CreateOrderCommand command)
    {
        // 发布验证命令到 Members 模块
        var isValid = await _bus.InvokeAsync<bool>(
            new ValidateMemberCommand(command.MemberId));
            
        if (!isValid)
        {
            throw new Exception("会员验证失败");
        }
        // ...
    }
}
```

## 垂直切片原则

### 每个切片（Feature）是自包含的

- 包含该用例的所有逻辑
- 不依赖横向 Service
- 不依赖其他模块的查询接口做业务决策

### 代码复用策略

如果多个切片有相似逻辑：

1. **优先考虑复制代码** - 保持切片独立性
2. 提取为领域服务 - 在模块内部，不要创建横向 Service
3. 使用领域事件 - 解耦切片间的依赖

## 依赖规则

本模块只能依赖：

- `Platform` - 技术能力
- 标准库类型
- 基础设施包

不能依赖：

- ❌ Members 模块
- ❌ Payments 模块
- ❌ 其他业务模块

## 参考

- [ADR-0001: 模块化单体与垂直切片架构决策](/docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
