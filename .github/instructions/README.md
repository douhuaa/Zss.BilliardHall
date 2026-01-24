# GitHub Copilot 指令

> **⚠️ 权威声明**  
> 本目录下所有指令文件仅作操作/辅导用，权威判据以 ADR 正文为准。  
> 若指令文件与 ADR 正文存在分歧，请及时修订指令文件，并以 ADR 正文为最终依据。

本目录包含基于角色的 GitHub Copilot 指令，以确保与项目架构治理保持一致的行为。

---

## 目的

这些指令文件定义了**Copilot 在此仓库中的个性和行为边界**。它们回答："你是什么样的助手？"

这与 [`docs/copilot/`](../../docs/copilot/) 不同，后者包含**详细的工作手册**，回答："你如何帮助完成特定任务？"

---

## 结构

```
.github/instructions/
  ├─ README.md                            ← 本文件
  ├─ base.instructions.md                 ← 核心行为（始终激活）
  ├─ backend.instructions.md              ← 后端开发
  ├─ testing.instructions.md              ← 测试编写
  ├─ documentation.instructions.md        ← 文档编写
  └─ architecture-review.instructions.md  ← PR 评审（最高风险）
```

**文件更新说明**（2026-01-24）：
- ✅ 所有文件已添加权威声明和维护提醒
- ✅ 完善了所有 ADR 引用的章节锚点
- ✅ 加强了 ADR-0005-Enforcement-Levels.md 场景链接
- ✅ 优化了文件结构，前置高风险防御点
- ✅ 增强了文档索引同步检查清单

---

## 工作原理

### 基础指令（始终激活）

[`base.instructions.md`](./base.instructions.md) 建立 Copilot 的基本行为：
- 尊重 ADR 作为宪法级法律
- 将架构测试视为硬性约束
- 绝不发明规则或绕过 CI
- 放大理解能力，不替代理解

---

### 角色特定指令（根据上下文）

根据你的工作内容应用额外指令：

| 工作内容... | 额外指令 | 关键特征 |
|---------------|------------------------|----------|
| Handler、用例、领域模型 | [`backend.instructions.md`](./backend.instructions.md) | 🚨 高风险防御点前置，执行级别标注 |
| 单元测试、集成测试 | [`testing.instructions.md`](./testing.instructions.md) | 🚨 架构测试防御点，执行级别分类 |
| ADR、指南、Prompt | [`documentation.instructions.md`](./documentation.instructions.md) | 🚨 文档/索引同步检查清单 |
| PR 评审、架构评估 | [`architecture-review.instructions.md`](./architecture-review.instructions.md) | 🚨 最高风险场景，三级危险扫描 |

---

## 与 docs/copilot/ 的关系

这两个系统协同工作但目的不同：

| [`.github/instructions/`](./) | [`docs/copilot/`](../../docs/copilot/) |
|------------------------|-----------------|
| Copilot **是谁** | Copilot **如何**工作 |
| 个性与边界 | 具体程序 |
| 行为约束 | 详细场景 |
| 绝不做什么 | 何时做什么 |
| 很少变更 | 随经验演进 |

---

### 示例

**基础指令说**：
> "绝不引入跨模块依赖"

**Copilot Prompt（[adr-0001.prompts.md](../../docs/copilot/adr-0001.prompts.md)）说**：
> "当开发者想访问另一个模块时：
> - ✅ 使用领域事件：`await _eventBus.Publish(...)`
> - ✅ 使用契约：`await _queryBus.Send(...)`
> - ✅ 使用原始类型：`Guid memberId`
> - ❌ 不要使用直接引用"

---

## 何时更新

### ✅ 更新指令文件的时机：
- 项目采用新架构原则
- 基本约束发生变化
- 出现新的"绝不做这个"规则
- 某个模式变得足够常见需要正式化
- 角色边界需要澄清
- 风险等级发生变化

### ❌ 不要更新的情况：
- 具体示例（放入 [`docs/copilot/`](../../docs/copilot/)）
- 临时例外
- 个别用例

### 🔄 更新流程：
1. 确认变更与 ADR 正文一致
2. 同步架构负责人
3. 更新相关指令文件
4. 更新本 README.md
5. 进行团队公告
6. 更新相关的 [`docs/copilot/`](../../docs/copilot/) 辅导材料

---

## 三层架构治理

```
┌─────────────────────────────────────────────────┐
│ 第 1 层：ADR（docs/adr/）                        │
│ - 宪法级法律                                     │
│ - 架构决策记录                                   │
│ - 最高权威                                       │
│ - 包含执行级别分类（Level 1/2/3）                │
└─────────────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────────────┐
│ 第 2 层：指令（.github/instructions/）          │
│ - Copilot 的个性                                 │
│ - 行为边界                                       │
│ - "我是什么样的助手？"                           │
│ - 🚨 高风险防御点前置                            │
└─────────────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────────────┐
│ 第 3 层：Prompt（docs/copilot/）                │
│ - 详细工作手册                                   │
│ - 场景特定指导                                   │
│ - "我如何处理情况 X？"                           │
│ - 包含具体代码示例和 CI 错误诊断                 │
└─────────────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────────────┐
│ 执行：ArchitectureTests + Roslyn Analyzers      │
│ - 自动化验证（Level 1）                          │
│ - 语义检查（Level 2）                            │
│ - CI/CD 门禁                                     │
│ - 人工 Gate（Level 3）                           │
└─────────────────────────────────────────────────┘
```

---

## 使用指南

### 对于开发者

与 Copilot 交互时：
1. Copilot 将自动遵循这些指令
2. 如果需要具体帮助，参考相关的 `docs/copilot/` 文件
3. 如果 Copilot 显得过于谨慎，这是有意为之——它在保护架构

### 对于维护者

更新指令时：
1. 确保所有文件之间的一致性
2. 更新版本号和日期
3. 测试变更不与现有 ADR 冲突
4. 在 PR 中记录重大变更

## 关键原则

> **Copilot 放大理解能力，不替代理解**

这些指令确保 Copilot：
- ✅ 帮助你更快理解 ADR
- ✅ 更早捕获违规
- ✅ 建议合规解决方案
- ❌ 不让你绕过学习
- ❌ 不替代人工判断
- ❌ 不覆盖架构测试

## 快速参考

| 场景 | 查看文件 | 关键特征 |
|----------|------------|----------|
| "我能在模块中做 X 吗？" | [`backend.instructions.md`](./backend.instructions.md) | 🚨 高风险防御点，执行级别 |
| "我应该如何测试这个？" | [`testing.instructions.md`](./testing.instructions.md) | 🚨 架构测试防御，级别分类 |
| "我应该如何记录这个？" | [`documentation.instructions.md`](./documentation.instructions.md) | 🚨 索引同步检查清单 |
| "这个 PR 架构上合理吗？" | [`architecture-review.instructions.md`](./architecture-review.instructions.md) | 🚨 三级危险扫描 |
| "核心规则是什么？" | [`base.instructions.md`](./base.instructions.md) | 基础行为边界 |

对于详细的"如何做"指导，始终参考 [`docs/copilot/`](../../docs/copilot/) 文件。

---

## 版本与变更历史

**当前版本**：2.0  
**最后更新**：2026-01-24

**v2.0 变更（2026-01-24）**：
- ✅ 所有文件增加权威声明和维护提醒
- ✅ 完善 ADR 引用的章节锚点
- ✅ 加强 ADR-0005-Enforcement-Levels.md 场景链接
- ✅ 优化文件结构，前置高风险防御点
- ✅ 增强文档索引同步检查清单
- ✅ 明确 Level 2/3 人工判定提示

**v1.0（2026-01-21）**：
- 初始版本，建立基础指令体系

---

## 维护提醒

> **🔄 重要**  
> 如本目录下任何文件内容与 ADR 正文存在不一致，或架构演进导致规则变更，请：
> 1. 同步架构负责人确认变更
> 2. 更新相应的指令文件以与 ADR 正文保持一致
> 3. 更新本 README.md 的版本历史
> 4. 进行团队公告，确保所有成员知晓变更
> 5. 更新相关的 [`docs/copilot/`](../../docs/copilot/) 辅导材料
> 6. 确保架构测试与 ADR 正文保持同步

---
