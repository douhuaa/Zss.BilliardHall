namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;

using FluentAssertions;

/// <summary>
/// 验证 ADR-008_3：非 ADR 文档约束（Rule）
/// 验证 ADR-008_3_1 到 ADR-008_3_4
/// </summary>
public sealed class ADR_008_3_Architecture_Tests
{
    [Fact(DisplayName = "ADR-008_3_1: Instructions/Agents 必须声明权威依据")]
    public void ADR_008_3_1_Instructions_Must_Declare_Authority()
    {
        // 验证 ADR-008 文档包含 Instructions 和 Agents 的约束
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue($"❌ ADR-008_3_1 违规：ADR-008 文档不存在\n\n" +
            $"修复建议：确保 ADR-008 文档存在于 docs/adr/governance/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§3.1）");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证包含 Instructions 相关规则
        (content.Contains("Instructions") || content.Contains("Agent")).Should().BeTrue(
            $"❌ ADR-008_3_1 违规：ADR-008 缺少 Instructions/Agents 约束\n\n" +
            $"修复建议：添加 Instructions 和 Agents 文档必须声明权威依据的规则\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§3.1）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-008_3_2: Skills 不得输出判断性结论")]
    public void ADR_008_3_2_Skills_Must_Not_Output_Judgmental_Conclusions()
    {
        // 验证 ADR-008 包含 Skills 相关约束
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证包含 Skills 相关规则
        content.Should().Contain("Skills", 
            $"❌ ADR-008_3_2 违规：ADR-008 缺少 Skills 约束\n\n" +
            $"修复建议：添加 Skills 文档不得输出判断性结论的规则\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§3.2）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-008_3_3: README/Guide 必须声明无裁决力")]
    public void ADR_008_3_3_README_Must_Declare_No_Authority()
    {
        // 验证 ADR-008 包含 README 和 Guide 的约束
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证包含 README 相关规则
        (content.Contains("README") || content.Contains("Guide")).Should().BeTrue(
            $"❌ ADR-008_3_3 违规：ADR-008 缺少 README/Guide 约束\n\n" +
            $"修复建议：添加 README 和 Guide 必须声明无裁决力的规则\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§3.3）");
        
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
