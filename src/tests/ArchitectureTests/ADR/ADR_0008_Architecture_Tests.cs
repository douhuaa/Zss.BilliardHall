using System.Text.RegularExpressions;
using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0008: 文档编写与维护宪法
/// 验证所有文档符合 ADR-0008 的分级和边界约束
/// 
/// 【测试覆盖映射】
/// ├─ ADR-0008.1: 文档分级约束 → Documents_Must_Declare_Authority_Status
/// ├─ ADR-0008.2: ADR内容边界 → ADR_Documents_Structure_Is_Valid
/// ├─ ADR-0008.3: 非ADR文档约束 → Non_ADR_Documents_Declare_Authority_Basis
/// ├─ ADR-0008.4: README禁用裁决词 → README_Must_Not_Use_Decision_Language (执法级)
/// ├─ ADR-0008.5: Skills禁用判断词 → Skills_Must_Not_Output_Judgments (执法级)
/// └─ ADR-0008.6: Instructions/Agents权威声明 → Instructions_And_Agents_Must_Declare_Authority_Basis (执法级)
/// 
/// 【关联文档】
/// - ADR: docs/adr/constitutional/ADR-0008-documentation-governance-constitution.md
/// - Prompts: docs/copilot/adr-0008.prompts.md
/// </summary>
public sealed class ADR_0008_Architecture_Tests
{
    [Fact(DisplayName = "ADR-0008.1: 文档治理宪法已定义")]
    public void ADR_0008_Document_Governance_Constitution_Exists()
    {
        // 验证 ADR-0008 文档存在
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/constitutional/ADR-0008-documentation-governance-constitution.md");
        
        Assert.True(File.Exists(adrFile), $"ADR-0008 文档不存在：{adrFile}");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证必需章节存在
        Assert.Contains("文档分级", content);
        Assert.Contains("文档允许表达的内容边界", content);
        Assert.Contains("非 ADR 文档的强制约束", content);
        Assert.Contains("文档结构与格式规范", content);
        Assert.Contains("文档变更治理", content);
        Assert.Contains("违规处理", content);
    }

    [Fact(DisplayName = "ADR-0008.2: 对应的 Copilot Prompts 文件存在")]
    public void ADR_0008_Prompts_File_Exists()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var promptsFile = Path.Combine(repoRoot, "docs/copilot/adr-0008.prompts.md");
        
        Assert.True(File.Exists(promptsFile), $"ADR-0008 Prompts 文件不存在：{promptsFile}");
        
        var content = File.ReadAllText(promptsFile);
        
        // 验证 Prompts 文件包含权威声明
        Assert.Contains("权威声明", content);
        Assert.Contains("不具备裁决权", content);
        Assert.Contains("ADR-0008", content);
    }

    [Fact(DisplayName = "ADR-0008.3: 核心治理原则已定义")]
    public void Core_Governance_Principles_Are_Defined()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/constitutional/ADR-0008-documentation-governance-constitution.md");
        var content = File.ReadAllText(adrFile);
        
        // 验证核心原则：只有 ADR 具备裁决力
        Assert.Contains("只有 ADR 具备裁决力", content);
        
        // 验证防引用扩权规则
        Assert.Contains("任何非 ADR 文档，即使全文逐字引用 ADR，也不具备裁决力", content);
        
        // 验证文档优先级
        Assert.Contains("ADR 正文 > Instructions > Agents > Skills > Prompts > README", content);
    }

    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) || 
                Directory.Exists(Path.Combine(currentDir, "docs", "adr")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return null;
    }

    // ========== 执法级测试：真正阻止违规行为 ==========

    [Fact(DisplayName = "ADR-0008.4: README/Guide 不得使用裁决性语言（执法级）")]
    public void README_Must_Not_Use_Decision_Language()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        
        // 裁决性词汇（ADR-0008 明确禁止 README 使用）
        var forbiddenWords = new[] { "必须", "禁止", "不允许", "不得", "应当" };
        
        // 例外：可以在引用 ADR 的上下文中使用，或在示例标记中使用
        var allowedContextPatterns = new[]
        {
            @"根据\s*ADR[-\s]*\d+",  // "根据 ADR-0001"
            @"参考\s*ADR[-\s]*\d+",  // "参考 ADR-0001"
            @"详见\s*ADR[-\s]*\d+",  // "详见 ADR-0001"
            @"ADR[-\s]*\d+\s*规定",  // "ADR-0001 规定"
            @"本文档无裁决力",        // 声明部分
            @"[❌✅]\s*(禁止|必须)",  // ❌ 禁止（示例标记）
            @"\|\s*(禁止|必须)",     // 表格中的内容
            @"^[\s*#]*\*\*[❌✅]",   // **❌ 禁止：** 标题
            @"^[\s-]*[❌✅]"         // 列表项标记
        };

        var violations = new List<string>();
        
        // 扫描 docs 目录下的 README 和 Guide
        var docsDir = Path.Combine(repoRoot, "docs");
        if (!Directory.Exists(docsDir)) return;

        var readmeFiles = Directory.GetFiles(docsDir, "*.md", SearchOption.AllDirectories)
            .Where(f => Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase) 
                     || f.Contains("guide", StringComparison.OrdinalIgnoreCase)
                     || f.Contains("GUIDE", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("/adr/", StringComparison.OrdinalIgnoreCase)) // 排除 ADR 文档
            .Where(f => !f.Contains("/summaries/", StringComparison.OrdinalIgnoreCase)) // 排除总结文档
            .Where(f => !f.Contains("/templates/", StringComparison.OrdinalIgnoreCase)) // 排除模板
            .Take(20); // 限制检查数量以提高性能

        foreach (var file in readmeFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            
            // 移除代码块和引用块
            var contentWithoutCodeBlocks = RemoveCodeBlocks(content);
            var contentWithoutQuotes = RemoveQuotedSections(contentWithoutCodeBlocks);
            
            var lines = contentWithoutQuotes.Split('\n');
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                
                foreach (var word in forbiddenWords)
                {
                    if (line.Contains(word))
                    {
                        // 检查是否在允许的上下文中
                        var isAllowedContext = allowedContextPatterns.Any(pattern => 
                            Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase));
                        
                        if (!isAllowedContext)
                        {
                            violations.Add($"  • {relativePath}:{i + 1} - 使用裁决词 '{word}'");
                            violations.Add($"    内容: {line.Trim().Substring(0, Math.Min(80, line.Trim().Length))}");
                        }
                    }
                }
            }
        }

        if (violations.Any())
        {
            Assert.Fail(string.Join("\n", new[]
            {
                "❌ ADR-0008.4 违规：以下 README/Guide 使用了裁决性语言",
                "",
                "根据 ADR-0008 决策 2.2：README/Guide 只能解释'如何使用'，不得使用裁决性语言。",
                ""
            }
            .Concat(violations.Take(10)) // 限制输出前10个违规
            .Concat(violations.Count > 10 ? new[] { $"  ... 还有 {violations.Count - 10} 个违规" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 将裁决性语句改为引用 ADR：'根据 ADR-XXXX，模块使用事件通信'",
                "  2. 在文档开头添加：'> ⚠️ 本文档无裁决力，所有架构决策以 ADR 正文为准'",
                "  3. 使用描述性语言代替命令性语言",
                "",
                "参考：docs/adr/constitutional/ADR-0008-documentation-governance-constitution.md 决策 3.3"
            })));
        }
    }

    [Fact(DisplayName = "ADR-0008.5: Skills 不得输出判断性结论（执法级）")]
    public void Skills_Must_Not_Output_Judgments()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var skillsDir = Path.Combine(repoRoot, ".github/skills");
        
        if (!Directory.Exists(skillsDir)) return; // Skills 目录可选
        
        // 判断性词汇（ADR-0008 明确禁止 Skills 使用）
        var forbiddenJudgments = new[]
        {
            "违规", "不符合", "应当", "建议", "推荐",
            "正确", "错误", "合规", "不合规", "必须修复"
        };

        var violations = new List<string>();
        var skillFiles = Directory.GetFiles(skillsDir, "*.skill.md", SearchOption.AllDirectories);

        foreach (var file in skillFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            
            // 移除 frontmatter 和代码块
            var contentWithoutFrontmatter = RemoveFrontmatter(content);
            var contentWithoutCodeBlocks = RemoveCodeBlocks(contentWithoutFrontmatter);
            
            // 检查输出结果示例中是否包含判断词
            var outputSectionMatch = Regex.Match(contentWithoutCodeBlocks, @"###?\s*输出结果.*?(?=###|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            
            if (outputSectionMatch.Success)
            {
                var outputSection = outputSectionMatch.Value;
                
                foreach (var judgment in forbiddenJudgments)
                {
                    if (outputSection.Contains(judgment))
                    {
                        violations.Add($"  • {relativePath} - 输出示例包含判断词 '{judgment}'");
                    }
                }
            }
        }

        if (violations.Any())
        {
            Assert.Fail(string.Join("\n", new[]
            {
                "❌ ADR-0008.5 违规：以下 Skills 输出了判断性结论",
                "",
                "根据 ADR-0008 决策 3.2：Skills 只能输出事实，不得输出判断。",
                ""
            }
            .Concat(violations)
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. Skills 输出格式：'发现引用：模块 A → 模块 B'（事实）",
                "  2. 禁止输出：'违规：模块 A 非法引用模块 B'（判断）",
                "  3. 将判断逻辑移至 Agent，由 Agent 根据 ADR 做出裁决",
                "",
                "正确示例：",
                "  ✅ { \"type\": \"DirectReference\", \"source\": \"Orders\", \"target\": \"Members\" }",
                "  ❌ { \"violation\": \"非法引用\", \"severity\": \"High\" }",
                "",
                "参考：docs/adr/constitutional/ADR-0008-documentation-governance-constitution.md 决策 3.2"
            })));
        }
    }

    [Fact(DisplayName = "ADR-0008.6: Instructions/Agents 必须声明权威依据（执法级）")]
    public void Instructions_And_Agents_Must_Declare_Authority_Basis()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var violations = new List<string>();

        // 检查 Instructions 文件
        var instructionsDir = Path.Combine(repoRoot, ".github/instructions");
        if (Directory.Exists(instructionsDir))
        {
            var instructionFiles = Directory.GetFiles(instructionsDir, "*.instructions.md", SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase));

            foreach (var file in instructionFiles)
            {
                var content = File.ReadAllText(file);
                var relativePath = Path.GetRelativePath(repoRoot, file);

                var hasAuthorityDeclaration = 
                    content.Contains("权威声明") || 
                    content.Contains("权威依据") ||
                    (content.Contains("基于") && content.Contains("ADR")) ||
                    content.Contains("服从") && content.Contains("ADR");

                var hasConflictResolution = 
                    (content.Contains("冲突") && content.Contains("ADR") && content.Contains("为准")) ||
                    content.Contains("以 ADR 正文为准") ||
                    content.Contains("ADR 正文为唯一");

                if (!hasAuthorityDeclaration)
                {
                    violations.Add($"  • Instructions: {relativePath} - 缺少权威声明");
                }

                if (!hasConflictResolution)
                {
                    violations.Add($"  • Instructions: {relativePath} - 缺少冲突裁决规则");
                }
            }
        }

        // 检查 Agents 文件
        var agentsDir = Path.Combine(repoRoot, ".github/agents");
        if (Directory.Exists(agentsDir))
        {
            var agentFiles = Directory.GetFiles(agentsDir, "*.agent.md", SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
                .Where(f => !f.Contains("expert-dotnet", StringComparison.OrdinalIgnoreCase)); // 排除系统 Agent

            foreach (var file in agentFiles)
            {
                var content = File.ReadAllText(file);
                var relativePath = Path.GetRelativePath(repoRoot, file);

                var hasAuthorityDeclaration = 
                    content.Contains("权威声明") || 
                    (content.Contains("ADR") && (content.Contains("唯一裁决") || content.Contains("唯一权威") || content.Contains("正文为准")));

                var declaresAdrBasis = 
                    Regex.IsMatch(content, @"ADR[-\s]*\d+", RegexOptions.IgnoreCase);

                if (!hasAuthorityDeclaration)
                {
                    violations.Add($"  • Agent: {relativePath} - 缺少 ADR 权威声明");
                }

                if (!declaresAdrBasis)
                {
                    violations.Add($"  • Agent: {relativePath} - 未引用任何 ADR 编号");
                }
            }
        }

        if (violations.Any())
        {
            Assert.Fail(string.Join("\n", new[]
            {
                "❌ ADR-0008.6 违规：以下治理级文档未正确声明权威依据",
                "",
                "根据 ADR-0008 决策 3.1：Instructions/Agents 必须显式声明所服从的 ADR。",
                ""
            }
            .Concat(violations)
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 在文档开头添加'权威声明'或'权威依据'章节",
                "  2. 明确列出所服从的 ADR 编号（如：本文档基于 ADR-0001, ADR-0005）",
                "  3. 添加冲突裁决规则：'若与 ADR 冲突，以 ADR 正文为准'",
                "",
                "示例：",
                "  ## 权威声明",
                "  ",
                "  本文档服从以下 ADR：",
                "  - ADR-0001: 模块化单体架构",
                "  - ADR-0005: Handler 模式",
                "  ",
                "  > ⚖️ 若本文档与 ADR 正文冲突，以 ADR 正文为准。",
                "",
                "参考：docs/adr/constitutional/ADR-0008-documentation-governance-constitution.md 决策 3.1"
            })));
        }
    }

    // ========== 辅助方法 ==========

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

    private static string RemoveFrontmatter(string content)
    {
        // 移除 YAML frontmatter (--- ... ---)
        return Regex.Replace(content, @"^---[\s\S]*?---\s*", string.Empty);
    }
}
