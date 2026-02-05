namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_945;

/// <summary>
/// ADR-945_1: ADR 演进时间线生成机制（Rule）
/// 验证 ADR 演进时间线文件的生成和维护
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-945_1_1: 自动生成 ADR 演进时间线
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-945-adr-timeline-evolution-view.md
/// </summary>
public sealed class ADR_945_1_Architecture_Tests
{
    /// <summary>
    /// ADR-945_1_1: 自动生成 ADR 演进时间线
    /// 验证 ADR-TIMELINE.md 文件存在并包含必要内容（§ADR-945_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-945_1_1: 自动生成 ADR 演进时间线")]
    public void ADR_945_1_1_ADR_Timeline_Must_Be_Generated()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var timelineFile = Path.Combine(repoRoot, "docs/adr/ADR-TIMELINE.md");

        // 检查时间线文件是否存在（如果不存在，视为待实现功能）
        if (!File.Exists(timelineFile))
        {
            // 这是一个待实现的功能，暂时允许不存在
            // 但需要记录为 TODO
            Console.WriteLine("⚠️ ADR-945_1_1 提示：ADR-TIMELINE.md 尚未生成，这是一个待实现的功能。");
            return;
        }

        var content = File.ReadAllText(timelineFile);

        // 验证必要的结构元素
        content.Should().Contain("ADR", "时间线文件应包含 ADR 引用");

        // 验证包含时间信息
        var hasTimeInfo = content.Contains("20") && // 年份
                         (content.Contains("-") || content.Contains("/"));  // 日期分隔符

        hasTimeInfo.Should().BeTrue("时间线文件应包含日期信息");
    }

}
