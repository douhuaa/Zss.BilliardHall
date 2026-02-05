namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

using FluentAssertions;

/// <summary>
/// 验证 ADR-007_6：Agent 变更治理（Rule）
/// 验证 ADR-007_6_1 到 ADR-007_6_2
/// </summary>
public sealed class ADR_007_6_Architecture_Tests
{
    [Fact(DisplayName = "ADR-007_6_1: Agent 配置变更规则校验")]
    public void ADR_007_6_1_Agent_Configuration_Change_Rules()
    {
        // 验证 ADR-007 文档包含 Agent 配置变更的规则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue($"❌ ADR-007_6_1 违规：ADR-007 文档不存在\n\n" +
            $"修复建议：确保 ADR-007 文档存在于 docs/adr/governance/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§6.1）");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证包含变更治理相关内容
        (content.Contains("变更") || content.Contains("Change") || content.Contains("治理")).Should().BeTrue(
            $"❌ ADR-007_6_1 违规：ADR-007 缺少 Agent 变更治理规则\n\n" +
            $"修复建议：添加 Agent 配置变更的审批和治理流程\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§6.1）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-007_6_2: Agent 变更判定规则验证")]
    public void ADR_007_6_2_Agent_Change_Decision_Rules()
    {
        // 验证 ADR-007 包含变更判定的规则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        // 验证文档包含判定规则
        (content.Contains("判定") || content.Contains("Decision")).Should().BeTrue(
            $"❌ ADR-007_6_2 违规：ADR-007 缺少变更判定规则\n\n" +
            $"修复建议：添加如何判定 Agent 配置变更是否合规的规则\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§6.2）");
        
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
