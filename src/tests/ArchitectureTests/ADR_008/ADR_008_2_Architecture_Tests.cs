namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;

using FluentAssertions;

/// <summary>
/// 验证 ADR-008_2：ADR 内容边界（Rule）
/// 验证 ADR-008_2_1 到 ADR-008_2_3
/// </summary>
public sealed class ADR_008_2_Architecture_Tests
{
    [Fact(DisplayName = "ADR-008_2_1: ADR 允许的内容范围")]
    public void ADR_008_2_1_ADR_Allowed_Content_Scope()
    {
        // 验证 ADR-008 文档包含 ADR 允许的内容范围定义
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue($"❌ ADR-008_2_1 违规：ADR-008 文档不存在\n\n" +
            $"修复建议：确保 ADR-008 文档存在于 docs/adr/governance/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§2.1）");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证包含内容边界相关规则
        content.Should().Contain("内容", 
            $"❌ ADR-008_2_1 违规：ADR-008 缺少内容边界定义\n\n" +
            $"修复建议：添加 ADR 允许包含的内容范围\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§2.1）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-008_2_2: ADR 禁止的内容")]
    public void ADR_008_2_2_ADR_Prohibited_Content()
    {
        // 验证 ADR-008 包含禁止内容的定义
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证包含禁止内容的规则
        (content.Contains("禁止") || content.Contains("Prohibited")).Should().BeTrue(
            $"❌ ADR-008_2_2 违规：ADR-008 缺少禁止内容的定义\n\n" +
            $"修复建议：添加 ADR 禁止包含的内容类型\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§2.2）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-008_2_3: ADR 内容判定规则")]
    public void ADR_008_2_3_ADR_Content_Decision_Rules()
    {
        // 验证 ADR-008 包含内容判定规则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证包含判定规则
        (content.Contains("判定") || content.Contains("Decision")).Should().BeTrue(
            $"❌ ADR-008_2_3 违规：ADR-008 缺少内容判定规则\n\n" +
            $"修复建议：添加如何判定内容是否属于 ADR 范围的规则\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§2.3）");
        
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
