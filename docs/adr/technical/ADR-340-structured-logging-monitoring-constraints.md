# ADR-340：结构化日志与监控约束

**状态**：✅ Final  
**级别**：技术方案层约束（Technical Constraint）  
**适用范围**：Platform 层、所有运行时环境（Host.Web、Host.Worker）  
**生效时间**：即刻  
**依赖 ADR**：ADR-0002（Platform/Application/Host 启动体系）

---

## 执行边界声明

> **本 ADR 中的 L1 规则仅验证结构与依赖的最低保证，不等价于完整运行时效果验证。**
>
> **L1（NetArchTest）可验证**：依赖引用、命名空间使用、类型依赖  
> **L2（Roslyn Analyzer）可验证**：代码语义、方法调用、字符串处理  
> **L3（人工审查）需要**：运行时行为、配置完整性、业务语义

---

## 规则本体（Rule）

> **本 ADR 仅包含可自动判定的裁决性规则。**

### ADR-340.1：Platform 层必须引用日志基础设施包【必须架构测试覆盖】

Platform 层**必须**引用结构化日志和监控的核心包。

**强制要求**：

- 必须引用 `Serilog.AspNetCore`
- 必须引用 `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- 必须引用 `OpenTelemetry.Extensions.Hosting`
- 必须引用 `OpenTelemetry.Instrumentation.AspNetCore`
- 必须引用 `OpenTelemetry.Instrumentation.Http`
- 必须引用 `OpenTelemetry.Instrumentation.Runtime`

**执行级别**：L1（NetArchTest - 依赖验证）

**注意**：此规则仅验证包引用，不保证实际配置。配置完整性由 L3（代码审查）保障。

---

### ADR-340.2：PlatformBootstrapper 必须包含日志配置代码【必须架构测试覆盖】

`PlatformBootstrapper.cs` 文件**必须**包含 Serilog 和 OpenTelemetry 相关配置代码。

**强制要求**：

- 文件必须引用 `Serilog` 命名空间
- 文件必须包含 `Log.Logger` 或 `LoggerConfiguration` 使用
- 文件必须包含 `AddOpenTelemetry` 方法调用
- 文件必须包含 `WithTracing` 和 `WithMetrics` 配置

**执行级别**：L1（文本匹配 - 最低保证）

**注意**：此规则仅验证代码存在，不验证配置正确性。配置语义由 L3（代码审查）保障。

---

### ADR-340.3：Handler 禁止使用 Console 输出【必须架构测试覆盖】

所有 Handler（Command/Query/Event）**禁止**使用控制台输出方法。

**禁止行为**：

- 调用 `Console.WriteLine()`
- 调用 `Console.Write()`
- 调用 `Debug.WriteLine()`
- 调用 `Trace.WriteLine()`

**执行级别**：L2（Roslyn Analyzer - 待实现）

**当前状态**：⚠️ Analyzer 未实现，此规则作为设计约束与 Code Review Gate，不参与 CI 阻断。

---

### ADR-340.4：日志调用禁止使用字符串插值【必须架构测试覆盖】

日志记录方法调用**禁止**使用字符串插值（`$"..."`）。

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

**执行级别**：L2（Roslyn Analyzer - 待实现）

**当前状态**：⚠️ Analyzer 未实现，此规则作为设计约束与 Code Review Gate，不参与 CI 阻断。

---

### ADR-340.5：禁止在 Application 或 Modules 层直接引用日志实现【必须架构测试覆盖】

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

| 规则编号 | 执行级 | 验证方式 | CI 阻断 | 测试方法 |
|---------|-------|---------|---------|---------|
| ADR-340.1 | L1 | 依赖引用检查 | ✅ 是 | `Platform_Must_Reference_Logging_Packages()` |
| ADR-340.2 | L1 | 文本匹配检查 | ✅ 是 | `PlatformBootstrapper_Must_Contain_Logging_Configuration()` |
| ADR-340.3 | L2 | Roslyn Analyzer | ❌ 否（待实现） | （未实现） |
| ADR-340.4 | L2 | Roslyn Analyzer | ❌ 否（待实现） | （未实现） |
| ADR-340.5 | L1 | 依赖隔离检查 | ✅ 是 | `Modules_Cannot_Reference_Logging_Implementation()` |

**执行能力说明**：

- **L1（当前可执行）**：ADR-340.1、340.2、340.5 可通过 NetArchTest 和文本检查立即验证
- **L2（待实现）**：ADR-340.3、340.4 需要 Roslyn Analyzer，当前作为 Review Gate
- **L3（人工审查）**：配置语义完整性、运行时行为验证需要 Code Review

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
