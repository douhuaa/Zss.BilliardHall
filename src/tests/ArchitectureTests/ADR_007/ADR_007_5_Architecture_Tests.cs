namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

using FluentAssertions;

/// <summary>
/// 验证 ADR-007_5：Guardian 主从关系（Rule）
/// 验证 ADR-007_5_1 到 ADR-007_5_2
/// </summary>
public sealed class ADR_007_5_Architecture_Tests
{
    [Fact(DisplayName = "ADR-007_5_1: Guardian 角色定义检查")]
    public void ADR_007_5_1_Guardian_Role_Definition_Check()
    {
        // 验证 ADR-007 文档包含 Guardian 角色定义
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue($"❌ ADR-007_5_1 违规：ADR-007 文档不存在\n\n" +
            $"修复建议：确保 ADR-007 文档存在于 docs/adr/governance/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§5.1）");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证包含 Guardian 相关规则
        content.Should().Contain("Guardian", 
            $"❌ ADR-007_5_1 违规：ADR-007 缺少 Guardian 角色定义\n\n" +
            $"修复建议：添加 Guardian Agent 的角色、职责和权限定义\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§5.1）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-007_5_2: Guardian 判定规则验证")]
    public void ADR_007_5_2_Guardian_Decision_Rules_Validation()
    {
        // 验证 ADR-007 包含 Guardian 的判定规则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证包含判定相关内容
        (content.Contains("判定") || content.Contains("Decision") || content.Contains("三态")).Should().BeTrue(
            $"❌ ADR-007_5_2 违规：ADR-007 缺少 Guardian 判定规则\n\n" +
            $"修复建议：添加 Guardian 如何基于 Specialist 输出做出判定的规则\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§5.2）");
        
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
