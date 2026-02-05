namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_965;

/// <summary>
/// ADR-965_3: 进度跟踪机制
/// 验证 Onboarding 进度跟踪机制的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-965_3_1: 必须实时追踪 Onboarding 进度
/// - ADR-965_3_2: GitHub Issue 进度条验证
/// - ADR-965_3_3: Project Board 集成验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md
/// - version: 2.0
/// </summary>
public sealed class ADR_965_3_Architecture_Tests
{
    /// <summary>
    /// ADR-965_3_1: 进度跟踪机制存在性验证
    /// 验证 ADR-965 定义了实时进度跟踪机制（§ADR-965_3_1）
    /// </summary>
    [Fact(DisplayName = "ADR-965_3_1: 必须实时追踪 Onboarding 进度")]
    public void ADR_965_3_1_Must_Track_Progress_Realtime()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr965Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        var content = File.ReadAllText(adr965Path);

        // 验证包含进度跟踪机制的说明
        content.Should().Contain("实时追踪",
            $"❌ ADR-965_3_1 违规：ADR-965 必须说明实时追踪 Onboarding 进度\n\n" +
            $"参考：docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md §3.1");
    }

    /// <summary>
    /// ADR-965_3_2: GitHub Issue 进度条验证
    /// 验证定义了 GitHub Issue 进度条机制（§ADR-965_3_2）
    /// </summary>
    [Fact(DisplayName = "ADR-965_3_2: 必须使用 GitHub Issue 进度条")]
    public void ADR_965_3_2_Must_Use_GitHub_Issue_Progress()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr965Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        var content = File.ReadAllText(adr965Path);

        // 验证定义了 GitHub Issue 进度条
        content.Should().Contain("GitHub Issue 进度条",
            $"❌ ADR-965_3_2 违规：ADR-965 必须定义 GitHub Issue 进度条机制\n\n" +
            $"参考：docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md §3.2");
    }

    /// <summary>
    /// ADR-965_3_3: Project Board 集成验证
    /// 验证定义了 Project Board 集成（§ADR-965_3_3）
    /// </summary>
    [Fact(DisplayName = "ADR-965_3_3: 必须集成 Project Board")]
    public void ADR_965_3_3_Must_Integrate_Project_Board()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr965Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        var content = File.ReadAllText(adr965Path);

        // 验证定义了 Project Board 集成
        content.Should().Contain("Project Board",
            $"❌ ADR-965_3_3 违规：ADR-965 必须定义 Project Board 集成\n\n" +
            $"参考：docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md §3.3");
    }

}
