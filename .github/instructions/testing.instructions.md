# 测试编写指令

## 适用场景：编写和维护测试

在协助测试时，在 `base.instructions.md` 的基础上应用这些额外约束。

## 权威依据

本文档服从以下 ADR：
- ADR-0000：架构测试与 CI 治理宪法
- ADR-0001：模块化单体与垂直切片架构
- ADR-0005：应用内交互模型与执行边界

**冲突裁决**：若本文档与 ADR 正文冲突，以 ADR 正文为准。

## ⚖️ 权威提醒

架构测试的唯一依据是 **ADR 正文**中标注【必须架构测试覆盖】的条款。

- 架构测试必须验证 ADR 正文中的约束
- 测试失败时，引用 ADR 正文的具体章节
- Prompt 文件可帮助理解，但测试逻辑基于 ADR 正文

## 测试组织

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

## 架构测试（关键）

### 位置

所有架构测试位于：`src/tests/ArchitectureTests/ADR/`

### 结构

每个 ADR 都有对应的测试类：

- `ADR_0001_Architecture_Tests.cs` - 模块隔离
- `ADR_0002_Architecture_Tests.cs` - Platform/Application/Host 边界
- `ADR_0003_Architecture_Tests.cs` - 命名空间规则
- `ADR_0004_Architecture_Tests.cs` - 包管理
- `ADR_0005_Architecture_Tests.cs` - CQRS 和 Handler 模式

### 绝不建议

❌ **不要建议**：

- 修改架构测试以使代码通过
- 注释掉失败的架构测试
- 在没有充分理由的情况下为架构测试添加例外/排除

✅ **应该建议**：

- 修复代码以符合架构
- 参考相关 ADR 以了解正确模式
- 查阅 `docs/copilot/adr-XXXX.prompts.md` 获取指导

## 单元测试

### Handler 测试

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

## 测试失败时

### 如果架构测试失败

1. **不要修改测试**
2. 复制失败消息
3. 建议："请参考 `docs/copilot/architecture-test-failures.md` 并将此错误粘贴给我进行诊断"
4. 用通俗语言解释 ADR 违规
5. 建议正确实现

### 如果单元/集成测试失败

1. 分析失败原因
2. 确定是合法的 bug 还是测试问题
3. 修复 bug，而非测试（除非测试确实有问题）
4. 确保所有相关测试通过

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

## 参考

对于架构测试失败：

- `docs/copilot/architecture-test-failures.md` - 诊断指南
- `docs/copilot/adr-XXXX.prompts.md` - 特定 ADR 指导
