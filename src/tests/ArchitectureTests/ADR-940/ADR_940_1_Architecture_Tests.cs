using System.Text.RegularExpressions;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-940_1: ADR 文档一致性规范（Rule）
/// 验证 ADR 文档的编号、文件名、目录、元数据一致性
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-940_1_1: 文件名必须使用 3 位编号格式
/// - ADR-940_1_2: 编号必须与所在目录范围匹配
/// - ADR-940_1_3: 文档必须包含有效的 Front Matter
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-940-adr-relationship-and-traceability.md
/// </summary>
public sealed class ADR_940_1_Architecture_Tests
{
    private readonly string _adrRoot;
    private readonly IReadOnlyList<string> _adrFiles;

    // 特殊文件白名单
    private static readonly HashSet<string> SpecialFiles = new()
    {
        "ADR-RELATIONSHIP-MAP.md",  // 关系图文档
        "README.md"                  // 目录说明
    };

    public ADR_940_1_Architecture_Tests()
    {
        var repoRoot = TestEnvironment.RepositoryRoot
            ?? throw new InvalidOperationException("未找到仓库根目录");

        _adrRoot = Path.Combine(repoRoot, "docs", "adr");
        _adrFiles = Directory.GetFiles(_adrRoot, "ADR-*.md", SearchOption.AllDirectories)
            .Where(f =>
            {
                var fileName = Path.GetFileName(f);
                return !SpecialFiles.Contains(fileName);
            })
            .Where(f => !f.Contains("/proposals/", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("\\proposals\\", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// ADR-940_1_1: 文件名必须使用 3 位编号格式
    /// 验证所有 ADR 文件名使用 ADR-XXX 格式（§ADR-940_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-940_1_1: 文件名必须使用 3 位编号格式")]
    public void ADR_940_1_1_Files_Must_Use_Three_Digit_Numbering()
    {
        var violations = new List<string>();
        var validPattern = new Regex(@"^ADR-\d{3}[^/\\]*\.md$", RegexOptions.Compiled);

        foreach (var file in _adrFiles)
        {
            var fileName = Path.GetFileName(file);

            if (!validPattern.IsMatch(fileName))
            {
                violations.Add(
                    $"❌ {fileName} 不符合 3 位编号格式\n" +
                    $"   文件：{file}\n" +
                    $"   期望格式：ADR-XXX-title.md（如 ADR-001-xxx.md）"
                );
            }
        }

        violations.Should().BeEmpty();
    }

    /// <summary>
    /// ADR-940_1_2: 编号必须与所在目录范围匹配
    /// 验证 ADR 文件放置在正确的分类目录中（§ADR-940_1_2）
    /// constitutional: 0000-0999
    /// structure: 0100-0299
    /// runtime: 0200-0299
    /// technical: 0300-0399
    /// governance: 0000, 0400-0999, 0900+
    /// </summary>
    [Fact(DisplayName = "ADR-940_1_2: 编号必须与所在目录范围匹配")]
    public void ADR_940_1_2_Number_Must_Match_Directory_Range()
    {
        var violations = new List<string>();
        var numberPattern = new Regex(@"ADR-(\d{4})", RegexOptions.Compiled);

        foreach (var file in _adrFiles)
        {
            var fileName = Path.GetFileName(file);
            var match = numberPattern.Match(fileName);

            if (!match.Success)
                continue;

            var number = int.Parse(match.Groups[1].Value);
            var directory = Path.GetFileName(Path.GetDirectoryName(file)) ?? "";

            var expectedDirectory = DetermineExpectedDirectory(number);

            if (!directory.Equals(expectedDirectory, StringComparison.OrdinalIgnoreCase))
            {
                violations.Add(
                    $"❌ ADR-{number:D4} 位于错误的目录\n" +
                    $"   当前目录：{directory}\n" +
                    $"   期望目录：{expectedDirectory}\n" +
                    $"   文件：{file}"
                );
            }
        }

        violations.Should().BeEmpty();
    }

    /// <summary>
    /// ADR-940_1_3: 文档必须包含有效的 Front Matter
    /// 验证 ADR 文档包含必需的 YAML Front Matter 元数据（§ADR-940_1_3）
    /// </summary>
    [Fact(DisplayName = "ADR-940_1_3: 文档必须包含有效的 Front Matter")]
    public void ADR_940_1_3_Documents_Must_Have_Valid_FrontMatter()
    {
        var violations = new List<string>();

        foreach (var file in _adrFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);

            // 检查是否以 YAML Front Matter 开头
            if (!content.TrimStart().StartsWith("---"))
            {
                violations.Add(
                    $"❌ {fileName} 缺少 Front Matter\n" +
                    $"   文件：{file}\n" +
                    $"   Front Matter 必须以 --- 开头"
                );
                continue;
            }

            // 提取 Front Matter
            var frontMatterMatch = Regex.Match(content, @"^---\s*\n(.*?)\n---", RegexOptions.Singleline);

            if (!frontMatterMatch.Success)
            {
                violations.Add(
                    $"❌ {fileName} Front Matter 格式错误\n" +
                    $"   文件：{file}"
                );
                continue;
            }

            var frontMatter = frontMatterMatch.Groups[1].Value;

            // 检查必需字段
            var requiredFields = new[] { "adr:", "title:", "status:", "level:" };

            foreach (var field in requiredFields)
            {
                if (!frontMatter.Contains(field, StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add(
                        $"❌ {fileName} Front Matter 缺少必需字段：{field.TrimEnd(':')}\n" +
                        $"   文件：{file}"
                    );
                }
            }
        }

        violations.Should().BeEmpty();
    }

    /// <summary>
    /// 根据编号确定 ADR 应该在哪个目录
    /// </summary>
    private static string DetermineExpectedDirectory(int number)
    {
        return number switch
        {
            0 => "governance",                       // ADR-900: 特殊，在 governance
            >= 1 and <= 9 => "constitutional",      // 0001-0009: 宪法级
            >= 10 and <= 99 => "constitutional",    // 0010-0099: 宪法级扩展
            >= 100 and <= 199 => "structure",       // 0100-0199: 结构级
            >= 200 and <= 299 => "runtime",         // 0200-0299: 运行时级
            >= 300 and <= 399 => "technical",       // 0300-0399: 技术级
            >= 400 and <= 899 => "governance",      // 0400-0899: 治理级
            >= 900 => "governance",                 // 0900+: 治理级（9xx 系列）
            _ => "unknown"
        };
    }
}
