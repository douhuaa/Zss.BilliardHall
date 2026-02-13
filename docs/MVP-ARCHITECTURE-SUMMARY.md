# Zss.BilliardHall MVP 架构总结

> **文档版本**: 1.0  
> **创建日期**: 2026-02-13  
> **基于**: PR #404 最终版本

本文档提供 Zss.BilliardHall MVP（最小可行产品）的架构概述，帮助快速理解系统的核心设计和实现。

---

## 📋 目录

1. [架构概览](#架构概览)
2. [核心设计原则](#核心设计原则)
3. [技术栈](#技术栈)
4. [目录结构](#目录结构)
5. [模块化设计](#模块化设计)
6. [启动流程](#启动流程)
7. [关键组件](#关键组件)
8. [开发指南](#开发指南)
9. [相关文档](#相关文档)

---

## 架构概览

Zss.BilliardHall MVP 采用**模块化单体（Modular Monolith）**架构，结合**垂直切片（Vertical Slice）**模式，基于 **Wolverine + Marten** 技术栈实现 CQRS/Event Sourcing。

### 架构分层

```
┌─────────────────────────────────────────┐
│          Host Layer (进程外壳)            │
│  ┌──────────────┐  ┌─────────────────┐  │
│  │   Web Host   │  │  Worker Host    │  │
│  │  (ASP.NET)   │  │ (BackgroundSvc) │  │
│  └──────────────┘  └─────────────────┘  │
└─────────────────────────────────────────┘
               ↓                ↓
┌─────────────────────────────────────────┐
│      Application Layer (应用装配)         │
│  • Wolverine + Marten 集成               │
│  • 模块发现与加载                         │
│  • 事务管理、消息路由                     │
└─────────────────────────────────────────┘
               ↓                ↓
┌─────────────────────────────────────────┐
│       Platform Layer (技术基座)          │
│  • Serilog 日志                         │
│  • OpenTelemetry 可观测性               │
│  • 异常体系、通用接口                    │
└─────────────────────────────────────────┘
               ↓                ↓
┌─────────────────────────────────────────┐
│       Modules Layer (业务模块)           │
│  ┌──────────┐  ┌──────────┐            │
│  │ Members  │  │  Orders  │  ...       │
│  └──────────┘  └──────────┘            │
└─────────────────────────────────────────┘
```

---

## 核心设计原则

### 1. 模块隔离（ADR-001）
- 模块间禁止直接引用
- 通过领域事件通信
- 每个模块独立可部署（未来支持）

### 2. 三层启动体系（ADR-002）
```csharp
// Host 层决定"怎么跑"
var builder = WebApplication.CreateBuilder(args);

// 1. Platform 配置技术基座
PlatformBootstrapper.Configure(...);

// 2. Host 提供模块清单
var moduleAssemblies = ModuleRegistry.GetEnabledAssemblies();

// 3. Application 装配业务能力
ApplicationBootstrapper.Configure(..., moduleAssemblies);

var app = builder.Build();
app.Run();
```

### 3. 垂直切片（ADR-001）
每个用例（Feature）包含完整的端到端实现：
- Command/Query
- Handler
- Endpoint
- DTO/Entity

**禁止横向分层**：不允许创建全局的 Services、Repositories 层。

### 4. 约定优于配置
- Wolverine 自动发现 Handlers 和 Endpoints
- Marten 自动创建表结构
- 模块通过 `IModule` 接口自注册

---

## 技术栈

### 运行时
- **.NET 10**: 应用程序框架
- **ASP.NET Core**: Web 框架
- **C# 13**: 编程语言

### 核心库
- **Wolverine 5.x**: CQRS/消息总线/中介者模式
- **Marten 8.x**: PostgreSQL 文档数据库 + Event Store
- **Serilog**: 结构化日志
- **OpenTelemetry**: 分布式追踪和指标

### 数据库
- **PostgreSQL 15+**: 主数据库

### 测试
- **xUnit**: 单元/集成测试
- **NetArchTest**: 架构测试

---

## 目录结构

```
Zss.BilliardHall/
├── src/
│   ├── Platform/                    # 技术基座
│   │   ├── Contracts/              # 通用接口
│   │   │   ├── IModule.cs          # 模块契约
│   │   │   ├── IMartenModule.cs    # Marten 配置契约
│   │   │   └── ICommandHandler.cs  # 命令处理器接口
│   │   ├── Exceptions/             # 异常体系
│   │   ├── ModuleLoader.cs         # 模块加载器
│   │   └── PlatformBootstrapper.cs # 平台启动器
│   │
│   ├── Application/                 # 应用装配
│   │   └── ApplicationBootstrapper.cs
│   │
│   ├── Host/
│   │   ├── Web/                    # Web Host
│   │   │   ├── HostBootstrapper.cs # 统一启动器
│   │   │   ├── ModuleRegistry.cs   # 模块注册表
│   │   │   ├── Program.cs          # 极简入口
│   │   │   └── appsettings.json    # 配置
│   │   └── Worker/                 # Worker Host
│   │       ├── HostBootstrapper.cs
│   │       ├── ModuleRegistry.cs
│   │       └── Program.cs
│   │
│   └── Modules/                     # 业务模块
│       ├── Members/                # 会员模块
│       │   ├── Features/
│       │   │   ├── CreateMember/  # 用例：创建会员
│       │   │   │   ├── CreateMemberCommand.cs
│       │   │   │   ├── CreateMemberCommandHandler.cs
│       │   │   │   ├── CreateMemberEndpoint.cs
│       │   │   │   └── CreateMemberResponse.cs
│       │   │   └── GetMemberById/ # 用例：查询会员
│       │   │       ├── GetMemberByIdQuery.cs
│       │   │       ├── GetMemberByIdQueryHandler.cs
│       │   │       ├── GetMemberByIdEndpoint.cs
│       │   │       └── MemberDto.cs
│       │   ├── Member.cs           # 实体
│       │   ├── MemberModule.cs     # 模块启动器
│       │   └── GlobalUsings.cs     # 全局引用
│       └── Orders/                 # 订单模块
│           └── OrderModule.cs
│
├── docs/
│   ├── MVP-RUNBOOK.md              # MVP 运行指南
│   ├── MVP-ARCHITECTURE-SUMMARY.md # 本文档
│   └── adr/                        # 架构决策记录
│
└── src/tests/
    └── ArchitectureTests/          # 架构测试
```

---

## 模块化设计

### 模块结构

每个业务模块遵循相同的结构：

```
ModuleName/
├── Features/            # 用例集合
│   └── FeatureName/    # 单个用例
│       ├── Command/Query.cs
│       ├── Handler.cs
│       ├── Endpoint.cs
│       └── DTO.cs
├── Entities/           # 领域实体（可选）
├── ModuleNameModule.cs # 模块启动器（实现 IModule）
└── GlobalUsings.cs     # 全局引用
```

### 模块注册机制

1. **定义模块**：实现 `IModule` 接口
```csharp
public class MemberModule : IModule, IMartenModule
{
    public void ConfigureServices(IServiceCollection services, 
        IConfiguration configuration, IHostEnvironment environment)
    {
        // 注册模块特定服务
    }

    public void ConfigureMarten(StoreOptions options)
    {
        // 配置 Marten 文档映射
        options.Schema.For<Member>().UniqueIndex(x => x.Email);
    }
}
```

2. **在 Host 中注册**：
```csharp
// Host/Web/ModuleRegistry.cs
private static readonly Assembly[] AllModuleAssemblies =
[
    typeof(MemberModule).Assembly,
    typeof(OrderModule).Assembly
];
```

3. **自动加载**：ApplicationBootstrapper 会自动发现并调用模块的 `ConfigureServices` 方法。

---

## 启动流程

### Web Host 启动流程

```
1. Program.cs 创建 WebApplicationBuilder
   ↓
2. HostBootstrapper.ConfigureServices(builder)
   ↓
3. PlatformBootstrapper.Configure(...)
   - 配置 Serilog
   - 配置 OpenTelemetry
   - 注册异常体系
   ↓
4. ModuleRegistry.GetEnabledAssemblies(...)
   - 返回启用的模块程序集
   ↓
5. ApplicationBootstrapper.Configure(..., moduleAssemblies)
   - 配置 Marten（连接数据库）
   - 配置 Wolverine（消息总线）
   - 加载模块（调用 IModule.ConfigureServices）
   ↓
6. HostBootstrapper.ConfigureApplication(app)
   - 映射 Wolverine HTTP 端点
   ↓
7. app.Run() 启动应用
```

---

## 关键组件

### 1. IModule 接口
```csharp
public interface IModule
{
    void ConfigureServices(IServiceCollection services, 
        IConfiguration configuration, IHostEnvironment environment);
}
```
每个业务模块必须实现此接口，用于自注册服务。

### 2. ModuleLoader
```csharp
public static class ModuleLoader
{
    public static void LoadModules(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        params Assembly[] moduleAssemblies)
    {
        // 通过反射发现并调用 IModule 实现
    }
}
```

### 3. ApplicationBootstrapper
```csharp
public static class ApplicationBootstrapper
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        Assembly[] moduleAssemblies)
    {
        // 配置 Marten + Wolverine
        // 加载模块
    }
}
```

### 4. Wolverine Handler 自动发现
```csharp
// Command
public sealed record CreateMemberCommand(string Name, string Email);

// Handler（Wolverine 自动发现）
public class CreateMemberCommandHandler(IDocumentSession session) 
    : ICommandHandler<CreateMemberCommand, Guid>
{
    public Task<Guid> Handle(CreateMemberCommand command)
    {
        var member = new Member { ... };
        session.Store(member);
        // Marten 事务自动提交（IntegrateWithWolverine）
        return Task.FromResult(member.Id);
    }
}
```

### 5. Wolverine Endpoint 自动发现
```csharp
public static class CreateMemberEndpoint
{
    [WolverinePost("/api/members")]
    public static Task<Guid> Create(
        CreateMemberCommand command, 
        IMessageBus bus, 
        CancellationToken ct = default) 
        => bus.InvokeAsync<Guid>(command, ct);
}
```

---

## 开发指南

### 添加新用例

1. **创建用例目录**：`Modules/ModuleName/Features/FeatureName/`
2. **定义 Command/Query**：
   ```csharp
   public sealed record MyCommand(string Data);
   ```
3. **实现 Handler**：
   ```csharp
   public class MyCommandHandler(IDocumentSession session) 
       : ICommandHandler<MyCommand, MyResult>
   {
       public Task<MyResult> Handle(MyCommand command) { ... }
   }
   ```
4. **创建 Endpoint**：
   ```csharp
   public static class MyEndpoint
   {
       [WolverinePost("/api/my-endpoint")]
       public static Task<MyResult> Execute(MyCommand command, IMessageBus bus)
           => bus.InvokeAsync<MyResult>(command);
   }
   ```

### 添加新模块

1. 在 `src/Modules/` 下创建模块目录
2. 创建 `XxxModule.cs` 实现 `IModule`
3. 在 `Host/Web/ModuleRegistry.cs` 和 `Host/Worker/ModuleRegistry.cs` 中注册
4. 按垂直切片组织用例

### 运行与测试

详见 [MVP-RUNBOOK.md](./MVP-RUNBOOK.md)

---

## 相关文档

### 架构决策记录（ADR）
- [ADR-001: 模块化单体与垂直切片架构](./adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)
- [ADR-002: Platform / Application / Host 三层启动体系](./adr/constitutional/ADR-002-platform-application-host-bootstrap.md)

### 运行指南
- [MVP-RUNBOOK.md](./MVP-RUNBOOK.md) - MVP 运行指南

### 模块文档
- [Members 模块 README](../src/Modules/Members/README.md)
- [Orders 模块 README](../src/Modules/Orders/README.md)

### 测试文档
- [架构测试说明](../src/tests/ArchitectureTests/README.md)

---

## 后续计划

MVP 已实现以下目标：
- ✅ 三层启动体系运行正常
- ✅ 模块化加载机制完成
- ✅ Members 模块示例实现
- ✅ 垂直切片架构验证
- ✅ 架构测试覆盖核心约束

待优化项：
- 🔄 动态模块加载（热插拔）
- 🔄 模块间事件通信示例
- 🔄 集成测试补充
- 🔄 CI/CD 流水线优化

---

**本文档基于 PR #404 最终版本整理，旨在快速理解 MVP 架构设计。**
