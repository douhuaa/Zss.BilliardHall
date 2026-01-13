# Saga 使用指南

> **目标读者**: 需要实现跨模块长事务编排的开发人员
>
> **阅读时间**: 10 分钟
>
> **前置要求**: 
> - 熟悉 Wolverine 基础概念
> - 了解事件驱动架构
> - 阅读过 [Wolverine模块化架构蓝图](../03_系统架构设计/Wolverine模块化架构蓝图.md)

---

## 一、何时使用 Saga

当业务流程跨越多个步骤、需要维护状态或涉及补偿逻辑时，使用 Wolverine Saga。

### 典型场景

- **跨模块的长时间运行业务流程**：如订单处理流程跨越订单、库存、支付、物流多个模块
- **需要等待外部事件的流程**：如支付回调、第三方系统响应
- **需要补偿/回滚的分布式事务**：如订单取消需要回滚库存、退款等操作

### 自助台球系统典型场景

```
开台 → 计时 → 暂停 → 恢复 → 结账 → 支付 → 关台
```

这是一个典型的跨模块长事务流程：
- Sessions 模块：时段管理
- Billing 模块：账单计算
- Payments 模块：支付处理
- Tables 模块：桌台状态管理

---

## 二、TableSessionSaga 完整示例

### 2.1 Saga 定义

```csharp
// 位置：Modules/Sessions/Sagas/TableSessionSaga.cs
namespace Zss.BilliardHall.Modules.Sessions.Sagas;

/// <summary>
/// 打球会话生命周期 Saga
/// 管理从开台到支付完成的完整流程
/// </summary>
public sealed class TableSessionSaga : Saga
{
    // Saga 状态（只存储必要的标识信息）
    public Guid SessionId { get; set; }
    public Guid TableId { get; set; }
    public Guid? BillId { get; set; }
    public SessionStatus Status { get; set; }
    
    /// <summary>
    /// 步骤 1: 时段开始
    /// 触发事件：SessionStarted
    /// </summary>
    public void Handle(SessionStarted @event)
    {
        SessionId = @event.SessionId;
        TableId = @event.TableId;
        Status = SessionStatus.Active;
    }
    
    /// <summary>
    /// 步骤 2: 时段结束 → 等待账单计算
    /// 触发事件：SessionEnded
    /// </summary>
    public void Handle(SessionEnded @event)
    {
        Status = SessionStatus.Ended;
    }
    
    /// <summary>
    /// 步骤 3: 账单计算完成 → 等待支付
    /// 触发事件：BillCalculated（来自 Billing 模块）
    /// </summary>
    public void Handle(BillCalculated @event)
    {
        BillId = @event.BillId;
    }
    
    /// <summary>
    /// 步骤 4: 支付完成 → 时段完成
    /// 触发事件：PaymentCompleted（来自 Payments 模块）
    /// </summary>
    public void Handle(PaymentCompleted @event)
    {
        if (@event.SessionId == SessionId)
        {
            Status = SessionStatus.Completed;
            Complete();  // 完成 Saga，自动清理状态
        }
    }
}
```

### 2.2 Saga 配置

在 `Program.cs` 或 `PersistenceExtensions.cs` 中配置：

```csharp
builder.Services.AddMarten(marten =>
{
    marten.Connection(connectionString);
    
    // 注册 Saga，使用 SessionId 作为唯一标识
    marten.Schema.For<TableSessionSaga>()
        .Identity(x => x.SessionId);
    
    // 可选：启用 Saga 超时处理
    // marten.Schema.For<TableSessionSaga>()
    //     .SoftDeleted()
    //     .DeletedAt(x => x.CompletedAt);
});

builder.Host.UseWolverine(opts =>
{
    // 启用持久化消息
    opts.PersistMessagesWithMarten();
    
    // 启用 Outbox 模式（确保 Saga 状态变更与事件发布原子性）
    opts.Policies.UseDurableInboxOnAllListeners();
    opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
});
```

### 2.3 事件定义

```csharp
// Sessions 模块事件
public sealed record SessionStarted(
    Guid SessionId,
    Guid TableId,
    Guid? MemberId,
    DateTimeOffset StartedAt
);

public sealed record SessionEnded(
    Guid SessionId,
    Guid TableId,
    TimeSpan Duration,
    DateTimeOffset EndedAt
);

// Billing 模块事件
public sealed record BillCalculated(
    Guid BillId,
    Guid SessionId,
    decimal Amount,
    DateTimeOffset CalculatedAt
);

// Payments 模块事件
public sealed record PaymentCompleted(
    Guid PaymentId,
    Guid SessionId,
    decimal Amount,
    DateTimeOffset CompletedAt
);
```

---

## 三、最佳实践

### 3.1 状态存储原则

**✅ 推荐**：只存储必要的标识信息
```csharp
public sealed class TableSessionSaga : Saga
{
    public Guid SessionId { get; set; }        // ✅ 必要标识
    public Guid TableId { get; set; }          // ✅ 必要标识
    public SessionStatus Status { get; set; }   // ✅ 状态枚举
}
```

**❌ 避免**：存储完整业务对象
```csharp
public sealed class TableSessionSaga : Saga
{
    public TableSession Session { get; set; }   // ❌ 完整对象，冗余
    public Table Table { get; set; }            // ❌ 完整对象，冗余
}
```

### 3.2 显式完成 Saga

**必须显式标记 Saga 完成**，否则状态会一直保留在数据库中：

```csharp
public void Handle(PaymentCompleted @event)
{
    Status = SessionStatus.Completed;
    
    // 方式 1：调用 Complete()
    Complete();
    
    // 方式 2：调用 MarkCompleted()
    // MarkCompleted();
}
```

### 3.3 幂等性处理

Saga Handler 方法应保持幂等性，防止重复处理：

```csharp
public void Handle(PaymentCompleted @event)
{
    // ✅ 检查是否已完成，防止重复处理
    if (Status == SessionStatus.Completed)
        return;
    
    // ✅ 检查 SessionId 匹配
    if (@event.SessionId != SessionId)
        return;
    
    Status = SessionStatus.Completed;
    Complete();
}
```

### 3.4 超时处理

对于可能长时间等待的 Saga，考虑添加超时机制：

```csharp
public sealed class TableSessionSaga : Saga
{
    public Guid SessionId { get; set; }
    public SessionStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
    // 超时处理：24 小时后自动取消
    public void Handle(SagaTimeout @event)
    {
        if (Status != SessionStatus.Completed)
        {
            Status = SessionStatus.Timeout;
            Complete();
        }
    }
}

// 在 Wolverine 配置中启用超时
opts.Policies.ConfigureSaga<TableSessionSaga>()
    .Timeout(TimeSpan.FromHours(24));
```

### 3.5 错误处理与补偿

当流程中某个步骤失败时，Saga 应触发补偿逻辑：

```csharp
public sealed class TableSessionSaga : Saga
{
    // ... 状态定义 ...
    
    /// <summary>
    /// 支付失败 → 触发补偿
    /// </summary>
    public async Task Handle(
        PaymentFailed @event,
        IMessageBus bus)
    {
        if (@event.SessionId == SessionId)
        {
            Status = SessionStatus.PaymentFailed;
            
            // 补偿操作：释放桌台、取消账单
            await bus.PublishAsync(new ReleaseTable(TableId));
            await bus.PublishAsync(new CancelBill(BillId.Value));
            
            Complete();
        }
    }
}
```

---

## 四、调试与监控

### 4.1 查看 Saga 状态

```sql
-- 查看所有活跃的 Saga
SELECT * FROM mt_doc_tablesessionsaga 
WHERE data->>'Status' != 'Completed';

-- 查看特定会话的 Saga 状态
SELECT * FROM mt_doc_tablesessionsaga 
WHERE id = 'your-session-id';
```

### 4.2 日志记录

在 Saga Handler 中添加结构化日志：

```csharp
public void Handle(
    SessionStarted @event,
    ILogger<TableSessionSaga> logger)
{
    SessionId = @event.SessionId;
    TableId = @event.TableId;
    Status = SessionStatus.Active;
    
    logger.LogInformation(
        "TableSessionSaga started: SessionId={SessionId}, TableId={TableId}",
        SessionId, TableId
    );
}
```

### 4.3 性能考虑

- Saga 状态存储在数据库中，每次事件处理都会有 I/O 开销
- 对于高频事件，考虑使用缓存或内存状态
- 定期清理已完成的 Saga 状态（软删除 + 归档）

---

## 五、常见问题

### Q1: Saga 与普通 Event Handler 的区别？

**Saga**：
- 有状态（存储在数据库中）
- 跨越多个步骤
- 需要等待外部事件
- 适合长事务编排

**Event Handler**：
- 无状态（或使用临时变量）
- 处理单个事件
- 立即完成
- 适合简单响应式逻辑

### Q2: 何时应该拆分 Saga？

当 Saga 出现以下情况时，考虑拆分：
- 处理超过 5 个事件类型
- 状态字段超过 10 个
- 包含多个独立的子流程

### Q3: Saga 失败后如何恢复？

Wolverine 会自动重试失败的 Handler。如果需要手动干预：

1. 查询数据库中的 Saga 状态
2. 手动发送补偿事件
3. 或直接修改 Saga 状态并标记完成

---

## 六、相关文档

- [Wolverine 模块化架构蓝图](../03_系统架构设计/Wolverine模块化架构蓝图.md) - 第五章：Saga 跨步骤业务流程
- [打球时段模块](../04_模块设计/打球时段模块.md) - TableSessionSaga 完整实现
- [Wolverine 官方文档 - Sagas](https://wolverine.netlify.app/guide/durability/sagas.html)

---

## 七、TODO 标签使用

在代码中添加 TODO 标签时，引用本文档：

```csharp
// TODO(wolverine): 若需添加 Saga，参考 TableSessionSaga 示例
// 详细文档：doc/06_开发规范/Saga使用指南.md
// 模块示例：doc/04_模块设计/打球时段模块.md #section-saga
```

---

**最后更新**: 2026-01-11  
**负责人**: 架构团队  
**审核状态**: ✅ 已审核
