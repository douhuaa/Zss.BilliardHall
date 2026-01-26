# ADR-122：测试代码组织与命名规范

> ⚖️ **本 ADR 定义测试代码组织与命名的唯一裁决规则。**

**状态**：✅ Accepted  
**级别**：结构层  
**影响范围**：所有测试代码  
**生效时间**：待审批通过后

---

## 聚焦内容（Focus）

- 测试目录结构必须镜像源码
- 测试类与方法命名规范
- 架构测试独立组织要求
- 测试项目命名约束
- 自动化执法机制

---

## 术语表（Glossary）

| 术语      | 定义                          | 英文对照                |
|---------|-------------------------------|------------------------|
| 镜像结构    | 测试目录与源码目录保持完全一致的相对路径关系 | Mirror Structure       |
| 架构测试    | 验证架构约束的专用测试，独立于业务测试    | Architecture Test      |
| L1 测试   | 静态可执行自动化测试              | Level 1 Test           |
| L2 测试   | 语义半自动化测试或人工审查          | Level 2 Test           |

---

## 决策（Decision）

### ADR-122.1：测试目录必须镜像源码结构

**规则**：
- 测试项目的目录结构**必须**完全镜像被测源码的目录结构
- 测试类文件路径与源码文件路径保持相同的相对路径
- 测试命名空间与源码命名空间对应

**判定**：
- ✅ 测试文件路径与源码文件路径保持相同相对结构
- ❌ 不同模块的测试混合在同一目录
- ❌ 测试目录结构与源码不一致

**示例**：
```
源码: src/Modules/Orders/UseCases/CreateOrder/CreateOrderHandler.cs
测试: tests/Modules.Orders.Tests/UseCases/CreateOrder/CreateOrderHandlerTests.cs
```

### ADR-122.2：测试类命名必须遵循 {TypeName}Tests 模式

**规则**：
- 测试类名称**必须**为被测类名加 `Tests` 后缀
- 禁止使用 `Test` 前缀或单数形式
- 禁止使用其他后缀（如 `Spec`）

**判定**：
- ✅ `CreateOrderHandler` → `CreateOrderHandlerTests`
- ✅ `Order` → `OrderTests`
- ❌ `TestCreateOrderHandler`（禁止 Test 前缀）
- ❌ `CreateOrderHandlerTest`（禁止单数 Test）
- ❌ `CreateOrderHandlerSpec`（禁止其他后缀）

### ADR-122.3：测试方法命名必须清晰表达意图

**规则**：
- 测试方法名称**必须**使用 `{MethodName}_{Scenario}_{ExpectedResult}` 模式
- 方法名必须清晰描述测试场景和预期结果
- 中文项目允许使用中文命名

**判定**：
- ✅ `Handle_ValidCommand_CreatesOrder`
- ✅ `ApplyDiscount_NegativePercentage_ThrowsException`
- ✅ `Handle_有效命令_创建订单`（中文项目）
- ❌ `Test1`、`TestHandle`（不清晰）
- ❌ `TestValidCase`（过于泛化）

### ADR-122.4：架构测试必须单独组织

**规则**：
- 架构测试**必须**位于独立的 `ArchitectureTests` 项目中
- 架构测试**必须**按 ADR 编号组织
- 禁止将架构测试混入业务测试项目

**判定**：
- ✅ 架构测试位于 `tests/ArchitectureTests/ADR/`
- ✅ 测试类命名：`ADR_XXXX_Architecture_Tests.cs`
- ❌ 架构测试混入 `Modules.*.Tests` 项目
- ❌ 架构测试分散在多个项目

**目录结构**：
```
tests/
  ArchitectureTests/
    ADR/
      ADR_0001_Architecture_Tests.cs
      ADR_0002_Architecture_Tests.cs
```

### ADR-122.5：测试项目命名必须遵循 {Module}.Tests 模式

**规则**：
- 测试项目名称**必须**为模块名加 `.Tests` 后缀
- 必须保持命名空间前缀
- `ArchitectureTests` 为特例，不加 `.Tests` 后缀

**判定**：
- ✅ `Modules.Orders.Tests`（测试 Orders 模块）
- ✅ `Platform.Tests`（测试 Platform 层）
- ✅ `ArchitectureTests`（架构测试特例）
- ❌ `OrdersTests`（缺少命名空间前缀）
- ❌ `Tests.Orders`（顺序错误）

---

## 快速参考表

| 约束编号       | 约束描述             | 测试方式       | 测试用例                                    | 必须遵守 |
|------------|------------------|------------|------------------------------------------|------|
| ADR-122.1  | 测试目录必须镜像源码结构     | L1 - 自动化测试 | Test_Directory_Structure_Must_Mirror_Source | ✅    |
| ADR-122.2  | 测试类必须以 Tests 结尾 | L1 - 自动化测试 | Test_Classes_Must_End_With_Tests           | ✅    |
| ADR-122.3  | 测试方法命名必须清晰       | L2 - Code Review | Test_Method_Naming_Clarity_Check          | ✅    |
| ADR-122.4  | 架构测试必须独立组织       | L1 - 自动化测试 | Architecture_Tests_Must_Be_In_Separate_Project | ✅    |
| ADR-122.5  | 测试项目命名规范         | L1 - 自动化测试 | Test_Projects_Must_Follow_Naming_Convention  | ✅    |

> **级别说明**：L1=静态自动化（脚本检查），L2=语义半自动或人工审查

---

## 必测/必拦架构测试（Enforcement）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_122_Architecture_Tests.cs` 强制验证：

- 测试文件路径与源码文件路径的映射关系验证
- 测试类名是否以 `Tests` 结尾
- 架构测试是否在专用项目中
- 测试项目命名是否符合规范

**L2 测试**：
- 通过 Code Review 检查测试方法命名的清晰度

**有一项违规视为架构违规，CI 自动阻断。**

---

## 检查清单

- [ ] 测试目录结构是否完全镜像源码？
- [ ] 测试类名是否以 Tests 结尾？
- [ ] 测试方法命名是否清晰表达意图？
- [ ] 架构测试是否在独立项目中？
- [ ] 测试项目命名是否符合规范？

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

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) - 测试组织基于模块和垂直切片结构
- [ADR-0003：命名空间与项目结构规范](../constitutional/ADR-0003-namespace-rules.md) - 测试命名空间遵循命名空间规范
- [ADR-0006：术语与编号宪法](../constitutional/ADR-0006-terminology-numbering-constitution.md) - 测试命名遵循术语规范

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0301：集成测试自动化](../technical/ADR-301-integration-test-automation.md) - 同为测试规范

---

## 版本历史

| 版本  | 日期         | 变更说明       | 修订人 |
|-----|------------|------------|-----|
| 2.0 | 2026-01-26 | 裁决型重构，添加决策章节 | GitHub Copilot |
| 1.0 | 2026-01-24 | 初始版本       | GitHub Copilot |

---

## 附注

本文件禁止添加示例/建议/FAQ/背景说明，仅维护自动化可判定的架构红线。

非裁决性参考（详细示例、测试组织最佳实践、常见问题排查）请查阅：
- `docs/copilot/adr-0122.prompts.md`
- `docs/TESTING-GUIDE.md`
