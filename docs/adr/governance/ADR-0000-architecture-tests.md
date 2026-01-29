---
adr: ADR-0000
title: "架构测试与 CI 治理宪法"
status: Final
level: Governance
deciders: "Architecture Board"
date: 2026-01-23
version: "2.0"
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---


# ADR-0000：架构测试与 CI 治理宪法

> **唯一架构执法元规则**：本文件定义架构合法性评判的唯一基准。所有架构测试、CI 校验、Prompt 映射、破例治理均以本 ADR 正文为裁定源。

---

## Focus（聚焦内容）

- ADR-测试一一映射与唯一性
- 自动化校验与 CI 阻断机制
- 架构约束的分级测试与溯源跟踪
- 破例治理与到期归还
- Prompts/流程/文档合规自检闭环

---

---

## Glossary（术语表）

| 术语       | 定义                   |
|----------|----------------------|
| 架构测试     | 可自动执行的结构约束型测试        |
| ADR-测试映射 | ADR 【必须架构测试覆盖】→ 测试用例 |
| CI 阻断    | 测试失败即阻断 PR / 发布      |
| 破例       | 已批准的临时性违规（需归还）       |

---

---

## Decision（裁决）

- 所有【必须架构测试覆盖】的 ADR 条款，须有自动化测试（静态/语义/人工 Gate）
- 测试类、方法名、失败消息必须显式标注 ADR 编号
- 架构测试失败 = 架构违规，CI 阻断，无例外
- 所有 Prompts、流程、辅助材料以 ADR-0000 条款为准，发现冲突应修订辅导材料
- 测试组织须按 ADR 编号、内容、类型归档
- 三级执行级（L1静态、L2语义半自动、L3人工Gate），覆盖范围与映射表公开


### ADR-0000.1:L1 规则冲突时的优先级裁决

当 ADR 规则发生冲突时，**必须**按以下优先级从高到低裁决：

1. **架构安全与数据一致性**（如 ADR-220.2 Outbox Pattern）
2. **跨模块稳定性与演进性**（如 ADR-210.x 事件版本化）
3. **生命周期与资源管理**（如 ADR-201.x Handler 生命周期）
4. **结构规范与可维护性**（如 ADR-122.x 测试组织）
5. **流程与治理规则**（如 ADR-930.x 代码审查）

**裁决原则**：
- ✅ 优先保障系统正确性和稳定性
- ✅ 优先保障长期演进能力
- ✅ 优先保障资源安全和性能
- ❌ 规范性要求在必要时可妥协（需记录）

**冲突示例**：
- ADR-220.2（事务性发布）vs ADR-201.4（资源释放）→ 优先 220.2
- ADR-210.3（版本保留）vs ADR-122.1（代码清理）→ 优先 210.3

---

### ADR-0000.2:L1 破例必须绑定偿还计划与到期监控

所有架构破例**必须**绑定明确的偿还计划和到期监控机制。

**破例强制要求**：
- ✅ 到期版本号（如 v2.5.0）
- ✅ 偿还负责人
- ✅ 偿还计划和时间表
- ✅ CI 自动扫描过期破例并失败构建

**实施机制**：
```markdown

---

## Enforcement（执法模型）

### ADR-0000 测试方法映射表

| ADR 条款 | 执行级别 | 测试类型 | 测试名称 | 失败即 |
|---------|----------|----------|----------|--------|
| ADR-0000.1 | L1 | 静态规则 | ADR0000_Conflict_Priority_Tests | CI 阻断 |
| ADR-0000.2 | L1 | 语义/配置 | ADR0000_Exception_Repayment_Tests | CI 阻断 |

| 测试方法 | 说明 |
|----------|------|
| Each_ADR_Must_Have_Exact_And_Unique_Architecture_Test | 每条 ADR 必须有且仅有唯一对应的架构测试类 |
| Architecture_Test_Classes_Must_Have_Minimum_Assertions | 架构测试类必须包含最少断言数（反作弊） |
| Test_Failure_Messages_Must_Include_ADR_Number | 测试失败消息必须包含 ADR 编号（反作弊） |
| Architecture_Tests_Must_Not_Be_Skipped | 禁止跳过架构测试（反作弊） |

---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- 待补充

---

## Prohibited（禁止行为）

- 跳过架构测试（除非显式记录在 ARCH-VIOLATIONS，季度审计）
- 架构测试失败不阻断 CI
- 破例无偿还计划或负责人
- 未同步更新 ADR 映射表、Prompts
- 破例延期超过 2 次

---

---

## Relationships（关系声明）

**Depends On**：
- 无（本 ADR 为元规则，不依赖其他 ADR）

**Depended By**：
- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0002：Platform / Application / Host 三层启动体系](../constitutional/ADR-0002-platform-application-host-bootstrap.md)
- [ADR-0003：命名空间规则](../constitutional/ADR-0003-namespace-rules.md)
- [ADR-0004：CPM 架构合约](../constitutional/ADR-0004-Cpm-Final.md)
- [ADR-0005：应用内交互模型与执行边界](../constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [ADR-0006：术语与编号宪法](../constitutional/ADR-0006-terminology-numbering-constitution.md)
- [ADR-0007：Agent 行为与权限宪法](../constitutional/ADR-0007-agent-behavior-permissions-constitution.md)
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md)
- [ADR-900：ADR 流程与生命周期](./ADR-900-adr-process.md)
- [ADR-901：ADR 语义宪法](./ADR-901-warning-constraint-semantics.md)
- [ADR-903：架构测试命名与组织规范](./ADR-903-architecture-tests-naming-organization.MD)
- [ADR-905：架构约束分类与裁决实施映射](./ADR-905-enforcement-level-classification.md)
- [ADR-906：Analyzer 与 CI Gate 映射协议](./ADR-906-analyzer-ci-gate-mapping-protocol.md)
- [ADR-910：README 编写与维护宪法](./ADR-910-readme-governance-constitution.md)
- [ADR-920：示例治理宪法](./ADR-920-examples-governance-constitution.md)
- [ADR-930：代码审查与 ADR 合规自检流程](./ADR-930-code-review-compliance.md)
- [ADR-940：ADR 关系与溯源管理](./ADR-940-adr-relationship-traceability-management.md)
- [ADR-970：自动化工具日志集成标准](./ADR-970-automation-log-integration-standard.md)
- [ADR-980：ADR 生命周期同步与版本管理](./ADR-980-adr-lifecycle-synchronization.md)
- [ADR-301：集成测试自动化](../technical/ADR-301-integration-test-automation.md)
- [ADR-360：CI/CD Pipeline 流程标准化](../technical/ADR-360-cicd-pipeline-standardization.md)

**Supersedes（替代）**：
- 无

**Superseded By（被替代）**：
- 无

---


---

---

## References（非裁决性参考）


- 待补充


---

---

## History（版本历史）

| 版本  | 日期         | 变更说明              |
|-----|------------|-------------------|
|2.1  | 2026-02-10 | 补充 ADR-0000.X/Y 规则 |
| 2.0 | 2026-01-23 | 聚焦自动化与治理闭环，细化执行分级 |
| 1.0 | 2026-01-20 | 初版                |
