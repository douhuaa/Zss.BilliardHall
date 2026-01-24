# ADR-240：Handler 异常处理与重试标准

**状态**：✅ Final  
**级别**：运行层约束（Runtime Constraint）  
**适用范围**：所有模块（Modules）、所有 Handler（Command/Query/Event Handler）  
**生效时间**：即刻  
**依赖 ADR**：ADR-0005（应用内交互模型）

---

## 本章聚焦内容（Focus）

- Handler 异常分类与处理规则
- 结构化异常体系定义
- 重试策略与幂等性要求
- 异常传播与转换边界
- 可观测性与日志规范

---

## 术语表（Glossary）

| 术语 | 定义 |
|------|------|
| 领域异常 | 业务规则违反引发的异常，需要业务层面处理 |
| 验证异常 | 输入数据不符合契约或约束的异常 |
| 基础设施异常 | 技术依赖（数据库、网络、外部服务）失败引发的异常 |
| 可重试异常 | 标记为可通过重试解决的暂时性失败异常 |
| 幂等性 | 多次执行相同操作结果一致的特性 |
| 异常边界 | 异常可以传播的模块/层级范围 |

---

## 规则本体（Rule）

### 1. 异常分类体系【必须架构测试覆盖】

Handler **必须**仅抛出以下三类结构化异常：

1. **领域异常（DomainException）**
   - 表示业务规则违反
   - 不可重试
   - 必须在领域模型中抛出
   - 示例：订单已取消、余额不足、状态不允许操作

2. **验证异常（ValidationException）**
   - 表示输入数据不符合契约
   - 不可重试
   - 必须在 Handler 入口处抛出
   - 示例：必填字段缺失、格式错误、业务约束违反

3. **基础设施异常（InfrastructureException）**
   - 表示技术依赖失败
   - 可能可重试（标记 IRetryable）
   - 必须在基础设施层抛出
   - 示例：数据库连接失败、网络超时、外部服务不可用

**禁止事项**：

- ❌ 禁止抛出 `System.Exception`
- ❌ 禁止抛出非结构化异常
- ❌ 禁止在 Handler 中捕获并吞噬异常
- ❌ 禁止跨类别异常转换（如将领域异常转为基础设施异常）

### 2. Handler 异常处理规则【必须架构测试覆盖】

**Command Handler 异常处理**：

- **必须**允许异常向上传播至调用者
- **必须**在异常发生时确保事务回滚
- **禁止**捕获并返回错误结果（应抛出异常）
- **可以**在抛出异常前记录日志和上下文

**Query Handler 异常处理**：

- **必须**允许异常向上传播至调用者
- **可以**在特定场景返回空结果而非抛出异常（如查询不存在的实体）
- **禁止**捕获并返回默认值掩盖错误

**Event Handler 异常处理**：

- **必须**允许异常向上传播至事件总线
- **必须**支持事件重试机制（对于 IRetryable 异常）
- **可以**实现补偿逻辑处理不可重试的失败
- **必须**记录所有异常以支持事件回溯

### 3. 重试策略标准【必须架构测试覆盖】

**可重试异常标准**：

仅以下情况的基础设施异常可标记为 `IRetryable`：

- ✅ 网络暂时性故障（连接超时、网络中断）
- ✅ 数据库暂时性故障（死锁、连接池耗尽）
- ✅ 外部服务暂时性不可用（HTTP 5xx、限流）
- ✅ 资源竞争导致的冲突（乐观锁冲突）

**禁止重试场景**：

- ❌ 领域异常（业务规则违反不会因重试而改变）
- ❌ 验证异常（输入错误不会因重试而修正）
- ❌ 永久性故障（HTTP 4xx、认证失败、资源不存在）

**重试策略要求**：

- **必须**使用指数退避策略（Exponential Backoff）
- **必须**设置最大重试次数上限（建议 3-5 次）
- **必须**设置总重试时间上限（建议 30-60 秒）
- **必须**在重试间添加抖动（Jitter）避免雪崩
- **必须**记录每次重试尝试和最终结果

### 4. 幂等性要求【必须架构测试覆盖】

**Command Handler 幂等性**：

- 支持重试的 Command Handler **必须**实现幂等性保障
- **必须**使用以下方式之一确保幂等：
  - ✅ 幂等键（Idempotency Key）检查
  - ✅ 自然键唯一性约束
  - ✅ 状态机防护（状态转换检查）
  - ✅ 乐观锁版本控制

**Event Handler 幂等性**：

- 所有 Event Handler **必须**实现幂等性
- **必须**能够安全处理重复事件
- **建议**记录已处理事件 ID 避免重复处理

### 5. 异常传播与转换边界

**模块内传播**：

- 领域层抛出的异常 **必须**原样传播到 Handler
- Handler **禁止**将领域异常转换为其他类型
- 基础设施层 **必须**将技术异常封装为 InfrastructureException

**跨模块传播**：

- 事件处理失败 **禁止**影响事件发布者
- 事件订阅者异常 **必须**由事件总线处理
- 跨模块调用（通过事件）**禁止**同步传播异常

**向外部传播**：

- Endpoint/API **必须**捕获 Handler 异常
- Endpoint **必须**将异常转换为合适的 HTTP 状态码
- Endpoint **禁止**暴露内部异常详情（仅暴露业务错误信息）

### 6. 可观测性与日志规范

**异常日志要求**：

- 所有未处理异常 **必须**记录完整堆栈跟踪
- 异常日志 **必须**包含关键上下文（如 CommandId、UserId、CorrelationId）
- 可重试异常 **必须**记录重试次数和间隔
- 最终失败 **必须**记录为 Error 级别

**结构化日志字段**：

```csharp
// 必须包含的字段
- ExceptionType: 异常类型全名
- IsRetryable: 是否可重试
- RetryAttempt: 当前重试次数（如适用）
- CorrelationId: 跨服务追踪 ID
- CommandId/QueryId/EventId: 业务操作标识
- UserId/MemberId: 业务主体标识
```

---

## 执法模型（Enforcement）

### 测试映射

| 规则编号 | 执行级 | 测试/手段 |
|---------|-------|----------|
| ADR-240.1 | L2 | `StructuredExceptionAnalyzer` (已存在) |
| ADR-240.2 | L1 | `Handler_Should_Not_Catch_And_Swallow_Exceptions_Test` |
| ADR-240.3 | L1 | `Retryable_Exceptions_Should_Be_Infrastructure_Only_Test` |
| ADR-240.4 | L2 | Code Review / 幂等性设计审查 |
| ADR-240.5 | L1 | `Cross_Module_Event_Exception_Should_Not_Propagate_Synchronously_Test` |
| ADR-240.6 | L2 | Code Review / 日志审查 |

### 执行级别说明

- **L1（静态分析）**：NetArchTest 自动验证，CI 阻断
- **L2（语义半自动）**：Roslyn Analyzer + Code Review
- **L3（人工审查）**：架构审查门（ARCH-GATE）

---

## 破例与归还（Exception）

### 允许破例的前提

破例**仅在以下情况允许**：

- 与遗留系统集成，无法使用结构化异常
- 性能关键路径需要特殊错误处理
- 外部库强制的异常处理模式

### 破例要求（不可省略）

每个破例**必须**：

- 记录在 `ARCH-VIOLATIONS.md`
- 指明 ADR-240 + 具体规则编号
- 指定失效日期（不超过 6 个月）
- 给出归还计划和责任人

---

## 变更政策（Change Policy）

### 变更规则

- 本 ADR 为运行层约束，可通过 RFC 流程修订
- 修改异常分类体系需架构委员会评审
- 新增异常类型需保持向后兼容

### 失效与替代

- 本 ADR 失效前必须有替代方案
- 替代 ADR 必须明确迁移路径

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：

- 具体异常消息内容和格式
- 异常的国际化和本地化
- 前端错误展示逻辑
- 业务层面的补偿事务设计细节
- 具体的重试库选型（如 Polly、Resilience4j）
- 异常的序列化和传输格式

---

## 非裁决性参考（References）

- [ADR-0005：应用内交互模型](../constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [StructuredExceptionAnalyzer](../../../src/tools/ArchitectureAnalyzers/StructuredExceptionAnalyzer.cs)

---

## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 | 影响级别 |
|------|------|----------|--------|---------|
| 1.0 | 2026-01-24 | 初始版本，定义 Handler 异常处理与重试标准 | @copilot | High |
