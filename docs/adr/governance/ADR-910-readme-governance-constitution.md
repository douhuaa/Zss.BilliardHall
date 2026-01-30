---
adr: ADR-910
title: "README 编写与维护治理规范"
status: Final
level: Governance
version: "1.1"
deciders: "Architecture Board"
date: 2026-01-30
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---


# ADR-910：README 编写与维护治理规范

> ⚖️ **本 ADR 是所有 README 文档的治理规范，定义 README 的边界、约束与执法机制。**

**状态**：✅ Final（裁决型ADR）  
## Focus（聚焦内容）

- README 的定位与权限边界
- README 禁用的语言与表达
- README 必须包含的声明
- README 与 ADR 的关系
- 违规检测与执法机制

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------------|------------------------------------------------|---------------------------|
| README     | 说明性文档，解释"如何使用"，无架构裁决力                         | README Document           |
| 裁决性语言      | 定义规则、做出判断的词汇，如"必须"、"禁止"、"不允许"                | Decision Language         |
| 描述性语言      | 解释用法、提供步骤的词汇，如"可以"、"推荐"、"示例"                 | Descriptive Language      |
| 无裁决力声明     | README 必须在开头声明无架构裁决权                           | No-Authority Declaration  |
| README 越界  | README 试图定义架构规则或做出架构判断                         | README Boundary Violation |

---

---

## Decision（裁决）

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

---

## Enforcement（执法模型）

所有规则通过以下方式强制验证：

- **架构测试**：`src/tests/ArchitectureTests/ADR/ADR_0910_Architecture_Tests.cs`
  - `README_Must_Not_Use_Decision_Language` - 检测裁决性语言
  - `README_Must_Declare_No_Authority` - 检查无裁决力声明

- **CI 脚本**：自动扫描所有 README 文档
- **Code Review**：人工审查 README 是否越界

**有一项违规视为架构违规，CI 自动阻断。**

---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **README 的视觉设计和排版**：不涉及字体、颜色、图标等视觉元素的设计规范
- **README 的长度限制**：不规定 README 文档的最大或最小字数要求
- **README 的多语言版本**：不涉及多语言 README 的创建和维护流程
- **README 的自动化生成工具**：不规定使用特定的文档生成或模板工具
- **README 中的代码示例风格**：不约束代码示例的具体编写风格和格式
- **README 的更新频率**：不规定 README 必须多久更新一次
- **README 在不同平台的适配**：不涉及 GitHub、GitLab、Bitbucket 等平台的特殊格式要求
- **README 的SEO优化**：不涉及搜索引擎优化、关键词布局等营销相关内容

---

## Prohibited（禁止行为）


以下行为明确禁止：

### 越权行为
- ❌ **禁止 README 使用裁决性语言**：不得使用"必须"、"禁止"、"不允许"等定义规则的词汇
- ❌ **禁止 README 定义架构约束**：不得在 README 中引入新的架构规则或限制
- ❌ **禁止 README 覆盖 ADR 规则**：不得与 ADR 冲突或试图修改 ADR 定义的规则

### 内容质量违规
- ❌ **禁止 README 缺少权威声明**：必须明确声明"若与 ADR 冲突，以 ADR 为准"
- ❌ **禁止 README 引用不存在的 ADR**：所有 ADR 引用必须指向真实存在的文件
- ❌ **禁止 README 使用模糊的相对路径**：ADR 引用必须使用清晰的相对或绝对路径

### 维护违规
- ❌ **禁止 README 与实际代码严重不符**：示例代码必须能实际运行或明确标注为伪代码
- ❌ **禁止 README 包含过期信息而不标注**：过期内容必须移除或明确标记为已废弃
- ❌ **禁止 README 复制粘贴 ADR 内容**：应该引用 ADR 而不是复制其内容


---

---

## Relationships（关系声明）

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

---

## References（非裁决性参考）

**相关外部资源**：
- [Make a README](https://www.makeareadme.com/) - README 编写指南和最佳实践
- [Awesome README](https://github.com/matiassingers/awesome-readme) - 优秀 README 示例集合
- [GitHub's Guide to README](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes) - GitHub 官方 README 指南

**相关内部文档**：
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 文档分级与权限划分
- [ADR-920：示例代码治理宪法](./ADR-920-examples-governance-constitution.md) - README 中代码示例的约束
- [ADR-950：Guide/FAQ 文档治理](./ADR-950-guide-faq-documentation-governance.md) - 与 Guide 文档的边界
- [ADR-960：入职文档治理](./ADR-960-onboarding-documentation-governance.md) - 入职文档与 README 的关系


---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
