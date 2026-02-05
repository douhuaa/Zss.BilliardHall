namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_006;

using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using static Zss.BilliardHall.Tests.ArchitectureTests.Shared.AssertionMessageBuilder;

/// <summary>
/// ADR-006_1: 编号分层规则
/// 验证 ADR-006_1_1：编号段层级映射
/// </summary>
public sealed class ADR_006_1_Architecture_Tests
{
    private readonly string _adrRoot;

    public ADR_006_1_Architecture_Tests()
    {
        _adrRoot = Path.Combine(TestEnvironment.RepositoryRoot, "docs", "adr");
    }

    [Fact(DisplayName = "ADR-006_1_1: 编号段层级映射检查")]
    public void ADR_006_1_1_Numbering_Segment_Must_Match_Layer()
    {
        var adrFiles = GetAllAdrFiles();

        foreach (var file in adrFiles)
        {
            var fileName = Path.GetFileName(file);
            var match = System.Text.RegularExpressions.Regex.Match(fileName, @"^ADR-(\d+)");

            match.Success.Should().BeTrue(Build(
                ruleId: "ADR-006_1_1",
                violation: "ADR 文件名必须以 'ADR-数字' 开头",
                evidence: new[] { $"文件: {file}", $"文件名: {fileName}" },
                remediation: new[]
                {
                    "1. 文件名必须以 ADR-XXX 格式开头（3位数字）",
                    "2. 所有编号统一使用3位格式（001-999）",
                    "3. 小于100的编号必须使用前导零（如 ADR-001, ADR-010）"
                },
                reference: "ADR-006_1_1 - 编号段层级映射"));

            if (match.Success)
            {
                var number = int.Parse(match.Groups[1].Value);
                var directory = Path.GetFileName(Path.GetDirectoryName(file)!);

                // Validate numbering ranges
                if (number is >= 1 and <= 9)
                {
                    directory.Should().Be("constitutional", BuildSimple(
                        ruleId: "ADR-006_1_1",
                        violation: $"ADR-{number:D3} 应位于 constitutional/ 目录",
                        detail: $"文件: {file}\n当前目录: {directory}",
                        remediation: "宪法层 ADR（001-009）必须位于 docs/adr/constitutional/ 目录",
                        reference: "ADR-006_1_1 - 编号段层级映射"));
                }
                else if (number == 0 || number is >= 900 and <= 999)
                {
                    directory.Should().Be("governance", BuildSimple(
                        ruleId: "ADR-006_1_1",
                        violation: $"ADR-{number:D3} 应位于 governance/ 目录",
                        detail: $"文件: {file}\n当前目录: {directory}",
                        remediation: "治理层 ADR（900-999）必须位于 docs/adr/governance/ 目录",
                        reference: "ADR-006_1_1 - 编号段层级映射"));
                }
                else if (number is >= 100 and <= 199)
                {
                    directory.Should().Be("structure", BuildSimple(
                        ruleId: "ADR-006_1_1",
                        violation: $"ADR-{number:D3} 应位于 structure/ 目录",
                        detail: $"文件: {file}\n当前目录: {directory}",
                        remediation: "结构层 ADR（100-199）必须位于 docs/adr/structure/ 目录",
                        reference: "ADR-006_1_1 - 编号段层级映射"));
                }
                else if (number is >= 200 and <= 299)
                {
                    directory.Should().Be("runtime", BuildSimple(
                        ruleId: "ADR-006_1_1",
                        violation: $"ADR-{number:D3} 应位于 runtime/ 目录",
                        detail: $"文件: {file}\n当前目录: {directory}",
                        remediation: "运行层 ADR（200-299）必须位于 docs/adr/runtime/ 目录",
                        reference: "ADR-006_1_1 - 编号段层级映射"));
                }
                else if (number is >= 300 and <= 399)
                {
                    directory.Should().Be("technical", BuildSimple(
                        ruleId: "ADR-006_1_1",
                        violation: $"ADR-{number:D3} 应位于 technical/ 目录",
                        detail: $"文件: {file}\n当前目录: {directory}",
                        remediation: "技术层 ADR（300-399）必须位于 docs/adr/technical/ 目录",
                        reference: "ADR-006_1_1 - 编号段层级映射"));
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
