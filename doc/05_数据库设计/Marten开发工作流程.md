# Marten 开发工作流程

## 1. 概述

本文档描述使用 Marten 进行数据库开发的完整工作流程，包括实体定义、配置、查询、迁移等操作。

## 2. 环境准备

### 2.1 安装 PostgreSQL

```bash
# macOS
brew install postgresql@16

# Ubuntu
sudo apt-get install postgresql-16

# Docker
docker run --name postgres -e POSTGRES_PASSWORD=password -p 5432:5432 -d postgres:16
```

### 2.2 安装 Marten NuGet 包

```bash
dotnet add package Marten
dotnet add package Marten.AspNetCore
```

### 2.3 配置连接字符串

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=billiard_hall;Username=postgres;Password=password"
  }
}
```

## 3. 实体定义

### 3.1 基本实体

Marten 使用 POCO（Plain Old CLR Object），无需继承基类或实现接口：

```csharp
// Domain/Entities/Member.cs
public class Member
{
    // Marten 要求实体有 Id 属性
    public Guid Id { get; set; }
    
    // 基本属性
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // 复杂类型（嵌套对象）
    public MemberProfile Profile { get; set; } = new();
    
    // 集合
    public List<string> Tags { get; set; } = new();
    
    // 枚举
    public MembershipLevel Level { get; set; }
    
    // 金额（使用 decimal）
    public decimal Balance { get; set; }
    
    // 时间（建议使用 UTC）
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // 软删除标记（可选）
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

// 嵌套对象
public class MemberProfile
{
    public string Avatar { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
    public DateTime? Birthday { get; set; }
}

public class Address
{
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
}

// 枚举
public enum MembershipLevel
{
    Regular = 0,
    Silver = 1,
    Gold = 2,
    Premium = 3
}
```

### 3.2 实体设计最佳实践

**✅ 推荐**:
```csharp
// 使用值对象封装业务概念
public record Money(decimal Amount, string Currency = "CNY");

public class Member
{
    public Guid Id { get; set; }
    public Money Balance { get; set; } = Money.Zero;
}

// 使用私有 setter 保护不变性
public class TableSession
{
    public Guid Id { get; private set; }
    public SessionStatus Status { get; private set; }
    
    public static TableSession Start(Guid tableId, Guid? memberId)
    {
        return new TableSession
        {
            Id = Guid.NewGuid(),
            TableId = tableId,
            MemberId = memberId,
            Status = SessionStatus.Active,
            StartTime = DateTime.UtcNow
        };
    }
    
    public void End()
    {
        if (Status != SessionStatus.Active)
            throw new InvalidOperationException("只有活动会话可以结束");
            
        Status = SessionStatus.Ended;
        EndTime = DateTime.UtcNow;
    }
}
```

**❌ 避免**:
```csharp
// 不要循环引用
public class Table
{
    public Guid Id { get; set; }
    public TableSession? CurrentSession { get; set; }  // ❌
}

public class TableSession
{
    public Guid Id { get; set; }
    public Table Table { get; set; }  // ❌ 循环引用（会导致 Marten 在 JSON 序列化时抛出运行时异常）
}

// ✅ 使用 ID 引用，避免循环引用
public class Table
{
    public Guid Id { get; set; }
    public Guid? CurrentSessionId { get; set; }  // ✅
}
```

## 4. Marten 配置

### 4.1 Program.cs 配置

```csharp
var builder = WebApplication.CreateBuilder(args);

// 添加 Marten
builder.Services.AddMarten(opts =>
{
    // 连接字符串
    var connectionString = builder.Configuration.GetConnectionString("Postgres")!;
    opts.Connection(connectionString);
    
    // 配置数据库架构名称（可选，默认为 public）
    opts.DatabaseSchemaName = "billiard_hall";
    
    // 配置文档类型
    ConfigureDocuments(opts);
    
    // 配置事件溯源（可选）
    ConfigureEventStore(opts);
    
    // 与 Wolverine 集成
    opts.IntegrateWithWolverine();
    
    // 开发环境自动创建/更新架构
    if (builder.Environment.IsDevelopment())
    {
        opts.AutoCreateSchemaObjects = AutoCreate.All;
    }
    else
    {
        // 生产环境不自动创建
        opts.AutoCreateSchemaObjects = AutoCreate.None;
    }
});

// 添加命令行工具支持（用于迁移）
builder.Services.AddMartenCommandLine();

// 配置文档
void ConfigureDocuments(StoreOptions opts)
{
    // Member 实体配置
    opts.Schema.For<Member>()
        .UseOptimisticConcurrency(true)        // 启用乐观并发
        .SoftDeleted()                         // 启用软删除
        .Index(x => x.Phone, idx =>            // 索引
        {
            idx.IsUnique = true;               // 唯一索引
            idx.IsConcurrent = true;           // 并发创建
        })
        .Index(x => x.Email, idx => idx.IsUnique = true)
        .Index(x => x.Level)
        .FullTextIndex(x => x.Name);           // 全文搜索索引

    // TableSession 实体配置
    opts.Schema.For<TableSession>()
        .UseOptimisticConcurrency(true)
        .Index(x => x.TableId)
        .Index(x => x.MemberId)
        .Index(x => x.Status)
        .Index(x => x.StartTime);

    // Table 实体配置
    opts.Schema.For<Table>()
        .Index(x => x.Status)
        .Index(x => x.StoreId)
        .Index(x => x.Type);

    // Payment 实体配置
    opts.Schema.For<Payment>()
        .Index(x => x.OrderId, idx => idx.IsUnique = true)
        .Index(x => x.MemberId)
        .Index(x => x.Status)
        .Index(x => x.CreatedAt);

    // 全局配置
    opts.Policies.ForAllDocuments(m =>
    {
        // 自动填充元数据
        if (m.IdType == typeof(Guid))
        {
            m.Metadata.Created.Enabled = true;         // 创建时间
            m.Metadata.LastModified.Enabled = true;    // 最后修改时间
        }
    });
}

// 配置事件溯源（可选）
void ConfigureEventStore(StoreOptions opts)
{
    opts.Events.AddEventType<SessionStarted>();
    opts.Events.AddEventType<SessionPaused>();
    opts.Events.AddEventType<SessionResumed>();
    opts.Events.AddEventType<SessionEnded>();
}

var app = builder.Build();

// 开发环境应用架构变更
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
    await store.Advanced.ApplyAllConfiguredChangesToDatabaseAsync();
}

app.Run();
```

## 5. 数据操作

### 5.1 基本 CRUD

**创建（Create）**:
```csharp
public class CreateMemberHandler
{
    public async Task<Guid> Handle(
        CreateMemberCommand command,
        IDocumentSession session)
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Phone = command.Phone,
            Email = command.Email,
            Level = MembershipLevel.Regular,
            Balance = 0,
            CreatedAt = DateTime.UtcNow
        };

        session.Store(member);
        await session.SaveChangesAsync();

        return member.Id;
    }
}
```

**读取（Read）**:
```csharp
// 通过 ID 加载
var member = await session.LoadAsync<Member>(memberId);

// 查询单个
var member = await session.Query<Member>()
    .FirstOrDefaultAsync(m => m.Email == email);

// 查询列表
var members = await session.Query<Member>()
    .Where(m => m.Level == MembershipLevel.Premium)
    .OrderBy(m => m.Name)
    .ToListAsync();

// 批量加载
var memberIds = new[] { id1, id2, id3 };
var members = await session.LoadManyAsync<Member>(memberIds);
```

**更新（Update）**:
```csharp
// 方式 1：加载、修改、保存
var member = await session.LoadAsync<Member>(memberId);
member.Name = "新名字";
member.UpdatedAt = DateTime.UtcNow;
await session.SaveChangesAsync();  // Marten 自动检测变更

// 方式 2：显式更新（使用 DirtyTrackedSession）
using var session = store.DirtyTrackedSession();
var member = await session.LoadAsync<Member>(memberId);
member.Name = "新名字";
await session.SaveChangesAsync();  // 自动检测并更新

// 方式 3：使用 Update API
await session.UpdateAsync<Member>(
    memberId,
    m => m.Balance += 100  // 更新余额
);
```

**删除（Delete）**:
```csharp
// 软删除（如果配置了 SoftDeleted）
var member = await session.LoadAsync<Member>(memberId);
session.Delete(member);
await session.SaveChangesAsync();

// 硬删除
session.HardDelete(member);
await session.SaveChangesAsync();

// 通过 ID 删除
session.Delete<Member>(memberId);
await session.SaveChangesAsync();
```

### 5.2 高级查询

**条件查询**:
```csharp
// 多条件
var members = await session.Query<Member>()
    .Where(m => 
        m.Level == MembershipLevel.Premium && 
        m.Balance > 1000 &&
        !m.IsDeleted)
    .ToListAsync();

// 字符串匹配
var members = await session.Query<Member>()
    .Where(m => m.Name.Contains("张"))
    .ToListAsync();

// 日期范围
var startDate = DateTime.UtcNow.AddDays(-30);
var sessions = await session.Query<TableSession>()
    .Where(s => s.StartTime >= startDate)
    .ToListAsync();

// 集合查询
var tags = new[] { "VIP", "活跃" };
var members = await session.Query<Member>()
    .Where(m => m.Tags.Any(t => tags.Contains(t)))
    .ToListAsync();
```

**分页查询**:
```csharp
public async Task<PagedResult<Member>> GetMembersPaged(
    int page,
    int pageSize,
    IDocumentSession session)
{
    var query = session.Query<Member>();

    // 总数
    var totalCount = await query.CountAsync();

    // 分页数据
    var items = await query
        .OrderBy(m => m.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResult<Member>
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}
```

**投影查询（只查询部分字段）**:
```csharp
// 投影到匿名类型
var memberNames = await session.Query<Member>()
    .Select(m => new { m.Id, m.Name, m.Level })
    .ToListAsync();

// 投影到 DTO
public class MemberListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

var dtos = await session.Query<Member>()
    .Select(m => new MemberListDto
    {
        Id = m.Id,
        Name = m.Name,
        Phone = m.Phone
    })
    .ToListAsync();
```

**聚合查询**:
```csharp
// 计数
var count = await session.Query<Member>()
    .Where(m => m.Level == MembershipLevel.Premium)
    .CountAsync();

// 判断存在
var exists = await session.Query<Member>()
    .Where(m => m.Email == email)
    .AnyAsync();

// 复杂聚合（使用原始 SQL）
var sql = @"
    SELECT 
        COUNT(*) as TotalCount,
        SUM((data->>'Balance')::numeric) as TotalBalance,
        AVG((data->>'Balance')::numeric) as AverageBalance
    FROM mt_doc_member
    WHERE data->>'Level' = 'Premium'
";

await using var reader = await session.Connection.ExecuteReaderAsync(sql);
await reader.ReadAsync();

var stats = new
{
    TotalCount = reader.GetInt32(0),
    TotalBalance = reader.GetDecimal(1),
    AverageBalance = reader.GetDecimal(2)
};
```

**编译查询（性能优化）**:
```csharp
// 定义编译查询
public static class CompiledQueries
{
    public static readonly Func<IQuerySession, string, Task<Member?>> GetMemberByEmail =
        CompiledQuery.For((IQuerySession session, string email) =>
            session.Query<Member>().FirstOrDefault(m => m.Email == email)
        );

    public static readonly Func<IQuerySession, MembershipLevel, Task<List<Member>>> GetMembersByLevel =
        CompiledQuery.For((IQuerySession session, MembershipLevel level) =>
            session.Query<Member>()
                .Where(m => m.Level == level)
                .OrderBy(m => m.Name)
                .ToList()
        );
}

// 使用编译查询（首次执行时编译，后续直接使用）
var member = await CompiledQueries.GetMemberByEmail(session, "zhang@example.com");
var premiumMembers = await CompiledQueries.GetMembersByLevel(session, MembershipLevel.Premium);
```

### 5.3 事务处理

**自动事务**:
```csharp
// SaveChangesAsync 自动包装在事务中
using var session = store.LightweightSession();

session.Store(member);
session.Store(auditLog);

// 两个操作在同一事务中
await session.SaveChangesAsync();
```

**显式事务**:
```csharp
using var session = store.LightweightSession();
using var tx = session.Connection.BeginTransaction();
session.EnlistInTransaction(tx);

try
{
    var member = await session.LoadAsync<Member>(memberId);
    member.Balance -= amount;
    
    var payment = new Payment { /* ... */ };
    session.Store(payment);
    
    await session.SaveChangesAsync();
    await tx.CommitAsync();
}
catch
{
    await tx.RollbackAsync();
    throw;
}
```

**与 Wolverine 集成（自动事务）**:
```csharp
// Wolverine 自动管理事务
public class TransferBalanceHandler
{
    public async Task Handle(
        TransferBalanceCommand cmd,
        IDocumentSession session)  // Wolverine 自动注入并管理事务
    {
        var fromMember = await session.LoadAsync<Member>(cmd.FromMemberId);
        var toMember = await session.LoadAsync<Member>(cmd.ToMemberId);
        
        fromMember.Balance -= cmd.Amount;
        toMember.Balance += cmd.Amount;
        
        // SaveChanges 由 Wolverine 在处理器完成后自动调用
        // 如果抛出异常，事务会自动回滚
    }
}
```

## 6. 架构管理和迁移

### 6.1 开发环境（自动迁移）

```csharp
// Program.cs
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
    
    // 自动应用所有配置的架构变更
    await store.Advanced.ApplyAllConfiguredChangesToDatabaseAsync();
}
```

### 6.2 生产环境（脚本迁移）

**生成迁移脚本**:
```bash
# 安装 Marten 命令行工具
dotnet tool install -g Marten.CommandLine

# 生成迁移脚本
marten-apply -c "Host=localhost;Database=billiard_hall;..." -o migration.sql

# 或使用 dotnet run
dotnet run -- marten-apply -o migration.sql
```

**应用迁移脚本**:
```bash
psql -h localhost -U postgres -d billiard_hall -f migration.sql
```

### 6.3 版本化迁移（可选）

创建迁移类：
```csharp
public class V1_AddEmailIndexToMember : Migration
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
```

## 7. 性能优化

### 7.1 Session 类型选择

```csharp
// LightweightSession: 默认，适合大多数场景
using var session = store.LightweightSession();

// DirtyTrackedSession: 自动检测变更
using var session = store.DirtyTrackedSession();

// QuerySession: 只读，性能最佳
using var session = store.QuerySession();
```

**选择建议**:
- 只读查询 → `QuerySession()`
- 简单 CRUD → `LightweightSession()`
- 复杂变更跟踪 → `DirtyTrackedSession()`

### 7.2 批量操作

```csharp
// 批量插入
var members = CreateManyMembers();
foreach (var member in members)
{
    session.Store(member);
}
await session.SaveChangesAsync();  // 一次提交

// 批量加载
var ids = members.Select(m => m.Id).ToList();
var loadedMembers = await session.LoadManyAsync<Member>(ids);
```

### 7.3 索引优化

```csharp
// 为常用查询字段添加索引
opts.Schema.For<Member>()
    .Index(x => x.Phone)     // 单列索引
    .Index(x => x.Email)
    .Index(x => x.Level, x => x.CreatedAt);  // 复合索引
```

### 7.4 查询优化

```csharp
// ✅ 使用投影，只查询需要的字段
var names = await session.Query<Member>()
    .Select(m => m.Name)
    .ToListAsync();

// ❌ 避免：查询完整文档后再提取字段
var members = await session.Query<Member>().ToListAsync();
var names = members.Select(m => m.Name).ToList();

// ✅ 使用编译查询
var member = await CompiledQueries.GetMemberByEmail(session, email);

// ❌ 避免：动态查询
var member = await session.Query<Member>()
    .FirstOrDefaultAsync(m => m.Email == email);
```

## 8. 调试和监控

### 8.1 启用 SQL 日志

```csharp
builder.Services.AddMarten(opts =>
{
    opts.Connection(connectionString);
    
    if (builder.Environment.IsDevelopment())
    {
        // 输出到控制台
        opts.Logger(new ConsoleMartenLogger());
        
        // 或集成到 ILogger
        opts.Logger(new SerilogMartenLogger(logger));
    }
});
```

### 8.2 查看生成的 SQL

```csharp
// 获取查询生成的 SQL
var query = session.Query<Member>()
    .Where(m => m.Email == "zhang@example.com");

var command = query.ToCommand();
Console.WriteLine(command.CommandText);

// 输出：
// SELECT data FROM mt_doc_member WHERE data->>'Email' = $1
```

### 8.3 诊断表

```sql
-- 查看文档数量
SELECT COUNT(*) FROM mt_doc_member;

-- 查看表结构
\d mt_doc_member

-- 查看索引
\di mt_doc_member*

-- 查看事件流
SELECT * FROM mt_events LIMIT 10;
```

## 9. 常见问题

### Q1: 如何处理并发冲突？

```csharp
opts.Schema.For<Member>().UseOptimisticConcurrency(true);

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

### Q2: 如何实现软删除？

```csharp
opts.Schema.For<Member>().SoftDeleted();

// 删除（标记为已删除）
session.Delete(member);

// 查询（自动过滤已删除）
var members = await session.Query<Member>().ToListAsync();

// 包含已删除
var all = await session.Query<Member>()
    .Where(m => m.MaybeDeleted())
    .ToListAsync();
```

### Q3: 如何处理大型 JSON 文档？

使用投影和索引：
```csharp
// 投影到 DTO
var dtos = await session.Query<Member>()
    .Select(m => new MemberDto { Id = m.Id, Name = m.Name })
    .ToListAsync();

// 为嵌套字段添加索引
opts.Schema.For<Member>()
    .Index(x => x.Profile.Address.City);
```

## 10. 参考资源

- [Marten 官方文档](https://martendb.io/)
- [Marten GitHub](https://github.com/JasperFx/marten)
- [性能优化指南](https://martendb.io/guide/performance/)
- [事件溯源](https://martendb.io/events/)

---

**最后更新**: 2024-01-15  
**负责人**: 开发团队  
**审核状态**: 待审核
