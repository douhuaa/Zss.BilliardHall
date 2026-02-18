---
adr: ADR-901
title: "语义元规则（Constraint / Warning / Notice）"
status: Final
level: Governance
deciders: "Architecture Board"
date: 2026-02-03
version: "2.0"
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
enforceable: false
---

# ADR-901：语义元规则（Constraint / Warning / Notice）

> ⚖️ **Constraint | L1** - 这是 ADR 体系中"风险与约束语言"的治理级元规则。  
> 本 ADR 统一定义：**风险表达的三态模型，以及它们如何被书写、识别和执行。**  
> 任何语义不合规的风险表达，**不具备治理效力**。

---

## Focus（聚焦内容）

本 ADR 聚焦解决以下结构性问题：

- 警告 / 注意 / 提示 / 约束 **语义混用**
- 同样的风险在不同 ADR / 文档中 **表达强度不一致**
- 人无法判断“这是建议还是硬性规则”
- 工具无法判断“这是必须阻断还是仅提示”
- CI / Review / 架构测试 **无法自动化识别风险等级**

**适用范围**：

- 所有 ADR
- 所有文档类规范（README / Governance / Docs ADR）
- 所有具有“警告、注意、约束、风险”表达的文本

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|----|----|----|
| 约束 | 违反即不允许的强制规则 | Constraint |
| 警告 | 强烈风险提示，可能阻断 | Warning |
| 提示 | 信息性提示，不构成约束 | Notice |
| Enforcement Level | 执行强度等级 | Enforcement Level |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-901 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-901_<Rule>_<Clause>
> ```

---

### ADR-901.1：风险表达语义模型（Rule）

#### ADR-901.1.1 风险表达必须使用三态语义模型

所有风险与提示 **必须** 明确归类为以下三种之一：

- `Constraint`
- `Warning`
- `Notice`

❌ 禁止出现：
- Suggestion
- Recommendation
- Attention
- Soft Rule
- Best Practice（若具约束性）

#### ADR-901.1.2 Constraint 的合法性条件

只有同时满足以下条件，才允许声明为 Constraint：

- 明确禁止 / 必须 / 不允许
- 明确适用范围
- 明确违反后果
- 明确执行级别
- 可被测试 / CI / Review 执行

否则 **必须降级**。

#### ADR-901.1.3 Warning 的边界

Warning **必须**：
- 明确风险后果
- 明确是否允许放行
- 明确放行责任主体
- 明确执行级别

❌ 禁止使用：
- “建议”
- “可以考虑”
- “最好”
- 暗含强制但不声明的表述

#### ADR-901.1.4 Notice 的纯信息性约束

Notice **只能**用于：
- 背景说明
- 设计动机
- 经验性解释

❌ Notice 中 **严禁**：
- MUST / SHOULD / SHALL
- 隐性规则
- 流程性约束

---

### ADR-901.2：语义执行与判定（Rule）

#### ADR-901.2.1 统一语义声明块

所有 Constraint / Warning / Notice  
**必须** 使用统一结构块，不允许自由文本表达。

**标准格式**：

```md
> 🚨 **Constraint | L1**
> **规则**：……
> **范围**：……
> **后果**：……
````

```md
> ⚠️ **Warning | L2**
> **风险**：……
> **放行**：……
```

```md
> ℹ️ **Notice**
> ……
```

#### ADR-901.2.2 不可识别语义等同不存在

任何风险表达如果：
- 无统一结构
- 无语义类型
- 无执行级别

→ **治理系统视为不存在**。

#### ADR-901.2.3 执行级别强制声明

所有 Constraint / Warning  
**必须显式声明执行级别**（L1 / L2 / L3）。

执行级别定义 **完全依赖 ADR-####**。

#### ADR-901.2.4 判定输出三态模型

所有风险表达在治理系统中 **必须被判定为**：

- ✅ **Allowed**：语义 + 格式 + 执行级别全部合规
- ⛔ **Blocked**：语义非法 / 伪装裁决
- ❓ **Uncertain**：语义合规但执行信息不完整

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-901 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-901.1.1** | L1 | ArchitectureTests 自动化验证语义类型 | §ADR-901.1.1 |
| **ADR-901.1.2** | L1 | ArchitectureTests 验证 Constraint 合法性 | §ADR-901.1.2 |
| **ADR-901.1.3** | L1 | ArchitectureTests 验证 Warning 边界 | §ADR-901.1.3 |
| **ADR-901.1.4** | L1 | ArchitectureTests 验证 Notice 纯信息性 | §ADR-901.1.4 |
| **ADR-901.2.1** | L1 | ArchitectureTests 验证统一结构块格式 | §ADR-901.2.1 |
| **ADR-901.2.2** | L1 | ArchitectureTests 检测不可识别语义 | §ADR-901.2.2 |
| **ADR-901.2.3** | L1 | ArchitectureTests 验证执行级别声明 | §ADR-901.2.3 |
| **ADR-901.2.4** | L1 | CI 系统三态判定输出 | §ADR-901.2.4 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决


## Non-Goals（明确不管什么）

- NLP 算法实现
- UI / 视觉表现
- 日志级别映射
- 国际化翻译
- 用户体验设计
- AI 置信度表达
- 法律免责声明
- 运行期异常处理

---

## Prohibited（禁止行为）

- 用 Warning 代替 Constraint
- 用 Notice 偷塞规则
- 不声明执行级别
- 双关语规避裁决
- 同一风险跨文档语义漂移

---

## Relationships（关系声明）

**Depends On**：

- [ADR-900：架构测试与 CI 治理元规则](ADR-900-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-900
- [ADR-905：架构执法等级定义](ADR-905-enforcement-level-classification.md) - 本 ADR 的执行级别定义依赖 ADR-905

**Depended By**：

- [ADR-902：ADR 结构与章节规范](./ADR-902-adr-template-structure-contract.md) - ADR 语义定义被 ADR 结构规范依赖
- [ADR-903：ArchitectureTests 命名与组织规范](ADR-903-architecture-tests-naming-organization.md) - 测试组织需要明确语义分类
- [ADR-904：ArchitectureTests 断言规范](ADR-904-architecturetests-minimum-assertion-semantics.md) - 断言规范需要明确语义分类
- [ADR-906：Analyzer 与 CI 阻断映射协议](ADR-906-analyzer-ci-gate-mapping-protocol.md) - 自动化阻断需要明确语义分类
- 所有文档类 ADR

---


## References（非裁决性参考）

- RFC 2119 / RFC 8174
- ISO/IEC/IEEE 42010

---

## History（版本历史）

| 版本 | 日期 | 变更说明 | 修订人 |
|------|------|----------|-------|
| 2.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
| 1.0 | 2025-01-30 | 初始正式版本 | Architecture Board |
