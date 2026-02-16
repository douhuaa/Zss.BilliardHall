# Agents 目录总览

本目录包含系统中所有 Agent 的详细文档说明。

## 文档说明（.agent.md）
- Markdown 格式，提供角色说明、职责、权限边界、ADR 映射和使用示例
- 供 Copilot AI 阅读，用于理解 Agent 的行为规范和约束
- 配合 `../.github/instructions/*.instructions.yaml` 文件使用

## 结构化指令（.instructions.yaml）
- 可执行的 YAML 指令文件，位于 `../.github/instructions/` 目录
- 按照 `INSTRUCTIONS-SCHEMA.md` 规范定义的结构化指令
- 用于系统执行和 CI/CD 集成

## 当前 Agent 列表

| Agent 名称 | 文档说明 | 结构化指令 | 角色定位 |
|------------|----------|-----------|---------|
| Architecture Guardian | `architecture-guardian.agent.md` | `../instructions/architecture-guardian.instructions.yaml` | 监督协调所有架构约束 |
| ADR Reviewer | `adr-reviewer.agent.md` | `../instructions/adr-reviewer.instructions.yaml` | 审查 ADR 文档质量 |
| Documentation Maintainer | `documentation-maintainer.agent.md` | `../instructions/documentation-maintainer.instructions.yaml` | 文档维护 |
| Expert .NET Software Engineer | `expert-dotnet-software-engineer.agent.md` | `../instructions/expert-dotnet-software-engineer.instructions.yaml` | .NET 技术咨询 |
| Handler Pattern Enforcer | `handler-pattern-enforcer.agent.md` | `../instructions/handler-pattern-enforcer.instructions.yaml` | Handler 模式执行 |
| Module Boundary Checker | `module-boundary-checker.agent.md` | `../instructions/module-boundary-checker.instructions.yaml` | 模块边界监督 |
| Test Generator | `test-generator.agent.md` | `../instructions/test-generator.instructions.yaml` | 测试代码生成 |
