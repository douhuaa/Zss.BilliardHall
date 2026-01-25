# ADR-122：测试代码组织与命名规范

**状态**：✅ Accepted  
**级别**：结构层  
**影响范围**：所有测试代码  
**生效时间**：待审批通过后

---

## 规则本体（Rule）

> **这是本 ADR 唯一具有裁决力的部分。**

### ADR-122.1：测试目录必须镜像源码结构

测试项目的目录结构**必须**完全镜像被测源码的目录结构。

**强制要求**：
- ✅ 测试类文件路径与源码文件路径保持相同的相对路径
- ✅ 测试命名空间与源码命名空间对应
- ❌ 禁止将不同模块的测试混合在同一目录

**示例映射**：
```
源码: src/Modules/Orders/UseCases/CreateOrder/CreateOrderHandler.cs
测试: tests/Modules.Orders.Tests/UseCases/CreateOrder/CreateOrderHandlerTests.cs
```

### ADR-122.2：测试类命名必须遵循 {TypeName}Tests 模式

测试类名称**必须**为被测类名加 `Tests` 后缀。

**命名规则**：
- ✅ `CreateOrderHandler` → `CreateOrderHandlerTests`
- ✅ `Order` → `OrderTests`
- ❌ `TestCreateOrderHandler`（禁止 Test 前缀）
- ❌ `CreateOrderHandlerTest`（禁止单数 Test）
- ❌ `CreateOrderHandlerSpec`（禁止其他后缀）

### ADR-122.3：测试方法命名必须清晰表达意图

测试方法名称**必须**使用 `{MethodName}_{Scenario}_{ExpectedResult}` 模式。

**命名规则**：
- ✅ `Handle_ValidCommand_CreatesOrder`
- ✅ `ApplyDiscount_NegativePercentage_ThrowsException`
- ✅ `Calculate_EmptyItems_ReturnsZero`
- ❌ `Test1`、`TestHandle`（不清晰）
- ❌ `TestValidCase`（过于泛化）

**中文项目允许**：
- ✅ `Handle_有效命令_创建订单`（如团队习惯中文）

### ADR-122.4：架构测试必须单独组织

架构测试**必须**位于独立的 `ArchitectureTests` 项目中，按 ADR 编号组织。

**目录结构**：
```
tests/
  ArchitectureTests/
    ADR/
      ADR_0001_Architecture_Tests.cs
      ADR_0002_Architecture_Tests.cs
      ...
    ArchitectureTests.csproj
```

**禁止**：
- ❌ 将架构测试混入业务测试项目
- ❌ 将架构测试分散在多个项目

### ADR-122.5：测试项目命名必须遵循 {Module}.Tests 模式

测试项目名称**必须**为模块名加 `.Tests` 后缀。

**命名规则**：
- ✅ `Modules.Orders.Tests`（测试 Orders 模块）
- ✅ `Platform.Tests`（测试 Platform 层）
- ✅ `ArchitectureTests`（架构测试特例）
- ❌ `OrdersTests`（缺少命名空间前缀）
- ❌ `Tests.Orders`（顺序错误）

---

## 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 测试映射

| 规则编号 | 执行级 | 测试/手段 |
|---------|--------|----------|
| ADR-122.1 | L1 | `Test_Directory_Structure_Must_Mirror_Source` |
| ADR-122.2 | L1 | `Test_Classes_Must_End_With_Tests` |
| ADR-122.3 | L2 | Code Review |
| ADR-122.4 | L1 | `Architecture_Tests_Must_Be_In_Separate_Project` |
| ADR-122.5 | L1 | `Test_Projects_Must_Follow_Naming_Convention` |

### 架构测试说明

**L1 测试**：
- 验证测试文件路径与源码文件路径的映射关系
- 检查测试类名是否以 `Tests` 结尾
- 验证架构测试是否在专用项目中
- 检查测试项目命名是否符合规范

**L2 测试**：
- 通过 Code Review 检查测试方法命名的清晰度

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例**仅在以下情况允许**：

1. **遗留测试迁移**：大规模重构前的临时过渡
2. **第三方测试框架限制**：框架强制要求特定结构
3. **集成测试特殊需求**：需要特殊目录组织

### 破例要求（不可省略）

每个破例**必须**：

- 记录在 `docs/summaries/arch-violations.md`
- 指明 ADR-122 + 具体规则编号
- 提供迁移计划和时间表
- 指定失效日期（不超过 3 个月）

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

* **结构层 ADR**
  * 修改需 Tech Lead 审批
  * 需评估对现有测试的影响
  * 可提供迁移脚本辅助

### 失效与替代

* 如有更优组织方案，可创建 ADR-12X 替代
* 被替代后，状态改为 Superseded

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

- ✗ 测试框架选择（xUnit/NUnit/MSTest）
- ✗ 测试覆盖率要求
- ✗ 单元测试 vs 集成测试的划分标准
- ✗ Mock 框架的使用规范
- ✗ 测试数据的生成策略

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 相关 ADR
- ADR-0001：模块化单体与垂直切片架构
- ADR-0003：命名空间与项目边界规范

### 实践指导
- 测试组织详细示例参见 `docs/TESTING-GUIDE.md`
- 常见问题排查参见 `docs/copilot/adr-0122.prompts.md`

---

## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 |
|-----|------|---------|--------|
| 1.0 Draft | 2026-01-24 | 初始版本 | GitHub Copilot |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
