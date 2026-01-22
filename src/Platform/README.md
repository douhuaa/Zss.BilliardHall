# Platform 层（技术基座）

## 概述

Platform 层是整个系统的**技术基座**，提供不感知业务的通用技术能力。

根据 [ADR-0002](../../docs/adr/constitutional/ADR-0002-platform-application-host-bootstrap.md)，Platform 层：
- ✅ 不依赖 Application 或 Modules
- ✅ 不感知业务逻辑或业务概念
- ✅ 只提供可复用的技术能力

## 目录结构

```
Platform/
├── Contracts/          # 数据契约基础接口
│   ├── IContract.cs   # 契约标记接口
│   └── IQuery.cs      # 查询接口标记
├── Exceptions/         # 异常处理体系
│   ├── DomainException.cs
│   ├── ValidationException.cs
│   ├── NotFoundException.cs
│   └── ConflictException.cs
├── Time/               # 时间抽象
│   ├── ISystemClock.cs
│   └── SystemClock.cs
├── Diagnostics/        # 诊断和监控
│   └── ApplicationInfo.cs
└── PlatformBootstrapper.cs  # 唯一装配入口
```

## 核心组件

### 1. 异常处理体系（Exceptions/）

Platform 提供结构化的异常基类，用于表示不同类型的领域错误：

#### DomainException
所有领域异常的基类：
```csharp
public class DomainException : Exception
{
    public string ErrorCode { get; }
    public DomainException(string errorCode, string message);
}
```

#### ValidationException
用于输入验证失败：
```csharp
var errors = new Dictionary<string, string[]> 
{
    ["Email"] = new[] { "无效的邮箱格式" }
};
throw new ValidationException(errors);
```

#### NotFoundException
用于资源不存在：
```csharp
throw new NotFoundException("Order", orderId);
```

#### ConflictException
用于状态冲突：
```csharp
throw new ConflictException("订单已经被取消，无法修改");
```

### 2. 时间抽象（Time/）

提供可测试的时间抽象：

```csharp
public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
    DateTimeOffset Now { get; }
}
```

**使用场景**：
- 在业务逻辑中需要获取当前时间
- 在测试中需要控制时间流动

**示例**：
```csharp
// 在业务代码中
public class OrderHandler
{
    private readonly ISystemClock _clock;
    
    public OrderHandler(ISystemClock clock)
    {
        _clock = clock;
    }
    
    public void Handle(CreateOrder command)
    {
        var order = new Order
        {
            CreatedAt = _clock.UtcNow
        };
    }
}

// 在测试中（需要自定义测试实现）
public class FakeSystemClock : ISystemClock
{
    private readonly DateTimeOffset _fixedTime;
    
    public FakeSystemClock(DateTimeOffset fixedTime)
    {
        _fixedTime = fixedTime;
    }
    
    public DateTimeOffset UtcNow => _fixedTime;
    public DateTimeOffset Now => _fixedTime.ToLocalTime();
}

// 使用
var fakeClock = new FakeSystemClock(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
var handler = new OrderHandler(fakeClock);
```

### 3. 数据契约（Contracts/）

提供模块间通信的基础接口标记。

详见 [Contracts/README.md](./Contracts/README.md)

### 4. 诊断信息（Diagnostics/）

提供应用程序运行时信息：

```csharp
public sealed class ApplicationInfo
{
    public string Name { get; init; }
    public string Version { get; init; }
    public string Environment { get; init; }
    public DateTimeOffset StartTime { get; init; }
}
```

### 5. PlatformBootstrapper

Platform 层的**唯一装配入口**：

```csharp
public static class PlatformBootstrapper
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // 注册所有 Platform 技术服务
        // - 时间抽象
        // - 应用信息
        // - 健康检查
        // - 日志和追踪（未来）
    }
}
```

## 使用方式

在 Host 的 Program.cs 中调用：

```csharp
using Zss.BilliardHall.Platform;
using Zss.BilliardHall.Application;

var builder = WebApplication.CreateBuilder(args);

// 1. 初始化 Platform（技术基座）
PlatformBootstrapper.Configure(
    builder.Services,
    builder.Configuration,
    builder.Environment);

// 2. 装配 Application（业务系统）
ApplicationBootstrapper.Configure(
    builder.Services,
    builder.Configuration,
    builder.Environment);

var app = builder.Build();
app.Run();
```

## 扩展指南

### 添加新的技术能力

当需要添加新的技术能力时：

1. **确认是否属于 Platform**：
   - ✅ 是技术性的，不感知业务
   - ✅ 可以被多个模块复用
   - ❌ 如果感知业务概念，应该放在模块中

2. **创建目录和组件**：
   ```
   Platform/
   └── NewCapability/
       ├── INewService.cs
       └── NewService.cs
   ```

3. **在 PlatformBootstrapper 中注册**：
   ```csharp
   public static void Configure(...)
   {
       // ... 现有注册
       
       // 新增能力
       services.AddSingleton<INewService, NewService>();
   }
   ```

### 不应该添加到 Platform 的内容

❌ **业务逻辑**：
```csharp
// ❌ 错误：包含业务概念
public class OrderValidator { ... }
```

❌ **对 Application 或 Modules 的依赖**：
```csharp
// ❌ 错误：依赖 Application
using Zss.BilliardHall.Application;
```

❌ **Host 特定的逻辑**：
```csharp
// ❌ 错误：判断 Host 类型
if (environment.IsWebHost()) { ... }
```

## 架构约束

Platform 层受以下架构测试保护：

1. `Platform_Should_Not_Depend_On_Application` - 确保不依赖 Application
2. `Platform_Should_Not_Depend_On_Modules` - 确保不依赖任何模块
3. `Platform_Should_Not_Contain_Business_Logic` - 确保不包含业务逻辑

违反这些约束将导致 CI 失败。

## 参考资料

- ADR-0002: Platform / Application / Host 三层启动体系
- ADR-0002 Copilot 提示词库
- 架构测试说明
