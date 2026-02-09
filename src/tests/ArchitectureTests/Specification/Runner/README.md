# 架构规则集执行器（Runner）

## 概述

本目录包含架构规则集的执行器和注册表，提供统一的规则执行入口。

## 核心组件

### CentralizedRuleSetRegistry

集中化规则集注册表，提供所有规则的统一访问和查询接口。

**主要功能**：
- `All(options)` - 获取所有规则
- `GetByLayer(layer, options)` - 按层级查询（Governance/Enforcement/Heuristics）
- `GetBySeverity(severity, options)` - 按严重程度查询（L1/L2/L3）
- `GetByAdr(adr, options)` - 按原 ADR 编号查询
- `GetStatistics(options)` - 获取规则统计信息

**使用示例**：
```csharp
var options = new ArchitectureRulesOptions();

// 获取所有规则
var allRules = CentralizedRuleSetRegistry.All(options);

// 获取治理层规则
var governanceRules = CentralizedRuleSetRegistry.GetGovernanceRules(options);

// 获取 L1 级别规则（阻断 CI）
var l1Rules = CentralizedRuleSetRegistry.GetL1Rules(options);

// 按 ADR 查询
var adr122Rules = CentralizedRuleSetRegistry.GetByAdr("ADR-122", options);

// 获取统计信息
var stats = CentralizedRuleSetRegistry.GetStatistics(options);
Console.WriteLine($"规则总数: {stats.Total}");
Console.WriteLine($"L1 级别: {stats.L1Count}");
```

### RuleSetRunnerTests

Theory 驱动的规则执行器，自动执行所有已注册的规则。

**执行策略**：
- **Governance/Enforcement (L1/L2)**：失败将阻断 CI 构建
- **Heuristics (L3)**：仅输出警告信息，不阻断构建

**特性**：
- 使用 `[Theory]` + `[MemberData]` 实现批量执行
- 每个规则作为独立的测试用例
- 详细的测试输出，包括规则信息和执行结果
- 自动统计和验证规则集完整性

## 规则编号系统（RS-###）

### 编号范围分配

| 范围 | 领域 | 映射 ADR | 说明 |
|------|------|----------|------|
| RS-001 ~ RS-009 | 命名与测试组织 | ADR-122 | 测试类命名、项目组织 |
| RS-010 ~ RS-019 | 领域事件 | ADR-120, ADR-210 | 事件命名、处理器规范 |
| RS-020 ~ RS-029 | 仓储模式 | ADR-123 | Repository 接口与实现 |
| RS-030 ~ RS-039 | 反作弊 | ADR-907 | 最小断言、测试有效性 |
| RS-040 ~ RS-049 | （预留） | - | 未来扩展 |
| RS-050 ~ RS-059 | （预留） | - | 未来扩展 |

### RS ↔ ADR 映射表

#### 命名与测试组织规则（RS-001 ~ RS-009）

| RS 编号 | ADR 编号 | 章节 | 规则标题 | 层级 | 严重程度 |
|---------|----------|------|----------|------|----------|
| RS-001 | ADR-122 | 1_2 | 架构测试必须在专用项目中 | Governance | L1 |
| RS-002 | ADR-122 | 1_1 | 测试类必须以 'Tests' 结尾 | Enforcement | L1 |
| RS-003 | ADR-122 | 1_3 | 测试项目必须遵循命名约定 | Enforcement | L1 |

#### 领域事件规则（RS-010 ~ RS-019）

| RS 编号 | ADR 编号 | 章节 | 规则标题 | 层级 | 严重程度 |
|---------|----------|------|----------|------|----------|
| RS-010 | ADR-120 | 1_1 | 领域事件名称必须以 'Event' 结尾 | Enforcement | L1 |
| RS-011 | ADR-120 | 1_2 | 领域事件必须在正确的命名空间下 | Enforcement | L1 |
| RS-012 | ADR-120 | 2_1 | 事件处理器名称必须以 'Handler' 结尾 | Enforcement | L1 |
| RS-013 | ADR-120 | 3_1 | 领域事件不应包含领域实体引用 | Heuristics | L3 |

#### 仓储模式规则（RS-020 ~ RS-029）

| RS 编号 | ADR 编号 | 章节 | 规则标题 | 层级 | 严重程度 |
|---------|----------|------|----------|------|----------|
| RS-020 | ADR-123 | 3 | Repository 接口以 'I' 开头，后缀 'Repository' | Enforcement | L1 |
| RS-021 | ADR-123 | 2 | Repository 接口必须在领域层 | Enforcement | L1 |
| RS-022 | ADR-123 | 1 | Repository 实现类应以 'Repository' 结尾 | Enforcement | L1 |
| RS-023 | ADR-123 | 4 | Repository 实现应在基础设施层 | Enforcement | L1 |

#### 反作弊规则（RS-030 ~ RS-039）

| RS 编号 | ADR 编号 | 章节 | 规则标题 | 层级 | 严重程度 |
|---------|----------|------|----------|------|----------|
| RS-030 | ADR-907 | 3_4 | 每个架构测试类至少包含指定数量的 Fact/Theory | Governance | L1 |
| RS-031 | ADR-907 | 3_3 | 禁止使用 Assert.True(true) 等无意义断言 | Governance | L1 |
| RS-032 | ADR-907 | 1 | 架构测试必须可执行且有效 | Governance | L1 |

## 运行测试

### 运行所有架构规则测试

```bash
dotnet test --filter "FullyQualifiedName~RuleSetRunnerTests.Should_Conform_To_Rules"
```

### 查看规则统计

```bash
dotnet test --filter "FullyQualifiedName~RuleSetRunnerTests.RuleSet_Statistics_Should_Be_Correct"
```

### 按层级运行

```bash
# 仅运行治理层规则
dotnet test --filter "FullyQualifiedName~RuleSetRunnerTests.Query_Rules_By_Layer_Should_Work" --filter "layer=Governance"
```

## 配置选项

通过 `ArchitectureRulesOptions` 配置规则行为：

```csharp
var options = new ArchitectureRulesOptions
{
    TestsProjectName = "ArchitectureTests",
    MinimumFactPerClass = 3,
    ModulesNamespacePrefix = "Zss.BilliardHall.Modules",
    EnableHeuristics = true,
    VerboseOutput = true
};
```

## 扩展新规则

### 1. 在对应的 Ruleset 文件中添加规则

```csharp
// 在 Domains/NamingRuleset.cs 中
yield return new RuleDefinition(
    new RuleId("RS-004", "ADR-122", "2_1"),
    "新的命名规则",
    RuleLayer.Enforcement,
    SeverityLevel.L1,
    assemblies =>
    {
        // 实现验证逻辑
        return RuleResult.Ok();
    });
```

### 2. 更新 CentralizedRuleSetRegistry

在 `CentralizedRuleSetRegistry.All()` 方法中确保新规则集被包含。

### 3. 更新映射表

在本 README 中更新 RS ↔ ADR 映射表。

## 设计原则

1. **最小侵入性**：新规则集与现有 ADR 规则集并存，不破坏现有测试
2. **可追溯性**：每个 RS 编号都映射到原 ADR 和章节
3. **分层治理**：Governance/Enforcement/Heuristics 三层明确
4. **渐进式采用**：可以逐步迁移规则到新系统

## 参考文档

- [ADR-900: 架构测试与 CI 治理元规则](../../../../docs/adr/governance/ADR-900-architecture-tests.md)
- [ADR-907: ArchitectureTests 执法治理体系](../../../../docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md)
- [ADR-122: 测试代码组织与命名规范](../../../../docs/adr/structure/ADR-122-test-naming-convention.md)
- [ADR-120: 领域事件命名规范](../../../../docs/adr/structure/ADR-120-domain-event-naming.md)
- [ADR-123: Repository 模式规范](../../../../docs/adr/structure/ADR-123-repository-pattern.md)
