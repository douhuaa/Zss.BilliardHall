# Agent Schema 使用说明

## 概述

本目录下的所有 `*.agent` 文件遵循统一的 YAML schema，作为可执行配置使用。

## Schema 版本：1

所有 agent 文件必须包含以下必填字段：

### 1. 元数据字段

```yaml
schema_version: 1           # Schema 版本号
name: agent-name           # Agent 名称（kebab-case）
version: "1.0"             # Agent 版本
type: guardian|specialist|evidence  # Agent 类型
status: active|frozen|disabled      # Agent 状态
risk_level: low|medium|high        # 风险级别
enforcement_level: L1|L2           # 执行级别
```

### 2. 触发器（GitHub Actions 风格 snake_case）

```yaml
triggers:
  - pull_request       # PR 创建或更新
  - push              # 代码推送
  - workflow_dispatch # 手动触发
  - schedule          # 定时任务
  - merge_group       # 合并队列
  - manual_review     # 人工审查
  - agent_delegation  # Agent 委托
```

### 3. 上报与委托

```yaml
reports_to: architecture-guardian  # Guardian 为 null
delegation_to:                     # Evidence Agent 为 null
  - agent-name-1
  - agent-name-2
```

### 4. 治理配置

```yaml
governance:
  adr_dependencies:
    - ADR-007    # Agent 行为与权限宪法
    - ADR-XXX    # 其他相关 ADR
  output_states:
    - Allowed
    - Blocked
    - Uncertain
  default_policy: deny_on_uncertain
  evidence_required: true
```

### 5. 能力配置

```yaml
capabilities:
  tools:
    - tool-1
    - tool-2
    - tool-n
```

### 6. 职责定义

```yaml
responsibilities:
  - id: R1
    text: "职责描述 1"
  - id: R2
    text: "职责描述 2"
```

### 7. 禁止行为

```yaml
prohibitions:
  - id: P1
    text: "禁止行为描述 1"
  - id: P2
    text: "禁止行为描述 2"
```

### 8. 证据契约（必填）

```yaml
evidence_contract:
  must_include:
    - adr_clause_refs   # ADR 条款引用（格式：ADR-XXX_Y_Z）
    - rule_ids          # 违反的规则 ID
    - file_paths        # 相关文件路径
    - decision_summary  # 决策摘要
```

## Registry 使用

`registry.yml` 是唯一的权威索引，包含：

- `registry_version`: Registry 版本号
- `defaults`: 全局默认策略
- `agents`: Agent 列表（name, file, group, status）
- `groups`: 分组说明
- `supported_triggers`: 支持的触发器列表
- `adr_dependencies`: 全局 ADR 依赖

## Agent 分组

- **guardian**: Guardian 层（协调与监督）
- **core**: 核心执行 Agent（L1 级别）
- **evidence**: 证据生成 Agent（L2 级别）
- **frozen**: 冻结或暂不启用 Agent

## 示例

参见 `architecture-guardian.agent`, `adr-reviewer.agent` 等文件。

## 验证

使用以下命令验证 YAML 格式：

```bash
python3 -c "import yaml; yaml.safe_load(open('your-file.agent'))"
```

## 破坏性变更说明

此次重构**完全移除**了以下文件：
- 所有 `*.agent.md` 文件
- `README.md`
- `AGENTS.md`
- `.agent` 聚合文件

如需查看 Agent 配置，请直接阅读对应的 `.agent` YAML 文件。
