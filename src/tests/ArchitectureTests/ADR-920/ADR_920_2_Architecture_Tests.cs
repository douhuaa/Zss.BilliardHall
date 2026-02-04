using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_920;

/// <summary>
/// ADR-920_2: 示例代码架构约束（Rule）
/// 验证示例代码不包含任何架构违规行为
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-920_2_1: 示例代码禁止的架构违规行为
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-920-examples-governance-constitution.md
/// </summary>
public sealed class ADR_920_2_Architecture_Tests
{
    private const int MaxExampleFilesToCheck = 50;

    // 架构违规模式（示例中禁止出现）
    private static readonly string[] ForbiddenPatterns = new[]
    {
        // 跨模块直接引用（ADR-001）
        @"using\s+Zss\.BilliardHall\.Modules\.\w+\.Domain",
        @"using\s+Zss\.BilliardHall\.Modules\.\w+\.Infrastructure",

        // Service 类（ADR-001）
        @"class\s+\w+Service\s*[:{]",
        @"interface\s+I\w+Service\s*[:{]",
    };

    // 允许的上下文模式（即使包含违规代码也可豁免）
    private static readonly string[] AllowedContextPatterns = new[]
    {
        @"//\s*❌\s*(错误|禁止|不推荐)",  // 明确标记的错误示例
        @"//\s*BAD\s*EXAMPLE",           // 英文错误示例标记
        @"//\s*WRONG",                   // 错误标记
        @"/\*\*.*示例.*违规.*\*/",        // 注释说明这是违规示例
        @"//\s*反例\s*（禁止）",          // ADR 正文反例标记
        @"//\s*📐\s*结构示意",            // ADR 正文结构示意
    };

    /// <summary>
    /// ADR-920_2_1: 示例代码禁止的架构违规行为
    /// 验证示例代码不包含跨模块引用、Service层等架构违规（§ADR-920_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-920_2_1: 示例代码不得包含架构违规")]
    public void ADR_920_2_1_Examples_Must_Not_Contain_Architecture_Violations()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var violations = new List<string>();

        // 扫描示例文件
        var exampleFiles = new List<string>();

        // 1. examples/ 目录（如果存在）
        var examplesDir = Path.Combine(repoRoot, "examples");
        if (Directory.Exists(examplesDir))
        {
            exampleFiles.AddRange(
                Directory.GetFiles(examplesDir, "*.cs", SearchOption.AllDirectories)
            );
        }

        // 2. docs/examples/ 目录（如果存在）
        var docsExamplesDir = Path.Combine(repoRoot, "docs", "examples");
        if (Directory.Exists(docsExamplesDir))
        {
            exampleFiles.AddRange(
                Directory.GetFiles(docsExamplesDir, "*.cs", SearchOption.AllDirectories)
            );
        }

        foreach (var file in exampleFiles.Take(MaxExampleFilesToCheck))
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            var lines = content.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                foreach (var pattern in ForbiddenPatterns)
                {
                    if (Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase))
                    {
                        // 检查是否在允许的上下文中（如明确标记的错误示例）
                        var isAllowedContext = CheckAllowedContext(lines, i);

                        if (!isAllowedContext)
                        {
                            violations.Add($"  • {relativePath}:{i + 1}");
                            var displayLine = line.Trim();
                            if (displayLine.Length > 80)
                            {
                                displayLine = displayLine.Substring(0, 80) + "...";
                            }
                            violations.Add($"    内容: {displayLine}");
                            violations.Add($"    违规模式: {pattern}");
                        }
                    }
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(string.Join("\n", new[]
            {
                "❌ ADR-920_2_1 违规：以下示例代码包含架构违规",
                "",
                "根据 ADR-920_2_1：示例代码禁止的架构违规行为。",
                ""
            }
            .Concat(violations.Take(20))
            .Concat(violations.Count > 20 ? new[] { $"  ... 还有 {violations.Count - 20} 个违规" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 移除跨模块直接引用，使用事件或契约（ADR-001）",
                "  2. 移除 Service 类，使用垂直切片 Handler（ADR-001）",
                "  3. 确保 Platform 层不依赖业务层（ADR-002）",
                "  4. 如果这是错误示例，请明确标记：// ❌ 错误：...",
                "",
                "允许的标记方式：",
                "  ✅ '// ❌ 错误：直接引用其他模块'",
                "  ✅ '// BAD EXAMPLE: cross-module reference'",
                "  ✅ '/* 以下是违规示例，仅用于教学 */'",
                "",
                "参考：docs/adr/governance/ADR-920-examples-governance-constitution.md §ADR-920_2_1"
            })));
        }
    }

    // ========== 辅助方法 ==========

    private static string? FindRepositoryRoot()
    {
        var envRoot = Environment.GetEnvironmentVariable("REPO_ROOT");
        if (!string.IsNullOrEmpty(envRoot) && Directory.Exists(envRoot))
        {
            return envRoot;
        }

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

    private static bool CheckAllowedContext(string[] lines, int lineIndex)
    {
        // 检查当前行及其前后几行是否有允许的上下文标记
        int startLine = Math.Max(0, lineIndex - 2);
        int endLine = Math.Min(lines.Length - 1, lineIndex + 2);

        for (int i = startLine; i <= endLine; i++)
        {
            foreach (var pattern in AllowedContextPatterns)
            {
                if (Regex.IsMatch(lines[i], pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
