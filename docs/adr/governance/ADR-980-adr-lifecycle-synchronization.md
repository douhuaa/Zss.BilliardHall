---
adr: ADR-980
title: "ADR 生命周期一体化同步机制宪法"
status: Accepted
level: Governance
deciders: "Architecture Board"
date: 2026-01-26
version: "1.0"
maintainer: "架构委员会"
reviewer: "待定"
supersedes: null
superseded_by: null
---

# ADR-980：ADR 生命周期一体化同步机制宪法

> ⚖️ **本 ADR 是 ADR 变更同步管理的唯一裁决源，定义 ADR/测试/Prompt 三位一体的强制同步机制。**

**状态**：✅ Accepted  
**级别**：架构元规则 / 治理层  
**适用范围**：所有 ADR 及其关联的测试和 Prompt 文档  
**生效时间**：即刻

---

## 聚焦内容（Focus）

- ADR/测试/Prompt 版本号关联规则
- 同步检测自动化机制
- 变更传播清单标准化
- 责任人通知机制
- 不一致检测与阻断

---

## 术语表（Glossary）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 三位一体 | ADR 正文、架构测试、Copilot Prompt 的协同体 | Trinity |
| 版本同步 | 三者版本号必须保持一致 | Version Synchronization |
| 变更传播 | ADR 变更时相关文件的级联更新 | Change Propagation |
| 同步检测 | 自动检测版本号一致性的机制 | Sync Detection |
| 责任人通知 | 基于 CODEOWNERS 的变更通知 | Owner Notification |
| 阻断构建 | 版本不一致时阻止 CI 通过 | Build Blocking |

---

## 决策（Decision）

### 版本号关联规则（ADR-980.1）

**规则**：

ADR 正文、架构测试、Copilot Prompt **必须**使用一致的版本号。

**版本号格式**：
- ADR 正文：`**版本**：X.Y`（在元数据区）
- 架构测试：`// Version: X.Y`（在类注释中）
- Copilot Prompt：`**版本**：X.Y`（在元数据区）

**示例**：

ADR 正文 (`ADR-0001-modular-monolith-vertical-slice-architecture.md`)：
```markdown
# ADR-0001：模块化单体与垂直切片架构

**状态**：✅ Accepted
**版本**：3.2
```

架构测试 (`ADR_0001_Architecture_Tests.cs`)：
```csharp
// Version: 3.2
// ADR: ADR-0001
public class ADR_0001_Architecture_Tests
{
    // ...
}
```

Copilot Prompt (`adr-0001.prompts.md`)：
```markdown
# ADR-0001 Copilot Prompts

**版本**：3.2
**对应 ADR**：ADR-0001-modular-monolith-vertical-slice-architecture
```

**核心原则**：
> 三位一体同版本，变更即同步。

**判定**：
- ❌ ADR 版本 3.2，测试版本 3.1
- ❌ ADR 版本 3.2，Prompt 版本缺失
- ✅ ADR/测试/Prompt 均为 3.2

---

### 同步检测自动化（ADR-980.2）

**规则**：

CI **必须**包含版本同步检测步骤，不一致时 **必须**阻断构建。

**检测范围**：
1. 对比 ADR 正文版本号
2. 对比对应架构测试版本号（如存在）
3. 对比对应 Copilot Prompt 版本号（如存在）

**检测时机**：
- 每次 PR 提交
- 每次推送到主分支
- 每日定时检查

**检测工具**：
- 脚本：`scripts/validate-adr-version-sync.sh`
- CI Workflow：`.github/workflows/adr-version-sync.yml`

**失败处理**：
```
❌ 版本同步检测失败

ADR-0001 版本不一致：
- ADR 正文：3.2
- 架构测试：3.1
- Copilot Prompt：3.2

请同步所有文件的版本号至 3.2
参考：ADR-980 变更传播清单
```

**核心原则**：
> 不一致不允许合并。

**判定**：
- ❌ 版本不一致但 CI 通过
- ❌ 手动检测版本同步
- ✅ 自动检测并阻断不一致

---

### 变更传播清单（ADR-980.3）

**规则**：

修改 ADR 时 **必须**遵循以下变更传播清单：

**标准清单**：
```markdown
## ADR-XXXX 变更清单

修改 ADR-XXXX 时必须完成：
- [ ] ADR 正文版本号 +0.1（小改）或 +1.0（大改）
- [ ] 更新 ADR 正文的"版本历史"章节
- [ ] 同步架构测试版本号（如存在）
- [ ] 同步 Copilot Prompt 版本号（如存在）
- [ ] 更新 ADR 关系图（如关系变更）
- [ ] 通知相关责任人（通过 PR 评论）
- [ ] 运行 `scripts/validate-adr-version-sync.sh`
- [ ] 确认 CI 版本检测通过

影响范围：
- 依赖本 ADR 的其他 ADR：[列出]
- 相关测试：[列出]
- 相关 Prompt：[列出]
```

**版本号变更规则**：
- **+0.1**（小版本）：澄清、示例、非规则性变更
- **+1.0**（大版本）：新增/修改/删除规则、影响测试

**核心原则**：
> 变更必须清单化，不留遗漏。

**判定**：
- ❌ 修改 ADR 但未更新版本号
- ❌ 修改 ADR 但未同步测试/Prompt
- ✅ 完成完整的变更传播清单

---

### 责任人通知机制（ADR-980.4）

**规则**：

利用 `CODEOWNERS` 机制 **必须**自动通知相关责任人。

**CODEOWNERS 配置示例**：
```
# ADR 正文
/docs/adr/constitutional/ @architecture-committee
/docs/adr/governance/ @architecture-committee @tech-lead

# 架构测试
/src/tests/ArchitectureTests/ADR/ @architecture-committee @qa-lead

# Copilot Prompts
/docs/copilot/ @tech-lead @copilot-team
```

**通知触发**：
- PR 创建时自动添加责任人为 Reviewer
- PR 合并后自动通知订阅者
- ADR 状态变更时通知相关方

**通知内容**：
```
📢 ADR-XXXX 已变更

版本：3.1 → 3.2
变更类型：规则修改
影响范围：[列出]

请审查相关文件是否需要同步更新：
- [ ] 架构测试
- [ ] Copilot Prompt
- [ ] 依赖的其他 ADR
```

**核心原则**：
> 自动通知，不遗漏责任人。

**判定**：
- ❌ ADR 变更但责任人不知情
- ❌ 手动通知责任人
- ✅ 基于 CODEOWNERS 自动通知

---

### 不一致的补救流程（ADR-980.5）

**规则**：

发现版本不一致时 **必须**遵循以下补救流程：

**发现途径**：
1. CI 自动检测失败
2. 人工审查发现
3. 定期审计发现

**补救步骤**：
```markdown
1. 创建 Issue：标题 `[VERSION-SYNC] ADR-XXXX 版本不一致`
2. 标签：`governance-inconsistency`, `urgent`
3. 分配：文件 CODEOWNERS 责任人
4. 修复：
   - 确定正确版本号（以 ADR 正文为准）
   - 同步所有关联文件
   - 更新变更日志
5. 验证：运行 `scripts/validate-adr-version-sync.sh`
6. PR：提交修复 PR，标题 `fix: 同步 ADR-XXXX 版本号至 X.Y`
7. 审查：必须经过至少 1 名架构委员会成员审查
8. 合并：CI 通过后合并
9. 关闭：关闭原 Issue
```

**优先级**：
- 🔴 **P0**：核心 ADR（0000-0008）版本不一致
- 🟡 **P1**：其他 ADR 版本不一致

**核心原则**：
> 发现即修复，不延期。

**判定**：
- ❌ 发现不一致但不处理
- ❌ 仅修复部分文件
- ✅ 完整补救流程

---

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 基于其 CI 检测机制
- [ADR-900：ADR 新增与修订流程](../governance/ADR-900-adr-process.md) - 基于其"三位一体交付"要求
- [ADR-940：ADR 关系与溯源管理宪法](./ADR-940-adr-relationship-traceability-management.md) - 版本同步需要关系图更新

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 文档版本管理
- [ADR-940：ADR 关系与溯源管理宪法](../governance/ADR-940-adr-relationship-traceability-management.md) - 关系图更新

---

## 执法模型（Enforcement）

| 规则编号 | 执行级别 | 测试/手段 | 说明 |
|---------|---------|----------|------|
| ADR-980.1 | L1 | `validate-adr-version-sync.sh` | CI 自动检测版本一致性 |
| ADR-980.2 | L1 | `.github/workflows/adr-version-sync.yml` | CI Workflow 阻断 |
| ADR-980.3 | L2 | PR Template + Code Review | 人工审查清单完成度 |
| ADR-980.4 | L1 | `CODEOWNERS` | GitHub 自动通知 |
| ADR-980.5 | L2 | Issue Template + Code Review | 补救流程标准化 |

---

## 破例与归还（Exception）

### 允许破例的前提

破例 **仅在以下情况允许**：
- 新创建的 ADR 尚未有对应测试/Prompt（7 天宽限期）
- 迁移期间批量更新（需架构委员会批准）
- 测试文件或 Prompt 文件确实不适用（需文档化原因）

### 破例要求

每个破例 **必须**：
- 记录在 PR 描述中，说明原因
- 指明预期完成同步的日期
- 标记 `sync-pending` 标签
- 创建 Follow-up Issue

**未记录的破例 = 未授权架构违规。**

---

## 变更政策（Change Policy）

### 变更规则

本 ADR 属于 **治理层元规则**：
- 修改需架构委员会 100% 同意
- 需全量回归所有 ADR 的版本同步状态
- 需更新所有相关脚本和 CI Workflow

### 失效与替代

- 本 ADR 一旦被替代，**必须**更新 ADR-900
- 不允许"隐性废弃"

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：
- 文档内容质量（由 ADR-0008 负责）
- 文档结构标准（由 ADR-0008 负责）
- 代码版本号管理（仅管理文档版本）
- Git 提交消息规范
- 发布版本号策略

---

## 非裁决性参考（References）

### 相关 ADR
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md)
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md)
- [ADR-900：ADR 新增与修订流程](../governance/ADR-900-adr-process.md)
- [ADR-940：ADR 关系与溯源管理宪法](../governance/ADR-940-adr-relationship-traceability-management.md)

### 实施工具
- `scripts/validate-adr-version-sync.sh` - 版本同步检测脚本
- `.github/workflows/adr-version-sync.yml` - CI Workflow
- `.github/CODEOWNERS` - 责任人配置

### 背景材料
- [ADR-Documentation-Governance-Gap-Analysis.md](../proposals/ADR-Documentation-Governance-Gap-Analysis.md) - 原始提案

---

## 版本历史（Version History）

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|----------|------|
| 1.0 | 2026-01-26 | 初版：定义 ADR/测试/Prompt 三位一体同步机制 | GitHub Copilot |

