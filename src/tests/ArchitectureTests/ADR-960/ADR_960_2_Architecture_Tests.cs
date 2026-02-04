using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_960;

/// <summary>
/// ADR-960_2: Onboarding 与其他文档的分离边界
/// 验证 Onboarding 文档与其他文档类型的分离边界
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-960_2_1: 内容类型限制
/// - ADR-960_2_2: 核心原则
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-960-onboarding-documentation-governance.md
/// </summary>
public sealed class ADR_960_2_Architecture_Tests
{
    /// <summary>
    /// ADR-960_2_1: 内容类型限制
    /// 验证 Onboarding 文档不包含禁止的内容类型（§ADR-960_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-960_2_1: Onboarding 必须遵循内容类型限制")]
    public void ADR_960_2_1_Onboarding_Must_Follow_Content_Type_Restrictions()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr960Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-960-onboarding-documentation-governance.md");
        
        File.Exists(adr960Path).Should().BeTrue(
            $"❌ ADR-960_2_1 违规：ADR-960 文档不存在");
        
        var content = File.ReadAllText(adr960Path);
        
        // 验证定义了内容类型限制表格
        var hasContentTypeTable = content.Contains("| 内容类型", StringComparison.OrdinalIgnoreCase) &&
                                 content.Contains("是否允许出现在 Onboarding", StringComparison.OrdinalIgnoreCase);
        
        hasContentTypeTable.Should().BeTrue(
            $"❌ ADR-960_2_1 违规：ADR-960 必须定义 Onboarding 的内容类型限制表\n\n" +
            $"参考：docs/adr/governance/ADR-960-onboarding-documentation-governance.md §2.1");
        
        // 验证表格包含关键内容类型限制
        content.Should().Contain("架构约束定义",
            $"❌ ADR-960_2_1 违规：内容类型表必须包含'架构约束定义'");
        
        content.Should().Contain("示例代码",
            $"❌ ADR-960_2_1 违规：内容类型表必须包含'示例代码'限制");
    }

    /// <summary>
    /// ADR-960_2_2: 核心原则
    /// 验证 Onboarding 文档遵循三个核心问题原则（§ADR-960_2_2）
    /// </summary>
    [Fact(DisplayName = "ADR-960_2_2: Onboarding 必须遵循三个核心问题原则")]
    public void ADR_960_2_2_Onboarding_Must_Follow_Core_Principles()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr960Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-960-onboarding-documentation-governance.md");
        
        File.Exists(adr960Path).Should().BeTrue(
            $"❌ ADR-960_2_2 违规：ADR-960 文档不存在");
        
        var content = File.ReadAllText(adr960Path);
        
        // 验证定义了三个核心问题
        var hasThreeCoreQuestions = content.Contains("我是谁", StringComparison.OrdinalIgnoreCase) &&
                                   content.Contains("我先看什么", StringComparison.OrdinalIgnoreCase) &&
                                   content.Contains("我下一步去哪", StringComparison.OrdinalIgnoreCase);
        
        hasThreeCoreQuestions.Should().BeTrue(
            $"❌ ADR-960_2_2 违规：ADR-960 必须定义 Onboarding 的三个核心问题\n\n" +
            $"核心原则：\n" +
            $"  1. 我是谁（这个项目是什么）\n" +
            $"  2. 我先看什么\n" +
            $"  3. 我下一步去哪\n\n" +
            $"参考：docs/adr/governance/ADR-960-onboarding-documentation-governance.md §2.2");
    }

    // ========== 辅助方法 ==========

    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) || 
                Directory.Exists(Path.Combine(currentDir, "docs", "adr")) ||
                File.Exists(Path.Combine(currentDir, "Zss.BilliardHall.slnx")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return null;
    }
}
