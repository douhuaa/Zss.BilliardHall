---
adr: ADR-240
title: "Handler 异常约束"
status: Final
level: Runtime
version: "4.0"
deciders: "Architecture Board"
date: 2026-02-06
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---


# ADR-240：Handler 异常约束

> ⚖️ **本 ADR 定义 Handler 异常处理的强制规则，确保异常可分类、可重试、可追溯。**

**适用范围**：所有 Handler（Command/Query/Event Handler）  
## Focus（聚焦内容）

- Handler 结构化异常要求
- 可重试标记约束
- 异常吞噬禁止
- 异常命名空间规范
- 跨模块事件异常隔离
- 异常处理测试执法

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|-----|------|---------|
| 结构化异常 | 继承自特定基类的分类异常 | Structured Exception |
| DomainException | 业务逻辑错误异常基类 | Domain Exception |
| ValidationException | 验证失败异常类型 | Validation Exception |
| InfrastructureException | 基础设施错误异常基类 | Infrastructure Exception |
| IRetryable | 标记异常可重试的接口 | Retryable Interface |
| 异常吞噬 | 捕获异常后不重新抛出 | Exception Swallowing |
| 异常传播 | 异常向上层调用者传递 | Exception Propagation |

---

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-240 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-240_<Rule>_<Clause>
> ```

---

### ADR-240.1：结构化异常要求（Rule）

#### ADR-240.1.1 禁止抛出通用异常

**规则**：
- Handler 禁止抛出 `System.Exception`
- 必须使用以下三类结构化异常之一：
  - ✅ `DomainException` - 业务逻辑错误
  - ✅ `ValidationException` - 验证失败
  - ✅ `InfrastructureException` - 基础设施错误

**判定**：
- ❌ Handler 直接抛出 `throw new Exception()`
- ❌ Handler 抛出 `ApplicationException`
- ❌ Handler 抛出其他通用异常类型
- ✅ Handler 抛出 DomainException 及其子类
- ✅ Handler 抛出 ValidationException
- ✅ Handler 抛出 InfrastructureException 及其子类

---

---

### ADR-240.2：可重试标记约束（Rule）

#### ADR-240.2.1 IRetryable 接口使用约束

**规则**：
- 实现 `IRetryable` 接口的异常必须继承自 `InfrastructureException`
- `DomainException` 和 `ValidationException` 禁止实现 `IRetryable`
- 业务逻辑错误和验证错误不应重试

**判定**：
- ❌ DomainException 实现 IRetryable
- ❌ ValidationException 实现 IRetryable
- ❌ 自定义异常实现 IRetryable 但不继承 InfrastructureException
- ✅ InfrastructureException 子类实现 IRetryable
- ✅ DomainException 不实现 IRetryable
- ✅ ValidationException 不实现 IRetryable

---

---

### ADR-240.3：异常传播约束（Rule）

#### ADR-240.3.1 禁止吞噬异常

**规则**：
- Handler 禁止捕获异常后不重新抛出
- 异常必须向上传播到统一异常处理中间件
- 允许捕获后转换为更具体的异常类型

**判定**：
- ❌ `catch (Exception) { return; }` - 吞噬异常
- ❌ `catch (Exception) { /* 仅记录日志 */ }` - 吞噬异常
- ❌ `catch (Exception e) { }` - 空catch块
- ✅ `catch (Exception e) { throw; }` - 重新抛出
- ✅ `catch (DbException e) { throw new InfrastructureException(..., e); }` - 转换后抛出

---

---

### ADR-240.4：异常命名空间约束（Rule）

#### ADR-240.4.1 异常命名空间组织规范

**规则**：
- 所有自定义异常必须位于 `*.Exceptions` 命名空间
- 命名空间结构：
  - ✅ `Zss.BilliardHall.Platform.Exceptions`
  - ✅ `Zss.BilliardHall.Modules.Orders.Exceptions`
  - ❌ 其他命名空间

**判定**：
- ❌ 异常类在 `*.Domain` 命名空间
- ❌ 异常类在 `*.UseCases` 命名空间
- ❌ 异常类在其他非 `*.Exceptions` 命名空间
- ✅ 异常类在 `*.Exceptions` 命名空间

---

---

### ADR-240.5：跨模块事件异常隔离（Rule）

#### ADR-240.5.1 事件异常隔离要求

**规则**：
- Event Handler 异常禁止同步传播到事件发布者
- 事件订阅失败必须由事件总线处理
- 发布方不得感知订阅方的处理结果

**判定**：
- ❌ 事件发布后等待订阅者处理结果
- ❌ 订阅者异常直接传播到发布者
- ✅ 事件总线捕获订阅者异常
- ✅ 发布者不感知订阅者执行状态

---

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-240 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-240.1.1** | L1 | Roslyn Analyzer + ArchitectureTests | §ADR-240.1.1 禁止抛出通用异常 |
| **ADR-240.2.1** | L1 | ArchitectureTests 自动化验证 | §ADR-240.2.1 IRetryable 接口使用约束 |
| **ADR-240.3.1** | L1 | Roslyn Analyzer + 人工审查 | §ADR-240.3.1 禁止吞噬异常 |
| **ADR-240.4.1** | L1 | ArchitectureTests 自动化验证 | §ADR-240.4.1 异常命名空间组织规范 |
| **ADR-240.5.1** | L1 | ArchitectureTests 自动化验证 | §ADR-240.5.1 事件异常隔离要求 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决


---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- 异常处理中间件的具体实现（ASP.NET Core/gRPC）
- 异常日志记录的格式和存储位置
- 异常监控和告警的具体实现
- 用户友好错误消息的翻译和本地化
- 异常重试策略的具体参数（重试次数、间隔等）
- 死信队列的具体实现和管理

---

## Prohibited（禁止行为）

以下行为明确禁止：

- ❌ Handler 直接抛出 `System.Exception` 或 `ApplicationException`
- ❌ DomainException 或 ValidationException 实现 IRetryable 接口
- ❌ 捕获异常后既不重新抛出也不转换（异常吞噬）
- ❌ 在非 `*.Exceptions` 命名空间定义异常类
- ❌ Event Handler 异常同步传播到事件发布者
- ❌ 空的 catch 块（`catch (Exception) { }`）


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-005：应用内交互模型与执行边界](../constitutional/ADR-005-Application-Interaction-Model-Final.md) - Handler 异常约束基于 Handler 模式

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-201：Handler 生命周期管理](./ADR-201-handler-lifecycle-management.md) - 异常处理是生命周期的一部分
- [ADR-220：事件总线集成规范](./ADR-220-event-bus-integration.md) - 事件异常隔离相关

---

---

## References（非裁决性参考）

> **仅供理解，不具裁决力。**

### 相关 ADR
- [ADR-005：应用内交互模型](../constitutional/ADR-005-Application-Interaction-Model-Final.md)
- [ADR-201：Handler 生命周期管理](ADR-201-handler-lifecycle-management.md)

### 技术资源
- [Handler 异常处理与重试工程标准](../../guides/handler-exception-retry-standard.md)（非裁决性）
- [StructuredExceptionAnalyzer](../../../src/tools/ArchitectureAnalyzers/StructuredExceptionAnalyzer.cs)

### 实践指导
- 异常处理详细示例参见 `docs/copilot/adr-240.prompts.md`

---

---

## History（版本历史）

| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 4.0 | 2026-02-06 | 对齐 ADR-907-A v2.0 标准：转换为 Rule/Clause 双层编号体系，补充完整 Enforcement 映射表、Non-Goals 和 Prohibited 章节 |
| 3.0 | 2026-01-25 | 补充跨模块事件异常隔离规则 |
| 2.0 | 2026-01-23 | 补充可重试标记约束 |
| 1.0 | 2026-01-29 | 初始版本 |
