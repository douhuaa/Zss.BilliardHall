---
name: "Generate ADR"
description: "生成符合 ADR-902 和 ADR-907 规范的 ADR 文档"
version: "1.0"
risk_level: "高"
category: "文档生成"
required_agent: "adr-reviewer"
dependencies:
  - "verify-adr-number-available"  # 前置：验证 ADR 编号未被使用
post_execution:
  - "update-documentation"  # 建议：更新 ADR 索引和关系图
  - "validate-adr-structure"  # 建议：验证 ADR 结构完整性
---

# Generate ADR Skill

**类别**：文档生成  
**风险等级**：高  
**版本**：1.0

---

## 功能定义

### 用途

根据 ADR-902 标准模板与结构契约生成标准化的 ADR 文档模板，并对齐 ADR-907 Rule/Clause 双层编号体系。

### 输入参数

- `adrNumber`：字符串，ADR 编号（如 "0001"）
- `title`：字符串，ADR 标题
- `level`：字符串枚举，"Constitutional" / "Governance" / "Structure" / "Runtime" / "Technical"
- `category`：字符串，类别（如 "constitutional", "governance"）
- `relatedAdrs`：字符串数组，相关 ADR 列表

### 输出结果

```json
{
  "generated": true,
  "files": [
    {
      "path": "docs/adr/constitutional/ADR-001-modular-monolith.md",
      "content": "...",
      "type": "ADR"
    }
  ],
  "summary": {
    "adrNumber": "0001",
    "level": "Constitutional",
    "sectionsIncluded": [
      "Focus",
      "Glossary",
      "Decision",
      "Enforcement",
      "Non-Goals",
      "Prohibited",
      "Relationships",
      "References",
      "History"
    ]
  }
}
```

---

## 前置条件

### 必须满足的条件

- [ ] ADR 编号未被使用
- [ ] 类别目录存在
- [ ] 标题符合命名规范
- [ ] 符合 ADR-902 结构合规性

### 必须的 Agent 授权

- **需要**：`adr-reviewer`
- **理由**：生成 ADR 直接影响架构决策记录的完整性

---

## 执行步骤

1. **验证输入参数**
   - 检查 ADR 编号唯一性
   - 验证级别有效（映射到 ADR-902 枚举）
   - 验证类别目录存在

2. **生成文件名**
   - 格式：`ADR-{Number}-{kebab-case-title}.md`
   - 确定目标路径

3. **生成文档内容**
   - 使用 ADR-902 标准模板
   - 填充 Front Matter（符合 ADR-902_1_3）
   - 添加所有必需章节（ADR-902_1_4）
   - 插入 Rule/Clause 结构占位符（ADR-907）
   - 插入占位符

4. **创建文件**
   - 写入目标路径
   - 设置文件权限

5. **记录日志**

---

## ADR 文档模板

```markdown
---
adr: ADR-{Number}
title: "{Title}"
status: Draft
level: {Level}
deciders: "Architecture Board"
date: {Date}
version: "1.0"
maintainer: "Architecture Board"
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
primary_enforcement: L2
---

# ADR-{Number}：{Title}

---

## Focus（聚焦内容）

<!-- 简述本 ADR 关注的问题域和决策范围 -->

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| <!-- 添加术语 --> | <!-- 定义 --> | <!-- English Term --> |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-{Number} 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-{Number}_<Rule>_<Clause>
> ```

---

### ADR-{Number}_1：<Rule名称>（Rule）

#### ADR-{Number}_1_1 <Clause标题>
<!-- 规则内容 -->

#### ADR-{Number}_1_2 <Clause标题>
<!-- 规则内容 -->

---

### ADR-{Number}_2：<Rule名称>（Rule）

#### ADR-{Number}_2_1 <Clause标题>
<!-- 规则内容 -->

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-{Number} 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-{Number}_1_1** | L1 | <!-- 执法方式 --> | §ADR-{Number}_1_1 |
| **ADR-{Number}_1_2** | L1 | <!-- 执法方式 --> | §ADR-{Number}_1_2 |
| **ADR-{Number}_2_1** | L2 | <!-- 执法方式 --> | §ADR-{Number}_2_1 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决

### 执行时机

- **CI 阶段**：结构违规直接阻断
- **PR Review**：L2 违规需人工裁定
- **审计阶段**：历史 ADR 结构一致性检查

---

## Non-Goals（明确不管什么）

<!-- 列出本 ADR 明确不涉及的内容 -->

---

## Prohibited（禁止行为）

<!-- 列出明确禁止的行为或模式 -->

---

## Relationships（关系声明）

**Depends On**：

- <!-- 依赖的 ADR -->

**Depended By**：

- <!-- 被依赖的 ADR -->

**Related**：

- <!-- 相关的 ADR -->

---

## References（非裁决性参考）

<!-- 非裁决性参考资料 -->

---

## History（版本历史）

| 版本 | 日期 | 变更说明 | 修订人 |
|------|------|----------|--------|
| 1.0 | {Date} | 初始版本 | Architecture Board |

---

## 验证规则

### ADR 结构检查

- [ ] 文件名符合规范
- [ ] 包含所有 ADR-902 必需章节（Focus, Glossary, Decision, Enforcement, Non-Goals, Prohibited, Relationships, References, History）
- [ ] Front Matter 符合 ADR-902_1_3 规范
- [ ] Decision 章节使用 ADR-907 Rule/Clause 双层编号体系
- [ ] 使用简体中文
- [ ] 包含代码示例（如果适用）
- [ ] 标注测试覆盖要求

### 元数据检查

- [ ] status: Draft | Accepted | Final | Superseded
- [ ] level: Constitutional | Governance | Structure | Runtime | Technical
- [ ] 日期格式正确 (YYYY-MM-DD)
- [ ] 作者信息完整

---

## 回滚机制

### 如何回滚

1. 删除生成的 ADR 文件
2. 清理空的类别目录
3. 验证索引未更新

### 回滚验证

- [ ] ADR 文件已删除
- [ ] 其他 ADR 不受影响
- [ ] 编号可重用

---

## 危险信号

🚨 **必须阻止**：
- ADR 编号重复
- 缺少必需章节
- 格式不符合规范

---

## 使用示例

### 示例 1：生成宪法级 ADR

**输入**：
```json
{
  "adrNumber": "0010",
  "title": "Event Sourcing Pattern",
  "level": "Constitutional",
  "category": "constitutional",
  "relatedAdrs": ["ADR-005", "ADR-001"]
}
```

**输出**：
- 生成 `docs/adr/constitutional/ADR-010-event-sourcing-pattern.md`
- 包含 ADR-902 标准结构和 ADR-907 Rule/Clause 体系

---

## 参考资料

- [ADR-902：ADR 标准模板与结构契约](../../../docs/adr/governance/ADR-902-adr-template-structure-contract.md)
- [ADR-907：ADR 对齐执行标准](../../../docs/ADR-907-ALIGNMENT-GUIDE.md)
- [ADR-008：文档编写与维护宪法](../../../docs/adr/constitutional/ADR-008-documentation-governance-constitution.md)
- [ADR 模板](../../../docs/templates/adr-template.md)

---

**维护者**：Architecture Board  
**最后更新**：2026-02-03  
**状态**：✅ Active
