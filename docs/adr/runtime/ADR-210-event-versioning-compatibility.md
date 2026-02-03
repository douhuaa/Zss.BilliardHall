---
adr: ADR-210
title: "领域事件版本化与兼容性"
status: Final
level: Runtime
version: "2.0"
deciders: "Architecture Board"
date: 2026-01-25
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---


# ADR-210：领域事件版本化与兼容性

> ⚖️ **本 ADR 定义领域事件的版本管理规则，确保跨版本兼容性和系统稳定性。**

**适用范围**：所有领域事件  
## Focus（聚焦内容）

- 事件破坏性变更与版本控制
- SchemaVersion 属性要求
- 旧版本事件保留策略
- 订阅者多版本处理要求
- 事件序列化兼容性
- 版本异常容错机制

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|-----|------|---------|
| 破坏性变更 | 导致旧版本无法正常处理的事件变更 | Breaking Change |
| SchemaVersion | 事件中标识版本号的属性 | Schema Version |
| 语义化版本 | Major.Minor 格式的版本号体系 | Semantic Versioning |
| 向前兼容 | 新代码能读取旧数据 | Forward Compatibility |
| 活跃版本 | 当前系统中仍在使用的事件版本 | Active Version |
| 死信队列 | 存储无法处理消息的队列 | Dead Letter Queue |
| Fallback Handler | 处理未知版本事件的兜底处理器 | Fallback Handler |

---

---

## Decision（裁决）

### 破坏性变更必须创建新版本（ADR-210.1）【必须架构测试覆盖】

**规则**：
- 破坏性变更定义：
  - ❌ 删除字段
  - ❌ 修改字段类型
  - ❌ 重命名字段
  - ❌ 添加必需字段（无默认值）
  
- 非破坏性变更：
  - ✅ 添加可选字段（有默认值）
  - ✅ 添加新的可选属性
  
- 版本命名：
  - ✅ `OrderCreated` → `OrderCreatedV2` → `OrderCreatedV3`
  - ❌ `OrderCreated2`、`OrderCreated_New`（不规范）

**判定**：
- ❌ 删除或修改现有事件字段
- ❌ 重命名事件类型而不创建新版本
- ❌ 添加必需字段无默认值
- ❌ 版本命名不符合规范
- ✅ 破坏性变更创建新版本事件

### 事件必须包含 SchemaVersion（ADR-210.2）【必须架构测试覆盖】

**规则**：
- 所有领域事件必须包含 `SchemaVersion` 属性
- 版本号格式：语义化版本号（Major.Minor）
- 破坏性变更递增 Major
- 兼容性变更递增 Minor

**判定**：
- ❌ 事件类型无 `SchemaVersion` 属性
- ❌ 版本号格式不符合 Major.Minor
- ✅ 事件包含正确格式的 SchemaVersion

### 旧版本保留策略（ADR-210.3）【必须架构测试覆盖】

**规则**：
- 旧版本事件必须保持至少 2 个大版本周期
- V1 创建后，至少等到 V3 发布才能删除 V1
- 删除前至少一个版本标记为 `[Obsolete]`
- 禁止立即删除旧版本

**废弃流程**：
1. V2 发布时：`[Obsolete("Use OrderCreatedV2", false)]`
2. V3 发布时：`[Obsolete("Use OrderCreatedV2", true)]`（编译错误）
3. V4 发布时：可删除 V1

**判定**：
- ❌ 新版本发布后立即删除旧版本
- ❌ 删除未经 Obsolete 标记的版本
- ❌ 未满 2 个大版本周期删除
- ✅ 遵循废弃流程并满足保留周期

### 订阅者多版本处理（ADR-210.4）【必须架构测试覆盖】

**规则**：
- 订阅者必须处理所有活跃版本
- 处理策略：
  - ✅ 方式 1：为每个版本创建独立 Handler
  - ✅ 方式 2：在 Handler 内部转换为统一版本
- 禁止仅处理最新版本而忽略旧版本

**判定**：
- ❌ Handler 仅处理最新版本
- ❌ Handler 对旧版本抛出异常
- ✅ Handler 处理所有活跃版本

### 事件序列化兼容性（ADR-210.5）【必须架构测试覆盖】

**规则**：
- 序列化必须支持向前兼容（新代码读旧数据）
- 序列化要求：
  - ✅ 使用 JSON 作为默认格式
  - ✅ 忽略未知字段（反序列化时）
  - ✅ 为新字段提供默认值
  - ❌ 禁止遇到未知字段抛出异常

**判定**：
- ❌ 反序列化遇到未知字段抛出异常
- ❌ 新字段无默认值导致反序列化失败
- ✅ 序列化支持向前兼容

### 版本异常容错机制（ADR-210.6）【必须架构测试覆盖】

**规则**：
- 版本异常必须降级处理，不得中断消费
- 容错策略：
  - ✅ 未识别 SchemaVersion → Warning + Fallback Handler
  - ✅ 反序列化失败 → Error + 死信队列
  - ✅ 语义不兼容 → Warning + 旧版本逻辑
  - ❌ 禁止因版本异常停止消费者

**生产原则**：
- 事件系统第一原则：**不死**
- 版本错误是数据问题，不是系统故障
- 系统必须在版本异常时继续运行

**判定**：
- ❌ 版本异常导致消费者停止
- ❌ 版本异常直接抛出未处理
- ✅ 版本异常降级处理并记录日志
- ✅ 系统在版本异常时继续运行

---

---

## Enforcement（执法模型）


### 执行方式

待补充...


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
- [ADR-120：领域事件命名约定](../structure/ADR-120-domain-event-naming-convention.md) - 事件版本化基于事件命名规范
- [ADR-005：应用内交互模型与执行边界](../constitutional/ADR-005-Application-Interaction-Model-Final.md) - 事件版本化基于事件驱动模式

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-220：事件总线集成](./ADR-220-event-bus-integration.md) - 事件序列化和版本化相关

---

---

## References（非裁决性参考）

> **仅供理解，不具裁决力。**

### 相关 ADR
- [ADR-001：模块化单体与垂直切片架构](../constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)
- [ADR-120：领域事件命名规范](../structure/ADR-120-event-naming-standard.md)
- [ADR-220：事件总线集成规范](ADR-220-event-bus-integration.md)

### 技术资源
- [语义化版本规范](https://semver.org/lang/zh-CN/)
- [JSON Schema 文档](https://json-schema.org/)

### 实践指导
- 事件版本化详细示例参见 `docs/copilot/adr-210.prompts.md`

---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
