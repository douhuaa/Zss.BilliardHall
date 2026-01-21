# Backend Development Instructions

## Specific to: Backend/Business Logic Development

When assisting with backend development, apply these additional constraints on top of `base.instructions.md`.

## Vertical Slice Organization

Every business use case MUST be organized as a complete vertical slice:

```
UseCases/
  CreateOrder/
    CreateOrder.cs              ← Command/Query
    CreateOrderHandler.cs        ← Handler (THE authority for this use case)
    CreateOrderEndpoint.cs       ← Optional: HTTP adapter
    CreateOrderTests.cs          ← Tests
```

**Never suggest**:
- ❌ Horizontal Service layers (e.g., `OrderService`)
- ❌ Shared business logic across use cases
- ❌ Generic `Manager` or `Helper` classes with business logic

## Handler Rules (ADR-0005)

### Command Handlers
- MUST return `void` or ID only (Guid, int, string)
- MUST NOT return business data (use separate Query for that)
- MUST NOT depend on Contracts (DTOs) for business decisions
- MUST load domain models, execute business logic, save state
- CAN publish domain events

**Correct Command Handler**:
```csharp
public class CreateOrderHandler : ICommandHandler<CreateOrder>
{
    public async Task<Guid> Handle(CreateOrder command)
    {
        // ✅ Load/create aggregate
        var order = new Order(command.MemberId, command.Items);
        
        // ✅ Execute business logic (in domain model)
        order.Calculate();
        
        // ✅ Save
        await _repository.SaveAsync(order);
        
        // ✅ Publish event (optional)
        await _eventBus.Publish(new OrderCreated(order.Id));
        
        return order.Id;
    }
}
```

**Incorrect patterns to BLOCK**:
```csharp
// ❌ Command Handler returning business data
public async Task<OrderDto> Handle(CreateOrder command) { ... }

// ❌ Command Handler depending on Contracts
var memberDto = await _queryBus.Send(new GetMemberById(...));
if (memberDto.Balance > 1000) { ... } // ❌ Business decision based on DTO
```

### Query Handlers
- MUST return Contracts (DTOs)
- MUST NOT modify state
- MUST NOT publish events
- CAN optimize for read performance
- CAN query across module boundaries (via Contracts)

## Endpoint Rules

Endpoints MUST be thin adapters:

```csharp
public class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/orders", async (
            CreateOrderRequest request, 
            IMessageBus bus) =>
        {
            // ✅ Map to Command
            var command = new CreateOrder(request.MemberId, request.Items);
            
            // ✅ Delegate to Handler
            var orderId = await bus.InvokeAsync(command);
            
            // ✅ Return HTTP response
            return Results.Created($"/orders/{orderId}", orderId);
        });
    }
}
```

**Never allow in Endpoints**:
- ❌ Business logic or validation
- ❌ Direct database access
- ❌ Direct domain model manipulation

## Module Communication

When one module needs data/notification from another:

### ✅ DO: Use Domain Events (Async)
```csharp
// In Orders module
await _eventBus.Publish(new OrderCreated(orderId, memberId));

// In Members module (subscriber)
public class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    public async Task Handle(OrderCreated @event)
    {
        // Update member statistics
    }
}
```

### ✅ DO: Use Contracts for Queries
```csharp
// Query another module's data
var memberDto = await _queryBus.Send(new GetMemberById(memberId));
// Use memberDto.Name, memberDto.Email, etc. (read-only)
```

### ✅ DO: Use Primitive Types
```csharp
// Just pass the ID
var orderId = Guid.NewGuid();
var command = new NotifyMember(memberId); // Guid, not Member object
```

### ❌ DON'T: Direct References
```csharp
// ❌ NEVER reference other module's internals
using Zss.BilliardHall.Modules.Members.Domain;
var member = await _memberRepository.GetByIdAsync(id);
```

### ❌ DON'T: Synchronous Cross-Module Commands
```csharp
// ❌ Don't call another module's Command synchronously
await _commandBus.Send(new UpdateMemberStatistics(memberId));
```

## Domain Model Guidelines

Place business logic in domain models, not in Handlers or Services:

```csharp
// ✅ Correct: Business logic in domain model
public class Order
{
    public void ApplyDiscount(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new InvalidDiscountException();
        
        _discount = percentage;
        AddDomainEvent(new DiscountApplied(Id, percentage));
    }
}

// Handler just orchestrates
public class ApplyDiscountHandler
{
    public async Task Handle(ApplyDiscount command)
    {
        var order = await _repository.GetByIdAsync(command.OrderId);
        order.ApplyDiscount(command.Percentage); // ✅ Logic in domain model
        await _repository.SaveAsync(order);
    }
}
```

## What to Suggest When

| Developer says... | Suggest checking... |
|-------------------|---------------------|
| "I need to call another module's logic" | ADR-0001 (use events), `docs/copilot/adr-0001.prompts.md` |
| "I need to share code between modules" | Is it technical (→ BuildingBlocks) or business (→ rethink design)? |
| "I need to return data from a Command" | ADR-0005 (Commands return ID, use separate Query) |
| "I need to validate using another module's data" | Query via Contract (read-only), don't use for business decisions |

## Quick Red Flags

Stop and warn if you see:
- 🚩 `using Zss.BilliardHall.Modules.X` in another module
- 🚩 `class OrderService` or any `*Service` in modules
- 🚩 Command Handler returning DTOs
- 🚩 Query Handler modifying state
- 🚩 Business logic in Endpoints
- 🚩 Shared domain models between modules

## Reference

For detailed scenarios and examples:
- `docs/copilot/adr-0001.prompts.md` - Module isolation
- `docs/copilot/adr-0005.prompts.md` - Handler patterns and CQRS
