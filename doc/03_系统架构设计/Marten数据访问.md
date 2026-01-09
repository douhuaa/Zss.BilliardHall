# Marten 数据访问

## 1. 概述

Marten 是一个 .NET 的文档数据库和事件存储库，基于 PostgreSQL。它允许开发者使用 PostgreSQL 的 JSONB 功能来存储和查询文档，同时保留关系数据库的优势（ACID 事务、索引、全文搜索等）。

**官方网站**: https://martendb.io/

**核心理念**: "PostgreSQL 作为文档数据库"

## 2. 核心特性

### 2.1 文档存储

Marten 将 .NET 对象序列化为 JSON 存储在 PostgreSQL 中：

```csharp
// 定义实体（普通 POCO，无需继承基类）
public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public MembershipLevel Level { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// 存储文档
using var session = store.LightweightSession();
var member = new Member
{
    Id = Guid.NewGuid(),
    Name = "张三",
    Phone = "13800138000",
    Email = "zhang@example.com",
    Balance = 0,
    Level = MembershipLevel.Regular,
    CreatedAt = DateTime.UtcNow
};

session.Store(member);
await session.SaveChangesAsync();

// 查询文档
var member = await session.LoadAsync<Member>(memberId);

// LINQ 查询
var activeMembers = await session
    .Query<Member>()
    .Where(m => m.Balance > 0)
    .OrderBy(m => m.Name)
    .ToListAsync();
```

**背后的实现**:
```sql
-- Marten 自动创建表
CREATE TABLE mt_doc_member (
    id UUID PRIMARY KEY,
    data JSONB NOT NULL,
    mt_version INT NOT NULL DEFAULT 1,
    mt_last_modified TIMESTAMP NOT NULL DEFAULT now()
);

-- 插入数据
INSERT INTO mt_doc_member (id, data, mt_version)
VALUES (
    '123e4567-e89b-12d3-a456-426614174000',
    '{"Id": "123e4567-e89b-12d3-a456-426614174000", "Name": "张三", "Phone": "13800138000", ...}',
    1
);
```

### 2.2 强类型查询

Marten 支持 LINQ，并将其转换为高效的 SQL 查询：

```csharp
// 简单查询
var member = await session.Query<Member>()
    .FirstOrDefaultAsync(m => m.Email == "zhang@example.com");

// 复杂查询
var premiumMembers = await session.Query<Member>()
    .Where(m => m.Level == MembershipLevel.Premium && m.Balance > 1000)
    .OrderByDescending(m => m.Balance)
    .Take(10)
    .ToListAsync();

// JSON 属性查询
var members = await session.Query<Member>()
    .Where(m => m.Phone.StartsWith("138"))
    .ToListAsync();

// 全文搜索（需要配置）
var results = await session.Query<Member>()
    .Where(m => m.Search(x => x.Name, "张三"))
    .ToListAsync();
```

### 2.3 乐观并发控制

```csharp
// 配置并发检查
builder.Services.AddMarten(opts =>
{
    opts.Schema.For<Member>().UseOptimisticConcurrency(true);
});

// 更新时自动检查版本
var member = await session.LoadAsync<Member>(memberId);
member.Balance += 100;

try
{
    await session.SaveChangesAsync(); // 如果版本不匹配，抛出异常
}
catch (ConcurrencyException ex)
{
    // 处理并发冲突
    logger.LogWarning("并发冲突: {MemberId}", memberId);
}
```

### 2.4 索引优化

Marten 支持多种索引策略：

```csharp
builder.Services.AddMarten(opts =>
{
    opts.Schema.For<Member>()
        // GIN 索引（用于 JSONB 查询）
        .GinIndexJsonData()
        
        // 特定字段索引
        .Index(x => x.Email, idx =>
        {
            idx.IsUnique = true; // 唯一索引
            idx.IsConcurrent = true; // 并发创建索引
        })
        
        // 复合索引
        .Index(x => x.Level, x => x.CreatedAt)
        
        // 全文搜索索引
        .FullTextIndex(x => x.Name);
});
```

**生成的 SQL**:
```sql
CREATE UNIQUE INDEX mt_doc_member_email ON mt_doc_member USING btree ((data->>'Email'));
CREATE INDEX mt_doc_member_level_createdat ON mt_doc_member USING btree ((data->>'Level'), (data->>'CreatedAt'));
CREATE INDEX mt_doc_member_name_fts ON mt_doc_member USING gin (to_tsvector('english', data->>'Name'));
```

### 2.5 事务支持

Marten 完全支持 PostgreSQL 事务：

```csharp
// 自动事务（SaveChanges 时提交）
using var session = store.LightweightSession();
session.Store(member);
session.Store(auditLog);
await session.SaveChangesAsync(); // 自动包装在事务中

// 显式事务控制
using var session = store.LightweightSession();
using var tx = session.Connection.BeginTransaction();
session.EnlistInTransaction(tx);

try
{
    session.Store(member);
    session.Store(payment);
    await session.SaveChangesAsync();
    
    await tx.CommitAsync();
}
catch
{
    await tx.RollbackAsync();
    throw;
}

// 与 Wolverine 集成（自动事务管理）
public class CreateMemberHandler
{
    // Wolverine + Marten 自动管理事务
    public async Task<MemberCreated> Handle(
        CreateMemberCommand command,
        IDocumentSession session) // 自动参与事务
    {
        var member = new Member { /* ... */ };
        session.Store(member);
        // SaveChanges 由 Wolverine 在处理器完成后自动调用
        return new MemberCreated(member.Id);
    }
}
```

### 2.6 事件溯源（Event Sourcing）

Marten 内置了完整的事件溯源支持：

```csharp
// 定义事件
public record SessionStarted(Guid SessionId, Guid TableId, DateTime StartTime);
public record SessionPaused(Guid SessionId, DateTime PauseTime);
public record SessionResumed(Guid SessionId, DateTime ResumeTime);
public record SessionEnded(Guid SessionId, DateTime EndTime);

// 定义聚合（投影）
public class TableSessionProjection
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan TotalPauseDuration { get; set; }
    public SessionStatus Status { get; set; }

    // 事件处理方法
    public void Apply(SessionStarted evt)
    {
        Id = evt.SessionId;
        TableId = evt.TableId;
        StartTime = evt.StartTime;
        Status = SessionStatus.Active;
    }

    public void Apply(SessionPaused evt)
    {
        Status = SessionStatus.Paused;
    }

    public void Apply(SessionEnded evt)
    {
        EndTime = evt.EndTime;
        Status = SessionStatus.Ended;
    }
}

// 存储事件
session.Events.StartStream<TableSessionProjection>(
    sessionId,
    new SessionStarted(sessionId, tableId, DateTime.UtcNow)
);
await session.SaveChangesAsync();

// 追加事件
session.Events.Append(sessionId, new SessionPaused(sessionId, DateTime.UtcNow));
await session.SaveChangesAsync();

// 重建聚合状态
var sessionState = await session.Events.AggregateStreamAsync<TableSessionProjection>(sessionId);
```

**优势**:
- 完整的审计日志（所有变更都是事件）
- 可以重建任意时间点的状态
- 支持事件回溯和分析

## 3. 在本项目中的应用

### 3.1 配置和初始化

**Program.cs**:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(opts =>
{
    // 连接字符串
    opts.Connection(builder.Configuration.GetConnectionString("Postgres")!);
    
    // 注册文档类型
    opts.Schema.For<Member>()
        .UseOptimisticConcurrency(true)
        .Index(x => x.Email, idx => idx.IsUnique = true)
        .Index(x => x.Phone)
        .Index(x => x.Level);

    opts.Schema.For<Table>()
        .Index(x => x.Status)
        .Index(x => x.StoreId);

    opts.Schema.For<TableSession>()
        .UseOptimisticConcurrency(true)
        .Index(x => x.TableId)
        .Index(x => x.MemberId)
        .Index(x => x.Status)
        .Index(x => x.StartTime);

    opts.Schema.For<Payment>()
        .Index(x => x.OrderId, idx => idx.IsUnique = true)
        .Index(x => x.MemberId)
        .Index(x => x.Status)
        .Index(x => x.CreatedAt);

    // 软删除
    opts.Schema.For<Member>().SoftDeleted();
    
    // 审计字段（自动填充创建和修改时间）
    opts.Policies.ForAllDocuments(m =>
    {
        if (m.IdType == typeof(Guid))
        {
            m.Metadata.Created.Enabled = true;
            m.Metadata.LastModified.Enabled = true;
        }
    });

    // 与 Wolverine 集成
    opts.IntegrateWithWolverine();

    // 自动创建/更新数据库架构（开发环境）
    if (builder.Environment.IsDevelopment())
    {
        opts.AutoCreateSchemaObjects = AutoCreate.All;
    }
});

// 添加 Marten 命令行工具支持（用于迁移）
builder.Services.AddMartenCommandLine();
```

### 3.2 数据访问模式

#### 模式 1：Repository 模式（可选）

虽然 Marten 的 `IDocumentSession` 已经是一个通用的 Repository，但如果需要封装特定逻辑：

```csharp
public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Member?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<List<Member>> GetPremiumMembersAsync(CancellationToken ct = default);
    Task SaveAsync(Member member, CancellationToken ct = default);
}

public class MemberRepository : IMemberRepository
{
    private readonly IDocumentSession _session;

    public MemberRepository(IDocumentSession session)
    {
        _session = session;
    }

    public Task<Member?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _session.LoadAsync<Member>(id, ct);

    public Task<Member?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _session.Query<Member>().FirstOrDefaultAsync(m => m.Email == email, ct);

    public Task<List<Member>> GetPremiumMembersAsync(CancellationToken ct = default)
        => _session.Query<Member>()
            .Where(m => m.Level == MembershipLevel.Premium)
            .ToListAsync(ct);

    public Task SaveAsync(Member member, CancellationToken ct = default)
    {
        _session.Store(member);
        return _session.SaveChangesAsync(ct);
    }
}
```

**注意**: 这种模式是可选的，但不推荐。在仓储方法中调用 `SaveChangesAsync` 违反了工作单元模式。在垂直切片架构中，直接在 Handler 中使用 `IDocumentSession` 更简单直接，并且由调用者（Handler）统一管理事务边界。

#### 模式 2：直接使用 IDocumentSession（推荐）

```csharp
public class GetMemberByEmailHandler
{
    public async Task<Member?> Handle(
        GetMemberByEmailQuery query,
        IDocumentSession session,
        CancellationToken ct)
    {
        return await session.Query<Member>()
            .FirstOrDefaultAsync(m => m.Email == query.Email, ct);
    }
}
```

### 3.3 批量操作

```csharp
// 批量插入
public class ImportMembersHandler
{
    public async Task Handle(
        ImportMembersCommand command,
        IDocumentSession session)
    {
        foreach (var memberDto in command.Members)
        {
            var member = new Member
            {
                Id = Guid.NewGuid(),
                Name = memberDto.Name,
                Email = memberDto.Email,
                // ...
            };
            session.Store(member);
        }
        
        // 批量提交（一个事务）
        await session.SaveChangesAsync();
    }
}

// 批量更新
public class UpdateMemberLevelsHandler
{
    public async Task Handle(
        UpdateMemberLevelsCommand command,
        IDocumentSession session)
    {
        var members = await session.LoadManyAsync<Member>(command.MemberIds);
        
        foreach (var member in members)
        {
            member.Level = MembershipLevel.Premium;
        }
        
        await session.SaveChangesAsync();
    }
}

// 批量删除（软删除）
public class DeactivateMembersHandler
{
    public async Task Handle(
        DeactivateMembersCommand command,
        IDocumentSession session)
    {
        var members = await session.LoadManyAsync<Member>(command.MemberIds);
        
        foreach (var member in members)
        {
            session.Delete(member); // 软删除（如果配置了 SoftDeleted）
        }
        
        await session.SaveChangesAsync();
    }
}
```

### 3.4 复杂查询

```csharp
// 分页查询
public class GetMembersPagedHandler
{
    public async Task<PagedResult<Member>> Handle(
        GetMembersPagedQuery query,
        IDocumentSession session)
    {
        var queryable = session.Query<Member>();

        // 过滤
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            queryable = queryable.Where(m => 
                m.Name.Contains(query.SearchTerm) || 
                m.Email.Contains(query.SearchTerm));
        }

        if (query.Level.HasValue)
        {
            queryable = queryable.Where(m => m.Level == query.Level.Value);
        }

        // 总数
        var total = await queryable.CountAsync();

        // 分页
        var items = await queryable
            .OrderBy(m => m.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<Member>(items, total, query.Page, query.PageSize);
    }
}

// 统计查询
public class GetMemberStatisticsHandler
{
    public async Task<MemberStatistics> Handle(
        GetMemberStatisticsQuery query,
        IDocumentSession session)
    {
        // Marten 支持原始 SQL（用于复杂聚合）
        var sql = @"
            SELECT 
                COUNT(*) as TotalCount,
                COUNT(*) FILTER (WHERE data->>'Level' = 'Premium') as PremiumCount,
                SUM((data->>'Balance')::numeric) as TotalBalance,
                AVG((data->>'Balance')::numeric) as AverageBalance
            FROM mt_doc_member
            WHERE mt_deleted = false
        ";

        await using var reader = await session.Connection.ExecuteReaderAsync(sql);
        await reader.ReadAsync();

        return new MemberStatistics
        {
            TotalCount = reader.GetInt32(0),
            PremiumCount = reader.GetInt32(1),
            TotalBalance = reader.GetDecimal(2),
            AverageBalance = reader.GetDecimal(3)
        };
    }
}

// 连接查询（使用编译查询优化性能）
public static class CompiledQueries
{
    public static readonly Func<IQuerySession, Guid, Task<TableWithSession?>> GetTableWithActiveSession =
        CompiledQuery.For((IQuerySession session, Guid tableId) =>
            session.Query<Table>()
                .Where(t => t.Id == tableId)
                .Select(t => new TableWithSession
                {
                    Table = t,
                    ActiveSession = session.Query<TableSession>()
                        .Where(s => s.TableId == tableId && s.Status == SessionStatus.Active)
                        .FirstOrDefault()
                })
                .FirstOrDefault()
        );
}

// 使用编译查询
var result = await CompiledQueries.GetTableWithActiveSession(session, tableId);
```

### 3.5 迁移和版本管理

Marten 支持两种迁移方式：

#### 方式 1：自动迁移（开发环境）

```csharp
// Program.cs
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
    
    // 自动应用所有架构变更
    await store.Advanced.ApplyAllConfiguredChangesToDatabaseAsync();
}
```

#### 方式 2：版本化迁移（生产环境）

```csharp
// 创建迁移类
public class AddEmailIndexToMember : Migration
{
    public override void Up()
    {
        this.SchemaFor<Member>()
            .Index(x => x.Email, idx => idx.IsUnique = true);
    }

    public override void Down()
    {
        Execute("DROP INDEX IF EXISTS mt_doc_member_email");
    }
}

// 使用命令行工具应用迁移
// dotnet run -- marten-apply
```

## 4. 性能优化

### 4.1 使用轻量级 Session

```csharp
// Identity Map Session（默认，跟踪所有加载的对象）
using var session = store.LightweightSession();

// Dirty Tracking Session（跟踪变更，自动检测更新）
using var session = store.DirtyTrackedSession();

// Query Session（只读，性能最佳）
using var session = store.QuerySession();
```

**选择建议**:
- 只读查询：使用 `QuerySession()`
- 简单 CRUD：使用 `LightweightSession()`
- 复杂变更跟踪：使用 `DirtyTrackedSession()`

### 4.2 批量加载

```csharp
// 避免 N+1 查询
// ❌ 错误（会发起多次查询）
foreach (var session in sessions)
{
    var table = await _session.LoadAsync<Table>(session.TableId);
    // ...
}

// ✅ 正确（一次查询）
var tableIds = sessions.Select(s => s.TableId).ToList();
var tables = await _session.LoadManyAsync<Table>(tableIds);
var tableDict = tables.ToDictionary(t => t.Id);

foreach (var session in sessions)
{
    var table = tableDict[session.TableId];
    // ...
}
```

### 4.3 编译查询（Compiled Queries）

对于频繁执行的查询，使用编译查询以提高性能：

```csharp
public static class CompiledQueries
{
    // 编译查询在首次使用时编译，后续直接使用
    public static readonly Func<IQuerySession, string, Task<Member?>> GetMemberByEmail =
        CompiledQuery.For((IQuerySession session, string email) =>
            session.Query<Member>()
                .FirstOrDefault(m => m.Email == email)
        );
}

// 使用
var member = await CompiledQueries.GetMemberByEmail(session, "zhang@example.com");
```

### 4.4 投影和选择

```csharp
// ❌ 加载完整文档（可能很大）
var members = await session.Query<Member>().ToListAsync();

// ✅ 只选择需要的字段
var memberNames = await session.Query<Member>()
    .Select(m => new { m.Id, m.Name, m.Email })
    .ToListAsync();
```

## 5. 与 EF Core 的对比

| 特性 | Marten | EF Core |
|------|--------|---------|
| 数据库 | PostgreSQL only | 多种数据库 |
| 数据模型 | 文档（JSON） + 关系表 | 关系表 |
| 架构方式 | Code First | Code First / DB First |
| 查询 | LINQ + 原始 SQL | LINQ + 原始 SQL |
| 变更跟踪 | 可选 | 默认启用 |
| 迁移 | 自动 + 手动 | 迁移脚本 |
| 性能 | 高（JSONB 索引优化） | 中等 |
| 学习曲线 | 中等 | 较低 |
| 事件溯源 | 原生支持 | 需要自行实现 |
| 适用场景 | 文档为主、读多写少 | 复杂关系、事务密集 |

**选择建议**:
- 需要灵活的文档模型、事件溯源 → Marten
- 复杂的关系模型、多数据库支持 → EF Core
- 混合使用（关系表用 EF Core，文档用 Marten）也是一种选择

## 6. 最佳实践

### 6.1 实体设计

```csharp
// ✅ 好的实体设计
public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public MemberProfile Profile { get; set; } = new(); // 嵌套对象
    public List<string> Tags { get; set; } = new(); // 集合
}

public class MemberProfile
{
    public string Phone { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
}

// ❌ 避免循环引用
public class Table
{
    public Guid Id { get; set; }
    public TableSession? CurrentSession { get; set; } // ❌ 不要这样做
}

// ✅ 使用 ID 引用
public class Table
{
    public Guid Id { get; set; }
    public Guid? CurrentSessionId { get; set; } // ✅ 只存储 ID
}
```

### 6.2 并发控制

```csharp
// 对于高并发更新的实体，启用乐观并发
opts.Schema.For<Member>().UseOptimisticConcurrency(true);

// 处理并发冲突
try
{
    await session.SaveChangesAsync();
}
catch (ConcurrencyException)
{
    // 重新加载并重试
    session.MarkAsAddedForDeletion(member);
    member = await session.LoadAsync<Member>(memberId);
    member.Balance += amount;
    await session.SaveChangesAsync();
}
```

### 6.3 软删除

```csharp
// 配置软删除
opts.Schema.For<Member>().SoftDeleted();

// 删除操作（实际上是标记为已删除）
session.Delete(member);
await session.SaveChangesAsync();

// 查询时自动过滤已删除的文档
var members = await session.Query<Member>().ToListAsync(); // 不包含已删除的

// 包含已删除的文档
var allMembers = await session.Query<Member>()
    .Where(m => m.MaybeDeleted())
    .ToListAsync();
```

## 7. 监控和调试

```csharp
// 启用 SQL 日志
builder.Services.AddMarten(opts =>
{
    opts.Connection(connectionString);
    
    if (builder.Environment.IsDevelopment())
    {
        // 记录所有生成的 SQL
        opts.Logger(new ConsoleMartenLogger());
        
        // 或集成到 ILogger
        opts.Logger(new MartenLogger(logger));
    }
});

// 查看生成的 SQL
var sql = session.Query<Member>()
    .Where(m => m.Email == "zhang@example.com")
    .ToCommand() // 获取生成的 SQL 命令
    .CommandText;

Console.WriteLine(sql);
```

## 8. 参考资源

- [Marten 官方文档](https://martendb.io/)
- [Marten GitHub](https://github.com/JasperFx/marten)
- [与 Wolverine 集成](https://wolverine.netlify.app/guide/durability/marten/)
- [事件溯源指南](https://martendb.io/events/)

---

**最后更新**: 2024-01-15  
**负责人**: 架构团队  
**审核状态**: 待审核
