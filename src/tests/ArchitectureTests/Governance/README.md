# Governance 层测试

> **定义什么是"合法的治理边界"，而不是怎么执行。**

## 层级定位

| 属性 | 值 |
|------|-----|
| **本质** | 宪法级规则 |
| **失败策略** | ❌ 不允许破例 |
| **失败含义** | 架构宪法违规 |

## 职责范围

Governance 层只验证三件事：

1. **裁决权归属定义是否存在**
2. **文档分级体系是否定义**
3. **治理原则是否完整**

## 禁止内容

Governance 测试**不应该**包含：

- ❌ 具体的禁用词列表
- ❌ 正则表达式匹配
- ❌ 文件扫描逻辑
- ❌ 执行细节

## 测试示例

```csharp
[Fact(DisplayName = "ADR-0008.G1: 文档治理宪法已定义")]
public void ADR_0008_Document_Governance_Constitution_Exists()
{
    // 验证 ADR-0008 文档存在
    // 验证宪法级章节存在（不验证具体内容）
}
```

## 与其他层的关系

```
Governance (这里)
  ↓ 定义原则
Enforcement
  ↓ 执行检查
Heuristics
```

Governance 定义"什么是合法"，Enforcement 检查"是否违规"，Heuristics 提供"如何改进"。

## 当前测试

- `ADR_0008_Governance_Tests.cs` - ADR-0008 文档治理宪法的治理原则验证

## 何时添加新测试

当需要验证以下内容时：

- ✅ 新的 ADR 宪法文档是否定义
- ✅ 核心治理原则是否存在
- ✅ 文档分级体系是否完整

**不要添加**：
- ❌ 具体的违规检查（属于 Enforcement）
- ❌ 风格建议（属于 Heuristics）

---

**参考**: [三层测试架构说明](../../../../docs/THREE-LAYER-TEST-ARCHITECTURE.md)
