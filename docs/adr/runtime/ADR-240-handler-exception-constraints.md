---
adr: ADR-240
title: "Handler 异常约束"
status: Final
level: Runtime
version: "3.0"
deciders: "Architecture Board"
date: 2026-01-25
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---

# ADR-240：Handler 异常约束

> ⚖️ **本 ADR 定义 Handler 异常处理的强制规则，确保异常可分类、可重试、可追溯。**

**适用范围**：所有 Handler（Command/Query/Event Handler）  
**生效时间**：即刻  
**依赖 ADR**：ADR-0005（应用内交互模型）

---

## Focus（聚焦内容）

- Handler 结构化异常要求
- 可重试标记约束
- 异常吞噬禁止
- 异常命名空间规范
- 跨模块事件异常隔离
- 异常处理测试执法

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

## Decision（裁决）

### 禁止抛出通用异常（ADR-240.1）【必须架构测试覆盖】

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

### 可重试标记约束（ADR-240.2）【必须架构测试覆盖】

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

### 禁止吞噬异常（ADR-240.3）【必须架构测试覆盖】

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

### 异常命名空间约束（ADR-240.4）【必须架构测试覆盖】

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

### 跨模块事件异常隔离（ADR-240.5）【必须架构测试覆盖】

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

## 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 测试映射

| 规则编号 | 执行级 | 测试/手段 |
|---------|-------|----------|
| ADR-240.1 | L2 | `StructuredExceptionAnalyzer` (Roslyn) |
| ADR-240.2 | L1 | `ADR_240_Architecture_Tests.Retryable_Must_Be_Infrastructure` |
| ADR-240.3 | L2 | Roslyn Analyzer (待实现) |
| ADR-240.4 | L1 | `ADR_240_Architecture_Tests.Exceptions_Must_Be_In_Exceptions_Namespace` |
| ADR-240.5 | L3 | ARCH-GATE（架构审查） |

### 执行说明

**L1 测试**：
- 检测可重试异常是否继承自 InfrastructureException
- 验证异常类命名空间是否为 `*.Exceptions`

**L2 测试**：
- Roslyn Analyzer 检测是否抛出通用异常
- 静态分析检测异常吞噬模式

**L3 测试**：
- 架构审查验证事件异常隔离
- 人工审查跨模块通信异常处理

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例**仅在以下情况允许**：

1. **遗留系统集成**：与遗留系统集成，无法使用结构化异常
2. **外部库约束**：外部库强制的异常处理模式
3. **迁移期**：从旧异常体系向新体系迁移的过渡阶段

### 破例要求（不可省略）

每个破例**必须**：

- 记录在 `docs/summaries/arch-violations.md`
- 指明 ADR-240 + 具体规则编号（如 ADR-240.1）
- 指定失效日期（不超过 6 个月）
- 给出归还计划和责任人

**未记录的破例 = 未授权架构违规。**

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

* **运行时层 ADR**
  * 修改需 Tech Lead/架构师审批
  * 新增规则必须满足可自动判定性
  * 必须更新相关架构测试

### 失效与替代

* 如有更优方案，可创建 ADR-24X 替代本 ADR
* 被替代后，本 ADR 状态改为 Superseded

---

## Non-Goals（明确不管什么）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

- ✗ 异常消息内容和格式
- ✗ 重试策略的具体实现（指数退避、延迟时间等）
- ✗ 日志记录的具体字段和格式
- ✗ 幂等性的具体实现方式
- ✗ HTTP 状态码映射规则
- ✗ 异常的序列化和传输格式

> 上述内容属于工程标准与实践指南，参见《Handler 异常处理与重试工程标准》。

---

## References（非裁决性参考）

> **仅供理解，不具裁决力。**

### 相关 ADR
- [ADR-0005：应用内交互模型](../constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [ADR-201：Handler 生命周期管理](ADR-201-handler-lifecycle-management.md)

### 技术资源
- [Handler 异常处理与重试工程标准](../../guides/handler-exception-retry-standard.md)（非裁决性）
- [StructuredExceptionAnalyzer](../../../src/tools/ArchitectureAnalyzers/StructuredExceptionAnalyzer.cs)

### 实践指导
- 异常处理详细示例参见 `docs/copilot/adr-0240.prompts.md`

---


## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-0005：应用内交互模型与执行边界](../constitutional/ADR-0005-Application-Interaction-Model-Final.md) - Handler 异常约束基于 Handler 模式

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-201：Handler 生命周期管理](./ADR-201-handler-lifecycle-management.md) - 异常处理是生命周期的一部分

---


## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 | 影响级别 |
|------|------|----------|--------|---------|
| 3.0 | 2026-01-25 | 重构为裁决型格式，添加决策章节 | GitHub Copilot | High |
| 2.0 | 2026-01-24 | 精简为裁决型规则，将工程指南分离 | @copilot | High |
| 1.0 | 2026-01-24 | 初始版本（已废弃，内容过于详细） | @copilot | High |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**


---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充


---

## Non-Goals（明确不管什么）


本 ADR 明确不涉及以下内容：

- 待补充


---

## Enforcement（执法模型）


### 执行方式

待补充...


---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 待补充 | 初始版本 |
