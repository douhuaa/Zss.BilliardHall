# Testing Instructions

## Specific to: Writing and Maintaining Tests

When assisting with testing, apply these additional constraints on top of `base.instructions.md`.

## Test Organization

Tests MUST mirror the source structure:

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

## Architecture Tests (Critical)

### Location
All architecture tests are in: `src/tests/ArchitectureTests/ADR/`

### Structure
Each ADR has a corresponding test class:
- `ADR_0001_Architecture_Tests.cs` - Module isolation
- `ADR_0002_Architecture_Tests.cs` - Platform/Application/Host boundaries
- `ADR_0003_Architecture_Tests.cs` - Namespace rules
- `ADR_0004_Architecture_Tests.cs` - Package management
- `ADR_0005_Architecture_Tests.cs` - CQRS and Handler patterns

### Never Suggest

❌ **Do NOT suggest**:
- Modifying architecture tests to make code pass
- Commenting out failing architecture tests
- Adding exceptions/exclusions to architecture tests without strong justification

✅ **Instead, suggest**:
- Fix the code to comply with architecture
- Refer to the relevant ADR for correct pattern
- Consult `docs/copilot/adr-XXXX.prompts.md` for guidance

## Unit Tests

### Handler Tests
Test Handlers in isolation:

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

### Domain Model Tests
Test business logic in domain models:

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

## Integration Tests

Use actual dependencies but isolated database:

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

## Test Patterns to Follow

### ✅ Good Patterns
```csharp
// Clear test names describing behavior
[Fact]
public async Task Handle_InactiveMember_ThrowsException()

// Use FluentAssertions for readability
result.Should().NotBeNull();
result.Orders.Should().HaveCount(3);

// Test one thing per test
[Fact]
public async Task Handle_ValidInput_CreatesOrder() // One behavior

// Arrange-Act-Assert structure
var command = new CreateOrder(...); // Arrange
var result = await handler.Handle(command); // Act
result.Should().NotBeEmpty(); // Assert
```

### ❌ Bad Patterns
```csharp
// ❌ Vague test names
[Fact]
public async Task Test1()

// ❌ Multiple assertions unrelated to main behavior
[Fact]
public async Task Handle_Test()
{
    // Tests 5 different things
}

// ❌ Testing implementation details instead of behavior
[Fact]
public async Task Handle_CallsRepository() // Too implementation-focused
```

## When Tests Fail

### If Architecture Tests Fail
1. **DO NOT modify the test**
2. Copy the failure message
3. Suggest: "Please refer to `docs/copilot/architecture-test-failures.md` and paste this error to me for diagnosis"
4. Explain the ADR violation in plain language
5. Suggest correct implementation

### If Unit/Integration Tests Fail
1. Analyze the failure reason
2. Determine if it's a legitimate bug or test issue
3. Fix the bug, not the test (unless test is actually wrong)
4. Ensure all related tests pass

## Coverage Guidelines

Don't chase coverage percentage. Focus on:
- ✅ All business logic in domain models
- ✅ All Handler orchestration flows
- ✅ All edge cases and validations
- ✅ Critical integration paths

Can skip:
- ⏭️ Simple DTOs/Contracts (no logic)
- ⏭️ Trivial property getters/setters
- ⏭️ Infrastructure boilerplate

## Test Data Builders

Prefer builders for complex setup:

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

// Usage
var order = new OrderBuilder()
    .WithMember(memberId)
    .WithItem("product1", 2)
    .Build();
```

## Running Tests Locally

Suggest this workflow:
```bash
# Run all architecture tests
dotnet test src/tests/ArchitectureTests/

# Run specific ADR tests
dotnet test --filter "FullyQualifiedName~ADR_0001"

# Run module tests
dotnet test src/tests/Modules.Orders.Tests/

# Run all tests
dotnet test
```

## Reference

For architecture test failures:
- `docs/copilot/architecture-test-failures.md` - Diagnostic guide
- `docs/copilot/adr-XXXX.prompts.md` - Specific ADR guidance
