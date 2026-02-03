namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// ADR-006: 术语与编号宪法
/// 验证 ADR 编号体系和文件命名规范
/// 
/// 测试映射：
/// - All_ADR_Files_Should_Match_Numbering_Rules → ADR-006.1 (ADR 文件编号应符合分段规则)
/// - All_ADR_Files_Should_Have_Correct_Filename_Format → ADR-006.2 (ADR 文件名应与编号一致)
/// - Constitutional_ADRs_Should_Use_Leading_Zeros → ADR-006.3 (宪法层 ADR 应使用前导零)
/// - Non_Constitutional_ADRs_Should_Not_Use_Leading_Zeros → ADR-006.4 (非宪法层 ADR 不应使用前导零)
/// - All_ADR_Files_Should_Be_In_Correct_Directory → ADR-006.5 (ADR 文件应位于正确的目录)
/// </summary>
public sealed class ADR_006_Architecture_Tests
{
    private readonly string _adrRoot;

    public ADR_006_Architecture_Tests()
    {
        _adrRoot = Path.Combine(TestEnvironment.RepositoryRoot, "docs", "adr");
    }

    [Fact(DisplayName = "ADR-006_1_1: ADR 文件编号应符合分段规则")]
    public void All_ADR_Files_Should_Match_Numbering_Rules()
    {
        var adrFiles = GetAllAdrFiles();

        foreach (var file in adrFiles)
        {
            var fileName = Path.GetFileName(file);
            var match = System.Text.RegularExpressions.Regex.Match(fileName, @"^ADR-(\d+)");

            match.Success.Should().BeTrue($"❌ ADR-006_1_1 违规: ADR 文件名必须以 'ADR-数字' 开头\n\n" +
                $"文件: {file}\n" +
                $"文件名: {fileName}\n\n" +
                $"修复建议:\n" +
                $"1. 文件名必须以 ADR-XXXX 格式开头\n" +
                $"2. 宪法层（0001~0009）使用前导零\n" +
                $"3. 其他层（100+）不使用前导零\n\n" +
                $"参考: docs/copilot/adr-006.prompts.md (场景 5)");

            if (match.Success)
            {
                var number = int.Parse(match.Groups[1].Value);
                var directory = Path.GetFileName(Path.GetDirectoryName(file)!);

                // Validate numbering ranges
                if (number is >= 1 and <= 9)
                {
                    directory.Should().Be("constitutional");
                }
                else if (number == 0 || number is >= 900 and <= 999)
                {
                    directory.Should().Be("governance");
                }
                else if (number is >= 100 and <= 199)
                {
                    directory.Should().Be("structure");
                }
                else if (number is >= 200 and <= 299)
                {
                    directory.Should().Be("runtime");
                }
                else if (number is >= 300 and <= 399)
                {
                    directory.Should().Be("technical");
                }
            }
        }
    }

    [Fact(DisplayName = "ADR-006_1_2: ADR 文件名应与编号一致")]
    public void All_ADR_Files_Should_Have_Correct_Filename_Format()
    {
        var adrFiles = GetAllAdrFiles();

        // Historical exceptions (grandfathered in)
        var historicalExceptions = new[]
        {
            "ADR-004-Cpm-Final.md",
            "ADR-005-Application-Interaction-Model-Final.md",
            "ADR-005-Enforcement-Levels.md"
        };

        foreach (var file in adrFiles)
        {
            var fileName = Path.GetFileName(file);

            // Skip historical exceptions
            if (historicalExceptions.Contains(fileName))
            {
                continue;
            }

            // Check format: ADR-XXXX-descriptive-title.md
            var isValid = System.Text.RegularExpressions.Regex.IsMatch(fileName, @"^ADR-\d+-[a-z0-9-]+\.md$");

            isValid.Should().BeTrue($"❌ ADR-006_1_2 违规: ADR 文件名格式不正确\n\n" +
                $"文件: {file}\n" +
                $"文件名: {fileName}\n\n" +
                $"正确格式: ADR-XXXX-descriptive-title.md\n" +
                $"要求:\n" +
                $"1. 使用小写字母和连字符\n" +
                $"2. 不要使用下划线或驼峰命名\n" +
                $"3. 标题应简洁但具描述性\n\n" +
                $"说明:\n" +
                $"历史文件（如 ADR-004-Cpm-Final.md）已被豁免，但新 ADR 必须遵循规范\n\n" +
                $"参考: docs/copilot/adr-006.prompts.md (场景 5)");
        }
    }

    [Fact(DisplayName = "ADR-006_1_3: 宪法层 ADR 应使用前导零")]
    public void Constitutional_ADRs_Should_Use_Leading_Zeros()
    {
        var constitutionalDir = Path.Combine(_adrRoot, "constitutional");
        var adrFiles = Directory.GetFiles(constitutionalDir, "ADR-*.md", SearchOption.TopDirectoryOnly);

        foreach (var file in adrFiles)
        {
            var fileName = Path.GetFileName(file);
            var match = System.Text.RegularExpressions.Regex.Match(fileName, @"^ADR-(\d+)");

            if (match.Success)
            {
                var numberStr = match.Groups[1].Value;
                var number = int.Parse(numberStr);

                if (number is >= 1 and <= 9)
                {
                    (numberStr.Length == 4 && numberStr.StartsWith("000")).Should().BeTrue($"❌ ADR-006_1_3 违规: 宪法层 ADR 必须使用前导零\n\n" +
                        $"文件: {file}\n" +
                        $"当前编号: ADR-{numberStr}\n" +
                        $"正确编号: ADR-{number:D4}\n\n" +
                        $"修复建议:\n" +
                        $"1. 宪法层（0001~0009）必须使用前导零\n" +
                        $"2. 文件名格式: ADR-001, ADR-002, ..., ADR-0009\n\n" +
                        $"参考: docs/copilot/adr-006.prompts.md (常见错误 2)");
                }
            }
        }
    }

    [Fact(DisplayName = "ADR-006_1_4: 非宪法层 ADR 不应使用多余前导零")]
    public void Non_Constitutional_ADRs_Should_Not_Use_Leading_Zeros()
    {
        var directories = new[] { "structure", "runtime", "technical", "governance" };

        foreach (var dir in directories)
        {
            var dirPath = Path.Combine(_adrRoot, dir);
            if (!Directory.Exists(dirPath)) continue;

            var adrFiles = Directory.GetFiles(dirPath, "ADR-*.md", SearchOption.TopDirectoryOnly);

            foreach (var file in adrFiles)
            {
                var fileName = Path.GetFileName(file);
                var match = System.Text.RegularExpressions.Regex.Match(fileName, @"^ADR-(\d+)");

                if (match.Success)
                {
                    var numberStr = match.Groups[1].Value;
                    var number = int.Parse(numberStr);

                    // Special cases: ADR-900 and ADR-0900 are allowed to have leading zeros
                    if (number == 0 || number == 900)
                    {
                        continue;
                    }

                    // For numbers >= 100, should not have leading zeros (except 0900)
                    if (number >= 100)
                    {
                        var hasLeadingZero = numberStr.StartsWith("0") && numberStr.Length > number.ToString().Length;

                        hasLeadingZero.Should().BeFalse($"⚠️ ADR-006.4 建议: 非宪法层 ADR 不应使用多余前导零\n\n" +
                            $"文件: {file}\n" +
                            $"当前编号: ADR-{numberStr}\n" +
                            $"建议编号: ADR-{number}\n\n" +
                            $"说明:\n" +
                            $"1. 只有宪法层（0001~0009）和特殊编号（0000, 0900）使用前导零\n" +
                            $"2. 其他层（100~999）不使用前导零\n" +
                            $"3. 历史原因（如 ADR-0900）可保留\n\n" +
                            $"参考: docs/copilot/adr-006.prompts.md (常见错误 2)");
                    }
                }
            }
        }
    }

    [Fact(DisplayName = "ADR-006_1_5: ADR 文件应位于正确的目录")]
    public void All_ADR_Files_Should_Be_In_Correct_Directory()
    {
        var mapping = new Dictionary<string, (int Min, int Max)>
        {
            ["constitutional"] = (1, 9),
            ["governance"] = (0, 0), // ADR-900 special case, and 900-999
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

                    isInCorrectRange.Should().BeTrue($"❌ ADR-006_1_5 违规: ADR 文件位于错误的目录\n\n" +
                        $"文件: {file}\n" +
                        $"编号: ADR-{number}\n" +
                        $"当前目录: {directory}\n\n" +
                        $"正确目录应为:\n" +
                        $"- 0001~0009: constitutional/\n" +
                        $"- 0000, 900~999: governance/\n" +
                        $"- 100~199: structure/\n" +
                        $"- 200~299: runtime/\n" +
                        $"- 300~399: technical/\n\n" +
                        $"参考: docs/copilot/adr-006.prompts.md (场景 3)");
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
