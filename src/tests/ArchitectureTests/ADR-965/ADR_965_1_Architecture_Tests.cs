namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_965;

/// <summary>
/// ADR-965_1: 互动式清单设计
/// 验证 Onboarding 互动式学习路径的清单设计规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-965_1_1: 必须包含可互动的任务清单
/// - ADR-965_1_2: 清单格式（GitHub Issue Template）
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md
/// - version: 2.0
/// </summary>
public sealed class ADR_965_1_Architecture_Tests
{
    /// <summary>
    /// ADR-965_1_1: 互动清单存在性验证
    /// 验证 ADR-965 定义了可互动的任务清单（§ADR-965_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-965_1_1: 必须包含可互动的任务清单")]
    public void ADR_965_1_1_Must_Include_Interactive_Checklist()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr965Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        File.Exists(adr965Path).Should().BeTrue(
            $"❌ ADR-965_1_1 违规：ADR-965 文档不存在\n\n" +
            $"参考：docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md §1.1");

        var content = File.ReadAllText(adr965Path);

        // 验证包含互动清单的说明
        content.Should().Contain("可互动",
            $"❌ ADR-965_1_1 违规：ADR-965 必须说明包含可互动的任务清单\n\n" +
            $"参考：docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md §1.1");
    }

    /// <summary>
    /// ADR-965_1_2: GitHub Issue Template 验证
    /// 验证定义了使用 GitHub Issue Template 的规范（§ADR-965_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-965_1_2: 必须使用 GitHub Issue Template")]
    public void ADR_965_1_2_Must_Use_GitHub_Issue_Template()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr965Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        var content = File.ReadAllText(adr965Path);

        // 验证定义了 GitHub Issue Template 格式
        content.Should().Contain("GitHub Issue Template",
            $"❌ ADR-965_1_2 违规：ADR-965 必须定义 GitHub Issue Template 格式\n\n" +
            $"参考：docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md §1.2");

        // 验证包含第1-4周的清单结构
        content.Should().Contain("第 1 周",
            $"❌ ADR-965_1_2 违规：必须包含第 1 周学习清单");
    }

}
