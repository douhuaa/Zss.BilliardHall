# Architecture Test Specification

## 概述

本目录包含架构测试的规范定义，是整个架构治理体系的核心。通过将 ADR 文档转换为可执行的规范，实现"把 ADR 变成可执行规范"的目标。

## 设计原则

### 三条铁律

1. **ADR ≠ Test ≠ Specification**（三者必须物理隔离）
   - ADR 是文档，定义"规则是什么"
   - Specification 是规范，定义"规则如何表达"
   - Test 是测试，定义"规则如何验证"

2. **Rule 是最小裁决单元，ADR 只是容器**
   - 每个规则（Rule）和条款（Clause）都有唯一标识
   - ADR 文档是规则的逻辑分组，不是裁决单元

3. **测试只能"引用规则"，不能"定义规则"**
   - 测试通过 RuleSetRegistry 获取规则定义
   - 禁止在测试中硬编码规则内容

## 目录结构

```
/Specification
├── ArchitectureTestSpecification.cs    # 根聚合（统一入口）
├── _ArchitectureRules.cs                # 向后兼容层
├── _DecisionLanguage.cs                 # DecisionLanguage 聚合
│
├── /DecisionLanguage                    # 语义宪法层
│   ├── DecisionLevel.cs                 # 裁决级别（MUST/MUST_NOT/SHOULD）
│   ├── DecisionRule.cs                  # 裁决规则定义
│   ├── DecisionResult.cs                # 裁决结果
│   └── README.md
│
├── /RuleSets                            # 规则集定义（按 ADR 拆分）
│   ├── /ADR0001
│   │   └── Adr0001RuleSet.cs           # ADR-001 的规则集
│   ├── /ADR0002
│   │   └── Adr0002RuleSet.cs           # ADR-002 的规则集
│   ├── /ADR0003
│   │   └── Adr0003RuleSet.cs           # ADR-003 的规则集
│   ├── /ADR0120
│   │   └── Adr0120RuleSet.cs           # ADR-120 的规则集
│   ├── /ADR0201
│   │   └── Adr0201RuleSet.cs           # ADR-201 的规则集
│   ├── /ADR0900
│   │   └── Adr0900RuleSet.cs           # ADR-900 的规则集
│   └── /ADR0907
│       └── Adr0907RuleSet.cs           # ADR-907 的规则集
│
├── /Index                               # 规则集索引层
│   ├── RuleSetRegistry.cs               # 规则集注册表（统一访问入口）
│   └── AdrRuleIndex.cs                  # 规则索引（快速查询）
│
├── /Rules                               # 规则基础设施
│   ├── ArchitectureRuleSet.cs           # 规则集基类
│   ├── ArchitectureRuleDefinition.cs    # 规则定义
│   ├── ArchitectureClauseDefinition.cs  # 条款定义
│   ├── ArchitectureRuleId.cs            # 规则ID
│   ├── RuleSeverity.cs                  # 严重程度
│   ├── RuleScope.cs                     # 作用域
│   └── RuleLevel.cs                     # 规则层级
│
└── /Tests                               # Specification 自身的测试
    ├── ArchitectureRulesTests.cs
    ├── DecisionLanguageTests.cs
    └── ...
```

## 三层架构说明

### 第一层：DecisionLanguage（语义宪法层）

**职责**：定义架构规则的"语言模型"

这一层定义了规则如何表达裁决语义：
- `DecisionLevel.Must`：强制性要求（阻断 CI）
- `DecisionLevel.MustNot`：明确禁止（阻断 CI）
- `DecisionLevel.Should`：推荐建议（仅警告）

**特点**：
- 极度稳定，很少变化
- 与具体 ADR 无关
- 与技术细节无关
- 对齐 ADR-905 执行级别分类

**示例**：
```csharp
// 解析自然语言文本中的裁决语义
var result = ArchitectureTestSpecification.DecisionLanguage.Parse("模块必须物理隔离");
// result.Level = DecisionLevel.Must
// result.IsBlocking = true
```

### 第二层：RuleSets（规则定义层）

**职责**：定义每个 ADR 的具体规则

每个 ADR 对应一个独立的 RuleSet 文件：
- **一个文件夹**：`/RuleSets/ADR{编号}/`
- **一个文件**：`Adr{编号}RuleSet.cs`
- **一个静态类**：`Adr{编号}RuleSet`
- **一个静态属性**：`RuleSet`（返回 `ArchitectureRuleSet`）

**特点**：
- 只表达"规范"，不知道如何测试
- 不包含测试逻辑
- 不包含技术实现细节
- 是未来自动生成文档、Copilot 指令、Analyzer 的源头

**示例**：
```csharp
// /RuleSets/ADR0001/Adr0001RuleSet.cs
public static class Adr0001RuleSet
{
    public const int AdrNumber = 1;
    
    public static ArchitectureRuleSet RuleSet => LazyRuleSet.Value;
    
    private static readonly Lazy<ArchitectureRuleSet> LazyRuleSet = new(() =>
    {
        var ruleSet = new ArchitectureRuleSet(AdrNumber);
        
        // Rule 1: 模块物理隔离
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "模块物理隔离",
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Module);
            
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "模块按业务能力独立划分",
            enforcement: "通过 NetArchTest 验证模块不相互引用");
            
        return ruleSet;
    });
}
```

### 第三层：Index（索引访问层）

**职责**：提供统一的规则集访问入口

包含两个核心类：
1. **RuleSetRegistry**：规则集注册表
2. **AdrRuleIndex**：规则快速索引

**特点**：
- 架构测试、CLI、CI、Analyzer 的唯一入口
- 提供多种查询方式
- 支持按编号、严重程度、作用域等维度筛选
- 禁止测试直接 `new RuleSet()`

**示例**：
```csharp
// 获取指定 ADR 的规则集
var ruleSet = RuleSetRegistry.Get(1);

// 获取指定规则
var rule = AdrRuleIndex.GetRule("ADR-001_1");

// 获取指定条款
var clause = AdrRuleIndex.GetClause("ADR-001_1_1");

// 获取所有宪法层规则集
var constitutional = RuleSetRegistry.GetConstitutionalRuleSets();

// 获取所有治理层规则集
var governance = RuleSetRegistry.GetGovernanceRuleSets();
```

## 使用指南

### 在架构测试中使用

**✅ 推荐方式（新代码）**：

```csharp
[Fact]
public void ADR_001_1_1_模块按业务能力独立划分()
{
    // 通过 Registry 获取规则集
    var ruleSet = RuleSetRegistry.Get(1);
    var clause = ruleSet.GetClause(1, 1);
    
    // 执行验证逻辑
    var result = ArchitectureTestHelper.ValidateModuleIsolation();
    
    // 使用规则信息构造失败消息
    Assert.True(result.IsSuccess, 
        $"违反 {clause.Id}：{clause.Condition}");
}
```

**⚠️ 兼容方式（旧代码）**：

```csharp
[Fact]
public void ADR_001_1_1_模块按业务能力独立划分()
{
    // 旧方式仍然可用，但建议迁移
    var ruleSet = ArchitectureTestSpecification.ArchitectureRules.Adr001;
    // ...
}
```

### 添加新的 ADR 规则集

1. 在 `/RuleSets/` 下创建新目录：`ADR{编号}/`
2. 创建规则集文件：`Adr{编号}RuleSet.cs`
3. 定义规则集类（参考现有 RuleSet）
4. 在 `RuleSetRegistry.BuildRegistry()` 中注册
5. （可选）在 `_ArchitectureRules.cs` 中添加向后兼容属性

### 查询规则集

```csharp
// 按编号获取
var adr001 = RuleSetRegistry.Get(1);
var adr900 = RuleSetRegistry.Get("ADR-900");

// 按分类获取
var constitutional = RuleSetRegistry.GetConstitutionalRuleSets(); // ADR-001 ~ 008
var governance = RuleSetRegistry.GetGovernanceRuleSets();         // ADR-900 ~ 999
var runtime = RuleSetRegistry.GetRuntimeRuleSets();               // ADR-201 ~ 240
var structure = RuleSetRegistry.GetStructureRuleSets();           // ADR-120 ~ 124

// 按严重程度获取
var constitutional = RuleSetRegistry.GetBySeverity(RuleSeverity.Constitutional);

// 按作用域获取
var moduleRules = RuleSetRegistry.GetByScope(RuleScope.Module);
```

## 迁移指南

### 从旧方式迁移到新方式

**旧代码**：
```csharp
var ruleSet = new ArchitectureRuleSet(1);
ruleSet.AddRule(...);
```

**新代码**：
```csharp
// 定义在 /RuleSets/ADR0001/Adr0001RuleSet.cs
public static class Adr0001RuleSet
{
    public static ArchitectureRuleSet RuleSet => LazyRuleSet.Value;
    // ...
}

// 使用
var ruleSet = RuleSetRegistry.Get(1);
// 或
var ruleSet = Adr0001RuleSet.RuleSet;
```

## 未来扩展

当前架构支持以下未来扩展：

1. **Roslyn Analyzer 集成**
   - RuleSet 可直接转换为 Analyzer 规则
   - DecisionLanguage 可用于分析代码注释

2. **Source Generator 集成**
   - 自动生成测试方法骨架
   - 自动生成规则文档

3. **Copilot Agent 集成**
   - RuleSet 可作为 Agent 的知识库
   - DecisionLanguage 可用于指令解析

4. **规则版本管理**
   - RuleSetRegistry 可扩展为支持多版本
   - 支持规则演进和废弃管理

5. **规则验证工具**
   - 验证 RuleSet 定义的完整性
   - 检查 ADR 文档与 RuleSet 的一致性

## 参考文档

- [ADR-900：架构测试与 CI 治理元规则](../../../docs/adr/ADR-900.md)
- [ADR-905：执行级别分类](../../../docs/adr/ADR-905.md)
- [ADR-907：ArchitectureTests 执法治理体系](../../../docs/adr/ADR-907.md)
- [ADR-907-A：RuleId 格式规范](../../../docs/adr/ADR-907-A.md)

## 常见问题

### Q: 为什么要拆分成三层？
A: 
1. **DecisionLanguage 层**极度稳定，很少变化
2. **RuleSets 层**会随 ADR 增长（目标 100+ ADR）
3. **Index 层**提供统一访问，解耦使用方和定义方

### Q: 为什么每个 ADR 要独立一个文件？
A: 
- 当有 100+ ADR 时，单文件会变得难以维护
- 独立文件便于代码审查和版本控制
- 便于并行开发，减少合并冲突

### Q: 旧代码需要立即迁移吗？
A: 
不需要。旧 API（`ArchitectureRules.AdrXXX`）会一直保留，保证向后兼容。但建议新代码使用 `RuleSetRegistry`。

### Q: 如何验证规则集是否正确注册？
A:
```csharp
// 检查是否已注册
bool isRegistered = RuleSetRegistry.Contains(1);

// 获取所有已注册的 ADR 编号
var allAdrs = RuleSetRegistry.GetAllAdrNumbers();

// 获取所有规则集
var allRuleSets = RuleSetRegistry.GetAllRuleSets();
```
