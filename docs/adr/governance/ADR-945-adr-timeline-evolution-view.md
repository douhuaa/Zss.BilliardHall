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
**版本**：1.0
**级别**：治理层 / 工具规范  
**适用范围**：所有 ADR 文档  
**生效时间**：即刻

---

## Focus（聚焦内容）

- ADR 演进历史时间线生成
- 变更频率统计和分析
- 高频修改 ADR 识别
- 演进视图可视化标准

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 时间线 | ADR 按时间顺序排列的演进历史 | Timeline |
| 演进视图 | ADR 变更频率和影响的可视化表示 | Evolution View |
| 变更频率 | ADR 在特定时间段内的修订次数 | Change Frequency |
| 高频 ADR | 变更频率超过阈值的 ADR | High-Frequency ADR |

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
## ADR 变更频率报告

**报告期**：2026 Q1

### 总体统计
- 总 ADR 数：15
- 本期新增：3
- 本期修订：5
- 本期废弃：1

### 高频修改 ADR（≥3 次）
| ADR | 标题 | 修订次数 | 平均间隔 | 最近修订 |
|-----|------|----------|----------|----------|
| ADR-0001 | 模块化架构 | 5 | 12 天 | 2026-01-20 |
| ADR-0005 | 交互模型 | 4 | 15 天 | 2026-01-18 |

### 按层级活跃度
| 层级 | ADR 数 | 修订次数 | 平均频率 |
|------|--------|----------|----------|
| 宪法层 | 5 | 8 | 1.6 |
| 治理层 | 3 | 12 | 4.0 |

### 观察结果（仅供参考）
- **高频修订 ADR**：ADR-0001（频繁修订可能表示规则演进中）
- **信息价值**：供团队讨论和架构回顾参考
```

**更新频率**：每季度生成

**判定**：
- ❌ 报告缺失或数据不准确
- ✅ 报告准确反映 ADR 变更频率

---

### 高频修改 ADR 识别（ADR-945.3）

**规则**：

**识别阈值**：
- 季度修订次数 ≥ 3 次
- 或平均修订间隔 < 14 天

**自动标记**：
在 ADR-CHANGE-FREQUENCY-REPORT.md 中标记高频 ADR

**Telemetry Only（仅观察）**：
高频 ADR 识别结果 **仅用于信息展示**，**禁止**作为以下用途：
- ❌ CI 阻断条件
- ❌ PR 自动拒绝
- ❌ 强制 Review 要求
- ❌ 合并权限控制

**核心原则**：
> ADR-945 永远只做 Telemetry（遥测），不做 Judgement（裁决）

**永久禁止升级条款（不可协商）**：
- 任何频率/时间分布指标 **永远不得** 升级为 CI Gate
- 任何演进统计 **永远不得** 作为 Merge Block 条件
- 本条款 **不可**通过 ADR 修订放宽
- 违反此条款即违反 ADR-945 根本原则

**信息用途**：
识别结果仅供人工参考：
1. 提供架构演进洞察
2. 辅助团队讨论
3. 识别潜在改进点

**判定**：
- ❌ 高频 ADR 未识别
- ❌ 识别结果被用于自动化决策
- ✅ 高频 ADR 已识别且仅用于信息展示

---

## 执法模型（Enforcement）

### 测试映射

| 规则编号 | 执行级 | 测试/手段 |
|---------|--------|----------|
| ADR-945.1 | L2 | `scripts/generate-adr-timeline.sh` |
| ADR-945.2 | L2 | `scripts/generate-change-frequency-report.sh` |
| ADR-945.3 | L2 | 报告中包含识别结果 |

### CI 集成

```yaml
# .github/workflows/adr-timeline.yml
name: Generate ADR Timeline

on:
  push:
    paths:
      - 'docs/adr/**/*.md'

jobs:
  generate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Generate Timeline
        run: ./scripts/generate-adr-timeline.sh
      - name: Commit Changes
        run: |
          git config user.name "ADR Bot"
          git config user.email "bot@example.com"
          git add docs/adr/ADR-TIMELINE.md
          git commit -m "docs: 更新 ADR 时间线" || echo "No changes"
```

---

## 破例与归还（Exception）

### 允许破例的前提

破例 **仅在以下情况允许**：
- 工具开发初期，手动维护时间线

### 破例要求

每个破例 **必须**：
- 记录在 `ARCH-VIOLATIONS.md`
- 指定失效日期（不超过 1 个月）
- 给出自动化计划

### 治理熔断（Governance Kill-Switch）

在以下情况下，**仅 Maintainer 级别**可临时关闭 ADR-945 自动生成机制：

**熔断触发条件**：
- ADR 编号规则发生系统性迁移
- Git 历史重写（rebase / filter-repo / squash）
- 脚本存在已知缺陷但尚未修复
- 批量 ADR 重构期间

**权限约束**：
- 触发人：**仅 Maintainer 角色**
- 审批：需在 Issue 中说明理由
- 通知：必须通知架构委员会

**熔断要求**：
1. **明确关闭范围**：
   - Timeline 生成
   - Frequency Report 生成
   - 或两者

2. **指定恢复条件**：
   - 具体的技术条件（如"迁移完成"）
   - 或明确的时间点（如"2026-02-15"）

3. **记录熔断**：
   - 在 `ARCH-VIOLATIONS.md` 中记录
   - 标注为 `ADR-945-KILL-SWITCH`
   - 包含熔断原因和恢复计划

4. **事后审计**：
   - 熔断结束后，必须提交 Issue 总结
   - 包含：触发原因、持续时间、影响范围、经验教训

**熔断期限**：
- 不超过 2 周
- 超期自动失效，工具恢复运行

**判定**：
- ❌ 熔断未记录或超期
- ✅ 熔断按规范执行且及时恢复

---

## 变更政策（Change Policy）

### 变更规则

- **修改阈值**：Tech Lead 可批准
- **修改格式**：Tech Lead 可批准
- **修改工具**：Tech Lead 可批准

---

## Non-Goals（明确不管什么）

本 ADR **不负责**：
- ADR 内容质量评审
- ADR 变更原因分析（由人工审查）
- ADR 优先级排序

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-940：ADR 关系与溯源管理宪法](ADR-940-adr-relationship-traceability-management.md) - 基于关系图数据

**被依赖（Depended By）**：无

**替代（Supersedes）**：无

**被替代（Superseded By）**：无

**相关（Related）**：
- [ADR-980：ADR 生命周期一体化同步机制](ADR-980-adr-lifecycle-synchronization-mechanism.md) - 版本追踪相关

---

## References（非裁决性参考）

### 工具和脚本

- `scripts/generate-adr-timeline.sh` - 时间线生成脚本
- `scripts/generate-change-frequency-report.sh` - 频率报告生成脚本

### 相关文档

- [ADR 时间线](../ADR-TIMELINE.md) - 演进时间线
- [ADR 变更频率报告](../ADR-CHANGE-FREQUENCY-REPORT.md) - 频率统计

---

## 版本历史

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|----------|------|
| 1.0 | 2026-01-26 | 初始版本，定义 ADR 演进视图 | GitHub Copilot |




---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充


---

## Non-Goals（明确不管什么）


本 ADR 明确不涉及以下内容：

- 待补充


---

## Enforcement（执法模型）


### 执行方式

待补充...


---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 待补充 | 初始版本 |
