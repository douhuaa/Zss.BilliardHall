using System.Text.RegularExpressions;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_910;

/// <summary>
/// ADR-910_2: README 与 ADR 的关系治理（Rule）
/// 验证 README 正确引用 ADR，并遵循变更治理规则
/// 
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-910_2_1: README 引用 ADR 的规范
/// - ADR-910_2_2: README 的变更治理规则（L2警告级）
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-910-readme-governance-constitution.md
/// </summary>
public sealed class ADR_910_2_Architecture_Tests
{
    private const string DocsPath = "docs";
    private const string AdrPath = "docs/adr";
    private const int MaxReadmeFilesToCheck = 50;

    /// <summary>
    /// ADR-910_2_1: README 引用 ADR 的规范
    /// 验证 README 正确引用 ADR，不复制 ADR 内容（§2.1）
    /// </summary>
    [Fact(DisplayName = "ADR-910_2_1: README 引用 ADR 的规范")]
    public void ADR_910_2_1_README_Must_Reference_ADR_Properly()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
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
            
            // 查找 ADR 引用
            var adrReferences = Regex.Matches(content, @"ADR-\d{3,4}");
            
            if (adrReferences.Count > 0)
            {
                // 检查 ADR 引用的方式
                foreach (Match adrRef in adrReferences)
                {
                    var adrNumber = adrRef.Value;
                    var position = adrRef.Index;
                    
                    // 检查引用周围的上下文
                    var startPos = Math.Max(0, position - 100);
                    var length = Math.Min(content.Length - startPos, 200);
                    var context = content.Substring(startPos, length);
                    
                    // 检查是否是链接形式（推荐）
                    var hasLink = Regex.IsMatch(context, $@"\[{Regex.Escape(adrNumber)}\]\(.*?\)");
                    
                    // 检查是否复制了大量 ADR 内容（禁止）
                    // 如果在同一段落中有大量 Decision 章节的内容，视为复制
                    var hasDecisionCopy = context.Contains("Decision") || context.Contains("裁决");
                    var hasRuleCopy = context.Contains("Rule") || context.Contains("规则编号");
                    
                    if (hasDecisionCopy && hasRuleCopy)
                    {
                        violations.Add($"  ❌ {relativePath}");
                        violations.Add($"     - 可能复制了 {adrNumber} 的内容（而非引用）");
                        violations.Add($"     - 上下文: ...{context.Substring(0, Math.Min(100, context.Length))}...");
                        break; // 每个文件只报告一次
                    }
                }
                
                // 检查是否存在长篇转述（超过 200 字的架构规则描述）
                var paragraphs = content.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var paragraph in paragraphs)
                {
                    if (paragraph.Length > 300 && 
                        Regex.IsMatch(paragraph, @"ADR-\d{3,4}") &&
                        (paragraph.Contains("必须") || paragraph.Contains("禁止") || paragraph.Contains("不允许")))
                    {
                        // 可能是长篇转述 ADR 内容
                        var wordCount = paragraph.Length;
                        if (wordCount > 500)
                        {
                            violations.Add($"  ⚠️ {relativePath}");
                            violations.Add($"     - 可能包含长篇 ADR 内容转述（{wordCount} 字符）");
                            violations.Add($"     - 建议：使用简短说明 + ADR 链接，而非复制完整内容");
                            break;
                        }
                    }
                }
            }
        }

        if (violations.Any())
        {
            var errorMessage = string.Join("\n", new[]
            {
                "❌ ADR-910_2_1 违规：以下 README 文档的 ADR 引用方式不规范",
                "",
                "根据 ADR-910_2_1：README 是 ADR 的索引和指南，不是 ADR 的副本或替代品。",
                "",
                "允许的引用方式：",
                "  ✅ '根据 ADR-001，模块使用事件通信。详见 [ADR-001](链接)'",
                "  ✅ '参考 ADR-005 了解 Handler 模式'",
                "",
                "禁止的做法：",
                "  ❌ 长篇复制 ADR 内容",
                "  ❌ 转述 ADR 为自己的命令",
                "  ❌ 复制 ADR 的 Decision 章节",
                ""
            }
            .Concat(violations.Take(20))
            .Concat(violations.Count > 20 ? new[] { $"  ... 还有 {violations.Count - 20} 个违规" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 使用链接而非复制内容",
                "  2. 简短说明 + ADR 引用",
                "  3. 避免转述为命令性语言",
                "",
                "参考：docs/adr/governance/ADR-910-readme-governance-constitution.md §2.1"
            }));
            
            Assert.Fail(errorMessage);
        }
    }

    /// <summary>
    /// ADR-910_2_2: README 的变更治理规则
    /// 验证 README 变更遵循治理规则，与 ADR 保持一致（L2警告级）（§2.2）
    /// </summary>
    [Fact(DisplayName = "ADR-910_2_2: README 的变更治理规则（L2警告）")]
    public void ADR_910_2_2_README_Changes_Must_Follow_Governance()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var warnings = new List<string>();
        
        var docsDirectory = Path.Combine(repoRoot, DocsPath);
        Directory.Exists(docsDirectory).Should().BeTrue($"未找到文档目录：{docsDirectory}");
        
        var adrDirectory = Path.Combine(repoRoot, AdrPath);
        Directory.Exists(adrDirectory).Should().BeTrue($"未找到 ADR 目录：{adrDirectory}");

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
            
            // 查找 ADR 引用
            var adrReferences = Regex.Matches(content, @"ADR-(\d{3,4})");
            
            foreach (Match adrRef in adrReferences)
            {
                var adrNumber = "ADR-" + adrRef.Groups[1].Value;
                
                // 检查引用的 ADR 是否存在
                var adrExists = CheckAdrExists(repoRoot, adrNumber);
                
                if (!adrExists)
                {
                    warnings.Add($"  ⚠️ {relativePath}");
                    warnings.Add($"     - 引用了不存在的 ADR：{adrNumber}");
                }
            }
            
            // 检查 README 是否声明了变更治理规则
            // 对于仓库根 README，应该包含对文档治理的说明
            if (file == rootReadme)
            {
                var mentionsDocumentationGovernance = 
                    content.Contains("文档治理") || 
                    content.Contains("ADR-008") ||
                    content.Contains("ADR-910");
                
                if (!mentionsDocumentationGovernance && content.Contains("架构"))
                {
                    warnings.Add($"  ⚠️ {relativePath}");
                    warnings.Add($"     - 建议：根 README 应说明文档治理规则或引用相关 ADR");
                }
            }
        }

        // L2 警告级别：警告但不失败构建
        if (warnings.Any())
        {
            var warningMessage = string.Join("\n", new[]
            {
                "⚠️ ADR-910_2_2 警告（L2）：以下 README 文档的变更治理需要审查",
                "",
                "根据 ADR-910_2_2：README 引用的 ADR 必须存在，且根 README 应说明文档治理规则。",
                ""
            }
            .Concat(warnings.Take(20))
            .Concat(warnings.Count > 20 ? new[] { $"  ... 还有 {warnings.Count - 20} 个警告" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "建议：",
                "  1. 确保引用的 ADR 文档存在",
                "  2. 在根 README 中说明文档治理规则",
                "  3. README 变更时，确保与相关 ADR 保持一致",
                "",
                "注意：这是 L2 警告级别，不会阻断构建。",
                "需要在 Code Review 中人工审查变更的合规性。",
                "",
                "参考：docs/adr/governance/ADR-910-readme-governance-constitution.md §2.2"
            }));
            
            Console.WriteLine(warningMessage);
            Console.WriteLine();
        }
        
        // L2 警告：测试总是通过，但已输出警告信息
    }

    // ========== 辅助方法 ==========


    private static bool CheckAdrExists(string repoRoot, string adrNumber)
    {
        var adrDirectory = Path.Combine(repoRoot, AdrPath);
        
        // 搜索所有子目录中的 ADR 文件
        var adrFiles = Directory
            .GetFiles(adrDirectory, $"{adrNumber}*.md", SearchOption.AllDirectories);
        
        return adrFiles.Length > 0;
    }
}
