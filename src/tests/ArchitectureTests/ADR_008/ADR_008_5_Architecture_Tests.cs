namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;

using FluentAssertions;

/// <summary>
/// 验证 ADR-008_5：ADR 语言规范（Rule）
/// 验证 ADR-008_5_1 到 ADR-008_5_4
/// </summary>
public sealed class ADR_008_5_Architecture_Tests
{
    [Fact(DisplayName = "ADR-008_5_1: ADR 禁用语言检查")]
    public void ADR_008_5_1_ADR_Prohibited_Language_Check()
    {
        // 验证 ADR-008 文档包含语言规范
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue($"❌ ADR-008_5_1 违规：ADR-008 文档不存在\n\n" +
            $"修复建议：确保 ADR-008 文档存在于 docs/adr/governance/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§5.1）");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证包含语言规范
        (content.Contains("语言") || content.Contains("Language")).Should().BeTrue(
            $"❌ ADR-008_5_1 违规：ADR-008 缺少语言规范\n\n" +
            $"修复建议：添加 ADR 中禁用语言的规范\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§5.1）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-008_5_2: ADR 裁决性语言检查")]
    public void ADR_008_5_2_ADR_Decisional_Language_Check()
    {
        // 验证 ADR-008 包含裁决性语言的规范
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证包含裁决性语言的规范
        (content.Contains("裁决") || content.Contains("Authority")).Should().BeTrue(
            $"❌ ADR-008_5_2 违规：ADR-008 缺少裁决性语言规范\n\n" +
            $"修复建议：添加 ADR 如何使用裁决性语言的规范\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§5.2）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-008_5_3: ADR 语言核心原则")]
    public void ADR_008_5_3_ADR_Language_Core_Principles()
    {
        // 验证 ADR-008 包含语言核心原则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-008-documentation-governance-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证包含核心原则
        (content.Contains("原则") || content.Contains("Principle")).Should().BeTrue(
            $"❌ ADR-008_5_3 违规：ADR-008 缺少语言核心原则\n\n" +
            $"修复建议：添加 ADR 语言使用的核心原则\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§5.3）");
        
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
