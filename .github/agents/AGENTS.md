---
name: AGENTS
version: 1.0
risk_level: 高
maintainer: Architecture Board
---

# AGENTS.md - 可执行基线体系

> ⚠️ **本文件是 ADR-007 / ADR-007-A 指定的 Agent 执行规范。**
> 所有 Agent 输出必须遵循 ADR-007 三态输出与禁止行为规则。

---

## 1. Guardian 层

### Architecture Guardian

- **角色**：协调与监督所有 Agent，保证架构约束不被违反
- **权限**：

  - Allowed / Blocked / Uncertain 三态输出
  - 阻断明确违规的 CI / PR
  - 调用其他 Agent 获取分析证据
- **禁止**：

  - 裁决架构破例
  - 输出非审计化判断
  - 修改 ADR 内容
- **依赖**：ADR-007, ADR-007-A, ADR-900
- **执行级**：L1/L2

---

## 2. 核心执行 Agent

### module-boundary-checker

- **角色**：检查模块边界是否被违规访问
- **权限**：仅生成 Evidence，提交给 Guardian
- **输出**：Allowed / Blocked / Uncertain
- **依赖**：ADR-007_1, ADR-007_5
- **执行级**：L1

### handler-pattern-enforcer

- **角色**：校验 Handler 模式执行正确性
- **权限**：生成 Evidence，报告 Guardian
- **输出**：Allowed / Blocked / Uncertain
- **依赖**：ADR-0240, ADR-007_1
- **执行级**：L1

### architecture-test-runner

- **角色**：运行所有 ArchitectureTests
- **权限**：生成测试结果，报告 Guardian
- **输出**：Allowed / Blocked
- **依赖**：ADR-900, ADR-007_2
- **执行级**：L1

---

## 3. 证据生成 Agent

### adr-reviewer

- **角色**：审查 ADR 文档完整性与格式
- **权限**：只能生成 Evidence
- **输出**：Uncertain / Allowed（仅作为 Evidence）
- **依赖**：ADR-007_3, ADR-008
- **执行级**：L2

### dependency-scanner

- **角色**：分析依赖关系，识别潜在违规
- **权限**：只能生成 Evidence
- **输出**：Allowed / Blocked / Uncertain
- **依赖**：ADR-007_1, ADR-007_5
- **执行级**：L2

---

## 4. 冻结或暂不启用 Agent

> 以下 Agent 不参与裁决链，只作为参考或未来扩展：

- test-generator
- documentation-maintainer
- expert-dotnet-software-engineer

**理由**：防止在 Guardian 决策链中引入非审计化输出或效率偏差。

---

## 5. 输出规范

- 所有 Agent 输出必须包含：

  - 触发源（PR / CI / ManualReview / AgentDelegation）
  - 相关 ADR Clause
  - Evidence 摘要（不超过 5 行）
- Guardian 输出必须生成 FailureObject（参考 ADR-007-A）

---

## 6. 关系映射

**Depends On**：
- [ADR-007](../../docs/adr/constitutional/ADR-007-agent-behavior-permissions-constitution.md)
- [ADR-007-A](../../docs/adr/constitutional/derived/ADR-007-A-guardian-failure-feedback.md)
- [ADR-900](../../docs/adr/governance/ADR-900-architecture-tests.md)
**Depended By**：
- 所有 Copilot Instructions 与 Agent 配置文件
**Enforcement**：
- L1 / L2 执行级对应 Guardian 决策触发
