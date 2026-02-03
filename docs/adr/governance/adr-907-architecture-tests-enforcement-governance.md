---
adr: ADR-907
title: "ArchitectureTests 执法治理体系"
status: Final
level: Governance
version: "2.1"
deciders: "Architecture Board"
date: 2026-02-02
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: ADR-903, ADR-904, ADR-906
superseded_by: null
---


# ADR-907：ArchitectureTests 执法治理体系

> ⚖️ **本 ADR 链整合 ArchitectureTests 的命名、组织、最小断言及 CI / Analyzer 映射规则，实现完整的自动裁决闭环。**

---

## Focus（聚焦内容）

本体系覆盖以下治理目标：

1. **命名与组织规范（ADR-903）**  
   - 测试类、方法与项目命名必须明确映射 ADR  
   - 测试目录结构与 ADR 编号一一对应  
   - 禁止跨 ADR 混合测试或空弱测试

2. **最小断言语义（ADR-904）**  
   - 每条 ArchitectureTest 至少包含 1 个有效断言  
   - 测试方法映射单一 ADR 子规则  
   - 弱断言或形式化断言禁止

3. **Analyzer / CI Gate 映射协议（ADR-906）**  
   - 所有 ArchitectureTests 必须自动注册至 CI / Analyzer  
   - 测试失败直接映射 ADR 子规则，支持 L1/L2 执行等级  
   - 支持破例与偿还机制  
   - ADR 生命周期变更自动同步

**目标**：让 ArchitectureTests 成为 ADR 的"可执行镜像"，支持自动化裁决与反作弊机制。

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| ArchitectureTests | 验证 ADR 架构约束的测试集合 | ArchitectureTests |
| ADR 镜像 | 测试结构与 ADR 条目一一对应 | ADR Mirror |
| RuleId | 与 ADR 条目对应的唯一规则编号，格式为 `ADR-XXX_<Rule>_<Clause>`（如 ADR-907_2_1） | RuleId |
| Rule | 主要规则分类，对应测试类（如 ADR-907_1, ADR-907_2） | Rule |
| Clause | 具体条款，对应测试方法（如 ADR-907_2_1, ADR-907_2_2） | Clause |
| 最小断言 | 每个测试类必须包含的有效断言数量 | Minimum Assertions |
| 有效断言 | 验证结构约束或 ADR 条目，不为空或形式化 | Effective Assertion |
| CI Gate | 持续集成管道中自动执行架构测试 | CI Gate |
| Analyzer | 静态/运行时分析工具，验证规则与 ADR 映射 | Analyzer |
| Enforcement Level | L1 / L2 执行等级，L1 可自动阻断 | Enforcement Level |
| Exception Mechanism | ADR-900 定义的破例 / 补救机制 | Exception Mechanism |


---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-907 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-907_<Rule>_<Clause>
> ```
> 
> - **Rule**：主要规则编号（1-4）
> - **Clause**：具体条款编号（1-n）
> - 每个 Clause 对应一个可测试的架构约束
> - 测试方法必须一一映射到 Clause

---

### ADR-907_1：ArchitectureTests 的法律地位（Rule）

#### ADR-907_1_1 唯一自动化执法形式
- ArchitectureTests 是 ADR 的**唯一自动化执法形式**
- ADR-907_1 的测试：
  - 允许使用 文件系统扫描
  - 允许使用 Roslyn / Reflection
  - 不要求使用 NetArchTest

#### ADR-907_1_2 可执法性要求
- 任何具备裁决力的 ADR 必须满足以下条件之一：
  - 已有对应的 ArchitectureTests
  - 明确声明为 _Non-Enforceable_

#### ADR-907_1_3 禁止文档约束例外
- 不存在声明为"文档专属、拒绝自动化"的架构规则

---

### ADR-907_2：命名与组织规范（Rule，原 ADR-903）

#### ADR-907_2_1 独立测试项目要求
- ArchitectureTests **必须集中于独立测试项目**
- 项目命名格式：`<SolutionName>.Tests.Architecture`

#### ADR-907_2_2 ADR 编号目录分组
- 测试目录必须按 ADR 编号分组
- 目录格式：`/ADR-XXXX/`

#### ADR-907_2_3 禁止跨 ADR 混合测试
- 单个测试类或文件仅允许覆盖一个 ADR
- 每个测试类专注于单一 ADR 的约束验证

#### ADR-907_2_4 测试类命名规范
- 测试类命名必须显式绑定 ADR
- 命名格式：`ADR_<编号>_<Rule>_Architecture_Tests`
- 示例：`ADR_907_2_Architecture_Tests.cs`

#### ADR-907_2_5 测试方法命名规范
- 测试方法必须映射 ADR 子规则
- 命名格式：`ADR_<编号>_<Rule>_<Clause>_<行为描述>`
- 示例：`ADR_907_2_1_Test_Method_Naming_Convention()`

#### ADR-907_2_6 失败信息溯源要求
- 测试失败信息必须包含 ADR 编号与子规则
- 支持从失败信息反向追溯到 ADR 条款

#### ADR-907_2_7 禁止空弱断言
- ArchitectureTests 不得为空、占位或弱断言
- 每个测试必须包含有效的架构约束验证

#### ADR-907_2_8 禁止跳过测试
- 不得 Skip、条件禁用测试（除非走破例机制）
- 所有测试必须正常执行或通过破例流程处理

---

### ADR-907_3：最小断言语义规范（Rule，原 ADR-904）

#### ADR-907_3_1 最小断言数量要求
- 每个测试类 **至少包含 1 个有效断言**
- 验证至少一个架构约束

#### ADR-907_3_2 单一子规则映射
- 每个测试方法 **只能映射一个 ADR 子规则**
- 保持测试的单一职责和清晰性

#### ADR-907_3_3 失败信息可溯源性
- 所有断言失败信息必须可反向溯源到 ADR
- 包含 ADR 编号和具体条款引用

#### ADR-907_3_4 禁止形式化断言
- 禁止使用无实质验证的断言
- 如：`Assert.True(true)` 或 `Assert.NotNull(new object())`

---

### ADR-907_4：CI / Analyzer 映射协议（Rule，原 ADR-906）

#### ADR-907_4_1 自动注册要求
- 所有 ArchitectureTests 必须在 CI 管道中自动发现并执行
- 不得通过手动配置排除测试

#### ADR-907_4_2 失败映射规则
- 测试失败必须直接映射到 RuleId
- CI 报告必须显示具体违规的 ADR 条款

#### ADR-907_4_3 执行级别支持
- L1 违规：自动阻断 CI，阻止合并
- L2 违规：记录告警，需 Code Review 批准
- L3 违规：仅记录，需架构师裁决

#### ADR-907_4_4 破例机制集成
- 支持 ADR-900 定义的破例流程
- 破例记录必须关联 RuleId 和偿还计划

#### ADR-907_4_5 生命周期同步
- ADR 废弃时，对应测试必须标记为废弃
- ADR 修订时，测试必须同步更新

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-907 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-907_1_1** | L2 | 人工审查（架构测试存在性检查） | §ADR-907_1_1 |
| **ADR-907_1_2** | L2 | 人工审查（ADR 可执法性声明） | §ADR-907_1_2 |
| **ADR-907_1_3** | L2 | 人工审查（ADR 内容审查） | §ADR-907_1_3 |
| **ADR-907_2_1** | L1 | ArchitectureTests 自动化验证 | §ADR-907_2_1 |
| **ADR-907_2_2** | L1 | ArchitectureTests 自动化验证 | §ADR-907_2_2 |
| **ADR-907_2_3** | L1 | ArchitectureTests 自动化验证 | §ADR-907_2_3 |
| **ADR-907_2_4** | L1 | ArchitectureTests 自动化验证 | §ADR-907_2_4 |
| **ADR-907_2_5** | L1 | ArchitectureTests 自动化验证 | §ADR-907_2_5 |
| **ADR-907_2_6** | L1 | ArchitectureTests 自动化验证 | §ADR-907_2_6 |
| **ADR-907_2_7** | L1 | ArchitectureTests 自动化验证 | §ADR-907_2_7 |
| **ADR-907_2_8** | L1 | ArchitectureTests 自动化验证 | §ADR-907_2_8 |
| **ADR-907_3_1** | L1 | ArchitectureTests 自动化验证 | §ADR-907_3_1 |
| **ADR-907_3_2** | L1 | ArchitectureTests 自动化验证 | §ADR-907_3_2 |
| **ADR-907_3_3** | L1 | ArchitectureTests 自动化验证 | §ADR-907_3_3 |
| **ADR-907_3_4** | L1 | ArchitectureTests 自动化验证 | §ADR-907_3_4 |
| **ADR-907_4_1** | L1 | CI 管道配置验证 | §ADR-907_4_1 |
| **ADR-907_4_2** | L1 | CI 报告格式验证 | §ADR-907_4_2 |
| **ADR-907_4_3** | L1 | CI 执行级别验证 | §ADR-907_4_3 |
| **ADR-907_4_4** | L2 | 人工审查（破例流程） | §ADR-907_4_4 |
| **ADR-907_4_5** | L2 | 人工审查（ADR 变更同步） | §ADR-907_4_5 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决

---

## Relationships（关联关系）

### 继承与整合
- **整合自**：ADR-903, ADR-904, ADR-906
- **依赖**：ADR-900（ADR 流程）
- **关联**：ADR-0000（架构测试基础）

### 影响范围
- 所有后续 ADR 的架构测试组织
- CI/CD 管道配置
- Analyzer 工具集成

---

## History（历史记录）

| 版本 | 日期 | 变更说明 | 变更人 |
|------|------|---------|--------|
| 2.1 | 2026-02-02 | 细化 RuleId 格式，增加测试类命名示例 | Architecture Board |
| 2.0 | 2026-02-02 | 整合 ADR-903/904/906，引入 Rule/Clause 双层编号体系 | Architecture Board |
| 1.0 | 2025-12-15 | 初始版本 | Architecture Board |

---

**维护**：Architecture Board  
**审核**：@douhuaa  
**状态**：✅ Final
