# ADR-907 测试结构说明

## 概述

本目录包含 ADR-907（ArchitectureTests 执法治理体系）的测试实现。测试采用分部类（partial class）结构组织，以提升可读性、可维护性和扩展性。

## 文件结构

```
tests/
├── README.md                    # 本文档
├── Adr907Tests.Base.cs          # 基类分部文件（公共基础设施）
├── Adr907Tests.Rule1.cs         # Rule 1 测试（ArchitectureTests 的法律地位）
├── Adr907Tests.Rule2.cs         # Rule 2 测试（待实现）
├── Adr907Tests.Rule3.cs         # Rule 3 测试（待实现）
├── Adr907Tests.Rule4.cs         # Rule 4 测试（待实现）
├── Adr907TestHelpers.cs         # 公共辅助方法（静态工具类）
└── [其他现有测试文件...]
```

## 设计理念

### 1. 分部类结构

每个规则的测试被分离到独立的文件中，但仍属于同一个 `Adr907Tests` 类：

```csharp
// Adr907Tests.Base.cs - 公共基础设施
public partial class Adr907Tests
{
    private readonly Adr907RuleSet _ruleSet;
    // 共享字段、属性、辅助方法...
}

// Adr907Tests.Rule1.cs - Rule 1 测试
public partial class Adr907Tests
{
    [Fact]
    public void Rule1_Clause1_ArchitectureTestsIsOnlyEnforcer_Should_Pass()
    {
        // 测试实现...
    }
}
```

### 2. 命名约定

- **测试方法命名**：`MethodUnderTest_Condition_ExpectedBehavior`
  - 示例：`Rule1_Clause1_ArchitectureTestsIsOnlyEnforcer_Should_Pass`
  
- **文件命名**：`Adr907Tests.Rule{N}.cs`
  - 示例：`Adr907Tests.Rule1.cs`、`Adr907Tests.Rule2.cs`

### 3. 测试结构

每个测试方法遵循 **Arrange/Act/Assert** 三段式结构：

```csharp
[Fact(DisplayName = "ADR-907_1_1: ArchitectureTests 是 ADR 的唯一自动化执法形式")]
public void Rule1_Clause1_ArchitectureTestsIsOnlyEnforcer_Should_Pass()
{
    // Arrange - 准备测试数据和前置条件
    AssertRuleExists(1, "ArchitectureTests 的法律地位");
    AssertClauseExists(1, 1, "ArchitectureTests 是 ADR 的唯一自动化执法形式");

    // Act - 执行被测试的操作
    var violations = CheckOtherProjectsForNetArchTest();

    // Assert - 验证结果
    AssertNoViolations("ADR-907_1_1", "...", violations, remediationSteps);
}
```

## 基类提供的公共方法

`Adr907Tests.Base.cs` 提供了以下公共辅助方法：

### 规则和条款访问

- `GetRule(int ruleNumber)` - 获取指定规则
- `GetClause(int ruleNumber, int clauseNumber)` - 获取指定条款
- `AssertRuleExists(int ruleNumber, string? expectedSummary = null)` - 断言规则存在
- `AssertClauseExists(int ruleNumber, int clauseNumber, string? expectedCondition = null)` - 断言条款存在

### 违规断言

- `AssertNoViolations(string ruleId, string summary, IEnumerable<string> violations, IEnumerable<string> remediationSteps)` - 断言没有违规，如果有违规则生成详细的错误消息

### 属性

- `RuleSet` - 已定义的 ADR-907 架构规则集

## 运行测试

### 运行所有 ADR-907 测试

```bash
dotnet test --filter "FullyQualifiedName~Adr907Tests"
```

### 运行特定规则的测试

```bash
# 运行 Rule 1 的所有测试
dotnet test --filter "FullyQualifiedName~Adr907Tests&Method~Rule1"
```

### 运行特定条款的测试

```bash
# 运行 Rule 1 Clause 1 的测试
dotnet test --filter "FullyQualifiedName~Adr907Tests&Method~Rule1_Clause1"
```

## 如何添加新的规则测试

### 步骤 1：创建新的分部文件

创建 `Adr907Tests.Rule{N}.cs`，例如 `Adr907Tests.Rule2.cs`：

```csharp
namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.tests;

/// <summary>
/// ADR-907 Rule 2 测试：命名与组织规范
/// </summary>
public partial class Adr907Tests
{
    #region Rule 2 测试方法

    [Fact(DisplayName = "ADR-907_2_1: ArchitectureTests 必须集中于独立测试项目")]
    public void Rule2_Clause1_IndependentTestProject_Should_Exist()
    {
        // Arrange
        AssertClauseExists(2, 1, "ArchitectureTests 必须集中于独立测试项目");

        // Act
        var projectExists = CheckArchitectureTestsProjectExists();

        // Assert
        projectExists.Should().BeTrue("ArchitectureTests 项目必须存在");
    }

    #endregion

    #region Rule 2 私有辅助方法

    private bool CheckArchitectureTestsProjectExists()
    {
        // 实现检查逻辑...
        return true;
    }

    #endregion
}
```

### 步骤 2：实现测试逻辑

- 使用基类提供的 `AssertRuleExists`、`AssertClauseExists` 等方法
- 遵循 Arrange/Act/Assert 三段式结构
- 将具体的检查逻辑提取到私有辅助方法中

### 步骤 3：运行并验证测试

```bash
dotnet build
dotnet test --filter "FullyQualifiedName~Adr907Tests"
```

## 最佳实践

1. **保持方法简短**：每个测试方法只验证一件事
2. **使用描述性命名**：测试方法名应清楚说明被测场景和预期结果
3. **提取辅助方法**：将复杂的检查逻辑提取到私有辅助方法
4. **避免重复**：使用基类提供的公共方法来消除重复代码
5. **添加注释**：对复杂的验证逻辑添加必要的注释说明

## 未来扩展点

### 待实现的规则测试

- [ ] **Rule 2**：命名与组织规范（8 个条款）
- [ ] **Rule 3**：测试质量要求（4 个条款）
- [ ] **Rule 4**：执行与治理机制（6 个条款）

### 可能的改进

1. **使用 Theory + InlineData**：对于需要多个测试用例的场景，可以使用 `[Theory]` 和 `[InlineData]`
2. **参数化测试**：将相似的测试合并为参数化测试以减少重复
3. **性能优化**：对于耗时的检查，考虑添加缓存机制
4. **自动生成测试**：基于 ADR 定义自动生成测试骨架

## 注意事项

1. **占位实现**：某些测试可能包含 TODO 注释，标记需要完善的实现
2. **编译安全**：所有测试代码都应该能够编译通过，即使某些功能尚未完全实现
3. **向后兼容**：新的分部类结构与现有的测试文件共存，保持向后兼容
4. **命名空间**：所有测试文件使用命名空间 `Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.tests`

## 相关文档

- [ADR-907：ArchitectureTests 执法治理体系](../../../../../docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md)
- [ADR-907-A：ADR-907 对齐执行标准](../../../../../docs/adr/governance/adr-907-a-adr-alignment-execution-standard.md)
- [测试辅助工具文档](./Adr907TestHelpers.cs)

## 问题反馈

如有问题或建议，请在项目的 Issue 跟踪器中提出。
