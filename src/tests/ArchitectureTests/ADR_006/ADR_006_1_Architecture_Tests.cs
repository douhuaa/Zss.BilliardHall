namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_006;


/// <summary>
/// ADR-006_1: 编号分层规则
/// 验证 ADR-006_1_1：编号段层级映射
/// </summary>
public sealed class ADR_006_1_Architecture_Tests
{
    private readonly string _adrRoot;

    public ADR_006_1_Architecture_Tests()
    {
        _adrRoot = FileSystemTestHelper.GetAbsolutePath("docs/adr");
    }

    [Fact(DisplayName = "ADR-006_1_1: 编号段层级映射检查")]
    public void ADR_006_1_1_Numbering_Segment_Must_Match_Layer()
    {
        var adrFiles = GetAllAdrFiles();

        foreach (var file in adrFiles)
        {
            var fileName = Path.GetFileName(file);
            var match = System.Text.RegularExpressions.Regex.Match(fileName, @"^ADR-(\d+)");

            var fileNameMessage = AssertionMessageBuilder.Build(
                ruleId: "ADR-006_1_1",
                summary: "ADR 文件名必须以 'ADR-数字' 开头",
                currentState: $"文件: {file}\n文件名: {fileName}",
                remediationSteps: new[]
                {
                    "文件名必须以 ADR-XXX 格式开头（3位数字）",
                    "所有编号统一使用3位格式（001-999）",
                    "小于100的编号必须使用前导零（如 ADR-001, ADR-010）"
                },
                adrReference: "docs/adr/governance/ADR-006-adr-numbering-hierarchy.md");
            match.Success.Should().BeTrue(fileNameMessage);

            if (match.Success)
            {
                var number = int.Parse(match.Groups[1].Value);
                var directory = Path.GetFileName(Path.GetDirectoryName(file)!);

                // Validate numbering ranges
                if (number is >= 1 and <= 9)
                {
                    var constitutionalMessage = AssertionMessageBuilder.Build(
                        ruleId: "ADR-006_1_1",
                        summary: $"ADR-{number:D3} 应位于 constitutional/ 目录",
                        currentState: $"文件: {file}\n当前目录: {directory}",
                        remediationSteps: new[]
                        {
                            "宪法层 ADR（001-009）必须位于 docs/adr/constitutional/ 目录"
                        },
                        adrReference: "docs/adr/governance/ADR-006-adr-numbering-hierarchy.md");
                    directory.Should().Be("constitutional", constitutionalMessage);
                }
                else if (number == 0 || number is >= 900 and <= 999)
                {
                    var governanceMessage = AssertionMessageBuilder.Build(
                        ruleId: "ADR-006_1_1",
                        summary: $"ADR-{number:D3} 应位于 governance/ 目录",
                        currentState: $"文件: {file}\n当前目录: {directory}",
                        remediationSteps: new[]
                        {
                            "治理层 ADR（900-999）必须位于 docs/adr/governance/ 目录"
                        },
                        adrReference: "docs/adr/governance/ADR-006-adr-numbering-hierarchy.md");
                    directory.Should().Be("governance", governanceMessage);
                }
                else if (number is >= 100 and <= 199)
                {
                    var structureMessage = AssertionMessageBuilder.Build(
                        ruleId: "ADR-006_1_1",
                        summary: $"ADR-{number:D3} 应位于 structure/ 目录",
                        currentState: $"文件: {file}\n当前目录: {directory}",
                        remediationSteps: new[]
                        {
                            "结构层 ADR（100-199）必须位于 docs/adr/structure/ 目录"
                        },
                        adrReference: "docs/adr/governance/ADR-006-adr-numbering-hierarchy.md");
                    directory.Should().Be("structure", structureMessage);
                }
                else if (number is >= 200 and <= 299)
                {
                    var runtimeMessage = AssertionMessageBuilder.Build(
                        ruleId: "ADR-006_1_1",
                        summary: $"ADR-{number:D3} 应位于 runtime/ 目录",
                        currentState: $"文件: {file}\n当前目录: {directory}",
                        remediationSteps: new[]
                        {
                            "运行层 ADR（200-299）必须位于 docs/adr/runtime/ 目录"
                        },
                        adrReference: "docs/adr/governance/ADR-006-adr-numbering-hierarchy.md");
                    directory.Should().Be("runtime", runtimeMessage);
                }
                else if (number is >= 300 and <= 399)
                {
                    var technicalMessage = AssertionMessageBuilder.Build(
                        ruleId: "ADR-006_1_1",
                        summary: $"ADR-{number:D3} 应位于 technical/ 目录",
                        currentState: $"文件: {file}\n当前目录: {directory}",
                        remediationSteps: new[]
                        {
                            "技术层 ADR（300-399）必须位于 docs/adr/technical/ 目录"
                        },
                        adrReference: "docs/adr/governance/ADR-006-adr-numbering-hierarchy.md");
                    directory.Should().Be("technical", technicalMessage);
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
