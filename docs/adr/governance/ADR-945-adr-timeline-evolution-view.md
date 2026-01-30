---
adr: ADR-945
title: "ADR 全局时间线与演进视图"
status: Accepted
level: Governance
deciders: "Tech Lead"
date: 2026-01-26
version: "1.0"
maintainer: "Tech Lead"
primary_enforcement: L1
reviewer: "@douhuaa"
supersedes: null
superseded_by: null
---


# ADR-945：ADR 全局时间线与演进视图

> ⚖️ **本 ADR 定义 ADR 演进历史的可视化标准和自动化生成机制。**

**状态**：✅ Accepted（已采纳）  
## Focus（聚焦内容）

- ADR 演进历史时间线生成
- 变更频率统计和分析
- 高频修改 ADR 识别
- 演进视图可视化标准

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 时间线 | ADR 按时间顺序排列的演进历史 | Timeline |
| 演进视图 | ADR 变更频率和影响的可视化表示 | Evolution View |
| 变更频率 | ADR 在特定时间段内的修订次数 | Change Frequency |
| 高频 ADR | 变更频率超过阈值的 ADR | High-Frequency ADR |

---

---

## Decision（裁决）

### 自动生成 ADR 演进时间线（ADR-945.1）

**规则**：

**生成工具**：`scripts/generate-adr-timeline.sh`

**时间线位置**：`docs/adr/ADR-TIMELINE.md`

**时间线内容**：
1. 按时间倒序列出所有 ADR 变更
2. 包含：日期、ADR 编号、标题、版本、变更类型
3. 按年份和季度分组

**表格格式（主要视图）**：
| 日期 | ADR | 版本 | 变更类型 | 说明 |
|------|-----|------|----------|------|
| 2026-01-26 | ADR-940 | 1.0 | 新增 | ADR 关系管理 |
| 2026-01-20 | ADR-0001 | 1.1 | 修订 | 细化模块隔离规则 |

**可视化视图（抽样）**：
- Mermaid 甘特图仅显示近期活跃 ADR
- 默认范围：最近 1-2 个季度
- 或按变更频率 Top N（默认 Top 20）
- 避免全量可视化导致的可读性问题

**更新频率**：每次 ADR 变更后自动生成

**判定**：
- ❌ 时间线缺失或过时
- ✅ 时间线准确反映所有 ADR 变更

---

### 变更频率统计和分析（ADR-945.2）

**规则**：

**统计维度**：
1. 按 ADR 统计修订次数
2. 按时间段统计变更频率
3. 按层级统计活跃度

**生成报告位置**：`docs/adr/ADR-CHANGE-FREQUENCY-REPORT.md`

**报告内容**：
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
- [ADR-940：ADR 关系与溯源管理治理规范](ADR-940-adr-relationship-traceability-management.md) - 基于关系图数据

**被依赖（Depended By）**：无

**替代（Supersedes）**：无

**被替代（Superseded By）**：无

**相关（Related）**：
- [ADR-980：ADR 生命周期一体化同步机制](ADR-980-adr-lifecycle-synchronization-mechanism.md) - 版本追踪相关

---

---

## References（非裁决性参考）

### 工具和脚本

- `scripts/generate-adr-timeline.sh` - 时间线生成脚本
- `scripts/generate-change-frequency-report.sh` - 频率报告生成脚本

### 相关文档

- [ADR 时间线](../ADR-TIMELINE.md) - 演进时间线
- [ADR 变更频率报告](../ADR-CHANGE-FREQUENCY-REPORT.md) - 频率统计

---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
