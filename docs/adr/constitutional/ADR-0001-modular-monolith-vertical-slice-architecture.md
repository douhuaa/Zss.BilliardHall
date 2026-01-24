# ADR-0001：模块化单体与垂直切片架构

**状态**：Final  
**级别**：宪法层  
**影响范围**：所有 Host、模块、各类测试、未来扩展子系统  
**生效时间**：即刻

---

## 规则本体（Rule）

> **这是本 ADR 唯一具有裁决力的部分。**

### R1.1 模块定义与隔离

模块**必须**：
- 按业务能力划分（如 Members/Orders/Payments）
- 为独立程序集，具有清晰边界目录和命名空间
- 外部仅暴露契约和事件，内部实现完全不可被引用

模块**禁止**：
- 引用其他模块代码、资源及类型
- 跨模块继承或共用领域类型
- 被其他模块的项目文件/程序集直接引用

### R1.2 垂直切片组织

每个业务用例**必须**：
- 以用例（UseCase）为最小组织单元
- 组织在 `UseCases/{UseCaseName}/` 目录
- 包含完整的端到端实现（Endpoint → Command/Query → Handler → 业务逻辑 → 存储）

**禁止**：
- 将业务逻辑抽象到横向 Service 层
- 创建横向领域/基础设施依赖

### R1.3 跨模块通信

跨模块通信**必须**仅使用以下方式之一：
- 领域事件（异步，发布-订阅）
- 数据契约（只读 DTO）
- 原始类型（Guid、string、int）

**禁止**：
- 直接依赖其他模块的 Entity/Aggregate/VO
- 直接依赖其他模块的服务接口
- 同步跨模块方法调用

### R1.4 契约约束

契约（Contract）**必须**：
- 仅用于数据传递
- 只读且单向
- 版本化管理

契约**禁止**：
- 包含业务决策/行为方法
- 包含用于业务判断的字段（如 CanRefund、IsActive 等状态流转字段）

### R1.5 Platform 层限制

Platform 层**必须**：
- 仅提供技术能力
- 不感知业务逻辑

Platform 层**禁止**：
- 下沉业务判断或分流逻辑
- 包含业务相关的验证器或服务

---

## 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 执行级别

| 级别 | 名称      | 执法方式               | 后果    |
| ---- | ------- | ------------------ | ----- |
| L1   | 静态可执行   | 自动化测试（NetArchTest） | CI 阻断 |
| L2   | 语义半自动   | Roslyn Analyzer / 启发式 | 人工复核  |
| L3   | 人工 Gate | Code Review / Checklist | 架构裁决  |

### 测试映射

| 规则编号 | 执行级 | 测试 / 手段                        |
| ------- | --- | ------------------------------ |
| R1.1    | L1  | `Modules_Should_Not_Reference_Other_Modules` |
| R1.1    | L1  | `Module_Csproj_Should_Not_Reference_Other_Modules` |
| R1.1    | L1  | `Namespace_Should_Match_Module_Boundaries` |
| R1.2    | L2  | `Handlers_Should_Be_In_UseCases_Namespace` |
| R1.2    | L1  | `Modules_Should_Not_Contain_Service_Classes` |
| R1.3    | L2  | `Contract_Rules_Semantic_Check` |
| R1.4    | L2/L3 | `Contract_Business_Field_Analyzer` |
| R1.5    | L1  | Platform 依赖方向测试（ADR-0002） |

### 测试位置

所有架构测试位于：`src/tests/ArchitectureTests/ADR/ADR_0001_Architecture_Tests.cs`

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例 **仅在以下情况允许**：

* 迁移期遗留代码（必须在 6 个月内归还）
* 第三方库的技术限制（需架构委员会审批）
* 性能关键路径的特殊优化（需架构委员会审批）

### 破例要求（不可省略）

每个破例 **必须**：

* 记录在 `docs/summaries/ARCH-VIOLATIONS.md`
* 指明 ADR-0001 + 规则编号（如 R1.1）
* 指定失效日期（不超过 6 个月）
* 给出归还计划（具体到季度）

**未记录的破例 = 未授权架构违规。**

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

* **宪法层 ADR**（ADR-0001~0005）

  * 修改 = 架构修宪
  * 需要架构委员会 100% 同意
  * 需要 2 周公示期
  * 需要全量回归测试

### 失效与替代

* Superseded ADR **必须**：
  - 状态标记为 "Superseded by ADR-YYYY"
  - 指向替代 ADR
  - 保留在仓库中（不删除）
  - 移除或更新对应测试

* 不允许"隐性废弃"（偷偷删除或不标记状态）

### 同步更新

ADR 变更时 **必须** 同步更新：

* 架构测试代码
* Copilot prompts 文件（`docs/copilot/adr-0001.prompts.md`）
* 映射脚本
* README 导航

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

* 模块内部的组织结构（如是否使用 DDD 聚合设计）→ 模块自治
* 代码风格和命名约定（如是否使用 I 前缀）→ `.editorconfig`
* 具体技术选型（如使用哪个 ORM）→ ADR-300+ 技术层
* 教学示例和最佳实践 → `docs/copilot/adr-0001.prompts.md`
* 领域建模方法论 → 团队培训和实践
* 用例内部的实现细节 → 开发者自主决策

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 术语表

| 术语            | 定义说明 |
|----------------|--------------------------------------|
| 模块化单体       | 单进程，按业务能力/上下文独立模块，物理分离，逻辑松耦合 |
| 垂直切片         | 以单个业务用例为最小代码/目录/依赖单元，横穿端到端实现   |
| 契约（Contract） | 只读、单向、版本化的数据 DTO，只用于信息传递           |
| 领域事件         | 描述业务事实、供其他模块/系统订阅                       |
| 横向分层         | 传统 Controller/Service/Repository 抽象，本架构禁止 |
| 依赖隔离         | 外部模块对内部不可见，不可反射/直接调用/重用其实现      |
| 企业级模块       | 可单独治理、可独立测试、发布、可迁移的业务单元          |

### 相关 ADR

- [ADR-0000：架构测试与 CI 治理](../governance/ADR-0000-architecture-tests.md)
- [ADR-0002：Platform/Application/Host 启动体系](ADR-0002-platform-application-host-bootstrap.md)
- [ADR-0003：命名空间与项目边界规范](ADR-0003-namespace-rules.md)
- [ADR-0004：中央包管理与依赖](ADR-0004-Cpm-Final.md)
- [ADR-0005：应用内交互模型](ADR-0005-Application-Interaction-Model-Final.md)

### 辅导材料

- `docs/copilot/adr-0001.prompts.md` - 示例代码和常见问题
- `docs/copilot/backend-development.instructions.md` - 后端开发指导

### 推荐结构示例

```
/Modules/Orders/
  ├── UseCases/
  │    ├── CreateOrder/
  │    │    ├── CreateOrderEndpoint.cs
  │    │    ├── CreateOrderCommand.cs
  │    │    └── CreateOrderHandler.cs
  │    └── CancelOrder/
  │         ├── CancelOrderEndpoint.cs
  │         ├── CancelOrderCommand.cs
  │         └── CancelOrderHandler.cs
  ├── Domain/
  │    └── Order.cs
  └── Contracts/
       └── OrderDto.cs
```

（注：此为参考，非强制）

### 代码示例

**合规示例**：

```csharp
// ✅ 只通过只读契约/领域事件传递数据
public record OrderInfoDto(Guid Id, decimal Amount);
public record OrderPaidEvent(Guid OrderId, decimal Amount);

// ✅ 垂直切片组织
namespace Zss.BilliardHall.Modules.Orders.UseCases.CreateOrder;

public class CreateOrderHandler : ICommandHandler<CreateOrder>
{
    public async Task<Guid> Handle(CreateOrder command)
    {
        var order = new Order(command.MemberId, command.Items);
        await _repository.SaveAsync(order);
        await _eventBus.Publish(new OrderCreated(order.Id));
        return order.Id;
    }
}
```

**违规示例**：

```csharp
// ❌ 业务 Handler 依赖其他模块的 Repository/Service/Entity
public class PaymentHandler
{
    private readonly OrderRepository _orderRepo; // 违规：引用其他模块
}

// ❌ Contracts 包含业务流转字段
public class OrderContract 
{ 
    public bool CanRefund { get; set; } // 用于业务状态流转，违规
}

// ❌ 横向 Service 层
public class OrderService // 违规：横向抽象
{
    public void CreateOrder() { }
    public void CancelOrder() { }
}
```

### 用例职责分工

| 角色 | 职责 | 禁止行为 |
|------|------|---------|
| Command/Query | 仅表意，不含逻辑 | 包含业务规则 |
| Handler | 核心业务规则+一致性保证 | 转发决策、依赖横向服务 |
| 领域对象 | 内聚不变量、不感知外部 | 被外部直接依赖 |
| Endpoint | HTTP 适配，映射请求/响应 | 包含业务逻辑 |

### 扩展落地建议（非强制）

- **契约分包**：推荐所有跨模块 Contracts 独立 `*.Contracts` 项目，分层版本化管理
- **事件设计**：领域/集成事件建议统一格式+工具化校验
- **模块治理**：配套平台治理工具/自动审核模块互依
- **Onboarding**：新人开发者须先阅读本 ADR，配合全自动化独立测试

### 检查清单

开发时自检：

- [ ] 模块按业务能力划分且物理完全隔离？
- [ ] 垂直切片以用例（UseCase）为唯一最小组织单元？
- [ ] 是否杜绝了横向 Service、领域模型共享、同步耦合？
- [ ] 模块间通信仅允许事件、契约和原始类型？
- [ ] 合约和事件定义是否无业务决策字段？
- [ ] 所有隔离规则是否有自动化测试或人工 Gate？

### 版本历史

| 版本 | 日期 | 变更摘要 |
|------|------|---------|
| 4.0 | 2026-01-24 | 采用终极模板，明确规则与执法分离 |
| 3.2 | 2026-01-23 | 术语&执行等级补充，约束映射与示例细化，企业级实践加注 |
| 3.1 | 2026-01-21 | 快速表/依赖细化 |
| 3.0 | 2026-01-20 | 分层体系与目录规则固化 |
| 1.0 | 初始 | 初版发布 |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
