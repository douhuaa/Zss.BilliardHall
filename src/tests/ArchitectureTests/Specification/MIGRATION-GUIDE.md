# 架构规范重组迁移指南

## 背景

本次重组将架构测试规范从"单文件堆砌模式"升级为"三层分离架构"，以支持未来 100+ ADR 的扩展需求。

## 重组概览

### 变更前（旧结构）

```
/Specification
├── _ArchitectureRules.cs           # 所有规则定义堆在一个文件
├── _DecisionLanguage.cs
└── /Rules
    └── ArchitectureRuleSet.cs
```

### 变更后（新结构）

```
/Specification
├── ArchitectureTestSpecification.cs    # 根聚合
├── _ArchitectureRules.cs                # 向后兼容层（轻量化）
├── _DecisionLanguage.cs
│
├── /DecisionLanguage                    # 语义宪法层
│   ├── DecisionLevel.cs
│   ├── DecisionRule.cs
│   └── DecisionResult.cs
│
├── /RuleSets                            # 规则集（按 ADR 独立）
│   ├── /ADR0001
│   │   └── Adr0001RuleSet.cs
│   ├── /ADR0002
│   │   └── Adr0002RuleSet.cs
│   └── ...
│
├── /Index                               # 索引访问层
│   ├── RuleSetRegistry.cs
│   └── AdrRuleIndex.cs
│
└── /Rules                               # 规则基础设施
    └── ...
```

## 破坏性变更

### ✅ 无破坏性变更

本次重组**完全向后兼容**，所有现有代码无需修改即可继续工作：

```csharp
// 旧 API 仍然可用
var ruleSet = ArchitectureTestSpecification.ArchitectureRules.Adr001;
```

### 🔄 推荐迁移

虽然不强制，但建议新代码使用新 API：

```csharp
// 推荐：使用 Registry
var ruleSet = RuleSetRegistry.Get(1);
```

## 迁移步骤

### 步骤 1：了解新 API

#### 旧方式（仍然有效）

```csharp
using static Zss.BilliardHall.Tests.ArchitectureTests.Specification.ArchitectureTestSpecification;

// 直接访问规则集
var adr001 = ArchitectureRules.Adr001;
var adr900 = ArchitectureRules.Adr900;

// 通过 GetRuleSet
var adr907 = ArchitectureRules.GetRuleSet(907);
```

#### 新方式（推荐）

```csharp
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Index;

// 通过 Registry 获取
var adr001 = RuleSetRegistry.Get(1);
var adr900 = RuleSetRegistry.Get("ADR-900");

// 直接访问 RuleSet 静态属性
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0001;
var adr001 = Adr0001RuleSet.RuleSet;
```

### 步骤 2：更新测试代码（可选）

#### 示例 1：简单规则访问

**旧代码**：
```csharp
[Fact]
public void Test_ADR_001_Rule_1()
{
    var ruleSet = ArchitectureTestSpecification.ArchitectureRules.Adr001;
    var rule = ruleSet.GetRule(1);
    
    // 测试逻辑...
}
```

**新代码**：
```csharp
[Fact]
public void Test_ADR_001_Rule_1()
{
    // 方式 1：通过 Registry
    var ruleSet = RuleSetRegistry.Get(1);
    var rule = ruleSet.GetRule(1);
    
    // 方式 2：直接使用 RuleSet
    var rule = Adr0001RuleSet.RuleSet.GetRule(1);
    
    // 测试逻辑...
}
```

#### 示例 2：规则查询

**新增功能**（旧方式不支持）：
```csharp
// 按严重程度查询
var constitutionalRules = RuleSetRegistry.GetBySeverity(RuleSeverity.Constitutional);

// 按作用域查询
var moduleRules = RuleSetRegistry.GetByScope(RuleScope.Module);

// 按分类查询
var governanceRules = RuleSetRegistry.GetGovernanceRuleSets();
```

#### 示例 3：规则索引

**新增功能**：
```csharp
// 快速查找规则
var rule = AdrRuleIndex.GetRule("ADR-001_1");

// 快速查找条款
var clause = AdrRuleIndex.GetClause("ADR-001_1_1");

// 验证 RuleId 是否存在
bool exists = AdrRuleIndex.RuleExists("ADR-001_1");
```

### 步骤 3：添加新的 ADR 规则集

#### 旧方式（不再推荐）

在 `_ArchitectureRules.cs` 中添加 Lazy 属性和初始化逻辑。

#### 新方式（推荐）

1. 创建新目录和文件：

```bash
mkdir -p /RuleSets/ADR0XXX
touch /RuleSets/ADR0XXX/AdrXXXRuleSet.cs
```

2. 定义规则集：

```csharp
namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0XXX;

using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

public static class AdrXXXRuleSet
{
    public const int AdrNumber = XXX;
    
    public static ArchitectureRuleSet RuleSet => LazyRuleSet.Value;
    
    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(AdrNumber);
        
        // 添加规则...
        
        return ruleSet;
    });
}
```

3. 在 `RuleSetRegistry.cs` 中注册：

```csharp
private static IReadOnlyDictionary<int, ArchitectureRuleSet> BuildRegistry()
{
    var registry = new Dictionary<int, ArchitectureRuleSet>();
    
    // ... 现有注册 ...
    
    // 添加新规则集
    Register(registry, AdrXXXRuleSet.AdrNumber, AdrXXXRuleSet.RuleSet);
    
    return registry;
}
```

4. （可选）在 `_ArchitectureRules.cs` 中添加向后兼容属性：

```csharp
public static class ArchitectureRules
{
    // ...
    
    /// <summary>
    /// ADR-XXX：规则集名称
    /// ⚠️ 向后兼容属性，新代码请使用 RuleSetRegistry.Get(XXX)
    /// </summary>
    public static ArchitectureRuleSet AdrXXX => AdrXXXRuleSet.RuleSet;
}
```

## 常见迁移场景

### 场景 1：测试中使用规则集

**现状**：
```csharp
var ruleSet = ArchitectureTestSpecification.ArchitectureRules.Adr001;
```

**迁移建议**：
- 无需立即迁移
- 新测试使用 `RuleSetRegistry.Get(1)`

### 场景 2：遍历所有规则集

**现状**：
```csharp
var allRuleSets = ArchitectureTestSpecification.ArchitectureRules.GetAllRuleSets();
```

**迁移建议**：
```csharp
// 新方式
var allRuleSets = RuleSetRegistry.GetAllRuleSets();
```

### 场景 3：检查规则是否存在

**现状**：
```csharp
var ruleSet = ArchitectureTestSpecification.ArchitectureRules.GetRuleSet(907);
if (ruleSet != null)
{
    // ...
}
```

**迁移建议**：
```csharp
// 新方式 1：直接检查
if (RuleSetRegistry.Contains(907))
{
    var ruleSet = RuleSetRegistry.Get(907);
    // ...
}

// 新方式 2：使用 null 检查
var ruleSet = RuleSetRegistry.Get(907);
if (ruleSet != null)
{
    // ...
}
```

### 场景 4：按类型筛选规则

**现状**：
```csharp
// 需要手动过滤
var allRuleSets = ArchitectureTestSpecification.ArchitectureRules.GetAllRuleSets();
var governanceRules = allRuleSets
    .Where(rs => rs.AdrNumber >= 900 && rs.AdrNumber <= 999)
    .ToList();
```

**迁移建议**：
```csharp
// 新方式：使用内置方法
var governanceRules = RuleSetRegistry.GetGovernanceRuleSets();
```

## 验证迁移

### 编译时验证

```bash
dotnet build src/tests/ArchitectureTests/ArchitectureTests.csproj
```

应该无任何错误。警告可以暂时忽略。

### 运行时验证

```bash
dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj --filter "FullyQualifiedName~Specification"
```

所有测试应该通过。

### 手动验证

```csharp
// 验证所有规则集都已正确注册
var expectedAdrs = new[] { 1, 2, 3, 120, 201, 900, 907 };
var registeredAdrs = RuleSetRegistry.GetAllAdrNumbers().ToArray();

Assert.Equal(expectedAdrs.Length, registeredAdrs.Length);
foreach (var adr in expectedAdrs)
{
    Assert.True(RuleSetRegistry.Contains(adr), $"ADR-{adr} 未注册");
}
```

## 回滚方案

如果遇到问题需要回滚：

1. **不需要回滚代码**：旧 API 仍然可用
2. **仅需删除新文件**：
   - `/RuleSets/*`
   - `/Index/*`
3. **恢复 `_ArchitectureRules.cs`**：从 git 历史恢复原始版本

但由于本次重组完全向后兼容，理论上不需要回滚。

## 时间线

- **第一阶段（已完成）**：核心重组，保持向后兼容
- **第二阶段（可选）**：逐步迁移现有测试到新 API
- **第三阶段（未来）**：废弃旧 API，完全切换到新架构

目前处于第一阶段，旧代码无需修改。

## 需要帮助？

如有疑问，请参考：
- [Specification/README.md](./README.md) - 完整架构说明
- [ADR-907: ArchitectureTests 执法治理体系](../../../docs/adr/ADR-907.md) - 测试组织规范
- 或提交 Issue 询问
