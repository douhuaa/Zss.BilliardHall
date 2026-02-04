using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_970;

/// <summary>
/// ADR-970_4: 自动化报告生成
/// 验证 CI 必须自动生成结构化日志的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-970_4_1: CI 必须自动生成结构化日志
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-970-automation-log-integration-standard.md
/// - version: 2.0
/// </summary>
public sealed class ADR_970_4_Architecture_Tests
{
    [Fact(DisplayName = "ADR-970_4_1: CI 必须自动生成结构化日志")]
    public void ADR_970_4_1_CI_Must_Generate_Structured_Logs()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr970Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-970-automation-log-integration-standard.md");

        var content = File.ReadAllText(adr970Path);

        content.Should().Contain("CI 必须自动生成结构化日志",
            $"❌ ADR-970_4_1 违规：ADR-970 必须要求 CI 自动生成结构化日志\n\n" +
            $"参考：docs/adr/governance/ADR-970-automation-log-integration-standard.md §4.1");
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
