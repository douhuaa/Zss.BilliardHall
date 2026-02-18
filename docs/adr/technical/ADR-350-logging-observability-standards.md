---
adr: ADR-350
title: "日志与可观测性字段标准"
status: Final
level: Technical
version: "3.0"
deciders: "Architecture Board"
date: 2026-02-03
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

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-350 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-350_<Rule>_<Clause>
> ```

---

### ADR-350.1：请求日志标准（Rule）

#### ADR-350.1.1：请求日志必须包含 CorrelationId【必须架构测试覆盖】

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

#### ADR-350.1.2：日志字段必须使用 PascalCase 命名【必须架构测试覆盖】

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

---

### ADR-350.2：敏感信息保护与错误处理（Rule）

#### ADR-350.2.1：禁止记录敏感信息【必须架构测试覆盖】

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

#### ADR-350.2.2：错误日志必须包含异常详情【必须架构测试覆盖】

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

#### ADR-350.2.3：日志级别使用必须符合标准【必须架构测试覆盖】

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

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-350 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-350.1.1** | L1 | ArchitectureTests 验证 CorrelationId 参数 | §ADR-350.1.1 |
| **ADR-350.1.2** | L1 | ArchitectureTests 验证字段命名规范 | §ADR-350.1.2 |
| **ADR-350.2.1** | L2 | Code Review + 敏感信息扫描工具 | §ADR-350.2.1 |
| **ADR-350.2.2** | L1 | Roslyn Analyzer 强制异常参数 | §ADR-350.2.2 |
| **ADR-350.2.3** | L2 | Code Review 检查日志级别使用 | §ADR-350.2.3 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决

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
- 日志配置示例参见 `docs/copilot/adr-350.prompts.md`（待创建）

---

---

## History（版本历史）

| 版本  | 日期         | 变更说明   | 修订人 |
|-----|------------|--------|-------|
| 3.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
| 2.0 | 2026-01-26 | 更新版本 | Architecture Board |
| 1.0 | 2026-01-29 | 初始版本 | Architecture Board |
