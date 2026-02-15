# 测试项目结构说明

本目录包含了 Zss.BilliardHall 项目的所有测试项目，按照职责拆分为以下几个独立项目：

## 📁 项目结构

```
src/tests/
├── ArchitectureTests/       # 架构规则测试
├── IntegrationTests/        # 集成测试
├── UnitTests/              # 单元测试
├── SharedTestHelpers/      # 共享测试辅助代码
└── Directory.Build.props   # 测试项目共享配置
```

## 🎯 各项目说明

### ArchitectureTests

**职责**：验证架构规则和约束，确保代码符合 ADR（架构决策记录）

**依赖**：
- NetArchTest.Rules - 架构测试框架
- SharedTestHelpers - 共享辅助代码

**包含内容**：
- ADR 规则验证测试
- 命名空间约束测试
- 模块边界测试
- 依赖关系测试

**运行方式**：
```bash
dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj
```

### IntegrationTests

**职责**：测试多个组件之间的集成，包括数据库、HTTP、消息队列等

**依赖**：
- Testcontainers - Docker 容器测试
- Testcontainers.PostgreSql - PostgreSQL 容器
- Microsoft.AspNetCore.Mvc.Testing - Web API 测试
- Marten.AspNetCore - 文档数据库
- WolverineFx.Http - HTTP 客户端
- SharedTestHelpers - 共享辅助代码

**包含内容**：
- API 端点集成测试
- 数据库集成测试
- 消息处理集成测试
- 完整业务流程测试

**运行方式**：
```bash
# 需要 Docker 环境
dotnet test src/tests/IntegrationTests/IntegrationTests.csproj
```

### UnitTests

**职责**：单元测试，验证单个类或方法的行为

**依赖**：
- Moq - Mock 框架
- FluentAssertions - 断言库
- SharedTestHelpers - 共享辅助代码

**包含内容**：
- 业务逻辑单元测试
- 领域模型测试
- 工具类测试

**运行方式**：
```bash
dotnet test src/tests/UnitTests/UnitTests.csproj
```

### SharedTestHelpers

**职责**：提供可复用的测试辅助代码、Fixtures 和工具类

**包含内容**：
- `Adr/` - ADR 文档加载和解析
- `Assemblies/` - 程序集加载和分析
- `FileSystem/` - 文件系统测试工具
- `Testing/` - 通用测试工具

**不是测试项目**：此项目不包含测试，仅提供测试基础设施

## 🚀 CI/CD 集成

测试在 CI 中按以下策略执行：

### 1. unit-and-architecture-tests（并行执行，快速反馈）
- ✅ 单元测试
- ✅ 架构测试
- ⚡ 不需要外部依赖
- ⏱️ 预计耗时：< 5 分钟

### 2. integration-tests（串行执行，依赖前者成功）
- ✅ 集成测试
- 🐳 需要 Docker 支持
- ⏱️ 预计耗时：5-10 分钟

## 📝 命名约定

- 测试类名：`{TestedClass}_Tests.cs` 或 `{Feature}_Tests.cs`
- 测试方法名：`{MethodName}_{Scenario}_{ExpectedBehavior}`
- 测试项目命名空间：`Zss.BilliardHall.Tests.{ProjectName}`

## 🔧 添加新测试

### 添加单元测试

```csharp
// src/tests/UnitTests/Domain/OrderCalculator_Tests.cs
namespace Zss.BilliardHall.Tests.UnitTests.Domain;

public class OrderCalculator_Tests
{
    [Fact]
    public void CalculateTotal_WithValidItems_ReturnsCorrectSum()
    {
        // Arrange
        var calculator = new OrderCalculator();
        
        // Act
        var total = calculator.CalculateTotal(items);
        
        // Assert
        total.Should().Be(expectedTotal);
    }
}
```

### 添加集成测试

```csharp
// src/tests/IntegrationTests/Api/OrderEndpoints_Tests.cs
namespace Zss.BilliardHall.Tests.IntegrationTests.Api;

public class OrderEndpoints_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public OrderEndpoints_Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreatedOrder()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.PostAsJsonAsync("/api/orders", orderData);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### 添加架构测试

```csharp
// src/tests/ArchitectureTests/Specification/RuleSets/ADR999/Adr999_Tests.cs
namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR999;

public class Adr999_Tests
{
    [Fact]
    public void Rule_MyArchitectureRule_ShouldBeValid()
    {
        // Arrange
        var types = Types.InAssembly(typeof(MyClass).Assembly);
        
        // Act
        var result = types
            .That().ResideInNamespace("MyNamespace")
            .Should().HaveNameEndingWith("Handler")
            .GetResult();
        
        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}
```

## 📚 相关文档

- [ADR-900: 架构测试与 CI 治理元规则](../../docs/adr/governance/ADR-900-Architecture-Tests-Metadata-Enforcement.md)
- [ADR-907: ArchitectureTests 执法治理体系](../../docs/adr/governance/ADR-907-ArchitectureTests-Enforcement-Governance.md)
- [测试最佳实践](../../docs/testing-best-practices.md)

## 🛠️ 常见问题

### Q: 集成测试需要 Docker 吗？
A: 是的，集成测试使用 Testcontainers 需要 Docker 环境。本地开发时确保 Docker 已启动。

### Q: 如何在 CI 中禁用集成测试？
A: 集成测试在 CI 中是独立的 job，可以在 workflow 中注释掉 `integration-tests` job。

### Q: 共享的测试工具应该放在哪里？
A: 放在 `SharedTestHelpers` 项目中，按功能分类到不同的子目录。

### Q: 如何运行特定类别的测试？
A: 使用 `--filter` 参数：
```bash
# 运行特定测试类
dotnet test --filter "FullyQualifiedName~MyTests"

# 运行特定命名空间
dotnet test --filter "FullyQualifiedName~MyNamespace"
```

## 📊 测试覆盖率

测试覆盖率报告在 CI 中自动生成，可以在 Actions artifacts 中下载。

本地生成覆盖率报告：
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 🔄 迁移说明

如果你有旧的测试代码需要迁移：

1. **单元测试** → 移动到 `UnitTests` 项目
2. **集成测试** → 移动到 `IntegrationTests` 项目
3. **架构测试** → 移动到 `ArchitectureTests` 项目
4. **测试工具** → 移动到 `SharedTestHelpers` 项目

迁移时记得更新命名空间和项目引用。
