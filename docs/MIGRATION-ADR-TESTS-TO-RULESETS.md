# ADR 测试迁移到新治理体系指南

## 概述

本文档记录了将现有 ADR 测试迁移到 PR #330 引入的新治理体系的过程和方法。

## 背景

PR #330 引入了新的架构治理体系，核心理念是：
- **ADR ≠ Test ≠ Specification**（三者物理隔离）
- **Rule 是最小裁决单元**
- **测试只能"引用规则"，不能"定义规则"**

## 迁移内容

### 创建的 RuleSet 数量

- **总计**：43 个 RuleSet 定义
- **宪法层（Constitutional）**：ADR-001 ~ 008（8个）
- **结构层（Structure）**：ADR-120 ~ 124（5个）
- **运行时层（Runtime）**：ADR-201, 210, 220, 240（4个）
- **技术层（Technical）**：ADR-301, 340, 350, 360（4个）
- **治理层（Governance）**：ADR-900 ~ 990（22个）

### RuleSet 位置

所有 RuleSet 定义文件位于：
```
src/tests/ArchitectureTests/Specification/RuleSets/
├── ADR001/Adr001RuleSet.cs
├── ADR002/Adr002RuleSet.cs
├── ADR003/Adr003RuleSet.cs
├── ADR004/Adr004RuleSet.cs
├── ...
└── ADR990/Adr990RuleSet.cs
```

## RuleSet 结构说明

每个 RuleSet 遵循以下结构：

```csharp
namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR{编号};

/// <summary>
/// ADR-{编号}：{标题}
/// {描述}
/// </summary>
public sealed class Adr{编号}RuleSet : IArchitectureRuleSetDefinition
{
    public int AdrNumber => {编号};
    
    public ArchitectureRuleSet Define() => LazyRuleSet.Value;
    
    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet({编号});
        
        // Rule 1: ...
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "规则摘要",
            decision: DecisionLevel.Must,      // Must/MustNot/Should
            severity: RuleSeverity.Constitutional,  // Constitutional/Governance/Technical
            scope: RuleScope.Module);          // Module/Project/Repository/Test/Agent/Document
            
        // Clause 1.1: ...
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "条款条件",
            enforcement: "执行方式",
            executionType: ClauseExecutionType.Convention);  // Convention/StaticAnalysis/Runtime
            
        return ruleSet;
    });
}
```

## 使用 RuleSetRegistry

### 基本用法

```csharp
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Index;

// 获取指定 ADR 的规则集
var ruleSet = RuleSetRegistry.Get(1);
var ruleSet = RuleSetRegistry.Get("ADR-001");

// 获取所有已注册的 ADR 编号
var allAdrs = RuleSetRegistry.GetAllAdrNumbers();

// 按分类获取
var constitutional = RuleSetRegistry.GetConstitutionalRuleSets(); // ADR-001 ~ 008
var governance = RuleSetRegistry.GetGovernanceRuleSets();         // ADR-900 ~ 999
var runtime = RuleSetRegistry.GetRuntimeRuleSets();               // ADR-201 ~ 240
var structure = RuleSetRegistry.GetStructureRuleSets();           // ADR-120 ~ 124
```

### 在测试中使用

```csharp
[Fact]
public void ADR_001_1_1_模块按业务能力独立划分()
{
    // 通过 Registry 获取规则集
    var ruleSet = RuleSetRegistry.GetStrict(1);
    var clause = ruleSet.GetClause(1, 1);
    
    // 执行验证逻辑
    var result = ArchitectureTestHelper.ValidateModuleIsolation();
    
    // 使用规则信息构造失败消息
    Assert.True(result.IsSuccess, 
        $"违反 {clause.Id}：{clause.Condition}\n" +
        $"执行方式：{clause.Enforcement}");
}
```

### 宽容模式 vs 严格模式

- **Get() - 宽容模式**：不存在时返回 null（适用于探索性查询）
- **GetStrict() - 严格模式**：不存在时抛出异常（适用于测试/CI/Analyzer）

```csharp
// 探索性查询 - 使用宽容模式
var ruleSet = RuleSetRegistry.Get(999);  // 返回 null

// 测试/CI 场景 - 使用严格模式
var ruleSet = RuleSetRegistry.GetStrict(999);  // 抛出 InvalidOperationException
```

## 特殊情况处理

### ADR-907-A

ADR-907-A 是 ADR-907 的补充规范，它们共用编号 907。因此：
- ❌ 不需要创建独立的 ADR907A RuleSet
- ✅ 相关规则可以合并到 ADR-907 RuleSet 中
- ✅ 测试目录 `ADR_907_A/` 中的测试引用 ADR-907 的规则

## 验证迁移

### 编译验证

```bash
cd /home/runner/work/Zss.BilliardHall/Zss.BilliardHall
dotnet build src/tests/ArchitectureTests/ArchitectureTests.csproj
```

### 运行 RuleSetRegistry 测试

```bash
dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj \
  --filter "FullyQualifiedName~RuleSetRegistryTests"
```

### 验证所有 RuleSet 已注册

```csharp
// 在测试中验证
var allAdrs = RuleSetRegistry.GetAllAdrNumbers().ToList();
Console.WriteLine($"已注册的 ADR 数量: {allAdrs.Count}");
// 预期输出: 43
```

## 后续工作

### 1. 更新现有测试
将现有的 ADR 测试更新为使用 RuleSetRegistry 获取规则信息：

```csharp
// 旧方式 - 硬编码规则信息
Assert.True(condition, "违反 ADR-001_1_1: 模块不应相互引用");

// 新方式 - 从 Registry 获取规则信息
var clause = RuleSetRegistry.GetStrict(1).GetClause(1, 1);
Assert.True(condition, $"违反 {clause.Id}: {clause.Condition}");
```

### 2. 完善 RuleSet 定义
某些 RuleSet（特别是治理层）目前是框架性定义，需要根据实际测试文件逐步完善：
- 添加更详细的规则描述
- 补充遗漏的 Clause
- 调整 DecisionLevel 和 Severity

### 3. 建立映射关系
创建 RuleSet ↔ 测试方法 ↔ ADR 文档的三向映射：
- 文档生成器：从 RuleSet 生成 ADR 文档
- 测试生成器：从 RuleSet 生成测试骨架
- 一致性验证：确保三者保持同步

### 4. 实现自动化工具
- **RuleSet 验证器**：验证 RuleSet 定义的完整性
- **ADR 文档同步工具**：检查 ADR 文档与 RuleSet 的一致性
- **测试覆盖率分析**：确保每个 Rule/Clause 都有对应的测试

## 参考文档

- [ADR-900: 架构测试与 CI 治理元规则](./adr/governance/ADR-900-architecture-tests.md)
- [ADR-905: 执行级别分类](./adr/governance/ADR-905-enforcement-level-classification.md)
- [ADR-907: ArchitectureTests 执法治理体系](./adr/governance/ADR-907-architecture-tests-enforcement-governance.md)
- [ADR-907-A: RuleId 格式规范](./adr/governance/ADR-907-a-alignment-checklist.md)
- [Specification README](../src/tests/ArchitectureTests/Specification/README.md)

## 常见问题

### Q: 为什么要创建 RuleSet？
A: RuleSet 实现了规则定义与测试逻辑的分离，使得规则可以被多种工具使用（测试、Analyzer、文档生成器、Copilot Agent 等）。

### Q: 如何处理编号冲突（如 ADR-907-A）？
A: 补充规范（-A, -B 等）应该合并到主 ADR 的 RuleSet 中，不需要创建独立的 RuleSet。

### Q: RuleSet 定义是否需要与测试完全同步？
A: 是的。RuleSet 应该准确反映实际的测试覆盖范围。发现不一致时应及时更新。

### Q: 如何验证 RuleSet 是否正确注册？
A: 运行 RuleSetRegistry 测试，或使用以下代码：
```csharp
bool isRegistered = RuleSetRegistry.Contains(1);
var allAdrs = RuleSetRegistry.GetAllAdrNumbers();
```

## 迁移检查清单

- [x] 创建所有缺失的 RuleSet 定义
- [x] 修复编号冲突问题
- [x] 验证编译通过
- [x] 验证 RuleSetRegistry 测试通过
- [ ] 更新现有测试使用 RuleSetRegistry
- [ ] 运行完整测试套件
- [ ] 完善治理层 RuleSet 定义
- [ ] 创建 RuleSet 验证工具
- [ ] 更新 CI 流程
- [ ] 更新文档

## 贡献者

- 迁移执行：Copilot Agent
- 审查：待定
- 日期：2026-02-07
