# Enforcement 层测试

> **把 Governance 的"不可讨论结论"变成可执行规则。**

## 层级定位

| 属性 | 值 |
|------|-----|
| **本质** | 可执行硬约束 |
| **失败策略** | ⚠️ 允许登记破例 |
| **失败含义** | CI 阻断 |

## 职责范围

Enforcement 层负责：

1. **精确、机械地检查违规**
2. **提供清晰的失败信息**（文件、行号、对应 ADR）
3. **执行 Governance 层定义的原则**

## 测试命名规范

✅ **按功能命名**（推荐）：
```csharp
public class DocumentationDecisionLanguageTests
public class DocumentationAuthorityDeclarationTests
public class SkillsJudgmentLanguageTests
```

❌ **按 ADR 编号命名**（不推荐）：
```csharp
public class ADR_0008_Architecture_Tests  // 太宽泛
```

**原因**：ADR 是来源，不是实现命名空间。

## 失败信息格式

Enforcement 测试失败时，必须提供：

```
❌ Enforcement 违规：[简要说明]

根据 ADR-XXXX 决策 X.X：[规则原文]

  • [文件路径]:[行号] - [问题描述]
  • [文件路径]:[行号] - [问题描述]

修复建议：
  1. [具体修复步骤]
  2. [具体修复步骤]

参考：[ADR 文档路径]
```

## 当前测试

- `DocumentationDecisionLanguageTests.cs` - README/Guide 裁决语言检查
- `DocumentationAuthorityDeclarationTests.cs` - Instructions/Agents 权威声明检查
- `SkillsJudgmentLanguageTests.cs` - Skills 判断性语言检查
- `AdrStructureTests.cs` - ADR 结构验证

## 破例机制

如需破例（极少情况），必须：

1. 在 `docs/summaries/governance/arch-violations.md` 中登记
2. 注明破例原因和失效日期（不超过 3 个月）
3. 指定负责人

**未登记的破例 = 未授权违规**

## 性能考虑

- 限制文件扫描范围（避免 CI 超时）
- 使用缓存避免重复检查
- 保持测试运行时间 < 5 秒

## 何时添加新测试

当需要以下内容时：

- ✅ 检查具体的代码/文档违规
- ✅ 执行 ADR 中定义的硬约束
- ✅ 自动化人工 Code Review 的重复性检查

**不要添加**：
- ❌ 治理原则验证（属于 Governance）
- ❌ 风格建议（属于 Heuristics）

---

**参考**: [三层测试架构说明](../../../../docs/THREE-LAYER-TEST-ARCHITECTURE.md)
