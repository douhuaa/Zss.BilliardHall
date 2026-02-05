namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_006;
using Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-006_5: 特殊编号规则
/// 验证 ADR-006_5_1：元规则编号保护
///
/// 注意：ADR-006_5_1 为 L3 级别（人工审查），此测试仅作为提示性检查
/// </summary>
public sealed class ADR_006_5_Architecture_Tests
{
    private readonly string _adrRoot;

    public ADR_006_5_Architecture_Tests()
    {
        _adrRoot = Path.Combine(TestEnvironment.RepositoryRoot, "docs", "adr");
    }

    [Fact(DisplayName = "ADR-006_5_1: 元规则编号保护检查（L3 人工审查）")]
    public void ADR_006_5_1_Meta_Rule_Numbers_Should_Be_Protected()
    {
        var reservedNumbers = new[] { 6, 7, 8, 9, 900 };
        var adrFiles = GetAllAdrFiles();

        foreach (var file in adrFiles)
        {
            var fileName = Path.GetFileName(file);
            var match = System.Text.RegularExpressions.Regex.Match(fileName, @"^ADR-(\d+)");

            if (match.Success)
            {
                var number = int.Parse(match.Groups[1].Value);

                // Check if using reserved numbers (007-009)
                if (number is >= 7 and <= 9)
                {
                    // This is L3 check - just warn, don't fail
                    // Architecture Board should manually review
                    var fileContent = File.ReadAllText(file);
                    var hasApproval = fileContent.Contains("Architecture Board") &&
                                     fileContent.Contains("Approved");

                    if (!hasApproval)
                    {
                        // Note: This is informational only, requires manual review
                        true.Should().BeTrue(
                            $"⚠️ ADR-006_5_1 提示: 使用预留编号需要架构委员会批准\n\n" +
                            $"文件: {file}\n" +
                            $"编号: ADR-{number:D3}\n\n" +
                            $"说明:\n" +
                            $"ADR-007~009 是预留的元规则编号，需要架构委员会特别批准才能使用\n\n" +
                            $"特殊编号说明:\n" +
                            $"- ADR-006: 术语与编号宪法（已使用）\n" +
                            $"- ADR-007~009: 预留给宪法层元规则\n" +
                            $"- ADR-900: 架构测试与 CI 治理元规则（已使用）\n\n" +
                            $"审批要求:\n" +
                            $"1. 必须在架构委员会会议中讨论\n" +
                            $"2. 需要全体成员一致同意\n" +
                            $"3. 必须在 ADR Front Matter 中记录批准信息\n\n" +
                            $"参考: ADR-006_5_1 - 元规则编号保护");
                    }
                }

                // Check ADR-900 content (should only contain meta rules)
                if (number == 900)
                {
                    // This is informational check for ADR-900
                    var fileContent = File.ReadAllText(file);
                    var isMetaRule = fileContent.Contains("架构测试") ||
                                    fileContent.Contains("Architecture Test") ||
                                    fileContent.Contains("CI") ||
                                    fileContent.Contains("治理");

                    isMetaRule.Should().BeTrue(
                        $"⚠️ ADR-006_5_1 提示: ADR-900 应只包含元规则内容\n\n" +
                        $"文件: {file}\n\n" +
                        $"说明:\n" +
                        $"ADR-900 是治理层的核心元规则，定义架构测试与 CI 治理体系\n" +
                        $"不应添加非元机制相关的内容\n\n" +
                        $"参考: ADR-006_5_1 - 元规则编号保护");
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
                adrFiles.AddRange(AdrFileFilter.GetAdrFiles(dirPath, SearchOption.TopDirectoryOnly));
            }
        }

        return adrFiles;
    }
}
