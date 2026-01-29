# 架构测试编写案例

**难度**：🟡 中等  
**相关 ADR**：[ADR-0000](../adr/constitutional/ADR-0000-architecture-test-ci-governance-constitution.md)  
**作者**：@copilot  
**日期**：2026-01-29  
**标签**：架构测试, NetArchTest, 自动化, 治理

---

## 适用场景

当你需要为新的架构约束编写自动化测试时，本案例展示如何使用 NetArchTest 编写架构测试。

**适用于**：
- 新增 ADR 后需要添加对应的架构测试
- 发现架构违规需要防止再次发生
- 需要验证模块隔离、依赖方向等架构约束

---

## 背景

根据 ADR-0000，所有架构约束都必须通过自动化测试强制执行。架构测试是"司法权"，用于验证代码是否符合 ADR（立法权）定义的规则。

### 为什么需要架构测试

- **人工审查不可靠**：Code Review 容易遗漏
- **持续验证**：CI/CD 自动运行，每次提交都检查
- **文档即测试**：测试即是可执行的架构文档
- **快速反馈**：违规时立即发现，而非上线后

---

## 解决方案

### 架构设计

架构测试位于 `src/tests/ArchitectureTests/` 目录，按 ADR 编号组织：

```
src/tests/ArchitectureTests/
├── ADR/
│   ├── ADR_0000_Architecture_Tests.cs
│   ├── ADR_0001_Architecture_Tests.cs
│   ├── ADR_0002_Architecture_Tests.cs
│   └── ...
└── ArchitectureTests.csproj
```

每个 ADR 的架构测试都在独立的类中，便于维护和追溯。

### 代码实现

#### 示例 1：验证模块隔离（ADR-0001）

```csharp
using NetArchTest.Rules;
using Xunit;

namespace ArchitectureTests.ADR;

/// <summary>
/// ADR-0001：模块化单体与垂直切片架构
/// 验证模块隔离规则
/// </summary>
public class ADR_0001_Architecture_Tests
{
    private const string ModulesNamespace = "Zss.BilliardHall.Modules";

    [Fact]
    public void Modules_Should_Not_Reference_Other_Modules()
    {
        // Arrange
        var modules = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(ModulesNamespace)
            .GetTypes();

        // Act & Assert
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace($"{ModulesNamespace}.Orders")
            .ShouldNot()
            .HaveDependencyOn($"{ModulesNamespace}.Members")
            .And()
            .ShouldNot()
            .HaveDependencyOn($"{ModulesNamespace}.Billing")
            .GetResult();

        Assert.True(result.IsSuccessful, 
            $"模块间不应直接引用。违规类型：{string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Modules_Should_Only_Communicate_Through_Events_Or_Contracts()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace($"{ModulesNamespace}.Orders.Domain")
            .ShouldNot()
            .HaveDependencyOn($"{ModulesNamespace}.Members.Domain")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            "模块的领域模型不应依赖其他模块的领域模型");
    }
}
```

#### 示例 2：验证依赖方向（ADR-0002）

```csharp
/// <summary>
/// ADR-0002：平台、应用与主机启动器架构
/// 验证层级依赖方向
/// </summary>
public class ADR_0002_Architecture_Tests
{
    [Fact]
    public void Platform_Should_Not_Depend_On_Application()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace("Zss.BilliardHall.Platform")
            .ShouldNot()
            .HaveDependencyOn("Zss.BilliardHall.Application")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Platform 层不应依赖 Application 层。违规类型：{string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Platform_Should_Not_Depend_On_Host()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace("Zss.BilliardHall.Platform")
            .ShouldNot()
            .HaveDependencyOn("Zss.BilliardHall.Host")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            "Platform 层不应依赖 Host 层");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Host()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace("Zss.BilliardHall.Application")
            .ShouldNot()
            .HaveDependencyOn("Zss.BilliardHall.Host")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            "Application 层不应依赖 Host 层");
    }
}
```

#### 示例 3：验证命名约定（ADR-0005）

```csharp
/// <summary>
/// ADR-0005：应用内交互模型与执行边界
/// 验证 Handler 规则
/// </summary>
public class ADR_0005_Architecture_Tests
{
    [Fact]
    public void Command_Handlers_Should_Return_Void_Or_Id()
    {
        // Arrange
        var handlers = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespaceEndingWith(".UseCases")
            .And()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        // Act
        var invalidHandlers = handlers
            .Where(IsCommandHandler)
            .Where(h => !ReturnsVoidOrId(h))
            .ToList();

        // Assert
        Assert.Empty(invalidHandlers);
    }

    private static bool IsCommandHandler(Type type)
    {
        // 检查是否实现 IRequestHandler<TCommand, TResponse>
        // 且 TCommand 的名称不包含 "Query"
        var interfaces = type.GetInterfaces();
        return interfaces.Any(i => 
            i.IsGenericType &&
            i.GetGenericTypeDefinition().Name.Contains("IRequestHandler") &&
            !i.GetGenericArguments()[0].Name.Contains("Query"));
    }

    private static bool ReturnsVoidOrId(Type handlerType)
    {
        var returnType = handlerType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType)?
            .GetGenericArguments()
            .LastOrDefault();

        return returnType == typeof(void) ||
               returnType == typeof(Guid) ||
               returnType == typeof(int) ||
               returnType == typeof(string);
    }
}
```

### 测试验证

运行架构测试：

```bash
# 运行所有架构测试
dotnet test src/tests/ArchitectureTests/

# 运行特定 ADR 的测试
dotnet test --filter "FullyQualifiedName~ADR_0001"

# 在 CI 中运行
dotnet test src/tests/ArchitectureTests/ --logger "console;verbosity=detailed"
```

**预期结果**：
- ✅ 所有测试通过 - 代码符合架构约束
- ❌ 测试失败 - 输出违规的具体类型和位置

---

## 常见陷阱

### 1. 测试过于宽松

❌ **错误**：
```csharp
[Fact]
public void Some_Test()
{
    var result = Types.InAssembly(assembly)
        .Should()
        .ResideInNamespace("SomeNamespace")
        .GetResult();
    
    // 没有实际验证任何约束
    Assert.True(result.IsSuccessful);
}
```

✅ **正确**：
```csharp
[Fact]
public void Modules_Should_Not_Reference_Other_Modules()
{
    var result = Types.InAssembly(assembly)
        .That()
        .ResideInNamespace("Modules.Orders")
        .ShouldNot()
        .HaveDependencyOn("Modules.Members")  // 明确的约束
        .GetResult();
    
    Assert.True(result.IsSuccessful, "提供清晰的错误消息");
}
```

### 2. 忘记排除测试代码

❌ **错误**：
```csharp
var result = Types.InCurrentDomain()  // 包含测试程序集
    .That()
    .ResideInNamespace("Platform")
    .ShouldNot()
    .HaveDependencyOn("Application")
    .GetResult();
```

✅ **正确**：
```csharp
var result = Types.InAssembly(typeof(Program).Assembly)  // 仅生产代码
    .That()
    .ResideInNamespace("Platform")
    .ShouldNot()
    .HaveDependencyOn("Application")
    .GetResult();
```

### 3. 错误消息不清晰

❌ **错误**：
```csharp
Assert.True(result.IsSuccessful);  // 失败时不知道原因
```

✅ **正确**：
```csharp
Assert.True(result.IsSuccessful, 
    $"发现违规类型：{string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
```

### 4. 测试名称不明确

❌ **错误**：
```csharp
[Fact]
public void Test1() { }
```

✅ **正确**：
```csharp
[Fact]
public void Platform_Should_Not_Depend_On_Application() { }
```

---

## 最佳实践

### 1. 一个测试验证一个规则

```csharp
// 好的实践：清晰单一的职责
[Fact]
public void Platform_Should_Not_Depend_On_Application() { }

[Fact]
public void Platform_Should_Not_Depend_On_Host() { }
```

### 2. 使用描述性的测试名称

测试名称应该清楚地说明它验证的规则：
- `{What}_Should_{Expected_Behavior}`
- `{What}_ShouldNot_{Prohibited_Behavior}`

### 3. 提供有用的错误消息

```csharp
Assert.True(result.IsSuccessful, 
    $"违规说明：{规则描述}\n" +
    $"违规类型：{string.Join("\n", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
```

### 4. 组织测试代码

按 ADR 组织，每个 ADR 一个测试类：
- 便于维护和追溯
- 测试失败时快速定位相关 ADR
- 与 ADR 版本同步更新

---

## 参考资料

- [ADR-0000：架构测试与 CI 治理宪法](../adr/constitutional/ADR-0000-architecture-test-ci-governance-constitution.md)
- [ADR-0001：模块化单体与垂直切片架构](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0002：平台、应用与主机启动器架构](../adr/constitutional/ADR-0002-platform-application-host-bootstrap.md)
- [NetArchTest 文档](https://github.com/BenMorris/NetArchTest)
- [架构测试指南](../guides/test-architecture-guide.md)

---

**维护**：Tech Lead  
**状态**：✅ Active  
**版本**：1.0
