using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_965;

/// <summary>
/// ADR-965_2: 学习路径可视化
/// 验证 Onboarding 学习路径的可视化规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-965_2_1: 必须包含可视化学习路径图
/// - ADR-965_2_2: 路径图位置验证
/// - ADR-965_2_3: 可视化格式（Mermaid）验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md
/// - version: 2.0
/// </summary>
public sealed class ADR_965_2_Architecture_Tests
{
    /// <summary>
    /// ADR-965_2_1: 可视化路径图存在性验证
    /// 验证 ADR-965 定义了可视化学习路径图（§ADR-965_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-965_2_1: 必须包含可视化学习路径图")]
    public void ADR_965_2_1_Must_Include_Visual_Learning_Path()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr965Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        var content = File.ReadAllText(adr965Path);

        // 验证包含可视化路径图的说明
        content.Should().Contain("可视化学习路径图",
            $"❌ ADR-965_2_1 违规：ADR-965 必须说明包含可视化学习路径图\n\n" +
            $"参考：docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md §2.1");
    }

    /// <summary>
    /// ADR-965_2_2: 路径图位置验证
    /// 验证定义了路径图的标准位置（§ADR-965_2_2）
    /// </summary>
    [Fact(DisplayName = "ADR-965_2_2: 必须定义路径图位置")]
    public void ADR_965_2_2_Path_Location_Must_Be_Defined()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr965Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        var content = File.ReadAllText(adr965Path);

        // 验证定义了路径图位置
        content.Should().Contain("docs/onboarding/README.md",
            $"❌ ADR-965_2_2 违规：ADR-965 必须定义路径图位置为 docs/onboarding/README.md\n\n" +
            $"参考：docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md §2.2");
    }

    /// <summary>
    /// ADR-965_2_3: 可视化格式验证
    /// 验证定义了使用 Mermaid 图表格式（§ADR-965_2_3）
    /// </summary>
    [Fact(DisplayName = "ADR-965_2_3: 必须使用 Mermaid 格式")]
    public void ADR_965_2_3_Must_Use_Mermaid_Format()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr965Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md");

        var content = File.ReadAllText(adr965Path);

        // 验证定义了 Mermaid 格式
        content.Should().Contain("Mermaid",
            $"❌ ADR-965_2_3 违规：ADR-965 必须定义使用 Mermaid 图表格式\n\n" +
            $"参考：docs/adr/governance/ADR-965-onboarding-interactive-learning-path.md §2.3");

        // 验证包含 Mermaid 代码块示例
        content.Should().Contain("```mermaid",
            $"❌ ADR-965_2_3 违规：ADR-965 必须包含 Mermaid 图表示例");
    }

    private static string? FindRepositoryRoot()
    {
        var directory = Directory.GetCurrentDirectory();
        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory, ".git")))
            {
                return directory;
            }
            directory = Directory.GetParent(directory)?.FullName;
        }
        return null;
    }
}
