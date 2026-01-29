---
adr: ADR-904
title: "架构测试命名规范"
status: Superseded
level: Governance
deciders: "Architecture Board"
date: 2026-01-25
version: "1.0"
maintainer: "Architecture Board"
reviewer: "Architecture Board"
supersedes: null
superseded_by: "ADR-907"
---

> 🏛️ **Archived Notice**
>
> 本 ADR 已被 [ADR-907](../../governance/ADR-907-architecture-tests-enforcement-governance.md) 完全吸收并取代。
> 
> - 本文件 **不再具备任何裁决力**
> - **不得** 编写或维护对应 ArchitectureTests
> - **不得** 被 Analyzer / CI Gate 读取
> - 仅用于 **历史追溯与设计演进说明**

# ADR-904：架构测试命名规范

**状态**：❌ Superseded（已被 ADR-907 取代）  
**级别**：治理层  
**版本**：1.0  
**创建日期**：2026-01-25  
**归档日期**：2026-01-29  
**适用范围**：（已失效）  
**生效时间**：（已失效）

---

## 历史背景（Historical Context）

本 ADR 最初定义了架构测试的命名规范，包括：

- 测试类命名：`ADR_{XXXX}_Architecture_Tests.cs`
- 测试方法命名：`{Subject}_{Constraint}_{Violation_Description}`
- 失败消息格式：`❌ ADR-XXXX.Y 违规: {原因}。\n修复建议：{建议}\n参考：{文档}`

---

## 为什么被取代（Superseded Rationale）

命名规范与测试分类、执行流程紧密相关，单独维护导致：

1. **理解成本高**：开发者需要同时参考 ADR-903（分类）、ADR-904（命名）、ADR-906（流程）
2. **一致性风险**：三个 ADR 的示例可能不同步
3. **变更复杂**：修改命名规则需要同时更新分类和流程文档

ADR-907 将命名规范作为第 2 节（ADR-907.2）和第 3 节（ADR-907.3）进行统一管理。

---

## 原始规则（Original Rules）

> ⚠️ **以下内容仅供历史追溯，不具裁决力。所有规则已迁移至 ADR-907。**

### 测试类命名

- **规则**：`ADR_{XXXX}_Architecture_Tests.cs`
- **位置**：`src/tests/ArchitectureTests/ADR/`

**示例**：
- ✅ `ADR_0001_Architecture_Tests.cs`
- ✅ `ADR_0907_Architecture_Tests.cs`
- ❌ `ModuleTests.cs`（缺少 ADR 编号）
- ❌ `ADR-0001-Tests.cs`（格式错误）

### 测试方法命名

- **规则**：`{Subject}_{Constraint}_{Violation_Description}`

**示例**：
- ✅ `Modules_Should_Not_Reference_Other_Modules`
- ✅ `Handlers_Should_Not_Depend_On_AspNet`
- ❌ `Test1` / `CheckModules`（不清晰）
- ❌ `TestHandlerNaming`（缺少约束描述）

### 失败消息格式

```csharp
Assert.True(result.IsSuccessful,
    $"❌ ADR-{adrNumber}.{ruleNumber} 违规: {violation}。\n" +
    $"修复建议：\n" +
    $"  1. {suggestion1}\n" +
    $"  2. {suggestion2}\n" +
    $"参考：docs/adr/{category}/ADR-{adrNumber}-{filename}.md");
```

---

## 迁移指南（Migration Guide）

如果你在旧文档或代码中看到对 ADR-904 的引用：

1. **ADR 文档引用** → 更改为 ADR-907
2. **测试命名规则引用** → 更改为 ADR-907.2（组织与命名）
3. **失败消息规则引用** → 更改为 ADR-907.3（失败消息标准）
4. **Prompt 文件引用** → 更改为 ADR-907

---

## 关系（Relationships）

### 被取代（Superseded By）

- [ADR-907](../../governance/ADR-907-architecture-tests-enforcement-governance.md) - 架构测试执行治理宪章（唯一执法入口）

### 原始依赖（Original Dependencies）

- [ADR-0000](../../governance/ADR-0000-architecture-tests.md) - 架构测试与 CI 治理宪法
- [ADR-122](../../structure/ADR-122-test-organization-naming.md) - 测试代码组织与命名规范

---

## 版本历史（Version History）

| 版本  | 日期         | 变更说明                       | 状态         |
|-----|------------|----------------------------|------------|
| 1.0 | 2026-01-25 | 初始版本，定义命名规范               | Active     |
| -   | 2026-01-29 | 被 ADR-907 完全取代，移至 archive | Superseded |

---

**维护者**：Architecture Board  
**归档者**：Architecture Board  
**归档日期**：2026-01-29

> 📌 **重要提醒**：本文档已失效，请勿用于架构决策或测试实施。所有相关规则已迁移至 ADR-907。
