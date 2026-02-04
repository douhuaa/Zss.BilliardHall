---
adr: ADR-400
title: "测试工程规范（Testing Engineering Standards）"
status: Draft
level: Technical
deciders: "Architecture Board"
date: 2026-02-04
version: "1.0"
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---

# ADR-400：测试工程规范（Testing Engineering Standards）

> ⚖️ **本 ADR 旨在规范与架构相关的测试工程结构、命名规则和映射方式。**
> 
> 此规范确保测试与架构决策记录（ADR）一一对应，增强自动化检测和代码治理。

---

## Focus（聚焦内容）

本 ADR 聚焦解决以下问题：

- 测试代码目录与文件结构的标准化
- 测试命名规则的统一性
- Test 与 RuleId 映射关系的明确性
- 测试类型与覆盖范围的定义
- CI/CD 集成与 Guardian 执法的自动化

**本 ADR 的目标**：

> **建立可执行、可验证、可追溯的测试工程体系，确保每个架构决策都有对应的自动化测试验证。**

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 架构测试 | 针对 ADR 规则的自动化验证测试 | Architecture Tests |
| Rule → Test 映射 | ADR 规则与测试用例的一一对应关系 | Rule-to-Test Mapping |
| RuleId | ADR 规则的唯一标识符，格式：ADR-XXX_Y_Z | Rule Identifier |
| 测试覆盖率 | 架构决策规则被测试覆盖的百分比 | Test Coverage |
| Guardian 执法 | 自动化检查系统验证规则-测试映射关系 | Guardian Enforcement |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-400 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-400_<Rule>_<Clause>
> ```

---

### ADR-400_1：测试代码目录与文件结构（Rule）

#### ADR-400_1_1 测试目录结构规范【必须架构测试覆盖】

测试代码**必须**按照以下目录结构进行组织：

```text
src/tests/
│
├── ArchitectureTests/
│   ├── ADR-001/
│   │   └── ADR_001_Architecture_Tests.cs
│   ├── ADR-002/
│   │   └── ADR_002_Architecture_Tests.cs
│   └── ADR-XXX/
│       └── ADR_XXX_Y_Architecture_Tests.cs
│
├── IntegrationTests/      (如存在)
│   └── <Module>_Integration_Tests.cs
│
├── UnitTests/             (如存在)
│   └── <Module>_Unit_Tests.cs
│
└── Shared/                (测试工具和辅助类)
    └── TestHelpers.cs
```

**规则**：

- 所有架构测试**必须**位于 `src/tests/ArchitectureTests/` 目录下
- 每个 ADR **必须**有独立的子目录，目录名为 `ADR-XXX`
- 架构测试目录**禁止**与其他类型测试混合
- 测试项目名称**必须**为 `ArchitectureTests.csproj`

**判定**：

- ❌ 架构测试分散在多个目录
- ❌ 架构测试与集成测试混合存放
- ✅ 架构测试统一在 ArchitectureTests 目录下，按 ADR 编号组织

---

#### ADR-400_1_2 测试文件命名规则【必须架构测试覆盖】

测试文件**必须**遵循以下命名约定：

**架构测试命名规则**：

- 单一 Rule 测试：`ADR_XXX_Y_Architecture_Tests.cs`
- 整体 ADR 测试：`ADR_XXX_Architecture_Tests.cs`

**示例**：

- `ADR_001_Architecture_Tests.cs` - 覆盖 ADR-001 所有规则
- `ADR_902_1_Architecture_Tests.cs` - 仅覆盖 ADR-902_1 规则
- `ADR_902_2_Architecture_Tests.cs` - 仅覆盖 ADR-902_2 规则

**其他测试命名规则**（如适用）：

- 集成测试：`<Module>_Integration_Tests.cs`
- 单元测试：`<Module>_Unit_Tests.cs`
- 辅助工具：`<Utility>.cs`（无 `_Tests` 后缀）

**规则**：

- 架构测试文件名**必须**包含 `ADR_XXX` 前缀
- 架构测试类名**必须**以 `_Architecture_Tests` 结尾
- **禁止**使用其他命名模式（如 `Adr`, `adr`, `Test_ADR` 等）
- 命名空间**必须**为 `Zss.BilliardHall.Tests.ArchitectureTests.ADR_XXX`

**判定**：

- ❌ 文件名为 `Adr001Tests.cs` 或 `ADR001Test.cs`
- ❌ 命名空间为 `Zss.BilliardHall.ArchitectureTests`
- ✅ 文件名为 `ADR_001_Architecture_Tests.cs`
- ✅ 命名空间为 `Zss.BilliardHall.Tests.ArchitectureTests.ADR_001`

---

### ADR-400_2：Rule → Test 映射方式（Rule）

#### ADR-400_2_1 每个 Rule 必须对应至少一个测试【必须架构测试覆盖】

每个架构决策记录（ADR）中的 Rule **必须**有对应的测试用例。

**规则**：

- 每个 `ADR-XXX_Y`（Rule 级别）**必须**有至少一个测试方法
- 测试方法名**应该**包含 RuleId 或清晰描述验证的规则
- **禁止**存在没有对应测试的 Rule
- **禁止**测试方法名与 RuleId 完全无关联

**测试方法命名推荐**：

```csharp
// 方式 1：直接映射 RuleId
[Fact]
public void ADR_XXX_Y_Z_ShouldEnforceSpecificConstraint()

// 方式 2：描述性命名（注释说明 RuleId）
/// <summary>
/// 验证 ADR-XXX_Y_Z: 规则描述
/// </summary>
[Fact]
public void ShouldEnforceModuleIsolation()
```

**判定**：

- ❌ ADR-001_1 存在但无任何测试覆盖
- ❌ 测试方法命名为 `Test1()`, `TestMethod()`
- ✅ 每个 Rule 都有对应的测试方法
- ✅ 测试方法名清晰表明验证的规则

---

#### ADR-400_2_2 测试必须包含 RuleId 引用【必须架构测试覆盖】

每个架构测试方法或测试类**必须**明确声明其验证的 RuleId。

**规则**：

- 测试类注释**必须**列出覆盖的 RuleId 清单
- 测试方法**应该**在注释中说明验证的具体 RuleId
- **禁止**无 RuleId 引用的架构测试

**示例**：

```csharp
/// <summary>
/// ADR-001_1: 模块隔离规则
/// 验证模块之间不能相互引用
/// 
/// 测试覆盖映射：
/// - ADR-001_1_1: 模块之间禁止直接引用
/// - ADR-001_1_2: 模块之间禁止类型依赖
/// </summary>
public sealed class ADR_001_1_Architecture_Tests
{
    /// <summary>
    /// 验证 ADR-001_1_1: 模块之间禁止直接引用
    /// </summary>
    [Fact]
    public void ADR_001_1_1_ModulesShouldNotReferenceEachOther()
    {
        // 测试实现
    }
}
```

**判定**：

- ❌ 测试类无任何 RuleId 注释
- ❌ 测试方法无法追溯到具体 Rule
- ✅ 测试类和方法都清晰标注 RuleId
- ✅ RuleId 格式符合 `ADR-XXX_Y_Z` 标准

---

### ADR-400_3：架构测试类型与覆盖范围（Rule）

#### ADR-400_3_1 架构测试必须验证 ADR 核心约束【必须架构测试覆盖】

架构测试的主要职责是验证 ADR 中 Decision 章节的核心约束。

**规则**：

- 架构测试**必须**验证 ADR Decision 章节中的"必须"（MUST）规则
- 架构测试**应该**验证 ADR Decision 章节中的"应该"（SHOULD）规则
- 架构测试**可以**验证 ADR Prohibited 章节中的禁止行为
- **禁止**架构测试验证非架构性约束（如业务逻辑正确性）

**判定**：

- ❌ 架构测试验证业务逻辑（如订单金额计算正确性）
- ❌ 架构测试验证 UI 显示效果
- ✅ 架构测试验证模块隔离规则
- ✅ 架构测试验证命名空间约束
- ✅ 架构测试验证依赖方向规则

---

#### ADR-400_3_2 架构测试覆盖率要求【必须架构测试覆盖】

架构测试**必须**覆盖所有关键架构决策。

**规则**：

- 宪法层 ADR（ADR-001 至 ADR-010）**必须**100% 覆盖
- 治理层 ADR（ADR-900+）**必须**覆盖关键治理规则
- 其他层级 ADR **应该**有对应的架构测试
- **禁止**跳过架构测试（使用 `Skip` 属性）

**覆盖率计算**：

```
架构测试覆盖率 = (已测试的 Rule 数量 / 总 Rule 数量) × 100%
```

**判定**：

- ❌ 宪法层 ADR 存在未测试的 Rule
- ❌ 架构测试使用 `[Fact(Skip = "TODO")]`
- ✅ 所有宪法层 ADR 都有完整测试覆盖
- ✅ 测试覆盖率报告可追溯到具体 Rule

---

### ADR-400_4：CI/CD 集成与 Guardian 执法（Rule）

#### ADR-400_4_1 架构测试必须集成到 CI/CD【必须架构测试覆盖】

所有架构测试**必须**通过 CI/CD 自动执行。

**规则**：

- 每次代码提交**必须**触发架构测试
- 每次 Pull Request **必须**执行架构测试
- 架构测试失败**必须**阻断 CI 构建
- **禁止**在 CI 中跳过架构测试

**CI 工作流配置要求**：

```yaml
# .github/workflows/architecture-tests.yml
name: Architecture Tests
on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  architecture-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
      - name: Build
        run: dotnet build
      - name: Run Architecture Tests
        run: dotnet test src/tests/ArchitectureTests
```

**判定**：

- ❌ CI 配置中无架构测试步骤
- ❌ 架构测试失败但 CI 仍然通过
- ✅ 架构测试集成在 CI 工作流中
- ✅ 架构测试失败导致 CI 失败

---

#### ADR-400_4_2 Guardian 必须验证 Rule-Test 映射【必须架构测试覆盖】

Guardian 系统**必须**监控架构决策记录（ADR）与测试的关联性。

**规则**：

- Guardian **必须**验证每个 Rule 都有对应测试
- Guardian **必须**验证测试文件命名符合规范
- Guardian **必须**验证测试目录结构符合规范
- 若发现**Rule 未对应测试**，Guardian **必须**自动报错并阻止代码合并

**Guardian 检查项**：

1. ✅ 每个 ADR 都有对应的测试目录
2. ✅ 每个 Rule 都有对应的测试方法
3. ✅ 测试文件命名符合规范
4. ✅ 测试类包含 RuleId 引用

**判定**：

- ❌ 新增 ADR 但未创建对应测试
- ❌ Guardian 检查通过但存在未测试的 Rule
- ✅ Guardian 检测到未测试 Rule 并阻断 PR
- ✅ Guardian 验证测试命名规范

---

## Enforcement（执法模型）

### 执行级别

本 ADR 所有规则均为 **L1（静态可执行）**：

- 通过架构测试自动验证
- CI 自动阻断不符合规范的提交
- 零容忍，不允许破例

### 执行方法

1. **自动化测试验证**：
   - 所有规则通过架构测试自动验证
   - 测试失败 = 规则违反
   - CI 中自动执行

2. **Guardian 实时监控**：
   - 监控 ADR 与测试的映射关系
   - 检测测试命名和结构规范
   - 自动报告违规情况

3. **Code Review 人工检查**：
   - 架构师审查新增 ADR 的测试覆盖
   - 验证测试质量和有效性
   - 确认 RuleId 映射准确性

### 监控指标

| 指标 | 目标 | 监控方式 |
|------|------|---------|
| 架构测试覆盖率 | 100% | 自动化报告 |
| Rule-Test 映射完整性 | 100% | Guardian 检查 |
| CI 集成率 | 100% | CI 配置审查 |
| 测试命名规范符合率 | 100% | 自动化检查 |

---

## Non-Goals（明确不管什么）

本 ADR **不涉及**以下内容：

1. **单元测试规范**：
   - 单元测试的组织方式和命名规则
   - 由各模块团队自行决定

2. **集成测试规范**：
   - 集成测试的环境配置和数据管理
   - 由 ADR-301 等专门 ADR 规范

3. **测试框架选择**：
   - 使用 xUnit、NUnit 还是 MSTest
   - 由技术栈决策确定

4. **测试性能优化**：
   - 测试执行速度优化策略
   - 由各团队根据实际情况优化

5. **业务逻辑测试**：
   - 业务功能的正确性验证
   - 由功能测试和验收测试覆盖

---

## Prohibited（禁止行为）

### ADR-400_P_1 禁止无测试的架构决策

任何架构决策若未能提供对应的测试用例，**必须**视为**无效决策**，不予通过。

**理由**：

- 无测试的架构决策无法验证执行情况
- 无法在 CI 中自动执行
- 无法保证架构约束得到遵守

**违规示例**：

- ❌ 创建新 ADR 但未创建对应的架构测试
- ❌ 修改现有 ADR 的 Rule 但未更新测试

**正确做法**：

- ✅ 创建 ADR 的同时创建对应的架构测试
- ✅ 修改 Rule 时同步更新相关测试

---

### ADR-400_P_2 禁止缺乏映射关系的测试

任何测试若不能与明确的 RuleId 映射，**必须**视为**无效测试**，不予执行。

**理由**：

- 无法追溯测试验证的具体架构约束
- 无法评估架构决策的测试覆盖率
- 增加测试维护成本

**违规示例**：

- ❌ 测试类和方法无任何 RuleId 注释
- ❌ 测试方法名为 `Test1()`, `TestMethod()`
- ❌ 测试注释中 RuleId 格式错误（如 `ADR001.1`）

**正确做法**：

- ✅ 测试类注释列出覆盖的 RuleId 清单
- ✅ 测试方法使用标准 RuleId 格式
- ✅ 测试文档中建立 Rule → Test 映射表

---

### ADR-400_P_3 禁止跳过架构测试

**禁止**在代码中使用 `Skip` 属性跳过架构测试。

**理由**：

- 跳过的测试无法验证架构约束
- 容易被遗忘，导致技术债务累积
- 破坏测试覆盖率完整性

**违规示例**：

```csharp
// ❌ 禁止
[Fact(Skip = "TODO: fix later")]
public void ADR_001_1_1_ShouldEnforceModuleIsolation()
{
    // 测试实现
}
```

**正确做法**：

- ✅ 立即修复失败的测试
- ✅ 如确实需要临时禁用，创建 Issue 追踪并在 PR 中说明
- ✅ 通过 Code Review 确保跳过的测试尽快修复

---

### ADR-400_P_4 禁止在 CI 中跳过架构测试

**禁止**在 CI 配置中跳过或忽略架构测试失败。

**理由**：

- CI 是架构约束执行的最后防线
- 跳过 CI 中的架构测试等于放弃架构治理
- 无法保证架构规范得到严格执行

**违规示例**：

```yaml
# ❌ 禁止
- name: Run Architecture Tests
  run: dotnet test src/tests/ArchitectureTests
  continue-on-error: true  # 禁止这样做
```

**正确做法**：

```yaml
# ✅ 正确
- name: Run Architecture Tests
  run: dotnet test src/tests/ArchitectureTests
  # 测试失败应该导致 CI 失败
```

---

## Relationships（关系声明）

### 上游依赖

本 ADR 依赖以下 ADR：

- **ADR-902**：ADR 标准模板与结构契约
  - 定义了 ADR 文档的结构规范
  - 提供了 RuleId 格式标准

- **ADR-907**：ArchitectureTests 执法治理体系
  - 定义了架构测试的治理框架
  - 提供了测试执行和失败处理机制

- **ADR-900**：架构测试与 CI 治理元规则
  - 定义了架构测试的元规则
  - 提供了 CI 集成和破例治理机制

### 下游影响

本 ADR 影响以下内容：

- **所有新增 ADR**：
  - 必须按照本规范创建对应的架构测试
  - 必须遵循 Rule → Test 映射规则

- **现有架构测试**：
  - 应该逐步迁移到标准目录结构
  - 应该补充 RuleId 映射注释

- **CI/CD 配置**：
  - 必须包含架构测试执行步骤
  - 必须配置测试失败阻断机制

### 相关文档

- 架构测试 README：`src/tests/ArchitectureTests/README.md`
- CI 配置文件：`.github/workflows/architecture-tests.yml`
- ADR 关系映射：`docs/adr/ADR-RELATIONSHIP-MAP.md`

---

## References（非裁决性参考）

### 测试框架

- xUnit: https://xunit.net/
- NetArchTest: https://github.com/BenMorris/NetArchTest

### 最佳实践

- ADR 测试映射最佳实践文档（待补充）
- 架构测试编写指南（待补充）

### 示例

参考现有架构测试实现：

- `src/tests/ArchitectureTests/ADR-902/` - ADR 结构测试示例
- `src/tests/ArchitectureTests/ADR-907/` - 执法体系测试示例

---

## History（版本历史）

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|---------|------|
| 1.0 | 2026-02-04 | 初始版本，定义测试工程规范 | Architecture Board |

