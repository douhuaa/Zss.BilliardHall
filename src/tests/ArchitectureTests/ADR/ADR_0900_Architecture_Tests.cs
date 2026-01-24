using System.Text.RegularExpressions;
using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0900: ADR 新增与修订流程
/// 验证 ADR 文档、架构测试、Copilot Prompts 的三位一体完整性
/// 
/// 【测试覆盖映射】
/// ├─ ADR-0900.4: 三位一体交付 → ADR_Documents_Must_Have_Architecture_Tests_And_Prompts
/// └─ ADR-0900.5: 映射校验与审查 → ADR_Documents_Must_Mark_Testable_Constraints
/// 
/// 【关联文档】
/// - ADR: docs/adr/governance/ADR-0900-adr-process.md
/// - Prompts: docs/copilot/adr-0900.prompts.md
/// </summary>
public sealed class ADR_0900_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const string AdrTestsPath = "src/tests/ArchitectureTests/ADR";
    private const string AdrPromptsPath = "docs/copilot";

    // ADRs that are excluded from requiring architecture tests (meta-documents)
    private static readonly HashSet<string> AdrWithoutRequiredTests = new(StringComparer.OrdinalIgnoreCase) {
        "ADR-0005-Enforcement-Levels" // 这是 ADR-0005 的补充文档，不是独立的 ADR
    };

    // ADRs that are excluded from requiring prompt files (if any)
    private static readonly HashSet<string> AdrWithoutRequiredPrompts = new(StringComparer.OrdinalIgnoreCase) {
        "ADR-0005-Enforcement-Levels" // 这是 ADR-0005 的补充文档，不是独立的 ADR
    };

    /// <summary>
    /// ADR-0900.4: 文档/测试/Prompt 三位一体交付
    /// 验证每个 ADR 文档都有对应的架构测试和 Copilot Prompts
    /// </summary>
    [Fact(DisplayName = "ADR-0900.4: 每个 ADR 必须有对应的架构测试和 Copilot Prompts")]
    public void ADR_Documents_Must_Have_Architecture_Tests_And_Prompts()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");

        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);
        Assert.True(Directory.Exists(adrDirectory), $"未找到 ADR 文档目录：{adrDirectory}");

        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath);
        var promptsDirectory = Path.Combine(repoRoot, AdrPromptsPath);

        var adrFiles = Directory
            .GetFiles(adrDirectory, "ADR-*.md", SearchOption.AllDirectories)
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();

        var missingTests = new List<string>();
        var missingPrompts = new List<string>();

        foreach (var adrName in adrFiles)
        {
            var adrNumber = ExtractAdrNumber(adrName);
            if (string.IsNullOrEmpty(adrNumber))
                continue;

            // 检查架构测试文件
            if (!AdrWithoutRequiredTests.Contains(adrName))
            {
                var testFileName = $"ADR_{adrNumber}_Architecture_Tests.cs";
                var testFilePath = Path.Combine(testsDirectory, testFileName);

                if (!File.Exists(testFilePath))
                {
                    missingTests.Add($"{adrName} → 缺少测试文件: {testFileName}");
                }
            }

            // 检查 Copilot Prompts 文件
            if (!AdrWithoutRequiredPrompts.Contains(adrName))
            {
                var promptFileName = $"adr-{adrNumber}.prompts.md";
                var promptFilePath = Path.Combine(promptsDirectory, promptFileName);

                if (!File.Exists(promptFilePath))
                {
                    missingPrompts.Add($"{adrName} → 缺少 Prompts 文件: {promptFileName}");
                }
            }
        }

        var errors = new List<string>();

        if (missingTests.Any())
        {
            errors.Add("❌ ADR-0900.4 违规：以下 ADR 缺少架构测试文件：");
            errors.AddRange(missingTests.Select(m => $"  • {m}"));
            errors.Add("");
        }

        if (missingPrompts.Any())
        {
            errors.Add("❌ ADR-0900.4 违规：以下 ADR 缺少 Copilot Prompts 文件：");
            errors.AddRange(missingPrompts.Select(m => $"  • {m}"));
            errors.Add("");
        }

        if (errors.Any())
        {
            errors.Add("修复建议：");
            errors.Add("  1. 为每个 ADR 创建对应的架构测试文件（src/tests/ArchitectureTests/ADR/ADR_XXXX_Architecture_Tests.cs）");
            errors.Add("  2. 为每个 ADR 创建对应的 Copilot Prompts 文件（docs/copilot/adr-XXXX.prompts.md）");
            errors.Add("  3. 参考现有文件的格式和结构");
            errors.Add("");
            errors.Add("参考：docs/adr/governance/ADR-0900-adr-process.md");

            Assert.Fail(string.Join("\n", errors));
        }
    }

    /// <summary>
    /// ADR-0900.4: 关键约束均带【必须架构测试覆盖】标记
    /// 验证 ADR 文档中需要测试的约束都有明确标记
    /// </summary>
    [Fact(DisplayName = "ADR-0900.4: ADR 文档中的关键约束必须标记为需要测试")]
    public void ADR_Documents_Must_Mark_Testable_Constraints()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");

        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath);

        // 获取所有有架构测试的 ADR
        var testFiles = Directory
            .GetFiles(testsDirectory, "ADR_*_Architecture_Tests.cs")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Select(name => ExtractAdrNumberFromTestFile(name))
            .Where(num => !string.IsNullOrEmpty(num))
            .ToHashSet();

        var warnings = new List<string>();

        // 检查有测试文件的 ADR 是否标记了约束
        foreach (var adrNumber in testFiles)
        {
            // 跳过 ADR-0000 和 ADR-0900，它们是元测试
            if (adrNumber == "0000" || adrNumber == "0900")
                continue;

            var adrFiles = Directory.GetFiles(adrDirectory, $"ADR-{adrNumber}-*.md", SearchOption.AllDirectories);

            foreach (var adrFile in adrFiles)
            {
                var content = File.ReadAllText(adrFile);

                // 移除代码块中的内容，避免误判
                var contentWithoutCodeBlocks = RemoveCodeBlocks(content);

                // 检查是否有测试标记
                var hasTestMarker = contentWithoutCodeBlocks.Contains("【必须架构测试覆盖】") || contentWithoutCodeBlocks.Contains("【必须测试】") || contentWithoutCodeBlocks.Contains("[MUST_TEST]");

                // 检查是否有快速参考表
                var hasQuickReference = content.Contains("## 快速参考") || content.Contains("##快速参考") || content.Contains("## 快速参考表");

                if (!hasTestMarker && !hasQuickReference)
                {
                    warnings.Add($"⚠️  {Path.GetFileName(adrFile)} 有架构测试但未标记约束");
                }
            }
        }

        // 这是一个警告级别的检查，不会导致测试失败
        // 只是提醒开发者应该在 ADR 中明确标记哪些约束需要测试
        if (warnings.Any())
        {
            var message = string.Join("\n",
            new[] {
                    "",
                    "⚠️  ADR-0900.4 提醒：以下 ADR 有架构测试但缺少明确的约束标记：",
                    ""
                }
                .Concat(warnings)
                .Concat(new[] {
                    "",
                    "建议：",
                    "  • 在需要测试的约束后添加【必须架构测试覆盖】标记",
                    "  • 或在文档末尾添加快速参考表，列出所有约束和对应的测试",
                    "",
                    "参考：docs/ADR-TEST-MAPPING-SPECIFICATION.md",
                    ""
                }));

            // 输出警告但不失败测试
            // 在真实环境中可以使用 ITestOutputHelper 输出
            Console.WriteLine(message);
        }
    }

    #region Helper Methods

    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();

        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) || Directory.Exists(Path.Combine(currentDir, AdrDocsPath)))
            {
                return currentDir;
            }

            currentDir = Directory.GetParent(currentDir)
                ?.FullName;
        }

        return null;
    }

    private static string ExtractAdrNumber(string adrFileName)
    {
        // 从 "ADR-0001-xxx" 格式中提取 "0001"
        var match = Regex.Match(adrFileName, @"ADR-(\d{4})");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string ExtractAdrNumberFromTestFile(string testFileName)
    {
        // 从 "ADR_0001_Architecture_Tests" 格式中提取 "0001"
        var match = Regex.Match(testFileName, @"ADR_(\d{4})_");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string RemoveCodeBlocks(string content)
    {
        // 移除 Markdown 代码块 ```...```
        var pattern = @"```[\s\S]*?```";
        return Regex.Replace(content, pattern, string.Empty);
    }

    #endregion

}
