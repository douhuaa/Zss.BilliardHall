using System.Text.RegularExpressions;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_910;

/// <summary>
/// ADR-910_1: README 的定位与权限边界（Rule）
/// 验证所有 README 文档遵守边界约束，不得包含裁决性语言
/// 
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-910_1_1: README 是使用说明不是架构裁决书
/// - ADR-910_1_2: 禁用裁决性语言规则
/// - ADR-910_1_3: 必须包含无裁决力声明
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-910-readme-governance-constitution.md
/// </summary>
public sealed class ADR_910_1_Architecture_Tests
{
    private const string DocsPath = "docs";
    private const int MaxReadmeFilesToCheck = 50;

    // 裁决性词汇列表（根据 ADR-910_1_2）
    private static readonly string[] DecisionLanguageWords = new[] { "必须", "禁止", "不允许", "应当", "违规" };
    
    // 无裁决力声明的标准格式
    private const string NoAuthorityDeclaration = "本文档不具备裁决力";

    /// <summary>
    /// ADR-910_1_1: README 是使用说明不是架构裁决书
    /// 验证 README 文档不得定义架构规则或做出架构判断（§1.1）
    /// </summary>
    [Fact(DisplayName = "ADR-910_1_1: README 是使用说明不是架构裁决书")]
    public void ADR_910_1_1_README_Must_Be_Usage_Guide_Not_Decision_Document()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var violations = new List<string>();
        
        var docsDirectory = Path.Combine(repoRoot, DocsPath);
        Directory.Exists(docsDirectory).Should().BeTrue($"未找到文档目录：{docsDirectory}");

        // 扫描所有 README 文件
        var readmeFiles = Directory
            .GetFiles(docsDirectory, "README.md", SearchOption.AllDirectories)
            .Take(MaxReadmeFilesToCheck)
            .ToList();
        
        // 也检查根目录的 README
        var rootReadme = Path.Combine(repoRoot, "README.md");
        if (File.Exists(rootReadme))
        {
            readmeFiles.Add(rootReadme);
        }

        foreach (var file in readmeFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            
            // 检查是否包含架构规则定义的关键词（未引用 ADR）
            // 如果出现裁决性语言但没有 ADR 引用，视为违规
            var hasDecisionLanguage = DecisionLanguageWords.Any(word => 
                content.Contains(word, StringComparison.Ordinal));
            
            if (hasDecisionLanguage)
            {
                // 检查是否有适当的 ADR 引用
                var hasAdrReference = Regex.IsMatch(content, @"ADR-\d{3,4}");
                
                if (!hasAdrReference)
                {
                    // 检查是否在代码示例或表格中（这些是允许的上下文）
                    var decisionWordsInContext = FindDecisionWordsWithoutAdrContext(content);
                    
                    if (decisionWordsInContext.Any())
                    {
                        violations.Add($"  ❌ {relativePath}");
                        foreach (var word in decisionWordsInContext.Take(3))
                        {
                            violations.Add($"     - 使用了裁决性词汇: '{word}'（未引用 ADR）");
        }

        if (violations.Any())
        {
            var errorMessage = string.Join("\n", new[]
            {
                "❌ ADR-910_1_1 违规：以下 README 文档包含裁决性语言但未引用 ADR",
                "",
                "根据 ADR-910_1_1：README 仅允许解释如何使用，禁止定义架构规则或做出架构判断。",
                ""
            }
            .Concat(violations.Take(20))
            .Concat(violations.Count > 20 ? new[] { $"  ... 还有 {violations.Count - 20} 个违规" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 将裁决性语言替换为描述性语言（如：'必须' → '建议'）",
                "  2. 或添加明确的 ADR 引用（如：'根据 ADR-001，模块必须隔离'）",
                "  3. 或将内容移到代码示例标记中（如：// ✅ 正确 // ❌ 错误）",
                "",
                "参考：docs/adr/governance/ADR-910-readme-governance-constitution.md §1.1"
            }));
            
            Assert.Fail(errorMessage);
        }
    }

    /// <summary>
    /// ADR-910_1_2: 禁用裁决性语言规则
    /// 验证 README 不使用裁决性词汇，除非有 ADR 引用支撑（§1.2）
    /// </summary>
    [Fact(DisplayName = "ADR-910_1_2: 禁用裁决性语言规则")]
    public void ADR_910_1_2_README_Must_Not_Use_Decision_Language()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var violations = new List<string>();
        
        var docsDirectory = Path.Combine(repoRoot, DocsPath);
        Directory.Exists(docsDirectory).Should().BeTrue($"未找到文档目录：{docsDirectory}");

        // 扫描所有 README 文件
        var readmeFiles = Directory
            .GetFiles(docsDirectory, "README.md", SearchOption.AllDirectories)
            .Take(MaxReadmeFilesToCheck)
            .ToList();
        
        var rootReadme = Path.Combine(repoRoot, "README.md");
        if (File.Exists(rootReadme))
        {
            readmeFiles.Add(rootReadme);
        }

        foreach (var file in readmeFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            
            // 对于每个裁决性词汇，检查其使用上下文
            foreach (var decisionWord in DecisionLanguageWords)
            {
                var pattern = $@"(?<context>.{{0,50}}){Regex.Escape(decisionWord)}(?<after>.{{0,50}})";
                var matches = Regex.Matches(content, pattern);
                
                foreach (Match match in matches)
                {
                    var context = match.Groups["context"].Value + decisionWord + match.Groups["after"].Value;
                    
                    // 检查是否在允许的上下文中
                    var isInCodeBlock = IsInCodeBlock(content, match.Index);
                    var hasAdrReference = context.Contains("ADR-") || context.Contains("根据");
                    var isInComparisonTable = context.Contains("|") && (context.Contains("✅") || context.Contains("❌"));
                    
                    if (!isInCodeBlock && !hasAdrReference && !isInComparisonTable)
                    {
                        violations.Add($"  ❌ {relativePath}");
                        violations.Add($"     - 使用了裁决性词汇: '{decisionWord}'");
                        violations.Add($"     - 上下文: ...{context.Trim()}...");
                        break; // 每个文件只报告一次

        if (violations.Any())
        {
            var errorMessage = string.Join("\n", new[]
            {
                "❌ ADR-910_1_2 违规：以下 README 文档使用了未授权的裁决性语言",
                "",
                "根据 ADR-910_1_2：README 禁止使用裁决性词汇（必须、禁止、不允许、应当、违规），",
                "除非在以下允许的上下文中：",
                "  1. 明确引用 ADR（如：'根据 ADR-001，模块必须隔离'）",
                "  2. 代码示例标记（如：// ✅ 正确 // ❌ 错误）",
                "  3. 对比表格（如：| 操作 | 是否允许 | 依据 |）",
                ""
            }
            .Concat(violations.Take(20))
            .Concat(violations.Count > 20 ? new[] { $"  ... 还有 {violations.Count - 20} 个违规" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 将裁决性词汇替换为描述性表达（如：'必须' → '建议'、'推荐'）",
                "  2. 或添加明确的 ADR 引用",
                "  3. 或将内容移入允许的上下文（代码块、表格）",
                "",
                "参考：docs/adr/governance/ADR-910-readme-governance-constitution.md §1.2"
            }));
            
            Assert.Fail(errorMessage);
        }
    }

    /// <summary>
    /// ADR-910_1_3: 必须包含无裁决力声明
    /// 验证所有涉及架构内容的 README 必须在开头包含无裁决力声明（§1.3）
    /// </summary>
    [Fact(DisplayName = "ADR-910_1_3: 必须包含无裁决力声明")]
    public void ADR_910_1_3_README_Must_Declare_No_Authority()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var violations = new List<string>();
        
        var docsDirectory = Path.Combine(repoRoot, DocsPath);
        Directory.Exists(docsDirectory).Should().BeTrue($"未找到文档目录：{docsDirectory}");

        // 扫描所有 README 文件
        var readmeFiles = Directory
            .GetFiles(docsDirectory, "README.md", SearchOption.AllDirectories)
            .Take(MaxReadmeFilesToCheck)
            .ToList();
        
        var rootReadme = Path.Combine(repoRoot, "README.md");
        if (File.Exists(rootReadme))
        {
            readmeFiles.Add(rootReadme);
        }

        foreach (var file in readmeFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            
            // 检查文档是否涉及架构内容（包含 ADR 引用或架构关键词）
            var hasArchitectureContent = 
                Regex.IsMatch(content, @"ADR-\d{3,4}") || 
                content.Contains("架构", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("模块", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("设计", StringComparison.OrdinalIgnoreCase);
            
            if (hasArchitectureContent)
            {
                // 检查是否是纯操作性 README（例外情况）
                var isPureOperational = 
                    !Regex.IsMatch(content, @"ADR-\d{3,4}") && 
                    content.Length < 500; // 简短的操作说明
                
                if (!isPureOperational)
                {
                    // 检查是否包含无裁决力声明
                    var hasDeclaration = content.Contains(NoAuthorityDeclaration, StringComparison.Ordinal);
                    
                    if (!hasDeclaration)
                    {
                        // 检查声明位置是否正确（应在开头，标题之后）
                        var lines = content.Split('\n');
                        var titleLineIndex = -1;
                        for (int i = 0; i < Math.Min(lines.Length, 10); i++)
                        {
                            if (lines[i].StartsWith("# "))
                            {
                                titleLineIndex = i;
                                break;
                            }
                        }
                        
                        var hasDeclarationInCorrectPosition = false;
                        if (titleLineIndex >= 0 && titleLineIndex + 5 < lines.Length)
                        {
                            var earlyContent = string.Join("\n", lines.Take(titleLineIndex + 5));
                            hasDeclarationInCorrectPosition = earlyContent.Contains(NoAuthorityDeclaration);
                        }
                        
                        if (!hasDeclaration || !hasDeclarationInCorrectPosition)
                        {
                            violations.Add($"  ❌ {relativePath}");
                            if (!hasDeclaration)
                            {
                                violations.Add($"     - 缺少无裁决力声明");
                            }
                            else
                            {
                                violations.Add($"     - 无裁决力声明位置不正确（应在标题之后，架构内容之前）");
            }
        }

        if (violations.Any())
        {
            var errorMessage = string.Join("\n", new[]
            {
                "❌ ADR-910_1_3 违规：以下 README 文档缺少无裁决力声明或位置不正确",
                "",
                "根据 ADR-910_1_3：所有涉及架构内容的 README 必须在开头包含无裁决力声明。",
                "",
                "标准格式：",
                "```markdown",
                "⚠️ 本文档不具备裁决力。所有架构决策以对应 ADR 正文为准。",
                "```",
                ""
            }
            .Concat(violations.Take(20))
            .Concat(violations.Count > 20 ? new[] { $"  ... 还有 {violations.Count - 20} 个违规" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 在 README 标题之后添加无裁决力声明",
                "  2. 使用 ⚠️ 警告符号",
                "  3. 确保声明在任何架构内容之前",
                "",
                "例外情况（无需声明）：",
                "  - 纯操作性 README（仅命令示例）",
                "  - 第三方库的 README",
                "  - 自动生成的 README",
                "",
                "参考：docs/adr/governance/ADR-910-readme-governance-constitution.md §1.3"
            }));
            
            Assert.Fail(errorMessage);
        }
    }

    private static List<string> FindDecisionWordsWithoutAdrContext(string content)
    {
        var foundWords = new List<string>();
        
        foreach (var word in DecisionLanguageWords)
        {
            var pattern = $@"(?<before>.{{0,100}}){Regex.Escape(word)}(?<after>.{{0,100}})";
            var matches = Regex.Matches(content, pattern);
            
            foreach (Match match in matches)
            {
                var before = match.Groups["before"].Value;
                var after = match.Groups["after"].Value;
                var context = before + word + after;
                
                // 检查上下文中是否有 ADR 引用
                if (!context.Contains("ADR-") && 
                    !context.Contains("根据") && 
                    !IsInCodeBlock(content, match.Index) &&
                    !(context.Contains("|") && (context.Contains("✅") || context.Contains("❌"))))
                {
                    foundWords.Add(word);
        
        return foundWords.Distinct().ToList();
    }

    private static bool IsInCodeBlock(string content, int position)
    {
        // 简单检查：查找位置前后是否有代码块标记
        var beforeContent = content.Substring(0, Math.Min(position, content.Length));
        var afterContent = content.Substring(position, Math.Min(200, content.Length - position));
        
        // 检查是否在 ``` 代码块中
        var codeBlockCount = beforeContent.Split("```").Length - 1;
        var isInCodeBlock = codeBlockCount % 2 == 1;
        
        // 检查是否在行内代码中 `...`
        var lastBacktick = beforeContent.LastIndexOf('`');
        var nextBacktick = afterContent.IndexOf('`');
        if (lastBacktick >= 0 && nextBacktick >= 0)
        {
            var betweenBackticks = beforeContent.Substring(lastBacktick + 1) + afterContent.Substring(0, nextBacktick);
            if (!betweenBackticks.Contains('\n'))
            {
                isInCodeBlock = true;
            }
        }
        
        return isInCodeBlock;
}
}
}
}
}
}
}
}
}
}
}
}
}
}
}
}
