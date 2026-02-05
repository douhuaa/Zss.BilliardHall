---
adr: ADR-947
title: "关系声明区的结构与解析安全规则"
status: Accepted
level: Governance
deciders: "Architecture Board"
date: 2026-01-26
version: "1.0"
maintainer: "架构委员会"
primary_enforcement: L1
reviewer: "@douhuaa"
supersedes: null
superseded_by: null
---

# ADR-947：关系声明区的结构与解析安全规则

> ⚖️ **本 ADR 是防止解析歧义、自指、模板污染、循环链放大的最小可执行规则集。**

**状态**：✅ Accepted

## Focus（聚焦内容）

- 关系声明区的唯一性与边界
- ADR 编号的使用约束
- 同编号多文档禁止
- 循环依赖的声明约束
- 机器解析的结构保证

---

## Glossary（术语表）

| 术语    | 定义                   | 英文对照                             |
|-------|----------------------|----------------------------------|
| 关系声明区 | ADR 中声明与其他 ADR 关系的章节 | Relationship Declaration Section |
| 解析歧义  | 机器解析工具无法准确识别语义边界     | Parsing Ambiguity                |
| 模板污染  | 模板示例被误判为实际依赖关系       | Template Pollution               |
| 自指    | ADR 声明依赖自身           | Self-Reference                   |
| 循环声明  | 双向依赖同时出现在关系声明中       | Bidirectional Assertion          |

---

## Decision（裁决）

> 🔒 **统一铁律**：  
> 所有条款必须具备稳定 RuleId，格式为：
> ```
> ADR-947_<Rule>_<Clause>
> ```
> - Rule：主要规则编号（1-5）
> - Clause：具体条款编号
> - 每条 Clause 对应一个可执行 ArchitectureTest
> - 测试方法必须一一映射到 Clause

---

### ADR-947_1：唯一顶级关系区原则（Rule）

#### ADR-947_1_1 唯一顶级关系区

- 每个 ADR **必须且仅能**包含一个 `## Relationships` 章节（中文标题为 `##` 级别的关系声明章节）
- 禁止：
  - ❌ 出现第二个同名 `##` 级别的关系声明章节
  - ❌ 模板、示例、说明中使用同级标题
- 允许：
  - ✅ 模板示例降级为 `###` 或更低
- 判定：
  - ❌ 文档中存在多个 `##` 级别的关系声明章节
  - ✅ 仅有一个顶级关系声明区

---

### ADR-947_2：关系区边界即标题边界（Rule）

#### ADR-947_2_1 边界限制

- "关系声明"区内容仅存在于从 `## Relationships` 到下一个同级 `##` 标题 或 `#`
- 禁止：
  - ❌ 使用 `###` / `####` 扩展关系区
  - ❌ 包含 ADR 编号说明性文字
- 允许：
  - ✅ 仅包含依赖、被依赖、替代、被替代、相关五类列表
- 判定：
  - ❌ 关系区内包含说明性段落或子章节
  - ✅ 仅包含列表项

---

### ADR-947_3：禁止显式循环声明（Rule）

#### ADR-947_3_1 循环检测

- 禁止在关系声明区出现 A→B 且 B→A
- 历史闭环：
  - ✅ 单向声明依赖
  - ✅ 另一侧列为相关
- 判定：
  - ❌ 同时声明 A→B 与 B→A
  - ✅ 单向声明 + 相关关系

## Enforcement（执法模型）

| 规则编号        | 执行级 | 执法方式                           | Decision 映射  |
|-------------|-----|--------------------------------|--------------|
| ADR-947_1_1 | L1  | ArchitectureTest 自动验证唯一顶级关系区   | §ADR-947_1_1 |
| ADR-947_2_1 | L1  | ArchitectureTest 验证关系区边界       | §ADR-947_2_1 |
| ADR-947_3_1 | L1  | ArchitectureTest 检测显式循环声明      | §ADR-947_3_1 |

- **L1（阻断级）**：违规直接导致 CI 失败、阻止 PR 合并
- 测试类建议：`ADR_947_Architecture_Tests.cs`
- 测试方法数量：3 个，对应 3 个 Clause

---

## Non-Goals（明确不管什么）

- ADR 内业务逻辑正确性
- 非关系声明的内容验证
- 测试框架、断言风格

---

## Prohibited（禁止行为）

- 多个顶级关系声明区
- 模板或示例污染主关系声明
- 循环声明未处理

---

## Relationships（关系声明）

**Depends On**：

- [ADR-940：ADR 关系与溯源管理治理规范](./ADR-940-adr-relationship-traceability-management.md) - 关系管理的基础规则
- [ADR-946：ADR 标题级别即语义级别约束](./ADR-946-adr-heading-level-semantic-constraint.md) - 标题语义约束
- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md) - 文档结构规范

**Depended By**：

- 无

**Supersedes**：

- 无

**Superseded By**：

- 无

**Related**：

- [ADR-006：术语与编号宪法](../constitutional/ADR-006-terminology-numbering-constitution.md) - ADR 编号规范

---

## References（非裁决性参考）

- ArchitectureTests 执行框架
- sed / grep / awk 边界提取实践
- CI 阻断策略

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                  | 修订人 |
|-----|------------|---------------------------------------|-------|
| 3.0 | 2026-02-05 | 废除 947_3（禁止 ADR 编号出现在非声明语义中）和 947_4（禁止同编号多文档）规则。原 947_5 重新编号为 947_3。这些规则过于严格，影响了文档的可读性和正常引用。保留核心规则：唯一顶级关系区、关系区边界、禁止显式循环声明。 | Copilot Agent |
| 2.0 | 2026-02-04 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系。创建 ADR_947_1/2/3/4/5_Architecture_Tests，实现所有 5 个 Rule（唯一顶级关系区、关系区边界、禁止 ADR 编号非声明语义、禁止同编号多文档、禁止显式循环声明）和 5 个 Clause 的测试用例。 | Copilot Agent |
| 1.0 | 2026-01-29 | 初始版本                                  | Architecture Board |
