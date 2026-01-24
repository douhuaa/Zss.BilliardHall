# ADR-240：Handler 异常约束

**状态**：✅ Final  
**级别**：运行层约束（Runtime Constraint）  
**适用范围**：所有模块（Modules）、所有 Handler（Command/Query/Event Handler）  
**生效时间**：即刻  
**依赖 ADR**：ADR-0005（应用内交互模型）

---

## 规则本体（Rule）

> **本 ADR 仅包含可自动判定的裁决性规则。**

### ADR-240.1：Handler 禁止抛出通用异常【必须架构测试覆盖】

Handler **禁止**抛出 `System.Exception`。

**必须**使用以下三类结构化异常之一：

- `DomainException` - 继承此基类
- `ValidationException` - 使用此类型
- `InfrastructureException` - 继承此基类

**执行级别**：L2（Roslyn Analyzer）

---

### ADR-240.2：可重试标记仅限基础设施异常【必须架构测试覆盖】

实现 `IRetryable` 接口的异常**必须**继承自 `InfrastructureException`。

`DomainException` 和 `ValidationException` **禁止**实现 `IRetryable`。

**执行级别**：L1（NetArchTest）

---

### ADR-240.3：Handler 禁止吞噬异常【必须架构测试覆盖】

Handler **禁止**捕获异常后不重新抛出。

**违规示例**：

```csharp
// ❌ 禁止
public async Task Handle(Command cmd)
{
    try { /* ... */ }
    catch (Exception) { return; } // 吞噬异常
}
```

**执行级别**：L2（Roslyn Analyzer）

---

### ADR-240.4：异常类型命名空间约束【必须架构测试覆盖】

所有自定义异常**必须**位于 `*.Exceptions` 命名空间。

**正确示例**：

- `Zss.BilliardHall.Platform.Exceptions`
- `Zss.BilliardHall.Modules.Orders.Exceptions`

**执行级别**：L1（NetArchTest）

---

### ADR-240.5：跨模块事件异常禁止同步传播【必须架构测试覆盖】

Event Handler 异常**禁止**同步传播到事件发布者。

事件订阅失败**必须**由事件总线处理，不得影响发布方。

**执行级别**：L3（架构审查门）

---

## 执法模型（Enforcement）

| 规则编号 | 执行级 | 测试/手段 |
|---------|-------|----------|
| ADR-240.1 | L2 | `StructuredExceptionAnalyzer` (已存在) |
| ADR-240.2 | L1 | `ADR_0240_Architecture_Tests` |
| ADR-240.3 | L2 | Roslyn Analyzer (待实现) |
| ADR-240.4 | L1 | `ADR_0240_Architecture_Tests` |
| ADR-240.5 | L3 | ARCH-GATE |

---

## 破例与归还（Exception）

### 允许破例的前提

- 与遗留系统集成，无法使用结构化异常
- 外部库强制的异常处理模式

### 破例要求

每个破例**必须**：

- 记录在 `ARCH-VIOLATIONS.md`
- 指明 ADR-240 + 具体规则编号
- 指定失效日期（不超过 6 个月）
- 给出归还计划和责任人

---

## 变更政策（Change Policy）

- 本 ADR 为运行层约束，可通过 RFC 流程修订
- 新增规则必须满足可自动判定性
- 规则修改需架构委员会评审

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：

- 异常消息内容和格式
- 重试策略的具体实现（指数退避、延迟时间等）
- 日志记录的具体字段和格式
- 幂等性的具体实现方式
- HTTP 状态码映射规则
- 异常的序列化和传输格式

> 上述内容属于工程标准与实践指南，参见《Handler 异常处理与重试工程标准》。

---

## 非裁决性参考（References）

- [ADR-0005：应用内交互模型](../constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [Handler 异常处理与重试工程标准](../../guides/handler-exception-retry-standard.md)（非裁决性）
- [StructuredExceptionAnalyzer](../../../src/tools/ArchitectureAnalyzers/StructuredExceptionAnalyzer.cs)

---

## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 | 影响级别 |
|------|------|----------|--------|---------|
| 2.0 | 2026-01-24 | 精简为裁决型规则，将工程指南分离 | @copilot | High |
| 1.0 | 2026-01-24 | 初始版本（已废弃，内容过于详细） | @copilot | High |
