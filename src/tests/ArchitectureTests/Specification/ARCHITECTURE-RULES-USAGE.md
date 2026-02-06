# ArchitectureRules 使用指南

## 概述

`ArchitectureTestSpecification.ArchitectureRules` 是从 ADR 文档迁移而来的强类型规则集定义，作为架构测试的唯一规范源。

## 为什么需要 ArchitectureRules？

### 迁移前的问题

**旧方式**：在测试中硬编码 RuleId 和规则描述

```csharp
[Fact(DisplayName = "ADR-001_1_1: 模块不应相互引用")]
public void ADR_001_1_1_Modules_Should_Not_Reference_Other_Modules()
{
    // 硬编码的 RuleId 和描述
    var ruleId = "ADR-001_1_1";
    var summary = "模块按业务能力独立划分";
    
    // 问题：
    // 1. 规则描述可能与 ADR 文档不一致
    // 2. 难以维护（需要手动同步 ADR 和测试）
    // 3. 无法验证 RuleId 是否存在
}
```

**新方式**：从 ArchitectureRules 获取规则定义

```csharp
[Fact(DisplayName = "ADR-001_1_1: 模块不应相互引用")]
public void ADR_001_1_1_Modules_Should_Not_Reference_Other_Modules()
{
    // 从 ArchitectureRules 获取规则定义
    var ruleSet = ArchitectureTestSpecification.ArchitectureRules.Adr001;
    var clause = ruleSet.GetClause(1, 1)!;
    
    // 优势：
    // 1. 规则描述来自单一规范源
    // 2. 自动验证（如果规则不存在，编译失败）
    // 3. 易于维护（只需更新 _ArchitectureRules.cs）
    
    // 使用规则进行测试...
    Assert.Equal("模块按业务能力独立划分", clause.Condition);
}
```

## 基本使用

### 1. 获取规则集

```csharp
// 方式 1：直接访问已知的规则集
var adr001 = ArchitectureTestSpecification.ArchitectureRules.Adr001;
var adr900 = ArchitectureTestSpecification.ArchitectureRules.Adr900;

// 方式 2：通过 ADR 编号获取
var ruleSet = ArchitectureTestSpecification.ArchitectureRules.GetRuleSet(907);

// 方式 3：获取所有规则集
var allRuleSets = ArchitectureTestSpecification.ArchitectureRules.GetAllRuleSets();
```

### 2. 获取规则（Rule）

```csharp
var ruleSet = ArchitectureTestSpecification.ArchitectureRules.Adr001;

// 检查规则是否存在
if (ruleSet.HasRule(1))
{
    var rule = ruleSet.GetRule(1)!;
    Console.WriteLine($"规则摘要: {rule.Summary}");
    Console.WriteLine($"严重程度: {rule.Severity}");
    Console.WriteLine($"作用域: {rule.Scope}");
}
```

### 3. 获取条款（Clause）

```csharp
var ruleSet = ArchitectureTestSpecification.ArchitectureRules.Adr001;

// 检查条款是否存在
if (ruleSet.HasClause(1, 1))
{
    var clause = ruleSet.GetClause(1, 1)!;
    Console.WriteLine($"条件: {clause.Condition}");
    Console.WriteLine($"执行要求: {clause.Enforcement}");
}
```

## 在架构测试中使用

### 示例 1：使用规则集验证模块隔离

```csharp
[Theory(DisplayName = "ADR-001_1_1: 模块不应相互引用")]
[ClassData(typeof(ModuleAssemblyData))]
public void ADR_001_1_1_Modules_Should_Not_Reference_Other_Modules(Assembly moduleAssembly)
{
    // 获取规则定义
    var ruleSet = ArchitectureTestSpecification.ArchitectureRules.Adr001;
    var clause = ruleSet.GetClause(1, 1)!;
    
    // 构建测试消息（引用规则定义）
    var message = $"""
        ❌ {clause.Id} 违规
        
        规则：{clause.Condition}
        执行要求：{clause.Enforcement}
        
        违规模块：{{moduleName}}
        """;
    
    // 执行测试...
    var result = Types
        .InAssembly(moduleAssembly)
        .ShouldNot()
        .HaveDependencyOn("Zss.BilliardHall.Modules.OtherModule")
        .GetResult();
        
    result.IsSuccessful.Should().BeTrue(message);
}
```

### 示例 2：验证事件命名规范

```csharp
[Theory(DisplayName = "ADR-120_1_1: 事件类型必须以 Event 后缀结尾")]
[ClassData(typeof(ModuleAssemblyData))]
public void Event_Types_Should_End_With_Event_Suffix(Assembly moduleAssembly)
{
    // 获取规则定义
    var ruleSet = ArchitectureTestSpecification.ArchitectureRules.Adr120;
    var clause = ruleSet.GetClause(1, 1)!;
    
    // 查找事件类型
    var eventTypes = Types
        .InAssembly(moduleAssembly)
        .That()
        .ResideInNamespaceContaining(".Events")
        .GetTypes();
    
    // 验证每个事件类型
    foreach (var eventType in eventTypes)
    {
        var message = $"""
            ❌ {clause.Id} 违规
            
            {clause.Condition}
            {clause.Enforcement}
            
            违规类型: {eventType.FullName}
            修复建议: 将类型重命名为 {eventType.Name}Event
            """;
            
        eventType.Name.Should().EndWith("Event", message);
    }
}
```

### 示例 3：使用规则集生成测试报告

```csharp
[Fact(DisplayName = "生成 ADR-001 规则覆盖报告")]
public void Generate_Adr001_Coverage_Report()
{
    var ruleSet = ArchitectureTestSpecification.ArchitectureRules.Adr001;
    
    var report = new StringBuilder();
    report.AppendLine($"# ADR-{ruleSet.AdrNumber:0000} 规则覆盖报告");
    report.AppendLine();
    report.AppendLine($"规则总数: {ruleSet.RuleCount}");
    report.AppendLine($"条款总数: {ruleSet.ClauseCount}");
    report.AppendLine();
    
    foreach (var rule in ruleSet.Rules)
    {
        report.AppendLine($"## {rule.Id}: {rule.Summary}");
        report.AppendLine($"- 严重程度: {rule.Severity}");
        report.AppendLine($"- 作用域: {rule.Scope}");
        report.AppendLine();
        
        // 获取该规则下的所有条款
        var clauses = ruleSet.Clauses
            .Where(c => c.Id.RuleNumber == rule.Id.RuleNumber)
            .OrderBy(c => c.Id.ClauseNumber);
            
        foreach (var clause in clauses)
        {
            report.AppendLine($"### {clause.Id}");
            report.AppendLine($"- **条件**: {clause.Condition}");
            report.AppendLine($"- **执行**: {clause.Enforcement}");
            report.AppendLine();
        }
    }
    
    Console.WriteLine(report.ToString());
}
```

## 高级用法

### 1. 枚举所有规则和条款

```csharp
// 获取所有已定义的 ADR 编号
var adrNumbers = ArchitectureTestSpecification.ArchitectureRules.GetAllAdrNumbers();

foreach (var adrNumber in adrNumbers)
{
    var ruleSet = ArchitectureTestSpecification.ArchitectureRules.GetRuleSet(adrNumber)!;
    
    Console.WriteLine($"ADR-{adrNumber:0000}: {ruleSet.RuleCount} 规则, {ruleSet.ClauseCount} 条款");
    
    // 列出所有规则
    foreach (var rule in ruleSet.Rules)
    {
        Console.WriteLine($"  - {rule.Id}: {rule.Summary}");
    }
}
```

### 2. 按严重程度过滤规则

```csharp
var allRuleSets = ArchitectureTestSpecification.ArchitectureRules.GetAllRuleSets();

// 获取所有宪法级规则
var constitutionalRules = allRuleSets
    .SelectMany(rs => rs.Rules)
    .Where(r => r.Severity == RuleSeverity.Constitutional)
    .ToList();

Console.WriteLine($"宪法级规则数量: {constitutionalRules.Count}");
```

### 3. 验证 RuleId 一致性

```csharp
/// <summary>
/// 验证测试方法名称中的 RuleId 与实际规则定义一致
/// </summary>
[Fact]
public void Test_Method_RuleIds_Should_Match_Specification()
{
    var testMethods = typeof(ADR_001_1_Architecture_Tests)
        .GetMethods()
        .Where(m => m.GetCustomAttribute<FactAttribute>() != null);
    
    foreach (var method in testMethods)
    {
        // 从方法名提取 RuleId（例如：ADR_001_1_1_...）
        var match = Regex.Match(method.Name, @"ADR_(\d+)_(\d+)_(\d+)_");
        if (match.Success)
        {
            var adr = int.Parse(match.Groups[1].Value);
            var rule = int.Parse(match.Groups[2].Value);
            var clause = int.Parse(match.Groups[3].Value);
            
            // 验证规则是否存在
            var ruleSet = ArchitectureTestSpecification.ArchitectureRules.GetRuleSet(adr);
            ruleSet.Should().NotBeNull($"ADR-{adr:0000} 应该在 ArchitectureRules 中定义");
            ruleSet!.HasClause(rule, clause).Should().BeTrue(
                $"测试方法 {method.Name} 引用的条款 ADR-{adr:0000}_{rule}_{clause} 应该存在");
        }
    }
}
```

## 迁移指南

### 步骤 1：识别现有测试中的硬编码规则

```csharp
// 查找所有包含硬编码 RuleId 的测试
grep -r "ADR-[0-9]" src/tests/ArchitectureTests/ADR_*/*.cs
```

### 步骤 2：将硬编码替换为 ArchitectureRules 引用

**Before**:
```csharp
var ruleId = "ADR-001_1_1";
var summary = "模块按业务能力独立划分";  // 可能与 ADR 文档不一致！
```

**After**:
```csharp
var clause = ArchitectureTestSpecification.ArchitectureRules.Adr001.GetClause(1, 1)!;
var ruleId = clause.Id.ToString();  // "ADR-0001.1.1"
var summary = clause.Condition;  // 来自单一规范源，保证一致性
```

### 步骤 3：验证迁移

```csharp
// 运行所有架构测试，确保迁移没有破坏现有功能
dotnet test --filter "FullyQualifiedName~ArchitectureTests"
```

## 扩展 ArchitectureRules

### 添加新的 ADR 规则集

编辑 `/src/tests/ArchitectureTests/Specification/_ArchitectureRules.cs`：

```csharp
/// <summary>
/// ADR-XXX：您的 ADR 标题
/// 描述
/// </summary>
public static ArchitectureRuleSet AdrXXX => LazyAdrXXX.Value;

private static readonly Lazy<ArchitectureRuleSet> LazyAdrXXX = new(() =>
{
    var ruleSet = new ArchitectureRuleSet(XXX);

    // 添加规则
    ruleSet.AddRule(
        ruleNumber: 1,
        summary: "规则摘要",
        severity: RuleSeverity.Constitutional,  // 或 Governance、Technical
        scope: RuleScope.Module);  // 或 Solution、Document、Test、Agent

    // 添加条款
    ruleSet.AddClause(
        ruleNumber: 1,
        clauseNumber: 1,
        condition: "条件描述",
        enforcement: "执行要求");

    return ruleSet;
});
```

### 更新辅助方法

```csharp
public static ArchitectureRuleSet? GetRuleSet(int adrNumber)
{
    return adrNumber switch
    {
        // ... 现有的 case
        XXX => AdrXXX,  // 添加新的 case
        _ => null
    };
}

public static IEnumerable<ArchitectureRuleSet> GetAllRuleSets()
{
    // ... 现有的 yield return
    yield return AdrXXX;  // 添加新的 yield
}
```

## 最佳实践

### 1. 始终使用 Specification 作为单一规范源

❌ **不要**：
```csharp
// 硬编码规则描述
var message = "模块不应相互引用";  // 可能过时或不准确
```

✅ **应该**：
```csharp
// 从 Specification 获取
var clause = ArchitectureTestSpecification.ArchitectureRules.Adr001.GetClause(1, 1)!;
var message = clause.Condition;  // 保证与 ADR 文档一致
```

### 2. 利用强类型避免错误

❌ **不要**：
```csharp
var ruleId = "ADR-001.1.1";  // 字符串，容易拼写错误
```

✅ **应该**：
```csharp
var ruleId = ArchitectureRuleId.Clause(1, 1, 1);  // 强类型，编译时检查
```

### 3. 保持 Specification 与 ADR 文档同步

- 当 ADR 文档更新时，立即更新 `_ArchitectureRules.cs`
- 使用自动化测试验证一致性
- 定期审查 Specification 与 ADR 文档的对齐情况

### 4. 使用惰性初始化提高性能

```csharp
// 规则集使用 Lazy<T> 初始化，只在首次访问时创建
var ruleSet = ArchitectureTestSpecification.ArchitectureRules.Adr001;  // 首次访问，创建实例
var ruleSet2 = ArchitectureTestSpecification.ArchitectureRules.Adr001; // 再次访问，返回同一实例
```

## 常见问题

### Q: ArchitectureRules 与 ADR 文档的关系是什么？

A: ArchitectureRules 是 ADR 文档的强类型镜像，作为"代码即规范"的实现。它从 ADR 文档中提取规则定义，转换为可执行的代码。

### Q: 为什么不直接解析 ADR Markdown 文件？

A: 
- **性能**：编译时类型检查比运行时解析快
- **可靠性**：强类型防止拼写错误和引用错误
- **IDE 支持**：自动完成、重构等功能
- **版本控制**：代码变更更容易追踪和审查

### Q: 如何处理 ADR 文档更新？

A:
1. 更新 ADR 文档
2. 更新 `_ArchitectureRules.cs` 中对应的规则集
3. 运行测试验证一致性
4. 提交代码和文档变更

### Q: 所有 ADR 都需要在 ArchitectureRules 中定义吗？

A: 不一定。优先为以下 ADR 创建规则集：
- 有自动化测试的 ADR
- 需要在代码中引用的 ADR
- 核心的宪法层和治理层 ADR

## 未来扩展

- **自动生成**：从 ADR Markdown 自动生成 ArchitectureRules
- **双向同步**：从 ArchitectureRules 生成 ADR 文档
- **可视化**：生成规则关系图和依赖图
- **Roslyn Analyzer**：将 ArchitectureRules 用于编译时检查
- **GitHub Copilot Agent**：Agent 直接引用 ArchitectureRules 进行代码审查

## 参考资料

- [ADR-907: ArchitectureTests 执法治理体系](../../docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md)
- [ADR-900: 架构测试与 CI 治理元规则](../../docs/adr/governance/ADR-900-architecture-tests.md)
- [ArchitectureRuleSet API 文档](./Rules/ArchitectureRuleSet.cs)
- [ArchitectureRuleId API 文档](./Rules/ArchitectureRuleId.cs)
