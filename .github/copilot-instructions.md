# Copilot Instructions

> 本文档统一调度所有 Specialist 与 Guardian instructions，并定义触发规则、委托原则与反馈闭环。

---

## 委托原则

* **所有任务**必须通过 `run_subagent` 调用对应 Agent 执行。
* Guardian **仅协调与汇总**，禁止直接做最终裁决。
* 未明确职责的操作必须标记为 `Uncertain` 并引导人工确认。

---

## 可用 Agent（Specialist & Guardian）

| Agent 名称                               | 说明                             | instructions 文件                                                                                                         |
|----------------------------------------|--------------------------------|-------------------------------------------------------------------------------------------------------------------------|
| 架构守护（Guardian）                         | 监督协调所有架构约束，三态判定 Agent 输出       | [`architecture-guardian.instructions.yaml`](./instructions/architecture-guardian.instructions.yaml)                     |
| ADR 审查（ADR Reviewer）                   | 审查 ADR 文档质量、关系与变更              | [`adr-reviewer.instructions.yaml`](./instructions/adr-reviewer.instructions.yaml)                                       |
| 文档维护（Documentation Maintainer）         | 文档维护、链接检查、索引更新                 | [`documentation-maintainer.instructions.yaml`](./instructions/documentation-maintainer.instructions.yaml)               |
| .NET 专家工程（Expert Dotnet Engineer）      | 提供 .NET 技术建议、代码规范检查            | [`expert-dotnet-software-engineer.instructions.yaml`](./instructions/expert-dotnet-software-engineer.instructions.yaml) |
| Handler 模式强制（Handler Pattern Enforcer） | 强制 Handler 模式执行、约束验证           | [`handler-pattern-enforcer.instructions.yaml`](./instructions/handler-pattern-enforcer.instructions.yaml)               |
| 模块边界检查（Module Boundary Checker）        | 检查模块边界规则、依赖约束                  | [`module-boundary-checker.instructions.yaml`](./instructions/module-boundary-checker.instructions.yaml)                 |
| 测试生成（Test Generator）                   | 根据 ADR 与约束生成 ArchitectureTests | [`test-generator.instructions.yaml`](./instructions/test-generator.instructions.yaml)                                   |

---

## 任务触发规则（Trigger Rules）

```yaml
pull_request:
  run_subagents:
    - adr-reviewer
    - architecture-guardian
    - test-generator

push:
  paths:
    - "src/**":
        run_subagents:
          - module-boundary-checker
          - handler-pattern-enforcer
          - expert-dotnet-software-engineer
    - "docs/adr/**":
        run_subagents:
          - documentation-maintainer
          - adr-reviewer

ci_pipeline:
  run_subagents:
    - architecture-guardian
    - all_L1_architecture_tests
```

> 每次任务执行都会生成 `FailureObject`，保证可追踪、可审计。

---

## 反馈与报告（Feedback & Reporting）

```yaml
feedback_loop:
  source: guardian
  failure_object:
    required: true
  actions:
    - update_adr
    - enhance_architecture_tests
    - adjust_agent_config
```

* Guardian 汇总所有 Agent 输出，并执行三态判定（✅ Allowed / ⚠️ Blocked / ❓ Uncertain）。
* 任何失败或异常必须产生治理产物，不可仅修改 Prompt。
