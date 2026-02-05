namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;

using FluentAssertions;

/// <summary>
/// 验证 ADR-008_6：文档变更治理（Rule）
/// 验证 ADR-008_6_1 到 ADR-008_6_3
/// </summary>
public sealed class ADR_008_6_Architecture_Tests
{
    [Fact(DisplayName = "ADR-008_6_1: 文档变更要求检查")]
    public void ADR_008_6_1_Document_Change_Requirements()
    {
        // 验证 ADR-008 文档包含文档变更的要求
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue($"❌ ADR-008_6_1 违规：ADR-008 文档不存在\n\n" +
            $"修复建议：确保 ADR-008 文档存在于 docs/adr/governance/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§6.1）");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证包含变更治理相关内容
        (content.Contains("变更") || content.Contains("Change")).Should().BeTrue(
            $"❌ ADR-008_6_1 违规：ADR-008 缺少文档变更要求\n\n" +
            $"修复建议：添加文档变更的审批和治理流程\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§6.1）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-008_6_2: 冲突裁决优先级")]
    public void ADR_008_6_2_Conflict_Resolution_Priority()
    {
        // 验证 ADR-008 包含冲突裁决优先级规则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证包含优先级相关规则
        (content.Contains("优先级") || content.Contains("Priority") || content.Contains("冲突")).Should().BeTrue(
            $"❌ ADR-008_6_2 违规：ADR-008 缺少冲突裁决优先级规则\n\n" +
            $"修复建议：添加文档冲突时的裁决优先级顺序\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§6.2）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-008_6_3: 文档变更判定规则")]
    public void ADR_008_6_3_Document_Change_Decision_Rules()
    {
        // 验证 ADR-008 包含文档变更判定规则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证包含判定规则
        (content.Contains("判定") || content.Contains("Decision")).Should().BeTrue(
            $"❌ ADR-008_6_3 违规：ADR-008 缺少文档变更判定规则\n\n" +
            $"修复建议：添加如何判定文档变更是否合规的规则\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§6.3）");
        
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
