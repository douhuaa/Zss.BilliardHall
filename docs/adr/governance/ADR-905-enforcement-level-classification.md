---
adr: ADR-905
title: "执行级别分类"
status: Final
level: Governance
deciders: "Architecture Board"
date: 2026-02-03
version: "3.0"
maintainer: "架构委员会"
primary_enforcement: L1
reviewer: "@douhuaa"
supersedes: null
superseded_by: null
enforceable: false
---


# ADR-905：执行级别分类

> ⚖️ **本 ADR 定义架构规则的执行级别分类体系，明确静态测试、语义分析和人工审查的边界。**

---

## Focus（聚焦内容）

本 ADR 聚焦于解决以下架构治理问题：

* 架构规则的**可执行性边界不明确**，导致验证歧义
* 开发者误以为"测试通过 = 完全合规"
* 架构师不知道哪些规则需要人工 Gate
* 工具能力与规则语义之间的**错配**

**适用场景**：
* 定义 ADR 规则的执行级别
* 指导架构测试实现
* 规范人工审查流程

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|---------|
| 执行级别 | 架构规则的自动化验证能力分级 | Enforcement Level |
| L1 静态可执行 | 可通过静态分析工具完全自动化检查 | Level 1: Static Enforceable |
| L2 语义半自动 | 需要语义分析，当前为启发式检查 | Level 2: Semantic Semi-Auto |
| L3 人工 Gate | 无法或不应该完全自动化，需人工裁决 | Level 3: Manual Gate |
| NetArchTest | .NET 架构测试静态分析工具 | NetArchTest |
| Roslyn Analyzer | C# 编译器语义分析器 | Roslyn Analyzer |

---

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-905 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-905_<Rule>_<Clause>
> ```

---

### ADR-905_1：执行级别分类体系（Rule）

#### ADR-905_1_1 架构规则必须分级执行

**规则**：

所有架构规则（特别是宪法层 ADR）**必须**明确声明其执行级别：
- **Level 1（L1）**：静态可执行 - 通过 NetArchTest 等静态分析工具自动化检查
- **Level 2（L2）**：语义半自动 - 通过 Roslyn Analyzer 或启发式检查
- **Level 3（L3）**：人工 Gate - 需要人工审查和决策

**判定**：
- ✅ ADR 规则定义时明确标注执行级别
- ✅ 不同级别规则采用相应的验证手段
- ❌ 使用错误级别的工具验证规则

---

#### ADR-905_1_2 Level 1（L1）规则的执行标准

**定义**：可以通过 NetArchTest 等静态分析工具完全自动化检查的规则。

**特征**：
- 基于类型、命名空间、依赖关系的静态约束
- 测试失败 = 绝对违规
- CI 阻断

**工具**：ArchitectureTests（NetArchTest）

**适用规则示例**（基于 ADR-####）：

| 规则编号 | 规则描述 | 测试方法 |
|---------|---------|---------|
| ADR-####.1 | Handler 应有明确的命名约定 | `Handlers_Should_Have_Clear_Naming_Convention` |
| ADR-####.3 | Handler 不应依赖 ASP.NET 类型 | `Handlers_Should_Not_Depend_On_AspNet` |
| ADR-####.4 | Handler 应该是无状态的（字段检查） | `Handlers_Should_Be_Stateless` |
| ADR-####.5 | 模块间不应有未审批的同步调用（依赖检查） | `Modules_Should_Not_Have_Synchronous_Cross_Module_Calls` |
| ADR-####.6 | 异步方法应遵循命名约定 | `Async_Methods_Should_Follow_Naming_Convention` |
| ADR-####.7 | 模块不应共享领域实体 | `Modules_Should_Not_Share_Domain_Entities` |
| ADR-####.9 | Command 和 Query Handler 应明确分离 | `Command_And_Query_Handlers_Should_Be_Separated` |
| ADR-####.12 | 所有 Handler 应在模块程序集中 | `All_Handlers_Should_Be_In_Module_Assemblies` |

**判定**：
- ✅ L1 规则测试失败必须修复代码，不可破例
- ❌ 修改或注释测试以通过 CI
- ❌ 为不属于 L1 的规则创建静态测试

---

#### ADR-905_1_3 Level 2（L2）规则的执行标准

**定义**：需要语义分析的规则，建议通过 Roslyn Analyzer 检查，当前测试只能做启发式检查。

**特征**：
- 需要理解代码语义（如方法调用链、业务逻辑复杂度）
- 当前测试是"建议性"而非"强制性"
- 测试失败 = 需要人工审查

**工具**：
- 当前：ArchitectureTests（启发式检查）
- 建议：Roslyn Analyzer（自定义分析器）

**适用规则示例**（基于 ADR-####）：

| 规则编号 | 规则描述 | 当前测试方法 | Analyzer 需求 |
|---------|---------|-------------|--------------|
| ADR-####.2 | Endpoint 不应包含业务逻辑 | `Endpoints_Should_Not_Contain_Business_Logic` | 检查方法体复杂度、业务规则调用 |
| ADR-####.5 | 模块间异步通信（Handler 调用检查） | `Modules_Should_Not_Have_Synchronous_Cross_Module_Calls` | 检查方法调用链、跨模块 public 方法调用 |
| ADR-####.10 | Command Handler 不应返回业务数据 | `CommandHandlers_Should_Not_Return_Business_Data` | 检查返回类型的语义（简单类型 vs 业务对象） |
| ADR-####.11 | Handler 应使用结构化异常 | `Handlers_Should_Use_Structured_Exceptions` | IL 分析，检查 `throw new Exception` |

**判定**：
- ✅ L2 规则测试失败可申请破例，需充分理由
- ✅ 优先实现 Roslyn Analyzer 提高准确度
- ❌ 将 L2 规则当作 L1 强制执行

---

#### ADR-905_1_4 Level 3（L3）规则的执行标准

**定义**：无法（或不应该）完全自动化的规则，需要人工审查和决策。

**特征**：
- 涉及业务语义、设计意图、架构权衡
- 测试无法覆盖，或覆盖成本过高
- 依赖 PR Review + ARCH-VIOLATION 标记

**工具**：
- PR Checklist
- 架构师 Code Review
- ARCH-VIOLATION 记录表

**适用规则示例**（基于 ADR-####）：

| 规则编号 | 规则描述 | 人工审查要点 |
|---------|---------|-------------|
| ADR-####.5 | 模块间同步调用破例审批 | 是否有明确的远程调用契约？是否处理了超时/降级？ |
| ADR-####.8 | Query Handler 可以返回 Contracts | 返回的 Contract 是否版本化？是否只读？ |
| 事务边界 | 跨模块事务与 Saga | 是否真的需要强一致性？Saga 补偿逻辑是否完整？ |
| 业务决策 | Handler 职责划分 | 业务逻辑是否集中在一个 Handler？是否有隐藏的跨 Handler 协作？ |

**人工 Gate 流程**：

1. **PR 提交时**：开发者在 PR 模板中声明是否存在架构破例
2. **CI 检查**：如果 ArchitectureTests 失败但 PR 声明"无违规" → 自动拒绝
3. **架构审查**：如果 PR 声明"有破例" → 架构师审查并决定是否批准
4. **记录归档**：所有破例必须记录在 `docs/summaries/arch-violations.md` 中

**判定**：
- ✅ L3 规则破例必须经过人工审批
- ✅ 破例必须记录并定期审计
- ❌ 跳过人工审查直接合并
- ❌ 破例不记录或不设归还计划

---

#### ADR-905_1_5 执行级别的分级意义

**规则**：

三级分类体系**必须**被以下角色理解和执行：

**对开发者**：
- 明确知道哪些规则是"红线"（Level 1 测试失败必须修复）
- 明确知道哪些规则是"建议"（Level 2 可以申请破例）
- 明确知道哪些规则需要"沟通"（Level 3 必须人工审批）

**对架构师**：
- Level 1：零容忍，CI 自动阻断
- Level 2：可协商，但需要充分理由
- Level 3：预期破例，重点在于记录和可追溯

**对工具链**：
- Level 1 → NetArchTest（已实现）
- Level 2 → Roslyn Analyzer（建议实现）
- Level 3 → PR Template + Review Process（必须实现）

**判定**：
- ✅ 所有架构规则明确标注执行级别
- ✅ 不同级别采用对应的验证工具
- ❌ 混淆不同级别的执行标准

---

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-905 各条款（Clause）的执法方式及执行级别。
>
> 本节为唯一架构执法映射表，所有必测/必拦规则均需在此列明。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-905_1_1** | L2 | ADR 编写时的执行级别声明检查（Code Review） | §ADR-905_1_1 |
| **ADR-905_1_2** | L1 | NetArchTest 架构测试（CI 阻断） | §ADR-905_1_2 |
| **ADR-905_1_3** | L2 | Roslyn Analyzer（警告）+ 人工审查 | §ADR-905_1_3 |
| **ADR-905_1_4** | L3 | PR Template + 架构师审查 + ARCH-VIOLATIONS 记录 | §ADR-905_1_4 |
| **ADR-905_1_5** | L2 | Code Review + 教育培训 | §ADR-905_1_5 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决

---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **具体技术的执行级别判定**：不为每一个具体技术栈或工具指定执行级别
- **业务逻辑的重要性分级**：不涉及业务功能的优先级或重要性评估
- **团队能力和成熟度评估**：不评估团队是否有能力执行特定级别的规则
- **执行成本的量化分析**：不计算每个级别规则的实施成本和ROI
- **执行级别的动态调整机制**：不建立根据项目阶段动态调整级别的流程
- **人工审查的详细流程**：不规定人工Gate的具体审查步骤和检查清单
- **CI工具的具体配置**：不涉及GitHub Actions、Azure Pipelines等工具的配置细节
- **警告信息的展示方式**：不规定L2/L3警告在开发环境中的具体展示形式

---

## Prohibited（禁止行为）

本 ADR 执行中 **严禁**：

* 将 L1 规则降级为 L2/L3 以"方便通过"
* 将 L3 规则强行自动化导致误报
* 修改或注释测试以通过 CI
* 跳过人工 Gate 流程直接合并破例代码
* 破例不记录或不设归还计划
* 混淆不同级别的执行标准

---

---

## Relationships（关系声明）

**Depends On**：
- [ADR-005：应用内交互模型与执行边界](../constitutional/ADR-005-Application-Interaction-Model-Final.md) - 本 ADR 为 ADR-005 的执行级别补充
- [ADR-900：架构测试与 CI 治理元规则](./ADR-900-architecture-tests.md) - 执行级别基于测试和 CI 治理机制
- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md) - 文档治理宪法
- [ADR-902：治理类 ADR 标准格式](./ADR-902-adr-template-structure-contract.md) - 本文档标准来源

**Depended By**：
- [ADR-903：ArchitectureTests 命名与组织规范](./ADR-903-architecture-tests-naming-organization.MD) - 测试组织需要明确执行级别
- [ADR-906：Analyzer 与 CI Gate 映射协议](./ADR-906-analyzer-ci-gate-mapping-protocol.md) - CI Gate 映射依赖执行级别分类

**Supersedes**：
- 无

**Superseded By**：
- 无

**Related**：
- [ADR-001：模块化单体与垂直切片架构](../constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md) - 执行级别应用于模块化约束
- [ADR-002：Platform / Application / Host 三层启动体系](../constitutional/ADR-002-platform-application-host-bootstrap.md) - 执行级别应用于三层约束
- [ADR-003：命名空间与项目边界规范](../constitutional/ADR-003-namespace-rules.md) - 执行级别应用于命名空间约束
- [ADR-004：中央包管理（CPM）规范](../constitutional/ADR-004-Cpm-Final.md) - 执行级别应用于依赖管理约束
- [ADR-124：Endpoint 命名及参数约束规范](../structure/ADR-124-endpoint-naming-constraints.md) - 参考本文档的执行级别
- [ADR-120：领域事件命名规范](../structure/ADR-120-domain-event-naming-convention.md) - 参考本文档的执行级别
- [ADR-121：契约（Contract）与 DTO 命名组织规范](../structure/ADR-121-contract-dto-naming-organization.md) - 参考本文档的执行级别
- [ADR-210：领域事件版本化与兼容性](../runtime/ADR-210-event-versioning-compatibility.md) - 参考本文档的执行级别
- [ADR-240：Handler 异常约束](../runtime/ADR-240-handler-exception-constraints.md) - 参考本文档的执行级别
- [ADR-201：Handler 生命周期管理](../runtime/ADR-201-handler-lifecycle-management.md) - 参考本文档的执行级别
- [ADR-920：示例代码治理规范](./ADR-920-examples-governance-constitution.md) - 参考本文档的执行级别
- [ADR-930：代码审查与 ADR 合规自检流程](./ADR-930-code-review-compliance.md) - 人工 Gate 流程依赖代码审查

---

---

## References（非裁决性参考）

### 背景与原理

ADR-005 定义的是"运行时秩序"，但工具能力有物理极限：

- **NetArchTest** 只能做编译期静态分析
- **Roslyn Analyzer** 可以做语义级检查
- **人工审查** 是最后的防线

如果不明确区分这三个层次，会导致：
- 开发者误以为"测试通过 = 完全合规"
- 架构师不知道哪些规则需要人工 Gate
- 新人不理解为什么"测试过了还不允许"

### 从"文档体系"到"语言级治理系统"

此 ADR 标志着架构治理从**单一工具验证**升级为**三层防御体系**：

- **自动化层**（Level 1）：拦截 90% 的明显违规
- **辅助工具层**（Level 2）：提示潜在风险，辅助人工决策
- **人工决策层**（Level 3）：处理复杂场景，记录权衡过程

> **核心观点**：ADR-005 的本质是"运行时秩序"，静态测试是"近似执行"，不是"完全执行"。
> 通过三级分类，我们承认工具的局限性，不依赖单一工具的"完美"，建立了人机协作的架构治理体系。
> 这不是妥协，这是成熟。

### Roslyn Analyzer 实现建议

```csharp
// 示例：Endpoint 业务逻辑检查
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EndpointBusinessLogicAnalyzer : DiagnosticAnalyzer
{
    // 检测：
    // 1. Endpoint 方法体超过 10 行
    // 2. Endpoint 方法包含 if/switch 业务判断
    // 3. Endpoint 方法直接操作 DbContext
}
```

### 当前状态与改进路径

**✅ 已完成**：
- Level 1 规则已通过 ArchitectureTests 覆盖
- 测试失败消息明确标注违规规则
- Level 2 的 Roslyn Analyzer 已实现（Endpoint 业务逻辑检查、Handler 跨模块调用语义检查、异常使用规范检查）
- Level 3 的人工 Gate 流程已建立（PR Template、ARCH-VIOLATIONS 记录表、破例审批流程）

**⚠️ 待完善（可选）**：
- 扩展更多 Roslyn Analyzer（Command Handler 返回值检查、异步方法命名约定检查等）
- 定期评审和优化（收集 Analyzer 误报案例、根据团队反馈调整严重级别）

---


---

---

## History（版本历史）

| 版本 | 日期 | 变更说明 | 修订人 |
|------|------|----------|------|
| 3.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
| 2.0 | 2026-01-26 | 按照 ADR-901 标准格式重写，规范章节结构和裁决表达 | GitHub Copilot |
| 1.0 | 2025-01-20 | 初始版本，定义三级执行分类体系 | Architecture Board |
