# ADR-0002：Platform / Application / Host 启动体系

**状态**：Final  
**级别**：宪法层  
**影响范围**：所有 Host、Platform、Application 及其装配过程  
**生效时间**：即刻

---

## 1. 规则本体（Rule）

> **这是本 ADR 唯一具有裁决力的部分。**

### R2.1 层级定义与职责

三层**必须**按以下职责划分：

- **Platform**：仅提供技术基座能力（日志/追踪/异常/健康检查）
- **Application**：系统装配层，负责模块扫描、用例注册、Wolverine/Marten 配置
- **Host**：进程外壳，决定运行方式（Web/Worker/Test）

### R2.2 依赖方向

依赖方向**必须**：

```
Host → Application → Platform
```

**禁止**：
- Platform 依赖 Application 或 Host
- Application 依赖 Host
- 任何反向依赖

### R2.3 Bootstrapper 入口

每层**必须**：
- 有且仅有一个 Bootstrapper 静态类作为装配入口
- Platform：`PlatformBootstrapper.Configure()`
- Application：`ApplicationBootstrapper.Configure()`

**禁止**：
- 多个装配入口
- Host 直接注册服务或配置模块

### R2.4 Platform 层约束

Platform 层**禁止**：
- 引用 Application 或 Host
- 引用任何 Modules
- 模块扫描或 Handler 注册
- 读取业务配置
- 包含业务逻辑

### R2.5 Application 层约束

Application 层**禁止**：
- 依赖 Host 层
- 直接依赖任何 Modules
- 使用 HttpContext 等 Host 专属类型
- 包含端口/协议配置

### R2.6 Host 层约束

Host 层**必须**：
- Program.cs ≤ 50 行
- 仅调用 Platform 和 Application 的 Bootstrapper

Host 层**禁止**：
- 直接依赖任何 Modules
- 包含业务类型或逻辑
- 直接注册 Service 或 Handler
- 配置 Wolverine/Marten

---

## 2. 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 2.1 执行级别

| 级别 | 名称      | 执法方式               | 后果    |
| ---- | ------- | ------------------ | ----- |
| L1   | 静态可执行   | 自动化测试（NetArchTest） | CI 阻断 |
| L2   | 语义半自动   | Roslyn Analyzer / 启发式 | 人工复核  |
| L3   | 人工 Gate | Code Review / Checklist | 架构裁决  |

### 2.2 测试映射

| 规则编号 | 执行级 | 测试 / 手段                        |
| ------- | --- | ------------------------------ |
| R2.1    | L3  | 人工 Code Review |
| R2.2    | L1  | `Verify_Complete_Three_Layer_Dependency_Direction` |
| R2.3    | L1  | `Platform_Should_Have_Single_Bootstrapper_Entry_Point` |
| R2.3    | L1  | `Application_Should_Have_Single_Bootstrapper_Entry_Point` |
| R2.4    | L1  | `Platform_Should_Not_Depend_On_Application` |
| R2.4    | L1  | `Platform_Should_Not_Depend_On_Host` |
| R2.4    | L1  | `Platform_Should_Not_Depend_On_Modules` |
| R2.5    | L1  | `Application_Should_Not_Depend_On_Host` |
| R2.5    | L1  | `Application_Should_Not_Depend_On_Modules` |
| R2.5    | L1  | `Application_Should_Not_Use_HttpContext` |
| R2.6    | L1  | `Program_Cs_Should_Be_Concise` |
| R2.6    | L1  | `Program_Cs_Should_Only_Call_Bootstrapper` |
| R2.6    | L1  | `Host_Should_Not_Depend_On_Modules` |
| R2.6    | L1  | `Host_Should_Not_Contain_Business_Types` |
| R2.6    | L1  | `Host_Csproj_Should_Not_Reference_Modules` |

### 2.3 测试位置

所有架构测试位于：`src/tests/ArchitectureTests/ADR/ADR_0002_Architecture_Tests.cs`

---

## 3. 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 3.1 允许破例的前提

破例 **仅在以下情况允许**：

* 迁移期遗留代码（必须在 6 个月内归还）
* 第三方库的技术限制（需架构委员会审批）
* 性能关键路径的特殊优化（需架构委员会审批）

### 3.2 破例要求（不可省略）

每个破例 **必须**：

* 记录在 `docs/summaries/ARCH-VIOLATIONS.md`
* 指明 ADR-0002 + 规则编号（如 R2.4）
* 指定失效日期（不超过 6 个月）
* 给出归还计划（具体到季度）

**未记录的破例 = 未授权架构违规。**

---

## 4. 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 4.1 变更规则

* **宪法层 ADR**（ADR-0001~0005）

  * 修改 = 架构修宪
  * 需要架构委员会 100% 同意
  * 需要 2 周公示期
  * 需要全量回归测试

### 4.2 失效与替代

* Superseded ADR **必须**：
  - 状态标记为 "Superseded by ADR-YYYY"
  - 指向替代 ADR
  - 保留在仓库中（不删除）
  - 移除或更新对应测试

* 不允许"隐性废弃"（偷偷删除或不标记状态）

### 4.3 同步更新

ADR 变更时 **必须** 同步更新：

* 架构测试代码
* Copilot prompts 文件（`docs/copilot/adr-0002.prompts.md`）
* 映射脚本
* README 导航

---

## 5. 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

* 命名空间自动推导规则 → ADR-0003
* 依赖包管理规则 → ADR-0004
* 运行时交互模型（Handler/CQRS） → ADR-0005
* 模块内部组织结构 → ADR-0001
* 具体技术选型（如使用哪个日志库） → ADR-300+ 技术层
* 教学示例和最佳实践 → `docs/copilot/adr-0002.prompts.md`

---

## 6. 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 术语表

| 术语            | 定义说明 |
|----------------|--------------------------------------|
| Platform       | 技术基座，仅提供技术能力，不感知业务 |
| Application    | 应用装配层，定义"系统是什么"，聚合模块和用例 |
| Host           | 进程外壳，决定"怎么跑"，如 Web/Worker/Test |
| Bootstrapper   | 唯一的装配入口，负责注册服务和配置 |
| 单向依赖        | Host → Application → Platform，禁止反向依赖 |

### 相关 ADR

- [ADR-0000：架构测试与 CI 治理](../governance/ADR-0000-architecture-tests.md)
- [ADR-0001：模块化单体与垂直切片架构](ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0003：命名空间与项目边界规范](ADR-0003-namespace-rules.md)
- [ADR-0004：中央包管理与依赖](ADR-0004-Cpm-Final.md)
- [ADR-0005：应用内交互模型](ADR-0005-Application-Interaction-Model-Final.md)

### 辅导材料

- `docs/copilot/adr-0002.prompts.md` - 示例代码和常见问题
- `docs/copilot/backend-development.instructions.md` - 后端开发指导

### 推荐结构示例

```
/src
├─ Platform/
│  └─ Zss.BilliardHall.Platform
│     └─ PlatformBootstrapper.cs
├─ Application/
│  └─ Zss.BilliardHall.Application
│     └─ ApplicationBootstrapper.cs
├─ Modules/
│  ├─ Members/
│  │  └─ Zss.BilliardHall.Modules.Members
│  └─ Orders/
│     └─ Zss.BilliardHall.Modules.Orders
└─ Host/
   ├─ Web/
   │  └─ Zss.BilliardHall.Host.Web
   │     └─ Program.cs
   ├─ Worker/
   │  └─ Zss.BilliardHall.Host.Worker
   └─ Test/
      └─ Zss.BilliardHall.Host.Test
```

（注：此为参考，非强制）

### 代码示例

**合规的 Program.cs**：

```csharp
using Zss.BilliardHall.Platform;
using Zss.BilliardHall.Application;

var builder = WebApplication.CreateBuilder(args);

// ✅ 仅调用 Bootstrapper
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

**违规示例**：

```csharp
// ❌ Host 直接注册服务
builder.Services.AddScoped<IOrderService, OrderService>();

// ❌ Host 配置 Wolverine
builder.Services.AddWolverine(opts => { ... });

// ❌ Host 扫描模块
builder.Services.Scan(scan => scan.FromAssembliesOf<OrdersModule>());

// ❌ Program.cs 超过 50 行，包含业务逻辑
```

### 检查清单

开发时自检：

- [ ] Platform 是否只提供技术能力，不依赖业务？
- [ ] Application 只做装配，无 Host/业务依赖？
- [ ] Host 仅负责装配和启动，无业务逻辑、无服务注册？
- [ ] Program.cs 是否极简（≤50行）、只调用 Bootstrapper？
- [ ] 每一层均有唯一 Bootstrapper 入口定义？
- [ ] 所有依赖方向只允许单向流动？

### 版本历史

| 版本 | 日期 | 变更摘要 |
|------|------|---------|
| 4.0 | 2026-01-24 | 采用终极模板，明确规则与执法分离 |
| 3.0 | 2026-01-22 | 结构升级、去编号化、加强测试映射 |
| 2.0 | 2026-01-20 | 目录与依赖方向细化 |
| 1.0 | 初版 | 初始发布 |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
