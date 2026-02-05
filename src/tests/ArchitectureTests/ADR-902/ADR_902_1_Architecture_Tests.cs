namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_902;

/// <summary>
/// ADR-902_1: ADR 结构完整性
/// 验证 ADR 文档符合结构完整性要求
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-902_1_1: 规则条目必须独立编号
/// - ADR-902_1_2: 统一模板结构
/// - ADR-902_1_3: 标准 Front Matter
/// - ADR-902_1_4: 包含完整章节集合
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-902-adr-template-structure-contract.md
/// - Prompts: docs/copilot/adr-902.prompts.md
/// </summary>
public sealed class ADR_902_1_Architecture_Tests
{
    private const int MaxAdrFilesToCheckL1 = 50;

    // ADR-902 要求的必需 Front Matter 字段
    private static readonly string[] RequiredFrontMatterFields = new[]
    {
        "adr",
        "title",
        "status",
        "level",
        "deciders",
        "date",
        "version",
        "maintainer",
        "reviewer",
        "supersedes",
        "superseded_by"
    };

    // ADR-902 要求的章节（Canonical Name）
    private static readonly string[] RequiredSections = new[]
    {
        "Focus",
        "Glossary",
        "Decision",
        "Enforcement",
        "Non-Goals",
        "Prohibited",
        "Relationships",
        "References",
        "History"
    };

    // Status 合法值
    private static readonly string[] ValidStatusValues = new[]
    {
        "Draft",
        "Accepted",
        "Final",
        "Superseded"
    };

    // Level 合法值
    private static readonly string[] ValidLevelValues = new[]
    {
        "Constitutional",
        "Governance",
        "Structure",
        "Runtime",
        "Technical"
    };

    /// <summary>
    /// ADR-902_1_1: 规则条目必须独立编号
    /// 验证 ADR 中的规则按照 Rule/Clause 格式独立编号（§1.1）
    /// </summary>
    [Fact(DisplayName = "ADR-902_1_1: 规则条目必须独立编号")]
    public void ADR_902_1_1_Rules_Must_Have_Independent_Numbering()
    {
        var violations = new List<string>();

        var adrDirectory = FileSystemTestHelper.GetAbsolutePath(TestConstants.AdrDocsPath);

        FileSystemTestHelper.AssertDirectoryExists(adrDirectory,
            AssertionMessageBuilder.BuildDirectoryNotFoundMessage(
                ruleId: "ADR-902_1_1",
                directoryPath: adrDirectory,
                directoryDescription: "ADR 文档目录",
                remediationSteps: new[] { "创建 ADR 文档目录" },
                adrReference: TestConstants.Adr902Path));

        // 扫描所有 ADR 文档
        var adrFiles = FileSystemTestHelper.GetAdrFiles()
            .Take(MaxAdrFilesToCheckL1);

        foreach (var file in adrFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(TestEnvironment.RepositoryRoot!, file);

            // 提取 ADR 编号（如 ADR-902）
            var adrNumber = ExtractAdrNumber(Path.GetFileNameWithoutExtension(file));
            if (string.IsNullOrEmpty(adrNumber))
                continue;

            // 查找 Decision 章节
            var decisionSection = ExtractDecisionSection(content);
            if (string.IsNullOrEmpty(decisionSection))
                continue;

            // 检查规则标题格式：### ADR-XXX_Y：<规则标题>（Rule）
            var rulePattern = $@"^###\s+{Regex.Escape(adrNumber)}_(\d+)：.+（Rule）";
            var ruleMatches = Regex.Matches(decisionSection, rulePattern, RegexOptions.Multiline);

            if (ruleMatches.Count > 0)
            {
                // 验证规则编号连续性
                var ruleNumbers = ruleMatches
                    .Select(m => int.Parse(m.Groups[1].Value))
                    .OrderBy(n => n)
                    .ToList();

                for (int i = 0; i < ruleNumbers.Count; i++)
                {
                    if (ruleNumbers[i] != i + 1)
                    {
                        violations.Add($"  • {relativePath} - 规则编号不连续，期望 {i + 1}，实际 {ruleNumbers[i]}");
                        break;
                    }
                }
            }
        }

        if (violations.Any())
        {
            var message = AssertionMessageBuilder.BuildWithViolations(
                ruleId: "ADR-902_1_1",
                summary: "以下 ADR 的规则编号不符合要求",
                failingTypes: violations,
                remediationSteps: new[]
                {
                    "每条规则必须是独立的三级标题",
                    "使用格式：### ADR-XXX_Y：<规则标题>（Rule）",
                    "规则编号必须连续：ADR-XXX_1, ADR-XXX_2, ..."
                },
                adrReference: TestConstants.Adr902Path);

            violations.Should().BeEmpty(message);
        }
    }

    /// <summary>
    /// ADR-902_1_2: 统一模板结构
    /// 验证所有 ADR 文档符合标准模板（§1.2）
    /// </summary>
    [Fact(DisplayName = "ADR-902_1_2: 统一模板结构")]
    public void ADR_902_1_2_ADR_Documents_Must_Follow_Template_Structure()
    {
        // 验证 ADR-902 文档存在
        var adrFile = FileSystemTestHelper.GetAbsolutePath(TestConstants.Adr902Path);

        var fileMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-902_1_2",
            filePath: adrFile,
            fileDescription: "ADR-902 文档",
            remediationSteps: new[] { "确保 ADR-902 文档存在以定义模板结构" },
            adrReference: TestConstants.Adr902Path);

        File.Exists(adrFile).Should().BeTrue(fileMessage);

        var content = File.ReadAllText(adrFile);

        // 验证核心原则：ADR 必须符合模板
        content.Should().Contain("可被机器和人同时理解的治理工件",
            $"❌ ADR-902_1_2 违规：ADR-902 必须定义核心治理原则");

        // 验证模板结构要求
        content.Should().Contain("Front Matter",
            $"❌ ADR-902_1_2 违规：ADR-902 必须定义 Front Matter 要求");
        content.Should().Contain("章节集合",
            $"❌ ADR-902_1_2 违规：ADR-902 必须定义章节集合要求");
    }

    /// <summary>
    /// ADR-902_1_3: 标准 Front Matter
    /// 验证 ADR 文档包含标准 Front Matter（§1.3）
    /// </summary>
    [Fact(DisplayName = "ADR-902_1_3: 标准 Front Matter")]
    public void ADR_902_1_3_ADR_Documents_Must_Have_Standard_FrontMatter()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var violations = new List<string>();

        var adrDirectory = FileSystemTestHelper.GetAbsolutePath(TestConstants.AdrDocsPath);

        Directory.Exists(adrDirectory).Should().BeTrue($"未找到 ADR 文档目录：{adrDirectory}");

        // 扫描治理类 ADR 文档（ADR-902 发布后的新标准）
        var governanceDir = Path.Combine(adrDirectory, "governance");
        var adrFiles = new List<string>();

        if (Directory.Exists(governanceDir))
        {
            adrFiles.AddRange(Directory
                .GetFiles(governanceDir, "ADR-*.md", SearchOption.AllDirectories)
                .Where(f => !f.Contains("README", StringComparison.OrdinalIgnoreCase))
                .Where(f => !f.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase))
                .Take(MaxAdrFilesToCheckL1));
        }

        foreach (var file in adrFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);

            // 提取 Front Matter
            var frontMatter = ExtractFrontMatter(content);

            if (string.IsNullOrEmpty(frontMatter))
            {
                violations.Add($"  • {relativePath} - 缺少 YAML Front Matter");
                continue;
            }

            // 检查必需字段
            var missingFields = new List<string>();
            foreach (var field in RequiredFrontMatterFields)
            {
                if (!Regex.IsMatch(frontMatter, $@"^{field}:\s*.+", RegexOptions.Multiline))
                {
                    missingFields.Add(field);
                }
            }

            if (missingFields.Any())
            {
                violations.Add($"  • {relativePath} - 缺少字段: {string.Join(", ", missingFields)}");
            }

            // 验证 status 字段的值
            var statusMatch = Regex.Match(frontMatter, @"^status:\s*(.+)$", RegexOptions.Multiline);
            if (statusMatch.Success)
            {
                var status = statusMatch.Groups[1].Value.Trim();
                if (!ValidStatusValues.Contains(status, StringComparer.Ordinal))
                {
                    violations.Add($"  • {relativePath} - status 值 '{status}' 不合法，必须是: {string.Join(", ", ValidStatusValues)}");
                }
            }

            // 验证 level 字段的值
            var levelMatch = Regex.Match(frontMatter, @"^level:\s*(.+)$", RegexOptions.Multiline);
            if (levelMatch.Success)
            {
                var level = levelMatch.Groups[1].Value.Trim();
                if (!ValidLevelValues.Contains(level, StringComparer.Ordinal))
                {
                    violations.Add($"  • {relativePath} - level 值 '{level}' 不合法，必须是: {string.Join(", ", ValidLevelValues)}");
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(string.Join("\n", new[]
            {
                "❌ ADR-902_1_3 违规：以下 ADR 文档的 Front Matter 不符合标准",
                "",
                "根据 ADR-902_1_3：所有 ADR 必须包含标准 Front Matter 并使用合法的枚举值。",
                ""
            }
            .Concat(violations.Take(20))
            .Concat(violations.Count > 20 ? new[] { $"  ... 还有 {violations.Count - 20} 个违规" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 确保 ADR 文档以 YAML Front Matter 开头（用 --- 包围）",
                "  2. 包含所有必需字段：adr, title, status, level, deciders, date, version, maintainer, reviewer, supersedes, superseded_by",
                "  3. status 必须是：Draft, Accepted, Final, Superseded 之一",
                "  4. level 必须是：Constitutional, Governance, Structure, Runtime, Technical 之一",
                "",
                "参考：docs/adr/governance/ADR-902-adr-template-structure-contract.md §1.3"
            })));
        }
    }

    /// <summary>
    /// ADR-902_1_4: 包含完整章节集合
    /// 验证 ADR 文档包含所有必需章节（§1.4）
    /// </summary>
    [Fact(DisplayName = "ADR-902_1_4: 包含完整章节集合")]
    public void ADR_902_1_4_ADR_Documents_Must_Have_Complete_Sections()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var violations = new List<string>();

        var adrDirectory = FileSystemTestHelper.GetAbsolutePath(TestConstants.AdrDocsPath);

        Directory.Exists(adrDirectory).Should().BeTrue($"未找到 ADR 文档目录：{adrDirectory}");

        // 扫描治理类 ADR 文档
        var governanceDir = Path.Combine(adrDirectory, "governance");
        var adrFiles = new List<string>();

        if (Directory.Exists(governanceDir))
        {
            adrFiles.AddRange(Directory
                .GetFiles(governanceDir, "ADR-*.md", SearchOption.AllDirectories)
                .Where(f => !f.Contains("README", StringComparison.OrdinalIgnoreCase))
                .Where(f => !f.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase))
                .Take(MaxAdrFilesToCheckL1));
        }

        foreach (var file in adrFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);

            // 检查必需章节
            var missingSections = new List<string>();
            foreach (var section in RequiredSections)
            {
                var sectionPattern = $@"^##\s+{Regex.Escape(section)}(\s*（.+?）)?(\s|$)";
                if (!Regex.IsMatch(content, sectionPattern, RegexOptions.Multiline))
                {
                    missingSections.Add(section);
                }
            }

            if (missingSections.Any())
            {
                violations.Add($"  • {relativePath} - 缺少章节: {string.Join(", ", missingSections)}");
            }

            // 验证章节顺序
            var sections = ExtractSections(content);
            var expectedOrder = RequiredSections.ToList();
            var actualOrder = sections.Where(s => expectedOrder.Contains(s, StringComparer.Ordinal)).ToList();

            if (actualOrder.Count >= 2)
            {
                var hasCorrectOrder = true;
                var lastExpectedIndex = -1;

                foreach (var section in actualOrder)
                {
                    var expectedIndex = expectedOrder.FindIndex(s => s.Equals(section, StringComparison.Ordinal));
                    if (expectedIndex < lastExpectedIndex)
                    {
                        hasCorrectOrder = false;
                        break;
                    }
                    lastExpectedIndex = expectedIndex;
                }

                if (!hasCorrectOrder)
                {
                    violations.Add($"  • {relativePath} - 章节顺序不正确，应按：{string.Join(" → ", RequiredSections)}");
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(string.Join("\n", new[]
            {
                "❌ ADR-902_1_4 违规：以下 ADR 文档缺少必需章节或章节顺序不正确",
                "",
                "根据 ADR-902_1_4：所有 ADR 必须包含完整章节集合且顺序固定。",
                ""
            }
            .Concat(violations.Take(20))
            .Concat(violations.Count > 20 ? new[] { $"  ... 还有 {violations.Count - 20} 个违规" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 确保包含所有必需章节（使用英文 Canonical Name）：",
                "     Focus, Glossary, Decision, Enforcement, Non-Goals, Prohibited, Relationships, References, History",
                "  2. 章节必须按上述顺序排列",
                "  3. 可以添加中文别名，但必须保留英文名称：## Focus（聚焦内容）",
                "",
                "参考：docs/adr/governance/ADR-902-adr-template-structure-contract.md §1.4"
            })));
        }
    }

    // ========== 辅助方法 ==========


    private static string ExtractFrontMatter(string content)
    {
        var match = Regex.Match(content, @"^---\s*\n(.*?)\n---", RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static List<string> ExtractSections(string content)
    {
        var sections = new List<string>();
        var matches = Regex.Matches(content, @"^##\s+(\w+)", RegexOptions.Multiline);

        foreach (Match match in matches)
        {
            sections.Add(match.Groups[1].Value);
        }

        return sections;
    }

    private static string ExtractDecisionSection(string content)
    {
        var match = Regex.Match(content, @"^##\s+Decision.*?\n(.*?)(?=^##\s+|\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string ExtractAdrNumber(string fileName)
    {
        var match = Regex.Match(fileName, @"(ADR-\d{3,4})");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
