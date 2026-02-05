namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_900;

/// <summary>
/// ADR-900_2: 执行级别与测试映射
/// 验证架构规则的执行级别分类和 ADR ↔ 测试映射关系
///
/// 测试覆盖映射（严格遵循 ADR-900 v4.0 Rule/Clause 体系）：
/// - ADR-900_2_1: 执行级别分离原则 → ADR_900_2_1_Enforcement_Levels_Must_Be_Classified
/// - ADR-900_2_2: ADR ↔ 测试 ↔ CI 的一一映射 → ADR_900_2_2_ADR_Test_CI_Mapping_Required
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-900-architecture-tests.md
/// </summary>
public sealed class ADR_900_2_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const string AdrTestsPath = "src/tests/ArchitectureTests";

    /// <summary>
    /// ADR-900_2_1: 执行级别分离原则
    /// 验证所有架构规则必须被归类到 L1/L2/L3 执行级别之一
    /// </summary>
    [Fact(DisplayName = "ADR-900_2_1: 所有架构规则必须分类到执行级别")]
    public void ADR_900_2_1_Enforcement_Levels_Must_Be_Classified()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        // 获取所有 ADR 文件
        var adrFiles = Directory.GetFiles(adrDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\archive\\") && !f.Contains("\\README.md"))
            .ToArray();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 检查是否包含 Enforcement 章节
            content.Should().Contain("## Enforcement",
                $"❌ ADR-900_2_1 违规：{fileName} 未包含 Enforcement 章节\n\n" +
                $"修复建议：\n" +
                $"  为 ADR 添加 Enforcement 章节，定义各条款的执行级别\n\n" +
                $"参考：docs/adr/governance/ADR-900-architecture-tests.md §2.1");

            // 检查是否定义了执行级别
            var hasL1 = content.Contains("L1") || content.Contains("阻断级");
            var hasL2 = content.Contains("L2") || content.Contains("警告级");
            var hasL3 = content.Contains("L3") || content.Contains("人工级");

            (hasL1 || hasL2 || hasL3).Should().BeTrue(
                $"❌ ADR-900_2_1 违规：{fileName} 未定义任何执行级别 (L1/L2/L3)\n\n" +
                $"修复建议：\n" +
                $"  在 Enforcement 章节中为每个条款指定执行级别\n\n" +
                $"参考：docs/adr/governance/ADR-900-architecture-tests.md §2.1");
        }
    }

    /// <summary>
    /// ADR-900_2_2: ADR ↔ 测试 ↔ CI 的一一映射
    /// 验证所有可执法 ADR 条款都有对应的测试和 CI 映射
    /// </summary>
    [Fact(DisplayName = "ADR-900_2_2: ADR 条款必须有测试和 CI 映射")]
    public void ADR_900_2_2_ADR_Test_CI_Mapping_Required()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath);

        // 获取所有 ADR 文件
        var adrFiles = Directory.GetFiles(adrDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\archive\\") && !f.Contains("\\README.md"))
            .ToArray();

        // 获取所有测试文件
        var testFiles = Directory.GetFiles(testsDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => f.Contains("Architecture_Tests.cs"))
            .ToArray();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 提取 ADR 编号
            var adrNumber = ExtractAdrNumber(fileName);
            if (string.IsNullOrEmpty(adrNumber)) continue;

            // 检查是否有对应的测试文件
            var hasTestFile = testFiles.Any(f => Path.GetFileName(f).Contains($"ADR_{adrNumber}"));
            hasTestFile.Should().BeTrue(
                $"❌ ADR-900_2_2 违规：ADR {adrNumber} 缺少对应的 ArchitectureTests\n\n" +
                $"预期测试文件：ADR_{adrNumber}_*_Architecture_Tests.cs\n\n" +
                $"修复建议：\n" +
                $"  1. 为 ADR-{adrNumber} 创建对应的 ArchitectureTests\n" +
                $"  2. 确保测试覆盖所有可执法条款\n\n" +
                $"参考：docs/adr/governance/ADR-900-architecture-tests.md §2.2");

            // 检查 Enforcement 表格是否包含测试映射
            if (content.Contains("## Enforcement"))
            {
                content.Should().Contain("ArchitectureTests",
                    $"❌ ADR-900_2_2 违规：ADR {adrNumber} 的 Enforcement 章节未提及 ArchitectureTests\n\n" +
                    $"修复建议：\n" +
                    $"  在 Enforcement 表格的'执法方式'列中指定 ArchitectureTests\n\n" +
                    $"参考：docs/adr/governance/ADR-900-architecture-tests.md §2.2");
            }
        }
    }

    private static string ExtractAdrNumber(string fileName)
    {
        // 提取 ADR-XXX 或 ADR_XXX 中的编号
        var match = System.Text.RegularExpressions.Regex.Match(fileName, @"ADR[-_](?<number>\d+)");
        return match.Success ? match.Groups["number"].Value : null;
    }
}
