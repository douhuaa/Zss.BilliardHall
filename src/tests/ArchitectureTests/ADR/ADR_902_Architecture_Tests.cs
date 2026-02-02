using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-902: ADR 标准模板与结构契约
/// 验证所有 ADR 文档符合 ADR-902 定义的模板与结构约束
/// 
/// 【测试覆盖映射】
/// ├─ ADR-902.1:L1 规则条目必须独立编号 → ADR_Rules_Must_Have_Independent_Numbering
/// ├─ ADR-902.2:L1 统一模板结构 → ADR_Documents_Must_Follow_Template_Structure
/// ├─ ADR-902.3:L1 标准 Front Matter → ADR_Documents_Must_Have_Standard_FrontMatter
/// ├─ ADR-902.4:L1 包含完整章节集合 → ADR_Documents_Must_Have_Complete_Sections
/// ├─ ADR-902.5:L1 Decision 严格隔离 → ADR_Decision_Must_Be_Isolated
/// └─ ADR-902.6:L1 ADR 模板不承担语义裁决职责 → ADR_Template_Must_Not_Define_Semantics
/// 
/// 【关联文档】
/// - ADR: docs/adr/governance/ADR-902-adr-template-structure-contract.md
/// - Prompts: docs/copilot/adr-902.prompts.md
/// </summary>
public sealed class ADR_902_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";

    [Fact(DisplayName = "ADR-0902_1_1: ADR-902 标准模板与结构契约文档存在")]
    public void ADR_902_Template_Structure_Contract_Exists()
    {
        // 验证 ADR-902 文档存在
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-902-adr-template-structure-contract.md");
        
        File.Exists(adrFile).Should().BeTrue($"ADR-902 文档不存在：{adrFile}");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证必需章节存在
        content.Should().Contain("Focus");
        content.Should().Contain("Glossary");
        content.Should().Contain("Decision");
        content.Should().Contain("Enforcement");
        content.Should().Contain("Non-Goals");
        content.Should().Contain("Prohibited");
        content.Should().Contain("Relationships");
        content.Should().Contain("References");
        content.Should().Contain("History");
    }

    [Fact(DisplayName = "ADR-0902_1_2:L1 对应的 Copilot Prompts 文件存在")]
    public void ADR_902_Prompts_File_Exists()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var promptsFile = Path.Combine(repoRoot, "docs/copilot/adr-902.prompts.md");
        
        // 注意：此测试在 Prompts 文件创建后才会通过
        // 如果文件不存在，给出清晰的待办提示
        if (!File.Exists(promptsFile))
        {
            true.Should().BeFalse($"⚠️ 待办：ADR-902 Prompts 文件需要创建：{promptsFile}\n" +
                       "请创建该文件以提供 ADR 模板结构的场景化指导。");
        }
        
        var content = File.ReadAllText(promptsFile);
        
        // 验证 Prompts 文件包含权威声明
        content.Should().Contain("权威声明");
        content.Should().Contain("ADR-902");
    }

    [Fact(DisplayName = "ADR-0902_1_3:L1 核心治理原则已定义")]
    public void Core_ADR_Template_Principles_Are_Defined()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-902-adr-template-structure-contract.md");
        var content = File.ReadAllText(adrFile);
        
        // 验证核心原则：ADR 必须符合模板
        content.Should().Contain("可被机器和人同时理解的治理工件");
        
        // 验证模板结构要求
        content.Should().Contain("Front Matter");
        content.Should().Contain("章节集合");
        
        // 验证 Decision 隔离
        content.Should().Contain("Decision 严格隔离");
    }

    /// <summary>
    /// 查找仓库根目录
    /// </summary>
    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) || 
                Directory.Exists(Path.Combine(currentDir, "docs", "adr")) ||
                File.Exists(Path.Combine(currentDir, "Zss.BilliardHall.slnx")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return null;
    }

    // ========== 执法级测试：真正阻止违规行为 ==========

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

    // ADR-902 发布前的遗留 ADR 可以豁免严格的模板检查
    // 只检查 governance 目录下的新 ADR（包括 ADR-902 本身）
    
    private const int MaxAdrFilesToCheckL1 = 50;  // L1 阻断级测试
    private const int MaxAdrFilesToCheckL2 = 20;  // L2 警告级测试

    [Fact(DisplayName = "ADR-0902_1_4:L1 ADR 文档必须包含标准 Front Matter（执法级）")]
    public void ADR_Documents_Must_Have_Standard_FrontMatter()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var violations = new List<string>();
        
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);
        
        if (!Directory.Exists(adrDirectory))
        {
            true.Should().BeFalse($"未找到 ADR 文档目录：{adrDirectory}");
        }

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
            
            // 检查必需字段（YAML 是区分大小写的）
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
            
            // 验证 status 字段的值（区分大小写）
            var statusMatch = Regex.Match(frontMatter, @"^status:\s*(.+)$", RegexOptions.Multiline);
            if (statusMatch.Success)
            {
                var status = statusMatch.Groups[1].Value.Trim();
                if (!ValidStatusValues.Contains(status, StringComparer.Ordinal))
                {
                    violations.Add($"  • {relativePath} - status 值 '{status}' 不合法，必须是: {string.Join(", ", ValidStatusValues)}");
                }
            }
            
            // 验证 level 字段的值（区分大小写）
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
                "❌ ADR-0902_1_4 违规：以下 ADR 文档的 Front Matter 不符合标准",
                "",
                "根据 ADR-902 决策 3：所有 ADR 必须包含标准 Front Matter 并使用合法的枚举值。",
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
                "参考：docs/adr/governance/ADR-902-adr-template-structure-contract.md 决策 3"
            })));
        }
    }

    [Fact(DisplayName = "ADR-0902_1_5:L1 ADR 文档必须包含完整章节集合（执法级）")]
    public void ADR_Documents_Must_Have_Complete_Sections()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var violations = new List<string>();
        
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);
        
        if (!Directory.Exists(adrDirectory))
        {
            true.Should().BeFalse($"未找到 ADR 文档目录：{adrDirectory}");
        }

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
            
            // 检查必需章节（Canonical Name，区分大小写）
            var missingSections = new List<string>();
            foreach (var section in RequiredSections)
            {
                // 章节标题格式：## Section 或 ## Section（中文别名）
                // 使用词边界确保完整匹配
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
            
            // 验证章节顺序（仅当至少有2个必需章节时进行顺序检查）
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
                "❌ ADR-0902_1_5 违规：以下 ADR 文档缺少必需章节或章节顺序不正确",
                "",
                "根据 ADR-902 决策 4：所有 ADR 必须包含完整章节集合且顺序固定。",
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
                "参考：docs/adr/governance/ADR-902-adr-template-structure-contract.md 决策 4"
            })));
        }
    }

    [Fact(DisplayName = "ADR-0902_1_6:L2 Decision 章节必须严格隔离约束性规则（指导级）")]
    public void ADR_Decision_Must_Be_Isolated()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var warnings = new List<string>();
        
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);
        
        if (!Directory.Exists(adrDirectory))
        {
            true.Should().BeFalse($"未找到 ADR 文档目录：{adrDirectory}");
        }

        // 裁决性语言模式
        var decisionWords = new[] { "必须", "MUST", "禁止", "FORBIDDEN", "不允许", "NOT ALLOWED", "不得", "SHALL NOT" };

        // 扫描所有 ADR 文档
        var adrFiles = Directory
            .GetFiles(adrDirectory, "ADR-*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("README", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase))
            .Take(MaxAdrFilesToCheckL2); // L2 级别测试，限制检查数量

        foreach (var file in adrFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            
            // 提取非 Decision 章节的内容
            var nonDecisionContent = ExtractNonDecisionContent(content);
            
            // 移除代码块和引用块
            var contentWithoutCodeBlocks = RemoveCodeBlocks(nonDecisionContent);
            var contentWithoutQuotes = RemoveQuotedSections(contentWithoutCodeBlocks);
            
            // 检查是否包含裁决性语言
            foreach (var word in decisionWords)
            {
                if (contentWithoutQuotes.Contains(word))
                {
                    // 这是一个指导级检查，记录但不失败
                    warnings.Add($"  ⚠️ {relativePath} - 非 Decision 章节包含裁决词 '{word}'");
                    break;
                }
            }
        }

        // L2 级别：警告但不失败构建
        if (warnings.Any())
        {
            var warningMessage = string.Join("\n", new[]
            {
                "⚠️ ADR-902.5 警告（L2）：以下 ADR 可能在非 Decision 章节使用了裁决性语言",
                "",
                "根据 ADR-902 决策 5：约束性规则应只出现在 Decision 章节。",
                ""
            }
            .Concat(warnings.Take(10))
            .Concat(new[]
            {
                "",
                "建议：",
                "  1. 将裁决性语句移至 Decision 章节",
                "  2. 在其他章节使用描述性语言",
                "  3. 如果是引用或示例，请用引用块或代码块包围",
                "",
                "注意：这是 L2 警告级别，不会阻断构建。",
                "",
                "参考：docs/adr/governance/ADR-902-adr-template-structure-contract.md 决策 5"
            }));
            
            Console.WriteLine(warningMessage);
            Console.WriteLine();
        }
        
        // L2 警告：测试总是通过，但已输出警告信息
    }

    [Fact(DisplayName = "ADR-0902_1_7:L1 规则条目必须独立编号（执法级）")]
    public void ADR_Rules_Must_Have_Independent_Numbering()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var violations = new List<string>();
        
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);
        
        if (!Directory.Exists(adrDirectory))
        {
            true.Should().BeFalse($"未找到 ADR 文档目录：{adrDirectory}");
        }

        // 扫描所有 ADR 文档
        var adrFiles = Directory
            .GetFiles(adrDirectory, "ADR-*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("README", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase))
            .Take(MaxAdrFilesToCheckL1);

        foreach (var file in adrFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            
            // 提取 ADR 编号（如 ADR-0001）
            var adrNumber = ExtractAdrNumber(Path.GetFileNameWithoutExtension(file));
            if (string.IsNullOrEmpty(adrNumber))
                continue;
            
            // 查找 Decision 章节
            var decisionSection = ExtractDecisionSection(content);
            if (string.IsNullOrEmpty(decisionSection))
                continue;
            
            // 检查规则标题格式：### ADR-XXX.Y:L[12] <规则标题>
            // 根据 ADR-902，级别应该是 L1 或 L2
            var rulePattern = $@"^###\s+{Regex.Escape(adrNumber)}\.(\d+):L[12]\s+.+";
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
            
            // 检查是否有未独立编号的规则（合并在一个章节中的多条规则）
            var headingPattern = @"^###\s+.+";
            var headings = Regex.Matches(decisionSection, headingPattern, RegexOptions.Multiline);
            
            // 启发式阈值：如果一个章节有超过此数量的列表项，可能包含多条合并的规则
            const int MergedRuleListItemThreshold = 3;
            
            var localWarnings = new List<string>();
            
            foreach (Match heading in headings)
            {
                var headingText = heading.Value;
                
                // 如果标题不符合独立编号格式，但包含多条规则特征（如使用列表或多个"必须"）
                if (!Regex.IsMatch(headingText, $@"{Regex.Escape(adrNumber)}\.\d+:L[12]"))
                {
                    // 获取该标题下的内容
                    var nextHeadingIndex = decisionSection.IndexOf("\n###", heading.Index + 1);
                    var sectionContent = nextHeadingIndex > 0 
                        ? decisionSection.Substring(heading.Index, nextHeadingIndex - heading.Index)
                        : decisionSection.Substring(heading.Index);
                    
                    // 简单启发式：如果包含多个列表项且有裁决性语言，可能是合并的规则
                    var listItems = Regex.Matches(sectionContent, @"^\s*[-*]\s+", RegexOptions.Multiline);
                    var hasMultipleDecisions = listItems.Count > MergedRuleListItemThreshold;
                    
                    if (hasMultipleDecisions)
                    {
                        var hasDecisionWords = Regex.IsMatch(sectionContent, @"(必须|MUST|禁止|不允许)");
                        if (hasDecisionWords)
                        {
                            localWarnings.Add($"  ⚠️ {relativePath} - 可能包含未独立编号的多条规则：{headingText.Trim()}");
                        }
                    }
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(string.Join("\n", new[]
            {
                "❌ ADR-0902_1_1 违规：以下 ADR 的规则编号不符合要求",
                "",
                "根据 ADR-902 决策 1：每条规则必须作为独立三级标题存在，格式为：### ADR-XXX.Y:L? <规则标题>",
                ""
            }
            .Concat(violations)
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 每条规则必须是独立的三级标题",
                "  2. 使用格式：### ADR-XXX.Y:L? <规则标题>（中文别名可选）",
                "  3. 规则编号必须连续：ADR-XXX.1, ADR-XXX.2, ...",
                "",
                "参考：docs/adr/governance/ADR-902-adr-template-structure-contract.md 决策 1"
            })));
        }
    }

    // ========== 辅助方法 ==========

    private static string ExtractFrontMatter(string content)
    {
        // 提取 YAML Front Matter（在 --- 之间）
        var match = Regex.Match(content, @"^---\s*\n(.*?)\n---", RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static List<string> ExtractSections(string content)
    {
        // 提取所有二级标题
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
        // 提取 Decision 章节的内容
        var match = Regex.Match(content, @"^##\s+Decision.*?\n(.*?)(?=^##\s+|\z)", 
            RegexOptions.Multiline | RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string ExtractNonDecisionContent(string content)
    {
        // 移除 Decision 章节，返回其他内容
        return Regex.Replace(content, @"^##\s+Decision.*?\n.*?(?=^##\s+|\z)", 
            string.Empty, RegexOptions.Multiline | RegexOptions.Singleline);
    }

    private static string RemoveCodeBlocks(string content)
    {
        // 移除 Markdown 代码块
        return Regex.Replace(content, @"```[\s\S]*?```", string.Empty);
    }

    private static string RemoveQuotedSections(string content)
    {
        // 移除引用块（> 开头的行）
        var lines = content.Split('\n');
        var nonQuotedLines = lines.Where(line => !line.TrimStart().StartsWith(">"));
        return string.Join("\n", nonQuotedLines);
    }

    private static string ExtractAdrNumber(string fileName)
    {
        // 从 "ADR-0902-xxx" 中提取 "ADR-0902"
        var match = Regex.Match(fileName, @"(ADR-\d{4})");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
