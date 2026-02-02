using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-920: 示例代码治理宪法
/// 验证所有示例代码符合 ADR-920 的约束
///
/// 【测试覆盖映射】
/// ├─ ADR-920.1: 示例代码不得跨模块直接引用 (L1) → Examples_Should_Not_Reference_Other_Modules
/// ├─ ADR-920.2: 示例文档必须包含免责声明 (L1) → Example_Documents_Must_Have_Disclaimer
/// ├─ ADR-920.3: README C# 代码块不得引入架构违规 (L2) → README_CSharp_Code_Examples_Should_Not_Violate_Architecture
/// ├─ ADR-920.4: 示例目录必须有责任人和目的说明 (L1) → Example_Directories_Must_Have_Owner_And_Purpose
/// ├─ ADR-920.5: 示例治理宪法已定义 → ADR_920_Examples_Governance_Constitution_Exists
/// └─ ADR-920.6: 对应的 Copilot Prompts 文件存在 → ADR_920_Prompts_File_Exists
///
/// 【执法分级】
/// - L1（阻断）：结构违规（跨模块引用、Service、缺少责任人）
/// - L2（警告）：模式偏差（Handler 返回类型、命名约定）
/// - L3（允许）：教学简化（省略异常处理、日志）
///
/// 【技术局限性】
/// - 当前使用正则表达式进行启发式检测（trade-off：性能 vs 精确度）
/// - 可能存在极小概率的误判或漏判（特别是复杂多行语句、新 C# 语法）
/// - 未来可升级为 Roslyn Analyzer 以提供语义级检测
/// - 测试未检出的违规仍是违规，需在 Code Review 中捕获
///
/// 【职责边界（重要）】
/// 本测试类**仅管**：示例 ≠ 规则（示例不可违反 ADR）
/// 本测试类**不管**：代码美学、教学质量、文档完整性
///
/// ⚠️ 避免职责膨胀：不要无限往此类添加规则
/// 新的治理关注点应开新测试类（如 Document_Governance_Tests）
///
/// 【关联文档】
/// - ADR: docs/adr/governance/ADR-920-examples-governance-constitution.md
/// - Prompts: docs/copilot/adr-920.prompts.md
/// </summary>
public sealed class ADR_920_Architecture_Tests
{
    [Fact(DisplayName = "ADR-0920_1_1: 示例治理宪法已定义")]
    public void ADR_920_Examples_Governance_Constitution_Exists()
    {
        // 验证 ADR-920 文档存在
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-920-examples-governance-constitution.md");

        File.Exists(adrFile).Should().BeTrue($"ADR-920 文档不存在：{adrFile}");

        var content = File.ReadAllText(adrFile);

        // 验证必需章节存在
        content.Should().Contain("示例代码的法律地位");
        content.Should().Contain("示例代码必须包含的免责声明");
        content.Should().Contain("示例代码禁止的架构违规行为");
        content.Should().Contain("示例 vs 测试 vs PoC");
        // content.Should().Contain("示例代码的自动化执法（分级处理）");
        content.Should().Contain("示例作者责任制");
    }

    [Fact(DisplayName = "ADR-0920_1_2: 对应的 Copilot Prompts 文件存在")]
    public void ADR_920_Prompts_File_Exists()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var promptsFile = Path.Combine(repoRoot, "docs/copilot/adr-920.prompts.md");

        // 注意：此测试在 Prompts 文件创建后才会通过
        // 如果文件不存在，给出清晰的待办提示
        if (!File.Exists(promptsFile))
        {
            true.Should().BeFalse($"⚠️ 待办：ADR-920 Prompts 文件需要创建：{promptsFile}\n" +
                       "请创建该文件以提供示例编写的场景化指导。");
        }

        var content = File.ReadAllText(promptsFile);

        // 验证 Prompts 文件包含权威声明
        content.Should().Contain("权威声明");
        content.Should().Contain("ADR-920");
    }

    [Fact(DisplayName = "ADR-0920_1_3: 核心治理原则已定义")]
    public void Core_Examples_Governance_Principles_Are_Defined()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-920-examples-governance-constitution.md");
        var content = File.ReadAllText(adrFile);

        // 验证核心原则：示例无裁决力
        content.Should().Contain("示例不是规范，只是演示");

        // 验证禁止行为约束
        content.Should().Contain("示例代码禁止的架构违规行为");

        // 验证免责声明要求
        content.Should().Contain("示例代码必须包含的免责声明");

        // 验证核心灵魂句
        content.Should().Contain("示例允许简化流程，但不允许简化规则");

        // 验证分级执法
        // content.Should().Contain("同规则、不同严重级别");

        // 验证责任制
        content.Should().Contain("示例作者责任制");
    }

    /// <summary>
    /// 查找仓库根目录
    /// ⚠️ 健壮性改进：支持环境变量覆盖，避免 CI shallow clone、mono-repo 等场景翻车
    /// </summary>
    private static string? FindRepositoryRoot()
    {
        // 优先使用环境变量（CI、mono-repo、NuGet 引用场景）
        var envRoot = Environment.GetEnvironmentVariable("REPO_ROOT");
        if (!string.IsNullOrEmpty(envRoot) && Directory.Exists(envRoot))
        {
            return envRoot;
        }

        // 回退到启发式查找
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            // 多重检测标记，提高鲁棒性
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

    // 架构违规模式（示例中禁止出现）
    // ⚠️ 职责边界：此列表只包含 ADR 明确禁止的核心模式
    // ⚠️ 规则漂移风险：不要无限膨胀此列表，每个 pattern 必须映射到 ADR 条号
    // ⚠️ 规则权威源：ADR 正文是唯一规则源，此处仅为最低可执行子集
    private static readonly string[] ForbiddenPatterns = new[]
    {
        // 跨模块直接引用（ADR-0001.1）
        @"using\s+Zss\.BilliardHall\.Modules\.\w+\.Domain",
        @"using\s+Zss\.BilliardHall\.Modules\.\w+\.Infrastructure",

        // Service 类（ADR-0001.4）
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

    // 限制检查的文件数量以提高性能
    private const int MaxExampleFilesToCheck = 50;

    [Fact(DisplayName = "ADR-0920_1_4: 示例代码不得跨模块直接引用（L1 阻断）")]
    public void Examples_Should_Not_Reference_Other_Modules()
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
                "❌ ADR-0920_1_4 违规：以下示例代码包含架构违规",
                "",
                "根据 ADR-920 决策 3：示例代码不得引入 ADR 未允许的结构或违反架构约束。",
                ""
            }
            .Concat(violations.Take(20)) // 限制输出前20个违规
            .Concat(violations.Count > 20 ? new[] { $"  ... 还有 {violations.Count - 20} 个违规" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 移除跨模块直接引用，使用事件或契约（ADR-0001）",
                "  2. 移除 Service 类，使用垂直切片 Handler（ADR-0001）",
                "  3. 确保 Platform 层不依赖业务层（ADR-0002）",
                "  4. 如果这是错误示例，请明确标记：// ❌ 错误：...",
                "",
                "允许的标记方式：",
                "  ✅ '// ❌ 错误：直接引用其他模块'",
                "  ✅ '// BAD EXAMPLE: cross-module reference'",
                "  ✅ '/* 以下是违规示例，仅用于教学 */'",
                "",
                "参考：docs/adr/governance/ADR-920-examples-governance-constitution.md 决策 3"
            })));
        }
    }

    [Fact(DisplayName = "ADR-0920_1_5: 示例文档必须包含免责声明（L1 阻断）")]
    public void Example_Documents_Must_Have_Disclaimer()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var violations = new List<string>();

        // 收集需要检查的示例文档
        var exampleDocs = new List<string>();

        // 1. examples/ 目录下的 Markdown 文件
        var examplesDir = Path.Combine(repoRoot, "examples");
        if (Directory.Exists(examplesDir))
        {
            exampleDocs.AddRange(
                Directory.GetFiles(examplesDir, "*.md", SearchOption.AllDirectories)
            );
        }

        // 2. docs/examples/ 目录下的 Markdown 文件
        var docsExamplesDir = Path.Combine(repoRoot, "docs", "examples");
        if (Directory.Exists(docsExamplesDir))
        {
            exampleDocs.AddRange(
                Directory.GetFiles(docsExamplesDir, "*.md", SearchOption.AllDirectories)
            );
        }

        // 检测模式
        var disclaimerPatterns = new[]
        {
            @"示例免责声明",
            @"示例代码.*仅用于",
            @"Example.*Disclaimer",
            @"仅用于演示",
            @"不代表.*最佳实践",
            @"具体.*约束.*ADR",
        };

        foreach (var file in exampleDocs.Take(MaxExampleFilesToCheck))
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);

            // 检查前1000个字符（声明应该在开头）
            var header = content.Length > 1000 ? content.Substring(0, 1000) : content;

            var hasDisclaimer = disclaimerPatterns.Any(pattern =>
                Regex.IsMatch(header, pattern, RegexOptions.IgnoreCase));

            if (!hasDisclaimer)
            {
                violations.Add($"  • {relativePath} - 缺少'示例免责声明'");
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(string.Join("\n", new[]
            {
                "❌ ADR-0920_1_5 违规：以下示例文档缺少'示例免责声明'",
                "",
                "根据 ADR-920 决策 2：所有示例文档必须在开头包含免责声明。",
                ""
            }
            .Concat(violations)
            .Concat(new[]
            {
                "",
                "修复建议：在示例文档开头添加以下声明：",
                "",
                "Markdown 格式：",
                "  ```markdown",
                "  ⚠️ **示例免责声明**",
                "  本示例代码仅用于说明用法，不代表架构最佳实践或完整实现。",
                "  具体架构约束以对应 ADR 正文为准。",
                "  ```",
                "",
                "代码文件格式：",
                "  ```csharp",
                "  /// <summary>",
                "  /// ⚠️ 示例代码：仅用于演示用法，不代表完整实现或架构最佳实践。",
                "  /// 具体约束请参考对应 ADR 文档。",
                "  /// </summary>",
                "  ```",
                "",
                "参考：docs/adr/governance/ADR-920-examples-governance-constitution.md 决策 2"
            })));
        }
    }

    // README 中的 C# 代码示例违规检测（L2 警告级别）
    [Fact(DisplayName = "ADR-0920_1_6: README C# 代码块不得引入明显架构违规（L2 警告）")]
    public void README_CSharp_Code_Examples_Should_Not_Violate_Architecture()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var warnings = new List<string>();

        // 收集 README 文件
        var readmeFiles = new List<string>();
        var rootReadme = Path.Combine(repoRoot, "README.md");
        if (File.Exists(rootReadme))
        {
            readmeFiles.Add(rootReadme);
        }

        var docsDir = Path.Combine(repoRoot, "docs");
        if (Directory.Exists(docsDir))
        {
            readmeFiles.AddRange(
                Directory.GetFiles(docsDir, "README.md", SearchOption.AllDirectories)
                    .Take(10)
            );
        }

        foreach (var file in readmeFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);

            // 提取 C# 代码块（仅检查标记为 csharp 的代码块）
            var csharpBlocks = ExtractCSharpCodeBlocks(content);

            int blockIndex = 0;
            foreach (var block in csharpBlocks)
            {
                blockIndex++;

                // 检测明显的架构违规模式（L1 级别）
                foreach (var pattern in ForbiddenPatterns)
                {
                    if (Regex.IsMatch(block, pattern, RegexOptions.IgnoreCase))
                    {
                        // 检查是否有错误标记或 ADR 正文标记
                        var hasErrorMarker = Regex.IsMatch(block, @"//\s*❌", RegexOptions.IgnoreCase);
                        var hasAdrMarker = Regex.IsMatch(block, @"//\s*(反例|结构示意)", RegexOptions.IgnoreCase);

                        if (!hasErrorMarker && !hasAdrMarker)
                        {
                            warnings.Add($"  ⚠️ {relativePath} - C# 代码块 #{blockIndex}");
                            var preview = block.Length > 100 ? block.Substring(0, 100) + "..." : block;
                            warnings.Add($"      预览: {preview.Replace("\n", " ").Trim()}");
                        }
                    }
                }
            }
        }

        // L2 级别：警告但不失败构建
        // ⚠️ 关键修复：L2 警告必须实际输出，否则"无牙老虎"
        if (warnings.Any())
        {
            var warningMessage = string.Join("\n", new[]
            {
                "⚠️ ADR-920.6 警告（L2）：以下 README C# 代码块可能包含架构违规",
                "",
                "根据 ADR-920 决策 3 和决策 5：README 中的 C# 代码块也不应违反架构约束。",
                "非 C# 代码块（bash、pseudo-code）不受此检测。",
                ""
            }
            .Concat(warnings.Take(15))
            .Concat(new[]
            {
                "",
                "建议：",
                "  1. 如果是正确示例，请移除违规代码",
                "  2. 如果是错误示例，请明确标记：// ❌ 错误：...",
                "  3. 如果是 ADR 正文的教学片段，请标记：// 反例（禁止）或 // 结构示意",
                "  4. 确保示例代码符合 ADR-0001、ADR-0002、ADR-0005 等架构约束",
                "",
                "注意：这是 L2 警告级别，不会阻断构建。"
            }));

            // 实际输出警告（Console + 测试名称）
            Console.WriteLine(warningMessage);
            Console.WriteLine(); // 空行分隔
        }

        // L2 警告：总是通过（但已输出警告信息）
        // 不需要断言 - 警告已通过 Console 输出
    }

    [Fact(DisplayName = "ADR-0920_1_7: 示例目录必须有责任人和目的说明（L1 阻断）")]
    public void Example_Directories_Must_Have_Owner_And_Purpose()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var violations = new List<string>();

        // 扫描 examples/ 目录
        var examplesDir = Path.Combine(repoRoot, "examples");
        if (!Directory.Exists(examplesDir))
        {
            // 如果 examples 目录不存在，测试通过
            return;
        }

        // 获取所有子目录
        var subDirs = Directory.GetDirectories(examplesDir, "*", SearchOption.TopDirectoryOnly);

        // 必填字段模式
        var authorPattern = @"(\*\*作者\*\*|Author)[:：]\s*@?\w+";
        var purposePattern = @"(\*\*目的\*\*|Purpose)[:：]\s*\w+"; // 放宽匹配，允许任何文字
        var createdPattern = @"(\*\*创建日期\*\*|Created)[:：]\s*\d{4}-\d{2}-\d{2}";
        var adrsPattern = @"(\*\*适用\s*ADR\*\*|ADRs?)[:：]";

        foreach (var dir in subDirs)
        {
            var dirName = Path.GetFileName(dir);
            var readmePath = Path.Combine(dir, "README.md");
            var relativePath = Path.GetRelativePath(repoRoot, dir);

            if (!File.Exists(readmePath))
            {
                violations.Add($"  • {relativePath}/ - 缺少 README.md");
                continue;
            }

            var content = File.ReadAllText(readmePath);

            // 检查必填字段
            var missingFields = new List<string>();

            if (!Regex.IsMatch(content, authorPattern, RegexOptions.IgnoreCase))
            {
                missingFields.Add("Author");
            }

            if (!Regex.IsMatch(content, purposePattern, RegexOptions.IgnoreCase))
            {
                missingFields.Add("Purpose");
            }

            if (!Regex.IsMatch(content, createdPattern, RegexOptions.IgnoreCase))
            {
                missingFields.Add("Created");
            }

            if (!Regex.IsMatch(content, adrsPattern, RegexOptions.IgnoreCase))
            {
                missingFields.Add("ADRs");
            }

            if (missingFields.Any())
            {
                violations.Add($"  • {relativePath}/ - 缺少字段: {string.Join(", ", missingFields)}");
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(string.Join("\n", new[]
            {
                "❌ ADR-0920_1_7 违规（L1）：以下示例目录缺少必需的维护信息",
                "",
                "根据 ADR-920 决策 6：每个示例目录必须有明确的责任人和目的说明。",
                ""
            }
            .Concat(violations)
            .Concat(new[]
            {
                "",
                "修复建议：在示例目录的 README.md 中添加以下信息：",
                "",
                "```markdown",
                "# 示例名称",
                "",
                "⚠️ **示例免责声明**",
                "本示例代码仅用于说明用法，不代表架构最佳实践或完整实现。",
                "",
                "**维护信息**：",
                "- **作者**：@username",
                "- **目的**：教学 / 演示 / Onboarding",
                "- **创建日期**：YYYY-MM-DD",
                "- **适用 ADR**：ADR-0001, ADR-0005",
                "```",
                "",
                "核心原则：没有责任人 = 没人维护 = 示例腐化",
                "",
                "参考：docs/adr/governance/ADR-920-examples-governance-constitution.md 决策 6"
            })));
        }
    }

    // ========== 辅助方法 ==========

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

    private static List<string> ExtractCSharpCodeBlocks(string markdown)
    {
        var blocks = new List<string>();
        // 只提取明确标记为 csharp 的代码块
        // 使用 [\r\n]+ 处理不同操作系统的换行符
        var pattern = @"```csharp[\r\n]+([\s\S]*?)```";
        var matches = Regex.Matches(markdown, pattern);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                blocks.Add(match.Groups[1].Value);
            }
        }

        return blocks;
    }
}
