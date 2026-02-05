namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_006;

/// <summary>
/// ADR-006_3: 前导零规则
/// 验证 ADR-006_3_1：前导零强制要求
/// </summary>
public sealed class ADR_006_3_Architecture_Tests
{
    private readonly string _adrRoot;

    public ADR_006_3_Architecture_Tests()
    {
        _adrRoot = Path.Combine(TestEnvironment.RepositoryRoot, "docs", "adr");
    }

    [Fact(DisplayName = "ADR-006_3_1: 所有 ADR 必须使用3位前导零格式")]
    public void ADR_006_3_1_All_ADRs_Must_Use_Leading_Zeros()
    {
        var adrFiles = GetAllAdrFiles();

        foreach (var file in adrFiles)
        {
            var fileName = Path.GetFileName(file);
            var match = System.Text.RegularExpressions.Regex.Match(fileName, @"^ADR-(\d+)");

            if (match.Success)
            {
                var numberStr = match.Groups[1].Value;
                var number = int.Parse(numberStr);

                // All ADR numbers must be exactly 3 digits (000-999)
                (numberStr.Length == 3).Should().BeTrue($"❌ ADR-006_3_1 违规: ADR 编号必须使用前导零\n\n" +
                    $"文件: {file}\n" +
                    $"当前编号: ADR-{numberStr}\n" +
                    $"正确编号: ADR-{number:D3}\n\n" +
                    $"问题分析:\n" +
                    $"编号小于100时必须使用前导零，确保统一的3位格式\n\n" +
                    $"修复建议：\n" +
                    $"1. 重命名文件为 ADR-{number:D3}-xxx.md\n" +
                    $"2. 更新文件内容中的 ADR 编号引用\n" +
                    $"3. 更新所有交叉引用该 ADR 的文档\n\n" +
                    $"编号规范:\n" +
                    $"- 宪法层: ADR-001 到 ADR-009\n" +
                    $"- 结构层: ADR-100 到 ADR-199\n" +
                    $"- 运行层: ADR-200 到 ADR-299\n" +
                    $"- 技术层: ADR-300 到 ADR-399\n" +
                    $"- 治理层: ADR-900 到 ADR-999\n\n" +
                    $"参考: ADR-006_3_1 - 前导零强制要求");

                // Specifically check for numbers < 100
                if (number < 100)
                {
                    numberStr.Should().StartWith("0",
                        $"❌ ADR-006_3_1 违规: 小于100的 ADR 编号必须使用前导零\n\n" +
                        $"文件: {file}\n" +
                        $"当前编号: ADR-{numberStr}\n" +
                        $"正确编号: ADR-{number:D3}\n\n" +
                        $"参考: ADR-006_3_1 - 前导零强制要求");
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
