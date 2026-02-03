---
adr: ADR-980
title: "ADR 生命周期一体化同步机制治理规范"
status: Accepted
level: Governance
deciders: "Architecture Board"
date: 2026-02-04
version: "2.0"
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

| 术语    | 定义                              | 英文对照                    |
|-------|---------------------------------|-------------------------|
| 三位一体  | ADR 正文、架构测试、Copilot Prompt 的协同体 | Trinity                 |
| 版本同步  | 三者版本号必须保持一致                     | Version Synchronization |
| 变更传播  | ADR 变更时相关文件的级联更新                | Change Propagation      |
| 同步检测  | 自动检测版本号一致性的机制                   | Sync Detection          |
| 责任人通知 | 基于 CODEOWNERS 的变更通知             | Owner Notification      |
| 阻断构建  | 版本不一致时阻止 CI 通过                  | Build Blocking          |

---

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
>
> 🔒 **统一铁律**：
>
> ADR-980 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-980_<Rule>_<Clause>
> ```

---

### ADR-980_1：版本号关联规则（Rule）

#### ADR-980_1_1 ADR 正文、架构测试、Copilot Prompt 必须使用一致的版本号

ADR 正文、架构测试、Copilot Prompt **必须**使用一致的版本号。

#### ADR-980_1_2 版本号格式

**版本号格式**：

- ADR 正文：`version：X.Y`（在元数据区）
- 架构测试：`// version: X.Y`（在类注释中）
- Copilot Prompt：`version：X.Y`（在元数据区）

#### ADR-980_1_3 示例

**示例**：

ADR 正文 (`ADR-001-modular-monolith-vertical-slice-architecture.md`)：

```markdown
---
version: "2.0"
---

# ADR-001：模块化单体与垂直切片架构
```

架构测试 (`ADR_001_Architecture_Tests.cs`)：

```csharp
// Version: 3.2
// ADR: ADR-001
public class ADR_001_Architecture_Tests
{
    // ...
}
```

Copilot Prompt (`adr-001.prompts.md`)：

```markdown
---
version: "2.0"
---

# ADR-001 Copilot Prompts

**对应 ADR**：ADR-001-modular-monolith-vertical-slice-architecture
```

#### ADR-980_1_4 核心原则

**核心原则**：
> 三位一体同版本，变更即同步。

**判定**：

- ❌ ADR 版本 3.2，测试版本 3.1
- ❌ ADR 版本 3.2，Prompt 版本缺失
- ✅ ADR/测试/Prompt 均为 3.2

#### ADR-980_1_5 版本号一致性管理

- **版本同步工具**：使用 `scripts/validate-adr-version-sync.sh`，强制每次提交或变更时检查所有相关文件版本一致。
- **版本更新记录**：每次版本号更新时，必须在 PR 描述中明确注明“更新版本号的文件”，并保证 CI 自动检测到一致性。
- **责任归属**：若检测到版本号不一致，CI 将直接阻断 PR，并自动通知相关负责人（通过 `CODEOWNERS`）。

#### ADR-980_1_6 三位一体存在性裁决（Rule）

除非 ADR 明确声明为 “Document-Only ADR”，
否则每一个 Accepted 状态的 ADR：

- 必须存在至少一个架构测试 或
- 必须存在对应 Copilot Prompt

任意一项缺失，视为版本同步违规。

---

### ADR-980_2：同步检测自动化（Rule）

#### ADR-980_2_1 CI 必须包含版本同步检测步骤，不一致时必须阻断构建

CI **必须**包含版本同步检测步骤，不一致时 **必须**阻断构建。

#### ADR-980_2_2 检测范围

**检测范围**：

1. 对比 ADR 正文版本号
2. 对比对应架构测试版本号,必须存在，否则失败
3. 对比对应 Copilot Prompt 版本号,必须存在，否则失败

#### ADR-980_2_3 检测时机

**检测时机**：

- 每次 PR 提交
- 每次推送到主分支
- 每日定时检查

#### ADR-980_2_4 检测工具

**检测工具**：

- 脚本：`scripts/validate-adr-version-sync.sh`
- CI Workflow：`.github/workflows/adr-version-sync.yml`

#### ADR-980_2_5 版本号不一致

**失败处理**：

```
❌ 版本同步检测失败

ADR-001 版本不一致：
- ADR 正文：3.2
- 架构测试：3.1
- Copilot Prompt：3.2

请同步所有文件的版本号至 3.2
参考：ADR-980 变更传播清单
```

#### ADR-980_2_6 责任人修复时限

1. 当版本号不一致时，CI 会提供错误日志并自动通知**相关责任人**（通过 `CODEOWNERS`）。
2. 责任人必须在 24 小时内修复问题。
3. 未在 24 小时内修复，将自动生成**补救时限过期通知**。
4. 超过 48 小时未修复，**架构委员会**有权强制合并并向责任人报告问题。


#### ADR-980_2_7 同步文件缺失

- **反馈机制**：在 PR 和 CI 反馈中增加详细信息：
  - **版本不一致时**：指出具体版本不一致的文件（ADR、测试、Prompt）
  - **失败详细日志**：CI 需要自动生成**详细错误日志**，帮助开发者定位问题并提供修复建议。

#### ADR-980_2_8 超时未修复

- **修复责任**：如果未能在 24 小时内修复，**相关责任人**（根据 `CODEOWNERS`）将收到额外提醒，并必须在 48 小时内修复。

#### ADR-980_2_9 核心原则

- **原则**：自动检测版本不一致并生成详细报告，确保无障碍修复。

**判定**：

- ❌ 版本不一致但 CI 通过
- ❌ 手动检测版本同步
- ✅ 自动检测并阻断不一致

#### ADR-980_2_10 缺失文件反馈

1. CI 失败时，必须提供详细的**缺失文件清单**，包括：
  - **文件类型**（ADR 文档 / 测试文件 / Prompt）
  - **缺失文件名**及其相关版本号
  - **修复路径**，即：在哪个文件夹、如何补充文件
2. 在错误日志中，必须明确标示 **修复负责人** 和 **修复时限**。
3. 如果文件缺失问题未在 24 小时内修复，必须再次提醒责任人，并在 PR 中标注“未修复的缺失文件”。

#### ADR-980_2_11 超时未修复后果

1. 超过 48 小时仍未修复，**架构委员会**有权通过**强制 PR 合并**，即便版本未同步。
2. 架构委员会将发布“版本同步滞后”报告，并记录滞后的责任人及滞后时长。
3. 未修复的责任人将会受到**至少一次的内部评审**，并在团队会议中报告其修复效率问题。

#### ADR-980_2_12 工具失效应急处理

1. 如果 `validate-adr-version-sync.sh` 脚本或 CI Workflow 发生故障或未能正确执行：
  - 必须在 1 小时内由维护人员修复。
  - 如果无法在 24 小时内修复，架构委员会有责任手动干预，确保版本同步性检测继续进行。
2. CI 仍然应阻断所有未同步版本的提交，直到问题解决。

---

### ADR-980_3：变更传播清单（Rule）

#### ADR-980_3_1 修改 ADR 时必须遵循以下变更传播清单

修改 ADR 时 **必须**遵循以下变更传播清单。

#### ADR-980_3_2 标准清单

- **更改传播步骤**：
  1. **标记版本变更**：每次修改 ADR 版本时，必须更新所有相关文件的版本号。
  2. **PR 描述要求**：变更描述中必须清楚列出所有需要同步的文件（ADR、架构测试、Copilot Prompt）。
  3. **CI 检查与阻断**：如未同步版本号，CI 会自动阻断 PR，提供详细反馈。
  4. **通知机制**：使用 `CODEOWNERS` 配置，确保相关负责人收到版本更新通知，并在 48 小时内修复版本不一致问题。

#### ADR-980_3_3 责任人通知与追踪

- **责任人配置**：使用 `.github/CODEOWNERS` 配置，确保每次版本不一致时，相关责任人会收到通知。
- **通知追踪**：
  1. **PR 阶段**：每次 PR 时，CI 检测到版本不一致会自动通知相关责任人（通过 GitHub Notification 或邮件）。
  2. **责任人跟踪**：在 PR 描述中明确“责任人”修复责任。
  3. **跟踪报告**：CI 会生成**跟踪报告**，记录每次责任人处理反馈和时效性。

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
>
> 下表展示了 ADR-980 各条款（Clause）的执法方式及执行级别。

| 规则编号            | 执行级 | 执法方式         | Decision 映射  |
|-----------------|-----|--------------|--------------|
| **ADR-980_1_1** | L1  | 文档扫描版本号一致性   | §ADR-980_1_1 |
| **ADR-980_1_2** | L1  | 文档扫描版本号格式    | §ADR-980_1_2 |
| **ADR-980_1_3** | L1  | 文档扫描版本号示例    | §ADR-980_1_3 |
| **ADR-980_1_4** | L1  | 文档扫描核心原则     | §ADR-980_1_4 |
| **ADR-980_1_5** | L1  | 文档扫描版本号一致性管理 | §ADR-980_1_5 |
| **ADR-980_1_6** | L1  | 文档扫描三位一体存在性   | §ADR-980_1_6 |
| **ADR-980_2_1** | L1  | CI 检测版本同步    | §ADR-980_2_1 |
| **ADR-980_2_2** | L1  | CI 检测范围      | §ADR-980_2_2 |
| **ADR-980_2_3** | L1  | CI 检测时机      | §ADR-980_2_3 |
| **ADR-980_2_4** | L1  | CI 检测工具      | §ADR-980_2_4 |
| **ADR-980_2_5** | L1  | CI 版本号不一致    | §ADR-980_2_5 |
| **ADR-980_2_6** | L1  | CI 同步文件缺失    | §ADR-980_2_6 |
| **ADR-980_2_7** | L1  | CI 超时未修复     | §ADR-980_2_7 |
| **ADR-980_2_8** | L1  | CI 核心原则      | §ADR-980_2_8 |
| **ADR-980_3_1** | L1  | 文档扫描变更传播清单   | §ADR-980_3_1 |
| **ADR-980_3_2** | L1  | 文档扫描标准清单     | §ADR-980_3_2 |
| **ADR-980_3_3** | L1  | 文档扫描责任人通知与追踪 | §ADR-980_3_3 |

### 执行方式

1. **版本同步检查集成**：
  - `scripts/validate-adr-version-sync.sh` 每次 PR 提交时运行，强制同步检查。
  - `.github/workflows/adr-version-sync.yml` 实现版本检测并阻断构建。

2. **版本同步统计仪表盘**：
  - 通过定期生成的报告监控版本同步状态，确保团队按期修复不一致问题。

3. **改进反馈机制**：
  - 每月进行一次版本同步改进报告，总结上个月的执行情况和改进点。

---

## Non-Goals（明确不管什么）

- 不涉及文档内容的编写风格和语法纠错
- 不涉及 ADR 之外的文档（如 README，产品文档等）的同步管理
- 不评估跨团队协作中具体实施的过程管理
- - 不对“是否应当提升版本号”做语义判断；版本号是否变化，仅以提交内容是否修改 version 字段为准。

## Prohibited（禁止行为）

- 禁止提交版本号不一致的 PR
- 禁止在未修复版本不一致时合并 PR
- 禁止绕过 CI 阻断

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

- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md) - 文档版本管理
- [ADR-940：ADR 关系与溯源管理治理规范](../governance/ADR-940-adr-relationship-traceability-management.md) - 关系图更新

---

## References（非裁决性参考）

### 相关 ADR

- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md)
- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md)
- [ADR-900：ADR 新增与修订流程](../governance/ADR-900-architecture-tests.md)
- [ADR-940：ADR 关系与溯源管理治理规范](../governance/ADR-940-adr-relationship-traceability-management.md)

### 实施工具

- `scripts/validate-adr-version-sync.sh` - 版本同步检测脚本
- `.github/workflows/adr-version-sync.yml` - CI Workflow
- `.github/CODEOWNERS` - 责任人配置

### 背景材料

- [ADR-Documentation-Governance-Gap-Analysis.md](../proposals/ADR-Documentation-Governance-Gap-Analysis.md) - 原始提案

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                  |
|-----|------------|---------------------------------------|
| 2.0 | 2026-02-04 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | 架构委员会 |
| 1.0 | 2026-01-29 | 初始版本                                  |
