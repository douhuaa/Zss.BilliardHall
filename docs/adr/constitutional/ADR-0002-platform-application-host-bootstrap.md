# ADR-0002：Platform / Application / Host 三层启动体系

**状态**：✅ 已采纳（Final，不可随意修改）  
**级别**：架构约束（Architectural Contract）  
**适用范围**：所有 Host、模块、测试、未来子系统  
**生效时间**：即刻

---

## 本章聚焦内容（Focus）

本 ADR 是**静态结构层**的核心文档，聚焦于：

1. **三层装配模型**：Platform / Application / Host 的职责划分
2. **层级依赖方向**：单向依赖规则（Host → Application → Platform）
3. **唯一入口规范**：Bootstrapper 作为各层的唯一装配入口
4. **目录结构标准**：支持多 Host 实例的标准目录组织
5. **命名与结构规则**：Host 项目的命名和 Program.cs 的规范

**不涉及**：

- ❌ 命名空间自动推导（见 ADR-0003）
- ❌ 依赖包管理（见 ADR-0004）
- ❌ 运行时交互模型（见 ADR-0005）
- ❌ 架构测试机制（见 ADR-0000）
- ❌ 模块内部组织（见 ADR-0001）

---

## 术语表（Glossary）

| 术语           | 定义                                   |
|--------------|--------------------------------------|
| Platform     | 技术基座，提供日志、追踪、异常等基础技术能力，不感知业务         |
| Application  | 应用装配层，定义"系统是什么"，负责模块扫描和用例注册          |
| Host         | 进程外壳，决定"怎么跑"，如 Web / Worker / Test   |
| Bootstrapper | 各层的唯一装配入口，负责注册服务和配置                  |
| 单向依赖         | Host → Application → Platform，禁止反向依赖 |
| 三权分立         | 三层各司其职，互不干涉                          |

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

**【必须架构测试覆盖】**

* ❌ 引用 Application
* ❌ 引用 Host
* ❌ 引用任何 Modules
* ❌ 模块扫描
* ❌ Handler / UseCase 注册
* ❌ 读取业务配置
* ❌ Host / Web / Worker 判断

### 4.4 标准入口（唯一）

**【必须架构测试覆盖】**

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

**【必须架构测试覆盖】**

* ❌ 依赖 Host 层
* ❌ 直接依赖任何 Modules
* ❌ HttpContext
* ❌ 端口 / 协议
* ❌ Host 类型判断
* ❌ Web / Worker 分支逻辑

### 5.4 核心约束

**【必须架构测试覆盖】**

> **ApplicationBootstrapper 是应用能力装配的唯一合法入口**
> 禁止存在任何 `AddXxxModule` 或 `AddYyyFeature` 被 Host 调用

### 5.5 标准入口（唯一）

**【必须架构测试覆盖】**

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

**【必须架构测试覆盖】**

* ❌ 直接依赖任何 Modules
* ❌ 包含业务类型
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

**【必须架构测试覆盖】**

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

命名空间的详细规则请参阅：[ADR-0003：命名空间与项目边界规范](ADR-0003-namespace-rules.md)

**本节仅说明与启动体系相关的命名空间约束**：

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

> 详细的 MSBuild 推导逻辑和防御规则见 ADR-0003

---

## 9. 强化与测试

**【必须架构测试覆盖】**

所有层级依赖规则必须通过自动化架构测试验证。

**架构测试详见**：[ADR-0000：架构测试与 CI 治理](ADR-0000-architecture-tests.md)

**核心测试用例**：

- Platform 不应依赖 Application
- Platform 不应依赖 Host
- Platform 不应依赖任何 Modules
- Platform 应有唯一的 PlatformBootstrapper 入口
- Application 不应依赖 Host
- Application 不应依赖任何 Modules
- Application 应有唯一的 ApplicationBootstrapper 入口
- Application 不应包含 HttpContext 等 Host 专属类型
- Host 不应依赖任何 Modules
- Host 不应包含业务类型
- Host 项目文件不应引用 Modules
- Program.cs 应保持简洁（建议 ≤ 50 行）
- Program.cs 只应调用 Bootstrapper（语义检查）
- 完整的三层依赖方向验证 (Host → Application → Platform)

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

---

## 与其他 ADR 关系（Related ADRs）

| ADR      | 关系                        |
|----------|---------------------------|
| ADR-0000 | 定义本 ADR 的自动化测试机制          |
| ADR-0001 | 定义模块的组织方式，本 ADR 定义模块的装配方式 |
| ADR-0003 | 定义命名空间自动推导规则              |
| ADR-0004 | 定义依赖包管理规则                 |
| ADR-0005 | 定义运行时交互模型                 |

**依赖关系**：

- 本 ADR 定义静态启动结构
- ADR-0003 补充命名空间规则
- ADR-0004 补充依赖管理规则
- ADR-0005 定义运行时行为

---

## 快速参考表（Quick Reference Table）

| 约束编号        | 约束描述                                         | 必须测试 | 测试覆盖                                                    | ADR 章节      |
|-------------|----------------------------------------------|------|---------------------------------------------------------|-------------|
| ADR-0002.1  | Platform 不应依赖 Application                    | ✅    | Platform_Should_Not_Depend_On_Application               | 4.3, 9      |
| ADR-0002.2  | Platform 不应依赖 Host                           | ✅    | Platform_Should_Not_Depend_On_Host                      | 4.3, 9      |
| ADR-0002.3  | Platform 不应依赖任何 Modules                      | ✅    | Platform_Should_Not_Depend_On_Modules                   | 4.3, 9      |
| ADR-0002.4  | Platform 应有唯一的 PlatformBootstrapper 入口       | ✅    | Platform_Should_Have_Single_Bootstrapper_Entry_Point    | 4.4, 9      |
| ADR-0002.5  | Application 不应依赖 Host                        | ✅    | Application_Should_Not_Depend_On_Host                   | 5.3, 9      |
| ADR-0002.6  | Application 不应依赖任何 Modules                   | ✅    | Application_Should_Not_Depend_On_Modules                | 5.3, 9      |
| ADR-0002.7  | Application 应有唯一的 ApplicationBootstrapper 入口 | ✅    | Application_Should_Have_Single_Bootstrapper_Entry_Point | 5.4, 5.5, 9 |
| ADR-0002.8  | Application 不应包含 HttpContext 等 Host 专属类型     | ✅    | Application_Should_Not_Use_HttpContext                  | 5.3, 9      |
| ADR-0002.9  | Host 不应依赖任何 Modules                          | ✅    | Host_Should_Not_Depend_On_Modules                       | 6.3, 9      |
| ADR-0002.10 | Host 不应包含业务类型                                | ✅    | Host_Should_Not_Contain_Business_Types                  | 6.3, 9      |
| ADR-0002.11 | Host 项目文件不应引用 Modules                        | ✅    | Host_Csproj_Should_Not_Reference_Modules                | 6.3, 9      |
| ADR-0002.12 | Program.cs 应保持简洁（建议 ≤ 50 行）                  | ✅    | Program_Cs_Should_Be_Concise                            | 6.5, 9      |
| ADR-0002.13 | Program.cs 只应调用 Bootstrapper（语义检查）           | ✅    | Program_Cs_Should_Only_Call_Bootstrapper                | 6.5, 9      |
| ADR-0002.14 | 验证完整的三层依赖方向                                  | ✅    | Verify_Complete_Three_Layer_Dependency_Direction        | 3, 9        |

---

## 快速参考（Quick Reference）

### Platform 层检查清单

- [ ] 是否只提供技术能力？
- [ ] 是否不感知业务？
- [ ] 是否不依赖 Application？
- [ ] 是否提供了标准入口 PlatformBootstrapper？

### Application 层检查清单

- [ ] 是否只做模块装配？
- [ ] 是否不依赖 Host？
- [ ] 是否提供了标准入口 ApplicationBootstrapper？
- [ ] 是否避免了 HttpContext 等 Host 特# ADR-0002：Platform / Application / Host 三层启动体系

**状态**：✅ 已采纳（Final，不可随意修改）  
**级别**：架构约束（Architectural Contract）  
**适用范围**：所有 Host、模块、测试、未来子系统  
**生效时间**：即刻

---

## 聚焦内容（Focus）

- 三层装配模型：Platform / Application / Host 职责分明
- 层级依赖方向：唯一单向依赖（Host → Application → Platform）
- Bootstrapping 的唯一入口
- 目录结构与命名标准，支持多 Host 实例
- Program.cs 极简化，所有启动集中到装配入口
- 与命名空间和包管理的关系（见 ADR-0003、0004）

---

## 术语表（Glossary）

| 术语           | 定义                             |
|--------------|--------------------------------|
| Platform     | 技术基座，仅提供技术能力，不感知业务             |
| Application  | 应用装配层，定义“系统是什么”，聚合模块和用例        |
| Host         | 进程外壳，决定“怎么跑”，如 Web/Worker/Test |
| Bootstrapper | 唯一的装配入口，负责注册服务和配置              |
| 单向依赖         | Host → Application → Platform  |

---

## 决策（Decision）

### Platform 层

- 只提供通用技术能力
- 不感知任何业务（不可访问 Application、Host、Modules）
- 必须有唯一入口`PlatformBootstrapper`
- 只允许注册日志、追踪、异常、序列化等

### Application 层

- 负责系统能力的装配和集成
- 禁止依赖 Host
- 合理注册 Wolverine/Marten/所有 Pipeline
- 必须有唯一入口`ApplicationBootstrapper`
- 不直接依赖 Host/Modules/任何业务对象

### Host 层

- 唯一职责：调用 Platform、Application 的 Bootstrapper
- 决定进程模型，不包含任何业务逻辑
- Program.cs 保持极简（建议 ≤30 行）
- 项目命名为`Zss.BilliardHall.Host.*`

**通用���止项：**

- ❌ 禁止 Platform 依赖 Application/Host
- ❌ 禁止 Application 依赖 Host
- ❌ 禁止 Host 包含任何业务代码或直接注册 Handler/Module

---

## 快速参考和架构测试映射

| 约束编号        | 描述                            | 层级 | 测试用例/自动化                                                | 章节             |
|-------------|-------------------------------|----|---------------------------------------------------------|----------------|
| ADR-0002.1  | Platform 不应依赖 Application     | L1 | Platform_Should_Not_Depend_On_Application               | 决策-Platform    |
| ADR-0002.2  | Platform 不应依赖 Host            | L1 | Platform_Should_Not_Depend_On_Host                      | 决策-Platform    |
| ADR-0002.3  | Platform 不应依赖 Modules         | L1 | Platform_Should_Not_Depend_On_Modules                   | 决策-Platform    |
| ADR-0002.4  | Platform 应有唯一 Bootstrapper    | L1 | Platform_Should_Have_Single_Bootstrapper_Entry_Point    | 决策-Platform    |
| ADR-0002.5  | Application 不依赖 Host          | L1 | Application_Should_Not_Depend_On_Host                   | 决策-Application |
| ADR-0002.6  | Application 不依赖 Modules       | L1 | Application_Should_Not_Depend_On_Modules                | 决策-Application |
| ADR-0002.7  | Application 应有唯一 Bootstrapper | L1 | Application_Should_Have_Single_Bootstrapper_Entry_Point | 决策-Application |
| ADR-0002.8  | Application 不含 Host 专属类型      | L1 | Application_Should_Not_Use_HttpContext                  | 决策-Application |
| ADR-0002.9  | Host 不依赖 Modules              | L1 | Host_Should_Not_Depend_On_Modules                       | 决策-Host        |
| ADR-0002.10 | Host 不含业务类型                   | L1 | Host_Should_Not_Contain_Business_Types                  | 决策-Host        |
| ADR-0002.11 | Host 项目文件不应引用 Modules         | L1 | Host_Csproj_Should_Not_Reference_Modules                | 决策-Host        |
| ADR-0002.12 | Program.cs 建议 ≤30行            | L1 | Program_Cs_Should_Be_Concise                            | 决策-Host        |
| ADR-0002.13 | Program.cs 只应调用 Bootstrapper  | L1 | Program_Cs_Should_Only_Call_Bootstrapper                | 决策-Host        |
| ADR-0002.14 | 三层唯一依赖方向验证                    | L1 | Verify_Complete_Three_Layer_Dependency_Direction        | 决策             |

> L1: 静态自动化可执行（ArchitectureTests）

---

## 依赖与相关ADR

- ADR-0001：模块组织与切片（配合定义模块边界）
- ADR-0003：命名空间自动推导
- ADR-0004：包管理与依赖分层
- ADR-0005：运行时交互模型
- ADR-0000：架构测试机制

---

## 检查清单

- [ ] Platform 是否只提供技术能力，不依赖业务？
- [ ] Application 只做装配，无 Host/业务依赖？
- [ ] Host 仅负责装配和启动，无业务逻辑、无服务注册？
- [ ] Program.cs 是否极简、只调用 Bootstrapper？
- [ ] 每一层均有唯一 Bootstrapper 入口定义？
- [ ] 所有依赖方向只允许单向流动？

---

## 扩展落地建议

- 将 Bootstrapper 明确模板化，统一团队新建
- 多 Host 支持通过目录+命名空间一致性自动校验
- 所有启动/装配规则写入 Onboarding 培训
- Program.cs 超 30 行主动触发架构审查
- 强制架构测试配合 CI，防止依赖倒置

---

## 版本历史

| 版本  | 日期         | 变更摘要             |
|-----|------------|------------------|
| 3.0 | 2026-01-22 | 结构升级、去编号化、加强测试映射 |
| 2.0 | 2026-01-20 | 目录与依赖方向细化        |
| 1.0 | 初版         | 初始发布             |

---

## 附件

- [ADR-0003 命名空间与项目边界规范](ADR-0003-namespace-rules.md)
- [ADR-0004 中央包管理与依赖](ADR-0004-Cpm-Final.md)定类型？

### Host 层检查清单

- [ ] Program.cs 是否少于 30 行？
- [ ] 是否只调用 Platform 和 Application 的 Bootstrapper？
- [ ] 是否不包含业务逻辑？
- [ ] 是否不直接注册服务？
