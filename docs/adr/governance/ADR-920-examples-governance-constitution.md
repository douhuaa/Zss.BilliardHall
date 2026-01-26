# ADR-920：示例代码治理宪法

> ⚖️ **本 ADR 是所有示例代码（Examples）的唯一裁决源，定义示例的边界、约束与执法机制。**

**状态**：✅ Final（裁决型ADR）  
**级别**：治理层 / 架构元规则  
**适用范围**：所有示例代码（examples/、docs/examples/、README 代码块、ADR 代码示例）  
**生效时间**：即刻

---

## 聚焦内容（Focus）

- 示例代码的定位与权限边界
- 示例代码禁止的架构违规行为
- 示例/测试/PoC 的明确区分
- 分级执法标准（L1/L2/L3）
- 示例作者责任制

---

## 术语表（Glossary）

| 术语         | 定义                                              | 英文对照                      |
|------------|------------------------------------------------|---------------------------|
| 示例代码       | 用于演示用法的代码片段，无架构裁决力                         | Example Code              |
| 免责声明       | 示例必须在开头声明无架构权威，仅用于说明用法                   | Disclaimer                |
| 架构违规       | 示例中违反 ADR 规则的代码                              | Architecture Violation    |
| 教学性简化      | 为便于理解而简化流程，但不违反架构规则                        | Educational Simplification|
| 规则简化       | 为便于演示而违反架构约束，**绝对禁止**                      | Rule Simplification (Forbidden) |
| 测试代码       | 用于验证功能的代码，可在受控条件下违规                        | Test Code                 |
| PoC/Spike  | 概念验证代码，允许违规，但**不可进入主干**                   | Proof of Concept / Spike  |

---

## 决策（Decision）

### 示例代码的法律地位（ADR-920.1）

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

### 示例代码必须包含的免责声明（ADR-920.2）

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

### 示例代码禁止的架构违规行为（ADR-920.3）

**规则**：

示例代码**禁止**包含以下"示例代码禁止的架构违规行为"：

#### 3.1 跨模块直接引用（ADR-0001）
```csharp
// ❌ 禁止
using Zss.BilliardHall.Modules.Members.Domain;

// ✅ 正确：使用事件或契约
await _eventBus.Publish(new OrderCreated(orderId));
var memberDto = await _queryBus.Send(new GetMemberById(memberId));
```

#### 3.2 违反 Handler 模式（ADR-0005）
```csharp
// ❌ 禁止：Command Handler 返回业务数据
public async Task<OrderDto> Handle(CreateOrder command)

// ✅ 正确：Command Handler 只返回 ID
public async Task<Guid> Handle(CreateOrder command)
```

#### 3.3 创建横向 Service 层（ADR-0001）
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

### 示例 vs 测试 vs PoC（ADR-920.4）

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

**Copilot 防污染规则**：
```markdown
# .github/copilot-instructions.md
## 忽略目录
以下目录的代码**不应**作为学习源：
- `.poc/` - 概念验证代码，允许违规
- `.experimental/` - 实验性代码，允许违规
```

**核心原则**：
> 示例 ≠ 测试 ≠ PoC，三者不可混淆。  
> 示例必须合规，PoC 允许违规但不可进主干。

**判定**：
- ❌ PoC 代码进入主干
- ❌ 示例目录包含违规代码
- ✅ PoC 在特殊目录且有防污染配置

---

### 示例代码的自动化执法（分级处理）（ADR-920.5）

**规则**：

| 级别 | 违规类型               | 执行方式  | 是否阻断 CI | 说明                  |
| -- | ------------------ | ----- | ------- | ------------------- |
| L1 | 结构违规（跨模块引用、Service） | 架构测试  | ✅ 阻断    | 根本性架构破坏，必须修复        |
| L2 | 模式偏差（Handler 返回类型）  | 架构测试  | ⚠️ 警告   | 架构模式不符，建议修复但不阻断     |
| L3 | 风格/教学简化（省略异常处理）    | Code Review | ❌ 忽略    | 允许教学简化，不强制与生产代码一致   |

**L1 违规（必须阻断）**：
- ❌ 跨模块直接引用（ADR-0001）
- ❌ 创建横向 Service 层（ADR-0001）
- ❌ Platform 依赖业务层（ADR-0002）
- ❌ 共享领域模型（ADR-0001）

**L2 违规（警告，不阻断）**：
- ⚠️ Command Handler 返回类型不正确（ADR-0005）
- ⚠️ 命名不符合约定（ADR-0003）

**L3 简化（允许）**：
- ✅ 省略异常处理
- ✅ 省略详细日志
- ✅ 省略性能优化

**核心原则**：
> 示例允许简化流程，但不允许简化规则。  
> 同规则、不同严重级别：结构违规 = 阻断，模式偏差 = 警告，教学简化 = 允许。

**判定**：
- ❌ L1 违规未修复
- ⚠️ L2 违规未说明
- ✅ L3 简化在允许范围内

---

### 示例作者责任制（ADR-920.6）

**规则**：

每个示例目录**必须**有明确的责任人和目的说明：

```markdown
# 示例名称

⚠️ **示例免责声明**  
本示例代码仅用于说明用法，不代表架构最佳实践或完整实现。

**维护信息**：
- **作者**：@username
- **目的**：教学 / 演示 / Onboarding
- **创建日期**：YYYY-MM-DD
- **适用 ADR**：ADR-0001, ADR-0005
```

**必填字段**：
- ✅ `Author` - 责任人（GitHub 用户名）
- ✅ `Purpose` - 目的（教学/演示/Onboarding）
- ✅ `Created` - 创建日期
- ✅ `ADRs` - 适用的 ADR 列表

**核心原则**：
> 没有责任人 = 没人维护 = 示例腐化。

**判定**：
- ❌ 缺失必填字段
- ❌ 示例目录无 README
- ✅ 完整的维护信息

---

## 快速参考表

| 约束编号       | 约束描述                | 测试方式             | 必须遵守 |
|------------|---------------------|------------------|------|
| ADR-920.1  | 示例代码无架构裁决力，必须遵守 ADR | L3 - Code Review | ✅    |
| ADR-920.2  | 示例必须包含免责声明          | L2 - CI 脚本检查 | ✅    |
| ADR-920.3  | 示例禁止 L1 结构违规        | L1 - 架构测试  | ✅    |
| ADR-920.4  | 示例/测试/PoC 必须明确区分    | L2 - CI 检查  | ✅    |
| ADR-920.5  | 示例执行分级测试（L1/L2/L3）  | L1/L2 - 架构测试 | ✅    |
| ADR-920.6  | 示例目录必须有责任人和目的说明     | L2 - CI 检查  | ✅    |

---

## 必测/必拦架构测试（Enforcement）

所有规则通过以下方式强制验证：

- **架构测试**：`src/tests/ArchitectureTests/ADR/ADR_0920_Architecture_Tests.cs`
  - `Examples_Should_Not_Reference_Other_Modules` - L1 阻断
  - `Example_Documents_Must_Have_Disclaimer` - L1 阻断
  - `README_CSharp_Code_Examples_Should_Not_Violate_Architecture` - L2 警告
  - `Example_Directories_Must_Have_Owner_And_Purpose` - L1 阻断

- **CI 脚本**：扫描示例目录和文档
- **Code Review**：检查示例是否引入未经 ADR 允许的模式

**有一项 L1 违规视为架构违规，CI 自动阻断。**

---

## 破例与归还（Exception）

### 允许破例的前提

示例代码规则的破例**仅在以下情况允许**：
- 临时性迁移示例（标注 `[DRAFT]` 或 `[迁移中]`）
- 历史遗留示例的过渡期（不超过 1 个月）
- 教学用的"反面教材"（必须明确标注 `// ❌ 错误示例`）

### 破例要求

每个破例**必须**：
- 记录在 `ARCH-VIOLATIONS.md` 的"示例治理破例"章节
- 指明破例的具体示例文件和原因
- 指定失效日期（不超过 1 个月）
- 给出归还计划

---

## 变更政策（Change Policy）

- **ADR-920**（治理层）
  - 修改需 Tech Lead 审批
  - 需要全量示例回归检查
  - 需要更新对应的架构测试

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：
- 示例代码的写作风格和美学
- 示例代码的注释详细程度
- 示例代码的性能优化
- 示例代码的完整性

---

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](./ADR-0000-architecture-tests.md) - 示例治理基于测试和 CI 机制
- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) - 示例必须遵守模块隔离规则
- [ADR-0002：平台、应用与主机启动器架构](../constitutional/ADR-0002-platform-application-host-bootstrap.md) - 示例必须遵守层级依赖规则
- [ADR-0005：应用内交互模型与执行边界](../constitutional/ADR-0005-Application-Interaction-Model-Final.md) - 示例必须遵守 Handler 模式
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 示例治理是文档治理的一部分

**被依赖（Depended By）**：
- [ADR-951：案例库管理](./ADR-951-case-repository-management.md) - 案例管理参考示例治理规则

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-910：README 编写与维护](./ADR-910-readme-governance-constitution.md) - 示例可出现在 README 中

---

## 版本历史

| 版本  | 日期         | 变更说明       |
|-----|------------|------------|
| 2.0 | 2026-01-26 | 裁决型重构，移除冗余 |
| 1.0 | 2026-01-25 | 初版，定义示例代码治理规则 |

---

## 附注

本文件禁止添加示例/建议/FAQ/背景说明，仅维护自动化可判定的架构红线。

非裁决性参考（详细示例、场景说明）请查阅：
- [ADR-0920 Copilot Prompts](../../copilot/adr-0920.prompts.md)

