using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-008: 文档编写与维护宪法 - 测试重定向
/// 
/// ⚠️ **重要变更通知**：ADR-008 测试已重构为三层架构
/// 
/// 根据问题陈述和 ADR 治理原则，ADR-008 的测试已从单一文件拆分为三层：
/// 
/// 1. **Governance 层**（宪法级原则验证）
///    位置: ArchitectureTests/Governance/ADR_008_Governance_Tests.cs
///    职责: 验证治理边界的宪法级原则（裁决权归属、文档分级）
///    失败: 架构宪法违规
/// 
/// 2. **Enforcement 层**（可执行硬约束）
///    位置: ArchitectureTests/Enforcement/
///      - DocumentationDecisionLanguageTests.cs
///      - DocumentationAuthorityDeclarationTests.cs
///      - SkillsJudgmentLanguageTests.cs
///      - AdrStructureTests.cs
///    职责: 执行具体规则（README 禁用词、权威声明要求）
///    失败: CI 阻断
/// 
/// 3. **Heuristics 层**（启发式建议）
///    位置: ArchitectureTests/Heuristics/DocumentationStyleHeuristicsTests.cs
///    职责: 提供风格和质量建议
///    失败: 永不失败，仅警告
/// 
/// 【设计哲学】
/// 
/// &gt; "文档治理 ≠ 纯规则校验"
/// &gt; "把所有检查都塞进一个 xUnit Test，是架构治理失败的早期症状。"
/// 
/// 三层拆分的目的：
/// - Governance: 定义什么是"合法的治理边界"
/// - Enforcement: 把宪法结论变成可执行规则
/// - Heuristics: 品味建议，不阻断流程
/// 
/// 【迁移指南】
/// 
/// 如果你正在查找原 ADR_008_Architecture_Tests 中的测试：
/// 
/// - `ADR_008_Document_Governance_Constitution_Exists` 
///   → Governance/ADR_008_Governance_Tests.cs
/// 
/// - `README_Must_Not_Use_Decision_Language`
///   → Enforcement/DocumentationDecisionLanguageTests.cs
/// 
/// - `Skills_Must_Not_Output_Judgments`
///   → Enforcement/SkillsJudgmentLanguageTests.cs
/// 
/// - `Instructions_And_Agents_Must_Declare_Authority_Basis`
///   → Enforcement/DocumentationAuthorityDeclarationTests.cs
/// 
/// 【参考】
/// - 问题陈述: issue #XXX "Tests 的命名与分层 ADR"
/// - ADR-008: docs/adr/constitutional/ADR-008-documentation-governance-constitution.md
/// </summary>
[Trait("Category", "Deprecated")]
public sealed class ADR_008_Architecture_Tests
{
    [Fact(DisplayName = "ADR-008_1_1: 测试已重构为三层架构")]
    public void Tests_Have_Been_Refactored_To_Three_Layer_Architecture()
    {
        // 这个测试永远通过，它只是一个重定向标记
        // 
        // 真正的测试现在位于：
        // - Governance/ADR_008_Governance_Tests.cs
        // - Enforcement/*.cs
        // - Heuristics/DocumentationStyleHeuristicsTests.cs
        
        true.Should().BeTrue("❌ ADR-008_1_1 违规：ADR-008 测试已迁移至三层架构。" +
            "请参考 Governance/、Enforcement/、Heuristics/ 目录。");
    }
}
