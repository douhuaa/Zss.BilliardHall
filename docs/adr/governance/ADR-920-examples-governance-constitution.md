# ADR-920：示例代码治理宪法

> ⚖️ **本 ADR 是所有示例代码（Examples）的唯一裁决源，定义示例的边界、约束与执法机制。**

**状态**：✅ Final（治理层，不可随意修改）  
**级别**：治理层 / 架构元规则  
**适用范围**：所有示例代码（examples/、docs/examples/、README 代码块、ADR 代码示例）  
**生效时间**：即刻  
**版本**：1.0  
**修订日期**：2026-01-25

---

## 聚焦内容（Focus）

- 示例代码的定位与权限边界
- 示例代码禁止的架构违规行为
- 示例代码必须包含的免责声明
- 示例/测试/PoC 的明确区分
- 示例代码的自动化检测与执法

---

## 为什么需要 ADR-920（Why This ADR Matters）

### 问题陈述

在 ADR-920 之前：

- ❌ 示例代码天然被当成"最佳实践"
- ❌ 示例里的违规不会被当成违规
- ❌ 示例代码绕过了架构测试
- ❌ Copilot 优先学习示例，而不是 ADR
- ❌ 示例一旦错误，传播速度是 ADR 的 10 倍
- ❌ 示例代码成为"事实上的法律"，夺取 ADR 的权威

### ADR-920 的定位

ADR-920 是：

- 📌 **示例代码违规的专门执法机制**
- 📌 **防止示例代码成为隐性规则源的专项约束**
- 📌 **确保示例代码遵守架构约束的根本保障**
- 📌 **所有示例编写的元规则（Meta-Rule）**

一句话定位：

> **示例不是规范，只是演示。示例允许简化流程，但不允许简化规则。**

---

## 术语表（Glossary）

| 术语         | 定义                                              | 英文对照                      |
|------------|------------------------------------------------|---------------------------|
| 示例代码       | 用于演示用法的代码片段，无架构裁决力                         | Example Code              |
| 免责声明       | 示例必须在开头声明无架构权威，仅用于说明用法                   | Disclaimer                |
| 架构违规       | 示例中违反 ADR 规则的代码（如跨模块直接引用）                 | Architecture Violation    |
| 教学性简化      | 为便于理解而简化流程，但不违反架构规则                        | Educational Simplification|
| 规则简化       | 为便于演示而违反架构约束，**绝对禁止**                      | Rule Simplification (Forbidden) |
| 示例越界       | 示例试图定义架构规则或引入 ADR 未允许的结构                  | Example Boundary Violation |
| 测试代码       | 用于验证功能的代码，可在受控条件下违规（如测试替身）               | Test Code                 |
| PoC/Spike  | 概念验证/探索性代码，允许违规，但**不可进入主干**              | Proof of Concept / Spike  |

---

## 核心决策（Decision）

### 决策 1：示例代码的法律地位（唯一裁决）

**规则本体**：

示例代码**不具备**以下权力：

- ❌ 定义架构规则
- ❌ 引入 ADR 中未允许的结构或模式
- ❌ 作为架构正确性的证据
- ❌ 覆盖或替代 ADR 的裁决

示例代码**只允许**表达以下内容：

- ✅ 演示如何使用已有的架构模式
- ✅ 说明具体的 API 调用方式
- ✅ 提供快速上手的代码片段
- ✅ 展示正确/错误的对比（需明确标记）

**核心原则**：

> **示例代码是"认知放大器"，不是"架构定义器"。**  
> **示例必须遵守 ADR，而非定义 ADR。**

---

### 决策 2：示例代码必须包含的免责声明

**规则本体**：

所有示例代码或示例文档**必须**在显著位置包含"示例免责声明"：

**强制格式**：

对于文档中的示例：

```markdown
⚠️ **示例免责声明**  
本示例代码仅用于说明用法，不代表架构最佳实践或完整实现。  
具体架构约束以对应 ADR 正文为准。
```

对于代码文件中的示例：

```csharp
/// <summary>
/// ⚠️ 示例代码：仅用于演示用法，不代表完整实现或架构最佳实践。
/// 具体约束请参考对应 ADR 文档。
/// </summary>
```

**位置要求**：

- ✅ **必须**在示例开头，代码/文档的第一段
- ✅ **必须**在任何代码实现之前
- ✅ **必须**显著可见（使用 ⚠️ 警告符号）

**例外**：

以下示例可以豁免此声明：

- ✅ 单元测试中的测试数据构建代码（有测试上下文）
- ✅ 架构测试中的违规示例（有测试上下文）

**特殊规则：ADR 正文中的代码片段**：

ADR 正文中的代码片段允许违规（用于教学），但**必须显式标注角色**：

```csharp
// ✅ 示例（教学）- 演示正确做法
public class CorrectExample { }

// ❌ 反例（禁止）- 展示错误做法
public class WrongExample { }

// 📐 结构示意（不可复制）- 仅说明概念
public abstract class ConceptualStructure { }
```

**未标注的代码片段 = Copilot 会误学习 = 必须修复。**

**检测规则**：

- ✅ CI 自动扫描所有示例目录和示例文档
- ✅ 检查是否包含"示例免责声明"或"示例代码"或"仅用于演示"
- ✅ 缺失声明 = CI 阻断

---

### 决策 3：示例代码禁止的架构违规行为

**规则本体**：

示例代码**禁止**包含以下架构违规：

#### 3.1 跨模块直接引用（ADR-0001）

```csharp
// ❌ 禁止：示例中直接引用其他模块
using Zss.BilliardHall.Modules.Members.Domain;

public class OrderExampleHandler
{
    private readonly MemberRepository _repo; // 跨模块引用
}
```

**正确示例**：

```csharp
// ✅ 正确：示例使用事件或契约
public class OrderExampleHandler
{
    public async Task Handle(CreateOrder command)
    {
        // 通过事件通知其他模块
        await _eventBus.Publish(new OrderCreated(orderId));
        
        // 或通过契约查询（只读）
        var memberDto = await _queryBus.Send(new GetMemberById(memberId));
    }
}
```

#### 3.2 违反 Handler 模式（ADR-0005）

```csharp
// ❌ 禁止：示例中 Command Handler 返回业务数据
public async Task<OrderDto> Handle(CreateOrder command)
{
    // ...
    return orderDto; // 违反 ADR-0005
}
```

**正确示例**：

```csharp
// ✅ 正确：Command Handler 只返回 ID
public async Task<Guid> Handle(CreateOrder command)
{
    // ...
    return order.Id;
}
```

#### 3.3 创建横向 Service 层（ADR-0001）

```csharp
// ❌ 禁止：示例中引入 Service 抽象
public class OrderService
{
    public void CreateOrder() { }
    public void CancelOrder() { }
}
```

**正确示例**：

```csharp
// ✅ 正确：每个用例独立的 Handler
public class CreateOrderHandler { }
public class CancelOrderHandler { }
```

**教学替代命名规范**：

如示例中需要承载"流程聚合"概念（教学目的），**必须**使用以下命名之一：

- ✅ `*UseCase` - 表示用例场景
- ✅ `*Scenario` - 表示业务场景
- ✅ `*Flow` - 表示流程编排

```csharp
// ✅ 允许：教学用的流程类（明确非 Service）
public class OrderCreationUseCase { }    // 用例
public class CheckoutScenario { }        // 场景
public class PaymentFlow { }             // 流程
```

**禁止**：

- ❌ `*Service`
- ❌ `*Manager`
- ❌ `*Helper` / `*Utility`（业务逻辑相关）

> **治理不是只靠禁令，还要给认知过渡物（安全出口）。**

#### 3.4 Platform 依赖业务层（ADR-0002）

```csharp
// ❌ 禁止：示例中 Platform 引用 Application/Modules
namespace Zss.BilliardHall.Platform.Logging
{
    using Zss.BilliardHall.Modules.Orders; // 违规
}
```

#### 3.5 共享领域模型（ADR-0001）

```csharp
// ❌ 禁止：示例中多个模块共享领域对象
public class SharedCustomer { } // 被多个模块使用
```

**正确示例**：

```csharp
// ✅ 正确：每个模块有自己的领域模型
namespace Orders.Domain { public class Customer { } }
namespace Members.Domain { public class Customer { } }
```

**核心原则**：

> **示例允许简化流程（如省略异常处理），但不允许简化规则（如跨模块引用）。**

---

### 决策 4：示例 vs 测试 vs PoC（必须立清）

**规则本体**：

明确区分三种代码形态的权限和约束：

| 类型          | 是否允许违规 | 是否可进主干 | 存放位置                 | 声明要求          |
| ----------- | ------ | ------ | -------------------- | ------------- |
| 示例 Example  | ❌      | ✅      | `examples/`、`docs/examples/` | 必须包含免责声明      |
| 测试 Test     | ⚠️（受控） | ✅      | `tests/`、`*Tests.cs`     | 无需声明（测试上下文明确） |
| PoC / Spike | ✅      | ❌      | 个人分支、临时仓库            | 必须标注 `[PoC]`  |

**示例（Example）**：

- 用途：演示如何使用架构模式
- 约束：**必须**遵守所有 ADR 规则
- 位置：`examples/`、`docs/examples/`、README 代码块
- 声明：必须包含"示例免责声明"
- 检测：受架构测试约束

**测试（Test）**：

- 用途：验证功能正确性
- 约束：可在受控条件下违规（如 Mock、Stub）
- 位置：`tests/`、`*Tests.cs`
- 声明：无需（测试上下文自明）
- 检测：测试代码可豁免部分架构规则（如依赖注入替身）

**PoC / Spike**：

- 用途：探索可行性、验证概念
- 约束：允许违规，快速试错
- 位置：**不可进入主干**，只存在于个人分支或临时仓库
- 目录：**必须**使用 `.poc/` 或 `.experimental/` 特殊目录
- 声明：必须在 README 中标注 `⚠️ NOT FOR REUSE - PoC Only`
- Copilot：必须在 `.github/copilot-instructions.md` 中声明忽略此目录
- 检测：不进主干，无需架构测试

**Copilot 防污染规则**：

```markdown
# .github/copilot-instructions.md

## 忽略目录

以下目录的代码**不应**作为学习源：
- `.poc/` - 概念验证代码，允许违规
- `.experimental/` - 实验性代码，允许违规
- `**/spike/` - 探索性代码，允许违规
```

**现实情况**：
- Copilot 会扫描 Fork、PR、历史
- 仅靠"不进主干"无法防止 Copilot 学习
- **必须**使用特殊目录 + Copilot 忽略规则

**核心原则**：

> **示例 ≠ 测试 ≠ PoC，三者不可混淆。**  
> **示例必须合规，PoC 允许违规但不可进主干。**

---

### 决策 5：示例代码的自动化执法（分级处理）

**规则本体**：

示例代码执行**同规则、不同严重级别**的架构测试：

> **核心原则**：威慑违规，但不制造寒蝉效应。

**分级执法标准**：

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
- ⚠️ 缺少契约版本化（ADR-0210）

**L3 简化（允许）**：

- ✅ 省略异常处理
- ✅ 省略详细日志
- ✅ 省略性能优化
- ✅ 省略完整验证逻辑

**核心原则（防寒蝉效应）**：

> **示例允许简化流程，但不允许简化规则。**  
> **结构违规 = 阻断，模式偏差 = 警告，教学简化 = 允许。**

**技术声明（检测局限性）**：

> ⚠️ **当前实现为启发式检测（Heuristic Detection）**
> 
> 当前架构违规检测基于正则表达式的静态文本匹配，存在以下已知局限：
> - 可能无法检测复杂的多行 using 语句
> - 可能无法正确处理 global using、file-scoped namespace 等 C# 新语法
> - 允许**极小概率的误判或漏判**（trade-off：性能 vs 精确度）
> 
> **未来可升级方向**：
> - 升级为 Roslyn Analyzer（语义级检测）
> - 集成到 IDE 实时检测
> - 提供更精确的 AST 分析
> 
> **重要**：启发式检测的局限性**不影响 ADR 规则的权威性**。
> 测试未检出的违规仍是违规，需在 Code Review 中捕获。

**执法机制**：

#### 5.1 架构测试覆盖（分级）

示例代码必须通过以下架构测试：

- ✅ `ADR_920_Architecture_Tests` - 示例专用测试套件（L1 + L2）
- ✅ 现有的模块边界测试（ADR-0001）- L1 阻断
- ✅ 现有的层级依赖测试（ADR-0002）- L1 阻断
- ⚠️ 现有的 Handler 模式测试（ADR-0005）- L2 警告

**实施规则**：

```csharp
// ADR_920_Architecture_Tests.cs

[Fact(DisplayName = "ADR-920.1: 示例代码不得跨模块直接引用（L1 阻断）")]
public void Examples_Should_Not_Reference_Other_Modules()
{
    // 扫描 examples/ 目录下的所有 .cs 文件
    // 检测是否违反 ADR-0001 的模块隔离规则
    // L1 违规 = CI 失败
}

[Fact(DisplayName = "ADR-920.2: 示例文档必须包含免责声明（L1 阻断）")]
public void Example_Documents_Must_Have_Disclaimer()
{
    // 扫描 examples/、docs/examples/ 下的所有 .md 文件
    // 检查是否包含"示例免责声明"
    // 缺失声明 = CI 失败
}

[Fact(DisplayName = "ADR-920.3: README C# 代码块不得引入架构违规（L2 警告）")]
public void README_CSharp_Code_Examples_Should_Not_Violate_Architecture()
{
    // 提取 README 中的 C# 代码块（```csharp）
    // 非 C# 代码块（bash/pseudo）不检测，避免误报
    // 静态分析是否包含 L1 违规模式
    // L2 违规 = 警告（不阻断）
}

[Fact(DisplayName = "ADR-920.4: 示例目录必须有责任人和目的说明（L1 阻断）")]
public void Example_Directories_Must_Have_Owner_And_Purpose()
{
    // 扫描 examples/ 下的子目录
    // 检查是否包含 README.md 且有 Author 和 Purpose 字段
    // 缺失 = CI 失败
}
```

#### 5.2 CI 阻断规则

- ✅ 示例代码违规 = CI 失败
- ✅ 缺失免责声明 = CI 失败
- ✅ 示例引入未允许的结构 = CI 失败

#### 5.3 豁免机制

以下情况可豁免架构测试：

- ✅ ADR 正文中的短代码片段（有文档上下文）
- ✅ 单元测试中的测试数据（有测试上下文）
- ✅ 错误示例（已明确标注 `// ❌ 错误`）

**豁免记录**：

豁免的示例必须记录在测试代码中：

```csharp
// 豁免示例：ADR 正文中的教学片段
var exemptPaths = new[]
{
    "docs/adr/constitutional/ADR-0001-*.md", // ADR 正文可包含违规示例
};
```

**核心原则**：

> **示例代码的执法标准 = 同规则、不同严重级别。**  
> **L1 结构违规 = 阻断，L2 模式偏差 = 警告，L3 教学简化 = 允许。**

---

### 决策 6：示例作者责任制

**规则本体**：

每个示例目录**必须**有明确的责任人和目的说明。

**强制格式**：

```markdown
# 示例名称

⚠️ **示例免责声明**  
本示例代码仅用于说明用法，不代表架构最佳实践或完整实现。

**维护信息**：
- **作者**：@username
- **目的**：教学 / 演示 / Onboarding
- **创建日期**：YYYY-MM-DD
- **适用 ADR**：ADR-0001, ADR-0005
- **更新日期**：YYYY-MM-DD

## 内容
...
```

**位置要求**：

```
examples/
  ├─ OrderCreation/
  │  ├─ README.md        ← 必须包含维护信息
  │  ├─ Example.cs
  │  └─ ...
  └─ ModuleCommunication/
     ├─ README.md        ← 必须包含维护信息
     └─ ...
```

**必填字段**：

- ✅ `Author` - 责任人（GitHub 用户名）
- ✅ `Purpose` - 目的（教学/演示/Onboarding）
- ✅ `Created` - 创建日期
- ✅ `ADRs` - 适用的 ADR 列表

**选填字段**：

- ⏭️ `Updated` - 最后更新日期
- ⏭️ `Maintainer` - 当前维护者（如果与作者不同）

**核心原则**：

> **没有责任人 = 没人维护 = 示例腐化。**  
> **这不是官僚，是治理定位锚点。**

**检测规则**：

- ✅ CI 自动扫描 `examples/` 下的所有子目录
- ✅ 检查是否包含 `README.md`
- ✅ 检查 README 是否包含必填字段（Author、Purpose、Created、ADRs）
- ✅ 缺失任一字段 = CI 阻断

---

## 执法模型（Enforcement）

### 测试映射

| 规则编号        | 执行级 | 测试 / 手段                                        |
|-------------|-----|------------------------------------------------|
| ADR-920.1  | L1  | ArchitectureTests: Examples_Should_Not_Reference_Other_Modules |
| ADR-920.2  | L1  | ArchitectureTests: Example_Documents_Must_Have_Disclaimer |
| ADR-920.3  | L2  | ArchitectureTests: README_CSharp_Code_Examples_Should_Not_Violate_Architecture |
| ADR-920.4  | L1  | 复用现有模块边界测试（ADR-0001）扫描示例目录                        |
| ADR-920.5  | L1  | ArchitectureTests: Example_Directories_Must_Have_Owner_And_Purpose |
| ADR-920.6  | L3  | Code Review 检查示例是否引入未经 ADR 允许的模式                |

### 自动化检查工具

**已实现**：

```csharp
// ADR_920_Architecture_Tests.cs

[Fact(DisplayName = "ADR-920.1: 示例代码不得跨模块直接引用（L1 阻断）")]
public void Examples_Should_Not_Reference_Other_Modules()

[Fact(DisplayName = "ADR-920.2: 示例文档必须包含免责声明（L1 阻断）")]
public void Example_Documents_Must_Have_Disclaimer()

[Fact(DisplayName = "ADR-920.3: README C# 代码块不得引入架构违规（L2 警告）")]
public void README_CSharp_Code_Examples_Should_Not_Violate_Architecture()

[Fact(DisplayName = "ADR-920.4: 示例目录必须有责任人和目的说明（L1 阻断）")]
public void Example_Directories_Must_Have_Owner_And_Purpose()
```

**检查逻辑**：

1. **扫描示例文件**
   - `examples/` 下的所有 `.cs` 文件
   - `docs/examples/` 下的所有 `.md` 文件
   - README.md 中的代码块

2. **检测架构违规**
   - 跨模块引用（`using Zss.BilliardHall.Modules.*`）
   - Service 类创建（`*Service.cs`）
   - Platform 依赖业务层
   - 共享领域模型

3. **检测免责声明**
   - 检查文档开头是否包含"示例免责声明"
   - 检查代码文件是否包含 `<summary>` 中的免责说明

4. **检测示例目录责任人**
   - 扫描 `examples/` 下的所有子目录
   - 检查是否包含 README.md
   - 检查 README 是否包含必填字段（Author、Purpose、Created、ADRs）

5. **报告违规**
   - 列出违规文件和行号
   - 给出修复建议和 ADR 引用
   - CI 阻断构建

---

## 破例与归还（Exception）

### 允许破例的前提

示例代码规则的破例**仅在以下情况允许**：

- 临时性迁移示例（需在标题标注 `[DRAFT]` 或 `[迁移中]`）
- 历史遗留示例的过渡期（不超过 1 个月）
- 第三方生成的示例（如框架模板，需单独目录隔离）
- 教学用的"反面教材"（必须明确标注 `// ❌ 错误示例`）

### 破例要求

每个破例**必须**：

- 记录在 `ARCH-VIOLATIONS.md` 的"示例治理破例"章节
- 指明破例的具体示例文件和原因
- 指定失效日期（不超过 1 个月）
- 给出归还计划

**未记录的破例 = 未授权违规。**

---

## 变更政策（Change Policy）

### 变更规则

- **ADR-920**（治理层）
  - 修改需 Tech Lead 审批
  - 需要全量示例回归检查
  - 需要更新对应的架构测试

- **示例代码**
  - 可自由修改
  - 但必须保持免责声明
  - 必须通过架构测试

### 失效与替代

- Superseded 示例规则**必须**在新规则中明确指出
- 不允许"隐式废弃"

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：

- 示例代码的写作风格和美学
- 示例代码的注释详细程度
- 示例代码的性能优化
- 示例代码的完整性（可以是部分实现）
- 示例代码的运行环境配置

这些由项目团队自行决定。

---

## 非裁决性参考（References）

- ADR-0001：模块化单体与垂直切片架构（示例必须遵守模块隔离）
- ADR-0002：平台、应用与主机启动器架构（示例必须遵守层级依赖）
- ADR-0005：应用内交互模型与执行边界（示例必须遵守 Handler 模式）
- ADR-0910：README 编写与维护宪法（示例可出现在 README 中）
- ADR-0000：架构测试与 CI 治理宪法（示例受架构测试约束）
- `docs/copilot/adr-920.prompts.md`：场景化示例指导（待创建）
- `src/tests/ArchitectureTests/ADR/ADR_920_Architecture_Tests.cs`：执行测试

---

## 核心原则总结

> **示例不是规范，只是演示。**

- 📌 示例代码无架构裁决力
- 📌 示例必须遵守 ADR，不可引入未允许的结构
- 📌 示例必须包含免责声明
- 📌 示例允许简化流程，但不允许简化规则
- 📌 示例 ≠ 测试 ≠ PoC，三者不可混淆
- 📌 PoC 必须使用 `.poc/` 或 `.experimental/` 特殊目录
- 📌 Copilot 必须忽略 PoC 目录，防止误学习
- 📌 示例执行分级测试：L1 阻断 / L2 警告 / L3 允许
- 📌 示例目录必须有责任人（Author）和目的（Purpose）
- 📌 违规的示例 = CI 阻断（L1）或警告（L2），无例外

---

## 版本历史

| 版本  | 日期         | 变更说明                                      |
|-----|------------|-------------------------------------------|
| 1.0 | 2026-01-25 | 初版，定义示例代码的法律地位、禁止行为、免责声明要求和自动化执法机制  |

---

**维护**：架构委员会  
**审核**：@douhuaa  
**状态**：✅ Final
