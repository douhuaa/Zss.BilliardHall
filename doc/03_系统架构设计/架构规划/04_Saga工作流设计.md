# Saga 工作流设计

> **定位**: Wolverine Saga 的使用场景与实现指南
> 
> **适用范围**: 跨步骤的业务流程编排

---

## 一、何时使用 Saga

### 1.1 适用场景

**典型特征**:
- 业务流程跨多个步骤
- 需要维护状态
- 涉及补偿/回滚逻辑
- 等待外部事件

**自助台球系统典型场景**:
```
开台 → 计时 → 暂停 → 恢复 → 结账 → 支付 → 关台
```

### 1.2 不适用场景

**不要使用 Saga**:
- 简单的一次性操作（直接用 Handler）
- 纯查询操作（使用 Query Handler）
- 单步骤流程（使用 Command Handler）

---

## 二、Saga 实现示例

### 2.1 TableSessionSaga

```csharp
namespace Zss.BilliardHall.Modules.Sessions.Sagas;

/// <summary>
/// 打球会话生命周期 Saga
/// </summary>
public sealed class TableSessionSaga : Saga
{
    // Saga 状态
    public Guid SessionId { get; set; }
    public Guid TableId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public SessionStatus Status { get; set; }
    
    /// <summary>
    /// 会话开始
    /// </summary>
    public void Start(SessionStarted @event)
    {
        SessionId = @event.SessionId;
        TableId = @event.TableId;
        StartTime = @event.StartedAt;
        Status = SessionStatus.Active;
    }

    /// <summary>
    /// 会话暂停
    /// </summary>
    public void Pause(SessionPaused @event)
    {
        if (Status != SessionStatus.Active)
            throw new InvalidOperationException("只有活跃会话可以暂停");
            
        Status = SessionStatus.Paused;
    }

    /// <summary>
    /// 会话恢复
    /// </summary>
    public void Resume(SessionResumed @event)
    {
        if (Status != SessionStatus.Paused)
            throw new InvalidOperationException("只有暂停会话可以恢复");
            
        Status = SessionStatus.Active;
    }

    /// <summary>
    /// 会话结束（等待支付）
    /// </summary>
    public void End(SessionEnded @event)
    {
        EndTime = @event.EndedAt;
        Status = SessionStatus.PendingPayment;
    }

    /// <summary>
    /// 支付完成（结束 Saga）
    /// </summary>
    public void Complete(PaymentCompleted @event)
    {
        if (@event.SessionId != SessionId)
            return;
            
        Status = SessionStatus.Completed;
        
        // 标记 Saga 完成
        MarkCompleted();
    }
}
```

### 2.2 Saga 配置

```csharp
// 在 Program.cs 中配置
builder.Host.UseWolverine(opts =>
{
    opts.Policies.UseDurableInboxOnAllListeners();
    opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
    
    // 启用 Saga 持久化
    opts.Services.AddMarten(marten =>
    {
        marten.Connection(connectionString);
        
        // 注册 Saga
        marten.Schema.For<TableSessionSaga>()
            .Identity(x => x.SessionId);
    });
});
```

---

## 三、Saga 状态管理

### 3.1 状态持久化

**自动持久化**:
- Saga 状态自动保存到 Marten
- 每次事件处理后自动更新状态
- 支持查询 Saga 当前状态

**状态查询**:
```csharp
public class GetSessionSagaHandler
{
    public async Task<TableSessionSaga?> Handle(
        GetSessionSaga query,
        IDocumentSession session,
        CancellationToken ct)
    {
        return await session.LoadAsync<TableSessionSaga>(query.SessionId, ct);
    }
}
```

### 3.2 状态转换

**状态机示例**:
```
Available → Active → Paused → Active → PendingPayment → Completed
                              ↓
                           Cancelled
```

**状态转换规则**:
- Active → Paused（暂停）
- Paused → Active（恢复）
- Active → PendingPayment（结账）
- PendingPayment → Completed（支付成功）
- Any → Cancelled（取消）

---

## 四、超时处理

### 4.1 超时配置

```csharp
public sealed class TableSessionSaga : Saga
{
    // ...其他代码
    
    /// <summary>
    /// 会话开始后设置超时
    /// </summary>
    public void Start(SessionStarted @event)
    {
        SessionId = @event.SessionId;
        StartTime = @event.StartedAt;
        Status = SessionStatus.Active;
        
        // 8小时后自动超时
        ScheduleTimeout<SessionTimeout>(TimeSpan.FromHours(8));
    }
    
    /// <summary>
    /// 处理超时
    /// </summary>
    public void Handle(SessionTimeout timeout)
    {
        if (Status == SessionStatus.Active)
        {
            Status = SessionStatus.TimedOut;
            MarkCompleted();
        }
    }
}

public record SessionTimeout(Guid SessionId);
```

---

## 五、补偿逻辑

### 5.1 补偿示例

```csharp
public sealed class OrderSaga : Saga
{
    public Guid OrderId { get; set; }
    public bool StockReserved { get; set; }
    public bool PaymentProcessed { get; set; }
    
    public void Handle(StockReserved @event)
    {
        StockReserved = true;
    }
    
    public void Handle(PaymentFailed @event)
    {
        // 补偿：释放库存
        if (StockReserved)
        {
            Publish(new ReleaseStock(OrderId));
        }
        
        MarkCompleted();
    }
}
```

---

## 六、最佳实践

### 6.1 Saga 设计原则

**DO（应该做）**:
- ✅ Saga 只存储必要的状态标识（ID、状态枚举）
- ✅ 使用 `Complete()` 显式结束 Saga
- ✅ Handler 方法保持幂等性
- ✅ 考虑超时处理
- ✅ 设计补偿逻辑

**DON'T（不应该做）**:
- ❌ 不在 Saga 中存储大量数据
- ❌ 不在 Saga 中执行复杂业务逻辑
- ❌ 不让 Saga 永久存在（必须有结束条件）

### 6.2 事件命名

**推荐命名**:
- 事件：名词 + 动词过去式（`OrderCreated`）
- 命令：动词 + 名词（`CreateOrder`）
- 超时：名词 + `Timeout`（`PaymentTimeout`）

### 6.3 状态查询

**支持状态查询**:
```csharp
// 查询当前 Saga 状态
var saga = await session.LoadAsync<TableSessionSaga>(sessionId);
if (saga?.Status == SessionStatus.Active)
{
    // 处理逻辑
}
```

---

## 七、调试 Saga

### 7.1 查看 Saga 状态

```csharp
// 在 Handler 中注入 IDocumentSession
public async Task Debug(IDocumentSession session)
{
    // 查询所有活跃的 Saga
    var activeSagas = await session
        .Query<TableSessionSaga>()
        .Where(x => x.Status == SessionStatus.Active)
        .ToListAsync();
        
    // 记录状态
    foreach (var saga in activeSagas)
    {
        logger.LogInformation(
            "Saga {SessionId} - Status: {Status}",
            saga.SessionId,
            saga.Status
        );
    }
}
```

### 7.2 常见问题

**问题 1: Saga 未被触发**
- 检查事件是否正确发布
- 检查 Saga 是否正确注册到 Marten
- 检查事件 Handler 方法签名

**问题 2: Saga 状态未更新**
- 检查是否启用事务
- 检查 Marten 配置是否正确
- 检查数据库连接

**问题 3: Saga 永久存在**
- 确保所有路径都调用 `MarkCompleted()`
- 添加超时机制
- 定期清理完成的 Saga

---

## 八、参考资源

### 相关文档
- [06_开发规范/Saga使用指南.md](../../06_开发规范/Saga使用指南.md) - 详细的 Saga 使用指南
- [04_模块设计/打球时段模块.md](../../04_模块设计/打球时段模块.md) - TableSessionSaga 完整示例

### 外部资源
- [Wolverine Saga Documentation](https://wolverine.netlify.app/guide/durability/sagas.html)
- [Saga Pattern](https://microservices.io/patterns/data/saga.html)

---

## 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| 1.0.0 | 2024-01-15 | 从 Wolverine模块化架构蓝图 拆分 |

---

**最后更新**: 2024-01-15  
**负责人**: 架构团队  
**审核状态**: ✅ 已审核
