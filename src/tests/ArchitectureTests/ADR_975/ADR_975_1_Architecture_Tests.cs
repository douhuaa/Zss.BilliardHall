namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_975;

/// <summary>
/// ADR-975_1: 质量指标定义
/// 验证文档质量指标的定义和量化评估标准
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-975_1_1: 文档质量必须通过指标量化评估
/// - ADR-975_1_2: 核心指标验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-975-documentation-quality-monitoring.md
/// - version: 2.0
/// </summary>
public sealed class ADR_975_1_Architecture_Tests
{
    /// <summary>
    /// ADR-975_1_1: 质量指标存在性验证
    /// 验证 ADR-975 定义了质量指标（§ADR-975_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-975_1_1: 必须定义文档质量指标")]
    public void ADR_975_1_1_Quality_Metrics_Must_Be_Defined()
    {
        var adr975Path = FileSystemTestHelper.GetAbsolutePath("docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        File.Exists(adr975Path).Should().BeTrue(
            $"❌ ADR-975_1_1 违规：ADR-975 文档不存在\n\n" +
            $"参考：docs/adr/governance/ADR-975-documentation-quality-monitoring.md §1.1");

        var content = FileSystemTestHelper.ReadFileContent(adr975Path);

        // 验证定义了质量指标
        content.Should().Contain("质量指标",
            $"❌ ADR-975_1_1 违规：ADR-975 必须定义质量指标\n\n" +
            $"参考：docs/adr/governance/ADR-975-documentation-quality-monitoring.md §1.1");
    }

    /// <summary>
    /// ADR-975_1_2: 核心指标验证
    /// 验证定义了核心质量指标（§ADR-975_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-975_1_2: 必须定义核心质量指标")]
    public void ADR_975_1_2_Core_Metrics_Must_Be_Defined()
    {
        var adr975Path = FileSystemTestHelper.GetAbsolutePath("docs/adr/governance/ADR-975-documentation-quality-monitoring.md");

        var content = FileSystemTestHelper.ReadFileContent(adr975Path);

        // 验证包含核心指标
        var requiredMetrics = new[] { "准确性", "完整性", "时效性", "链接有效性" };
        foreach (var metric in requiredMetrics)
        {
            content.Should().Contain(metric,
                $"❌ ADR-975_1_2 违规：核心指标必须包含 '{metric}'\n\n" +
                $"参考：docs/adr/governance/ADR-975-documentation-quality-monitoring.md §1.2");
        }
    }

}
