# 集中化架构规则集系统

## 概述

本系统是对 ArchitectureTests.Specification 下规则集的集中化重构，引入了统一的规则编号系统（RS-###）和按领域组织的规则集架构。

## 设计目标

1. **统一规则模型**：建立标准的 RuleDefinition/RuleResult 模型
2. **三层治理理念**：Governance/Enforcement/Heuristics 分类明确
3. **规则重新编号**：RS-### 连续编号，保留与 ADR 的映射关系
4. **领域化组织**：按业务领域（Naming/Repository/DomainEvent/AntiCheat）组织规则
5. **Theory 驱动执行**：统一的执行器，批量运行所有规则

## 目录结构

```
/Specification
├── Common/                          # 通用基础设施
│   ├── RuleModel.cs                 # 规则模型（RuleLayer, SeverityLevel, RuleId, RuleDefinition, RuleResult）
│   ├── RuleAdapters.NetArch.cs      # NetArchTest 适配器
│   └── ArchitectureRulesOptions.cs  # 配置选项
│
├── Domains/                         # 领域规则集
│   ├── NamingRuleset.cs             # 命名规范（RS-001 ~ RS-003, ADR-122）
│   ├── DomainEventRuleset.cs        # 领域事件（RS-010 ~ RS-013, ADR-120/210）
│   ├── RepositoryRuleset.cs         # 仓储模式（RS-020 ~ RS-023, ADR-123）
│   └── AntiCheatRuleset.cs          # 反作弊（RS-030 ~ RS-032, ADR-907）
│
└── Runner/                          # 执行器
    ├── CentralizedRuleSetRegistry.cs # 规则集注册表
    ├── RuleSetRunnerTests.cs         # Theory 驱动的测试执行器
    └── README.md                     # 详细使用文档和映射表
```

## 核心概念

### 规则层级（RuleLayer）

对齐 ADR-900 的三层治理理念：

| 层级 | 说明 | 失败行为 |
|------|------|----------|
| **Governance** | 定义"制度存在"的规则，对应宪法层 ADR | 阻断 CI |
| **Enforcement** | 定义"如何执行"的规则，对应技术约束 | 阻断 CI |
| **Heuristics** | 定义"最佳实践建议" | 仅警告，不阻断 |

### 严重程度（SeverityLevel）

对齐 ADR-907 的执行级别分类：

| 级别 | 说明 | 检测方式 |
|------|------|----------|
| **L1** | 架构测试失败 → 自动阻断 CI | 架构测试（本系统） |
| **L2** | Analyzer 告警 → 需人工审查 | Roslyn Analyzer |
| **L3** | 启发式建议 → 仅警告 | 架构测试（仅警告） |

### 规则标识（RuleId）

新规则编号系统，同时保留 ADR 映射：

```csharp
public sealed record RuleId(
    string NewCode,  // 新编号，如 "RS-001"
    string Adr,      // 原 ADR，如 "ADR-122"
    string Section   // 章节，如 "1_2"
);
```

**示例**：
```
RS-001 (ADR-122.1_2) - 架构测试必须在专用项目中
```

## 已实现规则

### 命名与测试组织（RS-001 ~ RS-009）

| RS 编号 | ADR 映射 | 规则标题 | 层级 | 严重程度 |
|---------|----------|----------|------|----------|
| RS-001 | ADR-122.1_2 | 架构测试必须在专用项目中 | Governance | L1 |
| RS-002 | ADR-122.1_1 | 测试类必须以 'Tests' 结尾 | Enforcement | L1 |
| RS-003 | ADR-122.1_3 | 测试项目必须遵循命名约定 | Enforcement | L1 |

### 领域事件规则（RS-010 ~ RS-019）

| RS 编号 | ADR 映射 | 规则标题 | 层级 | 严重程度 |
|---------|----------|----------|------|----------|
| RS-010 | ADR-120.1_1 | 领域事件名称必须以 'Event' 结尾 | Enforcement | L1 |
| RS-011 | ADR-120.1_2 | 领域事件必须在正确的命名空间下 | Enforcement | L1 |
| RS-012 | ADR-120.2_1 | 事件处理器名称必须以 'Handler' 结尾 | Enforcement | L1 |
| RS-013 | ADR-120.3_1 | 领域事件不应包含领域实体引用 | Heuristics | L3 |

### 仓储模式规则（RS-020 ~ RS-029）

| RS 编号 | ADR 映射 | 规则标题 | 层级 | 严重程度 |
|---------|----------|----------|------|----------|
| RS-020 | ADR-123.3 | Repository 接口以 'I' 开头，后缀 'Repository' | Enforcement | L1 |
| RS-021 | ADR-123.2 | Repository 接口必须在领域层 | Enforcement | L1 |
| RS-022 | ADR-123.1 | Repository 实现类应以 'Repository' 结尾 | Enforcement | L1 |
| RS-023 | ADR-123.4 | Repository 实现应在基础设施层 | Enforcement | L1 |

### 反作弊规则（RS-030 ~ RS-039）

| RS 编号 | ADR 映射 | 规则标题 | 层级 | 严重程度 |
|---------|----------|----------|------|----------|
| RS-030 | ADR-907.3_4 | 每个架构测试类至少包含指定数量的 Fact/Theory | Governance | L1 |
| RS-031 | ADR-907.3_3 | 禁止使用 Assert.True(true) 等无意义断言 | Governance | L1 |
| RS-032 | ADR-907.1 | 架构测试必须可执行且有效 | Governance | L1 |

## 使用方式

### 运行所有规则测试

```bash
# 运行所有规则
dotnet test --filter "FullyQualifiedName~RuleSetRunnerTests.Should_Conform_To_Rules"

# 查看规则统计
dotnet test --filter "FullyQualifiedName~RuleSetRunnerTests.RuleSet_Statistics_Should_Be_Correct"
```

### 在代码中使用

```csharp
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Common;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Runner;

// 获取所有规则
var options = new ArchitectureRulesOptions();
var allRules = CentralizedRuleSetRegistry.All(options);

// 按层级查询
var governanceRules = CentralizedRuleSetRegistry.GetGovernanceRules(options);
var enforcementRules = CentralizedRuleSetRegistry.GetEnforcementRules(options);
var heuristicsRules = CentralizedRuleSetRegistry.GetHeuristicsRules(options);

// 按严重程度查询
var l1Rules = CentralizedRuleSetRegistry.GetL1Rules(options);

// 按 ADR 查询
var adr122Rules = CentralizedRuleSetRegistry.GetByAdr("ADR-122", options);

// 获取统计信息
var stats = CentralizedRuleSetRegistry.GetStatistics(options);
Console.WriteLine($"规则总数: {stats.Total}");
Console.WriteLine($"治理层: {stats.GovernanceCount}");
Console.WriteLine($"执行层: {stats.EnforcementCount}");
Console.WriteLine($"启发层: {stats.HeuristicsCount}");
```

### 配置选项

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

## 添加新规则

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

### 2. 确保规则集被注册

在 `CentralizedRuleSetRegistry.All()` 方法中确保对应的规则集被包含：

```csharp
public static IEnumerable<RuleDefinition> All(ArchitectureRulesOptions options)
{
    foreach (var rule in NamingRuleset.GetRules(options))
        yield return rule;
    // ... 其他规则集
}
```

### 3. 更新文档

在本文档和 `Runner/README.md` 中更新规则映射表。

## 与现有系统的关系

### 兼容性

- ✅ **与现有 ADR 规则集并存**：新系统位于独立的命名空间，不影响现有的 `Index.RuleSetRegistry`
- ✅ **保留 ADR 映射**：每个 RS 规则都记录了对应的 ADR 和章节
- ✅ **渐进式采用**：可以逐步将规则迁移到新系统

### 区别

| 特性 | 现有系统（Index.RuleSetRegistry） | 新系统（CentralizedRuleSetRegistry） |
|------|----------------------------------|-------------------------------------|
| 组织方式 | 按 ADR 编号 | 按领域（Naming/Repository等） |
| 规则编号 | ADR-XXX_Y_Z | RS-### + ADR 映射 |
| 执行方式 | 独立测试方法 | Theory 驱动批量执行 |
| 规则定义 | ArchitectureRuleSet 类 | RuleDefinition 记录 |
| 适配器 | 无统一适配器 | RuleAdapters.NetArch |

## 设计原则

1. **最小侵入性**：新系统与现有系统并存，不破坏现有测试
2. **可追溯性**：每个 RS 编号都映射到原 ADR 和章节
3. **分层治理**：Governance/Enforcement/Heuristics 三层明确
4. **领域化**：规则按业务领域组织，便于理解和维护
5. **可扩展性**：易于添加新规则，支持未来扩展

## 未来扩展

### 短期（1-2 月）
- [ ] 添加更多领域规则集（Handler 模式、模块边界等）
- [ ] 完善 Heuristics 层规则
- [ ] 集成到 CI 流程

### 中期（3-6 月）
- [ ] 实现规则自动发现和注册
- [ ] 支持规则禁用和配置
- [ ] 生成规则执行报告

### 长期（6+ 月）
- [ ] 基于规则集生成 Roslyn Analyzer
- [ ] 规则版本管理和演进
- [ ] 规则执行性能优化

## 参考文档

- [ADR-900: 架构测试与 CI 治理元规则](../../../docs/adr/governance/ADR-900-architecture-tests.md)
- [ADR-907: ArchitectureTests 执法治理体系](../../../docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md)
- [ADR-122: 测试代码组织与命名规范](../../../docs/adr/structure/ADR-122-test-naming-convention.md)
- [ADR-120: 领域事件命名规范](../../../docs/adr/structure/ADR-120-domain-event-naming.md)
- [ADR-123: Repository 模式规范](../../../docs/adr/structure/ADR-123-repository-pattern.md)
- [Runner/README.md](./Runner/README.md) - 详细使用文档

## 常见问题

### Q: 为什么要引入新的规则编号系统？

A: 
1. **可读性**：RS-001 比 ADR-122_1_2 更易记和引用
2. **领域化**：按领域连续编号，RS-001~009 都是命名相关
3. **可扩展**：预留编号范围，便于未来添加新规则
4. **保留映射**：不丢失与 ADR 的关联，仍可追溯

### Q: 新系统与现有系统如何选择？

A:
- **现有系统**：继续用于 ADR 文档的规范定义和引用
- **新系统**：用于日常开发中的规则验证和 CI 集成
- **并存使用**：两者互不冲突，可以同时使用

### Q: 如何处理规则冲突？

A: 
1. 规则基于 ADR 文档，ADR 是唯一权威
2. 如发现冲突，以 ADR 正文为准
3. 可以禁用或调整规则适配器的实现

### Q: 规则测试失败了怎么办？

A:
1. **L1/L2 失败**：必须修复代码或修改规则
2. **L3 失败**：仅警告，可以暂时忽略
3. **合理的违规**：可以调整规则过滤条件或申请破例
