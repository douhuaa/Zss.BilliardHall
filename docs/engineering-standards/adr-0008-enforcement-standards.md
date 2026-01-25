# ADR-0008 执行标准：文档治理自动化检查

> ⚠️ **声明**：本文档是工程标准，不具备裁决力。  
> **裁决依据**：[ADR-0008 文档治理宪法](../adr/constitutional/ADR-0008-documentation-governance-constitution.md)

**版本**：1.0  
**最后更新**：2026-01-25  
**状态**：Active

---

## 目的

本文档定义 ADR-0008 的具体执行细节和自动化检查方法。

**重要区分**：
- **ADR-0008**：定义规则（WHAT）
- **本文档**：定义如何检查（HOW）

---

## 禁用词列表

### README/Guide 禁用的裁决性词汇

根据 ADR-0008 决策 3.3，以下词汇禁止在 README/Guide 中使用：

```
必须、禁止、不允许、不得、应当
```

**例外上下文**（允许使用）：
- 引用 ADR 时：`根据 ADR-0001，模块必须...`
- 示例标记：`❌ 禁止：...`（作为示例展示）
- 表格内容：表格单元格中的说明

**实现位置**：
- 测试：`Enforcement/DocumentationDecisionLanguageTests.cs`
- 方法：`README_Must_Not_Use_Decision_Language()`

### Skills 禁用的判断性词汇

根据 ADR-0008 决策 3.2，以下词汇禁止在 Skills 输出中使用：

```
违规、不符合、应当、建议、推荐、
正确、错误、合规、不合规、必须修复
```

**实现位置**：
- 测试：`Enforcement/SkillsJudgmentLanguageTests.cs`
- 方法：`Skills_Must_Not_Output_Judgments()`

---

## 相关资源

- **ADR-0008 正文**：[文档治理宪法](../adr/constitutional/ADR-0008-documentation-governance-constitution.md)
- **测试实现**：`src/tests/ArchitectureTests/Enforcement/`
- **Copilot 指导**：[adr-0008.prompts.md](../copilot/adr-0008.prompts.md)

---

**维护**：架构委员会  
**审核**：@douhuaa  
**状态**：✅ Active
