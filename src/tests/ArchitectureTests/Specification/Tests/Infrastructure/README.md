# 测试基础设施 (Test Infrastructure)

本目录包含用于支持 Specification 测试的辅助类和工具类。

## 设计理念

遵循以下原则构建测试基础设施：

1. **DRY (Don't Repeat Yourself)**: 提取重复的测试逻辑到可重用的辅助类
2. **Single Responsibility**: 每个工具类专注于单一职责
3. **Clear Assertions**: 提供清晰、有意义的断言消息
4. **Composition over Inheritance**: 使用静态辅助方法而非继承基类

## 核心组件

### 1. RuleIdAssertions

**职责**: 提供 RuleId 相关的断言和验证逻辑

**主要方法**:
- `AssertParsedRuleId()`: 断言解析结果的各个字段
- `AssertTryParseSuccess()`: 断言 TryParse 成功并验证结果
- `AssertTryParseFailed()`: 断言 TryParse 失败
- `AssertRuleIdEquals()`: 断言两个 RuleId 相等
- `AssertIsRule()`: 断言 RuleId 是 Rule 级别
- `AssertIsClause()`: 断言 RuleId 是 Clause 级别

**使用示例**:
```csharp
// 断言解析成功
RuleIdAssertions.AssertTryParseSuccess("ADR-907_3", expectedAdr: 907, expectedRule: 3, expectedClause: null);

// 断言是 Rule 级别
var ruleId = ArchitectureRuleId.Rule(907, 3);
RuleIdAssertions.AssertIsRule(ruleId);
```

### 2. RuleSetValidator

**职责**: 提供 RuleSet 结构完整性和一致性验证

**主要方法**:
- `ValidateRuleStructure()`: 验证 Rule 的结构完整性
- `ValidateClauseStructure()`: 验证 Clause 的结构完整性
- `ValidateClauseToRuleBinding()`: 验证 Clause 与父 Rule 的关联
- `ValidateCompleteness()`: 验证每个 Rule 至少有一个 Clause
- `ValidateFull()`: 执行完整的 RuleSet 验证

**使用示例**:
```csharp
var ruleSet = RuleSetRegistry.GetStrict(907);

// 完整验证
RuleSetValidator.ValidateFull(ruleSet, expectedAdrNumber: 907);

// 或单独验证
RuleSetValidator.ValidateRuleStructure(ruleSet, 907);
RuleSetValidator.ValidateClauseStructure(ruleSet, 907);
```

### 3. TestDataBuilder

**职责**: 提供流式 API 创建测试数据

**主要方法**:
- `CreateRuleSet()`: 创建 RuleSet 构建器
- `WithRule()`: 添加规则（支持默认值）
- `WithClause()`: 添加条款（支持默认值）
- `WithCompleteRule()`: 添加完整规则（Rule + Clause）
- `Build()`: 构建最终的 RuleSet

**使用示例**:
```csharp
var ruleSet = TestDataBuilder.CreateRuleSet(907)
    .WithCompleteRule(1, summary: "规则1")
    .WithRule(2, summary: "规则2")
    .WithClause(2, 1, condition: "条件1", enforcement: "执行1")
    .Build();
```

## 重构收益

### 代码复用
- **重构前**: 每个测试文件都有重复的断言逻辑
- **重构后**: 统一的辅助类，消除重复代码

### 可维护性
- **重构前**: 修改断言逻辑需要更新多个文件
- **重构后**: 只需修改一个工具类

### 可读性
- **重构前**: 测试代码混杂断言细节
- **重构后**: 测试意图更清晰，专注业务逻辑

### 一致性
- **重构前**: 不同测试可能使用不同的断言模式
- **重构后**: 统一的断言接口，保证一致性

## 最佳实践

### 1. 使用语义化的断言方法

❌ **不好的做法**:
```csharp
result.AdrNumber.Should().Be(907);
result.RuleNumber.Should().Be(3);
result.ClauseNumber.Should().BeNull();
```

✅ **好的做法**:
```csharp
RuleIdAssertions.AssertParsedRuleId(result, 907, 3, null, context: "解析 'ADR-907_3'");
```

### 2. 提供有意义的上下文信息

❌ **不好的做法**:
```csharp
ruleId.IsRule.Should().BeTrue();
```

✅ **好的做法**:
```csharp
RuleIdAssertions.AssertIsRule(ruleId, context: $"RuleId({adr}, {rule})");
```

### 3. 使用 TestDataBuilder 简化测试数据创建

❌ **不好的做法**:
```csharp
var ruleSet = new ArchitectureRuleSet(907);
ruleSet.AddRule(1, "规则1", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
ruleSet.AddClause(1, 1, "条件1", "执行1", ClauseExecutionType.Convention);
```

✅ **好的做法**:
```csharp
var ruleSet = TestDataBuilder.CreateRuleSet(907)
    .WithCompleteRule(1, summary: "规则1")
    .Build();
```

## 扩展指南

当需要添加新的测试辅助功能时，请遵循以下步骤：

1. **识别重复模式**: 查找测试代码中的重复逻辑
2. **创建辅助类**: 在 Infrastructure 目录下创建新的辅助类
3. **提供清晰API**: 使用语义化的方法名和参数
4. **添加文档**: 在类和方法上添加完整的 XML 注释
5. **更新 README**: 在本文档中记录新增的工具类

## 相关文档

- [SOLID 原则](https://en.wikipedia.org/wiki/SOLID)
- [Test-Driven Development](https://en.wikipedia.org/wiki/Test-driven_development)
- [Clean Code](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
