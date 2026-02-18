# NetArchTest 使用指南

> **文档版本**: 1.0  
> **最后更新**: 2026-02-09  
> **适用项目**: Zss.BilliardHall Architecture Tests

---

## 📋 目录

1. [什么是 NetArchTest](#什么是-netarchtest)
2. [为什么使用 NetArchTest](#为什么使用-netarchtest)
3. [最佳实践](#最佳实践)
4. [使用示例](#使用示例)
5. [常见模式](#常见模式)
6. [性能优化](#性能优化)
7. [故障排查](#故障排查)

---

## 什么是 NetArchTest

NetArchTest 是一个用于 .NET 的架构测试库，它提供流畅的 API 来验证代码库的架构约束。

**核心理念**：将架构规则转换为可执行的自动化测试，在 CI/CD 流程中及早发现架构违规。

**主要特性**：
- ✅ 流畅的 API（链式调用）
- ✅ 支持命名空间、依赖、继承等多种规则
- ✅ 集成主流测试框架（xUnit、NUnit、MSTest）
- ✅ 清晰的错误消息和违规类型列表

---

## 为什么使用 NetArchTest

### 传统方式的问题

**手动审查**：
```csharp
// ❌ 人工代码审查，易出错、不可扩展
// PR Review: "这个类不应该依赖那个模块..."
```

**运行时检查**：
```csharp
// ❌ 运行时才发现问题，成本高
if (someClass.GetType().Namespace.StartsWith("ForbiddenNamespace"))
{
    throw new InvalidOperationException("架构违规！");
}
```

### NetArchTest 方式

**编译时验证** + **CI 集成**：
```csharp
// ✅ 自动化、可重复、在 CI 中执行
[Fact]
public void Domain_Should_Not_Depend_On_Infrastructure()
{
    var result = Types.InAssembly(domainAssembly)
        .ShouldNot()
        .HaveDependencyOn("Infrastructure")
        .GetResult();
    
    result.IsSuccessful.Should().BeTrue();
}
```

**收益**：
- 📉 减少人工审查成本
- ⚡ 快速反馈（秒级）
- 🔒 强制执行架构约束
- 📊 可视化违规类型

---

## 最佳实践

### 1. 使用 `Types.InAssembly()` 而非 `Types.InCurrentDomain()`

```csharp
// ✅ 推荐：明确指定程序集，性能更好
var result = Types.InAssembly(typeof(MyClass).Assembly)
    .That()...
    .GetResult();

// ❌ 避免：扫描所有程序集，性能差
var result = Types.InCurrentDomain()
    .That()...
    .GetResult();
```

**原因**：
- `InAssembly()` 只扫描指定程序集，速度快
- `InCurrentDomain()` 扫描所有已加载程序集，包括框架程序集，速度慢

### 2. 使用链式调用提高可读性

```csharp
// ✅ 推荐：链式调用，语义清晰
var result = Types.InAssembly(assembly)
    .That()
    .ResideInNamespace("MyApp.Domain")
    .And()
    .AreClasses()
    .And()
    .AreNotAbstract()
    .Should()
    .BeSealed()
    .GetResult();

// ❌ 避免：拆分调用，可读性差
var types = Types.InAssembly(assembly);
var filtered = types.That().ResideInNamespace("MyApp.Domain");
var classes = filtered.And().AreClasses();
var result = classes.Should().BeSealed().GetResult();
```

### 3. 始终检查 `IsSuccessful` 并提供清晰错误消息

```csharp
// ✅ 推荐：标准化错误消息
var result = Types.InAssembly(assembly)
    .Should()
    .BeSealed()
    .GetResult();

if (!result.IsSuccessful)
{
    var message = AssertionMessageBuilder.BuildFromArchTestResult(
        ruleId: "ADR-001.1.1",
        summary: "类未标记为 sealed",
        failingTypeNames: result.FailingTypeNames,
        remediationSteps: new[]
        {
            "将违规类标记为 sealed",
            "或将类设计为 abstract 以允许继承"
        },
        adrReference: "docs/adr/001.md");
    
    result.IsSuccessful.Should().BeTrue(message);
}

// ❌ 避免：无错误消息或模糊消息
result.IsSuccessful.Should().BeTrue(); // 什么出错了？
result.IsSuccessful.Should().BeTrue("Some error"); // 太模糊
```

### 4. 使用辅助类封装常用模式

```csharp
// ✅ 推荐：封装到辅助类
NetArchTestHelper.AssertNamespaceConvention(
    assembly: moduleAssembly,
    expectedNamespacePrefix: "MyApp.Module",
    ruleId: "ADR-001.1.3",
    adrReference: "docs/adr/001.md");

// ❌ 避免：每个测试重复编写
var result = Types.InAssembly(assembly)
    .That()
    .AreNotNested()
    .Should()
    .ResideInNamespaceMatching("^MyApp\\.Module.*")
    .GetResult();
// ... 重复的错误处理代码
```

### 5. 集成 RuleSetRegistry

```csharp
// ✅ 推荐：从 Registry 获取规则元数据
public sealed class MyArchitectureTests
{
    private readonly ArchitectureRuleSet _ruleSet;

    public MyArchitectureTests()
    {
        _ruleSet = RuleSetRegistry.GetStrict(1);
    }

    [Fact]
    public void Test()
    {
        var clause = _ruleSet.GetClause(1, 1);
        var ruleId = clause.Id.ToString(); // "ADR-001.1.1"
        // 使用 ruleId 进行测试...
    }
}

// ❌ 避免：硬编码规则信息
[Fact]
public void Test()
{
    var ruleId = "ADR-001.1.1"; // 硬编码，不易维护
}
```

---

## 使用示例

### 示例 1: 命名空间约定

验证所有类型都在预期的命名空间下：

```csharp
[Theory]
[ClassData(typeof(ModuleAssemblyData))]
public void All_Types_Should_Be_In_Module_Namespace(Assembly moduleAssembly)
{
    var moduleName = moduleAssembly.GetName().Name?.Split('.').Last();
    var expectedPrefix = $"Zss.BilliardHall.Modules.{moduleName}";

    var result = Types.InAssembly(moduleAssembly)
        .That()
        .AreNotNested()
        .And()
        .DoNotHaveName("AssemblyInfo")
        .Should()
        .ResideInNamespaceMatching($"^{Regex.Escape(expectedPrefix)}.*")
        .GetResult();

    result.IsSuccessful.Should().BeTrue(
        $"违规类型: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
}
```

### 示例 2: 依赖规则

验证模块不依赖其他模块：

```csharp
[Theory]
[ClassData(typeof(ModuleAssemblyData))]
public void Module_Should_Not_Depend_On_Other_Modules(Assembly moduleAssembly)
{
    var currentModule = GetModuleName(moduleAssembly);
    var otherModules = GetOtherModuleNamespaces(currentModule);

    foreach (var otherModule in otherModules)
    {
        var result = Types.InAssembly(moduleAssembly)
            .ShouldNot()
            .HaveDependencyOn(otherModule)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"模块 {currentModule} 不应依赖 {otherModule}\n" +
            $"违规类型: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }
}
```

### 示例 3: Sealed 类规则

验证特定命名空间下的类都是 sealed：

```csharp
[Fact]
public void Service_Classes_Should_Be_Sealed()
{
    var result = Types.InAssembly(typeof(MyService).Assembly)
        .That()
        .ResideInNamespace("MyApp.Services")
        .And()
        .AreClasses()
        .And()
        .AreNotAbstract()
        .Should()
        .BeSealed()
        .GetResult();

    if (!result.IsSuccessful)
    {
        var violations = string.Join("\n  - ", 
            result.FailingTypeNames ?? Array.Empty<string>());
        
        result.IsSuccessful.Should().BeTrue(
            $"以下服务类未标记为 sealed:\n  - {violations}\n\n" +
            $"修复：在类定义前添加 'sealed' 关键字");
    }
}
```

### 示例 4: 接口实现规则

验证所有Repository类实现IRepository接口：

```csharp
[Fact]
public void Repository_Classes_Should_Implement_IRepository()
{
    var result = Types.InAssembly(typeof(MyRepository).Assembly)
        .That()
        .HaveNameEndingWith("Repository")
        .And()
        .AreClasses()
        .Should()
        .ImplementInterface("IRepository")
        .GetResult();

    result.IsSuccessful.Should().BeTrue(
        $"以下Repository类未实现IRepository接口:\n" +
        $"{string.Join("\n", result.FailingTypeNames ?? Array.Empty<string>())}");
}
```

---

## 常见模式

### 模式 1: That() + Should() 组合

```csharp
// 筛选 + 验证
Types.InAssembly(assembly)
    .That()                          // 开始筛选条件
    .ResideInNamespace("MyApp")      // 筛选：在某命名空间
    .And()                           // 连接多个筛选条件
    .AreClasses()                    // 筛选：是类
    .Should()                        // 开始验证条件
    .BeSealed()                      // 验证：应该是 sealed
    .GetResult();                    // 获取结果
```

### 模式 2: ShouldNot() 禁止性规则

```csharp
// 禁止依赖
Types.InAssembly(domainAssembly)
    .ShouldNot()                     // 禁止性规则
    .HaveDependencyOn("Infrastructure")
    .GetResult();
```

### 模式 3: HaveNameMatching() 正则匹配

```csharp
// 使用正则表达式匹配名称
Types.InAssembly(assembly)
    .That()
    .HaveNameMatching(".*Service$|.*Handler$")  // 以Service或Handler结尾
    .Should()
    .BeSealed()
    .GetResult();
```

### 模式 4: 参数化测试批量验证

```csharp
[Theory]
[MemberData(nameof(GetModuleAssemblies))]
public void Module_Tests(Assembly assembly)
{
    // 对每个模块执行相同测试
    NetArchTestHelper.AssertNoDependencyOn(...);
}

public static IEnumerable<object[]> GetModuleAssemblies()
{
    foreach (var asm in ModuleAssemblyData.ModuleAssemblies)
    {
        yield return new object[] { asm };
    }
}
```

---

## 性能优化

### 1. 程序集缓存

```csharp
// ✅ 推荐：使用 Lazy<T> 缓存程序集
private static readonly Lazy<Assembly[]> _assemblies = new(() => 
{
    return LoadAssemblies();
});

public static Assembly[] Assemblies => _assemblies.Value;
```

### 2. 避免重复扫描

```csharp
// ✅ 推荐：一次获取类型，多次使用
var types = Types.InAssembly(assembly).GetTypes();

// 然后对 types 进行多次过滤...
var sealedTypes = types.Where(t => t.IsSealed);
var abstractTypes = types.Where(t => t.IsAbstract);

// ❌ 避免：每次都重新扫描
Types.InAssembly(assembly).That().AreSealed().GetTypes();
Types.InAssembly(assembly).That().AreAbstract().GetTypes();
```

### 3. 使用并行测试（谨慎）

```csharp
// xUnit 默认并行运行测试类
// 确保测试无共享状态，可安全并行
public sealed class MyTests // sealed 类可并行
{
    [Fact]
    public void Test1() { }
    
    [Fact]
    public void Test2() { }
}
```

---

## 故障排查

### 问题 1: "未找到任何类型"

**症状**：
```
Expected result.IsSuccessful to be true, but found false.
Failing types: (empty)
```

**原因**：筛选条件过于严格，没有匹配到任何类型

**解决方案**：
```csharp
// 调试：先查看匹配了哪些类型
var matchedTypes = Types.InAssembly(assembly)
    .That()
    .ResideInNamespace("MyApp")
    .GetTypes();

Console.WriteLine($"匹配到 {matchedTypes.Count()} 个类型:");
foreach (var type in matchedTypes)
{
    Console.WriteLine($"  - {type.FullName}");
}
```

### 问题 2: "程序集未加载"

**症状**：
```
FileNotFoundException: Could not load file or assembly 'MyModule'
```

**解决方案**：
```csharp
// 确保程序集已构建
// 1. 运行 dotnet build
// 2. 检查输出路径
// 3. 使用 ModuleAssemblyData 自动发现
```

### 问题 3: "测试很慢"

**原因**：使用了 `Types.InCurrentDomain()` 或重复扫描

**解决方案**：
```csharp
// ✅ 使用 Types.InAssembly()
// ✅ 缓存程序集引用
// ✅ 避免不必要的 GetTypes() 调用
```

---

## 参考资源

- **NetArchTest GitHub**: https://github.com/BenMorris/NetArchTest
- **本项目架构测试指南**: [ARCHITECTURE-TEST-GUIDELINES.md](./ARCHITECTURE-TEST-GUIDELINES.md)
- **AssertionMessageBuilder 使用**: [Shared/AssertionMessageBuilder.cs](../../ArchitectureTests/Shared/AssertionMessageBuilder.cs)
- **NetArchTestHelper 使用**: [Shared/NetArchTestHelper.cs](../../ArchitectureTests/Shared/NetArchTestHelper.cs)

---

## 下一步

1. ✅ 阅读本指南
2. ✅ 查看 `Adr001_Module_Isolation_Tests.cs` 示例
3. ✅ 使用 `NetArchTestHelper` 编写自己的测试
4. ✅ 集成到 CI/CD 流程

**祝测试愉快！** 🎯
