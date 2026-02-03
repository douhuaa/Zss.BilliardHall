# Agents 目录总览

本目录包含系统中所有 Agent 的可执行配置与文档说明。

## 可执行配置（.agent）
- YAML 内容保留，用于系统执行
- 文件后缀统一为 `.agent`

## 文档说明（.agent.md）
- Markdown 格式，提供角色说明、职责和示例
- 仅作参考，不参与执行

## 当前 Agent 列表

| Agent 名称 | 配置文件 | 文档说明 | 角色定位 |
|------------|----------|----------|---------|
| Architecture Guardian | `ARCHITECTURE-GUARDIAN.agent` | `ARCHITECTURE-GUARDIAN.agent.md` | 监督协调所有架构约束 |
| ADR Reviewer | `ADR-REVIEWER.agent` | `ADR-REVIEWER.agent.md` | 审查 ADR 文档质量 |
| Documentation Maintainer | `DOCUMENTATION-MAINTAINER.agent` | `DOCUMENTATION-MAINTAINER.agent.md` | 文档维护 |
| Expert .NET Software Engineer | `EXPERT-DOTNET-SOFTWARE-ENGINEER.agent` | `EXPERT-DOTNET-SOFTWARE-ENGINEER.agent.md` | .NET 技术咨询 |
| Handler Pattern Enforcer | `HANDLER-PATTERN-ENFORCER.agent` | `HANDLER-PATTERN-ENFORCER.agent.md` | Handler 模式执行 |
| Module Boundary Checker | `MODULE-BOUNDARY-CHECKER.agent` | `MODULE-BOUNDARY-CHECKER.agent.md` | 模块边界监督 |
| Test Generator | `TEST-GENERATOR.agent` | `TEST-GENERATOR.agent.md` | 测试代码生成 |
