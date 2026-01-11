# Marten 基础设施配置指南

本文档说明如何使用 `AddMartenDefaults` 扩展方法配置 Marten 文档数据库。

---

## 📋 概述

`AddMartenDefaults` 提供了统一、可复用的 Marten 配置，自动对接 AppHost 中定义的 PostgreSQL 资源。

**核心特性**:
- ✅ 统一使用 `ConnectionStrings:Default` 配置键
- ✅ 约定 schema 命名为 `billiard`
- ✅ 自动配置 lightweight sessions（推荐模式）
- ✅ 与 Aspire AppHost 无缝集成
- ✅ 提供清晰的错误提示

---

## 🚀 快速开始

### 1. 在服务项目中使用

在 `Program.cs` 中添加 Marten 配置：

```csharp
var builder = WebApplication.CreateBuilder(args);

// 添加 Aspire ServiceDefaults
builder.AddServiceDefaults();

// 添加 Marten 默认配置
builder.AddMartenDefaults();

var app = builder.Build();
app.MapDefaultEndpoints();
app.Run();
```

### 2. 在 Handler 中使用

Marten 会自动注册 `IDocumentStore` 和 `IDocumentSession`，可以直接注入使用：

```csharp
using Marten;

public class RegisterMemberHandler
{
    private readonly IDocumentSession _session;

    public RegisterMemberHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<Member> Handle(RegisterMember command, CancellationToken cancellationToken = default)
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Phone = command.Phone
        };

        _session.Store(member);
        await _session.SaveChangesAsync(cancellationToken);

        return member;
    }
}
```

### 3. 查询示例

```csharp
// 按 ID 加载
var member = await _session.LoadAsync<Member>(memberId, cancellationToken);

// LINQ 查询
var members = await _session.Query<Member>()
    .Where(m => m.Name.Contains("张"))
    .ToListAsync(cancellationToken);

// 投影查询
var memberDtos = await _session.Query<Member>()
    .Select(m => new MemberDto { Id = m.Id, Name = m.Name })
    .ToListAsync(cancellationToken);
```

---

## ⚙️ 配置说明

### 默认配置

| 配置项 | 值 | 说明 |
|--------|-----|------|
| 连接字符串键 | `ConnectionStrings:Default` | 与 AppHost 数据库名称一致 |
| Schema 名称 | `billiard` | 所有表统一在此 schema 下 |
| Session 模式 | Lightweight | 最轻量、性能最佳的模式 |

### AppHost 配置

在 `AppHost.cs` 中，数据库必须命名为 `Default`：

```csharp
var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

// 数据库名称必须是 "Default"
var db = postgres.AddDatabase("Default");

builder.AddProject<Projects.Bootstrapper>("bootstrapper")
    .WithReference(db)  // 自动注入 ConnectionStrings:Default
    .WaitFor(db);
```

---

## 🧪 测试

### 单元测试中使用

在测试中可以使用内存配置提供连接字符串：

```csharp
[Fact]
public void Test_MartenConfiguration()
{
    // Arrange
    var builder = WebApplication.CreateBuilder();
    
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ConnectionStrings:Default"] = "Host=localhost;Database=test;Username=test;Password=test"
    });

    // Act
    builder.AddMartenDefaults();
    var app = builder.Build();

    // Assert
    var documentStore = app.Services.GetRequiredService<IDocumentStore>();
    documentStore.Should().NotBeNull();
}
```

### 集成测试

对于集成测试，建议使用 Testcontainers 启动真实的 PostgreSQL：

```csharp
// 待实现：使用 Testcontainers.PostgreSQL
```

---

## ⚠️ 错误处理

### 缺失连接字符串

如果 `ConnectionStrings:Default` 未配置，会抛出清晰的异常：

```
InvalidOperationException: Missing Default connection string. 
Ensure the database is referenced in AppHost and the connection string is properly injected.
```

**解决方法**:
1. 检查 AppHost 是否正确定义了数据库：`postgres.AddDatabase("Default")`
2. 检查服务是否引用了数据库：`.WithReference(db)`
3. 确保 AppHost 已启动并运行

---

## 📚 架构说明

### 为什么使用 Lightweight Sessions？

Marten 提供三种 session 模式：

| 模式 | 特点 | 使用场景 |
|------|------|----------|
| **Lightweight** ✅ | 无变更追踪，性能最佳 | 大多数 CRUD 场景（推荐） |
| Identity | 保证同一对象只有一个实例 | 需要对象唯一性时 |
| Dirty Tracked | 自动检测变更 | 复杂的对象图变更 |

本项目默认使用 **Lightweight**，这是 Marten 官方推荐的模式。

### Schema 命名约定

所有 Marten 表统一使用 `billiard` schema，与应用其他表隔离：

```
public
├── (EF Core 表，如果有)
└── billiard (Marten 表)
    ├── mt_doc_member
    ├── mt_doc_session
    └── mt_streams (事件流)
```

### 连接字符串统一

使用 `Default` 作为连接字符串键的优势：

- ✅ 符合 .NET 约定（默认连接字符串）
- ✅ 简化配置（无需记忆特殊键名）
- ✅ 与 Aspire 无缝集成
- ✅ 支持多环境配置

---

## 🔧 高级用法

### 自定义配置

如果需要自定义 Marten 配置，可以在调用 `AddMartenDefaults` 后继续配置：

```csharp
builder.AddMartenDefaults();

// 进一步自定义
builder.Services.ConfigureMarten(options =>
{
    // 启用事件存储
    options.Events.StreamIdentity = StreamIdentity.AsGuid;
    
    // 自定义序列化
    options.UseDefaultSerialization(serializerType: SerializerType.SystemTextJson);
});
```

### 多租户支持

Marten 支持多租户模式（未来可能需要）：

```csharp
builder.AddMartenDefaults();

builder.Services.ConfigureMarten(options =>
{
    options.Policies.AllDocumentsAreMultiTenanted();
});
```

---

## 📖 参考资源

### Marten 官方文档
- [Marten 官网](https://martendb.io/)
- [Document Sessions](https://martendb.io/documents/sessions.html)
- [LINQ Queries](https://martendb.io/documents/querying/linq/)
- [Event Store](https://martendb.io/events/)

### 项目文档
- [Wolverine 模块化架构蓝图](../../../../doc/03_系统架构设计/Wolverine模块化架构蓝图.md)
- [会员管理模块设计](../../../../doc/04_模块设计/会员管理模块.md)
- [Aspire 编排架构](../../../../doc/03_系统架构设计/Aspire编排架构.md)

---

## 🐛 常见问题

### Q: 为什么不使用 Repository 模式？

A: 在 Wolverine + 垂直切片架构中，Handler 直接使用 `IDocumentSession`，不需要额外的抽象层。这遵循架构原则：
- ✅ **拒绝传统 Repository 模式** - 避免过度抽象
- ✅ **Handler 即 Application Service** - 直接操作数据
- ✅ **保持简单** - 减少不必要的中间层

### Q: 如何处理并发冲突？

A: Marten 支持乐观并发控制：

```csharp
// 使用版本号
var member = await _session.LoadAsync<Member>(id);
member.Version = expectedVersion;
await _session.SaveChangesAsync();
```

### Q: 数据库迁移如何处理？

A: Marten 可以自动创建表结构：

```csharp
// 在应用启动时应用迁移
var store = app.Services.GetRequiredService<IDocumentStore>();
await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
```

---

## 版本信息

- **创建日期**: 2026-01-11
- **Marten 版本**: 8.17.0
- **.NET 版本**: 10.0
- **最后更新**: 2026-01-11

---

**维护者**: 架构团队  
**问题反馈**: 提交 Issue 或联系架构团队
