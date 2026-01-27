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
| 内容性质 | 决策与约束（根据 ADR） | 操作步骤 |

---

## Guide 分类

### 快速入门
- [快速开始指南](quick-start-guide.md) - 项目快速上手指南
- [AI 治理指南](ai-governance-guide.md) - AI 治理系统快速入门

### 架构实践
- [架构设计指南](architecture-design-guide.md) - 模块化单体架构设计指南
- [架构验证指南](architecture-verification-guide.md) - 三层架构验证体系说明
- [跨模块通信](cross-module-communication.md) - 如何实现跨模块通信

### 测试与质量
- [测试框架指南](testing-framework-guide.md) - 测试策略与实践指南
- [测试架构指南](test-architecture-guide.md) - 三层测试架构详解
- [ADR 测试一致性指南](adr-test-consistency-guide.md) - ADR 与测试映射规范
- [ADR 测试映射指南](adr-test-mapping-guide.md) - ADR-测试-Prompt 三向映射规范

### 开发标准
- [Handler 异常与重试标准](handler-exception-retry-standard.md) - Handler 异常处理规范
- [结构化日志与监控标准](structured-logging-monitoring-standard.md) - 日志与监控标准

### 工具与自动化
- [ADR 自动化工具指南](adr-automation-tools-guide.md) - ADR 自动化工具使用指南
- [CI/CD 集成指南](ci-cd-integration-guide.md) - CI/CD 流程与配置指南

---

## 编写 Guide 的原则

根据 ADR-950.2：

1. **何时写 Guide**：
   - ✅ 解释如何应用 ADR
   - ✅ 提供操作步骤说明
   - ✅ 工具使用教程
   - ✅ 多步骤流程

2. **Guide 不应包含**：
   - ❌ 定义新的架构规则（应写入 ADR）
   - ❌ 覆盖 ADR 决策
   - ❌ 包含与 ADR 冲突的建议

3. **Guide 结构**：
   - 建议包含：目的、前置条件、步骤、参考 ADR
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
