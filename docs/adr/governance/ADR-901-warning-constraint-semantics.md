---
adr: ADR-901
title: "语义宪法（Constraint / Warning / Notice）"
status: Final
level: Governance
deciders: "Architecture Board"
date: 2026-01-27
version: "1.0"
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---

# ADR-901：语义宪法（Constraint / Warning / Notice）

> ⚖️ **这是 ADR 体系中“风险与约束语言”的宪法。**  
> 本 ADR 统一定义：**什么是警告、什么是禁止、什么只是提示，以及它们如何被书写、识别和执行。**  
> 任何语义不合规的警告或提示，**不具备治理效力**。

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
| Constraint | 违反即不允许的强制规则 | Constraint |
| Warning | 强烈风险提示，可能阻断 | Warning |
| Notice | 信息性提示，不构成约束 | Notice |
| Enforcement Level | 执行强度等级 | Enforcement Level |

---

## 裁决（Decision）

> ⚠️ **本节是唯一裁决来源，其他章节不得产生新规则。**

### 规则列表

**ADR-901.1：风险表达必须使用三态语义模型**

所有风险与提示 **必须** 明确归类为以下三种之一：

- Constraint（约束）
- Warning（警告）
- Notice（提示）

禁止自定义第四种语义。

**ADR-901.2：Constraint 必须具备阻断语义**

Constraint **必须**：
- 明确“禁止 / 不允许 / 必须”
- 明确适用范围
- 明确违反后果
- 可被测试或流程阻断

否则不得称为 Constraint。

**ADR-901.3：Warning 不得伪装为建议**

Warning **必须**：
- 明确说明风险后果
- 明确是否可被人工放行
- 不得使用“建议”“可以考虑”等弱语气

模糊 Warning 视为无效。

**ADR-901.4：Notice 不得携带裁决语义**

Notice **只能**用于：
- 背景信息
- 解释性说明
- 设计动机

Notice 中 **严禁** 出现任何强制或隐性约束。

**ADR-901.5：语义必须使用统一格式声明**

所有 Constraint / Warning / Notice  
**必须** 使用统一结构块表达，不允许自由发挥。

**ADR-901.6：语义级别必须可被工具识别**

任何风险表达如果无法被工具或规则扫描识别，  
**等同不存在**。

**ADR-901.7：风险语义必须声明执行级别**

每一条 Constraint / Warning  
**必须** 显式声明执行级别（L1 / L2 / L3）。


### 判定输出语义

基于本 ADR，任一风险表达在治理系统中 **必须** 被判定为：

- ✅ Allowed：语义、格式、执行级别全部合规
- ⚠️ Blocked：语义非法或伪装裁决
- ❓ Uncertain：语义合规但执行策略不完整

---

## 执法模型（Enforcement）

### 测试映射

| 规则编号 | 执行级 | 执法方式 |
|----|----|----|
| ADR-901.1 | L1 | 风险语义关键字扫描 |
| ADR-901.2 | L1 | Constraint 结构完整性校验 |
| ADR-901.3 | L2 | Review + 语义审计 |
| ADR-901.4 | L1 | Notice 违规语义扫描 |
| ADR-901.5 | L1 | 标准块格式校验 |
| ADR-901.6 | L1 | 工具可识别性校验 |
| ADR-901.7 | L2 | 执行级别人工审查 |

---

## Non-Goals（明确不管什么）

本 ADR **不负责**：
- 文案措辞优雅程度
- 教学性说明
- 示例内容
- 业务规则设计
- 风险是否“应该存在”

---

## 结构标准（Structure Standard）

### Constraint

```
❌ Constraint
- 描述：
- 适用范围：
- 违反后果：
- 执行级别：
```

### Warning

```
⚠️ Warning
- 风险说明：
- 可否放行：
- 执行级别：
```

### Notice

```
ℹ️ Notice
- 说明内容：
```

---

## Prohibited（禁止行为）

严禁：
- 用 Warning 代替 Constraint
- 用 Notice 偷塞规则
- 不声明执行级别
- 使用“建议但实际上必须”的双关语
- 同一风险在不同文档中语义降级或升级

---

## Relationships（关系声明）

**Depends On**：

- [ADR-0000：架构测试与 CI 治理宪法](ADR-0000-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-0000

**Depended By**：

- [ADR-902：ADR 结构与章节规范](./ADR-902-adr-template-structure-contract.md) - ADR 语义定义被 ADR 结构规范依赖
- 所有文档类 ADR
- 所有治理与校验规则

---

## References（非裁决性参考）

- RFC 2119 / RFC 8174
- ISO/IEC/IEEE 42010

---


---

## Non-Goals（明确不管什么）


本 ADR 明确不涉及以下内容：

- 待补充


---

## Enforcement（执法模型）


### 执行方式

待补充...


---

## Decision（裁决）


> ⚠️ **本节是唯一裁决来源，其他章节不得产生新规则。**

### 待补充规则

待补充...


## History（版本历史）

| 版本 | 日期         | 变更说明 | 作者 |
|----|------------|----|----|
| 1.0 | 2025-01-28 | 初始正式版本 | Architecture Board |

