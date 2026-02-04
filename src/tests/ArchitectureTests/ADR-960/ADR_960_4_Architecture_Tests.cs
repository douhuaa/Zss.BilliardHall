using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_960;

/// <summary>
/// ADR-960_4: Onboarding 文档维护与失效治理
/// 验证 Onboarding 文档的维护和失效治理机制
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-960_4_1: 绑定 ADR 演进
/// - ADR-960_4_2: 失效处理
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-960-onboarding-documentation-governance.md
/// </summary>
public sealed class ADR_960_4_Architecture_Tests
{
    /// <summary>
    /// ADR-960_4_1: 绑定 ADR 演进
    /// 验证 Onboarding 维护规则已定义（§ADR-960_4_1）
    /// </summary>
    [Fact(DisplayName = "ADR-960_4_1: Onboarding 维护规则必须已定义")]
    public void ADR_960_4_1_Onboarding_Maintenance_Rules_Must_Be_Defined()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr960Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-960-onboarding-documentation-governance.md");
        
        File.Exists(adr960Path).Should().BeTrue(
            $"❌ ADR-960_4_1 违规：ADR-960 文档不存在");
        
        var content = File.ReadAllText(adr960Path);
        
        // 验证定义了维护规则
        var hasMaintenanceRules = content.Contains("绑定 ADR 演进", StringComparison.OrdinalIgnoreCase) ||
                                 content.Contains("必须评估是否更新", StringComparison.OrdinalIgnoreCase);
        
        hasMaintenanceRules.Should().BeTrue(
            $"❌ ADR-960_4_1 违规：ADR-960 必须定义 Onboarding 维护规则\n\n" +
            $"维护规则应包括：\n" +
            $"  1. 当新 ADR 被采纳时\n" +
            $"  2. 当 ADR 结构发生重大调整时\n" +
            $"  3. 当新的核心 Case 出现时\n" +
            $"  4. 至少每半年一次有效性审计\n\n" +
            $"参考：docs/adr/governance/ADR-960-onboarding-documentation-governance.md §4.1");
        
        // 验证定义了审计频率
        var hasSemiAnnualAudit = content.Contains("半年", StringComparison.OrdinalIgnoreCase) ||
                                content.Contains("6 个月", StringComparison.OrdinalIgnoreCase);
        
        hasSemiAnnualAudit.Should().BeTrue(
            $"❌ ADR-960_4_1 违规：ADR-960 必须定义审计频率（至少每半年一次）\n\n" +
            $"参考：docs/adr/governance/ADR-960-onboarding-documentation-governance.md §4.1");
    }

    /// <summary>
    /// ADR-960_4_2: 失效处理
    /// 验证 Onboarding 失效处理机制已定义（§ADR-960_4_2）
    /// </summary>
    [Fact(DisplayName = "ADR-960_4_2: Onboarding 失效处理机制必须已定义")]
    public void ADR_960_4_2_Onboarding_Deprecation_Mechanism_Must_Be_Defined()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr960Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-960-onboarding-documentation-governance.md");
        
        File.Exists(adr960Path).Should().BeTrue(
            $"❌ ADR-960_4_2 违规：ADR-960 文档不存在");
        
        var content = File.ReadAllText(adr960Path);
        
        // 验证定义了失效处理流程
        var hasDeprecationProcess = content.Contains("失效处理", StringComparison.OrdinalIgnoreCase) ||
                                   content.Contains("内容误导", StringComparison.OrdinalIgnoreCase);
        
        hasDeprecationProcess.Should().BeTrue(
            $"❌ ADR-960_4_2 违规：ADR-960 必须定义 Onboarding 失效处理流程\n\n" +
            $"失效处理应包括：\n" +
            $"  1. 发现内容误导 → 立即修复\n" +
            $"  2. 无法及时修复 → 标记 [可能过时]\n" +
            $"  3. 不允许长期错误但'懒得改'\n\n" +
            $"参考：docs/adr/governance/ADR-960-onboarding-documentation-governance.md §4.2");
        
        // 验证禁止过时内容长期存在
        var forbidsOutdatedContent = content.Contains("不允许", StringComparison.OrdinalIgnoreCase) &&
                                    (content.Contains("过时", StringComparison.OrdinalIgnoreCase) ||
                                     content.Contains("废弃", StringComparison.OrdinalIgnoreCase));
        
        forbidsOutdatedContent.Should().BeTrue(
            $"❌ ADR-960_4_2 违规：ADR-960 必须明确禁止过时内容长期存在\n\n" +
            $"参考：docs/adr/governance/ADR-960-onboarding-documentation-governance.md §4.2");
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
