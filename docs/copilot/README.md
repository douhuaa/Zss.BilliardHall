# Copilot 提示词库

> ⚠️ **无裁决力声明**：本文档仅供参考，不具备架构裁决权。
> 所有架构决策以相关 ADR 正文为准。

## 目录用途

为 GitHub Copilot 提供场景化提示词，辅助开发者理解和遵循架构规范。

## Copilot 角色定位

- 📖 架构守护者 - 提醒架构约束
- 🎓 规范解释器 - 解释 ADR 规则
- 🛡️ 违规检测器 - 早期捕获违例
- ❌ 非裁决者 - 不替代 ADR 和测试

## Prompts 列表

### ADR 特定提示词
- [adr-0001.prompts.md](adr-0001.prompts.md) - 模块隔离与垂直切片
- [adr-0002.prompts.md](adr-0002.prompts.md) - 三层启动架构
- [adr-0003.prompts.md](adr-0003.prompts.md) - 命名空间规则
- [adr-0004.prompts.md](adr-0004.prompts.md) - 中央包管理
- [adr-0005.prompts.md](adr-0005.prompts.md) - CQRS 与 Handler
- [adr-0008.prompts.md](adr-0008.prompts.md) - 文档治理

### 通用指南
- [architecture-test-failures.md](architecture-test-failures.md) - 架构测试失败诊断
- [pr-common-issues.prompts.md](pr-common-issues.prompts.md) - PR 常见问题

## 相关链接

- [上级目录：docs](../README.md)
- [ADR 目录](../adr/README.md)
- [Instructions 体系](../../.github/instructions/)
- [Agents 体系](../../.github/agents/)
