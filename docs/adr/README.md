# 架构决策记录（Architecture Decision Records）

**版本**：3.1  
**最后更新**：2026-01-22  
**状态**：Active

---

> ⚠️ **无裁决力声明**：本文档仅供参考，不具备架构裁决权。
> 具体裁决请以各 ADR 正文为准，详见 [ADR 目录](adr/README.md)。

---

## 概述

本目录收录了 Zss.BilliardHall 项目的所有 ADR，采用编号分段与分层目录，仅用于快速导航与索引。

---

## 编号与分层体系

| 层级      | 编号范围                | 目录                | 当前 ADR 数 |
|---------|---------------------|-------------------|----------|
| **宪法层** | `ADR-0001~0009`     | `constitutional/` | 8         |
| **结构层** | `ADR-100~199`       | `structure/`      | 2         |
| **运行层** | `ADR-200~299`       | `runtime/`        | 1         |
| **技术层** | `ADR-300~399`       | `technical/`      | 1         |
| **治理层** | `ADR-0000, 900~999` | `governance/`     | 2         |

编号含义、约束及变更政策以 [ADR-0006](constitutional/ADR-0006-terminology-numbering-constitution.md) 为准。

---

## 层级与用途简介

- **宪法层**：系统基础约束，[目录](constitutional/)，如模块隔离、启动体系、命名空间等
- **结构层**：静态组织与命名，[目录](structure/)
- **运行层**：运行时模型、异常处理、生命周期等，[目录](runtime/)
- **技术层**：具体实现与技术选型，[目录](technical/)
- **治理层**：流程、测试、破例管理，[目录](governance/)

---

## 快速导航

### 主要 ADR 链接

- [ADR-0001：模块化单体与垂直切片架构](constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0002：Platform / Application / Host 三层启动体系](constitutional/ADR-0002-platform-application-host-bootstrap.md)
- [ADR-0003：命名空间与项目边界规范](constitutional/ADR-0003-namespace-rules.md)
- [ADR-0004：中央包管理（CPM）规范](constitutional/ADR-0004-Cpm-Final.md)
- [ADR-0005：应用内交互模型与执行边界](constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [ADR-0006：术语与编号宪法](constitutional/ADR-0006-terminology-numbering-constitution.md)
- [ADR-0007：Agent 行为与权限宪法](constitutional/ADR-0007-agent-behavior-permissions-constitution.md)
- [ADR-0008：文档编写与维护宪法](constitutional/ADR-0008-documentation-governance-constitution.md)
- [ADR-900：ADR 新增与修订流程](governance/ADR-900-adr-process.md)
- [ADR-120：领域事件命名规范](structure/ADR-120-domain-event-naming-convention.md)
- [ADR-121：契约（Contract）与 DTO 命名组织规范](structure/ADR-121-contract-dto-naming-organization.md)
- [ADR-240：Handler 异常约束](runtime/ADR-240-handler-exception-constraints.md)
- [ADR-340：结构化日志与监控约束](technical/ADR-340-structured-logging-monitoring-constraints.md)
- [ADR-930：代码审查与 ADR 合规自检流程](governance/ADR-930-code-review-compliance.md)

---

## 层级目录浏览

- [宪法层 constitutional/](constitutional/)
- [结构层 structure/](structure/)
- [运行层 runtime/](runtime/)
- [技术层 technical/](technical/)
- [治理层 governance/](governance/)

---

## 索引与维护

- ADR 版本、层级、编号等重大事项以[ADR-0006](constitutional/ADR-0006-terminology-numbering-constitution.md)为主
- 变更流程详见 [ADR-900](governance/ADR-900-adr-process.md)
- 各层次变更均需相应审批���归档

---

## 🚀 ADR 对齐计划（2026 Q1）

> **状态**：✅ 执行中 | **开始日期**：2026-02-03 | **预计完成**：2026-Q2

我们正在将所有 ADR 对齐到 [ADR-907 v2.0](governance/ADR-907-architecture-tests-enforcement-governance.md) 定义的 **Rule/Clause 双层编号体系**。

### 关键文档

- 📋 **[ADR 对齐执行计划](../ADR-ALIGNMENT-EXECUTION-PLAN.md)** - 完整执行路线图（10-12 周）
- 📖 **[ADR-907：ArchitectureTests 执法治理体系](governance/ADR-907-architecture-tests-enforcement-governance.md)** - 新格式定义
- 📘 **[ADR-907-A：对齐执行标准](governance/ADR-907-A-adr-alignment-execution-standard.md)** - 强制规范
- ✅ **[对齐进度清单](governance/adr-907-a-alignment-checklist.md)** - 实时追踪
- 🚀 **[快速开始指南](governance/ADR-ALIGNMENT-GETTING-STARTED.md)** - 如何参与

### 当前进度

| 维度 | 完成率 |
|------|--------|
| ADR 对齐 | 54% (26/48) |
| 测试对齐 | 14% (4/29) |
| Instructions 更新 | 10% |

### 如何参与

1. 阅读 [快速开始指南](governance/ADR-ALIGNMENT-GETTING-STARTED.md)
2. 查看 [对齐清单](governance/adr-907-a-alignment-checklist.md) 选择工作项
3. 按照 [执行标准](governance/ADR-907-A-adr-alignment-execution-standard.md) 进行对齐
4. 提交 PR 并更新清单状态

---

## 关联资料

- [ADR 实施指南](ADR-IMPLEMENTATION-GUIDE.md)
- [待落地 ADR 提案跟踪清单](PENDING-ADR-PROPOSALS.md)

---

**如需裁决性规则、判例和解释，请查阅正文，不在本 README 给出。**
