---
adr: ADR-920
title: "示例代码治理规范"
status: Final
level: Governance
version: "2.1"
deciders: "Architecture Board"
date: 2026-02-03
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---

# ADR-920：示例代码治理规范

> ⚖️ **Constraint | L1** - 本 ADR 是所有示例代码（Examples）的治理规范，定义示例的边界、约束与执法机制。

## Focus（聚焦内容）

- 示例代码必须与实际模块、API 保持一致
- 不允许示例代码引入未批准的架构模式或依赖
- 需要包含执行和测试约束
- 违规处理与 Agent 执行映射

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|-----------|
| 示例代码 | README 或文档中用于说明功能的代码片段 | Example Code |
| 可执行性 | 示例代码可以被正确编译或运行 | Executable |
| 偏离 | 示例代码与真实 API/模块行为不符 | Deviation |
| 标记 | ✅/❌ 或注释说明正确性 | Marking |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-920 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-920_<Rule>_<Clause>
> ```

---

### ADR-920_1：示例代码权限边界（Rule）

#### ADR-920_1_1 示例代码的法律地位

**规则**：

示例代码**不具备**以下权力：
- ❌ 定义架构规则
- ❌ 引入 ADR 中未允许的结构或模式
- ❌ 作为架构正确性的证据
- ❌ 覆盖或替代 ADR 的裁决

示例代码**仅允许**：
- ✅ 演示如何使用已有的架构模式
- ✅ 说明具体的 API 调用方式
- ✅ 提供快速上手的代码片段
- ✅ 展示正确/错误的对比（需明确标记）

**核心原则**：
> 示例代码是"认知放大器"，不是"架构定义器"。  
> 示例必须遵守 ADR，而非定义 ADR。

**核心原则**：
> 示例不是规范，只是演示如何使用已有的架构模式。

**判定**：
- ❌ 示例引入 ADR 未允许的 Service 层
- ❌ 示例展示跨模块直接引用
- ✅ 示例演示符合 ADR 的事件通信

---

#### ADR-920_1_2 示例代码必须包含的免责声明

**规则**：

所有示例代码或示例文档**必须**包含"示例代码必须包含的免责声明"：

**文档中的示例**：
```markdown
⚠️ **示例免责声明**  
本示例代码仅用于说明用法，不代表架构最佳实践或完整实现。  
具体架构约束以对应 ADR 正文为准。
```

**代码文件中的示例**：
```csharp
/// <summary>
/// ⚠️ 示例代码：仅用于演示用法，不代表完整实现或架构最佳实践。
/// 具体约束请参考对应 ADR 文档。
/// </summary>
```

**位置要求**：
- 必须在示例开头
- 必须在代码实现之前
- 使用 ⚠️ 警告符号

**例外**：
- 单元测试中的测试数据构建代码
- 架构测试中的违规示例

**ADR 正文中的代码片段**必须显式标注角色：
```csharp
// ✅ 示例（教学）- 演示正确做法
// ❌ 反例（禁止）- 展示错误做法
// 📐 结构示意（不可复制）- 仅说明概念
```

**判定**：
- ❌ 缺失免责声明
- ❌ ADR 代码片段未标注角色
- ✅ 标准免责声明格式

---

### ADR-920_2：示例代码架构约束（Rule）

#### ADR-920_2_1 示例代码禁止的架构违规行为

**规则**：

示例代码**禁止**包含以下"示例代码禁止的架构违规行为"：

##### ADR-920_2_1_a 跨模块直接引用（ADR-####）
```csharp
// ❌ 禁止
using Zss.BilliardHall.Modules.Members.Domain;

// ✅ 正确：使用事件或契约
await _eventBus.Publish(new OrderCreated(orderId));
var memberDto = await _queryBus.Send(new GetMemberById(memberId));
```

##### ADR-920_2_1_b 违反 Handler 模式（ADR-####）
```csharp
// ❌ 禁止：Command Handler 返回业务数据
public async Task<OrderDto> Handle(CreateOrder command)

// ✅ 正确：Command Handler 只返回 ID
public async Task<Guid> Handle(CreateOrder command)
```

##### ADR-920_2_1_c 创建横向 Service 层（ADR-####）
```csharp
// ❌ 禁止
public class OrderService { }

// ✅ 正确：独立 Handler
public class CreateOrderHandler { }
```

**教学替代命名规范**（允许）：
- ✅ `*UseCase` - 表示用例场景
- ✅ `*Scenario` - 表示业务场景
- ✅ `*Flow` - 表示流程编排

**禁止**：
- ❌ `*Service`
- ❌ `*Manager`
- ❌ `*Helper` / `*Utility`（业务逻辑相关）

**核心原则**：
> 示例允许简化流程，但不允许简化规则。  
> 示例允许简化流程（如省略异常处理），但不允许简化规则（如跨模块引用）。

**判定**：
- ❌ 示例包含 L1 结构违规
- ⚠️ 示例包含 L2 模式偏差
- ✅ 示例仅做 L3 教学简化

---

### ADR-920_3：示例类型边界管理（Rule）

#### ADR-920_3_1 示例 vs 测试 vs PoC

**规则**：

| 类型          | 是否允许违规 | 是否可进主干 | 存放位置                 | 声明要求          |
| ----------- | ------ | ------ | -------------------- | ------------- |
| 示例 Example  | ❌      | ✅      | `examples/`、`docs/examples/` | 必须包含免责声明      |
| 测试 Test     | ⚠️（受控） | ✅      | `tests/`、`*Tests.cs`     | 无需声明（测试上下文明确） |
| PoC / Spike | ✅      | ❌      | `.poc/`、`.experimental/` | 必须标注 `[PoC]`  |

**PoC / Spike**：
- 允许违规，快速试错
- **不可进入主干**
- 目录**必须**使用 `.poc/` 或 `.experimental/`
- README 必须标注 `⚠️ NOT FOR REUSE - PoC Only`
- Copilot 必须在配置中忽略此目录

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-920 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-920_1_1** | L2 | 人工审查：示例是否引入未经 ADR 允许的模式 | §ADR-920_1_1 |
| **ADR-920_1_2** | L1 | ArchitectureTests 验证示例文档/代码包含免责声明 | §ADR-920_1_2 |
| **ADR-920_2_1** | L1 | ArchitectureTests 验证示例不包含架构违规 | §ADR-920_2_1 |
| **ADR-920_3_1** | L1 | ArchitectureTests 验证示例、测试、PoC 边界与存放位置 | §ADR-920_3_1 |

### 执行级别说明

- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决

---

## 测试实现参考

所有规则通过以下方式强制验证：

- **架构测试**：`src/tests/ArchitectureTests/ADR/ADR_920_Architecture_Tests.cs`
  - `ADR_920_1_2_Example_Documents_Must_Have_Disclaimer` - L1 阻断
  - `ADR_920_2_1_Examples_Should_Not_Reference_Other_Modules` - L1 阻断
  - `ADR_920_2_1_README_CSharp_Code_Examples_Should_Not_Violate_Architecture` - L2 警告
  - `ADR_920_3_1_Example_Directories_Must_Have_Owner_And_Purpose` - L1 阻断

- **CI 脚本**：扫描示例目录和文档
- **Code Review**：检查示例是否引入未经 ADR 允许的模式

**有一项 L1 违规视为架构违规，CI 自动阻断。**

---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **示例代码的编程风格**：不规定示例代码的缩进、命名等代码风格细节
- **示例代码的测试覆盖率**：不要求示例代码必须有单元测试或达到特定覆盖率
- **示例代码的性能优化**：不要求示例必须是最优化或最高效的实现
- **示例代码的完整性**：不要求示例必须是可独立运行的完整应用（允许代码片段）
- **示例代码的版本维护**：不建立示例代码随主代码库同步更新的强制机制
- **示例文档的格式和排版**：不规定示例文档的视觉呈现和排版细节
- **示例代码的语言选择**：不限制示例只能使用特定编程语言（只要不违反架构）
- **示例代码的复杂度**：不规定示例的复杂度级别（入门/中级/高级）

---

## Prohibited（禁止行为）


以下行为明确禁止：

### 示例质量违规
- ❌ **禁止示例代码违反架构约束**：示例必须完全遵守所有 ADR 定义的架构规则
- ❌ **禁止示例代码缺少免责声明**：所有示例必须明确标注"仅供参考，不保证生产可用"
- ❌ **禁止示例代码引用其他模块**：示例代码必须自包含，不得依赖其他业务模块

### 示例边界违规
- ❌ **禁止将实际业务代码标记为示例**：示例只能是教学用途，不能是生产代码的别名
- ❌ **禁止示例目录包含生产依赖**：示例不得被生产代码引用或依赖
- ❌ **禁止示例代码进入发布包**：构建和发布流程必须排除示例目录

### 示例维护违规
- ❌ **禁止示例代码过时未更新**：与当前架构规则冲突的示例必须更新或删除
- ❌ **禁止示例缺少所有者信息**：每个示例必须标明创建者和维护联系方式
- ❌ **禁止 Copilot 从示例学习不良模式**：必须配置 Copilot 忽略示例目录以防污染

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-900：架构测试与 CI 治理元规则](./ADR-900-architecture-tests.md) - 示例治理基于测试和 CI 机制
- [ADR-001：模块化单体与垂直切片架构](../constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md) - 示例必须遵守模块隔离规则
- [ADR-002：平台、应用与主机启动器架构](../constitutional/ADR-002-platform-application-host-bootstrap.md) - 示例必须遵守层级依赖规则
- [ADR-005：应用内交互模型与执行边界](../constitutional/ADR-005-Application-Interaction-Model-Final.md) - 示例必须遵守 Handler 模式
- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md) - 示例治理是文档治理的一部分

**被依赖（Depended By）**：
- [ADR-951：案例库管理](./ADR-951-case-repository-management.md) - 案例管理参考示例治理规则

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-910：README 编写与维护](./ADR-910-readme-governance-constitution.md) - 示例可出现在 README 中

---

## References（非裁决性参考）

**相关外部资源**：
- [Example Code Best Practices](https://google.github.io/styleguide/docguide/best_practices.html) - Google 文档风格指南中的示例代码部分
- [Microsoft Docs Contributor Guide](https://learn.microsoft.com/en-us/contribute/code-in-docs) - 微软文档中代码示例的编写规范
- [The Twelve-Factor App](https://12factor.net/) - 示例应用的架构原则参考

**相关内部文档**：
- [ADR-001：模块化单体与垂直切片架构](../constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md) - 示例必须遵守的核心架构
- [ADR-900：架构测试与 CI 治理元规则](./ADR-900-architecture-tests.md) - 示例的测试和验证机制
- [ADR-910：README 编写与维护治理规范](./ADR-910-readme-governance-constitution.md) - README 中的示例规范
- [ADR-950：Guide/FAQ 文档治理](./ADR-950-guide-faq-documentation-governance.md) - Guide 文档中的示例规范

---

## History（版本历史）

| 版本  | 日期         | 变更说明   | 修订人 |
|-----|------------|--------|-------|
| 2.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
| 1.0 | 2026-01-29 | 初始版本 | Architecture Board |
