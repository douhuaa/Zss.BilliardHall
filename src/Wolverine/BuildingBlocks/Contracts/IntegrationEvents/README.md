# Integration Events

## 用途

此文件夹存放**跨服务的集成事件**（Integration Events）。

## 与其他事件类型的区别

| 事件类型 | 范围 | 存放位置 | 可修改性 |
|---------|------|---------|---------|
| **Domain Event** | 模块内 | `Modules/{Module}/Events/` | ✅ 可自由修改 |
| **Module Event** | 本进程跨模块 | `Modules/{Module}/Events/` | ⚠️ 需考虑消费者 |
| **Integration Event** | 跨服务 | `BuildingBlocks/Contracts/IntegrationEvents/` | ❌ 严格版本管理 |

## 规范

### 命名规范
- 格式：`{Entity}{Action}IntegrationEvent`
- 示例：`PaymentCompletedIntegrationEvent`、`MemberRegisteredIntegrationEvent`

### 必须实现的接口
```csharp
public sealed record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    DateTimeOffset CompletedAt
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
```

### 准入标准

Integration Event 必须满足以下所有条件：

1. ✅ **跨服务通信**：被外部服务（非本进程）消费
2. ✅ **稳定契约**：变更需要严格的版本管理和兼容性保证
3. ✅ **实现 IIntegrationEvent**：包含 EventId 和 OccurredAt
4. ✅ **不可变**：使用 `record` 且属性为 `init`

### 修改影响

⚠️ **修改 Integration Event 会影响所有消费服务**

修改前必须：
1. 评估所有消费方的影响范围
2. 制定兼容性策略（新增字段 / 版本号升级）
3. 与所有相关团队沟通
4. 更新 API 文档和 Schema
5. 考虑使用事件版本号（v1、v2）

### 反模式

❌ **错误 1：将 Module Event 放在这里**
```csharp
// ❌ 这是跨模块事件，不是跨服务事件
// 应该在 Modules/Sessions/Events/
public sealed record SessionStarted(...);
```

❌ **错误 2：频繁修改**
```csharp
// ❌ 随意添加字段，破坏兼容性
public sealed record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    string NewField  // 突然添加，破坏消费者
);
```

## 示例

### ✅ 正确的 Integration Event

```csharp
namespace Zss.BilliardHall.BuildingBlocks.Contracts.IntegrationEvents;

/// <summary>
/// 支付完成集成事件（跨服务）
/// </summary>
/// <remarks>
/// 消费者：
/// - 外部财务系统
/// - 第三方对账服务
/// - BI 数据仓库
/// </remarks>
public sealed record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid MemberId,
    decimal Amount,
    string PaymentMethod,
    DateTimeOffset CompletedAt
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
```

### ✅ 版本化的 Integration Event

```csharp
namespace Zss.BilliardHall.BuildingBlocks.Contracts.IntegrationEvents.V2;

/// <summary>
/// 支付完成集成事件 V2（新增字段）
/// </summary>
public sealed record PaymentCompletedIntegrationEventV2(
    Guid PaymentId,
    Guid OrderId,
    Guid MemberId,
    decimal Amount,
    string PaymentMethod,
    string Currency,  // V2 新增
    DateTimeOffset CompletedAt
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
```

## 参考

详见：`doc/03_系统架构设计/Wolverine模块化架构蓝图.md` - 2.4 事件分类与边界管理
