# ADR-0002

## Platform / Application / Host 三层启动体系（Final）

**状态**：✅ 已采纳（Final，不可随意修改）
**级别**：架构约束（Architectural Contract）
**适用范围**：所有 Host、模块、测试、未来子系统
**生效时间**：即刻

---

## 1. 背景与目标

启动代码不是业务的一部分，但它**决定业务能否长期存活**。

在多数 `.NET` 项目中，`Program.cs` 是事故高发区：

* 模块注册散落
* 业务逻辑混入
* Host 与 Application 强耦合
* 新宿主复制粘贴导致架构分叉

**目标**：

* 可审查、可复制、可推演、可测试
* 任何新模块 / 新人 / 新宿主都“几乎无法犯错”
* 架构规则强制可验证

---

## 2. 核心立场（Strong Opinions）

1. 启动 ≠ 分层架构，而是**进程与装配模型**
2. `Program.cs` 的地位 ≈ 数据库 Schema
3. 启动代码必须比业务代码更保守
4. `Host` 只允许调用入口，不允许决策
5. 架构规则不可测试 = 等于不存在

---

## 3. 三层职责总览

```
┌────────────────────────────┐
│            Host            │  ← Web / Worker / Test
│  - 进程模型
│  - 协议 / 端口 / 运行方式
└────────────▲───────────────┘
             │
┌────────────┴───────────────┐
│        Application         │  ← 应用装配层（系统定义）
│  - 模块扫描与装配
│  - Wolverine / Marten
│  - 用例与 Vertical Slice
└────────────▲───────────────┘
             │
┌────────────┴───────────────┐
│          Platform          │  ← 技术平台基座
│  - Logging / Tracing
│  - ErrorModel / Exception
│  - Health / Metrics
└────────────────────────────┘
```

**唯一合法依赖方向**：

```
Host → Application → Platform
```

**任何反向依赖，均视为架构违规。**

---

## 4. Platform 层（技术基座）

### 4.1 定位

Platform 是**全系统技术基座**：

* 不感知业务
* 不感知模块
* 不感知 `Host` 类型

### 4.2 允许

* `Logging` / `Tracing`
* `ErrorCode` / `DomainException`
* 时间、`ID`、环境、进程信息
* `HealthChecks` / `Metrics`
* 通用 `BuildingBlocks`

### 4.3 严禁

* ❌ 引用 Application
* ❌ 模块扫描
* ❌ Handler / UseCase 注册
* ❌ 读取业务配置
* ❌ Host / Web / Worker 判断

### 4.4 标准入口（唯一）

```csharp
namespace Zss.BilliardHall.Platform;

public static class PlatformBootstrapper
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // logging / tracing / error model
    }
}
```

> Platform **不得依赖 ASP.NET Core 类型以外的 Host 语义**

---

## 5. Application 层（系统定义）

### 5.1 定位

Application 表示：

> **“这个系统是什么”**

不是 UI，不是 Web，不是 Worker。

### 5.2 允许

* Wolverine / MediatR 装配
* 模块扫描与加载
* Marten / EF / Persistence
* Pipeline / Policy
* Vertical Slice 注册

### 5.3 严禁

* ❌ HttpContext
* ❌ 端口 / 协议
* ❌ Host 类型判断
* ❌ Web / Worker 分支逻辑

### 5.4 核心约束

> **ApplicationBootstrapper 是应用能力装配的唯一合法入口**
> 禁止存在任何 `AddXxxModule` 或 `AddYyyFeature` 被 Host 调用

### 5.5 标准入口（唯一）

```csharp
namespace Zss.BilliardHall.Application;

public static class ApplicationBootstrapper
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration)
    {
        // modules / wolverine / persistence
    }
}
```

---

## 6. Host 层（必须薄到羞愧）

### 6.1 定位

Host 是**进程外壳**：

* 决定“怎么跑”
* 不决定“系统是什么”

### 6.2 允许

* 选择 Host 类型（Web / Worker / Test）
* 调用 Platform + Application
* 启动进程

### 6.3 严禁

* ❌ 业务逻辑
* ❌ 模块注册
* ❌ Wolverine / Marten 配置
* ❌ 领域对象操作
* ❌ 直接注册 Service

### 6.4 命名与结构规则

* 项目名 = `Zss.BilliardHall.Host.<Name>`
* RootNamespace = 项目名
* Host 是角色，不是技术（禁止 WebHost 词汇）

### 6.5 Program.cs（唯一推荐写法）

```csharp
using Zss.BilliardHall.Platform;
using Zss.BilliardHall.Application;

var builder = WebApplication.CreateBuilder(args);

PlatformBootstrapper.Configure(
    builder.Services,
    builder.Configuration,
    builder.Environment);

ApplicationBootstrapper.Configure(
    builder.Services,
    builder.Configuration);

var app = builder.Build();
app.Run();
```

> 超过 30 行 = 架构审查
> Program.cs 中允许中间件，仅限 Platform / Application 提供

---

## 7. 目录结构标准（支持 Host 多实例）

```
/src
├─ Platform/
│  └─ Zss.BilliardHall.Platform
├─ Application/
│  └─ Zss.BilliardHall.Application
├─ Modules/
│  ├─ Members/
│  │  └─ Zss.BilliardHall.Modules.Members
│  └─ Orders/
│     └─ Zss.BilliardHall.Modules.Orders
├─ Host/
│  ├─ Web/
│  │  └─ Zss.BilliardHall.Host.Web
│  ├─ Worker/
│  │  └─ Zss.BilliardHall.Host.Worker
│  └─ Test/
│     └─ Zss.BilliardHall.Host.Test
└─ Tests/
   ├─ ArchitectureTests
   └─ Modules.Tests
```

**硬性规则**

* Host 子目录名 = 项目名
* 单层 Host 目录（禁止 Web/Web.Api）
* Application 只做装配
* Platform 永远不碰业务
* Modules 必须完整 Vertical Slice

---

## 8. 命名空间约束（BaseNamespace）

### 8.1 Directory.Build.props 全局定义

```xml
<PropertyGroup>
    <CompanyNamespace>Zss</CompanyNamespace>
    <ProductNamespace>BilliardHall</ProductNamespace>
    <BaseNamespace>$(CompanyNamespace).$(ProductNamespace)</BaseNamespace>
</PropertyGroup>
```

### 8.2 项目 RootNamespace 强制规则

```xml
<PropertyGroup>
  <RootNamespace>$(BaseNamespace).Platform</RootNamespace>
</PropertyGroup>

<PropertyGroup>
  <RootNamespace>$(BaseNamespace).Application</RootNamespace>
</PropertyGroup>

<PropertyGroup>
  <RootNamespace>$(BaseNamespace).Modules.Members</RootNamespace>
</PropertyGroup>

<PropertyGroup>
  <RootNamespace>$(BaseNamespace).Host.Web</RootNamespace>
</PropertyGroup>
```

**禁止在单个项目中覆盖 BaseNamespace**

> 可用 MSBuild Target 强制拦截

---

## 9. 架构测试（CI 级铁律）

### 9.1 层级依赖约束

```csharp
[Fact]
public void Platform_Should_Not_Depend_On_Application() { ... }

[Fact]
public void Application_Should_Not_Depend_On_Host() { ... }
```

### 9.2 Host 边界约束

```csharp
[Fact]
public void Host_Should_Not_Contain_Business_Types() { ... }
```

### 9.3 Handler 约束

```csharp
[Fact]
public void Handlers_Should_Not_Depend_On_AspNet() { ... }
```

### 9.4 Host 注册服务约束（最终防线）

```csharp
[Fact]
public void Host_Should_Not_Register_Services_Directly() { ... }
```

---

## 10. 最终裁决（Final）

* 启动三层是**进程与装配模型，不是分层架构**
* Host 多实例是默认能力，不是扩展能力
* 架构规则必须可测试，否则等于不存在
* 违反本 ADR 的代码，**视为架构缺陷**

> ADR-0002 是启动体系的“宪法附录”，不是示例文档

---

## 11. 后续动作（强制）

* 架构测试接入 CI，失败禁止合并
* 固化 `ApplicationBootstrapper` 为唯一入口
* 后续 ADR 禁止推翻本 ADR，只允许补充
* 可考虑将此体系模板化 (`dotnet new`)
