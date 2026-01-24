# ADR-340：结构化日志与监控约束

**状态**：✅ Final  
**级别**：技术方案层约束（Technical Constraint）  
**适用范围**：Platform 层、所有运行时环境（Host.Web、Host.Worker）  
**生效时间**：即刻  
**依赖 ADR**：ADR-0002（Platform/Application/Host 启动体系）

---

## 规则本体（Rule）

> **本 ADR 仅包含可自动判定的裁决性规则。**

### ADR-340.1：Platform 层必须配置结构化日志【必须架构测试覆盖】

Platform 层**必须**通过 `PlatformBootstrapper` 配置结构化日志组件。

**强制要求**：

- 必须使用 Serilog 作为日志实现
- 必须在 `PlatformBootstrapper.Configure()` 中完成配置
- 必须配置至少一个 Sink（控制台或文件）

**执行级别**：L1（NetArchTest）

---

### ADR-340.2：Platform 层必须配置 OpenTelemetry【必须架构测试覆盖】

Platform 层**必须**通过 `PlatformBootstrapper` 配置 OpenTelemetry 追踪和指标。

**强制要求**：

- 必须配置 OpenTelemetry 追踪（Tracing）
- 必须配置 OpenTelemetry 指标（Metrics）
- 必须启用 AspNetCore、Http、Runtime 插桩

**执行级别**：L1（NetArchTest）

---

### ADR-340.3：Handler 必须使用结构化日志【必须架构测试覆盖】

所有 Handler（Command/Query/Event）**必须**使用结构化日志记录关键操作。

**强制要求**：

- Handler 必须注入 `ILogger<T>` 或使用静态日志上下文
- 禁止使用 `Console.WriteLine()` 或 `Debug.WriteLine()`
- 禁止使用字符串拼接的日志消息

**执行级别**：L2（Roslyn Analyzer）

---

### ADR-340.4：日志消息必须使用消息模板【必须架构测试覆盖】

日志记录**必须**使用消息模板（Message Templates），而非字符串插值。

**正确示例**：

```csharp
// ✅ 正确：使用消息模板
_logger.LogInformation("处理订单 {OrderId} 完成，会员 {MemberId}", orderId, memberId);
```

**违规示例**：

```csharp
// ❌ 禁止：字符串插值
_logger.LogInformation($"处理订单 {orderId} 完成，会员 {memberId}");

// ❌ 禁止：字符串拼接
_logger.LogInformation("处理订单 " + orderId + " 完成");
```

**执行级别**：L2（Roslyn Analyzer）

---

### ADR-340.5：禁止在 Application 或 Modules 层直接配置日志【必须架构测试覆盖】

Application 层和 Modules 层**禁止**配置日志框架或 OpenTelemetry。

**仅允许使用**：

- 注入 `ILogger<T>` 接口
- 使用日志方法（LogInformation、LogWarning 等）

**禁止行为**：

- 调用 Serilog 配置方法（如 `WriteTo.*`）
- 调用 OpenTelemetry 配置方法（如 `AddOpenTelemetry*`）
- 直接依赖 Serilog 或 OpenTelemetry 具体类型

**执行级别**：L1（NetArchTest）

---

## 执法模型（Enforcement）

| 规则编号 | 执行级 | 测试/手段 |
|---------|-------|----------|
| ADR-340.1 | L1 | `ADR_0340_Architecture_Tests.Platform_Must_Configure_Serilog()` |
| ADR-340.2 | L1 | `ADR_0340_Architecture_Tests.Platform_Must_Configure_OpenTelemetry()` |
| ADR-340.3 | L2 | Roslyn Analyzer（待实现） |
| ADR-340.4 | L2 | Roslyn Analyzer（待实现） |
| ADR-340.5 | L1 | `ADR_0340_Architecture_Tests.Modules_Cannot_Configure_Logging()` |

---

## 破例与归还（Exception）

### 允许破例的前提

- 遗留系统集成，无法迁移到结构化日志
- 第三方库强制的日志配置模式
- 特殊诊断场景（临时性）

### 破例要求

每个破例**必须**：

- 记录在 `ARCH-VIOLATIONS.md`
- 指明 ADR-340 + 具体规则编号
- 指定失效日期（不超过 6 个月）
- 给出归还计划和责任人

---

## 变更政策（Change Policy）

- 本 ADR 为技术方案层约束，可通过 RFC 流程修订
- 技术选型变更（如替换 Serilog）需评估架构测试影响
- 新增规则必须满足可自动判定性
- 规则修改需架构委员会评审

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：

- 日志消息的具体内容和格式
- 日志级别使用的详细规则（何时用 Info vs Warning）
- Serilog Sink 的具体配置参数（如文件滚动策略）
- OpenTelemetry 导出器的具体配置（OTLP endpoint 地址）
- 日志保留策略和存储方案
- 监控告警阈值和规则
- 追踪采样率和策略
- 日志字段的命名约定
- Correlation ID 的生成和传播机制

> 上述内容属于工程标准与实践指南，参见《结构化日志与监控工程标准》。

---

## 非裁决性参考（References）

- [ADR-0002：Platform/Application/Host 启动体系](../constitutional/ADR-0002-platform-application-host-bootstrap.md)
- [结构化日志与监控工程标准](../../guides/structured-logging-monitoring-standard.md)（非裁决性）
- [Serilog 官方文档](https://serilog.net/)（技术参考）
- [OpenTelemetry .NET 文档](https://opentelemetry.io/docs/languages/net/)（技术参考）

---

## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 | 影响级别 |
|------|------|----------|--------|---------|
| 1.0 | 2026-01-24 | 初始版本，定义结构化日志和监控约束 | @copilot | High |
