---
adr: ADR-980
title: "ADR 生命周期一体化同步机制治理规范"
status: Accepted
level: Governance
deciders: "Architecture Board"
date: 2026-01-30
version: "1.1"
maintainer: "架构委员会"
primary_enforcement: L1
reviewer: "待定"
supersedes: null
superseded_by: null
---


# ADR-980：ADR 生命周期一体化同步机制治理规范

> ⚖️ **本 ADR 是 ADR 变更同步管理的治理规范，定义 ADR/测试/Prompt 三位一体的强制同步机制。**

**状态**：✅ Accepted  
## Focus（聚焦内容）

- ADR/测试/Prompt 版本号关联规则
- 同步检测自动化机制
- 变更传播清单标准化
- 责任人通知机制
- 不一致检测与阻断

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 三位一体 | ADR 正文、架构测试、Copilot Prompt 的协同体 | Trinity |
| 版本同步 | 三者版本号必须保持一致 | Version Synchronization |
| 变更传播 | ADR 变更时相关文件的级联更新 | Change Propagation |
| 同步检测 | 自动检测版本号一致性的机制 | Sync Detection |
| 责任人通知 | 基于 CODEOWNERS 的变更通知 | Owner Notification |
| 阻断构建 | 版本不一致时阻止 CI 通过 | Build Blocking |

---

---

## Decision（裁决）

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

---

## Enforcement（执法模型）


### 执行方式

待补充...


---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- 待补充

---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md) - 基于其 CI 检测机制
- [ADR-900：ADR 新增与修订流程](../governance/ADR-900-architecture-tests.md) - 基于其"三位一体交付"要求
- [ADR-940：ADR 关系与溯源管理治理规范](./ADR-940-adr-relationship-traceability-management.md) - 版本同步需要关系图更新

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 文档版本管理
- [ADR-940：ADR 关系与溯源管理治理规范](../governance/ADR-940-adr-relationship-traceability-management.md) - 关系图更新

---

---

## References（非裁决性参考）

### 相关 ADR
- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md)
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md)
- [ADR-900：ADR 新增与修订流程](../governance/ADR-900-architecture-tests.md)
- [ADR-940：ADR 关系与溯源管理治理规范](../governance/ADR-940-adr-relationship-traceability-management.md)

### 实施工具
- `scripts/validate-adr-version-sync.sh` - 版本同步检测脚本
- `.github/workflows/adr-version-sync.yml` - CI Workflow
- `.github/CODEOWNERS` - 责任人配置

### 背景材料
- [ADR-Documentation-Governance-Gap-Analysis.md](../proposals/ADR-Documentation-Governance-Gap-Analysis.md) - 原始提案

---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
