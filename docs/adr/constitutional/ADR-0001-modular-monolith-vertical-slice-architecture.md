# ADR-0001：模块化单体与垂直切片架构

**状态**：✅ 已采纳（Final，不可随意修改）  
**级别**：架构约束 / 系统级 Contract  
**适用范围**：所有 Host、模块、各类测试、未来扩展子系统  
**生效时间**：即刻
**修订摘要**：

- 明确横向/垂直切片标准和破解示例
- 补充约束执行级别、企业实践分层设计建议
- 快速表/术语/依赖关系索引规范化

---

## 聚焦内容（Focus）

- 按业务能力/上下文划分独立模块
- 强制垂直切片与最小用例组织
- 模块隔离：禁止跨模块直接依赖与共享领域对象
- 明确**契约传递**、**事件驱动**为唯一互访方式
- 支持自动化测试闭环，全流程治理

---

## 术语表（Glossary）

| 术语           | 定义说明                                      |
|--------------|-------------------------------------------|
| 模块化单体        | 单进程，按业务能力/上下文独立模块，物理分离，逻辑松耦合              |
| 垂直切片         | 以单个业务用例为最小代码/目录/依赖单元，横穿端到端实现              |
| 契约（Contract） | 只读、单向、版本化的数据 DTO，只用于信息传递                  |
| 领域事件         | 描述业务事实、供其他模块/系统订阅                         |
| 横向分层         | 传统 Controller/Service/Repository 抽象，本架构禁止 |
| 依赖隔离         | 外部模块对内部不可见，不可反射/直接调用/重用其实现                |
| 企业级模块        | 可单独治理、可独立测试、发布、可迁移的业务单元                   |

---

## 决策（Decision）

### 模块定义与隔离

- 按业务能力聚合单独模块（如 Members/Orders/Payments）
- 每模块 = 独立程序集 = 清晰边界目录&命名空间
- 模块外部仅暴露契约和事件，内部实现完全不可被引用

**判例规范：**

- ❌ 禁止模块引用其他模块代码、资源及类型
- ❌ 禁止公共"跨模块继承"或"共用领域类型"
- ✅ 允许通过契约传递原始/DTO，事件发布订阅协作

### 垂直切片落地标准

- 以"用例"为最小组织单元，目录建议 `UseCases/xxx/`
- 每用例切片须涵盖 Endpoint/API → Command/Query → Handler → 业务逻辑 → 存储
- ❌ 禁止将业务逻辑抽象到横向 Service 层
- ❌ 禁止横向领域/基础设施依赖上移

**推荐结构：**

```
/Modules/Orders/
  ├── UseCases/
  │    ├── CreateOrder/
  │    │    ├── CreateOrderEndpoint.cs
  │    │    ├── CreateOrderCommand.cs
  │    │    └── CreateOrderHandler.cs
  │    └── CancelOrder/...
```

### 通信与契约边界

- 允许：领域事件、契约 DTO、原始/标注类型
- 严禁：直接依赖 Entity/Aggregate/VO 或服务接口
- 契约用途：仅作数据传递，不含决策/行为方法，不可做业务判断

**反例（不合规）：**

```csharp
// ❌ 业务 Handler 依赖其他模块的 Repository/Service/Entity
public class PaymentHandler
{
    private readonly OrderRepository _orderRepo; // 违规：引用其他模块
}
// ❌ Contracts 包含业务流转字段
public class OrderContract { public bool CanRefund { get; set; } } // 用于业务状态流转，违规
```

**正例（合规）：**

```csharp
// ✅ 只通过只读契约/领域事件传递数据
public record OrderInfoDto(Guid Id, decimal Amount);
public record OrderPaidEvent(Guid OrderId);
```

### 用例职责与执行分工

| 角色            | 职责          | 禁止行为        |
|---------------|-------------|-------------|
| Command/Query | 仅表意，不含逻辑    | -           |
| Handler       | 核心业务规则+一致性  | 转发决策、依赖横向服务 |
| 领域对象          | 内聚不变量、不感知外部 | 被外部依赖       |

### Platform 层限制

- 技术能力专属，不可感知业务
- 不可下沉业务判断/分流
- 仅注册日志、追踪、异常、序列化等

---

## 快速参考和架构测试映射

| 约束编号       | 描述                | 层级    | 测试用例/自动化                                         | 章节             |
|------------|-------------------|-------|--------------------------------------------------|----------------|
| ADR-0001.1 | 模块不可相互引用          | L1    | Modules_Should_Not_Reference_Other_Modules       | 决策与约束-模块定义与隔离  |
| ADR-0001.2 | 项目文件/程序集禁止引用其他模块  | L1    | Module_Csproj_Should_Not_Reference_Other_Modules | 决策与约束-模块定义与隔离  |
| ADR-0001.3 | 垂直切片/用例为最小单元      | L2    | Handlers_Should_Be_In_UseCases_Namespace         | 决策与约束-垂直切片落地标准 |
| ADR-0001.4 | 禁止横向 Service 抽象   | L1    | Modules_Should_Not_Contain_Service_Classes       | 决策与约束-垂直切片落地标准 |
| ADR-0001.5 | 只允许事件/契约/原始类型通信   | L2    | Contract_Rules_Semantic_Check                    | 决策与约束-通信与契约边界  |
| ADR-0001.6 | Contract 不含业务判断字段 | L2/L3 | Contract_Business_Field_Analyzer                 | 决策与约束-通信与契约边界  |
| ADR-0001.7 | 命名空间/目录强制隔离       | L1    | Namespace_Should_Match_Module_Boundaries         | 决策与约束-模块定义与隔离  |

> L1: 静态可执行自动化（ArchitectureTests），L2: 语义半自动（Roslyn/启发式），L3: 人工Gate

---

## 依赖与补充关系

| 关联 ADR   | 关系                |
|----------|-------------------|
| ADR-0000 | 自动化测试机制与执行分级      |
| ADR-0002 | 定义装配/启动方式         |
| ADR-0003 | 模块命名空间自动映射及目录防御规则 |
| ADR-0004 | 分层包依赖与 CPM        |
| ADR-0005 | 运行时交互/Handler职责   |

---

## 检查表

- [ ] 模块按业务能力划分且物理完全隔离？
- [ ] 垂直切片以用例（UseCase）为唯一最小组织单元？
- [ ] 是否杜绝了横向 Service、领域模型共享、同步耦合？
- [ ] 模块间通信仅允许事件、契约和原始类型？
- [ ] 合约和事件定义是否无业务决策字段？
- [ ] 所有隔离规则是否有自动化测试或人工 Gate？

---

## 扩展落地建议

- 契约分包：推荐所有跨模块 Contracts 独立 `*.Contracts` 项目，分层版本化管理
- 事件设计：领域/集成事件建议统一格式+工具化校验
- 模块治理：配套平台治理平台/自动审核模块互依
- Onboarding：新人开发者须先阅读本 ADR，配合全自动化独立测试

---

## 版本历史

| 版本  | 日期         | 变更摘要                        |
|-----|------------|-----------------------------|
| 3.2 | 2026-01-23 | 术语&执行等级补充，约束映射与示例细化，企业级实践加注 |
| 3.1 | 2026-01-21 | 快速表/依赖细化                    |
| 3.0 | 2026-01-20 | 分层体系与目录规则固化                 |
| 1.0 | 初始         | 初版发布                        |

---

## 附件

- [ADR-0002 Platform/Application/Host 三层启动体系](ADR-0002-platform-application-host-bootstrap.md)
- [ADR-0003 命名空间与项目边界规范](ADR-0003-namespace-rules.md)
- [ADR-0004 中央包管理与依赖](ADR-0004-Cpm-Final.md)
- [ADR-0005 应用内交互模型](ADR-0005-Application-Interaction-Model-Final.md)
