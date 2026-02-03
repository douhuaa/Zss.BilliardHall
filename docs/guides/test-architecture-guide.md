# 三层测试架构说明

> ⚠️ **无裁决力声明**：本文档仅供参考，不具备架构裁决权。
> 所有架构决策以相关 ADR 正文为准。详见 [ADR-905](adr/governance/ADR-905-enforcement-level-classification.md) 和 [ADR-900](adr/governance/ADR-900-architecture-tests.md)。

> **治理层级已经开始打架了。现在拆还来得及。**

**版本**：1.1  
**生效日期**：2026-01-27  
**状态**：Active

---

## 📍 文档定位

> **本文档专注于「测试架构分层」的深度解读**

### 本文档涵盖的内容

- ✅ Governance/Enforcement/Heuristics 三层架构原理
- ✅ 各层级的定位、职责和失败策略
- ✅ 测试设计原则和命名规范
- ✅ 分层的理论基础和设计思路

### 相关文档

- 📘 **测试全景**：[测试完整指南](testing-framework-guide.md) - 测试策略概览和快速上手
- 📘 **ADR-测试映射**：[ADR-测试一致性指南](adr-test-consistency-guide.md) - 编写测试的实操流程

---

## 核心观点

> **"文档治理 ≠ 纯规则校验"**  
> **"把所有检查都塞进一个 xUnit Test，是架构治理失败的早期症状。"**

你遇到的 README 裁决性语言问题，本质是三种不同性质的约束被混在了一起。

---

## 三层架构定义

根据 [ADR-905：执行级别分类宪法](adr/governance/ADR-905-enforcement-level-classification.md) 和 [ADR-900：架构测试与 CI 治理元规则](adr/governance/ADR-900-architecture-tests.md)，测试架构划分为三层：

| 层级 | 本质 | 是否允许破例 | 失败策略 | 依据 |
|------|------|------------|----------|------|
| **Governance** | 宪法级规则 | ❌ 不可破例 | 架构宪法违规 | ADR-905, ADR-900 |
| **Enforcement** | 可执行硬约束 | ⚠️ 允许登记 | CI 阻断 | ADR-905, ADR-900 |
| **Heuristics** | 风格/质量启发 | ✅ 允许 | 永不失败，仅警告 | ADR-905 |

**拆层是为了"治理权力分级"，不是为了文件好看。**

---

## Governance（治理宪法层）

根据 [ADR-900：架构测试与 CI 治理元规则](adr/governance/ADR-900-architecture-tests.md)，Governance 层负责验证治理宪法本身的存在性和完整性。

### 定位（一句话）

> **定义什么是"合法的治理边界"，而不是怎么写文档。**

### Governance 层该干什么

根据 ADR-900 和 ADR-905，只干三件事：

1. 定义裁决权归属
2. 定义文档角色边界
3. 定义 Enforcement / Heuristics 的合法存在性

### 示例：ADR-0008 Governance Tests

```csharp
// 位置: Governance/ADR_0008_Governance_Tests.cs

[Fact(DisplayName = "ADR-0008.G1: 文档治理宪法已定义")]
public void ADR_0008_Document_Governance_Constitution_Exists()
{
    // 验证 ADR-0008 文档存在
    // 验证宪法级章节存在（不验证具体内容）
}

[Fact(DisplayName = "ADR-0008.G2: 裁决权唯一归属原则已定义")]
public void Decision_Authority_Principle_Is_Defined()
{
    // 验证核心原则：只有 ADR 具备裁决力
}
```

### 不应出现

- ❌ "README 不得使用 XXX 词"（这是 Enforcement 层）
- ❌ 裁决词的检测细节（这是 Enforcement 层）
- ❌ 正则表达式、词表、测试实现（这是 Enforcement 层）

**Governance 不写正则、不写词表、不写测试实现。**

---

## Enforcement（强制执行层）

根据 [ADR-905：执行级别分类宪法](adr/governance/ADR-905-enforcement-level-classification.md)，Enforcement 层负责将 ADR 规则转换为可自动化执行的测试。

### 定位

> **把 Governance 的"不可讨论结论"变成可执行规则。**

### Enforcement 层该干什么

根据 ADR-905 和 ADR-900 的要求：

- 精确、机械、可失败
- 根据 ADR-900，失败信息应能定位到：
  - 文件
  - 行号
  - 对应 ADR

### 测试命名（非常重要）

```csharp
// ✅ 正确：按功能命名
public class DocumentationDecisionLanguageTests

// ❌ 错误：按 ADR 编号命名
public class ADR_0008_Architecture_Tests
```

**ADR 是来源，不是实现命名空间。**

### Enforcement Test 示例结构

```csharp
// 位置: Enforcement/DocumentationDecisionLanguageTests.cs

[Fact(DisplayName = "README/Guide 不得使用裁决性语言")]
public void README_Must_Not_Use_Decision_Language()
{
    // Given
    var forbiddenWords = new[] { "必须", "禁止", "一律", "严禁" };

    // When
    var violations = ScanDocs("docs", forbiddenWords);

    // Then
    violations.Should().BeEmpty(
        "根据 ADR-0008，README / Guide 不具备裁决权");
}
```

### 失败信息格式

```
❌ Enforcement 违规：以下 README/Guide 使用了裁决性语言

根据 ADR-0008 决策 2.2：README/Guide 只能解释'如何使用'，不得使用裁决性语言。

  • docs/adr/README.md:22 - 使用裁决词 '必须'
    内容: 模块必须使用事件通信

修复建议：
  1. 将裁决性语句改为引用 ADR：'根据 ADR-XXXX，模块使用事件通信'
  2. 在文档开头添加：'> ⚠️ 本文档无裁决力，所有架构决策以 ADR 正文为准'
  3. 使用描述性语言代替命令性语言
```

### 破例机制（应显式声明）

```markdown
## Exception
- Doc: docs/legacy/README.md
- Reason: 历史迁移中
- ExpireAt: 2026-06-30
- Owner: @arch
```

**没有登记的破例 = 没发生过审批。**

---

## Heuristics（启发式层）

根据 [ADR-905：执行级别分类宪法](adr/governance/ADR-905-enforcement-level-classification.md)，Heuristics 层提供改进建议但不阻断构建。

### 定位

> **不是规则，是品味。  
> 不是裁决，是提醒。**

### Heuristics 层应该做什么

根据 ADR-905 的定义：

- 语言质量建议
- 可读性
- 一致性
- 但 **不 Fail Build**

### 示例

```csharp
// 位置: Heuristics/DocumentationStyleHeuristicsTests.cs

[Fact(DisplayName = "Heuristics: README 建议使用描述性语言")]
public void README_Should_Prefer_Descriptive_Language()
{
    var suggestions = ScanForImperativeTone("docs");

    if (suggestions.Any())
    {
        _output.WriteLine("⚠️ Heuristics 建议：...");
        // 输出建议
    }

    // ✅ 永远通过 - Heuristics 不应该失败构建
    Assert.True(true);
}
```

### CI 行为

- ❌ 不失败
- ⚠️ 输出 warning
- 📋 进入 Report / Artifact

### 为什么一定要有这一层（强烈观点）

> **没有 Heuristics 的治理体系，最后一定会走向"要么放水、要么内耗"。**

- 规则太严 → 团队绕规则
- 太严格 → ADR 被嫌弃
- 没建议 → README 越写越烂

---

## 目录结构

```text
/tests/ArchitectureTests/
  ├─ Governance/
  │   └─ ADR_0008_Governance_Tests.cs          # 宪法级原则验证
  │
  ├─ Enforcement/
  │   ├─ DocumentationDecisionLanguageTests.cs  # README 裁决语言检查
  │   ├─ DocumentationAuthorityDeclarationTests.cs # Instructions/Agents 权威声明
  │   ├─ SkillsJudgmentLanguageTests.cs         # Skills 判断性语言检查
  │   └─ AdrStructureTests.cs                   # ADR 结构验证
  │
  ├─ Heuristics/
  │   └─ DocumentationStyleHeuristicsTests.cs   # 风格建议
  │
  └─ ADR/
      ├─ ADR_0001_Architecture_Tests.cs         # 模块隔离
      ├─ ADR_0002_Architecture_Tests.cs         # 三层启动
      ├─ ADR_0008_Architecture_Tests.cs         # 重定向文件
      └─ ...
```

---

## 迁移案例：ADR-0008

### 原有测试（问题）

```csharp
// ADR/ADR_0008_Architecture_Tests.cs（旧版）
public class ADR_0008_Architecture_Tests
{
    // ❌ 混合了三种性质的测试
    
    [Fact] public void ADR_0008_Document_Exists() { }          // Governance
    [Fact] public void README_Must_Not_Use_X() { }             // Enforcement
    [Fact] public void README_Should_Be_Concise() { }          // Heuristics
}
```

**问题**：
- 治理原则和执行细节混在一起
- 所有测试都会失败构建，缺乏灵活性
- 无法区分"宪法违规"和"风格建议"

### 重构后（解决方案）

#### Governance 层
```csharp
// Governance/ADR_0008_Governance_Tests.cs
// 职责：验证治理边界定义是否存在
[Fact] public void Decision_Authority_Principle_Is_Defined() { }
[Fact] public void Document_Hierarchy_Is_Defined() { }
```

#### Enforcement 层
```csharp
// Enforcement/DocumentationDecisionLanguageTests.cs
// 职责：检查具体违规，失败 = CI 阻断
[Fact] public void README_Must_Not_Use_Decision_Language() { }
```

#### Heuristics 层
```csharp
// Heuristics/DocumentationStyleHeuristicsTests.cs
// 职责：提供改进建议，永不失败
[Fact] public void README_Should_Prefer_Descriptive_Language() 
{
    // 输出建议，但永远通过
    Assert.True(true);
}
```

---

## 如何判断测试属于哪一层？

### 决策树

```
1. 这个测试验证的是"治理原则是否定义"？
   └─ 是 → Governance

2. 这个测试会检查具体代码/文档违规吗？
   └─ 是 → 根据 ADR-905，违规需要修复吗？
       ├─ 是 → Enforcement
       └─ 否 → Heuristics

3. 这个测试只是提供改进建议？
   └─ 是 → Heuristics
```

### 常见示例

| 测试内容 | 层级 | 原因 |
|---------|------|------|
| "ADR-0008 文档存在" | Governance | 验证治理宪法定义 |
| "README 使用裁决词" | Enforcement | 可执行硬约束，需要修复 |
| "ADR 缺少示例" | Heuristics | 品味建议，不强制 |
| "裁决权归属已定义" | Governance | 宪法级原则 |
| "Instructions 缺少权威声明" | Enforcement | 可执行硬约束 |
| "文档超过 500 行" | Heuristics | 可读性建议 |

---

## 常见问题

### Q: 为什么不能把所有测试都放在 Enforcement 层？

**A**: 因为会失去治理层次：
- 团队会绕规则（太严格）
- ADR 会被嫌弃（太教条）
- 文档质量会下降（没建议）

**三层是权力制衡，不是文件整理。**

### Q: Heuristics 永不失败，有什么用？

**A**: 
- 提供改进建议，不阻断开发
- 团队可选择性采纳
- 逐步提升文档质量

**避免"要么放水、要么内耗"的治理困境。**

### Q: 如何处理 Enforcement 层的误报？

**A**: 优先修改规则，而非代码妥协：
1. 检查是否是合理的例外场景
2. 更新测试的例外模式
3. 记录误报案例

**反作弊规则是治理工具，不是审判工具。**

### Q: 原有的 ADR_XXXX_Architecture_Tests.cs 文件怎么办？

**A**: 
- ADR-0008 已拆分为三层，保留重定向文件
- 其他 ADR 测试（如 ADR-0001 ~ ADR-0007）暂时保留在 ADR/ 目录
- 逐步评估是否需要拆分

---

## 相关资源

- **问题陈述**: Issue "Tests 的命名与分层 ADR"
- **ADR-0008**: [文档治理宪法](../../adr/constitutional/ADR-0008-documentation-governance-constitution.md)
- **工程标准**: [ADR-0008 执行标准](../../engineering-standards/adr-0008-enforcement-standards.md)
- **测试指南**: [Architecture Tests README](../../../src/tests/ArchitectureTests/README.md)

---

## 一句不中听但真实的话

> **你现在的问题不是"README 写错了"，  
> 是"治理层级已经开始打架了"。**

现在拆还来得及。  
再往后，所有 ADR 都会被拖进 Enforcement，最后没人敢写文档。

---

**维护**：架构委员会  
**审核**：@douhuaa  
**状态**：✅ Active
