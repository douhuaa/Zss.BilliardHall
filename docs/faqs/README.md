# FAQ 文档库

> 📚 **根据 ADR-950 建立的常见问题解答库**

---

## 概述

本目录包含项目相关的常见问题解答（FAQ），帮助团队成员快速查找答案。

### 与 ADR 的区别

| 类型 | ADR | FAQ |
|------|-----|-----|
| 目的 | 定义架构约束和规则 | 解答常见疑问 |
| 裁决力 | ✅ 有裁决力 | ❌ 无裁决力 |
| 更新频率 | 很少 | 按需 |
| 内容性质 | 决策与约束（根据 ADR） | 解释与理解 |

---

## FAQ 分类

### 架构相关
- [模块化架构 FAQ](architecture-faq.md) - 模块隔离、垂直切片、模块通信等问题

### 开发实践
- [开发实践 FAQ](development-practices-faq.md) - 代码组织、测试编写、依赖注入、日志记录等问题

### 工具和流程
- 待添加：CI/CD 流程、架构测试工具、自动化脚本等

---

## 编写 FAQ 的原则

根据 ADR-950.2：

1. **何时写 FAQ**：
   - ✅ 解答常见疑问
   - ✅ 澄清容易混淆的概念
   - ✅ 提供快速参考
   - ✅ 故障排查

2. **FAQ 不应包含**：
   - ❌ 定义新的架构规则（应写入 ADR）
   - ❌ 覆盖或修改 ADR 决策
   - ❌ 包含可执行的约束（应写入 ADR）

3. **FAQ 结构**：
   - 建议包含：分类、Q&A、ADR 参考
   - 每个问题格式：
     ```markdown
     ### Q: 问题描述
     
     **A**: 答案内容
     
     **参考 ADR**：[ADR-XXXX](../adr/path/to/adr.md)
     ```

---

## 如何贡献

1. 确认问题不应该成为 ADR
2. 使用标准 FAQ 模板（见 `docs/templates/faq-template.md`）
3. 提交 PR 并更新本 README

---

## 相关文档

- [ADR-950：指南与 FAQ 文档治理宪法](../adr/governance/ADR-950-guide-faq-documentation-governance.md)
- [Guide 文档库](../guides/README.md)
- [案例库](../cases/README.md)

---

**维护**：Tech Lead  
**状态**：✅ Active
