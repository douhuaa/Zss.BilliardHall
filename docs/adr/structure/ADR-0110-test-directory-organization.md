# ADR-0110：测试目录组织规范

**状态**：✅ 已采纳  
**级别**：结构层 / 最佳实践  
**适用范围**：所有测试代码  
**生效时间**：2026-01-22  
**编号段**：ADR-110~119（目录结构与工程组织）

---

## 本章聚焦内容（Focus）

本 ADR 定义测试代码的目录组织规范，明确：
- 架构测试的集中管理模式
- 单元测试的目录镜像规则
- 测试项目的命名约定
- 测试文件的组织原则

**不在本 ADR 范围内**：
- 测试编写方法论（测试驱动开发、行为驱动开发等）
- 具体测试框架的选择（参见技术层 ADR）
- 测试覆盖率要求（参见治理层规范）

---

## 术语表（Glossary）

| 术语 | 定义 |
|------|------|
| **架构测试** | 验证架构约束的测试，使用静态分析验证代码结构 |
| **单元测试** | 验证单个组件行为的测试 |
| **集成测试** | 验证多个组件协作的测试 |
| **测试镜像** | 测试目录结构与源代码目录结构保持一致 |
| **ADR 映射测试** | 每个 ADR 对应一个测试类的模式 |

---

## 核心决策（Decision）

### 1. 测试目录层次结构

```
src/
├── tests/                              # 测试根目录
│   ├── ArchitectureTests/              # 架构测试（集中式）
│   │   ├── ADR/                        # ADR 映射测试
│   │   │   ├── ADR_0001_Architecture_Tests.cs
│   │   │   ├── ADR_0002_Architecture_Tests.cs
│   │   │   └── ...
│   │   ├── ArchitectureTests.csproj
│   │   ├── TestData.cs
│   │   └── README.md
│   │
│   ├── Modules.Members.Tests/          # Members 模块单元测试
│   │   ├── Features/                   # 镜像源码 Features 结构
│   │   │   ├── CreateMember/
│   │   │   │   └── CreateMemberHandlerTests.cs
│   │   │   └── GetMemberById/
│   │   │       └── GetMemberByIdHandlerTests.cs
│   │   └── Modules.Members.Tests.csproj
│   │
│   ├── Modules.Orders.Tests/           # Orders 模块单元测试
│   │   └── Features/
│   │       └── CreateOrder/
│   │           └── CreateOrderHandlerTests.cs
│   │
│   └── Integration.Tests/              # 集成测试（可选）
│       ├── Scenarios/                  # 按业务场景组织
│       └── Integration.Tests.csproj
```

### 2. 架构测试规则

#### 2.1 集中式管理
**决策**：所有架构测试集中在单一项目 `ArchitectureTests` 中。

**理由**：
- 架构约束是**全局性**的，不属于特定模块
- 集中管理便于统一维护和演进
- 避免测试代码在各模块重复

**禁止**：
- ❌ 在各模块项目中分散架构测试
- ❌ 创建多个架构测试项目

#### 2.2 ADR 映射模式
**决策**：每个 ADR 对应一个测试类 `ADR_{编号:D4}_Architecture_Tests.cs`。

**命名规则**：
```csharp
// 文件名：ADR_0001_Architecture_Tests.cs
namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

public class ADR_0001_Architecture_Tests
{
    [Theory(DisplayName = "ADR-0001.1: 模块不应相互引用")]
    public void Modules_Should_Not_Reference_Each_Other(Assembly assembly)
    {
        // 测试实现
    }
}
```

**测试方法命名**：
- `[Theory(DisplayName = "ADR-{编号}.{子编号}: {约束描述}")]`
- 方法名使用英文，遵循 `{约束主体}_{Should/ShouldNot}_{预期行为}`

#### 2.3 测试数据提供者
**决策**：使用共享的测试数据提供者类获取待测程序集。

**示例**：
```csharp
// TestData.cs
public class ModuleAssemblyData : TheoryData<Assembly>
{
    public ModuleAssemblyData()
    {
        Add(typeof(Modules.Members.MembersBootstrapper).Assembly);
        Add(typeof(Modules.Orders.OrdersBootstrapper).Assembly);
    }
}

// 使用
[Theory(DisplayName = "ADR-0001.1: 模块不应相互引用")]
[ClassData(typeof(ModuleAssemblyData))]
public void Modules_Should_Not_Reference_Each_Other(Assembly assembly)
{
    // 测试实现
}
```

### 3. 单元测试规则

#### 3.1 目录镜像原则
**决策**：单元测试项目必须镜像源代码项目的目录结构。

**映射规则**：
```
源代码：src/Modules/Members/Features/CreateMember/CreateMemberHandler.cs
测试：  tests/Modules.Members.Tests/Features/CreateMember/CreateMemberHandlerTests.cs
```

**命名约定**：
- 测试项目：`{模块名称}.Tests`
- 测试类：`{被测类名}Tests`
- 测试方法：`{方法名}_{场景}_{预期结果}`

**示例**：
```csharp
// 文件：tests/Modules.Members.Tests/Features/CreateMember/CreateMemberHandlerTests.cs
namespace Zss.BilliardHall.Tests.Modules.Members.Features.CreateMember;

public class CreateMemberHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesMember()
    {
        // Arrange
        var handler = new CreateMemberCommandHandler();
        var command = new CreateMemberCommand { Name = "John" };
        
        // Act
        var result = await handler.Handle(command);
        
        // Assert
        result.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task Handle_InvalidEmail_ThrowsException()
    {
        // 测试实现
    }
}
```

#### 3.2 测试项目组织
**决策**：每个业务模块对应一个独立的测试项目。

**✅ 允许**：
- `Modules.Members.Tests` - Members 模块的单元测试
- `Modules.Orders.Tests` - Orders 模块的单元测试
- 每个测试项目镜像对应模块的目录结构

**❌ 禁止**：
- 在源代码项目中混合测试代码
- 创建横向的测试分类项目（如 `UnitTests`、`IntegrationTests`）
- 单一的 `Tests` 项目包含所有模块测试

### 4. 集成测试规则

#### 4.1 独立项目组织
**决策**：集成测试放在独立的 `Integration.Tests` 项目中。

**组织方式**：
```
Integration.Tests/
├── Scenarios/                  # 按业务场景组织
│   ├── MemberRegistration/    # 会员注册场景
│   └── OrderProcessing/       # 订单处理场景
├── Fixtures/                   # 测试夹具
└── Integration.Tests.csproj
```

**特点**：
- **场景导向** - 按端到端业务场景组织，而非按模块
- **跨模块验证** - 可以测试多模块协作
- **独立运行** - 可能需要外部依赖（数据库、消息队列等）

---

## 与其他 ADR 关系（Related ADRs）

### 依赖关系
- **ADR-0001（模块化单体架构）** - 定义了模块边界，测试镜像模块结构
- **ADR-0000（架构测试）** - 定义了架构测试的必要性和目标

### 细化关系
本 ADR 细化了架构测试和单元测试的**目录组织方式**，是对宪法层约束的具体化。

---

## 快速参考表（Quick Reference）

### 测试项目命名速查

| 源代码位置 | 测试项目 | 测试类示例 |
|-----------|---------|-----------|
| `src/Modules/Members/` | `tests/Modules.Members.Tests/` | `CreateMemberHandlerTests.cs` |
| `src/Modules/Orders/` | `tests/Modules.Orders.Tests/` | `CreateOrderHandlerTests.cs` |
| `src/Platform/` | `tests/Platform.Tests/` | `ContractsTests.cs` |
| `src/Application/` | `tests/Application.Tests/` | `BootstrapperTests.cs` |
| 架构约束 | `tests/ArchitectureTests/` | `ADR_0001_Architecture_Tests.cs` |

### 测试类型与位置速查

| 测试类型 | 位置 | 组织方式 | 示例 |
|---------|------|---------|------|
| **架构测试** | `ArchitectureTests/ADR/` | 按 ADR 编号 | `ADR_0001_Architecture_Tests.cs` |
| **单元测试** | `Modules.{模块}.Tests/` | 镜像源码结构 | `Features/CreateMember/CreateMemberHandlerTests.cs` |
| **集成测试** | `Integration.Tests/` | 按业务场景 | `Scenarios/MemberRegistration/` |

### 命名约定速查

| 项目 | 约定 |
|------|------|
| **测试项目名** | `{命名空间}.Tests` |
| **测试类名** | `{被测类名}Tests` |
| **测试方法名** | `{方法名}_{场景}_{预期结果}` |
| **架构测试类名** | `ADR_{编号:D4}_Architecture_Tests` |
| **架构测试方法** | DisplayName = `"ADR-{编号}.{子编号}: {约束描述}"` |

---

## 示例与反模式

### ✅ 正确的组织方式

```
src/
├── Modules/Members/Features/CreateMember/CreateMemberHandler.cs
└── tests/
    ├── ArchitectureTests/
    │   └── ADR/ADR_0001_Architecture_Tests.cs
    └── Modules.Members.Tests/
        └── Features/CreateMember/CreateMemberHandlerTests.cs
```

### ❌ 错误的组织方式

```
// ❌ 错误1：不镜像源码结构
tests/Modules.Members.Tests/
└── UnitTests/
    └── AllMemberTests.cs  // 所有测试混在一起

// ❌ 错误2：架构测试分散
tests/Modules.Members.Tests/
└── ArchitectureTests/  // 各模块分散维护架构测试
    └── IsolationTests.cs

// ❌ 错误3：横向分类
tests/
├── UnitTests/  // 按测试类型分类，而非按模块
├── IntegrationTests/
└── ArchitectureTests/
```

---

## 架构测试保障

本 ADR 的约束由以下测试保障：

1. **测试项目命名检查** - 确保测试项目遵循 `{命名空间}.Tests` 约定
2. **目录镜像检查** - 确保单元测试镜像源码结构
3. **ADR 映射检查** - 确保每个 ADR 有对应测试类

> **注意**：本 ADR 是结构层 ADR，需要添加对应的架构测试实现。

---

## 常见问题（FAQ）

### Q: 为什么架构测试要集中管理？
**A:** 架构约束是全局性的，跨所有模块生效。集中管理：
- 避免在各模块重复维护相同的架构测试
- 便于统一更新和演进架构规则
- 单一职责：架构测试专注于验证架构约束

### Q: 单元测试为什么要镜像源码结构？
**A:** 镜像结构带来以下好处：
- **可发现性**：快速找到对应的测试文件
- **一致性**：团队成员形成统一的心智模型
- **可维护性**：源码重构时测试同步重构

### Q: 集成测试为什么按场景组织？
**A:** 集成测试关注**业务流程**，而非单个组件：
- 场景组织更贴近业务语言
- 便于端到端验证业务价值
- 避免与单元测试的目录结构重复

### Q: 如果一个测试类测试多个源码类怎么办？
**A:** 优先遵循"一个测试类对应一个源码类"原则。如果确实需要：
- 将测试类放在**主要被测类**对应的目录
- 使用清晰的命名说明测试范围
- 考虑是否应该拆分为多个测试类

---

## 参考文档

- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0000：架构测试与 CI 治理](../governance/ADR-0000-architecture-tests.md)
- [Testing Guide](/docs/TESTING-GUIDE.md)

---

## 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|---------|
| 1.0 | 2026-01-22 | 初始版本，定义测试目录组织规范 |
