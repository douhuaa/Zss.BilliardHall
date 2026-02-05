namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;

using FluentAssertions;

/// <summary>
/// 验证 ADR-008_4：ADR 结构规范（Rule）
/// 验证 ADR-008_4_1 到 ADR-008_4_2
/// </summary>
public sealed class ADR_008_4_Architecture_Tests
{
    [Fact(DisplayName = "ADR-008_4_1: ADR 必需章节检查")]
    public void ADR_008_4_1_ADR_Required_Sections()
    {
        // 验证 ADR-008 文档包含 ADR 必需章节的定义
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue($"❌ ADR-008_4_1 违规：ADR-008 文档不存在\n\n" +
            $"修复建议：确保 ADR-008 文档存在于 docs/adr/governance/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§4.1）");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证包含结构规范相关内容
        (content.Contains("章节") || content.Contains("Section") || content.Contains("结构")).Should().BeTrue(
            $"❌ ADR-008_4_1 违规：ADR-008 缺少 ADR 结构规范\n\n" +
            $"修复建议：添加 ADR 必需章节的定义和要求\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§4.1）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-008_4_2: ADR 结构判定规则")]
    public void ADR_008_4_2_ADR_Structure_Decision_Rules()
    {
        // 验证 ADR-008 包含结构判定规则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证包含判定规则
        (content.Contains("判定") || content.Contains("Decision")).Should().BeTrue(
            $"❌ ADR-008_4_2 违规：ADR-008 缺少结构判定规则\n\n" +
            $"修复建议：添加如何判定 ADR 结构是否合规的规则\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§4.2）");
        
        true.Should().BeTrue();
    }

    private static string? FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "docs", "adr")) || 
                Directory.Exists(Path.Combine(current.FullName, ".git")))
                return current.FullName;
            
            current = current.Parent;
        }
        return null;
    }
}
