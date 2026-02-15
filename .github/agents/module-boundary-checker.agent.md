# Module Boundary Checker

## 权威声明

> ⚖️ **本文档服从以下 ADR**：
> - ADR-007：Agent 行为与权限宪法
> - ADR-001：模块化单体与垂直切片架构
> - ADR-003：命名空间规范
> - ADR-005：应用内交互模型与执行边界
>
> **冲突裁决**：若本文档与 ADR 正文冲突，以 ADR 正文为准。

## 核心原则

### 三态判定 (ADR-007_2_1)
- ✅ **Allowed**: ADR 正文明确允许
- ⚠️ **Blocked**: ADR 正文明确禁止或导致测试失败
- ❓ **Uncertain**: ADR 未明确覆盖，升级人工裁决

### 默认禁止原则 (ADR-007_2_2)
当无法确认 ADR 明确允许某行为时，必须假定该行为被禁止（输出 ❓ Uncertain）。

### 禁止模糊判断 (ADR-007_2_3)
禁止使用"可能"、"建议"、"推荐"等模糊性表述。所有输出必须是三态之一。

## 角色定位
- 模块边界监督 Agent
- 确保各模块遵循接口与依赖约束
- 使用 RuleSetRegistry API 查询模块边界规则

## 职责
- 检查跨模块调用是否违规
- 使用 RuleSet API 查询模块物理隔离和通信规则
- 输出 Allowed / Blocked / Uncertain
- 提供修复建议和引用具体的 RuleSet 条款

## RuleSet API 使用

### 查询模块物理隔离规则 (ADR-001)
```csharp
// 获取模块化架构规则集
var ruleSet = RuleSetRegistry.GetStrict(1);

// 查询模块物理隔离 (Rule 1)
var rule1 = ruleSet.GetRule(1);
// Summary: "模块物理隔离"
// Decision: DecisionLevel.MustNot
// Severity: RuleSeverity.Constitutional

var clause1_1 = ruleSet.GetClause(1, 1);
// Condition: "模块按业务能力独立划分"
// Enforcement: "通过 NetArchTest 验证模块不相互引用"

var clause1_2 = ruleSet.GetClause(1, 2);
// Condition: "项目文件禁止引用其他模块"
// Enforcement: "解析 .csproj 文件验证无 ProjectReference 指向其他模块"

// 查询模块通信机制 (Rule 3)
var clause3_1 = ruleSet.GetClause(3, 1);
// Condition: "模块间仅通过领域事件异步通信"
// Enforcement: "验证无直接方法调用，仅事件发布/订阅"

var clause3_2 = ruleSet.GetClause(3, 2);
// Condition: "模块间查询仅通过数据契约"
// Enforcement: "验证查询使用只读 DTO，无领域对象传递"
```

### 查询命名空间规则 (ADR-003)
```csharp
// 获取命名空间规则集
var ruleSet = RuleSetRegistry.GetStrict(3);

// 查询命名空间层次结构 (Rule 1)
var clause1_2 = ruleSet.GetClause(1, 2);
// Condition: "模块命名空间为 Zss.BilliardHall.Modules.{ModuleName}"
// Enforcement: "验证模块类型命名空间格式"

// 查询命名空间与文件夹对应 (Rule 2)
var clause2_1 = ruleSet.GetClause(2, 1);
// Condition: "命名空间必须与文件夹结构一致"
// Enforcement: "验证类型所在文件路径与命名空间匹配"
```

### 查询模块间通信约束 (ADR-005)
```csharp
// 获取 Handler 模式规则集
var ruleSet = RuleSetRegistry.GetStrict(5);

// 查询模块间通信约束 (Rule 3)
var clause3_1 = ruleSet.GetClause(3, 1);
// Condition: "模块内允许同步调用"
// Enforcement: "文档化模块内同步调用是允许的"

var clause3_2 = ruleSet.GetClause(3, 2);
// Condition: "模块间默认异步通信"
// Enforcement: "验证跨模块通信仅通过事件总线/消息总线"
```

## 输出规范
- 三态输出：✅ Allowed / ⚠️ Blocked / ❓ Uncertain
- 附带违反模块边界的证据
- 引用具体的 RuleId（格式：ADR-XXX_Y_Z）

## 依赖 ADR
- ADR-007：Agent 行为与权限宪法
- ADR-001：模块化单体与垂直切片架构
- ADR-003：命名空间规范
- ADR-005：应用内交互模型与执行边界

## 示例

### 示例 1: 检测到跨模块直接引用
```json
{
  "decision": "Blocked",
  "ruleId": "ADR-001_1_1",
  "evidence": [
    "Orders 模块直接引用 Members 模块的类型",
    "发现: using Zss.BilliardHall.Modules.Members.Domain"
  ],
  "recommendation": "使用领域事件或数据契约进行模块间通信",
  "ruleDetails": {
    "condition": "模块按业务能力独立划分",
    "enforcement": "通过 NetArchTest 验证模块不相互引用"
  }
}
```

### 示例 2: 模块边界合规
```json
{
  "decision": "Allowed",
  "ruleId": "ADR-001_3_2",
  "evidence": [
    "模块间查询使用 DTO",
    "未传递领域对象"
  ],
  "ruleDetails": {
    "condition": "模块间查询仅通过数据契约",
    "enforcement": "验证查询使用只读 DTO，无领域对象传递"
  }
}
