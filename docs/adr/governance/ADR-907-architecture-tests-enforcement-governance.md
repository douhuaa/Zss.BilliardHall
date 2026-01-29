---
adr: ADR-907
title: "ArchitectureTests 执法治理体系"
status: Final
level: Governance
version: "1.0"
deciders: "Architecture Board"
date: 2026-01-27
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

**目标**：让 ArchitectureTests 成为 ADR 的“可执行镜像”，支持自动化裁决与反作弊机制。

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| ArchitectureTests | 验证 ADR 架构约束的测试集合 | ArchitectureTests |
| ADR 镜像 | 测试结构与 ADR 条目一一对应 | ADR Mirror |
| RuleId | 与 ADR 条目对应的唯一规则编号（如 ADR-240.1） | RuleId |
| 最小断言 | 每个测试类必须包含的有效断言数量 | Minimum Assertions |
| 有效断言 | 验证结构约束或 ADR 条目，不为空或形式化 | Effective Assertion |
| CI Gate | 持续集成管道中自动执行架构测试 | CI Gate |
| Analyzer | 静态/运行时分析工具，验证规则与 ADR 映射 | Analyzer |
| Enforcement Level | L1 / L2 执行等级，L1 可自动阻断 | Enforcement Level |
| Exception Mechanism | ADR-0000 定义的破例 / 补救机制 | Exception Mechanism |


---

## Decision（裁决）

### 1. ArchitectureTests 的法律地位

1. ArchitectureTests 是 ADR 的**唯一自动化执法形式**
2. 任何具备裁决力的 ADR：
   - 要么已有 ArchitectureTests
   - 要么明确声明为 _Non-Enforceable_
3. 不存在“仅文档约束、不接受执行”的架构规则


---

### 2. 命名与组织规范（原 ADR-903）

1. ArchitectureTests **必须集中于独立测试项目**
   - `<SolutionName>.Tests.Architecture`
2. 测试目录必须按 ADR 编号分组
   - `/ADR-XXXX/`
3. 禁止跨 ADR 混合测试
   - 单个测试类或文件仅允许覆盖一个 ADR
4. 测试类命名必须显式绑定 ADR
   - `ADR_<编号>_Architecture_Tests`
5. 测试方法必须映射 ADR 子规则
   - `ADR_<编号>_<子规则>_<行为描述>`
6. 测试失败信息必须包含 ADR 编号与子规则
7. ArchitectureTests 不得为空、占位或弱断言
8. 不得 Skip、条件禁用测试（除非走破例机制）

---

### 3. 最小断言语义规范（原 ADR-904）

1. 每个测试类 **至少包含 1 个有效断言**
2. 每个测试方法 **只能映射一个 ADR 子规则**
3. 所有断言失败信息必须可反向溯源到 ADR
4. 明确禁止以下行为：
  - `Assert.True(true)` 等形式化断言
  - 仅验证测试可运行、不验证结构约束

---

### 4. Analyzer / CI Gate 映射协议（原 ADR-906）

1. 所有 ArchitectureTests 必须被 Analyzer 自动发现并注册
2. 测试失败必须精确映射至 ADR 子规则（RuleId）
3. 支持执行级别分类（依赖 ADR-905）：
   - **L1**：失败即阻断 CI / 合并 / 部署
   - **L2**：失败记录告警，进入人工 Code Review
4. 破例机制必须自动记录：
   - ADR 编号
   - 测试类 / 方法
   - 破例原因
   - 到期时间与偿还计划
5. Analyzer 必须具备以下检测能力：
   - 空测试 / 弱断言
   - 单测试覆盖多 ADR
   - 非 Final ADR 生成测试
6. ADR 生命周期变更必须同步：
   - Superseded / Obsolete ADR 对应测试必须标记或移除

---

## Enforcement（执法模型）

|规则编号|执行级|执法方式|
|---|---|---|
|ADR-907.1|L1|ArchitectureTests 项目存在性校验|
|ADR-907.2|L1|ADR 目录结构扫描|
|ADR-907.3|L1|测试类与 ADR 映射校验|
|ADR-907.4|L1|测试类命名正则校验|
|ADR-907.5|L1|测试方法与子规则解析|
|ADR-907.6|L1|失败信息 ADR 溯源校验|
|ADR-907.7|L2|最小断言数量与语义检测|
|ADR-907.8|L1|Skip / 条件禁用检测|
|ADR-907.9|L1|CI / Analyzer 自动注册校验|
|ADR-907.10|L1|L1 阻断 / L2 告警策略执行|
|ADR-907.11|L1|破例与偿还机制记录|
|ADR-907.12|L2|ADR 生命周期同步校验|

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

- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md)  
- [ADR-905：执行级别分类](../governance/ADR-905-enforcement-level-classification.md)  

**Depended By**：

- 所有 ArchitectureTests 执法流程  
- CI / Analyzer 自动裁决规则  

**Related**：

- [ADR-122：测试代码组织与命名规范](../structure/ADR-122-test-organization-naming.md)

**Supersedes**：

- [ADR-903：ArchitectureTests 命名与组织规范](../archive/governance/ADR-903-architecture-tests-naming-organization.MD)
- [ADR-904：ArchitectureTests 最小断言语义规范](../archive/governance/ADR-904-architecturetests-minimum-assertion-semantics.md)
- [ADR-906：Analyzer 与 CI Gate 映射协议](../archive/governance/ADR-906-analyzer-ci-gate-mapping-protocol.md)

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
