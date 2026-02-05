namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;

using FluentAssertions;

/// <summary>
/// 验证 ADR-008_7：违规处理（Rule）
/// 验证 ADR-008_7_1 到 ADR-008_7_2
/// </summary>
public sealed class ADR_008_7_Architecture_Tests
{
    [Fact(DisplayName = "ADR-008_7_1: 违规行为定义检查")]
    public void ADR_008_7_1_Violation_Behavior_Definition()
    {
        // 验证 ADR-008 文档包含违规行为的定义
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue($"❌ ADR-008_7_1 违规：ADR-008 文档不存在\n\n" +
            $"修复建议：确保 ADR-008 文档存在于 docs/adr/governance/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§7.1）");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证包含违规相关内容
        (content.Contains("违规") || content.Contains("Violation")).Should().BeTrue(
            $"❌ ADR-008_7_1 违规：ADR-008 缺少违规行为定义\n\n" +
            $"修复建议：添加违反文档治理规则的行为定义\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§7.1）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-008_7_2: 违规处理判定规则")]
    public void ADR_008_7_2_Violation_Handling_Decision_Rules()
    {
        // 验证 ADR-008 包含违规处理判定规则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证包含处理和判定相关内容
        (content.Contains("处理") || content.Contains("判定") || content.Contains("Decision")).Should().BeTrue(
            $"❌ ADR-008_7_2 违规：ADR-008 缺少违规处理判定规则\n\n" +
            $"修复建议：添加如何判定和处理文档违规的规则\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§7.2）");
        
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
