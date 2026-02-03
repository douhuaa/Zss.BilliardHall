---
adr: ADR-960
title: "Onboarding 文档治理规范"
status: Accepted
level: Governance
deciders: "Tech Lead & Onboarding Champion"
date: 2026-01-30
version: "1.1"
maintainer: "Tech Lead & Onboarding Champion"
primary_enforcement: L1
reviewer: "待定"
supersedes: null
superseded_by: null
---


# ADR-960：Onboarding 文档治理规范

> ⚖️ **本 ADR 是所有 Onboarding 文档（新人入职文档）的治理规范，定义其结构、审计和反馈机制。**

**状态**：✅ Accepted  
## Focus（聚焦内容）

- Onboarding 文档层级结构
- 周期性审计机制
- 新人反馈收集标准化
- 成功标准定义
- 责任人制度

---

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
- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md) - 基于其文档分级和编写标准
- [ADR-950：指南与 FAQ 文档治理规范](../governance/ADR-950-guide-faq-documentation-governance.md) - 基于其文档类型定义

**被依赖（Depended By）**：
- [ADR-965：Onboarding 互动式学习路径](../governance/ADR-965-onboarding-interactive-learning-path.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-965：Onboarding 互动式学习路径](../governance/ADR-965-onboarding-interactive-learning-path.md) - 互动式体验增强

---

---

## References（非裁决性参考）

### 相关 ADR
- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md)
- [ADR-950：指南与 FAQ 文档治理规范](../governance/ADR-950-guide-faq-documentation-governance.md)

### 实施工具
- `docs/onboarding/feedback-template.md` - 反馈表模板
- `.github/CODEOWNERS` - 责任人配置
- Issue Template：`onboarding-feedback`

### 背景材料
- [ADR-Documentation-Governance-Gap-Analysis.md](../proposals/ADR-Documentation-Governance-Gap-Analysis.md) - 原始提案

---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
