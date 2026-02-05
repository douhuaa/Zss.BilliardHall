using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR 关系图生成测试
/// 验证关系图可以正常生成
/// </summary>
public sealed class AdrRelationshipMapGenerationTests
{
    /// <summary>
    /// 验证可以成功生成 ADR 关系图
    /// </summary>
    [Fact(DisplayName = "ADR 关系图可以成功生成")]
    public void ADR_Relationship_Map_Can_Be_Generated()
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

    /// <summary>
    /// 查找仓库根目录
    /// </summary>
}
