using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Governance;

/// <summary>
/// ADR-008_1: 文档治理宪法原则（Rule）
/// 验证治理边界的宪法级原则
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-008_1_1: 文档治理宪法已定义
/// - ADR-008_1_2: 裁决权唯一归属原则已定义
/// - ADR-008_1_3: 文档分级体系已定义
/// - ADR-008_1_4: 冲突裁决优先级已定义
/// - ADR-008_1_5: 防语义扩权原则已定义
/// - ADR-008_1_6: Copilot Prompts 文件存在且声明无裁决力
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-008-documentation-governance-constitution.md
/// 
/// 定位说明：
/// - 验证治理边界的宪法级原则
/// - 不检查具体执行细节（如禁用词列表）
/// </summary>
public sealed class ADR_008_1_Architecture_Tests
{
    /// <summary>
    /// ADR-008_1_1: 文档治理宪法已定义
    /// 验证 ADR-008 文档存在且包含核心治理原则（§ADR-008_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-008_1_1: 文档治理宪法已定义")]
    public void ADR_008_1_1_Document_Governance_Constitution_Exists()
    {
        // 验证 ADR-008 文档存在
        var adrFile = FileSystemTestHelper.GetAbsolutePath("docs/adr/constitutional/ADR-008-documentation-governance-constitution.md");
        
        FileSystemTestHelper.AssertFileExists(adrFile, $"ADR-008 文档不存在：{adrFile}");
        
        var content = FileSystemTestHelper.ReadFileContent(adrFile);
        
        // 验证宪法级章节存在（不验证具体内容）
        content.Should().Contain("文档分级");
        content.Should().Contain("裁决");
    }

    /// <summary>
    /// ADR-008_1_2: 裁决权唯一归属原则已定义
    /// 验证核心原则：只有 ADR 具备裁决力（§ADR-008_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-008_1_2: 裁决权唯一归属原则已定义")]
    public void ADR_008_1_2_Decision_Authority_Principle_Is_Defined()
    {
        var adrFile = FileSystemTestHelper.GetAbsolutePath("docs/adr/constitutional/ADR-008-documentation-governance-constitution.md");
        var content = FileSystemTestHelper.ReadFileContent(adrFile);
        
        // 验证核心原则：只有 ADR 具备裁决力
        content.Should().Contain("只有 ADR 具备裁决力");
    }

    /// <summary>
    /// ADR-008_1_3: 文档分级体系已定义
    /// 验证分级体系存在（宪法级、治理级、执行级、说明级）（§ADR-008_1_3）
    /// </summary>
    [Fact(DisplayName = "ADR-008_1_3: 文档分级体系已定义")]
    public void ADR_008_1_3_Document_Hierarchy_Is_Defined()
    {
        var adrFile = FileSystemTestHelper.GetAbsolutePath("docs/adr/constitutional/ADR-008-documentation-governance-constitution.md");
        var content = FileSystemTestHelper.ReadFileContent(adrFile);
        
        // 验证分级体系存在（宪法级、治理级、执行级、说明级）
        Assert.Matches(@"宪法级.*ADR", content);
        Assert.Matches(@"治理级.*Instructions.*Agents", content);
        Assert.Matches(@"执行级.*Skills", content);
        Assert.Matches(@"说明级.*README.*Guide", content);
    }

    /// <summary>
    /// ADR-008_1_4: 冲突裁决优先级已定义
    /// 验证冲突裁决优先级规则（§ADR-008_1_4）
    /// </summary>
    [Fact(DisplayName = "ADR-008_1_4: 冲突裁决优先级已定义")]
    public void ADR_008_1_4_Conflict_Resolution_Priority_Is_Defined()
    {
        var adrFile = FileSystemTestHelper.GetAbsolutePath("docs/adr/constitutional/ADR-008-documentation-governance-constitution.md");
        var content = FileSystemTestHelper.ReadFileContent(adrFile);
        
        // 验证冲突裁决优先级规则
        content.Should().Contain("ADR 正文");
        Assert.Matches(@"ADR.*>.*Instructions", content);
    }

    /// <summary>
    /// ADR-008_1_5: 防语义扩权原则已定义
    /// 验证防引用扩权规则存在（§ADR-008_1_5）
    /// </summary>
    [Fact(DisplayName = "ADR-008_1_5: 防语义扩权原则已定义")]
    public void ADR_008_1_5_Semantic_Privilege_Escalation_Prevention_Is_Defined()
    {
        var adrFile = FileSystemTestHelper.GetAbsolutePath("docs/adr/constitutional/ADR-008-documentation-governance-constitution.md");
        var content = FileSystemTestHelper.ReadFileContent(adrFile);
        
        // 验证防引用扩权规则存在
        content.Should().Contain("任何非 ADR 文档，即使全文逐字引用 ADR，也不具备裁决力");
    }

    /// <summary>
    /// ADR-008_1_6: Copilot Prompts 文件存在且声明无裁决力
    /// 验证 Prompts 文件包含权威声明（§ADR-008_1_6）
    /// </summary>
    [Fact(DisplayName = "ADR-008_1_6: Copilot Prompts 文件存在且声明无裁决力")]
    public void ADR_008_1_6_Prompts_File_Exists_And_Declares_No_Authority()
    {
        var promptsFile = FileSystemTestHelper.GetAbsolutePath("docs/copilot/adr-008.prompts.md");
        
        FileSystemTestHelper.AssertFileExists(promptsFile, $"ADR-008 Prompts 文件不存在：{promptsFile}");
        
        var content = FileSystemTestHelper.ReadFileContent(promptsFile);
        
        // 验证 Prompts 文件包含权威声明
        content.Should().Contain("权威声明");
        content.Should().Contain("不具备裁决权");
        content.Should().Contain("ADR-008");
    }

}
