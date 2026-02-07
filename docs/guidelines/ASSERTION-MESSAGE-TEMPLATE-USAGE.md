# 断言消息模板使用指南

> **文档版本**: 1.1  
> **最后更新**: 2026-02-07  
> **相关文档**: [ARCHITECTURE-TEST-GUIDELINES.md](./ARCHITECTURE-TEST-GUIDELINES.md)

## 概述

`AssertionMessageBuilder` 是一个统一的断言消息构建器，位于 `Shared/AssertionMessageBuilder.cs`，提供标准化的断言消息模板。通过使用这个助手类，可以确保所有架构测试的错误消息保持一致性，并且便于维护。

> **🆕 版本 1.1 更新**（2026-02-07）：
> - 建议结合 **RuleSetRegistry** 使用，从规则集中获取 RuleId 和规则描述
> - 详见：[MIGRATION-ADR-TESTS-TO-RULESETS.md](../MIGRATION-ADR-TESTS-TO-RULESETS.md)

## 为什么要使用模板？

### 使用模板之前的问题

```csharp
// ❌ 旧方式：手动拼接字符串，容易出错且不一致
result.IsSuccessful.Should().BeTrue(
    $"❌ ADR-002_1_1 违规: Platform 层不应依赖 Application 层\n\n" +
    $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
    $"修复建议：\n" +
    $"1. 移除 Platform 对 Application 的引用\n" +
    $"2. 将共享的技术抽象提取到 Platform 层\n" +
    $"3. 确保依赖方向正确: Host → Application → Platform\n\n" +
    $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
```

**问题**：
- 字符串拼接冗长且容易出错
- 格式不一致（如冒号使用、空行数量）
- 维护困难：如果要修改格式，需要修改所有测试
- **硬编码规则信息**：RuleId 和描述直接写在代码中
- 可读性差：测试逻辑被大量字符串拼接淹没

### 使用模板之后的优势

```csharp
// ✅ 新方式：使用模板，简洁且一致
var message = AssertionMessageBuilder.BuildFromArchTestResult(
    ruleId: "ADR-002_1_1",
    summary: "Platform 层不应依赖 Application 层",
    failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
    remediationSteps: new[]
    {
        "移除 Platform 对 Application 的引用",
        "将共享的技术抽象提取到 Platform 层",
        "确保依赖方向正确: Host → Application → Platform"
    },
    adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");

result.IsSuccessful.Should().BeTrue(message);
```

```csharp
// ✅✅ 最佳方式：结合 RuleSetRegistry（v3.0 推荐）
// 从 RuleSetRegistry 获取规则信息
var ruleSet = RuleSetRegistry.GetStrict(2);
var clause = ruleSet.GetClause(1, 1);

var message = AssertionMessageBuilder.BuildFromArchTestResult(
    ruleId: clause.Id,              // 从 RuleSet 获取
    summary: clause.Condition,       // 从 RuleSet 获取
    failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
    remediationSteps: new[]
    {
        "移除 Platform 对 Application 的引用",
        "将共享的技术抽象提取到 Platform 层",
        "确保依赖方向正确: Host → Application → Platform"
    },
    adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");

result.IsSuccessful.Should().BeTrue(message);
```

**优势**：
- ✅ 代码简洁清晰
- ✅ 格式自动统一
- ✅ 集中维护：修改格式只需改一处
- ✅ 类型安全：参数明确，不易出错
- ✅ 测试逻辑更清晰
- ✅ **结合 RuleSetRegistry**：规则信息统一管理，避免硬编码

---

## 可用的模板方法

`AssertionMessageBuilder` 提供了多个方法来适应不同的测试场景：

### 1. BuildFromArchTestResult（推荐）

**适用场景**：NetArchTest 架构测试（最常用）

**方法签名**：
```csharp
public static string BuildFromArchTestResult(
    string ruleId,
    string summary,
    IEnumerable<string?>? failingTypeNames,
    IEnumerable<string> remediationSteps,
    string adrReference)
```

**使用示例**：
```csharp
[Fact(DisplayName = "ADR-002_1_1: Platform 不应依赖 Application")]
public void ADR_002_1_1_Platform_Should_Not_Depend_On_Application()
{
    // ✅ 使用 RuleSetRegistry 获取规则信息（v3.0 推荐）
    var ruleSet = RuleSetRegistry.GetStrict(2);
    var clause = ruleSet.GetClause(1, 1);
    
    var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
    var result = Types
        .InAssembly(platformAssembly)
        .ShouldNot()
        .HaveDependencyOn("Zss.BilliardHall.Application")
        .GetResult();

    var message = AssertionMessageBuilder.BuildFromArchTestResult(
        ruleId: clause.Id,           // 从 RuleSet 获取，不硬编码
        summary: clause.Condition,    // 从 RuleSet 获取，不硬编码
        failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
        remediationSteps: new[]
        {
            "移除 Platform 对 Application 的引用",
            "将共享的技术抽象提取到 Platform 层",
            "确保依赖方向正确: Host → Application → Platform"
        },
        adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");

    result.IsSuccessful.Should().BeTrue(message);
}
```

**生成的消息格式**：
```
❌ ADR-002_1_1 违规：Platform 层不应依赖 Application 层

当前状态：违规类型：
  - Zss.BilliardHall.Platform.SomeType

修复建议：
1. 移除 Platform 对 Application 的引用
2. 将共享的技术抽象提取到 Platform 层
3. 确保依赖方向正确: Host → Application → Platform

参考：docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md
```

---

### 2. BuildSimple（简化版）

**适用场景**：简单的检查（如文件存在性检查、单一条件验证）

**方法签名**：
```csharp
public static string BuildSimple(
    string ruleId,
    string summary,
    string currentState,
    string remediation,
    string adrReference)
```

**使用示例**：
```csharp
[Fact(DisplayName = "ADR-004_1_1: CPM 配置文件必须存在")]
public void ADR_004_1_1_Central_Package_Management_File_Must_Exist()
{
    var repoRoot = TestEnvironment.RepositoryRoot;
    var cpmFile = Path.Combine(repoRoot, "Directory.Packages.props");

    var message = AssertionMessageBuilder.BuildSimple(
        ruleId: "ADR-004_1_1",
        summary: "仓库根目录必须存在 Directory.Packages.props 文件",
        currentState: $"文件不存在：{cpmFile}",
        remediation: "在仓库根目录创建 Directory.Packages.props 文件并配置 CPM",
        adrReference: "docs/adr/constitutional/ADR-004-Cpm-Final.md");

    File.Exists(cpmFile).Should().BeTrue(message);
}
```

**生成的消息格式**：
```
❌ ADR-004_1_1 违规：仓库根目录必须存在 Directory.Packages.props 文件

当前状态：文件不存在：/path/to/Directory.Packages.props

修复建议：
1. 在仓库根目录创建 Directory.Packages.props 文件并配置 CPM

参考：docs/adr/constitutional/ADR-004-Cpm-Final.md
```

---

### 3. Build（标准格式）

**适用场景**：需要完全控制消息内容的场景

**方法签名**：
```csharp
public static string Build(
    string ruleId,
    string summary,
    string currentState,
    IEnumerable<string> remediationSteps,
    string adrReference,
    bool includeClauseReference = false)
```

**使用示例**：
```csharp
[Fact(DisplayName = "ADR-001_1_1: 模块不应相互引用")]
public void ADR_001_1_1_Modules_Should_Not_Reference_Other_Modules()
{
    // ... 测试逻辑 ...
    
    var message = AssertionMessageBuilder.Build(
        ruleId: "ADR-001_1_1",
        summary: "模块 Members 不应依赖模块 Orders",
        currentState: "发现跨模块引用：Members.CreateMember → Orders.GetOrder",
        remediationSteps: new[]
        {
            "使用领域事件进行异步通信",
            "使用数据契约进行只读查询",
            "传递原始类型（Guid、string）而非领域对象"
        },
        adrReference: "docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md",
        includeClauseReference: true);  // 包含 §ADR-001_1_1 引用

    result.IsSuccessful.Should().BeTrue(message);
}
```

---

### 4. BuildWithAnalysis（包含问题分析）

**适用场景**：需要解释问题背景和影响的复杂场景

**方法签名**：
```csharp
public static string BuildWithAnalysis(
    string ruleId,
    string summary,
    string currentState,
    string problemAnalysis,
    IEnumerable<string> remediationSteps,
    string adrReference,
    bool includeClauseReference = false)
```

**使用示例**：
```csharp
[Fact(DisplayName = "ADR-120_1_2: 事件名称必须使用动词过去式")]
public void Event_Names_Should_Use_Past_Tense_Verbs()
{
    // ... 测试逻辑 ...
    
    var message = AssertionMessageBuilder.BuildWithAnalysis(
        ruleId: "ADR-120_1_2",
        summary: "事件名称未使用动词过去式",
        currentState: $"违规事件：{eventType.FullName}",
        problemAnalysis: 
            "事件名称必须使用动词过去式，因为事件描述的是已发生的业务事实。\n" +
            "使用现在时或进行时会导致事件与命令混淆，造成概念污染。",
        remediationSteps: new[]
        {
            "将动词改为过去式形式（Creating → Created）",
            "确保命名遵循模式：{AggregateRoot}{Action}Event",
            "示例：OrderCreatedEvent, MemberUpgradedEvent"
        },
        adrReference: "docs/adr/structure/ADR-120-domain-event-naming-convention.md",
        includeClauseReference: true);

    true.Should().BeFalse(message);
}
```

**生成的消息格式**：
```
❌ ADR-120_1_2 违规：事件名称未使用动词过去式

当前状态：违规事件：Zss.BilliardHall.Orders.Events.OrderCreatingEvent

问题分析：
事件名称必须使用动词过去式，因为事件描述的是已发生的业务事实。
使用现在时或进行时会导致事件与命令混淆，造成概念污染。

修复建议：
1. 将动词改为过去式形式（Creating → Created）
2. 确保命名遵循模式：{AggregateRoot}{Action}Event
3. 示例：OrderCreatedEvent, MemberUpgradedEvent

参考：docs/adr/structure/ADR-120-domain-event-naming-convention.md §ADR-120_1_2
```

---

### 5. BuildWithViolations（包含违规类型列表）

**适用场景**：需要列举多个违规项的场景

**方法签名**：
```csharp
public static string BuildWithViolations(
    string ruleId,
    string summary,
    IEnumerable<string> failingTypes,
    IEnumerable<string> remediationSteps,
    string adrReference,
    bool includeClauseReference = false)
```

**使用示例**：
```csharp
[Fact(DisplayName = "ADR-003_2_1: 模块不应直接访问其他模块的数据库")]
public void Modules_Should_Not_Access_Other_Module_Database()
{
    // ... 测试逻辑 ...
    var violations = new List<string>
    {
        "Members.MemberRepository → Orders.OrdersDbContext",
        "Orders.OrderService → Members.MembersDbContext"
    };
    
    var message = AssertionMessageBuilder.BuildWithViolations(
        ruleId: "ADR-003_2_1",
        summary: "模块不应直接访问其他模块的数据库",
        failingTypes: violations,
        remediationSteps: new[]
        {
            "移除跨模块的直接数据库访问",
            "使用数据契约（Data Contract）进行跨模块查询",
            "使用领域事件同步数据"
        },
        adrReference: "docs/adr/constitutional/ADR-003-module-data-isolation.md");

    violations.Should().BeEmpty(message);
}
```

---

## 最佳实践

### 1. 选择合适的方法

| 场景 | 推荐方法 | 原因 |
|------|---------|------|
| NetArchTest 测试 | `BuildFromArchTestResult` | 自动处理类型列表 |
| 文件/目录检查 | `BuildSimple` | 简单场景无需复杂格式 |
| 需要解释背景 | `BuildWithAnalysis` | 包含问题分析字段 |
| 自定义违规列表 | `BuildWithViolations` | 灵活控制违规项 |
| 完全自定义 | `Build` | 最大灵活性 |

### 2. 编写清晰的修复建议

**❌ 不好的修复建议**：
```csharp
remediationSteps: new[] 
{ 
    "修复问题",  // 太模糊
    "检查代码"   // 没有具体行动
}
```

**✅ 好的修复建议**：
```csharp
remediationSteps: new[]
{
    "移除 Platform 对 Application 的引用",  // 具体明确
    "将共享的技术抽象提取到 Platform 层",  // 可操作
    "确保依赖方向正确: Host → Application → Platform"  // 包含示例
}
```

### 3. 使用命名参数

**✅ 推荐**：使用命名参数提高可读性
```csharp
var message = AssertionMessageBuilder.BuildSimple(
    ruleId: "ADR-004_1_1",
    summary: "文件必须存在",
    currentState: $"文件不存在：{path}",
    remediation: "创建配置文件",
    adrReference: "docs/adr/constitutional/ADR-004-Cpm-Final.md");
```

### 4. 保持参考路径一致

所有 ADR 参考路径都应该：
- 使用相对于仓库根目录的路径
- 使用正斜杠 `/`
- 不包含 `./` 前缀

**✅ 正确**：
```csharp
adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md"
```

**❌ 错误**：
```csharp
adrReference: "./docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md"
adrReference: "docs\\adr\\constitutional\\ADR-002-platform-application-host-bootstrap.md"
```

---

## 迁移指南

### 从旧格式迁移到模板

**步骤 1**：识别当前的断言消息格式
```csharp
// 旧代码
result.IsSuccessful.Should().BeTrue(
    $"❌ ADR-002_1_1 违规: Platform 层不应依赖 Application 层\n\n" +
    $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
    $"修复建议：\n" +
    $"1. 移除 Platform 对 Application 的引用\n" +
    $"2. 将共享的技术抽象提取到 Platform 层\n" +
    $"3. 确保依赖方向正确: Host → Application → Platform\n\n" +
    $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
```

**步骤 2**：提取各个部分
- `ruleId`: "ADR-002_1_1"
- `summary`: "Platform 层不应依赖 Application 层"
- `failingTypeNames`: `result.FailingTypes?.Select(t => t.FullName)`
- `remediationSteps`: 三个步骤
- `adrReference`: ADR 文档路径

**步骤 3**：使用模板重写
```csharp
// 新代码
var message = AssertionMessageBuilder.BuildFromArchTestResult(
    ruleId: "ADR-002_1_1",
    summary: "Platform 层不应依赖 Application 层",
    failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
    remediationSteps: new[]
    {
        "移除 Platform 对 Application 的引用",
        "将共享的技术抽象提取到 Platform 层",
        "确保依赖方向正确: Host → Application → Platform"
    },
    adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");

result.IsSuccessful.Should().BeTrue(message);
```

---

## 常见问题（FAQ）

### Q1: 为什么要使用模板而不是直接拼接字符串？

**A**: 
1. **一致性**：确保所有测试使用相同的格式
2. **可维护性**：修改格式只需改一处
3. **可读性**：测试代码更简洁清晰
4. **减少错误**：避免拼接错误（如忘记换行、冒号格式不一致）

### Q2: 如果需要自定义格式怎么办？

**A**: 可以使用 `Build` 方法，它提供最大的灵活性。如果确实需要完全自定义的格式，可以扩展 `AssertionMessageBuilder` 类添加新方法。

### Q3: 是否必须迁移所有现有测试？

**A**: 不是强制的，但强烈建议迁移。可以逐步迁移：
1. 新测试必须使用模板
2. 修改现有测试时顺便迁移
3. 有时间时批量迁移

### Q4: 如何处理多语言场景？

**A**: 当前模板只支持中文。如果未来需要支持多语言，可以：
1. 在 `AssertionMessageBuilder` 中添加语言参数
2. 使用资源文件管理文本
3. 保持方法签名不变

### Q5: 模板性能如何？

**A**: 模板使用 `StringBuilder` 构建字符串，性能很好。即使在大量测试中使用也不会有性能问题。

---

## 总结

使用 `AssertionMessageBuilder` 模板系统的好处：

1. ✅ **统一格式**：所有断言消息格式一致
2. ✅ **易于维护**：集中管理，修改一处生效全局
3. ✅ **减少错误**：避免手动拼接的错误
4. ✅ **提高效率**：减少重复代码，提高开发速度
5. ✅ **更好的可读性**：测试代码更清晰简洁

**记住**：好的错误消息是测试的一部分。使用统一的模板，让架构测试更专业、更易维护！
