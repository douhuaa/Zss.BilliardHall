---
adr: ADR-960
title: "Onboarding 文档治理宪法"
status: Accepted
level: Governance
deciders: "Tech Lead & Onboarding Champion"
date: 2026-01-26
version: "1.0"
maintainer: "Tech Lead & Onboarding Champion"
primary_enforcement: L1
reviewer: "待定"
supersedes: null
superseded_by: null
---

# ADR-960：Onboarding 文档治理宪法

> ⚖️ **本 ADR 是所有 Onboarding 文档（新人入职文档）的唯一治理标准，定义其结构、审计和反馈机制。**

**状态**：✅ Accepted  
**版本**：1.0
**级别**：文档专项治理 / 治理层  
**适用范围**：所有 Onboarding 相关文档  
**生效时间**：即刻

---

## Focus（聚焦内容）

- Onboarding 文档层级结构
- 周期性审计机制
- 新人反馈收集标准化
- 成功标准定义
- 责任人制度

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| Onboarding | 新人入职培训文档体系 | Onboarding |
| 快速上手 | 15 分钟极速体验，L1 级别 | Quick Start |
| 学习路径 | 按周组织的渐进式学习计划 | Learning Path |
| 成功标准 | 定义何时算"入门完成" | Success Criteria |
| 周期性审计 | 定期检查文档准确性 | Periodic Audit |
| 新人反馈 | 新成员对文档的评价和建议 | Onboarding Feedback |

---

## Decision（裁决）

### Onboarding 文档层级结构（ADR-960.1）

**规则**：

Onboarding 文档 **必须**按以下层级组织：

**文档层级**：
```
docs/
├── QUICK-START.md              # L1: 15 分钟极速上手
│                               # 目标：运行第一个测试
│
├── onboarding/
│   ├── README.md               # 入门导航（路线图）
│   │
│   ├── week-1-foundation.md    # L2: 第 1 周 - 基础理解
│   │                           # 目标：理解架构、熟悉工具
│   │
│   ├── week-2-development.md   # L3: 第 2 周 - 开发实践
│   │                           # 目标：创建第一个用例
│   │
│   ├── week-3-architecture.md  # L4: 第 3 周 - 架构深入
│   │                           # 目标：理解核心 ADR
│   │
│   └── week-4-contribution.md  # L5: 第 4 周 - 独立贡献
│                               # 目标：独立提交 PR
│
└── onboarding/
    └── feedback-template.md    # 新人反馈表模板
```

**各层级内容要求**：

| 层级 | 名称 | 时长 | 核心目标 | 必须包含 |
|------|------|------|----------|---------|
| L1 | QUICK-START.md | 15 分钟 | 运行第一个测试 | 环境搭建、运行测试、验证成功 |
| L2 | week-1-foundation.md | 1 周 | 基础理解 | 架构概览、工具链、关键术语 |
| L3 | week-2-development.md | 1 周 | 开发实践 | 创建用例、测试编写、提交规范 |
| L4 | week-3-architecture.md | 1 周 | 架构深入 | 核心 ADR、设计模式、约束理解 |
| L5 | week-4-contribution.md | 1 周 | 独立贡献 | 独立 PR、代码审查、架构讨论 |

**核心原则**：
> 渐进式学习，清晰里程碑，可验证进度。

**判定**：
- ❌ 所有内容堆在一个文档中
- ❌ 缺少明确的时间目标
- ✅ 按层级组织，每层有明确目标

---

### 周期性审计机制（ADR-960.2）

**规则**：

Onboarding 文档 **必须**每月进行准确性审计。

**审计清单**：
```markdown
## Onboarding 文档月度审计清单

审计日期：YYYY-MM-DD
审计人：[姓名]

### QUICK-START.md
- [ ] 环境搭建步骤准确（工具版本、命令）
- [ ] 代码示例可运行
- [ ] 链接有效
- [ ] 预计时长准确（15 分钟内）

### week-1-foundation.md
- [ ] 架构图与实际代码一致
- [ ] 关键 ADR 链接有效
- [ ] 工具链描述准确
- [ ] 术语表完整

### week-2-development.md
- [ ] 用例创建步骤准确
- [ ] 测试示例可运行
- [ ] 提交规范与实际一致
- [ ] 代码示例符合最新 ADR

### week-3-architecture.md
- [ ] ADR 列表完整
- [ ] 设计模式示例准确
- [ ] 约束描述与测试一致

### week-4-contribution.md
- [ ] PR 流程描述准确
- [ ] 代码审查指南有效
- [ ] 链接到最新的贡献指南

### 发现的问题
[列出需要修复的问题]

### 修复计划
[列出修复 Issue 和责任人]
```

**审计频率**：
- **每月**：常规审计
- **ADR 重大变更后**：立即审计
- **新人反馈后**：按需审计

**审计责任人**：
- 由 `CODEOWNERS` 指定
- 轮换制：每月不同成员负责

**核心原则**：
> 持续准确，及时更新。

**判定**：
- ❌ Onboarding 文档与实际代码脱节
- ❌ 从未审计过文档准确性
- ✅ 每月审计并记录

---

### 新人反馈收集机制（ADR-960.3）

**规则**：

新成员完成 Onboarding 后 **必须**填写反馈表。

**反馈表标准结构**：
```markdown
# Onboarding 反馈表

**姓名**：[可选]  
**完成日期**：YYYY-MM-DD  
**Onboarding 总时长**：[实际花费时间]

## L1: QUICK-START.md

**准确性**（1-5 分）：[ ]  
**清晰度**（1-5 分）：[ ]  
**实际耗时**：[ ] 分钟

**遇到的障碍**：
[描述]

**改进建议**：
[建议]

## L2: week-1-foundation.md

**准确性**（1-5 分）：[ ]  
**清晰度**（1-5 分）：[ ]  
**实际耗时**：[ ] 天

**遇到的障碍**：
[描述]

**改进建议**：
[建议]

## L3: week-2-development.md

（同上格式）

## L4: week-3-architecture.md

（同上格式）

## L5: week-4-contribution.md

（同上格式）

## 总体评价

**最有帮助的部分**：
[描述]

**最需要改进的部分**：
[描述]

**缺失的内容**：
[列出]

**其他建议**：
[自由描述]
```

**反馈处理流程**：
1. 新人完成 Onboarding 后创建 Issue（使用反馈表模板）
2. 标签：`onboarding-feedback`
3. 负责人：Onboarding 文档 CODEOWNERS
4. 评估：每月汇总反馈，识别模式
5. 改进：创建改进 Issue，排入优先级
6. 跟踪：关闭反馈 Issue，关联改进 Issue

**核心原则**：
> 听取新人声音，持续改进。

**判定**：
- ❌ 新人完成 Onboarding 但无反馈机制
- ❌ 收集反馈但不处理
- ✅ 系统化收集并改进

---

### 成功标准定义（ADR-960.4）

**规则**：

Onboarding **必须**定义明确的完成标准。

**完成标准清单**：
```markdown
## Onboarding 完成标准

新成员完成以下所有项目即视为"Onboarding 完成"：

### 技术能力
- [ ] 独立搭建开发环境
- [ ] 运行所有架构测试并通过
- [ ] 创建一个新用例（包含 Handler、Endpoint、测试）
- [ ] 提交至少 1 个 PR 并合并

### 架构理解
- [ ] 理解模块化单体架构（ADR-0001）
- [ ] 理解 CQRS 和 Handler 模式（ADR-0005）
- [ ] 理解测试与 ADR 的关系（ADR-0000）
- [ ] 能够解释垂直切片架构

### 流程熟悉
- [ ] 熟悉 Git 工作流和提交规范
- [ ] 熟悉 PR 流程和代码审查
- [ ] 熟悉 CI/CD 流程
- [ ] 熟悉 ADR 查阅和引用

### 工具掌握
- [ ] 熟练使用 IDE 和调试工具
- [ ] 熟悉测试运行和调试
- [ ] 熟悉 CLI 工具（如 dotnet、git）

### 团队协作
- [ ] 参加至少 1 次架构讨论
- [ ] 参加至少 2 次代码审查
- [ ] 填写 Onboarding 反馈表
```

**验证方式**：
- 技术能力：提交的 PR 和代码质量
- 架构理解：与 Mentor 的讨论或书面问答
- 流程熟悉：实际操作记录
- 工具掌握：实际使用情况
- 团队协作：参与记录

**时间目标**：
- **理想**：4 周内完成
- **可接受**：6 周内完成
- **需要关注**：超过 6 周

**核心原则**：
> 明确标准，可验证完成。

**判定**：
- ❌ 不知道何时算"入门完成"
- ❌ 标准过于主观或模糊
- ✅ 明确、可验证的完成标准

---

### 责任人制度（ADR-960.5）

**规则**：

Onboarding 文档 **必须**指定明确的责任人。

**CODEOWNERS 配置**：
```
# Onboarding 文档
/docs/QUICK-START.md @tech-lead @onboarding-champion
/docs/onboarding/ @tech-lead @onboarding-champion
```

**责任人职责**：
1. **内容准确性**：确保文档与实际代码一致
2. **月度审计**：执行月度审计清单
3. **反馈处理**：评估新人反馈并排优先级
4. **改进主导**：主导 Onboarding 文档改进
5. **新人支持**：回答新人 Onboarding 相关问题

**Onboarding Champion 角色**：
- **选拔**：团队推选，任期 6 个月
- **职责**：专注 Onboarding 体验改进
- **交接**：任期结束时更新文档并交接

**核心原则**：
> 明确责任，持续改进。

**判定**：
- ❌ Onboarding 文档无人负责
- ❌ 责任不清晰
- ✅ 明确指定责任人并履职

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 基于其文档分级和编写标准
- [ADR-950：指南与 FAQ 文档治理宪法](../governance/ADR-950-guide-faq-documentation-governance.md) - 基于其文档类型定义

**被依赖（Depended By）**：
- [ADR-965：Onboarding 互动式学习路径](../governance/ADR-965-onboarding-interactive-learning-path.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-965：Onboarding 互动式学习路径](../governance/ADR-965-onboarding-interactive-learning-path.md) - 互动式体验增强

---

## 执法模型（Enforcement）

| 规则编号 | 执行级别 | 测试/手段 | 说明 |
|---------|---------|----------|------|
| ADR-960.1 | L2 | Code Review | 人工审查文档结构 |
| ADR-960.2 | L2 | 月度审计 Issue | 追踪审计完成度 |
| ADR-960.3 | L2 | Issue Template | 标准化反馈收集 |
| ADR-960.4 | L2 | Mentor 验证 | 人工评估完成度 |
| ADR-960.5 | L1 | CODEOWNERS | GitHub 自动通知 |

---

## 破例与归还（Exception）

### 允许破例的前提

破例 **仅在以下情况允许**：
- 团队规模过小（<3 人）无法支持月度审计
- 新项目初期（<6 个月）Onboarding 流程尚未稳定
- 特殊时期（如大规模重构）暂停审计

### 破例要求

每个破例 **必须**：
- 记录在 Issue 中，说明原因和预期恢复时间
- 标记 `onboarding-audit-suspended` 标签
- 指定恢复审计的触发条件
- 架构委员会批准

**未记录的破例 = 未授权治理违规。**

---

## 变更政策（Change Policy）

### 变更规则

本 ADR 属于 **治理层文档专项规则**：
- 修改需 Tech Lead + Onboarding Champion 同意
- 需通知所有新近完成 Onboarding 的成员
- 需更新相关模板和清单

### 失效与替代

- 本 ADR 一旦被替代，**必须**更新所有 Onboarding 文档
- 不允许"隐性废弃"

---

## Non-Goals（明确不管什么）

本 ADR **不负责**：
- 技术培训内容的具体设计（由团队决定）
- Onboarding 文档的写作风格（由 ADR-0008 规范）
- 新人绩效评估标准
- 团队文化建设
- 非技术性的入职流程（HR 流程等）

---

## References（非裁决性参考）

### 相关 ADR
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md)
- [ADR-950：指南与 FAQ 文档治理宪法](../governance/ADR-950-guide-faq-documentation-governance.md)

### 实施工具
- `docs/onboarding/feedback-template.md` - 反馈表模板
- `.github/CODEOWNERS` - 责任人配置
- Issue Template：`onboarding-feedback`

### 背景材料
- [ADR-Documentation-Governance-Gap-Analysis.md](../proposals/ADR-Documentation-Governance-Gap-Analysis.md) - 原始提案

---

## 版本历史（Version History）

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|----------|------|
| 1.0 | 2026-01-26 | 初版：定义 Onboarding 文档治理机制 | GitHub Copilot |
