# ArchitectureTests.Shared - 测试工具类库

## 概述

本目录包含架构测试的共享测试工具类和基础设施，重点提供：

- **PostgreSQL Testcontainers** 集成（支持本地和 CI 环境）
- **Marten DocumentStore** 工厂和扩展方法
- **测试数据构建器（Builder Pattern）**
- **xUnit Fixture 和 Collection 定义**
- **测试隔离策略**

## 目录结构

```
Shared/
├── Fixtures/
│   ├── PostgresTestContainerFixture.cs   # PostgreSQL 容器管理
│   └── SharedTestFixture.cs              # 集成测试共享 Fixture
├── Factories/
│   └── DocumentStoreFactory.cs           # Marten DocumentStore 工厂
├── Extensions/
│   └── MartenTestExtensions.cs           # Marten 测试扩展方法
├── Builders/
│   └── TestDataBuilder.cs                # 测试数据构建器基类
├── Collections/
│   └── TestCollectionDefinitions.cs      # xUnit 测试集合定义
└── README.md                              # 本文档
```

---

## 核心组件

### 1. PostgresTestContainerFixture

**用途**：管理测试用的 PostgreSQL 数据库

**特性**：
- ✅ 支持两种模式：
  - **CI 模式**：从环境变量 `POSTGRES_CONNECTION_STRING` 读取（优先）
  - **本地模式**：使用 Testcontainers 启动临时 PostgreSQL 容器
- ✅ 自动启动和清理容器
- ✅ 连接串安全屏蔽（日志输出时隐藏密码）

**使用示例**：

```csharp
public class MyDatabaseTests : IAsyncLifetime
{
    private PostgresTestContainerFixture _fixture = null!;

    public async Task InitializeAsync()
    {
        _fixture = new PostgresTestContainerFixture();
        await _fixture.InitializeAsync();
    }

    [Fact]
    public async Task Should_Connect_To_Database()
    {
        var connectionString = _fixture.ConnectionString;
        // 使用连接字符串进行测试...
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }
}
```

---

### 2. DocumentStoreFactory

**用途**：创建测试隔离的 Marten DocumentStore

**特性**：
- ✅ 自动生成唯一 schema（`test_schema_{guid}`）实现测试隔离
- ✅ 支持自定义 schema 名称（用于集合级别共享）
- ✅ 预建 schema 对象，避免并发创建冲突
- ✅ 可选的日志配置

**使用示例**：

```csharp
// 方式 1：自动生成唯一 schema（推荐用于单测试隔离）
var store = DocumentStoreFactory.Create(connectionString);

// 方式 2：指定 schema（用于集合级别共享）
var store = DocumentStoreFactory.CreateForCollection(
    connectionString, 
    "my_test_collection"
);

// 方式 3：完全自定义
var store = DocumentStoreFactory.Create(
    connectionString,
    schema: "my_custom_schema",
    loggerFactory: loggerFactory,
    configureOptions: opts =>
    {
        opts.Schema.For<MyDocument>().Identity(x => x.Id);
    }
);
```

---

### 3. MartenTestExtensions

**用途**：提供 Marten 测试场景下的数据清理和管理扩展方法

**方法列表**：

| 方法 | 说明 |
|------|------|
| `ClearAllDataAsync` | 清空所有文档数据（保留 Schema 结构） |
| `CompletelyRemoveSchemaAsync` | 完全移除 Schema 中的所有对象 |
| `ResetSchemaAsync` | 重置 Schema（先移除，再重建） |
| `ClearDocumentTypeAsync<T>` | 清空指定文档类型的数据 |
| `BulkInsertAsync<T>` | 批量插入测试数据 |
| `VerifyConnectionAsync` | 验证连接是否正常 |

**使用示例**：

```csharp
// 测试前清理数据
await store.ClearAllDataAsync();

// 测试后重置 Schema
await store.ResetSchemaAsync();

// 只清理特定类型
await store.ClearDocumentTypeAsync<MyDocument>();

// 批量插入测试数据
var testData = new[] { new MyDocument(), new MyDocument() };
await store.BulkInsertAsync(testData);
```

---

### 4. SharedTestFixture

**用途**：为集成测试提供完整的测试环境（PostgreSQL + Marten + Host）

**特性**：
- ✅ 管理 PostgreSQL 容器生命周期
- ✅ 提供测试隔离的 DocumentStore
- ✅ 创建轻量级 IHost（可注册 Wolverine、应用服务等）
- ✅ 支持测试之间的数据清理

**使用示例**：

```csharp
[Collection(CollectionNames.IntegrationTests)]
public class MyIntegrationTests
{
    private readonly SharedTestFixture _fixture;

    public MyIntegrationTests(SharedTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_Save_And_Retrieve_Document()
    {
        // 使用 DocumentStore
        await using var session = _fixture.DocumentStore.LightweightSession();
        session.Store(new MyDocument { Id = Guid.NewGuid() });
        await session.SaveChangesAsync();

        // 使用依赖注入的服务
        var myService = _fixture.GetService<IMyService>();
        await myService.DoSomethingAsync();
    }

    [Fact]
    public async Task Test_With_Clean_State()
    {
        // 测试前清理数据，确保隔离
        await _fixture.ClearAllDataAsync();
        
        // 执行测试...
    }
}
```

---

### 5. TestCollectionDefinitions

**用途**：定义 xUnit 测试集合，支持 Fixture 共享

**集合类型**：

- **IntegrationTests**：集成测试集合（共享 SharedTestFixture）
- **IsolatedTests**：独立测试集合（每个测试类使用独立 Fixture）

**使用示例**：

```csharp
// 方式 1：使用共享 Fixture
[Collection(CollectionNames.IntegrationTests)]
public class Test1
{
    private readonly SharedTestFixture _fixture;
    public Test1(SharedTestFixture fixture) => _fixture = fixture;
}

[Collection(CollectionNames.IntegrationTests)]
public class Test2
{
    // Test1 和 Test2 共享同一个 SharedTestFixture 实例
    private readonly SharedTestFixture _fixture;
    public Test2(SharedTestFixture fixture) => _fixture = fixture;
}

// 方式 2：独立 Fixture（不使用 Collection）
public class IsolatedTest : IClassFixture<SharedTestFixture>
{
    // 此测试类有自己独立的 SharedTestFixture 实例
    private readonly SharedTestFixture _fixture;
    public IsolatedTest(SharedTestFixture fixture) => _fixture = fixture;
}
```

---

## 测试隔离策略

### 策略 1：Schema 级别隔离（推荐）

**适用场景**：大多数测试场景

**原理**：
- 共享一个 PostgreSQL 实例/容器
- 每个测试类/集合使用独立的 schema（`test_schema_{guid}` 或 `test_{collection}_{timestamp}`）
- 避免表级别的锁竞争

**优点**：
- ✅ 资源占用少（只启动一个数据库容器）
- ✅ 测试启动快
- ✅ 适合 CI 环境
- ✅ 支持并行测试

**使用方式**：
```csharp
// 自动生成唯一 schema
var store = DocumentStoreFactory.Create(connectionString);

// 或使用集合级别 schema
var store = DocumentStoreFactory.CreateForCollection(connectionString, "MyTestCollection");
```

---

### 策略 2：数据库级别隔离

**适用场景**：Schema 隔离无法解决的场景（如测试全局 DB 配置）

**原理**：
- 每个测试类/集合使用独立的数据库实例
- 需要更多资源和时间

**使用方式**：
```csharp
// 为每个集合启动独立的 PostgresTestContainerFixture
// （不推荐，除非必要）
```

---

### 策略 3：测试间数据清理

**适用场景**：同一集合内的测试需要隔离

**原理**：
- 在测试方法间清理数据（保留 schema）
- 使用 `ClearAllDataAsync()` 或 `ClearDocumentTypeAsync<T>()`

**使用方式**：
```csharp
[Collection(CollectionNames.IntegrationTests)]
public class MyTests
{
    private readonly SharedTestFixture _fixture;
    public MyTests(SharedTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Test1()
    {
        await _fixture.ClearAllDataAsync(); // 清理数据
        // 执行测试...
    }

    [Fact]
    public async Task Test2()
    {
        await _fixture.ClearAllDataAsync(); // 清理数据
        // 执行测试...
    }
}
```

---

## 本地运行配置

### 前置条件

1. 安装 Docker Desktop（用于运行 Testcontainers）
2. .NET 10.0 SDK

### 方式 1：使用 Testcontainers（推荐）

无需任何配置，测试会自动启动 PostgreSQL 容器：

```bash
cd src/tests/ArchitectureTests
dotnet test
```

### 方式 2：使用本地 PostgreSQL

1. 启动本地 PostgreSQL：
   ```bash
   docker run -d --name postgres-test \
     -e POSTGRES_USER=test_user \
     -e POSTGRES_PASSWORD=test_password \
     -e POSTGRES_DB=test_db \
     -p 5432:5432 \
     postgres:17-alpine
   ```

2. 设置环境变量：
   ```bash
   export POSTGRES_CONNECTION_STRING="Host=localhost;Port=5432;Database=test_db;Username=test_user;Password=test_password"
   ```

3. 运行测试：
   ```bash
   dotnet test
   ```

### 方式 3：使用 dotnet user-secrets

1. 初始化 user-secrets：
   ```bash
   cd src/tests/ArchitectureTests
   dotnet user-secrets init
   ```

2. 设置连接字符串：
   ```bash
   dotnet user-secrets set "POSTGRES_CONNECTION_STRING" "Host=localhost;Port=5432;Database=test_db;Username=test_user;Password=test_password"
   ```

3. 修改代码读取 user-secrets（可选）：
   ```csharp
   // 在 PostgresTestContainerFixture 中添加：
   var config = new ConfigurationBuilder()
       .AddUserSecrets<PostgresTestContainerFixture>()
       .Build();
   var connectionString = config["POSTGRES_CONNECTION_STRING"];
   ```

---

## CI 配置

### GitHub Actions 示例

```yaml
name: Architecture Tests

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest

    # 方式 1：使用 Testcontainers（推荐）
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Run Architecture Tests
        run: |
          cd src/tests/ArchitectureTests
          dotnet test --configuration Release --logger "console;verbosity=normal"

  test-with-service:
    runs-on: ubuntu-latest

    # 方式 2：使用 GitHub Service Container
    services:
      postgres:
        image: postgres:17-alpine
        env:
          POSTGRES_USER: test_user
          POSTGRES_PASSWORD: test_password
          POSTGRES_DB: test_db
        ports:
          - 5432:5432
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Run Architecture Tests with Service Container
        env:
          POSTGRES_CONNECTION_STRING: "Host=localhost;Port=5432;Database=test_db;Username=test_user;Password=test_password"
        run: |
          cd src/tests/ArchitectureTests
          dotnet test --configuration Release --logger "console;verbosity=normal"
```

### 使用 GitHub Secrets

1. 在仓库设置中添加 Secret：
   - Name: `POSTGRES_CONNECTION_STRING`
   - Value: `Host=your-db-host;Port=5432;Database=test_db;Username=user;Password=password`

2. 在 workflow 中使用：
   ```yaml
   - name: Run Tests
     env:
       POSTGRES_CONNECTION_STRING: ${{ secrets.POSTGRES_CONNECTION_STRING }}
     run: dotnet test
   ```

⚠️ **注意**：不要在代码中硬编码连接串或密码！

---

## 常见问题

### Q1: Docker 启动失败？

**解决方案**：
- 确保 Docker Desktop 正在运行
- 检查 Docker 权限：`sudo usermod -aG docker $USER`
- 在 CI 中确保有 Docker 支持

### Q2: 测试运行很慢？

**解决方案**：
- 使用 Schema 隔离而非每次重建容器
- 使用 Collection Fixture 共享容器
- 在 CI 中使用 Service Container 而非 Testcontainers

### Q3: 并发测试时出现冲突？

**解决方案**：
- 确保每个测试/集合使用不同的 schema
- 使用 `DocumentStoreFactory.Create()` 自动生成唯一 schema
- 避免在测试间共享可变状态

### Q4: 如何调试测试？

**解决方案**：
- 启用 Marten 日志：传入 `ILoggerFactory` 到 `DocumentStoreFactory.Create()`
- 查看容器日志：`docker logs <container_id>`
- 在 `PostgresTestContainerFixture` 中添加 `Console.WriteLine` 输出

---

## 迁移指南

### 从旧的测试工具类迁移

如果项目中已有类似的测试工具类，可以按以下步骤迁移：

1. **识别现有工具类**：
   - 查找所有 `*Fixture.cs`、`*Helper.cs`、`*Builder.cs` 文件
   - 确认它们的用途和依赖

2. **逐步替换**：
   - 先在新测试中使用新工具类
   - 逐步重构旧测试，替换为新的 API
   - 保持旧工具类直到所有测试迁移完成

3. **删除旧工具类**：
   - 确认无任何测试引用旧工具类
   - 删除旧文件
   - 更新文档

---

## 扩展建议

### 1. 添加领域特定的 Builder

```csharp
public class MemberBuilder : TestDataBuilder<Member, MemberBuilder>
{
    protected override Member CreateDefault()
    {
        return new Member
        {
            Id = Guid.NewGuid(),
            Name = "默认会员",
            Email = "member@example.com",
            CreatedAt = DateTime.UtcNow
        };
    }

    public MemberBuilder WithName(string name)
    {
        Entity.Name = name;
        return This;
    }

    public MemberBuilder WithEmail(string email)
    {
        Entity.Email = email;
        return This;
    }
}
```

### 2. 添加自定义扩展方法

```csharp
public static class MyTestExtensions
{
    public static async Task<Member> CreateTestMemberAsync(this IDocumentStore store, string name)
    {
        await using var session = store.LightweightSession();
        var member = new MemberBuilder().WithName(name).Build();
        session.Store(member);
        await session.SaveChangesAsync();
        return member;
    }
}
```

---

## 维护者

如需添加新的测试工具类或修改现有实现：

1. 确保遵循现有命名和组织规范
2. 添加 XML 文档注释
3. 提供使用示例
4. 更新本 README
5. 在至少 2-3 个测试类中验证新工具的可用性

---

## 许可证

本项目的一部分，遵循主仓库的许可证。
