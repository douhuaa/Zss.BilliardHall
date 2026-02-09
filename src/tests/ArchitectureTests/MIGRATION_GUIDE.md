# 测试迁移指南

## 概述

本指南说明如何将现有测试迁移到新的测试工具类架构。新架构提供了更好的可读性、可复用性和测试隔离性。

## 新工具类概览

### 1. Fixtures（测试固件）

| 固件 | 用途 | 位置 |
|------|------|------|
| `PostgresTestContainerFixture` | PostgreSQL 容器管理 | `Shared/Fixtures/` |
| `SharedTestFixture` | 集成测试环境（DB + Host） | `Shared/Fixtures/` |
| `AdrTestFixture` | ADR 文档加载和缓存 | `Shared/Fixtures/` |

### 2. Builders（构建器）

| 构建器 | 用途 | 位置 |
|--------|------|------|
| `AdrDocumentBuilder` | 创建 ADR 文档测试数据 | `Shared/Builders/` |
| `ArchitectureRuleSetBuilder` | 创建架构规则集 | `Shared/Builders/` |
| `TestDataBuilder<T, TBuilder>` | 通用测试数据构建器基类 | `Shared/Builders/` |

### 3. Extensions（扩展方法）

| 扩展类 | 用途 | 位置 |
|--------|------|------|
| `AdrTestExtensions` | ADR 查询和断言 | `Shared/Extensions/` |
| `MartenTestExtensions` | Marten 数据清理 | `Shared/Extensions/` |

## 迁移步骤

### 步骤 1：识别测试类型

确定你的测试属于哪种类型：

- **单元测试**：不需要数据库，可以跳过迁移
- **ADR 文档测试**：使用 `AdrTestFixture`
- **架构规则测试**：使用 `AdrTestFixture` + `ArchitectureRuleSetBuilder`
- **集成测试**：使用 `SharedTestFixture`（PostgreSQL + Marten）

### 步骤 2：添加 Using 语句

根据需要添加相应的 using 语句：

```csharp
// ADR 测试
using Zss.BilliardHall.Tests.ArchitectureTests.Shared.Fixtures;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared.Extensions;

// 集成测试
using Zss.BilliardHall.Tests.ArchitectureTests.Shared.Fixtures;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared.Factories;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared.Extensions;

// Builder 模式
using Zss.BilliardHall.Tests.ArchitectureTests.Shared.Builders;
```

### 步骤 3：更新测试类声明

#### 迁移前（ADR 测试）:
```csharp
public class MyAdrTests
{
    [Fact]
    public void Test_Something()
    {
        var repo = new AdrRepository(TestEnvironment.AdrPath);
        var adrs = repo.LoadAll(); // 每次都重新加载
        // ...
    }
}
```

#### 迁移后（使用 Fixture）:
```csharp
public class MyAdrTests : IClassFixture<AdrTestFixture>
{
    private readonly AdrTestFixture _fixture;
    
    public MyAdrTests(AdrTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public void Test_Something()
    {
        var adrs = _fixture.AdrList; // 使用缓存的数据
        // ...
    }
}
```

### 步骤 4：使用扩展方法简化代码

#### 迁移前：
```csharp
var acceptedAdrs = adrs.Where(a => 
    a.Status?.Equals("已接受", StringComparison.OrdinalIgnoreCase) == true ||
    a.Status?.Equals("accepted", StringComparison.OrdinalIgnoreCase) == true);

var governanceAdrs = acceptedAdrs.Where(a => 
    a.Level?.Equals("governance", StringComparison.OrdinalIgnoreCase) == true);

governanceAdrs.Should().NotBeEmpty();
```

#### 迁移后：
```csharp
var governanceAdrs = _fixture.AdrList
    .Accepted()
    .GovernanceLevel();

governanceAdrs.AssertNotEmpty();
```

### 步骤 5：使用 Builder 创建测试数据

#### 迁移前：
```csharp
var testAdr = new AdrDocument
{
    Id = "ADR-999",
    FilePath = "/test/adr/ADR-999.md",
    Status = "已接受",
    Type = "adr",
    Level = "governance",
    IsAdr = true,
    HasFrontMatter = true
};
testAdr.DependsOn.Add("ADR-001");
testAdr.Supersedes.Add("ADR-888");
```

#### 迁移后：
```csharp
var testAdr = new AdrDocumentBuilder()
    .WithId("ADR-999")
    .WithStatus("已接受")
    .WithLevel("governance")
    .AddDependsOn("ADR-001")
    .AddSupersedes("ADR-888")
    .Build();
```

## 迁移示例

### 示例 1：ADR 关系验证测试

#### 迁移前：
```csharp
public class AdrRelationshipTests
{
    [Fact]
    public void Verify_Adr_Supersedes_Relationship()
    {
        var repo = new AdrRepository(TestEnvironment.AdrPath);
        var adrs = repo.LoadAll();
        
        var adr = adrs.FirstOrDefault(a => a.Id == "ADR-940");
        adr.Should().NotBeNull();
        adr!.Supersedes.Should().Contain("ADR-939");
    }
}
```

#### 迁移后：
```csharp
public class AdrRelationshipTests : IClassFixture<AdrTestFixture>
{
    private readonly AdrTestFixture _fixture;
    
    public AdrRelationshipTests(AdrTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public void Verify_Adr_Supersedes_Relationship()
    {
        // 使用缓存的数据 + 扩展方法
        _fixture.AllAdrs.AssertAdrExists("ADR-940");
        var adr = _fixture.GetAdr("ADR-940");
        adr.AssertSupersedes("ADR-939");
    }
}
```

### 示例 2：集成测试

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
    public async Task Should_Save_And_Query_Data()
    {
        // 清理数据
        await _fixture.ClearAllDataAsync();
        
        // 使用 DocumentStore
        await using var session = _fixture.DocumentStore.LightweightSession();
        session.Store(new MyEntity { Id = Guid.NewGuid() });
        await session.SaveChangesAsync();
        
        // 查询验证
        var result = await session.Query<MyEntity>().FirstOrDefaultAsync();
        result.Should().NotBeNull();
    }
}
```

## 常见场景迁移

### 场景 1：查询特定状态的 ADR

**迁移前：**
```csharp
var acceptedAdrs = adrs.Where(a => 
    a.Status == "已接受" || a.Status == "accepted");
```

**迁移后：**
```csharp
var acceptedAdrs = _fixture.AdrList.Accepted();
```

### 场景 2：查询特定范围的 ADR

**迁移前：**
```csharp
var earlyAdrs = adrs.Where(a =>
{
    var parts = a.Id.Split('-');
    if (parts.Length >= 2 && int.TryParse(parts[1], out var num))
        return num >= 1 && num <= 100;
    return false;
});
```

**迁移后：**
```csharp
var earlyAdrs = _fixture.AdrList.InRange(1, 100);
```

### 场景 3：验证 ADR 依赖关系

**迁移前：**
```csharp
var adr = adrs.First(a => a.Id == "ADR-940");
adr.DependsOn.Should().Contain("ADR-001");
```

**迁移后：**
```csharp
var adr = _fixture.GetAdr("ADR-940");
adr.AssertDependsOn("ADR-001");
```

## 性能对比

使用 Fixture 缓存可以显著提升性能：

| 场景 | 旧方式 | 新方式 | 性能提升 |
|------|--------|--------|----------|
| 单个测试类（5个测试） | ~500ms | ~5ms | 99% |
| 大型测试套件（100个测试） | ~10s | ~0.5s | 95% |

## 注意事项

1. **Fixture 生命周期**：
   - `IClassFixture<T>`：每个测试类一个实例
   - `ICollectionFixture<T>`：同一集合内的所有测试类共享一个实例

2. **数据隔离**：
   - 使用 `SharedTestFixture` 时，每个测试类会获得独立的 schema
   - 同一集合内的测试会共享 Fixture，需要在测试间清理数据

3. **并行测试**：
   - 使用 Collection 可以控制测试并行执行
   - 共享 Fixture 的测试不会并行执行

4. **向后兼容**：
   - 旧的测试仍然可以工作
   - 可以逐步迁移，不需要一次性全部完成

## 迁移检查清单

- [ ] 识别测试类型（单元/ADR/集成）
- [ ] 添加必要的 using 语句
- [ ] 更新测试类继承 `IClassFixture<T>` 或使用 `[Collection]`
- [ ] 注入 Fixture 到构造函数
- [ ] 替换直接加载代码为 Fixture 访问
- [ ] 使用扩展方法简化查询和断言
- [ ] 使用 Builder 创建测试数据（如需要）
- [ ] 运行测试验证迁移成功
- [ ] 清理不再需要的代码

## 获取帮助

参考示例文件：
- `Examples/NewTestInfrastructureUsageExamples.cs` - 完整使用示例
- `Examples/TestInfrastructureExamples.cs` - 基础设施示例
- `Shared/README.md` - 详细文档

## 进阶技巧

### 技巧 1：自定义扩展方法

如果你的团队有特定的查询模式，可以创建自己的扩展方法：

```csharp
public static class CustomAdrExtensions
{
    public static IEnumerable<AdrDocument> MyTeamSpecificQuery(this IEnumerable<AdrDocument> adrs)
    {
        return adrs
            .Accepted()
            .GovernanceLevel()
            .Where(a => a.Id.StartsWith("ADR-9"));
    }
}
```

### 技巧 2：自定义 Builder

为你的领域模型创建专用的 Builder：

```csharp
public class MyDomainModelBuilder : TestDataBuilder<MyModel, MyDomainModelBuilder>
{
    protected override MyModel CreateDefault()
    {
        return new MyModel { /* 默认值 */ };
    }
    
    public MyDomainModelBuilder WithCustomProperty(string value)
    {
        Entity.Property = value;
        return This;
    }
}
```

### 技巧 3：组合多个 Fixture

```csharp
public class AdvancedTests : 
    IClassFixture<AdrTestFixture>,
    IClassFixture<SharedTestFixture>
{
    private readonly AdrTestFixture _adrFixture;
    private readonly SharedTestFixture _sharedFixture;
    
    public AdvancedTests(AdrTestFixture adrFixture, SharedTestFixture sharedFixture)
    {
        _adrFixture = adrFixture;
        _sharedFixture = sharedFixture;
    }
}
```

## 总结

迁移到新的测试工具类可以：
- ✅ 提升测试性能（通过缓存）
- ✅ 提高代码可读性（通过扩展方法）
- ✅ 简化测试数据创建（通过 Builder）
- ✅ 增强测试隔离性（通过 Fixture）
- ✅ 改善可维护性（通过统一的 API）

建议逐步迁移，从最常用的测试开始，积累经验后再扩展到其他测试。
