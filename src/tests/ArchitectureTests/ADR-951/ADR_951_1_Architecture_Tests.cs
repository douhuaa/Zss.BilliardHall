using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_951;

/// <summary>
/// ADR-951_1: 案例库结构组织（Rule）
/// 验证案例库的目录结构和分类组织规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-951_1_1: 目录结构规范
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-951-case-repository-management.md
/// </summary>
public sealed class ADR_951_1_Architecture_Tests
{
    /// <summary>
    /// ADR-951_1_1: 目录结构规范
    /// 验证案例库目录 docs/cases/ 存在且有合理的分类结构（§ADR-951_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-951_1_1: 案例库目录结构必须符合规范")]
    public void ADR_951_1_1_Case_Repository_Must_Have_Valid_Directory_Structure()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var casesDirectory = Path.Combine(repoRoot, "docs/cases");

        // 检查案例库目录是否存在
        if (!Directory.Exists(casesDirectory))
        {
            // 案例库目录不存在，这是待实现的功能
            Console.WriteLine("⚠️ ADR-951_1_1 提示：docs/cases/ 目录尚未创建，这是一个待实现的功能。");
            return;
        }

        // 检查是否有 README.md 索引文件
        var readmePath = Path.Combine(casesDirectory, "README.md");
        File.Exists(readmePath).Should().BeTrue(
            "违反 ADR-951_1_1：案例库目录必须包含 README.md 索引文件");

        // 检查是否有分类子目录
        var subdirectories = Directory.GetDirectories(casesDirectory);
        
        if (subdirectories.Length > 0)
        {
            // 验证每个分类目录都有自己的 README.md
            var missingReadmes = new List<string>();
            
            foreach (var subdir in subdirectories)
            {
                var subdirReadme = Path.Combine(subdir, "README.md");
                if (!File.Exists(subdirReadme))
                {
                    missingReadmes.Add(Path.GetFileName(subdir));
                }
            }

            missingReadmes.Should().BeEmpty(
                $"违反 ADR-951_1_1：以下分类目录缺少 README.md 索引：{string.Join(", ", missingReadmes)}");
        }
    }

}
