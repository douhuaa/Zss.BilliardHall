---
adr: ADR-009
title: "Guardian 决策失败与反馈宪法"
status: Draft
level: Constitutional
version: "0.1"
deciders: "Architecture Board"
date: 2026-02-03
maintainer: "Architecture Board"
primary_enforcement: L1/L2
reviewer: "Architecture Board"
depends_on: ADR-007
supersedes: null
superseded_by: null
---

# ADR-009：Guardian 决策失败与反馈宪法

> ⚖️ **本 ADR 是 ADR-007 的补充宪法，用于定义 Guardian 决策的失败模型、纠偏机制与治理反馈闭环。**  
> 本 ADR 的目标不是限制 Guardian 的协调能力，而是**防止 Guardian 在实践中演化为事实裁决者**。
> ⚠️ 本 ADR 当前处于 **Dormant / Deferred Enforcement** 状态，
> 除非在 ADR-007 指定的条件下显式启用，否则不参与 CI 或 Guardian 自动裁决。

---

## Focus（聚焦内容）

- Guardian 决策的失败类型定义
- False Allow / False Block 的制度化识别
- Guardian 决策的可审计性要求
- 人类与测试对 Guardian 的纠偏机制
- Guardian 决策的治理反馈闭环

---

## Glossary（术语表）

| 术语 | 定义 |
|-----|------|
| Guardian Decision | Guardian 对 Agent 输出进行协调后的最终三态结论 |
| False Allow | Guardian 判定 Allowed，但后续被 ADR / 测试 / 人类认定为错误 |
| False Block | Guardian 判定 Blocked，但后续被 ADR / 测试 / 人类认定为错误 |
| Decision Evidence | Guardian 决策所引用的 ADR 条款与测试证据 |
| Feedback Loop | 从错误识别到治理修正的闭环流程 |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**

> 🔒 **统一原则**：  
> Guardian 的任何决策都不是终态事实，而是**可被验证、可被否定、可被纠偏的中间治理产物**。

---

### ADR-009_1：Guardian 决策失败模型（Rule）

#### ADR-009_1_1 失败类型定义
Guardian 决策必须被允许归类为以下失败类型之一：

- **False Allow**：
  - Guardian 输出 ✅ Allowed
  - 后续出现以下任一情况：
    - 架构测试失败
    - ADR 明确禁止该行为
    - 架构委员会裁定为不合规

- **False Block**：
  - Guardian 输出 ⚠️ Blocked
  - 后续出现以下任一情况：
    - ADR 明确允许该行为
    - 架构委员会裁定 Guardian 阻断不成立

#### ADR-009_1_2 明确排除项
- ❌ Uncertain 不计入失败类型
- ❌ Prompts 变化不构成失败依据

---

### ADR-009_2：Guardian 决策可审计性（Rule）

#### ADR-009_2_1 决策证据要求
Guardian 的每一次三态输出**必须**显式包含：

- 引用的 ADR 编号与 Clause
- 相关测试名称（如存在）
- 判定路径摘要（不超过 5 行）

#### ADR-009_2_2 禁止行为
- ❌ 无 ADR 引用的 Allowed / Blocked
- ❌ 使用"综合判断"、"经验判断"等不可审计表述

---

### ADR-009_3：纠偏触发机制（Rule）

#### ADR-009_3_1 自动纠偏（L1）

以下情况必须自动触发纠偏：

- Guardian 判定 Allowed，CI 架构测试失败
- Guardian 判定 Blocked，但存在明确通过的 ADR 测试

触发结果：
- 该 Guardian 决策被标记为 **Invalid**
- 本次决策不得作为后续判例引用

#### ADR-009_3_2 人工纠偏（L2）

以下情况允许人工触发纠偏：

- 架构委员会 Review
- 标注 Issue：`guardian-false-decision`

---

### ADR-009_4：治理反馈闭环（Rule）

#### ADR-009_4_1 反馈对象
一次 Guardian 决策失败**必须**反馈至以下至少一项：

- ADR 澄清或补充
- 架构测试增强
- Agent 职责或配置调整

#### ADR-009_4_2 禁止行为
- ❌ 仅修正 Guardian Prompt 而不修正文档或测试
- ❌ 将失败视为个案而不产生治理产物

---

### ADR-009_5：Guardian 非判例原则（Rule）

#### ADR-009_5_1 非判例声明

Guardian 的历史决策：
- 不构成架构先例
- 不具备约束后续设计的法律效力
- 只能作为问题线索

#### ADR-009_5_2 禁止行为
- ❌ "之前 Guardian 允许过"
- ❌ "按照惯例 Guardian 会允许"

---

## Enforcement（执法模型）

| Rule / Clause | 执行级 | 执法方式 |
|--------------|-------|----------|
| ADR-009_1_1 | L1 | 决策结果一致性校验 |
| ADR-009_2_1 | L1 | Guardian 输出结构扫描 |
| ADR-009_3_1 | L1 | CI 自动失效处理 |
| ADR-009_3_2 | L2 | 人工 Review |
| ADR-009_4_1 | L2 | 治理产物检查 |
| ADR-009_5_1 | L1 | 判例引用检测 |

---

## Non-Goals（非目标）

- 不定义 Guardian 的实现方式
- 不规定日志系统或存储结构
- 不要求实时决策回滚
- 不涉及 Agent 性能或模型选择

---

## Relationships（关系）

**Depends On**：
- [ADR-007](../ADR-007-agent-behavior-permissions-constitution.md) - 定义 Guardian 的基本权限与行为边界

**Depended By**：
- 所有 Guardian 配置与 Instructions

**Supersedes**：
- 无

**Superseded By**：
- 无

**Related**：
- [ADR-900（架构测试与 CI 治理）](../../governance/ADR-900-architecture-tests.md)

---

## Activation Condition（激活条件）

**Activation Condition**：
- Guardian 决策被引用为判例
- Guardian 决策与 ADR/Test 冲突 ≥ N 次
- 架构委员会显式启用

---

## Rationale（设计动机）

> 一个无法被证明错误的协调者，迟早会被当作裁决者。  
> 本 ADR 的存在，是为了确保 Guardian **永远只能是治理工具，而不是权力来源**。

---

## History

| 版本 | 日期 | 说明 |
|----|------|------|
| 0.1 | 2026-02-03 | 初始草案，引入 Guardian 失败与纠偏模型 |


---

## Guardian Failure Object（规范性定义）

> **本对象是 Guardian 失败的唯一合法表达形式。**
> 
> 任何未能生成本对象的失败，视为 *未发生过*，不得进入治理体系。

### FailureObject（抽象模型）

```json
{
  "failure_id": "GF-YYYYMMDD-XXXX",
  "guardian": "ArchitectureGuardian",
  "failure_level": "Warning | Error | Fatal",
  "failure_type": "RuleViolation | Ambiguity | MissingContext | InconsistentADR | ToolingFailure",
  "decision_context": {
    "trigger": "PR | CI | ManualReview | AgentDelegation",
    "input_artifacts": ["ADR-0000", "ADR-0240", "PullRequest#123"],
    "assumptions": ["Assumption A", "Assumption B"]
  },
  "violated_rules": [
    {
      "adr": "ADR-0240",
      "rule_id": "ADR-0240.2",
      "description": "Handler must not swallow domain exceptions"
    }
  ],
  "guardian_judgement": {
    "confidence": "High | Medium | Low",
    "rationale": "Concise but explicit reasoning path",
    "non_decidable_reason": null
  },
  "required_followups": [
    "Add ArchitectureTest",
    "Clarify ADR-0240 exception semantics"
  ],
  "escalation": {
    "required": true,
    "target": "ArchitectureCouncil | HumanArchitect"
  },
  "timestamp": "2026-01-31T12:00:00Z"
}
```

---

### Failure Level 语义（强约束）

- **Warning**：
  - Guardian 能给出判断
  - 不阻断流程
  - 必须记录并可查询

- **Error**：
  - Guardian 判断存在明确违规
  - 必须阻断 CI / PR
  - 必须能追溯到具体 ADR Rule

- **Fatal**：
  - Guardian 无法在宪法框架内做出裁决
  - 或发现 ADR 体系自相矛盾
  - **必须升级，不允许自动化兜底**

---

### 宪法级约束（不可违背）

1. Guardian **不得**在 Fatal 情况下给出任何“建议性决策”
2. Guardian **不得**吞掉 FailureObject，仅输出自然语言
3. FailureObject **必须**可被 ArchitectureTests 反向断言
4. 所有 FailureObject **都是治理资产，不是运行日志**

---

### 明确非目标（Anti-Goals）

- ❌ 不定义具体实现语言
- ❌ 不定义 Agent Prompt
- ❌ 不提供 Retry / Fallback 策略

> 这些内容必须由派生 ADR 决定，禁止在本宪法中偷渡。

