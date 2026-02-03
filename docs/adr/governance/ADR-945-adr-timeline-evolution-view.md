---
adr: ADR-945
title: "ADR 全局时间线与演进视图"
status: Accepted
level: Governance
deciders: "Tech Lead"
date: 2026-02-03
version: "2.0"
maintainer: "Tech Lead"
primary_enforcement: L1
reviewer: "@douhuaa"
supersedes: null
superseded_by: null
---

# ADR-945：ADR 全局时间线与演进视图

> ⚖️ **本 ADR 定义 ADR 演进历史的可视化标准和自动化生成机制。**

## Focus（聚焦内容）

- ADR 演进历史时间线生成
- 变更频率统计和分析
- 高频修改 ADR 识别
- 演进视图可视化标准

---

## Glossary（术语表）

| 术语     | 定义                | 英文对照               |
|--------|-------------------|--------------------|
| 时间线    | ADR 按时间顺序排列的演进历史  | Timeline           |
| 演进视图   | ADR 变更频率和影响的可视化表示 | Evolution View     |
| 变更频率   | ADR 在特定时间段内的修订次数  | Change Frequency   |
| 高频 ADR | 变更频率超过阈值的 ADR     | High-Frequency ADR |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
>
> 🔒 **统一铁律**：
>
> ADR-945 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-945_<Rule>_<Clause>
> ```

---

### ADR-945_1：ADR 演进时间线生成机制（Rule）

#### ADR-945_1_1 自动生成 ADR 演进时间线

- 使用脚本：`scripts/generate-adr-timeline.sh`
- 输出位置：`docs/adr/ADR-TIMELINE.md`
- 内容：
  1. 按时间倒序列出所有 ADR 变更
  2. 包含：日期、ADR 编号、标题、版本、变更类型
  3. 按年份和季度分组
- 可视化：
  - Mermaid 甘特图仅显示近期活跃 ADR
  - 默认显示最近 1-2 个季度，或变更频率 Top N（默认 Top 20）
- 更新频率：每次 ADR 变更后自动生成
- 判定：
  - ❌ 时间线缺失或过时
  - ✅ 时间线准确反映所有 ADR 变更

---

### ADR-945_2：变更频率统计与分析（Rule）

#### ADR-945_2_1 变更频率统计和分析

- 统计维度：
  1. 按 ADR 统计修订次数
  2. 按时间段统计变更频率
  3. 按层级统计活跃度
- 输出报告位置：`docs/adr/ADR-CHANGE-FREQUENCY-REPORT.md`
- 内容：
  - 变更次数、活跃 ADR 列表、可视化柱状图/折线图
- 判定：
  - ❌ 统计报告缺失或未更新
  - ✅ 数据准确，反映真实变更频率

---

### ADR-945_3：高频 ADR 警示与治理（Rule）

#### ADR-945_3_1 高频 ADR 警示与治理

- 高频 ADR 定义：变更频率超过阈值（默认 3 次/季度）
- CI 自动分析高频 ADR
- 警示措施：
  1. 在 PR 评论中标注
  2. 生成高频 ADR 报告
- 判定：
  - ❌ 高频 ADR 未标注
  - ✅ 高频 ADR 被识别并记录

---

### ADR-945_4：演进视图可视化标准（Rule）

#### ADR-945_4_1 演进视图可视化标准

- 输出位置：`docs/adr/ADR-EVOLUTION-VIEW.md`
- 可视化标准：
  - 年度/季度分层
  - 高频 ADR 高亮
  - 活跃层级颜色区分
- 更新频率：每次 ADR 变更后自动生成
- 判定：
  - ❌ 演进视图缺失或过时
  - ✅ 演进视图准确反映历史和频率

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
>
> 下表展示了 ADR-945 各条款（Clause）的执法方式及执行级别。

| 规则编号            | 执行级 | 执法方式 | Decision 映射  |
|-----------------|-----|------|--------------|
| **ADR-945_1_1** | L1  |      | §ADR-945_1_1 |
| **ADR-945_2_1** | L1  |      | §ADR-945_2_1 |
| **ADR-945_3_1** | L2  |      | §ADR-945_3_1 |
| **ADR-945_4_1** | L2  |      | §ADR-945_4_1 |

### 执行级别说明

- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决

---

## Non-Goals（明确不管什么）

- ADR 内容质量评审
- ADR 审批流程
- 版本号管理
- 可视化样式微调

---

## Prohibited（禁止行为）

- ❌ 手动更新时间线或频率报告而不通过脚本
- ❌ 忽略高频 ADR 警示
- ❌ 演进视图与实际 ADR 数据不一致

---

## Relationships（关系声明）

### Depends On

- [ADR-940：ADR 关系与溯源管理治理规范](ADR-940-adr-relationship-traceability-management.md)

### Depended By

- 无

### Supersedes

- 无

### Superseded By

- 无

### Related

- [ADR-980：ADR 生命周期一体化同步机制](ADR-980-adr-lifecycle-synchronization-mechanism.md)

---

## References（非裁决性参考）

**工具和脚本**：

- `scripts/generate-adr-timeline.sh`
- `scripts/generate-change-frequency-report.sh`

**相关文档**：

- [ADR 时间线](../ADR-TIMELINE.md)
- [ADR 变更频率报告](../ADR-CHANGE-FREQUENCY-REPORT.md)
- [演进视图示例](../ADR-EVOLUTION-VIEW.md)

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                  | 修订人                |
|-----|------------|---------------------------------------|--------------------|
| 2.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
| 1.0 | 2026-01-29 | 初始版本，定义 ADR 时间线和演进视图标准                | Tech Lead          |
