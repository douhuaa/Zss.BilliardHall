# Zss.BilliardHall - Comprehensive Development Plan

## 1. Project Overview

### 1.1 Project Vision
Zss.BilliardHall is a modern billiard hall management system built on a Modular Monolith architecture using .NET 10.0. The system aims to provide a complete operational management solution for billiard hall businesses.

### 1.2 Core Architectural Principles
- **Module Isolation**: Strict isolation between business modules to prevent tight coupling
- **Vertical Slices**: Each module contains its complete business logic, avoiding layered architecture
- **Message-Driven**: Use Wolverine framework for asynchronous message communication between modules
- **Platform Sharing**: Common capabilities are consolidated in the Platform layer for module consumption
- **Architectural Constraints**: Architecture rules are enforced through automated tests

### 1.3 Technology Stack
- **Runtime**: .NET 10.0
- **Web Framework**: ASP.NET Core
- **Messaging Framework**: Wolverine (In-process message bus + CQRS)
- **Data Storage**: Marten (PostgreSQL document database) / EF Core
- **Testing**: xUnit + NetArchTest
- **Build Tools**: MSBuild / .NET CLI

### 1.4 Why Wolverine?

Wolverine is a modern messaging framework designed specifically for modular monolith architectures with the following advantages:

**Core Benefits**:
- **Low Code Ceremony**: No need to define interfaces or register services, convention-based auto-discovery
- **In-Process Message Bus**: Modules communicate through messages, avoiding direct references
- **Native CQRS Support**: Clear separation of commands, events, and queries
- **Durable Messaging**: Inbox/Outbox Pattern ensures message reliability
- **Smooth Evolution Path**: Easy transition from monolith to distributed systems (RabbitMQ/Azure Service Bus)

**Alignment with This Project**:
- ✅ Module Isolation: Orders and Members communicate via events, no direct dependencies
- ✅ Vertical Slices: One command/event handler per feature, clear code organization
- ✅ Testability: Handlers are pure functions, easy to unit test
- ✅ High Performance: In-process communication, no network overhead
- ✅ Observability: Built-in diagnostics, tracing, and logging integration

---

## 2. Current Project Structure

```
Zss.BilliardHall/
├── src/
│   ├── Platform/           # Platform layer: shared infrastructure and capabilities
│   ├── Application/        # Application layer: cross-module orchestration
│   ├── Modules/           # Business module layer
│   │   ├── Members/       # Member management module
│   │   └── Orders/        # Order management module
│   ├── Host/              # Hosting layer
│   │   ├── WebHost/       # Web API host
│   │   └── Worker/        # Background worker host
│   └── tests/
│       └── ArchitectureTests/  # Architecture constraint tests
└── docs/                  # Documentation project
```

---

## 3. Module Development Plans

### 3.1 Members Module

#### 3.1.1 Core Features
- [ ] **Member Registration & Authentication**
  - Member information registration (name, phone, ID, etc.)
  - Member login authentication (phone + verification code / password)
  - Member profile management
  
- [ ] **Membership Card Management**
  - Membership card type definitions (Regular, Gold, Diamond, etc.)
  - Card issuance
  - Card recharge
  - Balance inquiry
  - Card freeze/unfreeze
  
- [ ] **Member Loyalty System**
  - Points rule configuration
  - Automatic tier upgrade/downgrade
  - Tier benefits configuration
  
- [ ] **Member Analytics**
  - Member consumption statistics
  - Member activity analysis
  - Churn prediction and alerts

#### 3.1.2 Technical Implementation (Wolverine Vertical Slice Architecture)
```csharp
// Wolverine modular architecture recommended structure
Modules/Members/
  ├── Features/              # Vertical slices by feature
  │   ├── Registration/      # Member registration
  │   │   ├── RegisterMember.cs        # Command
  │   │   ├── RegisterMemberHandler.cs # Command Handler
  │   │   └── MemberRegistered.cs      # Domain Event
  │   ├── Cards/            # Membership card management
  │   │   ├── IssueCard.cs
  │   │   ├── RechargeCard.cs
  │   │   └── CardHandlers.cs
  │   ├── Loyalty/          # Points and tiers
  │   │   ├── AddPoints.cs
  │   │   ├── UpgradeTier.cs
  │   │   └── LoyaltyHandlers.cs
  │   └── Analytics/        # Statistics and analysis
  │       └── MemberStatisticsQuery.cs
  ├── Events/               # Domain events (published across modules)
  │   ├── MemberRegistered.cs
  │   ├── CardRecharged.cs
  │   └── TierUpgraded.cs
  ├── Contracts/            # Public contracts (for other modules)
  │   └── IMemberQueries.cs
  └── Members.csproj

// Wolverine command handler example
public record RegisterMember(string Name, string Phone);

public static class RegisterMemberHandler
{
    // Wolverine auto-discovers and registers this handler
    public static async Task<MemberRegistered> Handle(
        RegisterMember command, 
        IDocumentSession session)
    {
        var member = new Member(command.Name, command.Phone);
        session.Store(member);
        await session.SaveChangesAsync();
        
        // Return event, Wolverine auto-publishes it
        return new MemberRegistered(member.Id, member.Name);
    }
}
```

#### 3.1.3 Data Model Design
- **Member** (Member entity)
  - Id, Name, Phone, IdCard, RegisteredAt, Status
- **MemberCard** (Membership card)
  - Id, MemberId, CardType, Balance, Status, IssuedAt
- **MemberLevel** (Member tier)
  - Id, Name, RequiredPoints, Benefits
- **LoyaltyPoints** (Points record)
  - Id, MemberId, Points, Source, CreatedAt

---

### 3.2 Orders Module

#### 3.2.1 Core Features
- [ ] **Table Management**
  - Table information maintenance (number, type, status)
  - Real-time table status monitoring
  - Table reservation management
  
- [ ] **Pricing Rules**
  - Pricing method configuration (hourly, per game, time packages)
  - Price strategy configuration (member discounts, time-based pricing)
  - Overtime fee calculation
  
- [ ] **Session Management & Checkout**
  - Session start (table, member, start time)
  - Time tracking and billing
  - Product consumption (drinks, snacks, etc.)
  - Checkout (cash, membership card, mobile payment)
  
- [ ] **Order Query & Statistics**
  - Order history query
  - Revenue statistics
  - Peak hour analysis

#### 3.2.2 Technical Implementation (Wolverine Vertical Slice Architecture)
```csharp
Modules/Orders/
  ├── Features/
  │   ├── Tables/           # Table management
  │   │   ├── ReserveTable.cs
  │   │   └── TableHandlers.cs
  │   ├── Pricing/          # Pricing rules
  │   │   ├── ConfigurePricing.cs
  │   │   └── PricingHandlers.cs
  │   ├── Sessions/         # Session & checkout
  │   │   ├── StartSession.cs          # Command
  │   │   ├── EndSession.cs            # Command
  │   │   ├── SessionHandlers.cs       # Handlers
  │   │   └── SessionStarted.cs        # Event
  │   └── Reporting/        # Reports and statistics
  │       └── OrderStatisticsQuery.cs
  ├── Events/               # Domain events
  │   ├── SessionStarted.cs
  │   ├── SessionEnded.cs
  │   └── PaymentCompleted.cs
  ├── Contracts/
  └── Orders.csproj

// Cross-module communication example: Update member points after payment
// Orders module publishes event
public record PaymentCompleted(Guid MemberId, decimal Amount);

// Members module subscribes to event (Wolverine auto-routes)
public static class PaymentCompletedHandler
{
    public static AddPoints Handle(PaymentCompleted evt)
    {
        // Earn 1 point per dollar spent
        var points = (int)evt.Amount;
        return new AddPoints(evt.MemberId, points, "Purchase Points");
    }
}
```

#### 3.2.3 Data Model Design
- **Table** (Billiard table)
  - Id, Number, Type, Status, HourlyRate
- **PricingRule** (Pricing rule)
  - Id, Name, Type, BaseRate, MemberDiscount
- **Session** (Playing session)
  - Id, TableId, MemberId, StartTime, EndTime, Status
- **Order** (Order)
  - Id, SessionId, TotalAmount, PaymentMethod, PaidAt

---

### 3.3 Future Module Plans

#### 3.3.1 Products Module
- Product information management (drinks, snacks, billiard supplies)
- Inventory management
- Purchase and stock-in
- Stock alerts

#### 3.3.2 Staff Module
- Staff information management
- Shift scheduling
- Permission management
- Attendance tracking

#### 3.3.3 Marketing Module
- Coupon management
- Campaign management
- Member referrals
- SMS notifications

---

## 4. Platform Layer Development Plan

### 4.1 Infrastructure Capabilities

- [ ] **Wolverine Messaging Framework Integration**
  - In-process message bus configuration
  - Command/event routing rules
  - Message handling pipeline (middleware)
  - Message persistence (Inbox/Outbox Pattern)
  - Delayed messages and retry strategies
  - Future support for external message queues (RabbitMQ/Azure Service Bus)

- [ ] **Data Access (Recommended: Marten + PostgreSQL)**
  - Marten as document database + event store
  - Entity Framework Core (optional, for relational scenarios)
  - Unit of Work
  - Database migration management

- [ ] **Domain Events (via Wolverine)**
  - Use Wolverine's event publish/subscribe
  - Module decoupling through asynchronous messages
  - Durable messaging
  - Event versioning

- [ ] **Authentication & Authorization**
  - JWT Token generation and validation
  - Role-Based Access Control (RBAC)
  - Resource permission control

- [ ] **Logging & Monitoring**
  - Structured logging (Serilog)
  - Wolverine diagnostics and tracing
  - Performance metrics collection
  - Exception tracking

- [ ] **Caching**
  - In-memory cache (IMemoryCache)
  - Distributed cache (Redis)
  - Cache strategy encapsulation

- [ ] **Validation & Error Handling**
  - FluentValidation integration
  - Wolverine middleware for validation
  - Unified exception handling
  - Standardized error responses

### 4.2 Technical Structure
```csharp
Platform/
  ├── Messaging/            # Wolverine configuration & extensions
  │   ├── WolverineConfiguration.cs
  │   ├── MessageHandlerConventions.cs
  │   └── OutboxConfiguration.cs
  ├── Data/                 # Data access (Marten/EF Core)
  │   ├── MartenConfiguration.cs
  │   └── Repositories/
  ├── Events/               # Event-related infrastructure
  │   ├── EventMetadata.cs
  │   └── EventSerializer.cs
  ├── Auth/                 # Authentication & authorization
  ├── Logging/              # Logging
  ├── Caching/              # Caching
  ├── Validation/           # Validation
  └── Platform.csproj

// Wolverine configuration example
builder.Host.UseWolverine(opts =>
{
    // Configure message routing
    opts.PublishAllMessages()
        .ToLocalQueue("events")
        .UseDurableInbox();
    
    // Auto-discover handlers from all modules
    opts.Discovery.IncludeAssembly(typeof(Members.Module).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Orders.Module).Assembly);
    
    // Persistence configuration (Marten)
    opts.UseMarten(connectionString);
});
```
  └── Platform.csproj
```

---

## 5. Application Layer Development Plan

### 5.1 Core Responsibilities
- Module initialization and orchestration
- Cross-module workflow coordination
- Global configuration management

### 5.2 Implementation Items
- [ ] **Module Registration**
  - Auto-discover and register modules
  - Dependency injection configuration

- [ ] **Global Middleware**
  - Request logging
  - Exception handling
  - Performance monitoring

- [ ] **Configuration Management**
  - appsettings.json management
  - Environment variable configuration
  - Hot reload configuration

---

## 6. Host Layer Development Plan

### 6.1 WebHost Development
- [ ] **API Design**
  - RESTful API standards
  - Swagger / OpenAPI documentation
  - API versioning

- [ ] **Middleware Configuration**
  - CORS
  - Compression
  - Rate limiting

- [ ] **Health Checks**
  - Database connection check
  - Dependent service check
  - Readiness probe

### 6.2 Worker Development
- [ ] **Background Tasks**
  - Automatic member tier upgrade
  - Statistics report generation
  - Data cleanup and archival

- [ ] **Scheduled Tasks**
  - Quartz.NET integration
  - Task scheduling management

---

## 7. Testing Strategy

### 7.1 Architecture Tests (ArchitectureTests)
- [x] Module isolation tests
- [x] Namespace constraints
- [x] Dependency boundary checks
- [ ] Enhanced test coverage

### 7.2 Unit Tests
- [ ] Members module unit tests
- [ ] Orders module unit tests
- [ ] Platform utility tests

### 7.3 Integration Tests
- [ ] API end-to-end tests
- [ ] Database integration tests
- [ ] Cross-module integration tests

### 7.4 Performance Tests
- [ ] High concurrency scenario tests
- [ ] Stress testing
- [ ] Performance bottleneck analysis

---

## 8. Database Design

### 8.1 Database Selection
- **Primary DB**: PostgreSQL / SQL Server
- **Cache**: Redis
- **Message Queue**: RabbitMQ / Azure Service Bus (optional)

### 8.2 Database Architecture
- **Separate Schema per Module**: Logical isolation
  - `members.*` - Member module tables
  - `orders.*` - Orders module tables
  - `platform.*` - Platform shared tables

### 8.3 Migration Strategy
- Use EF Core Migrations
- Version management
- Automated deployment scripts

---

## 9. Development Phases & Milestones

### Phase 1: Infrastructure Setup (2-3 weeks)
- [x] Project structure setup
- [x] Architecture constraint tests
- [ ] Platform layer core capabilities
  - Data access encapsulation
  - Domain event infrastructure
  - Logging and exception handling
- [ ] WebHost basic configuration
- [ ] Database initialization

**Deliverable**: Runnable Web API framework with health checks

### Phase 2: Members Module Development (3-4 weeks)
- [ ] Member registration and authentication
- [ ] Membership card management
- [ ] Member loyalty system
- [ ] Unit and integration tests
- [ ] API documentation

**Deliverable**: Complete member management features with API and tests

### Phase 3: Orders Module Development (4-5 weeks)
- [ ] Table management
- [ ] Pricing rules engine
- [ ] Session start and checkout workflow
- [ ] Order query and reporting
- [ ] Unit and integration tests

**Deliverable**: Complete order management features supporting session billing and checkout

### Phase 4: Module Integration & Optimization (2-3 weeks)
- [ ] Member and order integration (member consumption points)
- [ ] Cross-module event handling
- [ ] Performance optimization
- [ ] Stress testing
- [ ] Monitoring and alerting configuration

**Deliverable**: Integrated system with performance benchmarks met

### Phase 5: Extended Features (As Needed)
- [ ] Products module
- [ ] Staff module
- [ ] Marketing module
- [ ] Mobile API
- [ ] Admin dashboard

---

## 10. Technical Debt & Optimization

### 10.1 Current TODO
- [ ] Complete Platform layer implementation
- [ ] Add Members and Orders module code
- [ ] Add database migrations
- [ ] Improve test coverage

### 10.2 Architecture Optimization Directions
- [ ] Introduce CQRS pattern (Command Query Responsibility Segregation)
- [ ] Implement read-write separation
- [ ] Introduce distributed caching
- [ ] Implement async message processing
- [ ] Containerized deployment (Docker)

### 10.3 Engineering Practices
- [ ] CI/CD pipeline configuration
- [ ] Code quality gates (SonarQube)
- [ ] Automated deployment scripts
- [ ] Monitoring and alerting system
- [ ] Automated documentation generation

---

## 11. Team Collaboration Standards

### 11.1 Branching Strategy
- `main` - Main branch, production-ready code
- `develop` - Development branch
- `feature/*` - Feature branches
- `bugfix/*` - Bug fix branches

### 11.2 Commit Convention
Use Conventional Commits:
- `feat:` - New feature
- `fix:` - Bug fix
- `refactor:` - Refactoring
- `test:` - Testing
- `docs:` - Documentation

### 11.3 Code Review
- All code must go through PR review
- At least one team member approval
- CI tests must pass
- Architecture constraint tests must pass

---

## 12. Risks & Challenges

### 12.1 Technical Risks
- **.NET 10.0 Stability**: New version may have unknown issues
  - Mitigation: Monitor official releases, update promptly
  
- **Module Boundary Definition**: Initial design may require adjustment
  - Mitigation: Enforce through architecture tests, regular reviews

### 12.2 Business Risks
- **Frequent Requirement Changes**: Billiard hall business model may evolve
  - Mitigation: Modular architecture enables localized adjustments
  
- **Performance Requirements**: High concurrency during peak hours
  - Mitigation: Early performance testing, reserve optimization capacity

---

## 13. References

### 13.1 Architecture Design
- [Modular Monolith: A Primer](https://www.kamilgrzybek.com/design/modular-monolith-primer/)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Vertical Slice Architecture](https://jimmybogard.com/vertical-slice-architecture/)
- [Wolverine Modular Monolith Tutorial](https://wolverinefx.net/tutorials/modular-monolith.html)
- [Wolverine 3.6 Modular Architecture Features](https://jeremydmiller.com/2025/01/12/wolverine-3-6-modular-monolith-and-vertical-slice-architecture-goodies/)

### 13.2 Technical Documentation
- [.NET 10.0 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Wolverine Official Documentation](https://wolverinefx.net/)
- [Marten Document Database](https://martendb.io/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)

### 13.3 Testing
- [xUnit Documentation](https://xunit.net/)
- [NetArchTest](https://github.com/BenMorris/NetArchTest)

---

## Appendix: Quick Start Guide

### Build the Project
```bash
# Clone repository
git clone https://github.com/douhuaa/Zss.BilliardHall.git
cd Zss.BilliardHall

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run architecture tests
dotnet test ./src/tests/ArchitectureTests/ArchitectureTests.csproj
```

### Run Web API
```bash
cd src/Host/WebHost
dotnet run
```

### Run Worker
```bash
cd src/Host/Worker
dotnet run
```

---

**Document Version**: v1.0  
**Last Updated**: 2026-01-18  
**Maintainer**: Development Team
