# 后端开发指令

> ⚠️ **权威声明与冲突处理**：参阅 [base.instructions.md](base.instructions.md) 顶部的权威声明和末尾的治理协同章节。

## 适用场景：后端/业务逻辑开发

在协助后端开发时，在 `base.instructions.md` 的基础上应用这些额外约束。

## 权威依据

本文档服从以下 ADR：
- ADR-0001：模块化单体与垂直切片架构
- ADR-0005：应用内交互模型与执行边界

**冲突裁决**：若本文档与 ADR 正文冲突，以 ADR 正文为准。

---

## 引用优先级（Reference Priority）

后端开发时按以下顺序引用：

1. **ADR 正文** - 架构约束的唯一依据
   - ADR-0001：模块隔离和垂直切片
   - ADR-0005：Handler 规则和 CQRS

2. **架构测试** - 强制验证机制
   - `ADR_0001_Architecture_Tests.cs`
   - `ADR_0005_Architecture_Tests.cs`

3. **Copilot Prompts** - 场景实施指南
   - `docs/copilot/adr-0001.prompts.md` - 模块隔离场景
   - `docs/copilot/adr-0005.prompts.md` - Handler 模式和 CQRS 场景

---

## ⚖️ 权威提醒

所有后端开发约束基于以下 **ADR 正文**：

- `ADR-0001-modular-monolith-vertical-slice-architecture.md` - 模块隔离和垂直切片
- `ADR-0005-Application-Interaction-Model-Final.md` - Handler 规则和 CQRS

引用规则时，必须以 ADR 正文为准，Prompt 文件仅为辅助理解。

---

## 垂直切片组织边界

**要求**：每个业务用例必须组织为完整的垂直切片

**禁止模式**：
- ❌ 水平 Service 层（如 `OrderService`）
- ❌ 跨用例共享业务逻辑
- ❌ 包含业务逻辑的通用 `Manager` 或 `Helper` 类

**组织规则**：详见 ADR-0001 第 3.2 节  
**实施示例**：参阅 `docs/copilot/adr-0001.prompts.md`

---

## Handler 规则边界（ADR-0005）

### Command Handler 边界

**必须遵守**：
- 返回 `void` 或仅返回 ID（Guid、int、string）
- 不得返回业务数据（使用单独的 Query）
- 不得依赖契约（DTO）进行业务决策
- 加载领域模型、执行业务逻辑、保存状态
- 可以发布领域事件

**禁止模式**：
- ❌ Command Handler 返回 DTO
- ❌ Command Handler 依赖契约做业务决策

**详细规则**：参阅 ADR-0005 第 2.1 节  
**实施示例**：参阅 `docs/copilot/adr-0005.prompts.md`

### Query Handler 边界

**必须遵守**：
- 返回契约（DTO）
- 不得修改状态
- 不得发布事件
- 可以优化读取性能
- 可以跨模块边界查询（通过契约）

**详细规则**：参阅 ADR-0005 第 2.2 节

---

## Endpoint 规则边界

**要求**：Endpoint 必须是薄适配器

**禁止行为**：
- ❌ 业务逻辑或验证
- ❌ 直接访问数据库
- ❌ 直接操作领域模型

**详细规则**：参阅 ADR-0005 第 3 节  
**实施示例**：参阅 `docs/copilot/adr-0005.prompts.md`

---

## 模块通信边界

当一个模块需要来自另一个模块的数据/通知时：

**允许的通信方式**：
- ✅ 领域事件（异步）
- ✅ 契约查询（只读 DTO）
- ✅ 原始类型（ID）

**禁止的通信方式**：
- ❌ 直接引用其他模块的内部实现
- ❌ 同步跨模块命令

**详细规则**：参阅 ADR-0001 第 2.2 节  
**实施示例**：参阅 `docs/copilot/adr-0001.prompts.md`（场景 3）

---

## 领域模型边界

**要求**：业务逻辑必须在领域模型中，而非 Handler 或 Service

**详细规则**：参阅 ADR-0001 第 3.3 节  
**实施示例**：参阅 `docs/copilot/adr-0001.prompts.md`

---

## 常见场景引导

| 开发者问...             | 引导查阅...                                           |
|---------------------|---------------------------------------------------|
| "我需要调用另一个模块的逻辑"     | ADR-0001 第 2.2 节，`docs/copilot/adr-0001.prompts.md` 场景 3 |
| "我需要在模块间共享代码"       | ADR-0001 第 3.1 节（BuildingBlocks 边界）          |
| "我需要从命令返回数据"        | ADR-0005 第 2.1 节，`docs/copilot/adr-0005.prompts.md` |
| "我需要使用另一个模块的数据进行验证" | ADR-0001 第 2.2 节（契约查询）                         |

---

## 危险信号检查清单

发现以下情况时停止并警告：

- 🚩 在另一个模块中出现 `using Zss.BilliardHall.Modules.X`
- 🚩 模块中出现 `class OrderService` 或任何 `*Service`
- 🚩 Command Handler 返回 DTO
- 🚩 Query Handler 修改状态
- 🚩 Endpoint 中的业务逻辑
- 🚩 模块间共享的领域模型

**检测到危险信号时**：
1. 指出违反的 ADR 和章节
2. 明确标识为 ⚠️ **Blocked**（基于 ADR-0007 三态输出规则）
3. 引导查阅相关 Prompts 文件了解正确模式
4. 不直接给出实施代码

> 📌 **三态输出规则**：所有诊断输出必须明确使用 `✅ Allowed / ⚠️ Blocked / ❓ Uncertain`，并始终注明"以 ADR-0007 和相关 ADR 正文为最终权威"。

---

## 参考

详细场景和实施示例：

- `docs/copilot/adr-0001.prompts.md` - 模块隔离
- `docs/copilot/adr-0005.prompts.md` - Handler 模式和 CQRS
