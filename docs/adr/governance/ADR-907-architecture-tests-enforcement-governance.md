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

1. **命名与组织规范（ADR-####）**  
   - 测试类、方法与项目命名必须明确映射 ADR  
   - 测试目录结构与 ADR 编号一一对应  
   - 禁止跨 ADR 混合测试或空弱测试

2. **最小断言语义（ADR-####）**  
   - 每条 ArchitectureTest 至少包含 1 个有效断言  
   - 测试方法映射单一 ADR 子规则  
   - 弱断言或形式化断言禁止

3. **Analyzer / CI Gate 映射协议（ADR-####）**  
   - 所有 ArchitectureTests 必须自动注册至 CI / Analyzer  
   - 测试失败直接映射 ADR 子规则，支持 L1/L2 执行等级  
   - 支持破例与偿还机制  
   - ADR 生命周期变更自动同步

**目标**：让 ArchitectureTests 成为 ADR 的“可执行镜像”，支持自动化裁决与反作弊机制。

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| ArchitectureTests | 验证 ADR 架构约束的测试集合 | ArchitectureTests |
| ADR 镜像 | 测试结构与 ADR 条目一一对应 | ADR Mirror |
| RuleId | 与 ADR 条目对应的唯一规则编号，格式为 `ADR-XXX.<Rule>_<Clause>`（如 ADR-907_2_1） | RuleId |
| Rule | 主要规则分类，对应测试类（如 ADR-907_1, ADR-907_2） | Rule |
| Clause | 具体条款，对应测试方法（如 ADR-907_2_1, ADR-907_2_2） | Clause |
| 最小断言 | 每个测试类必须包含的有效断言数量 | Minimum Assertions |
| 有效断言 | 验证结构约束或 ADR 条目，不为空或形式化 | Effective Assertion |
| CI Gate | 持续集成管道中自动执行架构测试 | CI Gate |
| Analyzer | 静态/运行时分析工具，验证规则与 ADR 映射 | Analyzer |
| Enforcement Level | L1 / L2 执行等级，L1 可自动阻断 | Enforcement Level |
| Exception Mechanism | ADR-#### 定义的破例 / 补救机制 | Exception Mechanism |


---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**

> 🔒 **统一铁律**：
> 
> ADR-907 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-907_<Rule>_<Clause>
> ```
> 
> - **Rule**：主要规则编号（1-4）
> - **Clause**：具体条款编号（1-n）
> - 每个 Clause 对应一个可测试的架构约束（L1）
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

### ADR-907_2：命名与组织规范（Rule，原 ADR-####）

#### ADR-907_2_1 独立测试项目要求
- ArchitectureTests **必须集中于独立测试项目**
- 项目命名格式：`<SolutionName>.Tests.Architecture`

#### ADR-907_2_2 ADR 编号目录分组
- 测试目录必须按 ADR 编号分组
- 目录格式：`/ADR-XXX/`

#### ADR-907_2_3 禁止跨 ADR 混合测试
- 单个测试类或文件仅允许覆盖一个 ADR
- 每个测试类专注于单一 ADR 的约束验证

#### ADR-907_2_4 测试类命名规范
- 测试类命名必须显式绑定 ADR
- 命名格式：`ADR_<编号>_Architecture_Tests`

#### ADR-907_2_5 测试方法命名规范
- 测试方法必须映射 ADR 子规则
- 命名格式：`ADR_<编号>_<子规则>_<行为描述>`

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

### ADR-907_3：最小断言语义规范（Rule，原 ADR-####）

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
- 明确禁止以下行为：
  - `Assert.True(true)` 等形式化断言
  - 仅验证测试可运行、不验证结构约束

---

### ADR-907_4：Analyzer / CI Gate 映射协议（Rule，原 ADR-####）

#### ADR-907_4_1 自动发现注册要求
- 所有 ArchitectureTests 必须被 Analyzer 自动发现并注册
- 支持 CI/CD 管道的自动执行

#### ADR-907_4_2 RuleId 精确映射
- 测试失败必须精确映射至 ADR 子规则（RuleId）
- 使用 `ADR-907_<Rule>_<Clause>` 格式标识

#### ADR-907_4_3 执行级别分类支持
- 支持执行级别分类（依赖 ADR-####）：
  - **L1**：失败即阻断 CI / 合并 / 部署
  - **L2**：失败记录告警，进入人工 Code Review

#### ADR-907_4_4 破例机制自动记录
- 破例机制必须自动记录：
  - ADR 编号
  - 测试类 / 方法
  - 破例原因
  - 到期时间与偿还计划

#### ADR-907_4_5 Analyzer 检测能力要求
- Analyzer 必须具备以下检测能力：
  - 空测试 / 弱断言
  - 单测试覆盖多 ADR
  - 非 Final ADR 生成测试

#### ADR-907_4_6 ADR 生命周期同步
- ADR 生命周期变更必须同步：
  - Superseded / Obsolete ADR 对应测试必须标记或移除

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-907 各条款（Clause）的执法方式及执行级别。
> 每条 Clause 对应一个或多个具体的测试方法，形成可机器执行的验证规则。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|-----|---------|--------------|
| **ADR-907_1_1** | L1  | ArchitectureTests 必须是唯一执法形式的元验证 | §ADR-907_1_1 |
| **ADR-907_1_2** | L1  | 检测 ADR 是否具备对应测试或 Non-Enforceable 声明 | §ADR-907_1_2 |
| **ADR-907_1_3** | L1  | 禁止无执法路径的架构规则存在 | §ADR-907_1_3 |
| **ADR-907_2_1** | L1  | ArchitectureTests 项目存在性校验 | §ADR-907_2_1 |
| **ADR-907_2_2** | L1  | ADR 目录结构扫描（/ADR-XXX/） | §ADR-907_2_2 |
| **ADR-907_2_3** | L1  | 测试类与 ADR 映射校验（单一 ADR） | §ADR-907_2_3 |
| **ADR-907_2_4** | L1  | 测试类命名正则校验 | §ADR-907_2_4 |
| **ADR-907_2_5** | L1  | 测试方法与子规则解析 | §ADR-907_2_5 |
| **ADR-907_2_6** | L1  | 失败信息 ADR 溯源校验 | §ADR-907_2_6 |
| **ADR-907_2_7** | L1  | 禁止空弱断言检测 | §ADR-907_2_7 |
| **ADR-907_2_8** | L1  | Skip / 条件禁用检测 | §ADR-907_2_8 |
| **ADR-907_3_1** | L1  | 最小断言数量语义检测 | §ADR-907_3_1 |
| **ADR-907_3_2** | L1  | 单一子规则映射校验 | §ADR-907_3_2 |
| **ADR-907_3_3** | L1  | 失败信息可溯源性校验 | §ADR-907_3_3 |
| **ADR-907_3_4** | L1  | 形式化断言检测 | §ADR-907_3_4 |
| **ADR-907_4_1** | L1  | CI / Analyzer 自动注册校验 | §ADR-907_4_1 |
| **ADR-907_4_2** | L1  | RuleId 精确映射验证 | §ADR-907_4_2 |
| **ADR-907_4_3** | L1  | L1 阻断 / L2 告警策略执行 | §ADR-907_4_3 |
| **ADR-907_4_4** | L1  | 破例与偿还机制记录 | §ADR-907_4_4 |
| **ADR-907_4_5** | L1  | Analyzer 检测能力完整性验证 | §ADR-907_4_5 |
| **ADR-907_4_6** | L1  | ADR 生命周期同步校验 | §ADR-907_4_6 |

### 执法级别说明

- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决

### 测试组织映射

基于上述 Enforcement 表，ADR-907 的测试应组织为：

- **测试类数量**：4 个（对应 4 个 Rule）
  - `ADR_907_1_Architecture_Tests.cs` → ADR-907_1（3 个测试方法）
  - `ADR_907_2_Architecture_Tests.cs` → ADR-907_2（8 个测试方法）
  - `ADR_907_3_Architecture_Tests.cs` → ADR-907_3（4 个测试方法）
  - `ADR_907_4_Architecture_Tests.cs` → ADR-907_4（6 个测试方法）

- **测试方法数量**：21 个（对应 21 个 Clause）
  - 每个测试方法命名格式：`ADR_907_<Rule>_<Clause>_<行为描述>`
  - 示例：`ADR_907_2_1_ArchitectureTests_Project_Must_Exist`

---

## Non-Goals（明确不管什么）

- 架构规则本身的业务正确性
- 单元测试 / 集成测试的编写规范
- 测试框架选型（xUnit / NUnit / MSTest）
- 测试覆盖率指标
- 测试性能与执行时间优化
- 测试代码风格与美学

---

## Prohibited（禁止行为）

- 无 ADR 编号的 ArchitectureTests  
- 一个测试类覆盖多个 ADR  
- 弱断言或空测试未被检测  
- 跳过 CI / Analyzer 阻断  
- 弃用 ADR 的测试未同步处理

---

## Relationships（关系声明）

**Depends On**：

- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md)  
- [ADR-905：执行级别分类](../governance/ADR-905-enforcement-level-classification.md)  

**Depended By**：

- 所有 ArchitectureTests 执法流程  
- CI / Analyzer 自动裁决规则
- [ADR-907-A：对齐执行标准](./adr-907-a-adr-alignment-execution-standard.md)

**Related**：

- [ADR-122：测试代码组织与命名规范](../structure/ADR-122-test-organization-naming.md)
- [ADR-907-A：对齐执行标准](./adr-907-a-adr-alignment-execution-standard.md)（执行附录）

**Supersedes**：
- [ADR-903：ArchitectureTests 命名与组织规范](../archive/governance/ADR-903-architecture-tests-naming-organization.MD)
- [ADR-904：ArchitectureTests 最小断言语义规范](../archive/governance/ADR-904-architecturetests-minimum-assertion-semantics.md)
- [ADR-906：Analyzer 与 CI Gate 映射协议](../archive/governance/ADR-906-analyzer-ci-gate-mapping-protocol.md)

**Superseded By**：
- 无

---

## Superseded ADRs（已替代的 ADR）

> ⚖️ **治理级宣告**：
> 
> ADR-903 / 904 / 906 不得被单独引用。
> 所有 ArchitectureTests、Analyzer 规则、CI Gate 只允许引用 ADR-907。
> 
> 上述 ADR 已正式归档，仅用于历史追溯与设计演进说明，不再具备任何裁决力或执法权。

---

## References（非裁决性参考）

- NetArchTest.Rules  
- GitHub Actions / Azure DevOps / Jenkins CI Gate 实践  
- xUnit / NUnit 架构测试实践  
- FluentAssertions 断言语义规范

---

## History（版本历史）

| 版本  | 日期         | 说明       | 修订人 |
|-----|------------|----------|-----|
| 1.0 | 2026-01-28 | 首次整合正式发布 | Architecture Board |
| 2.0 | 2026-02-02 | 引入 Rule/Clause 双层编号体系，实现 Decision 与 Enforcement 一一映射 | Architecture Board |
| 2.1 | 2026-02-02 | 新增执行附录 ADR-907-A，定义对齐执行标准、失败策略与测试绑定规则 | Architecture Board |
