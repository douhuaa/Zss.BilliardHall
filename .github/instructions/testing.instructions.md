# 测试编写指令

> **⚠️ 权威声明**  
> 本文件所列规则仅作操作/辅导用，权威判据以 ADR 正文为准。  
> 若本文件与 ADR 正文存在分歧，请及时修订本文件，并以 ADR 正文为最终依据。

## 适用场景：编写和维护测试

在协助测试时，在 [`base.instructions.md`](./base.instructions.md) 的基础上应用这些额外约束。

---

## 🚨 高风险防御点（架构测试）

### 绝不允许的操作（Level 1 - 自动阻止）

架构测试是 ADR 的守护者，以下行为绝对禁止：

❌ **致命违规**：
- 修改架构测试以使代码通过
- 注释掉失败的架构测试
- 在没有充分理由的情况下为架构测试添加例外/排除
- 删除或弱化现有的架构约束检查

✅ **正确做法**：
- 修复代码以符合架构
- 参考相关 ADR 了解正确模式
- 查阅辅导材料获取实施指导

**执行级别参考**：[ADR-0005-Enforcement-Levels.md](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md)

---

## ⚖️ 权威依据

架构测试的唯一依据是 **ADR 正文**中标注【必须架构测试覆盖】的条款。

- 架构测试必须验证 ADR 正文中的约束
- 测试失败时，引用 ADR 正文的具体章节
- Prompt 文件可帮助理解，但测试逻辑基于 ADR 正文

**核心 ADR 参考**：
- [ADR-0001：模块隔离](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0002：层级边界](../../docs/adr/constitutional/ADR-0002-platform-application-host-bootstrap.md)
- [ADR-0005：Handler 规范](../../docs/adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [ADR-0005：执行级别](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md)

---

## 测试组织

**参考**：[ADR-0001：测试组织规范](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md#测试组织)

测试必须镜像源代码结构：

```
src/
  Modules/
    Orders/
      UseCases/
        CreateOrder/
          CreateOrderHandler.cs
tests/
  Modules.Orders.Tests/
    UseCases/
      CreateOrder/
        CreateOrderHandlerTests.cs
```

---

## 架构测试（关键）

**权威依据**：[ADR-0000：架构测试与 CI 治理](../../docs/adr/constitutional/ADR-0000-architecture-tests.md)

### 测试位置
所有架构测试位于：`src/tests/ArchitectureTests/ADR/`

### 测试结构
每个 ADR 都有对应的测试类：
- `ADR_0001_Architecture_Tests.cs` - [模块隔离](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- `ADR_0002_Architecture_Tests.cs` - [Platform/Application/Host 边界](../../docs/adr/constitutional/ADR-0002-platform-application-host-bootstrap.md)
- `ADR_0003_Architecture_Tests.cs` - [命名空间规则](../../docs/adr/constitutional/ADR-0003-namespace-rules.md)
- `ADR_0004_Architecture_Tests.cs` - [包管理](../../docs/adr/constitutional/ADR-0004-Cpm-Final.md)
- `ADR_0005_Architecture_Tests.cs` - [CQRS 和 Handler 模式](../../docs/adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md)

### 执行级别分类

**Level 1（静态可执行）**：
- 测试失败 = 绝对违规
- CI 自动阻断
- 必须修复代码而非测试

**Level 2（语义半自动）**：
- 启发式检查
- 需要人工审查确认
- 可能存在误报

**Level 3（人工 Gate）**：
- 需要架构委员会审批
- 记录在 ARCH-VIOLATIONS.md
- 需要充分理由和偿还计划

**详细分类**：[ADR-0005-Enforcement-Levels.md](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md)

### 绝不建议（Level 1 - 严格禁止）

❌ **不要建议**：
- 修改架构测试以使代码通过
- 注释掉失败的架构测试
- 在没有充分理由的情况下为架构测试添加例外/排除

✅ **应该建议**：
- 修复代码以符合架构
- 参考相关 ADR 以了解正确模式
- 查阅辅导材料获取指导：
  - [architecture-test-failures.md](../../docs/copilot/architecture-test-failures.md) - 诊断指南
  - [adr-XXXX.prompts.md](../../docs/copilot/) - 特定 ADR 指导

---

## 单元测试

**原则**：测试行为，而非实现

### Handler 测试

**参考**：[ADR-0005：Handler 测试规范](../../docs/adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md#handler-测试)

独立测试 Handler：

```csharp
public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesOrder()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var handler = new CreateOrderHandler(repository, eventBus);
        var command = new CreateOrder(memberId: Guid.NewGuid(), items: []);
        
        // Act
        var orderId = await handler.Handle(command);
        
        // Assert
        await repository.Received(1).SaveAsync(Arg.Any<Order>());
        orderId.Should().NotBeEmpty();
    }
}
```

### 领域模型测试

**参考**：[ADR-0001：领域模型规范](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md#领域模型)

测试领域模型中的业务逻辑：

```csharp
public class OrderTests
{
    [Fact]
    public void ApplyDiscount_ValidPercentage_AppliesDiscount()
    {
        // Arrange
        var order = new Order(memberId, items);
        
        // Act
        order.ApplyDiscount(10);
        
        // Assert
        order.Discount.Should().Be(10);
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DiscountApplied>();
    }
    
    [Fact]
    public void ApplyDiscount_NegativePercentage_ThrowsException()
    {
        // Arrange
        var order = new Order(memberId, items);
        
        // Act & Assert
        order.Invoking(o => o.ApplyDiscount(-10))
            .Should().Throw<InvalidDiscountException>();
    }
}
```

## 集成测试

使用实际依赖但隔离数据库：

```csharp
[Collection("Integration")]
public class CreateOrderIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    
    [Fact]
    public async Task CreateOrder_EndToEnd_Success()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new CreateOrderRequest { ... };
        
        // Act
        var response = await client.PostAsJsonAsync("/orders", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

---

## 要遵循的测试模式

### ✅ 好的模式
```csharp
// 描述行为的清晰测试名称
[Fact]
public async Task Handle_InactiveMember_ThrowsException()

// 使用 FluentAssertions 提高可读性
result.Should().NotBeNull();
result.Orders.Should().HaveCount(3);

// 每个测试测试一件事
[Fact]
public async Task Handle_ValidInput_CreatesOrder() // 一个行为

// Arrange-Act-Assert 结构
var command = new CreateOrder(...); // Arrange
var result = await handler.Handle(command); // Act
result.Should().NotBeEmpty(); // Assert
```

### ❌ 坏的模式
```csharp
// ❌ 模糊的测试名称
[Fact]
public async Task Test1()

// ❌ 与主要行为无关的多个断言
[Fact]
public async Task Handle_Test()
{
    // 测试 5 个不同的东西
}

// ❌ 测试实现细节而非行为
[Fact]
public async Task Handle_CallsRepository() // 过于关注实现
```

---

## 测试失败时的处理流程

### 如果架构测试失败（Level 1 - 自动阻止）

**流程**：
1. **不要修改测试** - 测试是 ADR 的守护者
2. **复制失败消息** - 完整的错误输出
3. **查阅诊断指南**：
   - [architecture-test-failures.md](../../docs/copilot/architecture-test-failures.md) - 错误诊断
   - 相关的 [adr-XXXX.prompts.md](../../docs/copilot/) - 场景指导
4. **理解 ADR 违规** - 用通俗语言解释违反了哪条规则
5. **建议正确实现** - 提供符合 ADR 的代码示例

**示例响应**：
```markdown
⚠️ **架构测试失败**：模块隔离违规（Level 1）

**检测到的问题**：
测试 `Modules_Should_Not_Reference_Other_Modules` 失败

**违反的 ADR**：
[ADR-0001：模块隔离](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md#模块通信)

**原因**：
Orders 模块直接引用了 Members 模块的领域类型

**修复方案**：
使用三种合规模式之一：
1. 领域事件（异步通信）
2. 契约查询（只读数据）
3. 原始类型（仅传递 ID）

**参考**：[adr-0001.prompts.md](../../docs/copilot/adr-0001.prompts.md)
```

### 如果单元/集成测试失败（Level 2/3）
1. 分析失败原因
2. 确定是合法的 bug 还是测试问题
3. 修复 bug，而非测试（除非测试确实有问题）
4. 确保所有相关测试通过

---

## 覆盖率指南

不要追求覆盖率百分比。专注于：
- ✅ 领域模型中的所有业务逻辑
- ✅ 所有 Handler 编排流程
- ✅ 所有边界情况和验证
- ✅ 关键的集成路径

可以跳过：
- ⏭️ 简单的 DTO/契约（无逻辑）
- ⏭️ 琐碎的属性 getter/setter
- ⏭️ 基础设施样板代码

---

## 测试数据构建器

对于复杂的设置，优先使用构建器：

```csharp
public class OrderBuilder
{
    private Guid _memberId = Guid.NewGuid();
    private List<OrderItem> _items = [];
    
    public OrderBuilder WithMember(Guid memberId)
    {
        _memberId = memberId;
        return this;
    }
    
    public OrderBuilder WithItem(string productId, int quantity)
    {
        _items.Add(new OrderItem(productId, quantity));
        return this;
    }
    
    public Order Build() => new Order(_memberId, _items);
}

// 使用
var order = new OrderBuilder()
    .WithMember(memberId)
    .WithItem("product1", 2)
    .Build();
```

---

## 本地运行测试

建议这个工作流程：
```bash
# 运行所有架构测试
dotnet test src/tests/ArchitectureTests/

# 运行特定 ADR 测试
dotnet test --filter "FullyQualifiedName~ADR_0001"

# 运行模块测试
dotnet test src/tests/Modules.Orders.Tests/

# 运行所有测试
dotnet test
```

---

## 参考

### 架构测试失败诊断
- [architecture-test-failures.md](../../docs/copilot/architecture-test-failures.md) - 诊断指南
- [adr-0001.prompts.md](../../docs/copilot/adr-0001.prompts.md) - 模块隔离场景
- [adr-0005.prompts.md](../../docs/copilot/adr-0005.prompts.md) - Handler/CQRS 场景

### ADR 正文参考
- [ADR-0000：架构测试与 CI 治理](../../docs/adr/constitutional/ADR-0000-architecture-tests.md)
- [ADR-0001：模块化单体架构](../../docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0005：应用内交互模型](../../docs/adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [ADR-0005：执行级别分类](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md)

---

## 维护提醒

> **🔄 重要**  
> 如本文件内容与 ADR 正文存在不一致，或架构演进导致规则变更，请：
> 1. 同步架构负责人确认变更
> 2. 更新本文件以与 ADR 正文保持一致
> 3. 进行团队公告，确保所有成员知晓变更
> 4. 同步更新架构测试，确保测试与 ADR 正文对齐
> 5. 更新相关的 [`docs/copilot/`](../../docs/copilot/) 辅导材料

---
