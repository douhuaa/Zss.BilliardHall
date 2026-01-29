---
adr: ADR-906
title: "架构测试执行流程"
status: Superseded
level: Governance
deciders: "Architecture Board"
date: 2026-01-26
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

# ADR-906：架构测试执行流程

**状态**：❌ Superseded（已被 ADR-907 取代）  
**级别**：治理层  
**版本**：1.0  
**创建日期**：2026-01-26  
**归档日期**：2026-01-29  
**适用范围**：（已失效）  
**生效时间**：（已失效）

---

## 历史背景（Historical Context）

本 ADR 最初定义了架构测试的 CI 执行流程和阻断规则：

- **L1 测试**：失败 → CI 阻断 → PR 无法合并
- **L2 测试**：失败 → 警告日志 → 需人工审查
- **L3 检查**：PR 模板清单 → 人工确认

同时定义了破例治理流程：
- 破例必须记录在 `ARCH-VIOLATIONS.md`
- CI 自动扫描过期破例
- 过期破例 → 构建失败

---

## 为什么被取代（Superseded Rationale）

执行流程与测试分类、命名规范紧密相关，单独维护导致：

1. **流程不连贯**：开发者需要跨多个 ADR 理解完整工作流
2. **破例治理分散**：破例规则在 ADR-0000、ADR-906 中重复定义
3. **CI 配置复杂**：需要分别参考三个 ADR 才能正确配置 CI

ADR-907 将执行流程作为第 5 节（ADR-907.5）和破例治理作为第 6 节（ADR-907.6）进行统一管理。

---

## 原始规则（Original Rules）

> ⚠️ **以下内容仅供历史追溯，不具裁决力。所有规则已迁移至 ADR-907。**

### CI 执行流程

```yaml
# .github/workflows/architecture-tests.yml
steps:
  1. 运行所有 L1 测试（NetArchTest）
     - 失败 → CI 失败 → PR 阻断
  2. 运行所有 L2 测试（Roslyn Analyzer）
     - 失败 → 警告日志 → 需人工审查
  3. 检查 L3 检查清单
     - 未填写 → PR 阻断
```

### 破例治理流程

**破例记录格式**：
```markdown
## arch-violations.md

| ADR         | 规则 | 违规文件 | 到期版本 | 负责人 | 归还计划 | 状态 |
|-------------|------|---------|---------|--------|---------|------|
| ADR-0001.1  | 模块隔离 | Orders/Legacy.cs | v2.5.0 | @dev | 迁移至新模块 | 🚧 进行中 |
```

**CI 监控**：
```bash
# scripts/check-arch-violations.sh
# 每次构建执行，检查：
1. 所有破例是否有到期版本
2. 当前版本是否已超过到期版本
3. 过期破例 → 构建失败
```

---

## 迁移指南（Migration Guide）

如果你在旧文档或代码中看到对 ADR-906 的引用：

1. **ADR 文档引用** → 更改为 ADR-907
2. **CI 流程引用** → 更改为 ADR-907.5（CI 执行流程）
3. **破例治理引用** → 更改为 ADR-907.6（破例治理）
4. **Prompt 文件引用** → 更改为 ADR-907
5. **CI 配置引用** → 更改为 ADR-907.5

---

## 关系（Relationships）

### 被取代（Superseded By）

- [ADR-907](../../governance/ADR-907-architecture-tests-enforcement-governance.md) - 架构测试执行治理宪章（唯一执法入口）

### 原始依赖（Original Dependencies）

- [ADR-0000](../../governance/ADR-0000-architecture-tests.md) - 架构测试与 CI 治理宪法
- [ADR-903](./ADR-903-architecture-tests-classification.md) - 架构测试分类标准（已归档）
- [ADR-904](./ADR-904-architecture-tests-naming.md) - 架构测试命名规范（已归档）

---

## 版本历史（Version History）

| 版本  | 日期         | 变更说明                       | 状态         |
|-----|------------|----------------------------|------------|
| 1.0 | 2026-01-26 | 初始版本，定义执行流程和破例治理          | Active     |
| -   | 2026-01-29 | 被 ADR-907 完全取代，移至 archive | Superseded |

---

**维护者**：Architecture Board  
**归档者**：Architecture Board  
**归档日期**：2026-01-29

> 📌 **重要提醒**：本文档已失效，请勿用于架构决策或测试实施。所有相关规则已迁移至 ADR-907。
