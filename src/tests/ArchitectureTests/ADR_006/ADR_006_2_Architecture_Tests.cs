namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_006;

using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using static Zss.BilliardHall.Tests.ArchitectureTests.Shared.AssertionMessageBuilder;

/// <summary>
/// ADR-006_2: 编号格式规则
/// 验证 ADR-006_2_1：标准编号格式
/// 验证 ADR-006_2_2：文件命名规范
/// </summary>
public sealed class ADR_006_2_Architecture_Tests
{
    private readonly string _adrRoot;

    public ADR_006_2_Architecture_Tests()
    {
        _adrRoot = Path.Combine(TestEnvironment.RepositoryRoot, "docs", "adr");
    }

    [Fact(DisplayName = "ADR-006_2_1: 标准编号格式必须为3位数字")]
    public void ADR_006_2_1_Standard_Numbering_Format_Must_Be_Three_Digits()
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
                (numberStr.Length == 3).Should().BeTrue(Build(
                    ruleId: "ADR-006_2_1",
                    violation: "ADR 编号必须为3位格式",
                    evidence: new[]
                    {
                        $"文件: {file}",
                        $"当前编号: ADR-{numberStr}",
                        $"正确编号: ADR-{number:D3}"
                    },
                    remediation: new[]
                    {
                        "1. 所有 ADR 编号统一使用3位格式（000-999）",
                        "2. 小于100的编号必须使用前导零",
                        "   - 宪法层: ADR-001 到 ADR-009",
                        "   - 其他: ADR-010 到 ADR-099",
                        "3. 大于等于100的编号自然为3位",
                        "   - 结构层: ADR-100 到 ADR-199",
                        "   - 运行层: ADR-200 到 ADR-299",
                        "   - 技术层: ADR-300 到 ADR-399",
                        "   - 治理层: ADR-900 到 ADR-999"
                    },
                    reference: "ADR-006_2_1 - 标准编号格式"));
            }
        }
    }

    [Fact(DisplayName = "ADR-006_2_2: 文件命名规范检查")]
    public void ADR_006_2_2_Filename_Format_Should_Match_Standard()
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

            // Check format: ADR-XXX-descriptive-title.md
            var isValid = System.Text.RegularExpressions.Regex.IsMatch(fileName, @"^ADR-\d+-[a-z0-9-]+\.md$");

            isValid.Should().BeTrue(Build(
                ruleId: "ADR-006_2_2",
                violation: "ADR 文件名格式不正确",
                evidence: new[] { $"文件: {file}", $"文件名: {fileName}" },
                detail: "正确格式: ADR-XXX-descriptive-title.md",
                remediation: new[]
                {
                    "1. 使用小写字母和连字符",
                    "2. 不要使用下划线或驼峰命名",
                    "3. 标题应简洁但具描述性"
                },
                analysis: "历史文件（如 ADR-004-Cpm-Final.md）已被豁免，但新 ADR 必须遵循规范",
                reference: "ADR-006_2_2 - 文件命名规范"));
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
