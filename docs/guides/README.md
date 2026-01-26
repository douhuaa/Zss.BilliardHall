# Guide 文档库

> ⚠️ **根据 ADR-950 建立的操作指南库**  
> **无裁决力声明**：本文档无架构裁决权，所有决策以 ADR 正文为准。

---

## 概述

本目录包含项目的操作指南，解释如何应用 ADR 中定义的架构约束。

### 与 ADR 的区别

| 类型 | ADR | Guide |
|------|-----|-------|
| 目的 | 定义约束 | 解释应用 |
| 裁决力 | ✅ 有裁决力 | ❌ 无裁决力 |
| 更新频率 | 很少 | 随 ADR 变更 |
| 内容性质 | "是什么"和"必须/禁止" | "怎么做" |

---

## Guide 分类

### 架构实践
- 待添加：如何创建新模块
- 待添加：如何实现跨模块通信

### 开发标准
- [Handler 异常与重试标准](handler-exception-retry-standard.md)
- [结构化日志与监控标准](structured-logging-monitoring-standard.md)

### 工具使用
- 待添加：如何运行架构测试
- 待添加：如何使用 ADR CLI

---

## 编写 Guide 的原则

根据 ADR-950.2：

1. **何时写 Guide**：
   - ✅ 解释如何应用 ADR
   - ✅ 提供操作步骤说明
   - ✅ 工具使用教程
   - ✅ 多步骤流程

2. **Guide 禁止**：
   - ❌ 定义新的架构规则（应写入 ADR）
   - ❌ 覆盖 ADR 决策
   - ❌ 包含与 ADR 冲突的建议

3. **Guide 结构**：
   - 必须包含：目的、前置条件、步骤、参考 ADR
   - 步骤应清晰、可操作
   - 包含代码示例和常见错误

---

## 如何贡献

1. 确认内容不应该成为 ADR
2. 使用标准 Guide 模板（见 `docs/templates/guide-template.md`）
3. 确保与相关 ADR 一致
4. 提交 PR 并更新本 README

---

## 相关文档

- [ADR-950：指南与 FAQ 文档治理宪法](../adr/governance/ADR-950-guide-faq-documentation-governance.md)
- [FAQ 文档库](../faqs/README.md)
- [案例库](../cases/README.md)
- [ADR 目录](../adr/README.md)

---

**维护**：Tech Lead  
**状态**：✅ Active
