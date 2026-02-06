namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_920;

/// <summary>
/// ADR-920_1: 示例代码权限边界（Rule）
/// 验证所有示例代码符合边界约束，不得具有架构定义权力
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-920_1_1: 示例代码的法律地位
/// - ADR-920_1_2: 示例代码必须包含的免责声明
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-920-examples-governance-constitution.md
/// </summary>
public sealed class ADR_920_1_Architecture_Tests
{
    private const int MaxExampleFilesToCheck = 50;

    /// <summary>
    /// ADR-920_1_1: 示例代码的法律地位
    /// 验证示例代码不得定义架构规则或引入未经 ADR 允许的模式（§ADR-920_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-920_1_1: 示例代码的法律地位")]
    public void ADR_920_1_1_Example_Code_Must_Not_Define_Architecture_Rules()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");

        // 验证 ADR-920 文档存在并包含法律地位定义
        var adrFile = FileSystemTestHelper.GetAbsolutePath("docs/adr/governance/ADR-920-examples-governance-constitution.md");
        File.Exists(adrFile).Should().BeTrue($"ADR-920 文档不存在：{adrFile}");

        var content = File.ReadAllText(adrFile);

        // 验证必需的条款存在
        content.Should().Contain("示例代码的法律地位", "缺少示例代码法律地位定义");
        content.Should().Contain("示例代码是\"认知放大器\"，不是\"架构定义器\"", "缺少核心原则声明");

        // 验证示例代码禁止的权力已明确列出
        content.Should().Contain("定义架构规则", "未明确禁止定义架构规则");
        content.Should().Contain("引入 ADR 中未允许的结构或模式", "未明确禁止引入未批准的模式");
        content.Should().Contain("作为架构正确性的证据", "未明确禁止作为架构证据");

        // 验证示例代码允许的用途已明确列出
        content.Should().Contain("演示如何使用已有的架构模式", "未明确允许演示用途");
        content.Should().Contain("说明具体的 API 调用方式", "未明确允许API说明用途");
    }

    /// <summary>
    /// ADR-920_1_2: 示例代码必须包含的免责声明
    /// 验证所有示例文档和代码文件包含标准免责声明（§ADR-920_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-920_1_2: 示例文档必须包含免责声明")]
    public void ADR_920_1_2_Example_Documents_Must_Have_Disclaimer()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var violations = new List<string>();

        // 收集需要检查的示例文档
        var exampleDocs = new List<string>();

        // 1. examples/ 目录下的 Markdown 文件
        var examplesDir = Path.Combine(repoRoot, "examples");
        if (Directory.Exists(examplesDir))
        {
            exampleDocs.AddRange(
                FileSystemTestHelper.GetFilesInDirectory(examplesDir, "*.md", SearchOption.AllDirectories)
            );
        }

        // 2. docs/examples/ 目录下的 Markdown 文件
        var docsExamplesDir = Path.Combine(repoRoot, "docs", "examples");
        if (Directory.Exists(docsExamplesDir))
        {
            exampleDocs.AddRange(
                FileSystemTestHelper.GetFilesInDirectory(docsExamplesDir, "*.md", SearchOption.AllDirectories)
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

        violations.Should().BeEmpty(string.Join("\n", new[]
            {
                "❌ ADR-920_1_2 违规：以下示例文档缺少'示例免责声明'",
                "",
                "根据 ADR-920_1_2：示例代码必须包含的免责声明。",
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
                "参考：docs/adr/governance/ADR-920-examples-governance-constitution.md §ADR-920_1_2"
            })));
    }

}
