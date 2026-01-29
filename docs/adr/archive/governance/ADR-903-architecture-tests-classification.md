---
adr: ADR-903
title: "架构测试分类标准"
status: Superseded
level: Governance
deciders: "Architecture Board"
date: 2026-01-25
version: "1.0"
maintainer: "Architecture Board"
reviewer: "Architecture Board"
supersedes: null
superseded_by: "ADR-907"
---

> 🏛️ **Archived Notice**
>
> 本 ADR 已被 [ADR-907](../../governance/ADR-907-architecture-tests-enforcement-governance.md) 完全吸收并取代。
> 
> - 本文件 **不再具备任何裁决力**
> - **不得** 编写或维护对应 ArchitectureTests
> - **不得** 被 Analyzer / CI Gate 读取
> - 仅用于 **历史追溯与设计演进说明**

# ADR-903：架构测试分类标准

**状态**：❌ Superseded（已被 ADR-907 取代）  
**级别**：治理层  
**版本**：1.0  
**创建日期**：2026-01-25  
**归档日期**：2026-01-29  
**适用范围**：（已失效）  
**生效时间**：（已失效）

---

## 历史背景（Historical Context）

本 ADR 最初定义了架构测试的三级分类标准（L1/L2/L3），用于区分不同执行能力的测试：

- **L1 静态可执行**：NetArchTest 等静态分析工具
- **L2 语义半自动**：Roslyn Analyzer 或启发式检查
- **L3 人工门控**：Code Review 或架构审查

---

## 为什么被取代（Superseded Rationale）

随着架构治理体系的成熟，我们发现单独维护测试分类、命名、执行流程三个 ADR 导致：

1. **规则碎片化**：开发者需要查阅多个文档才能理解完整流程
2. **执法不统一**：三个 ADR 的优先级和冲突处理不明确
3. **维护成本高**：修改一个规则需要同步更新多个文档和测试

因此，架构委员会决定将 ADR-903/904/906 合并为单一的 **ADR-907：架构测试执行治理宪章**，实现：

- 唯一执法入口
- 集中规则管理
- 统一 CI 流程
- 简化破例治理

---

## 原始规则（Original Rules）

> ⚠️ **以下内容仅供历史追溯，不具裁决力。所有规则已迁移至 ADR-907。**

### 分类定义

| 级别 | 定义                  | 工具                    | 失败处理  |
|----|---------------------|------------------------|---------|
| L1 | 静态可执行（编译期）          | NetArchTest            | 绝对违规  |
| L2 | 语义半自动（调用链、复杂度）      | Roslyn Analyzer / 启发式 | 需人工审查 |
| L3 | 人工门控（设计意图）          | Code Review            | 人工决策  |

### 示例规则

- L1: `Modules_Should_Not_Reference_Other_Modules`（依赖检查）
- L2: `Endpoints_Should_Not_Contain_Business_Logic`（方法体分析）
- L3: `Handler_Should_Follow_Domain_Driven_Design`（设计原则）

---

## 迁移指南（Migration Guide）

如果你在旧文档或代码中看到对 ADR-903 的引用：

1. **ADR 文档引用** → 更改为 ADR-907
2. **测试注释引用** → 更改为 ADR-907.1（执行级别规则）
3. **CI 配置引用** → 更改为 ADR-907.5（CI 执行流程）
4. **Prompt 文件引用** → 更改为 ADR-907

---

## 关系（Relationships）

### 被取代（Superseded By）

- [ADR-907](../../governance/ADR-907-architecture-tests-enforcement-governance.md) - 架构测试执行治理宪章（唯一执法入口）

### 原始依赖（Original Dependencies）

- [ADR-0000](../../governance/ADR-0000-architecture-tests.md) - 架构测试与 CI 治理宪法
- [ADR-905](../../governance/ADR-905-enforcement-level-classification.md) - 执行级别分类

---

## 版本历史（Version History）

| 版本  | 日期         | 变更说明                       | 状态         |
|-----|------------|----------------------------|------------|
| 1.0 | 2026-01-25 | 初始版本，定义三级分类标准             | Active     |
| -   | 2026-01-29 | 被 ADR-907 完全取代，移至 archive | Superseded |

---

**维护者**：Architecture Board  
**归档者**：Architecture Board  
**归档日期**：2026-01-29

> 📌 **重要提醒**：本文档已失效，请勿用于架构决策或测试实施。所有相关规则已迁移至 ADR-907。
