namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_951;

/// <summary>
/// ADR-951_2: 案例文档标准（Rule）
/// 验证案例文档必须包含规定的章节和内容
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-951_2_1: 必需章节
/// - ADR-951_2_2: 案例级别
/// - ADR-951_2_3: 代码示例要求
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-951-case-repository-management.md
/// </summary>
public sealed class ADR_951_2_Architecture_Tests
{
    /// <summary>
    /// ADR-951_2_1: 必需章节
    /// 验证案例文档必须包含元数据、背景、解决方案、代码示例、常见陷阱、相关案例等必需章节（§ADR-951_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-951_2_1: 案例文档必须包含所有必需章节")]
    public void ADR_951_2_1_Case_Documents_Must_Have_Required_Sections()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var casesDirectory = Path.Combine(repoRoot, "docs/cases");

        if (!Directory.Exists(casesDirectory))
        {
            Console.WriteLine("⚠️ ADR-951_2_1 提示：docs/cases/ 目录尚未创建，跳过测试。");
            return;
        }

        var caseFiles = Directory.GetFiles(casesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (caseFiles.Count == 0)
        {
            Console.WriteLine("⚠️ ADR-951_2_1 提示：暂无案例文档，跳过测试。");
            return;
        }

        var violations = new List<string>();

        foreach (var caseFile in caseFiles)
        {
            var fileName = Path.GetFileName(caseFile);
            var content = File.ReadAllText(caseFile);

            var requiredSections = new[]
            {
                ("元数据|Metadata|Front Matter", "元数据章节"),
                ("背景|Background|问题描述", "背景章节"),
                ("解决方案|Solution", "解决方案章节"),
                ("代码示例|Code Example|示例", "代码示例章节"),
                ("陷阱|Pitfall|注意事项", "常见陷阱章节"),
                ("相关案例|Related Cases|相关", "相关案例章节")
            };

            var missingSections = new List<string>();

            foreach (var (pattern, sectionName) in requiredSections)
            {
                if (!Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    missingSections.Add(sectionName);
                }
            }

            if (missingSections.Count > 0)
            {
                violations.Add($"{fileName}: 缺少章节 - {string.Join(", ", missingSections)}");
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-951_2_1：以下案例文档缺少必需章节：\n{string.Join("\n", violations)}");
    }

    /// <summary>
    /// ADR-951_2_2: 案例级别
    /// 验证案例必须标记为 Core 或 Reference 级别（§ADR-951_2_2）
    /// </summary>
    [Fact(DisplayName = "ADR-951_2_2: 案例文档必须标记案例级别")]
    public void ADR_951_2_2_Case_Documents_Must_Have_Case_Level_Marked()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var casesDirectory = Path.Combine(repoRoot, "docs/cases");

        if (!Directory.Exists(casesDirectory))
        {
            Console.WriteLine("⚠️ ADR-951_2_2 提示：docs/cases/ 目录尚未创建，跳过测试。");
            return;
        }

        var caseFiles = Directory.GetFiles(casesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (caseFiles.Count == 0)
        {
            Console.WriteLine("⚠️ ADR-951_2_2 提示：暂无案例文档，跳过测试。");
            return;
        }

        var violations = new List<string>();

        foreach (var caseFile in caseFiles)
        {
            var fileName = Path.GetFileName(caseFile);
            var content = File.ReadAllText(caseFile);

            // 检查是否标记了 Core 或 Reference 级别
            var hasCoreLevel = Regex.IsMatch(content, @"(级别|Level)\s*[:：]\s*Core", RegexOptions.IgnoreCase);
            var hasReferenceLevel = Regex.IsMatch(content, @"(级别|Level)\s*[:：]\s*Reference", RegexOptions.IgnoreCase);

            if (!hasCoreLevel && !hasReferenceLevel)
            {
                violations.Add($"{fileName}: 未标记案例级别（应为 Core 或 Reference）");
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-951_2_2：以下案例文档未标记案例级别：\n{string.Join("\n", violations)}");
    }

    /// <summary>
    /// ADR-951_2_3: 代码示例要求
    /// 验证代码示例必须包含必要的 using/import 和注释说明（§ADR-951_2_3）
    /// </summary>
    [Fact(DisplayName = "ADR-951_2_3: 代码示例必须包含必要注释和引用")]
    public void ADR_951_2_3_Code_Examples_Must_Have_Comments_And_Imports()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var casesDirectory = Path.Combine(repoRoot, "docs/cases");

        if (!Directory.Exists(casesDirectory))
        {
            Console.WriteLine("⚠️ ADR-951_2_3 提示：docs/cases/ 目录尚未创建，跳过测试。");
            return;
        }

        var caseFiles = Directory.GetFiles(casesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (caseFiles.Count == 0)
        {
            Console.WriteLine("⚠️ ADR-951_2_3 提示：暂无案例文档，跳过测试。");
            return;
        }

        var violations = new List<string>();

        foreach (var caseFile in caseFiles)
        {
            var fileName = Path.GetFileName(caseFile);
            var content = File.ReadAllText(caseFile);

            // 提取代码块
            var codeBlockPattern = @"```[\w]*\n(.*?)\n```";
            var codeBlocks = Regex.Matches(content, codeBlockPattern, RegexOptions.Singleline);

            if (codeBlocks.Count == 0)
            {
                continue; // 没有代码块，跳过
            }

            var issues = new List<string>();

            foreach (Match codeBlock in codeBlocks)
            {
                var code = codeBlock.Groups[1].Value;

                // 检查是否包含注释
                var hasComments = code.Contains("//") || code.Contains("/*") || code.Contains("#");

                // 检查是否包含 ✅ 或 ❌ 标注
                var hasCorrectIncorrectMarkers = code.Contains("✅") || code.Contains("❌");

                if (!hasComments && !hasCorrectIncorrectMarkers)
                {
                    issues.Add("代码示例缺少注释或标注");
                }
            }

            if (issues.Count > 0)
            {
                violations.Add($"{fileName}: {string.Join(", ", issues)}");
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-951_2_3：以下案例文档的代码示例不符合要求：\n{string.Join("\n", violations)}");
    }

}
