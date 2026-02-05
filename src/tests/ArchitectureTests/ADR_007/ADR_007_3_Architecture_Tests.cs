namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

using FluentAssertions;

/// <summary>
/// 验证 ADR-007_3：Agent 禁止的语义行为（Rule）
/// 验证 ADR-007_3_1 到 ADR-007_3_6
/// </summary>
public sealed class ADR_007_3_Architecture_Tests
{
    [Fact(DisplayName = "ADR-007_3_1: Agent 禁止解释性扩权")]
    public void ADR_007_3_1_Agent_Must_Not_Expand_Authority_By_Interpretation()
    {
        // 验证 ADR-007 文档存在并包含禁止解释性扩权的规则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue($"❌ ADR-007_3_1 违规：ADR-007 文档不存在\n\n" +
            $"修复建议：确保 ADR-007 文档存在于 docs/adr/governance/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§3.1）");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证必需章节存在
        content.Should().Contain("禁止解释性扩权", 
            $"❌ ADR-007_3_1 违规：ADR-007 缺少禁止解释性扩权的规则\n\n" +
            $"修复建议：添加禁止 Agent 通过解释扩大权限的明确规则\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§3.1）");
        
        // 验证文档包含清晰的语义禁止规则
        (content.Contains("Agent") && content.Contains("权限")).Should().BeTrue(
            $"❌ ADR-007_3_1 违规：ADR-007 未明确定义 Agent 权限边界\n\n" +
            $"修复建议：确保 ADR-007 包含 Agent 权限和行为的清晰定义\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§3.1）");
    }

    [Fact(DisplayName = "ADR-007_3_2: Agent 禁止替代性裁决")]
    public void ADR_007_3_2_Agent_Must_Not_Make_Alternative_Decisions()
    {
        // 验证 ADR-007 包含禁止替代性裁决的规则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        content.Should().Contain("禁止替代性裁决", 
            $"❌ ADR-007_3_2 违规：ADR-007 缺少禁止替代性裁决的规则\n\n" +
            $"修复建议：添加禁止 Agent 替代 ADR 进行裁决的明确规则\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§3.2）");
        
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "ADR-007_3_3: Agent 禁止模糊输出")]
    public void ADR_007_3_3_Agent_Must_Not_Produce_Ambiguous_Output()
    {
        // 验证 ADR-007 包含禁止模糊输出的规则
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md");
        
        File.Exists(adrFile).Should().BeTrue();
        var content = File.ReadAllText(adrFile);
        
        content.Should().Contain("模糊", 
            $"❌ ADR-007_3_3 违规：ADR-007 缺少禁止模糊输出的规则\n\n" +
            $"修复建议：添加要求 Agent 输出必须明确的规则\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§3.3）");
        
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
