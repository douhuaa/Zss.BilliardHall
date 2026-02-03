---
adr: ADR-340
title: "结构化日志与监控约束"
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


# ADR-340：结构化日志与监控约束

**影响范围**：Platform 层、所有运行时环境（Host.Web、Host.Worker）  
## Focus（聚焦内容）

- Platform 层日志基础设施包引用约束
- 日志配置代码结构要求
- Handler 层日志输出规范
- 日志与监控的层级隔离规则

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
> ADR-340 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-340_<Rule>_<Clause>
> ```

---

### ADR-340_1：Platform 层日志基础设施（Rule）

#### ADR-340_1_1 Platform 层必须引用日志基础设施包【必须架构测试覆盖】

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

#### ADR-340_1_2 PlatformBootstrapper 必须包含日志配置【必须架构测试覆盖】

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

---

### ADR-340_2：业务层日志约束（Rule）

#### ADR-340_2_1 Handler 禁止使用控制台输出【必须架构测试覆盖】

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

#### ADR-340_2_2 日志调用禁止字符串插值【必须架构测试覆盖】

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

#### ADR-340_2_3 禁止业务层配置日志实现【必须架构测试覆盖】

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

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-340 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-340_1_1** | L1 | ArchitectureTests 验证 Platform 包引用 | §ADR-340_1_1 |
| **ADR-340_1_2** | L1 | ArchitectureTests 验证配置代码存在 | §ADR-340_1_2 |
| **ADR-340_2_1** | L2 | Code Review 检查控制台输出 | §ADR-340_2_1 |
| **ADR-340_2_2** | L2 | Code Review 检查字符串插值 | §ADR-340_2_2 |
| **ADR-340_2_3** | L1 | ArchitectureTests 验证层级隔离 | §ADR-340_2_3 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决
- PlatformBootstrapper 缺少配置代码 → CI 失败
- Application/Modules 引用日志实现包 → CI 失败

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
- [ADR-002：平台、应用与主机启动器架构](../constitutional/ADR-002-platform-application-host-bootstrap.md) - 日志架构基于三层体系
- [ADR-006：术语与编号宪法](../constitutional/ADR-006-terminology-numbering-constitution.md) - 日志术语遵循统一规范

**被依赖（Depended By）**：
- [ADR-350：日志可观测性标准](./ADR-350-logging-observability-standards.md) - 可观测性基于结构化日志

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- 无

---

---

## References（非裁决性参考）

### 相关 ADR
- ADR-002：Platform/Application/Host 启动体系
- ADR-350：日志与可观测性字段标准

### 技术资源
- [Serilog 官方文档](https://serilog.net/)
- [OpenTelemetry .NET 文档](https://opentelemetry.io/docs/languages/net/)

### 实践指导
- 详细配置示例参见 `docs/copilot/adr-340.prompts.md`（待创建）

---

---

## History（版本历史）

| 版本  | 日期         | 变更说明   | 修订人 |
|-----|------------|--------|-------|
| 3.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
| 2.0 | 2026-01-26 | 更新版本 | Architecture Board |
| 1.0 | 2026-01-29 | 初始版本 | Architecture Board |
