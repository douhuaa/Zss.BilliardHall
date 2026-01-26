# ADR-0005 执行级别分类（Enforcement Level Classification）

> **注意**：本文档是 [ADR-0005：应用内交互模型与执行边界](ADR-0005-Application-Interaction-Model-Final.md) 的补充文档。  
> 主 ADR 定义"是什么"，本文档定义"如何验证"。  
> 同时参见：[ADR-0000：架构测试与 CI 治理](ADR-0000-architecture-tests.md)

**状态**：✅ Active  
**级别**：架构约束 / 补充说明  
**适用范围**：ADR-0005 所有规则的验证和执行  
**关联 ADR**：ADR-0005 应用内交互模型与执行边界  
**目的**：明确定义 ADR-0005 各项规则的可执行性边界  
**生效时间**：即刻

---

## 背景

ADR-0005 定义的是"运行时秩序"，但工具能力有物理极限：

- **NetArchTest** 只能做编译期静态分析
- **Roslyn Analyzer** 可以做语义级检查
- **人工审查** 是最后的防线

如果不明确区分这三个层次，会导致：

- 开发者误以为"测试通过 = 完全合规"
- 架构师不知道哪些规则需要人工 Gate
- 新人不理解为什么"测试过了还不允许"

因此，本文档将 ADR-0005 的所有规则进行三级分类。

---

## 执行级别定义

### Level 1: 静态可执行（Static Enforceable）

**定义**：可以通过 NetArchTest 等静态分析工具完全自动化检查的规则。

**特征**：

- 基于类型、命名空间、依赖关系的静态约束
- 测试失败 = 绝对违规
- CI 阻断

**工具**：ArchitectureTests（NetArchTest）

**规则列表**：

| 规则编号        | 规则描述                          | 测试方法                                                     |
|-------------|-------------------------------|----------------------------------------------------------|
| ADR-0005.1  | Handler 应有明确的命名约定             | `Handlers_Should_Have_Clear_Naming_Convention`           |
| ADR-0005.3  | Handler 不应依赖 ASP.NET 类型       | `Handlers_Should_Not_Depend_On_AspNet`                   |
| ADR-0005.4  | Handler 应该是无状态的（字段检查）         | `Handlers_Should_Be_Stateless`                           |
| ADR-0005.5  | 模块间不应有未审批的同步调用（依赖检查）          | `Modules_Should_Not_Have_Synchronous_Cross_Module_Calls` |
| ADR-0005.6  | 异步方法应遵循命名约定                   | `Async_Methods_Should_Follow_Naming_Convention`          |
| ADR-0005.7  | 模块不应共享领域实体                    | `Modules_Should_Not_Share_Domain_Entities`               |
| ADR-0005.9  | Command 和 Query Handler 应明确分离 | `Command_And_Query_Handlers_Should_Be_Separated`         |
| ADR-0005.12 | 所有 Handler 应在模块程序集中           | `All_Handlers_Should_Be_In_Module_Assemblies`            |

---

### Level 2: 语义半自动（Semantic Semi-Auto）

**定义**：需要语义分析的规则，建议通过 Roslyn Analyzer 检查，当前测试只能做启发式检查。

**特征**：

- 需要理解代码语义（如方法调用链、业务逻辑复杂度）
- 当前测试是"建议性"而非"强制性"
- 测试失败 = 需要人工审查

**工具**：

- 当前：ArchitectureTests（启发式检查）
- 建议：Roslyn Analyzer（自定义分析器）

**规则列表**：

| 规则编号        | 规则描述                     | 当前测试方法                                                   | Analyzer 需求                    |
|-------------|--------------------------|----------------------------------------------------------|--------------------------------|
| ADR-0005.2  | Endpoint 不应包含业务逻辑        | `Endpoints_Should_Not_Contain_Business_Logic`            | 检查方法体复杂度、业务规则调用                |
| ADR-0005.5  | 模块间异步通信（Handler 调用检查）    | `Modules_Should_Not_Have_Synchronous_Cross_Module_Calls` | 检查方法调用链、跨模块 public 方法调用        |
| ADR-0005.10 | Command Handler 不应返回业务数据 | `CommandHandlers_Should_Not_Return_Business_Data`        | 检查返回类型的语义（简单类型 vs 业务对象）        |
| ADR-0005.11 | Handler 应使用结构化异常         | `Handlers_Should_Use_Structured_Exceptions`              | IL 分析，检查 `throw new Exception` |

**Roslyn Analyzer 实现建议**：

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

---

### Level 3: 人工 Gate（Manual Gate）

**定义**：无法（或不应该）完全自动化的规则，需要人工审查和决策。

**特征**：

- 涉及业务语义、设计意图、架构权衡
- 测试无法覆盖，或覆盖成本过高
- 依赖 PR Review + ARCH-VIOLATION 标记

**工具**：

- PR Checklist
- 架构师 Code Review
- ARCH-VIOLATION 记录表

**规则列表**：

| 规则编号       | 规则描述                         | 人工审查要点                                  |
|------------|------------------------------|-----------------------------------------|
| ADR-0005.5 | 模块间同步调用破例审批                  | 是否有明确的远程调用契约？是否处理了超时/降级？                |
| ADR-0005.8 | Query Handler 可以返回 Contracts | 返回的 Contract 是否版本化？是否只读？                |
| 事务边界       | 跨模块事务与 Saga                  | 是否真的需要强一致性？Saga 补偿逻辑是否完整？               |
| 业务决策       | Handler 职责划分                 | 业务逻辑是否集中在一个 Handler？是否有隐藏的跨 Handler 协作？ |

**人工 Gate 流程**：

1. **PR 提交时**：开发者在 PR 模板中声明是否存在架构破例
2. **CI 检查**：如果 ArchitectureTests 失败但 PR 声明"无违规" → 自动拒绝
3. **架构审查**：如果 PR 声明"有破例" → 架构师审查并决定是否批准
4. **记录归档**：所有破例必须记录在 [`ARCH-VIOLATIONS.md`](../summaries/arch-violations.md) 中

---

## 三级分类的意义

### 对开发者

- 明确知道哪些规则是"红线"（Level 1 测试失败必须修复）
- 明确知道哪些规则是"建议"（Level 2 可以申请破例）
- 明确知道哪些规则需要"沟通"（Level 3 必须人工审批）

### 对架构师

- Level 1：零容忍，CI 自动阻断
- Level 2：可协商，但需要充分理由
- Level 3：预期破例，重点在于记录和可追溯

### 对工具链

- Level 1 → NetArchTest（已实现）
- Level 2 → Roslyn Analyzer（建议实现）
- Level 3 → PR Template + Review Process（必须实现）

---

## 当前状态与改进路径

### ✅ 已完成

- [x] Level 1 规则已通过 ArchitectureTests 覆盖
- [x] 测试失败消息明确标注违规规则
- [x] Level 2 的 Roslyn Analyzer 已实现
  - ✅ Endpoint 业务逻辑检查（ADR0005_02）
  - ✅ Handler 跨模块调用语义检查（ADR0005_05）
  - ✅ 异常使用规范检查（ADR0005_11）
- [x] Level 3 的人工 Gate 流程已建立
  - ✅ PR Template 包含架构违规声明
  - ✅ ARCH-VIOLATIONS 记录表存在
  - ✅ 破例审批流程已文档化

### ⚠️ 待完善（可选）

- [ ] 扩展更多 Roslyn Analyzer
  - Command Handler 返回值检查（更严格的类型判断）
  - 异步方法命名约定检查
  - Handler 无状态字段检查（更深层的分析）
- [ ] 定期评审和优化
  - 收集 Analyzer 误报案例，优化检测逻辑
  - 根据团队反馈调整严重级别
  - 定期更新架构约束

### 🎯 最终目标

**ADR-0005 不是一个"全自动检查工具"，而是一个"三层防御体系"**：

1. **自动化层**（Level 1）：拦截 90% 的明显违规
2. **辅助工具层**（Level 2）：提示潜在风险，辅助人工决策
3. **人工决策层**（Level 3）：处理复杂场景，记录权衡过程

---

## 结论

> **ADR-0005 的本质是"运行时秩序"，静态测试是"近似执行"，不是"完全执行"。**

通过三级分类：

- 我们承认工具的局限性
- 我们不依赖单一工具的"完美"
- 我们建立了人机协作的架构治理体系

这不是妥协，这是成熟。

---

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0005：应用内交互模型与执行边界](./ADR-0005-Application-Interaction-Model-Final.md) - 本文档是 ADR-0005 的执行级别补充
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 执行级别基于测试和 CI 治理机制

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- 无

---
