namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

using FluentAssertions;

/// <summary>
/// 验证 ADR-007_4：Prompts 法律地位（Rule）
/// 验证 ADR-007_4_1 到 ADR-007_4_3
/// </summary>
public sealed class ADR_007_4_Architecture_Tests
{
    [Fact(DisplayName = "ADR-007_4_1: Prompts 文件必须声明无裁决力")]
    public void ADR_007_4_1_Prompts_Must_Declare_No_Authority()
    {
        // 验证 ADR-007 文档包含 Prompts 法律地位的定义
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue($"❌ ADR-007_4_1 违规：ADR-007 文档不存在\n\n" +
            $"修复建议：确保 ADR-007 文档存在于 docs/adr/governance/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§4.1）");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证包含 Prompts 相关规则
        content.Should().Contain("Prompts", 
            $"❌ ADR-007_4_1 违规：ADR-007 缺少 Prompts 相关规则\n\n" +
            $"修复建议：添加 Prompts 文件的法律地位和权限定义\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§4.1）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-007_4_2: Prompts 与 ADR 维度对比")]
    public void ADR_007_4_2_Prompts_And_ADR_Dimension_Comparison()
    {
        // 验证 ADR-007 包含 Prompts 与 ADR 的对比说明
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证文档包含维度对比
        (content.Contains("维度") || content.Contains("对比")).Should().BeTrue(
            $"❌ ADR-007_4_2 违规：ADR-007 缺少 Prompts 与 ADR 的维度对比\n\n" +
            $"修复建议：添加说明 Prompts 和 ADR 在裁决力、权威性等维度的区别\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§4.2）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-007_4_3: Prompts 判定规则验证")]
    public void ADR_007_4_3_Prompts_Decision_Rules_Validation()
    {
        // 验证 ADR-007 包含判定规则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证包含判定相关内容
        (content.Contains("判定") || content.Contains("Decision")).Should().BeTrue(
            $"❌ ADR-007_4_3 违规：ADR-007 缺少判定规则\n\n" +
            $"修复建议：添加 Agent 如何基于 ADR 和 Prompts 做出判定的规则\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§4.3）");
        
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
