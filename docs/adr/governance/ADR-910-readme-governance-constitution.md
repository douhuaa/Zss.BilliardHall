# ADR-910：README 编写与维护宪法

> ⚖️ **本 ADR 是所有 README 文档的唯一裁决源，定义 README 的边界、约束与执法机制。**

**状态**：✅ Final（裁决型ADR）  
**版本**：1.0
**级别**：治理层 / 架构元规则  
**适用范围**：所有 README 文档（仓库根目录、模块、工具、文档目录）  
**生效时间**：即刻

---

## 聚焦内容（Focus）

- README 的定位与权限边界
- README 禁用的语言与表达
- README 必须包含的声明
- README 与 ADR 的关系
- 违规检测与执法机制

---

## 术语表（Glossary）

| 术语         | 定义                                              | 英文对照                      |
|------------|------------------------------------------------|---------------------------|
| README     | 说明性文档，解释"如何使用"，无架构裁决力                         | README Document           |
| 裁决性语言      | 定义规则、做出判断的词汇，如"必须"、"禁止"、"不允许"                | Decision Language         |
| 描述性语言      | 解释用法、提供步骤的词汇，如"可以"、"推荐"、"示例"                 | Descriptive Language      |
| 无裁决力声明     | README 必须在开头声明无架构裁决权                           | No-Authority Declaration  |
| README 越界  | README 试图定义架构规则或做出架构判断                         | README Boundary Violation |

---

## 决策（Decision）

### README 是使用说明，不是架构裁决书（ADR-910.1）

**规则**：

README **仅允许**：
- ✅ 解释如何使用系统/模块/工具
- ✅ 提供快速开始指南
- ✅ 说明配置步骤
- ✅ 列举示例代码和命令
- ✅ 链接到 ADR 和其他权威文档

README **禁止**：
- ❌ 定义架构规则
- ❌ 做出架构判断
- ❌ 使用裁决性语言（见 ADR-910.2）
- ❌ 替代 ADR 解释架构约束
- ❌ 引入新的术语定义

**核心原则**：
> README 回答：怎么用、怎么做、示例是什么  
> README 不回答：是否允许、为什么、如何裁决

**判定**：
- ❌ README 定义"模块必须隔离"
- ❌ README 做出"这个设计符合架构"的判断
- ✅ README 链接到 ADR-0001 并说明如何遵守

---

### README 禁用的裁决性语言（ADR-910.2）

**规则**：

README **禁止**使用以下裁决性词汇：

| 裁决性词汇 | 说明                  | 替代表达          |
|-------|---------------------|---------------|
| 必须    | 定义强制要求              | "根据 ADR-XXXX" |
| 禁止    | 定义不允许的行为            | "根据 ADR-XXXX" |
| 不允许   | 定义不允许的行为            | "根据 ADR-XXXX" |
| 应当    | 定义规范性要求             | "建议"、"推荐"     |
| 违规    | 做出合规性判断             | "不符合 ADR-XXXX" |

**允许使用的上下文**：
1. 明确引用 ADR：`"根据 ADR-0001，模块必须隔离"`
2. 代码示例标记：`// ✅ 正确 // ❌ 错误`
3. 对比表格：`| 操作 | 是否允许 | 依据 | ... | 禁止 | ADR-0001 |`

**核心原则**：
> README 禁用的裁决性语言必须有 ADR 引用支撑。

**判定**：
- ❌ "模块必须使用事件通信"（无 ADR 引用）
- ✅ "根据 ADR-0001，模块必须使用事件通信"
- ✅ 代码块中的 `// ✅ 正确` 标记

---

### README 必须包含的声明（ADR-910.3）

**规则**：

所有 README **必须**在开头包含"无裁决力声明"：

```markdown
⚠️ 本文档不具备裁决力。所有架构决策以对应 ADR 正文为准。
```

**位置要求**：
- 必须在文档开头，标题之后
- 必须在任何架构内容之前
- 使用 ⚠️ 警告符号

**例外**：
- 纯操作性 README（仅命令示例）
- 第三方库的 README
- 自动生成的 README

**判定**：
- ❌ 缺失声明
- ❌ 声明位置不正确
- ✅ 文档开头包含标准声明

---

### README 与 ADR 的关系（ADR-910.4）

**规则**：

README 引用 ADR 必须遵循：

**允许**：
```markdown
✅ "根据 ADR-0001，模块使用事件通信。详见 [ADR-0001](链接)"
✅ "参考 ADR-0005 了解 Handler 模式"
```

**禁止**：
```markdown
❌ "模块必须使用事件通信，禁止直接引用"（无 ADR 引用）
❌ "这个设计不符合架构规范"（做出判断）
❌ 长篇复制 ADR 内容
```

**核心原则**：
> README 是 ADR 的索引和指南，不是 ADR 的副本或替代品。  
> README 只能指向 ADR，不能转述为命令。

**判定**：
- ❌ README 转述 ADR 为自己的命令
- ❌ README 复制大段 ADR 内容
- ✅ README 链接到 ADR 并简短说明

---

### README 的变更治理（ADR-910.5）

**规则**：

| README 类型    | 变更要求          | 审批流程        |
|--------------|---------------|-------------|
| 仓库根 README   | 若涉及架构概念，需回溯 ADR | Code Review |
| 模块 README    | 自由修改，但需保持声明   | Code Review |
| 工具 README | 自由修改          | Code Review |

**冲突裁决**：
- 当 README 与 ADR 冲突时，必须修正 README
- 不得以"README 说了"为由违反 ADR
- 若需要修改理解，必须先修订 ADR

**判定**：
- ❌ 仅更新 README 而不修订相关 ADR
- ✅ 先修订 ADR，再同步更新 README

---

## 快速参考表

| 约束编号       | 约束描述                | 测试方式             | 必须遵守 |
|------------|---------------------|------------------|------|
| ADR-910.1  | README 只允许解释使用，禁止定义架构规则 | L3 - Code Review | ✅    |
| ADR-910.2  | README 禁止使用裁决性语言（除非引用 ADR） | L2 - CI 脚本检查 | ✅    |
| ADR-910.3  | README 必须包含无裁决力声明    | L2 - CI 脚本检查 | ✅    |
| ADR-910.4  | README 引用 ADR 必须明确源头  | L3 - Code Review | ✅    |
| ADR-910.5  | README 冲突时必须修正 README | L3 - Code Review | ✅    |

---

## 必测/必拦架构测试（Enforcement）

所有规则通过以下方式强制验证：

- **架构测试**：`src/tests/ArchitectureTests/ADR/ADR_0910_Architecture_Tests.cs`
  - `README_Must_Not_Use_Decision_Language` - 检测裁决性语言
  - `README_Must_Declare_No_Authority` - 检查无裁决力声明

- **CI 脚本**：自动扫描所有 README 文档
- **Code Review**：人工审查 README 是否越界

**有一项违规视为架构违规，CI 自动阻断。**

---

## 破例与归还（Exception）

### 允许破例的前提

README 规则的破例**仅在以下情况允许**：
- 临时性迁移文档（标注 `[DRAFT]` 或 `[迁移中]`）
- 历史遗留 README 的过渡期（不超过 1 个月）
- 第三方生成的 README

### 破例要求

每个破例**必须**：
- 记录在 `ARCH-VIOLATIONS.md` 的"README 治理破例"章节
- 指明破例的具体文件和原因
- 指定失效日期（不超过 1 个月）
- 给出归还计划

---

## 变更政策（Change Policy）

- **ADR-910**（治理层）
  - 修改需 Tech Lead 审批
  - 需要全量 README 回归检查
  - 需要更新对应的架构测试

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：
- README 的写作风格和美学
- README 的长度限制
- README 的多语言版本
- README 的格式工具选择

---

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](./ADR-0000-architecture-tests.md) - README 治理基于测试和 CI 机制
- [ADR-0006：术语与编号宪法](../constitutional/ADR-0006-terminology-numbering-constitution.md) - README 术语使用遵循统一规范
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - README 约束是文档治理的细化

**被依赖（Depended By）**：
- 无（README 约束是终端规则，不被其他 ADR 依赖）

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-950：指南与 FAQ 文档治理](./ADR-950-guide-faq-documentation-governance.md) - 同为非裁决性文档治理

---

## 版本历史

| 版本  | 日期         | 变更说明       |
|-----|------------|------------|
| 2.0 | 2026-01-26 | 裁决型重构，移除冗余 |
| 1.0 | 2026-01-25 | 初版，从 ADR-0008 中独立 README 治理 |

---

## 附注

本文件禁止添加示例/建议/FAQ/背景说明，仅维护自动化可判定的架构红线。

非裁决性参考（详细示例、场景说明）请查阅：
- [ADR-0910 Copilot Prompts](../../copilot/adr-0910.prompts.md)

