# Agent-Skills 权限映射矩阵

**版本**: 1.0  
**最后更新**: 2026-02-09  
**状态**: Active

---

## 概述

本文档定义了各个 Agent 与 Skills 之间的授权关系，确保权限边界清晰、可审计。

### 设计原则

- **最小权限原则**：每个 Agent 仅被授权执行与其职责相关的 Skills
- **明确授权**：所有授权必须显式声明，不存在隐式权限
- **可追溯性**：每个授权都有明确的理由和 ADR 依据
- **定期审查**：权限矩阵每季度审查一次

---

## Agent 与 Skills 映射表

### 1. Architecture Guardian（架构守护）

**职责**：监督协调所有架构约束，三态判定 Agent 输出

**授权的 Skills**：

| Skill | 类别 | 风险等级 | 授权理由 | ADR 依据 |
|-------|------|---------|---------|----------|
| `generate-handler` | 代码生成 | 高 | 生成 Handler 直接影响架构合规性 | ADR-007, ADR-005 |
| `generate-endpoint` | 代码生成 | 高 | 生成 Endpoint 涉及模块边界 | ADR-007, ADR-001 |
| `post-comment` | CI/CD | 低 | 需要反馈架构决策到 PR | ADR-007 |

**禁止的操作**：
- ❌ 直接修改 ADR 文档
- ❌ 绕过测试直接批准
- ❌ 修改 Agent 配置

---

### 2. ADR Reviewer（ADR 审查员）

**职责**：审查 ADR 文档质量、关系与变更

**授权的 Skills**：

| Skill | 类别 | 风险等级 | 授权理由 | ADR 依据 |
|-------|------|---------|---------|----------|
| `generate-adr` | 文档生成 | 高 | 负责 ADR 文档的创建和规范性 | ADR-902, ADR-907-A |
| `post-comment` | CI/CD | 低 | 需要反馈 ADR 审查结果 | ADR-940 |

**禁止的操作**：
- ❌ 生成或修改代码
- ❌ 执行架构测试
- ❌ 修改模块结构

---

### 3. Documentation Maintainer（文档维护员）

**职责**：文档维护、链接检查、索引更新

**授权的 Skills**：

| Skill | 类别 | 风险等级 | 授权理由 | ADR 依据 |
|-------|------|---------|---------|----------|
| `update-documentation` | 文档生成 | 低 | 负责文档索引和链接维护 | ADR-008, ADR-910 |

**禁止的操作**：
- ❌ 修改 ADR 正文（只能更新索引和链接）
- ❌ 生成代码
- ❌ 执行测试

---

### 4. Test Generator（测试生成器）

**职责**：根据 ADR 与约束生成 ArchitectureTests

**授权的 Skills**：

| Skill | 类别 | 风险等级 | 授权理由 | ADR 依据 |
|-------|------|---------|---------|----------|
| `generate-test` | 代码生成 | 中 | 生成测试代码以验证架构约束 | ADR-900, ADR-907 |
| `run-architecture-tests` | 测试执行 | 低 | 需要验证生成的测试是否正确 | ADR-900 |
| `run-unit-tests` | 测试执行 | 低 | 验证单元测试覆盖率 | ADR-907 |

**禁止的操作**：
- ❌ 修改业务代码
- ❌ 生成 Handler 或 Endpoint
- ❌ 修改 ADR

---

### 5. Module Boundary Checker（模块边界检查器）

**职责**：检查模块边界规则、依赖约束

**授权的 Skills**：

| Skill | 类别 | 风险等级 | 授权理由 | ADR 依据 |
|-------|------|---------|---------|----------|
| `scan-cross-module-refs` | 代码分析 | 中 | 扫描和检测跨模块引用违规 | ADR-001, ADR-003, ADR-005 |

**禁止的操作**：
- ❌ 自动修复违规（只能报告）
- ❌ 生成代码
- ❌ 修改模块结构

---

### 6. Expert Dotnet Engineer（.NET 专家工程师）

**职责**：提供 .NET 技术建议、代码规范检查

**授权的 Skills**：

| Skill | 类别 | 风险等级 | 授权理由 | ADR 依据 |
|-------|------|---------|---------|----------|
| *(暂无)* | - | - | 当前主要提供咨询，未绑定具体 Skill | ADR-001 ~ 005 |

**说明**：该 Agent 主要作为技术顾问角色，提供 .NET 最佳实践建议，暂未授权自动化操作 Skills。

**潜在 Skills**（待开发）：
- `analyze-dotnet-conventions`（分析 .NET 代码规范）
- `suggest-refactoring`（建议重构方向）

---

### 7. Handler Pattern Enforcer（Handler 模式强制器）

**职责**：强制 Handler 模式执行、约束验证

**授权的 Skills**：

| Skill | 类别 | 风险等级 | 授权理由 | ADR 依据 |
|-------|------|---------|---------|----------|
| *(暂无)* | - | - | 当前主要进行模式验证，未绑定具体 Skill | ADR-201, ADR-240 |

**说明**：该 Agent 主要验证 Handler 模式的正确性，暂未授权自动化生成或修改 Skills。

**潜在 Skills**（待开发）：
- `validate-handler-structure`（验证 Handler 结构）
- `enforce-cqrs-pattern`（强制 CQRS 模式）

---

## Skills 使用频率统计

### 高频 Skills（每日使用）

| Skill | 调用频率 | 主要使用 Agent |
|-------|---------|---------------|
| `run-architecture-tests` | 高 | Test Generator, Architecture Guardian |
| `post-comment` | 高 | Architecture Guardian, ADR Reviewer |

### 中频 Skills（每周使用）

| Skill | 调用频率 | 主要使用 Agent |
|-------|---------|---------------|
| `generate-test` | 中 | Test Generator |
| `scan-cross-module-refs` | 中 | Module Boundary Checker |
| `update-documentation` | 中 | Documentation Maintainer |

### 低频 Skills（按需使用）

| Skill | 调用频率 | 主要使用 Agent |
|-------|---------|---------------|
| `generate-handler` | 低 | Architecture Guardian |
| `generate-endpoint` | 低 | Architecture Guardian |
| `generate-adr` | 低 | ADR Reviewer |
| `run-unit-tests` | 低 | Test Generator |

---

## 权限审计日志

### 审计要求

所有 Skill 调用必须记录以下信息：
```json
{
  "timestamp": "2026-02-09T16:00:00Z",
  "agent": "architecture-guardian",
  "skill": "generate-handler",
  "authorized": true,
  "executed_by": "copilot-agent",
  "pr_number": 362,
  "result": "success"
}
```

### 异常权限使用

如发现未授权的 Skill 调用，应：
1. 立即阻止执行
2. 生成安全告警
3. 记录审计日志
4. 通知架构委员会

---

## 权限变更流程

### 新增授权

1. 提交 Issue 说明授权理由
2. 关联相关 ADR
3. 架构委员会审批
4. 更新本文档
5. 更新 Agent 配置

### 撤销授权

1. 说明撤销原因
2. 评估影响范围
3. 更新本文档
4. 更新 Agent 配置
5. 通知相关团队

---

## 权限边界说明

### Agent 不能做的事

所有 Agent 共同禁止的操作：
- ❌ 修改 `.github/copilot-instructions.md`（主控制器）
- ❌ 修改其他 Agent 的 instructions 文件
- ❌ 绕过 ADR 约束
- ❌ 删除架构测试
- ❌ 直接访问生产环境
- ❌ 泄露敏感信息

### Skills 不能做的事

所有 Skills 共同禁止的操作：
- ❌ 自主决策（必须由 Agent 授权）
- ❌ 修改 ADR 正文
- ❌ 修改 Agent 配置
- ❌ 绕过前置条件检查
- ❌ 禁用审计日志

---

## 反向查询

### 按 Skill 查询授权 Agent

| Skill | 授权的 Agents | 备注 |
|-------|--------------|------|
| `generate-handler` | Architecture Guardian | 唯一授权 |
| `generate-endpoint` | Architecture Guardian | 唯一授权 |
| `generate-test` | Test Generator | 唯一授权 |
| `generate-adr` | ADR Reviewer | 唯一授权 |
| `update-documentation` | Documentation Maintainer | 唯一授权 |
| `scan-cross-module-refs` | Module Boundary Checker | 唯一授权 |
| `run-architecture-tests` | Test Generator, Architecture Guardian | 多 Agent 共享（只读操作） |
| `run-unit-tests` | Test Generator | 唯一授权 |
| `post-comment` | Architecture Guardian, ADR Reviewer | 多 Agent 共享（低风险操作） |

---

## 待开发的 Skills

### 高优先级

1. **`validate-handler-structure`** - 验证 Handler 结构
   - 授权给：Handler Pattern Enforcer
   - 风险等级：低
   - ADR 依据：ADR-201, ADR-240

2. **`analyze-dotnet-conventions`** - 分析 .NET 代码规范
   - 授权给：Expert Dotnet Engineer
   - 风险等级：低
   - ADR 依据：ADR-001 ~ 005

### 中优先级

3. **`enforce-cqrs-pattern`** - 强制 CQRS 模式
   - 授权给：Handler Pattern Enforcer
   - 风险等级：中
   - ADR 依据：ADR-005

4. **`suggest-refactoring`** - 建议重构方向
   - 授权给：Expert Dotnet Engineer
   - 风险等级：低
   - ADR 依据：ADR-001 ~ 005

---

## 版本历史

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|---------|------|
| 1.0 | 2026-02-09 | 初始版本：建立 Agent-Skills 权限映射体系 | Copilot Agent |

---

## 相关文档

- [Skills 体系总览](./skills/README.md)
- [Agents 配置](./agents/)
- [Instructions 规范](./instructions/)
- [ADR-007：Agent 行为与权限宪法](../docs/adr/constitutional/ADR-007-agent-behavior-permissions-constitution.md)
- [架构治理系统](../docs/ARCHITECTURE-GOVERNANCE-SYSTEM.md)

---

**维护责任**：架构委员会  
**审核周期**：每季度  
**状态**：✅ Active
