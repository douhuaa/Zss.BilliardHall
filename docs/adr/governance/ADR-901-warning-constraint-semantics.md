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

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|----|----|----|
| Constraint | 违反即不允许的强制规则 | Constraint |
| Warning | 强烈风险提示，可能阻断 | Warning |
| Notice | 信息性提示，不构成约束 | Notice |
| Enforcement Level | 执行强度等级 | Enforcement Level |

---

---

## Decision（裁决）


> ⚠️ **本节是唯一裁决来源，其他章节不得产生新规则。**

### 待补充规则

待补充...

---

## Enforcement（执法模型）


### 执行方式

待补充...


---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **自然语言处理算法**：不涉及如何用 NLP 技术解析语义的具体算法实现
- **UI 设计中的视觉提示**：不规定警告图标、颜色、字体等视觉元素的设计规范
- **国际化和本地化**：不涉及多语言环境下的语义表达和翻译规范
- **日志记录的格式**：不规定日志系统中的警告级别格式和输出方式
- **用户体验设计**：不涉及如何让用户更好理解和响应警告信息的交互设计
- **机器学习模型的置信度表达**：不涉及 AI 模型输出的不确定性表达方式
- **法律合规性声明**：不涉及法律文件中的免责声明、合规声明等格式要求
- **错误处理机制**：不涉及系统运行时的异常处理、错误恢复等技术细节

---

## Prohibited（禁止行为）

严禁：
- 用 Warning 代替 Constraint
- 用 Notice 偷塞规则
- 不声明执行级别
- 使用“建议但实际上必须”的双关语
- 同一风险在不同文档中语义降级或升级

---

---

## Relationships（关系声明）

**Depends On**：

- [ADR-0000：架构测试与 CI 治理宪法](ADR-0000-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-0000

**Depended By**：

- [ADR-902：ADR 结构与章节规范](./ADR-902-adr-template-structure-contract.md) - ADR 语义定义被 ADR 结构规范依赖
- 所有文档类 ADR
- 所有治理与校验规则

---

---

## References（非裁决性参考）

- RFC 2119 / RFC 8174
- ISO/IEC/IEEE 42010

---


---

---

## History（版本历史）

| 版本 | 日期         | 变更说明 | 作者 |
|----|------------|----|----|
| 1.0 | 2025-01-28 | 初始正式版本 | Architecture Board |
