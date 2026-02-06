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
        _adrRoot = FileSystemTestHelper.GetAbsolutePath("docs/adr");
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

                var message = AssertionMessageBuilder.BuildWithAnalysis(
                    ruleId: "ADR-006_3_1",
                    summary: "ADR 编号必须使用前导零",
                    currentState: $"文件: {file}\n当前编号: ADR-{numberStr}\n正确编号: ADR-{number:D3}",
                    problemAnalysis: "编号小于100时必须使用前导零，确保统一的3位格式",
                    remediationSteps: new[]
                    {
                        $"重命名文件为 ADR-{number:D3}-xxx.md",
                        "更新文件内容中的 ADR 编号引用",
                        "更新所有交叉引用该 ADR 的文档",
                        "编号规范：宪法层 ADR-001到009，结构层100-199，运行层200-299，技术层300-399，治理层900-999"
                    },
                    adrReference: "docs/adr/governance/ADR-006-adr-numbering-hierarchy.md");
                // All ADR numbers must be exactly 3 digits (000-999)
                (numberStr.Length == 3).Should().BeTrue(message);

                // Specifically check for numbers < 100
                if (number < 100)
                {
                    var startMessage = AssertionMessageBuilder.Build(
                        ruleId: "ADR-006_3_1",
                        summary: "小于100的 ADR 编号必须使用前导零",
                        currentState: $"文件: {file}\n当前编号: ADR-{numberStr}\n正确编号: ADR-{number:D3}",
                        remediationSteps: new[]
                        {
                            $"重命名为 ADR-{number:D3}-xxx.md",
                            "确保编号使用前导零"
                        },
                        adrReference: "docs/adr/governance/ADR-006-adr-numbering-hierarchy.md");
                    numberStr.Should().StartWith("0", startMessage);
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
