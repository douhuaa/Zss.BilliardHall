# ADR-350：日志与可观测性字段标准

**状态**：✅ Accepted  
**级别**：技术层  
**影响范围**：所有日志输出、结构化日志字段  
**生效时间**：待审批通过后

---

## 聚焦内容（Focus）

- 日志字段命名规范与一致性约束
- 敏感信息日志保护规则
- 错误日志必需字段要求
- 日志级别使用标准定义
- CorrelationId 传播约束

---

## 决策（Decision）

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

## 快速参考表

| 约束编号       | 约束描述                | 测试方式             | 必须遵守 |
|------------|---------------------|------------------|------|
| ADR-350.1 | 请求日志必须包含 CorrelationId | L1 - Enricher 强制 + 扫描 | ✅    |
| ADR-350.2 | 禁止记录敏感信息 | L2 - 自动扫描 + Code Review | ✅    |
| ADR-350.3 | 字段命名必须 PascalCase | L1 - 日志输出扫描 | ✅    |
| ADR-350.4 | 错误日志必须包含异常 | L1 - Roslyn Analyzer | ✅    |
| ADR-350.5 | 日志级别使用符合标准 | L2 - Code Review | ✅    |

---

## 必测/必拦架构测试（Enforcement）

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

## 破例与归还（Exception）

### 允许破例的前提

破例**仅在以下情况允许**：

1. **第三方库日志**：无法控制的第三方输出
2. **遗留系统集成**：外部系统要求特定格式
3. **性能关键路径**：经过压测证明日志导致性能问题

### 破例要求（不可省略）

每个破例**必须**：

- 记录在 `docs/summaries/arch-violations.md`
- 标明 ADR-350 + 具体规则编号
- 说明技术原因和风险评估

---

## 变更政策（Change Policy）

### 变更规则

* **技术层 ADR**
  * 修改需 Tech Lead 审批
  * 日志框架升级可触发更新
  * 字段命名标准调整需全局影响评估

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：

- ✗ 日志框架选择（Serilog/NLog）
- ✗ 日志聚合工具（ELK/Splunk）
- ✗ 日志保留策略和存储方案
- ✗ 日志采样率配置
- ✗ APM 工具集成
- ✗ 日志消息的具体格式和模板
- ✗ 监控指标定义和采集规则

---

## 非裁决性参考（References）

### 相关 ADR
- ADR-340：结构化日志与监控约束

### 技术资源
- [Structured Logging](https://messagetemplates.org/)
- [OpenTelemetry Specification](https://opentelemetry.io/docs/reference/specification/)

### 实践指导
- 日志配置示例参见 `docs/copilot/adr-0350.prompts.md`（待创建）

---


## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0340：结构化日志与监控约束](./ADR-340-structured-logging-monitoring-constraints.md) - 可观测性标准基于结构化日志

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0970：自动化工具日志集成标准](../governance/ADR-970-automation-log-integration-standard.md) - 自动化工具日志与应用日志相关

---


## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 |
|-----|------|---------|--------|
| 2.0 | 2026-01-26 | 裁决型重构，添加决策章节，移除冗余示例和说明 | GitHub Copilot |
| 1.0 Draft | 2026-01-24 | 初始版本 | GitHub Copilot |

---

## 附注

本文件禁止添加示例代码、配置详情、背景说明，仅维护自动化可判定的架构约束。

非裁决性参考（详细配置示例、常见问题、技术选型讨论）请查阅：
- [ADR-350 Copilot Prompts](../../copilot/adr-0350.prompts.md)（待创建）
- 工程标准（如有）
