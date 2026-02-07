# RuleId Language

## 定义

RuleIdLanguage 定义架构裁决系统中 RuleId 的语法、语义边界与合法性规则，是所有 RuleId 解析与验证的唯一来源。

## 核心组件

### ArchitectureRuleId
强类型的 RuleId 值对象，表示整个治理体系的最小不可再分单元。

**特性**：
- 不可变结构体（record struct）
- 类型安全的构造（通过工厂方法）
- 规范的字符串格式
- 可比较、可排序
- 明确的 Rule / Clause 边界

### RuleIdParser
RuleId 语言规范解析器，负责所有字符串到 RuleId 对象的转换。

**特性**：
- 严格模式（ParseStrict）：用于测试/CI/Analyzer
- 宽容模式（TryParse）：用于探索性查询
- 格式验证（IsValidRuleId）
- 支持多种格式（下划线、点号分隔）

## 支持的 RuleId 格式

推荐格式：
- `ADR-XXX_Y` - Rule 级别
- `ADR-XXX_Y_Z` - Clause 级别

兼容格式：
- `ADR-XXX.Y` - Rule 级别（旧格式）
- `ADR-XXX.Y.Z` - Clause 级别（旧格式）

简化格式：
- `XXX_Y` - Rule 级别（省略 ADR 前缀）
- `XXX_Y_Z` - Clause 级别（省略 ADR 前缀）

## 设计原则

1. **语言层独立性**：不依赖 ADR 文档，不依赖技术实现
2. **唯一语义源**：所有 RuleId 解析必须通过 RuleIdParser
3. **类型安全**：优先使用 ArchitectureRuleId 对象而非字符串
4. **可扩展性**：为未来的 Analyzer/Generator/Copilot 预留扩展点

## 使用示例

```csharp
// 解析 RuleId 字符串（宽容模式）
if (RuleIdParser.TryParse("ADR-001_1_1", out var ruleId))
{
    // 使用类型安全的 RuleId 对象
    bool isClause = ruleId.IsClause; // true
    int adrNumber = ruleId.AdrNumber; // 1
}

// 解析 RuleId 字符串（严格模式）
var ruleId = RuleIdParser.ParseStrict("ADR-907_3_2");

// 创建 RuleId 对象
var ruleId = ArchitectureRuleId.Rule(907, 3);
var clauseId = ArchitectureRuleId.Clause(907, 3, 2);
```

## 相关文档

- [ADR-905: 裁决语义模型](../../../docs/adr/governance/ADR-905-decision-semantic-model.md)
- [ADR-907: ArchitectureTests 执法治理体系](../../../docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md)
