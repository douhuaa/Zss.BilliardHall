using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_970;

/// <summary>
/// ADR-970_1: 日志分类与存储位置
/// 验证自动化工具日志的标准化存储规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-970_1_1: 日志必须按类型存储在标准位置
/// - ADR-970_1_2: 存储结构验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-970-automation-log-integration-standard.md
/// - version: 2.0
/// </summary>
public sealed class ADR_970_1_Architecture_Tests
{
    /// <summary>
    /// ADR-970_1_1: 日志存储位置验证
    /// 验证 ADR-970 定义了日志标准存储位置（§ADR-970_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-970_1_1: 日志必须按类型存储在标准位置")]
    public void ADR_970_1_1_Logs_Must_Be_Stored_In_Standard_Locations()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr970Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-970-automation-log-integration-standard.md");

        File.Exists(adr970Path).Should().BeTrue(
            $"❌ ADR-970_1_1 违规：ADR-970 文档不存在\n\n" +
            $"参考：docs/adr/governance/ADR-970-automation-log-integration-standard.md §1.1");

        var content = File.ReadAllText(adr970Path);

        // 验证定义了存储结构
        content.Should().Contain("docs/reports/",
            $"❌ ADR-970_1_1 违规：ADR-970 必须定义标准日志存储位置\n\n" +
            $"参考：docs/adr/governance/ADR-970-automation-log-integration-standard.md §1.1");
    }

    /// <summary>
    /// ADR-970_1_2: 存储结构验证
    /// 验证定义了完整的目录结构（§ADR-970_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-970_1_2: 必须定义完整的存储结构")]
    public void ADR_970_1_2_Storage_Structure_Must_Be_Defined()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr970Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-970-automation-log-integration-standard.md");

        var content = File.ReadAllText(adr970Path);

        // 验证包含主要子目录
        var requiredDirs = new[] { "architecture-tests", "dependencies", "security", "builds", "tests" };
        foreach (var dir in requiredDirs)
        {
            content.Should().Contain(dir,
                $"❌ ADR-970_1_2 违规：存储结构必须包含 '{dir}' 目录\n\n" +
                $"参考：docs/adr/governance/ADR-970-automation-log-integration-standard.md §1.2");
        }
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
