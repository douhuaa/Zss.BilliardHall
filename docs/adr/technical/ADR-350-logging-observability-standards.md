# ADR-350：日志与可观测性标签与字段标准

**状态**：✅ Accepted  
**级别**：技术层  
**影响范围**：所有日志输出  
**生效时间**：待审批通过后

---

## 规则本体（Rule）

> **这是本 ADR 唯一具有裁决力的部分。**

### ADR-350.1：所有请求日志必须包含 CorrelationId

所有与请求相关的日志**必须**包含 `CorrelationId` 字段。

**强制要求**：
- ✅ 每个请求生成唯一的 CorrelationId
- ✅ CorrelationId 在整个请求生命周期中传播
- ✅ 所有日志语句包含此字段
- ❌ 禁止遗漏 CorrelationId

**日志示例**：
```csharp
// ✅ 正确
_logger.LogInformation(
    "Order created {OrderId} {CorrelationId}",
    orderId, correlationId);

// ❌ 错误：缺少 CorrelationId
_logger.LogInformation("Order created {OrderId}", orderId);
```

### ADR-350.2：禁止记录敏感信息

日志**禁止**包含敏感信息。

**禁止记录的信息**：
- ❌ 密码、密钥、Token
- ❌ 信用卡号、身份证号
- ❌ 完整的个人隐私信息
- ❌ 加密密钥、API Secret

**允许记录**：
- ✅ 脱敏后的信息（如手机号 138****1234）
- ✅ 用户 ID（非 PII）
- ✅ 业务标识符

### ADR-350.3：结构化日志字段必须使用 PascalCase 命名

结构化日志的字段名**必须**使用 PascalCase 命名规范。

**命名规则**：
- ✅ `UserId`、`OrderId`、`CorrelationId`
- ❌ `user_id`（snake_case）
- ❌ `userid`（全小写）
- ❌ `USER_ID`（全大写）

**标准字段命名**：
- `CorrelationId`
- `UserId`
- `TraceId`
- `SpanId`
- `ServiceName`
- `Environment`

### ADR-350.4：错误日志必须包含异常详情

错误级别的日志**必须**包含完整的异常信息。

**必需字段**：
- ✅ `ExceptionType`（异常类型名）
- ✅ `ExceptionMessage`（异常消息）
- ✅ `StackTrace`（堆栈跟踪）
- ✅ `InnerException`（内部异常，如有）

**日志示例**：
```csharp
// ✅ 正确
_logger.LogError(exception,
    "Failed to create order {OrderId} {CorrelationId}",
    orderId, correlationId);

// ❌ 错误：未传递 exception 对象
_logger.LogError(
    "Failed to create order {OrderId}",
    orderId);
```

### ADR-350.5：日志级别使用必须符合标准定义

日志级别**必须**按标准定义使用。

**级别定义**：
- **Trace**：详细的调试信息（生产环境禁用）
- **Debug**：调试信息（生产环境禁用）
- **Information**：一般信息（如请求开始/结束）
- **Warning**：警告但不影响功能
- **Error**：错误导致操作失败
- **Critical**：严重错误影响系统运行

**禁止行为**：
- ❌ 使用 Information 记录错误
- ❌ 使用 Error 记录正常业务流程
- ❌ 过度使用 Critical（仅用于系统级故障）

---

## 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 测试映射

| 规则编号 | 执行级 | 测试/手段 |
|---------|--------|----------|
| ADR-350.1 | L1 | `Logs_Must_Include_CorrelationId` + Serilog Enricher 强制 |
| ADR-350.2 | L2 | Code Review + 自动扫描 |
| ADR-350.3 | L1 | `Log_Fields_Must_Use_PascalCase` |
| ADR-350.4 | L1 | Roslyn Analyzer: 禁止 `LogError(string)` 仅允许 `LogError(exception, string)` |
| ADR-350.5 | L2 | Code Review |

### 架构测试说明

**ADR-350.1 L1 实现**：
- 架构测试：扫描代码中的 `_logger.Log*(` 调用，验证包含 CorrelationId 参数
- Serilog Enricher：全局注入 CorrelationId，无需手动传递
- 推荐使用 Enricher 而非手动，降低遗漏风险

**ADR-350.4 L1 实现**：
- Roslyn Analyzer：检测 `LogError(string)` 模式并报编译错误
- 强制使用 `LogError(Exception, string)` 确保异常信息完整

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例**仅在以下情况允许**：

1. **第三方库日志**：无法控制的第三方输出
2. **遗留系统集成**：外部系统要求特定格式
3. **性能关键路径**：经过压测证明日志导致性能问题

### 破例要求（不可省略）

每个破例**必须**：

- 记录在 `docs/summaries/arch-violations.md`
- 说明技术原因
- 提供风险评估

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

* **技术层 ADR**
  * 修改需 Tech Lead 审批
  * 日志框架升级可触发更新

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

- ✗ 日志框架选择（Serilog/NLog）
- ✗ 日志聚合工具（ELK/Splunk）
- ✗ 日志保留策略
- ✗ 日志采样率配置
- ✗ APM 工具集成

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 相关 ADR
- ADR-340：结构化日志与监控约束

### 技术资源
- [Structured Logging](https://messagetemplates.org/)
- [OpenTelemetry Specification](https://opentelemetry.io/docs/reference/specification/)

### 实践指导
- 日志配置示例参见 `docs/copilot/adr-0350.prompts.md`

---

## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 |
|-----|------|---------|--------|
| 1.0 Draft | 2026-01-24 | 初始版本 | GitHub Copilot |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
