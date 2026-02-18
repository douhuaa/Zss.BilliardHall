---
adr: ADR-900
title: "架构测试与 CI 治理元规则"
status: Final
level: Governance
deciders: "Architecture Board"
date: 2026-02-03
version: "4.0"
maintainer: "Architecture Board"
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---

# ADR-900：架构测试与 CI 治理元规则

>⚖️ 唯一架构裁决源声明 
>本 ADR 是关于「架构合法性判定、CI 执法、破例治理」的最高治理级元规则。
所有架构测试、CI Gate、Analyzer、Prompt、文档与流程 不得凌驾于本 ADR 之上。

---

## Focus（聚焦内容）

- ADR-测试一一映射与唯一性
- 自动化校验与 CI 阻断机制
- 架构约束的分级测试与溯源跟踪
- 破例治理与到期归还
- Prompts/流程/文档合规自检闭环

---

## Glossary（术语表）

| 术语       | 定义                   |
|----------|----------------------|
| 架构测试     | 可自动执行的结构约束型测试        |
| ADR-测试映射 | ADR 【必须架构测试覆盖】→ 测试用例 |
| CI 阻断    | 测试失败即阻断 PR / 发布      |
| 破例       | 已批准的临时性违规（需归还）       |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-900 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-900_<Rule>_<Clause>
> ```

---

### ADR-900.1：架构裁决权威性（Rule）

#### ADR-900.1.1 审判权唯一性

- **ADR 正文是唯一裁决依据**
- README、Prompt、示例、脚本 **不具备裁决权**
- 若出现冲突，**以 ADR 正文为准**

> 判定理由：  
> 这是**确定性、静态可校验规则**，CI 可直接阻断 → **L1**

#### ADR-900.1.2 架构违规的判定原则

以下任一条件成立，即构成架构违规：

- 必须架构测试失败
- CI Gate / Analyzer 判定失败
- 人工 Gate 明确否决
- 存在未记录或已过期的破例

**架构违规 = CI 阻断（无例外）**

> 判定理由：  
> 结果确定、可自动阻断 → **L1**

---

### ADR-900.2：执行级别与测试映射（Rule）

#### ADR-900.2.1 执行级别分离原则

所有架构规则 **必须**被归类到执行级别之一：

- **L1**：静态、确定性、可自动阻断
- **L2**：语义型、需人工确认
- **L3**：不可程控、仅人工 Gate

> 执行级别的定义、判定标准与适用范围  
> **由 ADR-#### 唯一裁定**

> 判定理由：  
> 这是**分类强制规则**，不允许模糊 → **L1**

#### ADR-900.2.2 ADR ↔ 测试 ↔ CI 的一一映射

- 所有【必须架构测试覆盖】的 ADR 条款：
  - 必须存在可追溯的测试或 Gate
  - 测试与失败信息必须显式标注 ADR 编号

- 映射缺失 = 流程违规

> 判定理由：  
> 映射关系可被工具扫描 → **L1**

---

### ADR-900.3：破例治理机制（Rule）

#### ADR-900.3.1 破例强制要求

任何架构破例 **必须同时满足**：

- 显式记录（ADR + ARCH-VIOLATIONS）
- 明确到期版本
- 明确责任人
- 明确归还方案
- 可被 CI 自动扫描

**过期破例 = 架构违规**

> 判定理由：  
> 破例是否存在、是否过期是确定事实 → **L1**

#### ADR-900.3.2 CI 自动监控机制

CI **必须**自动扫描破例状态：

- 每月第一次构建扫描 arch-violations.md
- 发现过期破例 → 构建失败
- 强制团队偿还或延期（需重新审批）

**实施机制**：
```markdown
## arch-violations.md 格式

| ADR | 规则 | 到期版本 | 负责人 | 偿还计划 | 状态 |
|-----|------|---------|--------|---------|------|
| ADR-201.1 | Handler Scoped | v2.5.0 | @dev | 迁移至 Scoped | 🚧 |
```

> 判定理由：  
> CI 扫描机制可自动化执行 → **L1**

---

### ADR-900.4：冲突裁决优先级（Rule）

#### ADR-900.4.1 裁决优先级顺序

当 ADR 规则发生冲突时，裁决顺序为：

1. 架构安全与数据一致性
2. 系统稳定性与演进能力
3. 生命周期与资源安全
4. 结构一致性与可维护性
5. 流程与治理便利性

低优先级规则 **可以被临时牺牲，但必须记录破例**。

**冲突示例**：
- ADR-####.2（事务性发布）vs ADR-####.4（资源释放）→ 优先 220.2
- ADR-####.3（版本保留）vs ADR-####.1（代码清理）→ 优先 210.3

> 判定理由：  
> 冲突裁决 **必然涉及语境判断与权衡**，不可自动化 → **L3**

---


## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-900 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-900.1.1** | L1 | ArchitectureTests 验证 ADR 正文唯一裁决权 | §ADR-900.1.1 |
| **ADR-900.1.2** | L1 | ArchitectureTests 验证架构测试失败阻断 CI | §ADR-900.1.2 |
| **ADR-900.2.1** | L1 | ArchitectureTests 验证所有规则已分类至 L1/L2/L3 | §ADR-900.2.1 |
| **ADR-900.2.2** | L1 | ArchitectureTests 验证 ADR ↔ 测试映射完整性 | §ADR-900.2.2 |
| **ADR-900.3.1** | L1 | ArchitectureTests 验证破例记录完整性 | §ADR-900.3.1 |
| **ADR-900.3.2** | L1 | CI 自动扫描过期破例并失败构建 | §ADR-900.3.2 |
| **ADR-900.4.1** | L3 | 人工审查：架构委员会裁决冲突规则优先级 | §ADR-900.4.1 |

### 执行级别说明

- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构委员会人工裁决

---

## Non-Goals（明确不管什么）

- 不定义具体架构规则
- 不规定测试技术选型
- 不描述 CI Pipeline 细节
- 不提供教学或示例代码

---

## Prohibited（禁止行为）

- 跳过架构测试（除非显式记录在 ARCH-VIOLATIONS，季度审计）
- 架构测试失败不阻断 CI
- 破例无偿还计划或负责人
- 破例延期超过 2 次

---

## Relationships（关系声明）

**Depends On**：
- 无（本 ADR 为元规则，不依赖其他 ADR）

**Depended By**：
- [ADR-001：模块化单体与垂直切片架构](../constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)
- [ADR-002：Platform / Application / Host 三层启动体系](../constitutional/ADR-002-platform-application-host-bootstrap.md)
- [ADR-003：命名空间规则](../constitutional/ADR-003-namespace-rules.md)
- [ADR-004：CPM 架构合约](../constitutional/ADR-004-Cpm-Final.md)
- [ADR-005：应用内交互模型与执行边界](../constitutional/ADR-005-Application-Interaction-Model-Final.md)
- [ADR-006：术语与编号宪法](../constitutional/ADR-006-terminology-numbering-constitution.md)
- [ADR-007：Agent 行为与权限宪法](../constitutional/ADR-007-agent-behavior-permissions-constitution.md)
- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md)
- [ADR-900：ADR 流程与生命周期](./ADR-900-architecture-tests.md)
- [ADR-901：ADR 语义元规则](./ADR-901-warning-constraint-semantics.md)
- [ADR-903：架构测试命名与组织规范](./ADR-903-architecture-tests-naming-organization.MD)
- [ADR-905：架构约束分类与裁决实施映射](./ADR-905-enforcement-level-classification.md)
- [ADR-906：Analyzer 与 CI Gate 映射协议](./ADR-906-analyzer-ci-gate-mapping-protocol.md)
- [ADR-910：README 编写与维护治理规范](./ADR-910-readme-governance-constitution.md)
- [ADR-920：示例治理规范](./ADR-920-examples-governance-constitution.md)
- [ADR-930：代码审查与 ADR 合规自检流程](./ADR-930-code-review-compliance.md)
- [ADR-940：ADR 关系与溯源管理](./ADR-940-adr-relationship-traceability-management.md)
- [ADR-970：自动化工具日志集成标准](./ADR-970-automation-log-integration-standard.md)
- [ADR-980：ADR 生命周期同步与版本管理](./ADR-980-adr-lifecycle-synchronization.md)
- [ADR-301：集成测试自动化](../technical/ADR-301-integration-test-automation.md)
- [ADR-360：CI/CD Pipeline 流程标准化](../technical/ADR-360-cicd-pipeline-standardization.md)

**Supersedes**：
- 无

**Superseded By**：
- 无

---

## References

---

## History（版本历史）

| 版本  | 日期         | 变更说明              |
|-----|------------|-------------------|
| 4.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
|3.1| 2026-01-30 | 术语优化：宪法→元规则，消除与Constitutional层混淆 | Architecture Board |
|3.0| 2026-01-29 | 治理级重写，剥离实现细节 | Architecture Board |
|2.1  | 2026-01-24 | 补充 ADR-900.X/Y 规则 | Architecture Board |
| 2.0 | 2026-01-23 | 聚焦自动化与治理闭环，细化执行分级 | Architecture Board |
| 1.0 | 2026-01-20 | 初版                | Architecture Board |
