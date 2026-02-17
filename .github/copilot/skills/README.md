# GitHub Copilot Skills 配置

本目录包含 Skills 的 YAML 配置文件，作为**单一真相源**。

## 配置文件列表

| 文件名 | Skill 名称 | 风险等级 | 类别 | 负责 Agent |
|--------|-----------|---------|------|------------|
| `generate-test.yaml` | Generate Test | 中 | 代码生成 | test-generator |
| `generate-adr.yaml` | Generate ADR | 高 | 文档生成 | adr-reviewer |
| `generate-handler.yaml` | Generate Handler | 高 | 代码生成 | architecture-guardian |
| `generate-endpoint.yaml` | Generate Endpoint | 高 | 代码生成 | architecture-guardian |
| `run-architecture-tests.yaml` | Run Architecture Tests | 低 | 测试执行 | test-generator |
| `scan-cross-module-refs.yaml` | Scan Cross-Module References | 低 | 代码分析 | module-boundary-checker |
| `update-documentation.yaml` | Update Documentation | 低 | 文档生成 | documentation-maintainer |

## 配置文件结构

每个 YAML 文件包含以下字段：

- `name`: Skill 名称
- `description`: 简短描述
- `version`: 版本号
- `risk_level`: 风险等级（高/中/低）
- `category`: 类别
- `required_agent`: 必须由哪个 Agent 授权
- `dependencies`: 前置依赖的 Skill 列表（可选）
- `post_execution`: 建议后续执行的 Skill 列表（可选）

## 迁移说明

这些配置文件从 `.github/skills/**/*.skill.md` 的 YAML Front Matter 迁移而来，现在作为唯一真相源。旧的 Markdown 文件已被删除，避免配置双源问题。

## 相关文档

- [Skills 体系说明](../../skills/README.md) - Skills 的整体架构和使用规范
- [Agents 体系](../../agents/README.md) - Agent 系统说明
- [架构治理系统](../../../docs/ARCHITECTURE-GOVERNANCE-SYSTEM.md) - 整体治理体系

---

**版本**: 1.0  
**最后更新**: 2026-02-17  
**维护团队**: 架构委员会
