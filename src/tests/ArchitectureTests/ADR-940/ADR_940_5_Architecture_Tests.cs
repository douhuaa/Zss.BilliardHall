namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-940_5: ADR 关系图生成规范（Rule）
/// 验证 ADR 关系图可以正常生成
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-940_5_1: ADR 关系图可以成功生成
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-940-adr-relationship-and-traceability.md
/// </summary>
public sealed class ADR_940_5_Architecture_Tests
{
    /// <summary>
    /// ADR-940_5_1: ADR 关系图可以成功生成
    /// 验证关系图生成工具可以正常运行并生成有效的关系图文档（§ADR-940_5_1）
    /// </summary>
    [Fact(DisplayName = "ADR-940_5_1: ADR 关系图可以成功生成")]
    public void ADR_940_5_1_Relationship_Map_Can_Be_Generated()
    {
        var repoRoot = TestEnvironment.RepositoryRoot
            ?? throw new InvalidOperationException("未找到仓库根目录");

        var adrPath = Path.Combine(repoRoot, "docs", "adr");
        var outputPath = Path.Combine(Path.GetTempPath(), $"ADR-RELATIONSHIP-MAP-{Guid.NewGuid()}.md");

        try
        {
            // 生成关系图
            AdrRelationshipMapGenerator.GenerateRelationshipMap(adrPath, outputPath);

            // 验证文件已生成
            File.Exists(outputPath).Should().BeTrue("关系图文件应该被创建");

            // 验证文件不为空
            var content = File.ReadAllText(outputPath);
            string.IsNullOrWhiteSpace(content).Should().BeFalse("关系图内容不应为空");

            // 验证包含基本结构
            content.Should().Contain("# ADR 关系图");
            content.Should().Contain("## 统计");
            content.Should().Contain("## ADR 列表");
        }
        finally
        {
            // 清理临时文件
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }
}
