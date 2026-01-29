---
adr: ADR-340
title: "结构化日志与监控约束"
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

# ADR-340：结构化日志与监控约束

**影响范围**：Platform 层、所有运行时环境（Host.Web、Host.Worker）  
**生效时间**：即刻

---

## Focus（聚焦内容）

- Platform 层日志基础设施包引用约束
- 日志配置代码结构要求
- Handler 层日志输出规范
- 日志与监控的层级隔离规则

---

## Decision（裁决）

### ADR-340.1：Platform 层必须引用日志基础设施包【必须架构测试覆盖】

Platform 层**必须**引用结构化日志和监控的核心包。

**规则**：
- **必须**引用 `Serilog.AspNetCore`
- **必须**引用 `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- **必须**引用 `OpenTelemetry.Extensions.Hosting`
- **必须**引用 `OpenTelemetry.Instrumentation.AspNetCore`
- **必须**引用 `OpenTelemetry.Instrumentation.Http`
- **必须**引用 `OpenTelemetry.Instrumentation.Runtime`

**判定**：
- ❌ Platform 层缺少上述任一包引用
- ✅ Platform 层引用全部必需包

**说明**：此规则验证包引用，配置完整性由 Code Review 保障。

---

### ADR-340.2：PlatformBootstrapper 必须包含日志配置【必须架构测试覆盖】

`PlatformBootstrapper.cs` 文件**必须**包含 Serilog 和 OpenTelemetry 配置代码。

**规则**：
- 文件**必须**引用 `Serilog` 命名空间
- 文件**必须**包含 `Log.Logger` 或 `LoggerConfiguration`
- 文件**必须**包含 `AddOpenTelemetry` 方法调用
- 文件**必须**包含 `WithTracing` 和 `WithMetrics` 配置

**判定**：
- ❌ 文件缺少上述任一配置代码
- ✅ 文件包含全部必需配置

**说明**：此规则验证代码存在性，配置语义由 Code Review 保障。

---

### ADR-340.3：Handler 禁止使用控制台输出【必须架构测试覆盖】

所有 Handler（Command/Query/Event）**禁止**使用控制台输出。

**规则**：
- **禁止**调用 `Console.WriteLine()`
- **禁止**调用 `Console.Write()`
- **禁止**调用 `Debug.WriteLine()`
- **禁止**调用 `Trace.WriteLine()`

**判定**：
- ❌ Handler 代码包含上述任一方法调用
- ✅ Handler 仅使用 `ILogger<T>` 接口

**当前状态**：⚠️ Roslyn Analyzer 未实现，作为 Code Review Gate。

---

### ADR-340.4：日志调用禁止字符串插值【必须架构测试覆盖】

日志记录方法**禁止**使用字符串插值。

**规则**：
- **必须**使用消息模板（Message Template）
- **禁止**使用字符串插值（`$"..."`）
- **禁止**使用字符串拼接（`"..." + ...`）

**判定**：
- ❌ 日志调用使用 `$"..."`
- ❌ 日志调用使用 `"..." + ...`
- ✅ 日志调用使用 `"... {Param} ...", param`

**当前状态**：⚠️ Roslyn Analyzer 未实现，作为 Code Review Gate。

---

### ADR-340.5：禁止业务层配置日志实现【必须架构测试覆盖】

Application 层和 Modules 层**禁止**配置日志框架或 OpenTelemetry。

**规则**：
- **仅允许**注入 `ILogger<T>` 接口
- **仅允许**使用日志方法（LogInformation 等）
- **禁止**调用 Serilog 配置方法
- **禁止**调用 OpenTelemetry 配置方法
- **禁止**直接依赖 Serilog 或 OpenTelemetry 类型

**判定**：
- ❌ Application/Modules 引用 Serilog 配置包
- ❌ Application/Modules 调用 `AddOpenTelemetry*`
- ✅ Application/Modules 仅使用 `ILogger<T>`

---

## 快速参考表

| 约束编号       | 约束描述                | 测试方式             | CI 阻断 | 必须遵守 |
|------------|---------------------|------------------|---------|------|
| ADR-340.1 | Platform 层必须引用日志包 | L1 - 依赖检查 | ✅ 是 | ✅    |
| ADR-340.2 | PlatformBootstrapper 必须包含配置 | L1 - 文本检查 | ✅ 是 | ✅    |
| ADR-340.3 | Handler 禁止控制台输出 | L2 - Analyzer（待实现） | ❌ 否 | ✅    |
| ADR-340.4 | 日志禁止字符串插值 | L2 - Analyzer（待实现） | ❌ 否 | ✅    |
| ADR-340.5 | 业务层禁止配置日志实现 | L1 - 依赖检查 | ✅ 是 | ✅    |

> **级别说明**：L1=静态自动化（NetArchTest），L2=语义自动化（Roslyn Analyzer），L3=人工审查

---

## Enforcement（执法模型）

### 测试实现

**L1 当前可执行**：
- `Platform_Must_Reference_Logging_Packages()` - 验证 Platform 包引用
- `PlatformBootstrapper_Must_Contain_Logging_Configuration()` - 验证配置代码存在
- `Modules_Cannot_Reference_Logging_Implementation()` - 验证层级隔离

**L2 待实现（当前 Code Review）**：
- Handler 禁止控制台输出检查
- 日志字符串插值检查

**L3 人工审查**：
- 日志配置语义完整性验证
- 运行时日志输出行为验证
- 监控指标采集完整性验证

**CI 阻断条件**：
- Platform 层缺少必需包引用 → CI 失败
- PlatformBootstrapper 缺少配置代码 → CI 失败
- Application/Modules 引用日志实现包 → CI 失败

---

## 破例与归还（Exception）

### 允许破例的前提

破例**仅在以下情况允许**：

1. **遗留系统集成**：外部系统强制特定日志格式，无法迁移
2. **第三方库约束**：第三方库强制的日志配置模式
3. **特殊诊断场景**：临时性性能诊断或问题排查

### 破例要求（不可省略）

每个破例**必须**：

- 记录在 `docs/summaries/arch-violations.md`
- 标明 ADR-340 + 具体规则编号
- 指定失效日期（不超过 6 个月）
- 给出归还计划和责任人

---

## 变更政策（Change Policy）

### 变更规则

* **技术层 ADR**
  * 修改需 Tech Lead 审批
  * 技术选型变更（如替换 Serilog）需评估架构测试影响
  * 新增规则必须满足可自动判定性
  * 规则修改需架构委员会评审

### 失效与替代

* 本 ADR 被 Superseded 时**必须**指向新 ADR
* 日志框架升级需提供迁移指南

---

## Non-Goals（明确不管什么）

本 ADR **不负责**：

- ✗ 日志消息的具体内容和格式规范
- ✗ 日志级别使用详细规则（何时用 Info vs Warning）
- ✗ Serilog Sink 具体配置参数（文件滚动策略）
- ✗ OpenTelemetry 导出器具体配置（OTLP endpoint）
- ✗ 日志保留策略和存储方案
- ✗ 监控告警阈值和规则
- ✗ 追踪采样率和策略
- ✗ 日志字段命名约定（参见 ADR-350）
- ✗ Correlation ID 生成和传播机制

> 上述内容属于工程标准与实践指南，参见 ADR-350 和工程文档。

---

## References（非裁决性参考）

### 相关 ADR
- ADR-0002：Platform/Application/Host 启动体系
- ADR-350：日志与可观测性字段标准

### 技术资源
- [Serilog 官方文档](https://serilog.net/)
- [OpenTelemetry .NET 文档](https://opentelemetry.io/docs/languages/net/)

### 实践指导
- 详细配置示例参见 `docs/copilot/adr-0340.prompts.md`（待创建）

---


## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-0002：平台、应用与主机启动器架构](../constitutional/ADR-0002-platform-application-host-bootstrap.md) - 日志架构基于三层体系
- [ADR-0006：术语与编号宪法](../constitutional/ADR-0006-terminology-numbering-constitution.md) - 日志术语遵循统一规范

**被依赖（Depended By）**：
- [ADR-350：日志可观测性标准](./ADR-350-logging-observability-standards.md) - 可观测性基于结构化日志

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- 无

---


## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 | 影响级别 |
|------|------|----------|--------|---------|
| 2.0 | 2026-01-26 | 裁决型重构，添加决策章节，移除执行边界声明和冗余说明 | GitHub Copilot | High |
| 1.0 | 2026-01-24 | 初始版本，定义结构化日志和监控约束 | @copilot | High |

---

## 附注

本文件禁止添加示例代码、配置详情、背景说明，仅维护自动化可判定的架构约束。

非裁决性参考（详细配置示例、常见问题、技术选型讨论）请查阅：
- [ADR-340 Copilot Prompts](../../copilot/adr-0340.prompts.md)（待创建）
- 工程标准（如有）
