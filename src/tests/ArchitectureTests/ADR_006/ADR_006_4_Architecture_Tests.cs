namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_006;

using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// ADR-006_4: 目录归属规则
/// 验证 ADR-006_4_1：目录层级映射
/// </summary>
public sealed class ADR_006_4_Architecture_Tests
{
    private readonly string _adrRoot;

    public ADR_006_4_Architecture_Tests()
    {
        _adrRoot = Path.Combine(TestEnvironment.RepositoryRoot, "docs", "adr");
    }

    [Fact(DisplayName = "ADR-006_4_1: ADR 文件必须位于正确的目录")]
    public void ADR_006_4_1_ADR_Files_Must_Be_In_Correct_Directory()
    {
        var mapping = new Dictionary<string, (int Min, int Max)>
        {
            ["constitutional"] = (1, 9),
            ["governance"] = (0, 0), // ADR-000 and 900-999 special case
            ["structure"] = (100, 199),
            ["runtime"] = (200, 299),
            ["technical"] = (300, 399)
        };

        foreach (var (directory, (min, max)) in mapping)
        {
            var dirPath = Path.Combine(_adrRoot, directory);
            if (!Directory.Exists(dirPath)) continue;

            var adrFiles = Directory.GetFiles(dirPath, "ADR-*.md", SearchOption.TopDirectoryOnly);

            foreach (var file in adrFiles)
            {
                var fileName = Path.GetFileName(file);
                var match = System.Text.RegularExpressions.Regex.Match(fileName, @"^ADR-(\d+)");

                if (match.Success)
                {
                    var number = int.Parse(match.Groups[1].Value);
                    var isInCorrectRange = number is >= 1 and <= 9 && directory == "constitutional" ||
                                          (number == 0 || number >= 900) && directory == "governance" ||
                                          number is >= 100 and <= 199 && directory == "structure" ||
                                          number is >= 200 and <= 299 && directory == "runtime" ||
                                          number is >= 300 and <= 399 && directory == "technical";

                    isInCorrectRange.Should().BeTrue($"❌ ADR-006_4_1 违规: ADR 文件位于错误的目录\n\n" +
                        $"文件: {file}\n" +
                        $"编号: ADR-{number:D3}\n" +
                        $"当前目录: {directory}/\n\n" +
                        $"正确目录映射:\n" +
                        $"- 001~009: constitutional/ （宪法层）\n" +
                        $"- 000, 900~999: governance/ （治理层）\n" +
                        $"- 100~199: structure/ （结构层）\n" +
                        $"- 200~299: runtime/ （运行层）\n" +
                        $"- 300~399: technical/ （技术层）\n\n" +
                        $"修复建议：\n" +
                        $"1. 将文件移动到正确的目录\n" +
                        $"2. 更新所有交叉引用该 ADR 的文档路径\n" +
                        $"3. 确保测试中的路径引用也随之更新\n\n" +
                        $"参考: ADR-006_4_1 - 目录层级映射");
                }
            }
        }
    }

    private List<string> GetAllAdrFiles()
    {
        var adrFiles = new List<string>();
        var directories = new[] { "constitutional", "governance", "structure", "runtime", "technical" };

        foreach (var dir in directories)
        {
            var dirPath = Path.Combine(_adrRoot, dir);
            if (Directory.Exists(dirPath))
            {
                adrFiles.AddRange(Directory.GetFiles(dirPath, "ADR-*.md", SearchOption.TopDirectoryOnly));
            }
        }

        return adrFiles;
    }
}
