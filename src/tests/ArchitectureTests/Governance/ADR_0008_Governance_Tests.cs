using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Governance;

/// <summary>
/// ADR-008: 文档治理宪法 - Governance 层测试
/// 
/// 【定位】：验证治理边界的宪法级原则
/// 【不做】：不检查具体执行细节（如禁用词列表）
/// 
/// 本测试类只验证：
/// 1. 裁决权归属定义是否存在
/// 2. 文档分级体系是否定义
/// 3. 治理原则是否完整
/// 
/// 【关联文档】
/// - ADR: docs/adr/constitutional/ADR-008-documentation-governance-constitution.md
/// </summary>
public sealed class ADR_008_Governance_Tests
{
    [Fact(DisplayName = "ADR-008.G1: 文档治理宪法已定义")]
    public void ADR_008_Document_Governance_Constitution_Exists()
    {
        // 验证 ADR-008 文档存在
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adrFile = Path.Combine(repoRoot, "docs/adr/constitutional/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue($"ADR-008 文档不存在：{adrFile}");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证宪法级章节存在（不验证具体内容）
        content.Should().Contain("文档分级");
        content.Should().Contain("裁决");
    }

    [Fact(DisplayName = "ADR-008.G2: 裁决权唯一归属原则已定义")]
    public void Decision_Authority_Principle_Is_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adrFile = Path.Combine(repoRoot, "docs/adr/constitutional/ADR-008-documentation-governance-constitution.md");
        var content = File.ReadAllText(adrFile);
        
        // 验证核心原则：只有 ADR 具备裁决力
        content.Should().Contain("只有 ADR 具备裁决力");
    }

    [Fact(DisplayName = "ADR-008.G3: 文档分级体系已定义")]
    public void Document_Hierarchy_Is_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adrFile = Path.Combine(repoRoot, "docs/adr/constitutional/ADR-008-documentation-governance-constitution.md");
        var content = File.ReadAllText(adrFile);
        
        // 验证分级体系存在（宪法级、治理级、执行级、说明级）
        Assert.Matches(@"宪法级.*ADR", content);
        Assert.Matches(@"治理级.*Instructions.*Agents", content);
        Assert.Matches(@"执行级.*Skills", content);
        Assert.Matches(@"说明级.*README.*Guide", content);
    }

    [Fact(DisplayName = "ADR-008.G4: 冲突裁决优先级已定义")]
    public void Conflict_Resolution_Priority_Is_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adrFile = Path.Combine(repoRoot, "docs/adr/constitutional/ADR-008-documentation-governance-constitution.md");
        var content = File.ReadAllText(adrFile);
        
        // 验证冲突裁决优先级规则
        content.Should().Contain("ADR 正文");
        Assert.Matches(@"ADR.*>.*Instructions", content);
    }

    [Fact(DisplayName = "ADR-008.G5: 防语义扩权原则已定义")]
    public void Semantic_Privilege_Escalation_Prevention_Is_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adrFile = Path.Combine(repoRoot, "docs/adr/constitutional/ADR-008-documentation-governance-constitution.md");
        var content = File.ReadAllText(adrFile);
        
        // 验证防引用扩权规则存在
        content.Should().Contain("任何非 ADR 文档，即使全文逐字引用 ADR，也不具备裁决力");
    }

    [Fact(DisplayName = "ADR-008.G6: Copilot Prompts 文件存在且声明无裁决力")]
    public void ADR_008_Prompts_File_Exists_And_Declares_No_Authority()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var promptsFile = Path.Combine(repoRoot, "docs/copilot/adr-008.prompts.md");
        
        File.Exists(promptsFile).Should().BeTrue($"ADR-008 Prompts 文件不存在：{promptsFile}");
        
        var content = File.ReadAllText(promptsFile);
        
        // 验证 Prompts 文件包含权威声明
        content.Should().Contain("权威声明");
        content.Should().Contain("不具备裁决权");
        content.Should().Contain("ADR-008");
    }
}
