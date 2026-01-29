---
adr: ADR-350
title: "日志与可观测性字段标准"
status: Final
level: Technical
version: "2.0"
deciders: "Architecture Board"
date: 2026-01-26
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---


# ADR-350：日志与可观测性字段标准

**影响范围**：所有日志输出、结构化日志字段  
## Focus（聚焦内容）

- 日志字段命名规范与一致性约束
- 敏感信息日志保护规则
- 错误日志必需字段要求
- 日志级别使用标准定义
- CorrelationId 传播约束

---

---

## Glossary（术语表）


| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 待补充 | 待补充 | TBD |


---

---

## Decision（裁决）

### ADR-350.1：请求日志必须包含 CorrelationId【必须架构测试覆盖】

所有与请求相关的日志**必须**包含 `CorrelationId` 字段。

**规则**：
- 每个请求**必须**生成唯一 CorrelationId
- CorrelationId **必须**在请求生命周期中传播
- 所有请求相关日志**必须**包含此字段
- **禁止**遗漏 CorrelationId

**判定**：
- ❌ 请求日志缺少 CorrelationId
- ✅ 所有请求日志包含 CorrelationId

**推荐实现**：使用 Serilog Enricher 全局注入，避免手动传递。

---

### ADR-350.2：禁止记录敏感信息【必须架构测试覆盖】

日志**禁止**包含敏感信息。

**规则**：
- **禁止**记录密码、密钥、Token
- **禁止**记录信用卡号、身份证号
- **禁止**记录完整个人隐私信息
- **禁止**记录加密密钥、API Secret

**允许记录**：
- 脱敏后的信息（如手机号 138****1234）
- 用户 ID（非 PII）
- 业务标识符

**判定**：
- ❌ 日志包含明文密码或密钥
- ❌ 日志包含完整信用卡号
- ✅ 日志仅包含脱敏后的安全信息

---

### ADR-350.3：日志字段必须使用 PascalCase 命名【必须架构测试覆盖】

结构化日志字段名**必须**使用 PascalCase 命名规范。

**规则**：
- **必须**使用 PascalCase（如 `UserId`、`OrderId`）
- **禁止**使用 snake_case（如 `user_id`）
- **禁止**使用全小写（如 `userid`）
- **禁止**使用全大写（如 `USER_ID`）

**标准字段命名**：
- `CorrelationId`、`UserId`、`TraceId`、`SpanId`
- `ServiceName`、`Environment`、`OrderId`

**判定**：
- ❌ 字段命名 `user_id` 或 `userid`
- ✅ 字段命名 `UserId`

---

### ADR-350.4：错误日志必须包含异常详情【必须架构测试覆盖】

错误级别的日志**必须**包含完整的异常信息。

**规则**：
- **必须**传递 Exception 对象给日志方法
- **必须**包含 ExceptionType（异常类型名）
- **必须**包含 ExceptionMessage（异常消息）
- **必须**包含 StackTrace（堆栈跟踪）
- **必须**包含 InnerException（如有）

**判定**：
- ❌ `_logger.LogError("Failed to create order {OrderId}", orderId)` - 缺少 Exception
- ✅ `_logger.LogError(exception, "Failed to create order {OrderId}", orderId)` - 包含 Exception

**推荐实现**：使用 Roslyn Analyzer 禁止 `LogError(string)` 签名。

---

### ADR-350.5：日志级别使用必须符合标准【必须架构测试覆盖】

日志级别**必须**按标准定义使用。

**规则**：
- **Trace**：详细调试信息（生产环境禁用）
- **Debug**：调试信息（生产环境禁用）
- **Information**：一般信息（如请求开始/结束）
- **Warning**：警告但不影响功能
- **Error**：错误导致操作失败
- **Critical**：严重错误影响系统运行

**判定**：
- ❌ 使用 Information 记录错误
- ❌ 使用 Error 记录正常业务流程
- ❌ 过度使用 Critical（仅用于系统级故障）
- ✅ 级别使用符合定义

---

---

## Enforcement（执法模型）

### 测试实现

**ADR-350.1 实现**：
- 架构测试：扫描日志调用验证 CorrelationId 参数
- Serilog Enricher：全局注入 CorrelationId（推荐）
- 降低遗漏风险

**ADR-350.4 实现**：
- Roslyn Analyzer：检测 `LogError(string)` 模式并报编译错误
- 强制使用 `LogError(Exception, string)` 确保异常完整

**其他规则**：
- L2 测试通过 Code Review 执行
- 敏感信息扫描工具辅助检查

---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- 待补充

---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-340：结构化日志与监控约束](./ADR-340-structured-logging-monitoring-constraints.md) - 可观测性标准基于结构化日志

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-970：自动化工具日志集成标准](../governance/ADR-970-automation-log-integration-standard.md) - 自动化工具日志与应用日志相关

---

---

## References（非裁决性参考）

### 相关 ADR
- ADR-340：结构化日志与监控约束

### 技术资源
- [Structured Logging](https://messagetemplates.org/)
- [OpenTelemetry Specification](https://opentelemetry.io/docs/reference/specification/)

### 实践指导
- 日志配置示例参见 `docs/copilot/adr-0350.prompts.md`（待创建）

---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
