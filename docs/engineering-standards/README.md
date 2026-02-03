# 工程标准库

> ⚠️ **根据 ADR-950 建立的工程标准库**  
> **无裁决力声明**：本文档无架构裁决权，所有决策以 ADR 正文为准。

---

## 概述

本目录包含项目的工程标准，细化 ADR 中定义的架构约束的实施细节。

### 与 ADR 和 Guide 的区别

| 类型 | ADR | Engineering Standard | Guide |
|------|-----|---------------------|-------|
| 目的 | 定义约束 | 细化实施细节 | 解释应用 |
| 裁决力 | ✅ 最高裁决力 | ⚠️ 有限裁决力（基于 ADR） | ❌ 无裁决力 |
| 更新频率 | 很少 | 按需 | 随 ADR 变更 |
| 内容性质 | 决策与约束 | 工具配置、代码风格 | 操作步骤 |

**核心原则**：根据 ADR-950 和 ADR-952，工程标准应基于 ADR，不能引入 ADR 未授权的约束。

---

## 工程标准分类

### 文档标准
- [文档维护标准](documentation-maintenance-standard.md) - 文档维护流程和质量标准
- [ADR-008 执行标准](adr-008-enforcement-standards.md) - 文档治理执行细节

### 代码标准
- 待添加：代码格式标准
- 待添加：命名约定标准

### 配置标准
- 待添加：EditorConfig 标准
- 待添加：CI/CD 配置标准

---

## 编写工程标准的原则

根据 ADR-950.1 和 ADR-952：

1. **何时写工程标准**：
   - ⚠️ 细化 ADR 的实施细节
   - ⚠️ 工具配置标准
   - ⚠️ 代码风格约定
   - ⚠️ 根据 ADR-950，应基于 ADR，不能引入新约束

2. **工程标准不应包含**：
   - ❌ 引入 ADR 未定义的架构约束
   - ❌ 覆盖或削弱 ADR 规则
   - ❌ 独立于 ADR 的新规则

3. **工程标准结构**：
   - 建议包含：相关 ADR、适用范围、实施细节、示例
   - 明确标注所基于的 ADR
   - 详细的配置说明和示例

---

## 如何贡献

1. 确认内容应该是工程标准而非 ADR 或 Guide
2. 确保标准基于现有 ADR，不引入新约束
3. 使用标准工程标准模板（见 `docs/templates/standard-template.md`）
4. 提交 PR 并更新本 README

---

## 相关文档

- [ADR-950：指南与 FAQ 文档治理规范](../adr/governance/ADR-950-guide-faq-documentation-governance.md)
- [ADR-952：工程标准与 ADR 分离边界](../adr/governance/ADR-952-engineering-standard-adr-boundary.md)
- [Guide 文档库](../guides/README.md)
- [ADR 目录](../adr/README.md)

---

**维护**：Tech Lead  
**状态**：✅ Active
