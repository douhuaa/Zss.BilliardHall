# 架构测试编写指南（Architecture Test Guidelines）

> **文档版本**: 3.0  
> **最后更新**: 2026-02-07  
> **文档定位**: 非裁决性指导文档，提供最佳实践建议  
> **权威依据**: 本文档基于 ADR-900、ADR-905、ADR-907 及新的 RuleSet 治理体系
> 
> ⚠️ **重要提醒**：
> - ✅ 本文档是**指导性文档**，提供最佳实践建议
> - ✅ **权威依据**仍然是 ADR 文档（docs/adr/）
> - ✅ 如有冲突，以 ADR 正文为准

---

## 🎯 版本 3.0 主要变更

本次更新引入 **架构治理新体系**（PR #330），核心理念：

```
ADR ≠ Test ≠ Specification（三者物理隔离）
```

**关键变化**：
1. ✅ **新增 RuleSet 体系**：规则定义与测试逻辑分离
2. ✅ **新增 RuleSetRegistry**：统一规则集访问入口
3. ✅ **测试只能"引用规则"**：不能"定义规则"
4. ✅ **Rule 是最小裁决单元**：ADR 只是容器

**详细迁移指南**：[MIGRATION-ADR-TESTS-TO-RULESETS.md](../MIGRATION-ADR-TESTS-TO-RULESETS.md)

**Specification 架构文档**：[Specification README](../../src/tests/ArchitectureTests/Specification/README.md)

---

## 📊 执行摘要

本指南帮助开发者编写高质量、可维护的架构测试，消除代码重复，统一测试格式。

### 🎯 核心规范速查

| 规范类别 | 要求 | 优先级 | 采用率目标 |
|---------|------|--------|-----------|
| **RuleSetRegistry** | 从 Registry 获取规则信息 | 🔴 P0 | 100% |
| **参数化测试** | 使用 Theory + InlineData 减少重复 | 🔴 P1 | 80% |
| **TestEnvironment** | 使用共享路径常量 | 🔴 P0 | 100% |
| **FileAssertionHelper / FileContentAnalyzer / FileSearchHelper** | 使用专用文件操作工具 | 🔴 P1 | 80% |
| **AssertionMessageBuilder** | 使用标准断言消息 | 🔴 P1 | 80% |
| **AdrTestFixture** | 使用 ADR 文档缓存 | 🟡 P2 | 50% |
| **sealed 关键字** | 所有测试类必须 sealed | 🔴 P0 | 100% |
| **命名规范** | ADR_XXX_Y_Architecture_Tests | 🔴 P0 | 100% |

### 📈 当前状态

| 指标 | 现状 | 说明 |
|------|------|------|
| ✅ TestEnvironment 采用 | 65.6% (82/125) | FindRepositoryRoot 重复已基本消除 |
| ⚠️ 文件操作工具采用 | 17.6% (22/125) | 仍有 73 个文件直接使用 File/Directory |
| ⚠️ AssertionMessageBuilder 采用 | 22.4% (28/125) | 97 个文件手动构建断言消息 |
| 🚨 AdrTestFixture 采用 | 0.8% (1/125) | 40 个测试重复加载 ADR 文档 |

### 🎯 关键收益

遵循本指南可实现：
- 📉 **代码量减少 29%**：从 15,553 行降至 ~11,000 行
- 🔄 **重复代码减少 77%**：从 ~4,300 行降至 ~1,000 行  
- ⚡ **测试速度提升 20%**：通过 ADR 文档缓存
- 🛠️ **维护成本降低 50%**：统一格式和工具

---

## 🆕 使用 RuleSet 和 RuleSetRegistry

### 什么是 RuleSet？

**RuleSet** 是规则定义的中心化位置，将 ADR 文档转换为可执行的规范。

**核心理念**：
```
ADR（文档） → RuleSet（规范） → Test（验证）
```

**位置**：`src/tests/ArchitectureTests/Specification/RuleSets/`

**已创建数量**：43 个 RuleSet 定义（覆盖 ADR-001 至 ADR-990）

### RuleSetRegistry 基本用法

> **📌 注意**：`RuleSetRegistry` 相关的命名空间已包含在全局using中（`GlobalUsings.cs`），无需在测试文件中重复导入。

**获取规则集**：
```csharp
// 方式 1：按编号获取（推荐用于测试）
var ruleSet = RuleSetRegistry.GetStrict(1);     // 抛异常如果不存在（严格模式）
var ruleSet = RuleSetRegistry.Get(1);           // 返回 null 如果不存在（宽容模式）

// 方式 2：按字符串获取（支持多种格式）
var ruleSet = RuleSetRegistry.GetStrict("ADR-001");

// 方式 3：按分类获取
var constitutional = RuleSetRegistry.GetConstitutionalRuleSets(); // ADR-001 ~ 008
var governance = RuleSetRegistry.GetGovernanceRuleSets();         // ADR-900 ~ 999
var runtime = RuleSetRegistry.GetRuntimeRuleSets();               // ADR-201 ~ 240
```

**获取规则和条款**：
```csharp
// 获取规则集
var ruleSet = RuleSetRegistry.GetStrict(1);

// 获取特定规则
var rule = ruleSet.GetRule(1);           // 获取 Rule 1
Console.WriteLine($"规则: {rule.Id} - {rule.Summary}");

// 获取特定条款
var clause = ruleSet.GetClause(1, 1);    // 获取 Rule 1, Clause 1
Console.WriteLine($"条款: {clause.Id}");           // "ADR-001.1.1"
Console.WriteLine($"条件: {clause.Condition}");    // 规则的具体内容
Console.WriteLine($"执行方式: {clause.Enforcement}"); // 如何执行这个规则
```

### 宽容模式 vs 严格模式

| 模式 | 方法 | 不存在时行为 | 适用场景 |
|-----|------|------------|---------|
| 宽容模式 | `Get()` | 返回 `null` | 探索性查询、条件性测试 |
| 严格模式 | `GetStrict()` | 抛出异常 | **测试（推荐）、CI、Analyzer** |

**使用建议**：
- ✅ **测试中使用 `GetStrict()`**：测试中的 RuleId 错误应该立即失败
- ✅ **探索性查询使用 `Get()`**：需要检查 RuleSet 是否存在时

### 使用 RuleSetRegistry 的优势

| 传统方式 ❌ | RuleSetRegistry 方式 ✅ |
|-----------|---------------------|
| 硬编码 RuleId 字符串 | 从 Registry 获取，类型安全 |
| 手动拼接规则描述 | 使用 `clause.Condition` |
| 规则信息与测试耦合 | 规则定义集中管理 |
| 难以批量更新规则信息 | 修改 RuleSet 即可 |
| 无法验证 RuleId 正确性 | Registry 自动验证 |

---

## 📋 共性问题分析

### 🔴 高优先级问题

#### 问题 1：FindRepositoryRoot 重复（已基本解决）

| 维度 | 数据 |
|------|------|
| **影响范围** | 曾有 84 个文件，现剩 2 个 |
| **重复代码** | ~40 行待消除 |
| **改善幅度** | 96.4% |
| **剩余文件** | ADR_301、ADR_360 |

**✅ 解决方案**：使用 `TestEnvironment.RepositoryRoot`

```csharp
// ❌ 不推荐（已过时）
private static string? FindRepositoryRoot() { /* 20+ 行代码 */ }

// ✅ 推荐（标准方式）
var repoRoot = TestEnvironment.RepositoryRoot;
```

#### 问题 2：直接文件操作（73 个文件）

| 维度 | 数据 |
|------|------|
| **影响范围** | 58.4% 测试文件 |
| **潜在收益** | 减少 ~1,825 行代码 |
| **当前问题** | 缺少错误处理、格式不统一 |

**✅ 解决方案**：使用专用文件操作工具

```csharp
// ❌ 不推荐
var content = File.ReadAllText(filePath);
content.Should().Contain("关键词");

// ✅ 推荐 - 使用 FileContentAnalyzer
FileContentAnalyzer.AssertFileContains(filePath, "关键词", "文件应包含关键词");

// ✅ 推荐 - 使用 FileAssertionHelper
FileAssertionHelper.AssertFileExists(filePath, "❌ ADR-XXX_Y_Z 违规：文件不存在");
```

#### 问题 3：手动构建断言消息（97 个文件）

| 维度 | 数据 |
|------|------|
| **影响范围** | 77.6% 测试文件 |
| **潜在收益** | 统一 ~2,425 行代码 |
| **当前问题** | 格式不统一、质量参差不齐 |

**✅ 解决方案**：使用 `AssertionMessageBuilder`

```csharp
// ❌ 不推荐（手动拼接）
File.Exists(filePath).Should().BeTrue(
    $"❌ ADR-XXX_Y_Z 违规：文件不存在\n预期路径：{filePath}");

// ✅ 推荐（使用构建器）
var message = AssertionMessageBuilder.BuildFileNotFoundMessage(
    ruleId: "ADR-XXX_Y_Z",
    filePath: filePath,
    fileDescription: "配置文件",
    remediationSteps: new[] { "创建文件", "添加必要配置" },
    adrReference: "docs/adr/XXX.md");
File.Exists(filePath).Should().BeTrue(message);
```

### 🟡 中优先级问题

#### 问题 4：ADR 文档重复加载（40 个文件）

| 维度 | 数据 |
|------|------|
| **影响范围** | 32% 测试文件 |
| **性能影响** | 每次测试重新加载所有 ADR |
| **潜在收益** | 测试速度提升 ~20% |

**✅ 解决方案**：使用 `AdrTestFixture`

```csharp
// ❌ 不推荐（每次加载）
[Fact]
public void Test_Method()
{
    var repository = new AdrRepository(TestEnvironment.AdrPath);
    var adrs = repository.LoadAll();  // 重复加载
}

// ✅ 推荐（使用缓存）
public sealed class ADR_XXX_Tests : IClassFixture<AdrTestFixture>
{
    private readonly AdrTestFixture _fixture;
    public ADR_XXX_Tests(AdrTestFixture fixture) => _fixture = fixture;
    
    [Fact]
    public void Test_Method()
    {
        var adr = _fixture.GetAdr("ADR-XXX");  // 从缓存获取
    }
}
```

---

## 📐 标准测试结构

### 1️⃣ 测试类模板（使用 RuleSetRegistry）

```csharp
namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_XXX;

/// <summary>
/// ADR-XXX_Y: <Rule 标题>（Rule）
/// <简短说明：这个测试类验证什么>
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-XXX_Y_1: <Clause 1 简述>
/// - ADR-XXX_Y_2: <Clause 2 简述>
///
/// 关联文档：
/// - ADR: docs/adr/<category>/ADR-XXX-<title>.md
/// - RuleSet: src/tests/ArchitectureTests/Specification/RuleSets/ADRXXX/AdrXxxRuleSet.cs
/// </summary>
public sealed class ADR_XXX_Y_Architecture_Tests
{
    /// <summary>
    /// ADR-XXX_Y_1: <Clause 标题>
    /// <详细说明>（§ADR-XXX_Y_1）
    /// </summary>
    [Fact(DisplayName = "ADR-XXX_Y_1: <测试显示名称>")]
    public void ADR_XXX_Y_1_<TestMethodName>()
    {
        // Arrange - 从 RuleSetRegistry 获取规则信息
        var ruleSet = RuleSetRegistry.GetStrict(XXX);
        var clause = ruleSet.GetClause(Y, 1);
        
        var repoRoot = TestEnvironment.RepositoryRoot;
        
        // Act
        var result = /* 执行测试 */;
        
        // Assert - 使用规则信息构建断言消息
        var message = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: clause.Id,              // 从 RuleSet 获取
            filePath: expectedPath,
            fileDescription: clause.Condition,  // 从 RuleSet 获取
            remediationSteps: new[]
            {
                "步骤 1",
                "步骤 2"
            },
            adrReference: $"docs/adr/<category>/ADR-{XXX}-<title>.md");
        
        result.Should().BeTrue(message);
    }
}
```

### 2️⃣ 命名规范

| 元素 | 格式 | 示例 ✅ | 反例 ❌ |
|------|------|---------|---------|
| **测试类** | `ADR_<编号>_<Rule序号>_Architecture_Tests` | `ADR_002_1_Architecture_Tests` | `ADR002Tests` |
| **测试方法** | `ADR_<编号>_<Rule序号>_<Clause序号>_<描述>` | `ADR_002_1_1_Platform_Should_Not_Depend_On_Application` | `TestPlatformDependency` |
| **DisplayName** | `"ADR-<编号>_<Rule序号>_<Clause序号>: <中文描述>"` | `"ADR-002.1.1: Platform 不应依赖 Application"` | `"测试 Platform 依赖"` |

### 3️⃣ 断言消息标准格式

**必需字段结构**：

```
❌ ADR-XXX_Y_Z 违规：<简短问题描述>

当前状态：<具体违规情况>

修复建议：
1. <具体步骤 1>
2. <具体步骤 2>
3. <具体步骤 3>

参考：<ADR 文档路径> §ADR-XXX_Y_Z
```

**字段要求**：

| 字段 | 必需性 | 说明 |
|------|--------|------|
| ❌ + RuleId | ✅ 必需 | 必须使用 ❌ emoji + `ADR-XXX_Y_Z` 格式 |
| 问题描述 | ✅ 必需 | 一句话说明违规内容 |
| 当前状态 | ✅ 必需 | 具体数据和事实（文件路径、类型名称等）|
| 修复建议 | ✅ 必需 | 编号列表，至少 1 个可操作步骤 |
| 参考 | ✅ 必需 | 完整 ADR 路径 + § 引用 |
| 预期路径 | ⚪ 可选 | 文件/目录存在性验证时使用 |
| 问题分析 | ⚪ 可选 | 需要解释背景时使用 |

**质量对比**：

| 质量等级 | 占比 | 特征 |
|---------|------|------|
| 🟢 高质量 | 22.4% | 使用 AssertionMessageBuilder，包含所有必需字段 |
| 🟡 中等质量 | 25.6% | 手动构建但有基本信息 |
| 🔴 低质量 | 52.0% | 缺少上下文和修复建议 |

---

## 🧪 参数化测试（Theory 和 InlineData）

### 什么是参数化测试？

参数化测试允许使用不同的输入数据多次运行同一个测试方法，避免编写重复的测试代码。xUnit 提供了 `[Theory]` 和 `[InlineData]` 属性来支持参数化测试。

### 何时使用参数化测试？

**✅ 适合使用的场景**：
- 测试逻辑相同，只是输入和预期输出不同
- 需要测试多种边界条件
- 验证规则解析器、格式化器等纯函数

**❌ 不适合使用的场景**：
- 测试逻辑完全不同
- 需要不同的 Arrange 或 Assert 步骤
- 测试之间有依赖关系

### 基本用法

#### 1️⃣ 简单的参数化测试

```csharp
[Theory(DisplayName = "RuleId 解析器应该正确解析下划线格式")]
[InlineData("ADR-001.1", 1, 1, null)]
[InlineData("ADR-907.3", 907, 3, null)]
[InlineData("001_1", 1, 1, null)]
[InlineData("907_3", 907, 3, null)]
public void TryParse_Should_Parse_Underscore_Rule_Format(
    string input,
    int expectedAdr,
    int expectedRule,
    int? expectedClause)
{
    // Arrange & Act
    var success = RuleIdParser.TryParse(input, out var result);
    
    // Assert
    success.Should().BeTrue($"应该能够解析：{input}");
    result.AdrNumber.Should().Be(expectedAdr);
    result.RuleNumber.Should().Be(expectedRule);
    result.ClauseNumber.Should().Be(expectedClause);
}
```

**说明**：
- 使用 `[Theory]` 替代 `[Fact]`
- 每个 `[InlineData]` 提供一组测试参数
- 测试方法接收参数，参数顺序必须与 InlineData 一致

#### 2️⃣ 使用 MemberData 处理复杂数据

对于复杂的测试数据或需要对象实例的场景，使用 `[MemberData]`：

```csharp
public static IEnumerable<object[]> InvalidInputs { get; } = new List<object[]>
{
    new object[] { null },
    new object[] { "" },
    new object[] { "   " },
    new object[] { "invalid" },
    new object[] { "ADR-" },
    new object[] { "ADR-abc" },
};

[Theory(DisplayName = "TryParse 应该对无效格式返回 false")]
[MemberData(nameof(InvalidInputs))]
public void TryParse_Should_Return_False_For_Invalid_Format(string? input)
{
    // Act
    var success = RuleIdParser.TryParse(input, out _);
    
    // Assert
    success.Should().BeFalse($"不应该解析无效输入：{input ?? "(null)"}");
}
```

### 最佳实践

#### 1. 使用清晰的 DisplayName

```csharp
// ✅ 好的 DisplayName
[Theory(DisplayName = "RuleSet 应该正确返回指定的规则")]
[InlineData(1, 1, "模块物理隔离")]
[InlineData(900, 1, "架构裁决权威性")]

// ❌ 不好的 DisplayName
[Theory(DisplayName = "测试规则")]
```

#### 2. 参数命名要有意义

```csharp
// ✅ 好的参数命名
public void Should_Get_Rule_By_Number(
    int adrNumber,
    int ruleNumber,
    string expectedSummary)

// ❌ 不好的参数命名
public void Should_Get_Rule_By_Number(int a, int b, string c)
```

#### 3. 每组测试数据添加注释

```csharp
[Theory(DisplayName = "应该支持多种 RuleId 格式")]
[InlineData("ADR-001.1", 1, 1, null)]      // 标准格式
[InlineData("001_1", 1, 1, null)]          // 短格式（省略 ADR-）
[InlineData("ADR-001.1", 1, 1, null)]      // 旧格式（兼容性）
```

#### 4. 结合 RuleSetRegistry 使用

```csharp
[Theory(DisplayName = "RuleSet 应该包含正确的规则信息")]
[InlineData(1, 1, "模块物理隔离", RuleSeverity.Constitutional)]
[InlineData(900, 1, "架构裁决权威性", RuleSeverity.Governance)]
[InlineData(907, 3, "最小断言语义规范", null)]
public void RuleSet_Should_Contain_Correct_Rule_Info(
    int adrNumber,
    int ruleNumber,
    string expectedSummary,
    RuleSeverity? expectedSeverity)
{
    // Arrange
    var ruleSet = RuleSetRegistry.GetStrict(adrNumber);
    
    // Act
    var rule = ruleSet.GetRule(ruleNumber);
    
    // Assert
    rule.Summary.Should().Contain(expectedSummary);
    if (expectedSeverity.HasValue)
    {
        rule.Severity.Should().Be(expectedSeverity.Value);
    }
}
```

### 从多个 [Fact] 迁移到 [Theory]

**重构前（❌ 重复代码）**：

```csharp
[Fact(DisplayName = "应该解析 ADR-001.1")]
public void Should_Parse_ADR_001_1()
{
    var success = RuleIdParser.TryParse("ADR-001.1", out var result);
    success.Should().BeTrue();
    result.AdrNumber.Should().Be(1);
    result.RuleNumber.Should().Be(1);
}

[Fact(DisplayName = "应该解析 ADR-907.3")]
public void Should_Parse_ADR_907_3()
{
    var success = RuleIdParser.TryParse("ADR-907.3", out var result);
    success.Should().BeTrue();
    result.AdrNumber.Should().Be(907);
    result.RuleNumber.Should().Be(3);
}

// ... 更多重复的测试
```

**重构后（✅ 参数化测试）**：

```csharp
[Theory(DisplayName = "应该正确解析 RuleId")]
[InlineData("ADR-001.1", 1, 1)]
[InlineData("ADR-907.3", 907, 3)]
[InlineData("ADR-120.2", 120, 2)]
[InlineData("ADR-950.1", 950, 1)]
public void Should_Parse_RuleId_Correctly(
    string input,
    int expectedAdr,
    int expectedRule)
{
    // Act
    var success = RuleIdParser.TryParse(input, out var result);
    
    // Assert
    success.Should().BeTrue($"应该能够解析：{input}");
    result.AdrNumber.Should().Be(expectedAdr);
    result.RuleNumber.Should().Be(expectedRule);
}
```

**优势**：
- ✅ 减少代码重复（从 ~50 行减少到 ~20 行）
- ✅ 更容易添加新的测试用例
- ✅ 测试报告更清晰（显示每个数据组合）
- ✅ 维护成本更低

### 注意事项

1. **避免过多参数**：如果参数超过 5 个，考虑使用对象或 MemberData
2. **保持测试独立**：每个测试应该独立运行，不依赖其他测试
3. **避免复杂逻辑**：参数化测试应该保持简单，复杂逻辑应拆分为多个测试

---

## 🔄 迁移到新治理体系

### 为什么需要迁移？

新的治理体系（PR #330）实现了规则定义与测试逻辑的分离，带来以下好处：
- ✅ **规则信息集中管理**：修改规则描述只需更新 RuleSet
- ✅ **类型安全**：RuleSetRegistry 自动验证 RuleId 正确性
- ✅ **多工具复用**：RuleSet 可被测试、Analyzer、文档生成器等共享
- ✅ **一致性保证**：所有工具使用相同的规则定义

### 迁移步骤

#### 步骤 1：验证全局using

> **📌 注意**：ArchitectureTests项目已配置全局using（在`GlobalUsings.cs`中），包含了所有必要的命名空间，无需在测试文件中重复添加using语句。

#### 步骤 2：获取规则集和条款

```csharp
// ❌ 旧方式：硬编码规则信息
var ruleId = "ADR-002.1.1";
var summary = "Platform 不应依赖 Application";

// ✅ 新方式：从 RuleSetRegistry 获取
var ruleSet = RuleSetRegistry.GetStrict(2);
var clause = ruleSet.GetClause(1, 1);
var ruleId = clause.Id;        // "ADR-002.1.1"
var summary = clause.Condition; // 从 RuleSet 定义获取
```

#### 步骤 3：更新断言消息

```csharp
// ❌ 旧方式：手动拼接字符串
var message = 
    $"❌ ADR-002.1.1 违规：Platform 不应依赖 Application\n\n" +
    $"违规类型：\n{string.Join("\n", failingTypes)}\n\n" +
    // ... 更多手动拼接

// ✅ 新方式：结合 RuleSet 和 AssertionMessageBuilder
var message = AssertionMessageBuilder.BuildFromArchTestResult(
    ruleId: clause.Id,          // 从 RuleSet 获取
    summary: clause.Condition,   // 从 RuleSet 获取
    failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
    remediationSteps: new[]
    {
        "移除 Platform 对 Application 的引用",
        "将共享抽象提取到 Platform 层"
    },
    adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
```

#### 步骤 4：更新类注释

```csharp
/// <summary>
/// ADR-002.1: 依赖方向规则
/// ...
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md
/// - RuleSet: src/tests/ArchitectureTests/Specification/RuleSets/ADR002/Adr002RuleSet.cs  ✅ 添加这行
/// </summary>
```

### 迁移检查清单

在迁移测试文件时，请确保完成以下各项：

- [ ] **验证全局using**：确认GlobalUsings.cs已包含必要的命名空间
- [ ] **获取规则集**：使用 `RuleSetRegistry.GetStrict()`
- [ ] **获取条款**：使用 `ruleSet.GetClause()`
- [ ] **使用 clause.Id**：替代硬编码的 RuleId
- [ ] **使用 clause.Condition**：替代硬编码的描述
- [ ] **更新类注释**：添加 RuleSet 文件路径
- [ ] **删除硬编码常量**：移除测试类中的规则信息常量

### 完整示例：迁移前后对比

**迁移前 ❌**：
```csharp
namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_002;

public sealed class ADR_002_1_Architecture_Tests
{
    [Fact(DisplayName = "ADR-002.1.1: Platform 不应依赖 Application")]
    public void ADR_002_1_1_Platform_Should_Not_Depend_On_Application()
    {
        // 硬编码规则信息
        var ruleId = "ADR-002.1.1";
        var summary = "Platform 不应依赖 Application";
        
        var result = /* 执行测试 */;
        
        // 手动拼接断言消息
        var message = $"❌ {ruleId} 违规：{summary}\n\n...";
        result.Should().BeTrue(message);
    }
}
```

**迁移后 ✅**：
```csharp
namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_002;

/// <summary>
/// ADR-002.1: 依赖方向规则
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md
/// - RuleSet: src/tests/ArchitectureTests/Specification/RuleSets/ADR002/Adr002RuleSet.cs  ✅ 新增
/// </summary>
public sealed class ADR_002_1_Architecture_Tests
{
    [Fact(DisplayName = "ADR-002.1.1: Platform 不应依赖 Application")]
    public void ADR_002_1_1_Platform_Should_Not_Depend_On_Application()
    {
        // ✅ 从 RuleSetRegistry 获取规则信息
        var ruleSet = RuleSetRegistry.GetStrict(2);
        var clause = ruleSet.GetClause(1, 1);
        
        var result = /* 执行测试 */;
        
        // ✅ 使用 AssertionMessageBuilder + RuleSet 信息
        var message = AssertionMessageBuilder.BuildFromArchTestResult(
            ruleId: clause.Id,
            summary: clause.Condition,
            failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
            remediationSteps: new[] { "..." },
            adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
        
        result.Should().BeTrue(message);
    }
}
```

### 兼容性说明

✅ **向后兼容**：旧的测试仍然可以运行  
⚠️ **建议迁移**：新测试必须使用 RuleSetRegistry  
🎯 **迁移目标**：100% 测试使用 RuleSetRegistry

### 相关资源

- 📖 [迁移详细指南](../MIGRATION-ADR-TESTS-TO-RULESETS.md) - 完整的迁移过程文档
- 🏗️ [Specification README](../../src/tests/ArchitectureTests/Specification/README.md) - RuleSet 架构说明
- 📋 [ADR-907](../adr/governance/ADR-907-architecture-tests-enforcement-governance.md) - 执法治理体系

---

## 🛠️ 共享工具使用指南

### 0️⃣ RuleSetRegistry（新增 - P0 优先级）

**功能**：规则集统一访问入口

**核心方法**：

| 方法 | 说明 | 使用场景 |
|------|------|---------|
| `GetStrict(int)` | 获取规则集（严格模式） | **测试（推荐）** |
| `Get(int)` | 获取规则集（宽容模式） | 探索性查询 |
| `GetStrict(string)` | 字符串格式获取（严格） | 支持 "ADR-001" 格式 |
| `Get(string)` | 字符串格式获取（宽容） | 支持 "ADR-001" 格式 |
| `Contains(int)` | 检查是否存在 | 条件性测试 |
| `GetAllAdrNumbers()` | 获取所有编号 | 统计分析 |
| `GetConstitutionalRuleSets()` | 获取宪法层规则集（ADR-001~008） | 按分类测试 |
| `GetGovernanceRuleSets()` | 获取治理层规则集（ADR-900~999） | 按分类测试 |
| `GetRuntimeRuleSets()` | 获取运行时层规则集（ADR-201~240） | 按分类测试 |
| `GetStructureRuleSets()` | 获取结构层规则集（ADR-120~124） | 按分类测试 |
| `GetTechnicalRuleSets()` | 获取技术层规则集（ADR-301~360） | 按分类测试 |

**使用示例**：
```csharp
// 在测试中使用（推荐严格模式）
var ruleSet = RuleSetRegistry.GetStrict(2);
var clause = ruleSet.GetClause(1, 1);

// 使用规则信息
Console.WriteLine($"RuleId: {clause.Id}");           // "ADR-002.1.1"
Console.WriteLine($"条件: {clause.Condition}");      // 规则的具体内容
Console.WriteLine($"执行: {clause.Enforcement}");    // 如何执行

// 条件性测试（检查规则集是否存在）
if (RuleSetRegistry.Contains(999))
{
    var ruleSet = RuleSetRegistry.GetStrict(999);
    // 执行测试
}

// 按分类获取
var governanceRules = RuleSetRegistry.GetGovernanceRuleSets();
foreach (var rs in governanceRules)
{
    Console.WriteLine($"ADR-{rs.AdrNumber}");
}
```

**优势**：
- ✅ **类型安全**：自动验证 RuleId 正确性
- ✅ **集中管理**：规则信息统一维护
- ✅ **多工具复用**：可被测试、Analyzer、CI 共享

### 1️⃣ TestEnvironment（采用率目标 100%）

**功能**：提供仓库路径常量

| 属性 | 说明 | 示例 |
|------|------|------|
| `RepositoryRoot` | 仓库根目录 | `/path/to/repo` |
| `AdrPath` | ADR 文档目录 | `{root}/docs/adr` |
| `AgentFilesPath` | Agent 文件目录 | `{root}/.github/agents` |
| `ModulesPath` | 模块目录 | `{root}/src/modules` |

**使用方式**：
```csharp
var repoRoot = TestEnvironment.RepositoryRoot;
var adrPath = TestEnvironment.AdrPath;
```

### 2️⃣ 文件操作工具（采用率目标 80%）

> **重要更新（2026-02-09）**：FileSystemTestHelper 已重构为三个专用类，提升单一职责和性能。

**新的工具类结构**：

| 工具类 | 职责 | 主要方法 |
|--------|------|---------|
| **FileAssertionHelper** | 文件/目录断言 | `AssertFileExists()`, `AssertDirectoryExists()`, `AssertFileContains()` |
| **FileContentAnalyzer** | 内容分析 | `FileContainsAllKeywords()`, `CountPatternOccurrences()`, `FileContainsTable()` |
| **FileSearchHelper** | 文件搜索和路径 | `GetAdrFiles()`, `GetAbsolutePath()`, `GetRelativePath()` |
| **FileSystemTestHelper** | 向后兼容（已废弃）| ⚠️ 不推荐使用，使用上述专用类 |

**核心方法详解**：

#### FileAssertionHelper - 文件断言
```csharp
// 断言文件存在
FileAssertionHelper.AssertFileExists(path, "❌ 文件不存在");

// 断言目录存在
FileAssertionHelper.AssertDirectoryExists(dirPath, "❌ 目录不存在");

// 断言文件包含内容
FileAssertionHelper.AssertFileContains(path, "期望内容", "❌ 缺少必需内容");

// 断言文件内容长度
FileAssertionHelper.AssertFileContentLength(path, 100, "❌ 文件内容过短");
```

#### FileContentAnalyzer - 内容分析
```csharp
// 检查是否包含所有关键词（支持流式读取，性能优化）
bool hasAll = FileContentAnalyzer.FileContainsAllKeywords(
    path, 
    new[] { "关键词1", "关键词2" },
    ignoreCase: true);

// 检查是否包含表格
bool hasTable = FileContentAnalyzer.FileContainsTable(path, "表头");

// 统计模式出现次数（流式读取，适合大文件）
int count = FileContentAnalyzer.CountPatternOccurrences(path, @"ADR-\d{3,4}");
```

#### FileSearchHelper - 文件搜索
```csharp
// 获取 ADR 文件列表
var adrFiles = FileSearchHelper.GetAdrFiles();

// 获取 ADR 文件（指定子目录）
var govFiles = FileSearchHelper.GetAdrFiles(subfolder: "governance");

// 路径转换
var absPath = FileSearchHelper.GetAbsolutePath("docs/adr/ADR-001.md");
var relPath = FileSearchHelper.GetRelativePath(absPath);
```

**重构模式**：

| 场景 | 重构前 ❌ | 重构后 ✅ |
|------|----------|----------|
| 文件存在性检查 | `File.Exists(path).Should().BeTrue()` | `FileAssertionHelper.AssertFileExists(path, message)` |
| 读取文件内容 | `var content = File.ReadAllText(path);` | `var content = FileSearchHelper.ReadFileContent(path);` |
| 内容包含验证 | `File.ReadAllText(path).Should().Contain("text")` | `FileContentAnalyzer.AssertFileContains(path, "text", message)` |
| 获取 ADR 文件 | `Directory.GetFiles(adrPath, "*.md")` | `FileSearchHelper.GetAdrFiles()` |

**命名空间引用**：

所有工具类通过 `GlobalUsings.cs` 自动引入，无需显式 using：
```csharp
// ✅ 无需添加 using 语句，直接使用
FileAssertionHelper.AssertFileExists(path, message);
FileContentAnalyzer.FileContainsAllKeywords(path, keywords);
FileSearchHelper.GetAdrFiles();
```

**详细文档**：参见 [Shared/README.md](../../src/tests/ArchitectureTests/Shared/README.md)

### 3️⃣ AssertionMessageBuilder（采用率目标 80%）

**核心方法**：

| 方法 | 适用场景 | 输出格式 |
|------|---------|---------|
| `BuildFileNotFoundMessage()` | 文件不存在 | 包含预期路径 |
| `BuildContentMissingMessage()` | 内容缺失 | 包含预期内容 |
| `BuildFromArchTestResult()` | NetArchTest 失败 | 包含违规类型列表 |
| `BuildWithViolations()` | 多个违规项 | 包含违规列表 |

**使用示例**：
```csharp
var message = BuildFileNotFoundMessage(
    ruleId: "ADR-XXX_Y_Z",
    filePath: filePath,
    fileDescription: "配置文件",
    remediationSteps: new[] { "创建文件", "添加配置" },
    adrReference: "docs/adr/XXX.md");

File.Exists(filePath).Should().BeTrue(message);
```

### 4️⃣ AdrTestFixture（采用率目标 50%）

**功能**：ADR 文档缓存，避免重复加载

**使用方式**：
```csharp
public sealed class ADR_XXX_Tests : IClassFixture<AdrTestFixture>
{
    private readonly AdrTestFixture _fixture;
    
    public ADR_XXX_Tests(AdrTestFixture fixture) => _fixture = fixture;
    
    [Fact]
    public void Test_Method()
    {
        var adr = _fixture.GetAdr("ADR-XXX");  // 从缓存获取
    }
}
```

**性能收益**：测试执行速度提升 ~20%

---

## ✅ 迁移检查清单

### 📦 代码结构（P0 优先级）

| 检查项 | 要求 | 工具检测 |
|--------|------|---------|
| [ ] sealed 关键字 | 所有测试类必须 sealed | IDE 警告 |
| [ ] 类命名格式 | `ADR_XXX_Y_Architecture_Tests` | Code Review |
| [ ] 方法命名格式 | `ADR_XXX_Y_Z_<Description>` | Code Review |
| [ ] DisplayName 格式 | `"ADR-XXX_Y_Z: <中文描述>"` | 测试运行器 |
| [ ] **使用 RuleSetRegistry** | **从 Registry 获取规则信息** | **Code Review** |

### 📝 文档注释（P1 优先级）

| 检查项 | 要求 | 目标 |
|--------|------|------|
| [ ] 类 XML 注释 | 包含 Rule 说明、测试映射、关联文档 | 96%+ |
| [ ] 方法 XML 注释 | 包含 Clause 说明、§ 引用 | 90%+ |
| [ ] ADR 条款引用 | 使用 `§ADR-XXX_Y_Z` 格式 | 100% |
| [ ] **RuleSet 路径引用** | **添加 RuleSet 文件路径到类注释** | **100%** |

### 🔧 共享工具使用（P0-P2 优先级）

| 检查项 | 目标采用率 | 优先级 |
|--------|-----------|--------|
| [ ] **使用 RuleSetRegistry** | **100%** | **🔴 P0** |
| [ ] 使用 TestEnvironment | 100% | 🔴 P0 |
| [ ] 删除本地 FindRepositoryRoot | 100% | 🔴 P0 |
| [ ] 使用文件操作工具 | 80% | 🔴 P1 |
| [ ] 使用 AssertionMessageBuilder | 80% | 🔴 P1 |
| [ ] 使用 AdrTestFixture | 50% | 🟡 P2 |

### 📏 断言质量（P1 优先级）

**必需字段**：
- [ ] ❌ + RuleId 开头（使用 `clause.Id`）
- [ ] 当前状态字段
- [ ] 修复建议（编号列表）
- [ ] 参考字段（ADR 路径 + § 引用）

**质量检查**：
- [ ] 使用 `clause.Id` 作为 RuleId（不硬编码）
- [ ] 使用 `clause.Condition` 作为规则描述（不硬编码）
- [ ] RuleId 格式正确（下划线分隔）
- [ ] 问题描述清晰（一句话）
- [ ] 当前状态具体（含数据和事实）
- [ ] 修复步骤可操作（避免模糊）
- [ ] ADR 路径完整正确

---

## 📚 测试组织原则

### 目录结构

```
src/tests/ArchitectureTests/
├─ ADR-002/
│  ├─ ADR_002_1_Architecture_Tests.cs  # Rule 1
│  ├─ ADR_002_2_Architecture_Tests.cs  # Rule 2
│  └─ ADR_002_3_Architecture_Tests.cs  # Rule 3
├─ ADR-960/
│  ├─ ADR_960_1_Architecture_Tests.cs
│  └─ ADR_960_2_Architecture_Tests.cs
└─ Shared/
   ├─ Adr/                            # ADR 相关工具（11个）
   │  ├─ AdrCategoryClassifier.cs
   │  ├─ AdrParser.cs
   │  ├─ AdrRepository.cs
   │  └─ ...
   ├─ FileSystem/                     # 文件系统操作（4个）
   │  ├─ FileAssertionHelper.cs       # 文件断言
   │  ├─ FileContentAnalyzer.cs       # 内容分析（流式读取）
   │  ├─ FileSearchHelper.cs          # 文件搜索
   │  └─ FileSystemTestHelper.cs      # 向后兼容（已废弃）
   ├─ Assemblies/                     # 程序集加载（3个）
   │  ├─ AssemblyLoaderBase.cs
   │  ├─ ModuleAssemblyData.cs
   │  └─ HostAssemblyData.cs
   ├─ Testing/                        # 测试辅助（5个）
   │  ├─ TestEnvironment.cs           # 路径常量
   │  ├─ AssertionMessageBuilder.cs   # 断言消息
   │  ├─ NetArchTestHelper.cs         # NetArchTest 封装
   │  ├─ AdrTestFixture.cs            # ADR 缓存
   │  └─ TestConstants.cs             # 通用常量
   └─ README.md                       # 工具类使用指南
```

### 组织原则

| 原则 | 说明 |
|------|------|
| **一个 Rule 一个类** | 每个 ADR 的每个 Rule 对应一个测试类 |
| **按目录分组** | 同一 ADR 的测试放在同一子目录 |
| **Clause 同类聚合** | 一个 Rule 下的所有 Clause 测试在同一类中 |
| **sealed 禁止继承** | 所有测试类使用 sealed |

---

## ❓ 常见问题（FAQ）

### Q1：什么时候拆分测试类？
**A**：当 Rule 包含 Clause 超过 10 个时，考虑按功能子分类拆分。

### Q2：测试方法粒度？
**A**：一个方法验证一个 Clause。多个验证点可在同一方法中用多个断言。

### Q3：依赖多个 ADR 如何处理？
**A**：
- 放在主要验证的 ADR 测试类中
- 文档注释说明依赖关系
- 断言消息引用所有相关 ADR

### Q4：测试失败怎么办？
**A**：
1. 阅读错误信息的"修复建议"
2. 查看引用的 ADR 文档理解背景
3. 根据建议调整代码
4. 重新运行验证
5. 特殊情况按 ADR-900 破例流程处理

### Q5：如何选择合适的共享工具？

| 场景 | 推荐工具 | 优先级 |
|------|---------|--------|
| 获取仓库路径 | TestEnvironment | 🔴 必须 |
| 文件断言 | FileAssertionHelper | 🔴 必须 |
| 内容分析 | FileContentAnalyzer | 🔴 必须 |
| 文件搜索 | FileSearchHelper | 🔴 必须 |
| 构建断言消息 | AssertionMessageBuilder | 🔴 必须 |
| 加载 ADR 文档 | AdrTestFixture | 🟡 推荐 |
| 通用常量 | TestConstants | 🟡 推荐 |

---

## 🎯 行动计划

### 阶段 1：P0 基础巩固（1-2 天）

**目标**：消除基础重复，达到 100% 规范

- [ ] 消除剩余 2 个 FindRepositoryRoot（ADR_301、ADR_360）
- [ ] 补充缺失的 DisplayName
- [ ] 确保所有类使用 sealed

**验证**：运行全部测试，确保功能正常

### 阶段 2：P1 工具推广（1 周）

**目标**：FileSystemTestHelper 和 AssertionMessageBuilder 采用率达 50%

**FileSystemTestHelper**：
- [ ] 替换 20 个文件的 `File.ReadAllText`
- [ ] 替换 10 个文件的文件遍历逻辑
- [ ] 替换 9 个文件的路径拼接

**AssertionMessageBuilder**：
- [ ] 替换 20 个文件的文件存在性断言
- [ ] 替换 15 个文件的内容断言

**验证**：错误消息格式统一，包含完整信息

### 阶段 3：P2 性能优化（1 周）

**目标**：AdrTestFixture 采用率达 30%，测试速度提升 15%

- [ ] 为 15 个测试类添加 `IClassFixture<AdrTestFixture>`
- [ ] 删除重复的 ADR 加载代码
- [ ] 测量性能提升

**验证**：测试执行时间降低 15-20%

### 阶段 4：持续改进

**目标**：建立长期质量保障

- [ ] 每月更新采用率统计
- [ ] Code Review 强制检查共享工具使用
- [ ] 拒绝包含明显重复代码的 PR
- [ ] 补充新的共享工具（如 ValidationHelper）

---

## 📖 相关文档

| 文档 | 说明 |
|------|------|
| **[ADR 测试迁移到新治理体系指南](../MIGRATION-ADR-TESTS-TO-RULESETS.md)** | **完整的 RuleSet 迁移过程和方法** |
| **[Specification README](../../src/tests/ArchitectureTests/Specification/README.md)** | **RuleSet 架构设计和使用说明** |
| [架构测试分析报告](./ARCHITECTURE-TEST-ANALYSIS-REPORT.md) | 详细数据分析和统计 |
| [架构测试重构快速参考](./ARCHITECTURE-TEST-REFACTORING-REFERENCE.md) | 快速查阅的重构模式 |
| [断言消息模板使用指南](./ASSERTION-MESSAGE-TEMPLATE-USAGE.md) | AssertionMessageBuilder 详细说明 |
| [共享辅助工具 README](../../src/tests/ArchitectureTests/Shared/README.md) | 共享类 API 文档 |
| [架构测试 README](../../src/tests/ArchitectureTests/README.md) | 测试套件概览 |
| [ADR-900](../adr/governance/ADR-900-architecture-tests.md) | 架构测试与 CI 治理元规则 |
| [ADR-907](../adr/governance/ADR-907-architecture-tests-enforcement-governance.md) | ArchitectureTests 执法治理体系 |

---

## 📝 总结

### ✅ 核心价值

遵循本指南可以：
- 📉 减少代码重复 77%（4,300 → 1,000 行）
- 📈 提高代码一致性 60%
- 🚀 提升编写速度 40%
- 💰 降低维护成本 50%
- ⚡ 加快测试执行 20%

### 🎯 关键原则

1. **DRY**：使用共享工具，避免重复实现
2. **一致性**：统一命名、格式、断言消息
3. **可维护性**：清晰结构、完整文档、类型安全
4. **质量**：详细错误信息、可操作的修复建议

### ⚠️ 重要提醒

- ✅ 本文档是**指导性文档**，提供最佳实践建议
- ✅ **权威依据**仍然是 ADR 文档（docs/adr/）
- ✅ 如有冲突，以 ADR 正文为准
- ✅ 本文档会随着测试实践的演进持续更新

---

**文档维护**：架构委员会  
**最后更新**：2026-02-06  
**版本**：2.0

如有问题或建议，请通过 Issue 或 PR 提出。

---

## 🔄 版本历史

| 版本 | 日期 | 主要变更 |
|-----|------|---------|
| 3.0 | 2026-02-07 | **重大更新**：引入 RuleSet 治理体系，添加 RuleSetRegistry 使用指南，更新所有测试模板和示例 |
| 2.0 | 2026-02-06 | 重构文档结构：添加执行摘要、使用表格优化、层次化组织、优先级标记 |
| 1.1 | 2026-02-05 | 添加断言消息标准格式、共享工具说明 |
| 1.0 | 2026-01-XX | 初始版本，基于 133+ 测试文件分析 |
