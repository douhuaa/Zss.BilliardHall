namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;


/// <summary>
/// ADR-008_6: 文档风格启发式规范（Rule）
/// 文档风格品味建议，非强制规则
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-008_6_1: README 建议使用描述性语言
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-008-documentation-governance-constitution.md
///
/// 定位说明：
/// - 品味建议，非强制规则
/// - 不失败构建，仅输出警告
/// - 这些测试永远不应该 Fail，只输出建议
/// </summary>
public sealed class ADR_008_6_Architecture_Tests
{
    private readonly ITestOutputHelper _output;

    public ADR_008_6_Architecture_Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// ADR-008_6_1: README 建议使用描述性语言
    /// 启发式建议：使用描述性语言而非命令性语言（§ADR-008_6_1）
    /// </summary>
    [Fact(DisplayName = "ADR-008_6_1: README 建议使用描述性语言")]
    public void ADR_008_6_1_README_Should_Prefer_Descriptive_Language()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        if (repoRoot == null) return;

        var suggestions = new List<string>();

        // 命令式语气词汇（建议改为描述性）
        var imperativePhrases = new[]
        {
            "请注意", "务必", "一定要", "千万不要", "请确保"
        };

        // 扫描 README 文件
        var docsDir = Path.Combine(repoRoot, "docs");
        if (!Directory.Exists(docsDir)) return;

        var readmeFiles = Directory.GetFiles(docsDir, "README.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("/adr/", StringComparison.OrdinalIgnoreCase))
            .Take(10);

        foreach (var file in readmeFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);

            foreach (var phrase in imperativePhrases)
            {
                if (content.Contains(phrase))
                {
                    suggestions.Add($"  💡 {relativePath} - 建议将 '{phrase}' 改为描述性表达");
                }
            }
        }

        if (suggestions.Any())
        {
            _output.WriteLine("⚠️ Heuristics 建议：以下 README 可改进语言风格");
            _output.WriteLine("");
            _output.WriteLine("建议使用描述性语言而非命令性语言，提升文档的友好度。");
            _output.WriteLine("");
            foreach (var suggestion in suggestions.Take(5))
            {
                _output.WriteLine(suggestion);
            }
            _output.WriteLine("");
            _output.WriteLine("注意：这是建议，不是强制规则。");
        }

        // ✅ 永远通过 - Heuristics 不应该失败构建
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "Heuristics: ADR 建议包含示例")]
    public void ADR_Should_Include_Examples()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        if (repoRoot == null) return;

        var suggestions = new List<string>();

        // 扫描 ADR 文档
        var adrDir = Path.Combine(repoRoot, "docs/adr");
        if (!Directory.Exists(adrDir)) return;

        var adrFiles = AdrFileFilter.GetAdrFiles(adrDir).Take(15);

        foreach (var file in adrFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);

            // 检查是否包含代码示例
            var hasCodeExample = Regex.IsMatch(content, @"```[\s\S]*?```");
            var hasCheckMark = content.Contains("✅") || content.Contains("❌");

            if (!hasCodeExample && !hasCheckMark)
            {
                suggestions.Add($"  💡 {relativePath} - 建议添加代码示例或对比标记（✅/❌）");
            }
        }

        if (suggestions.Any())
        {
            _output.WriteLine("⚠️ Heuristics 建议：以下 ADR 可增加示例");
            _output.WriteLine("");
            _output.WriteLine("建议在 ADR 中添加代码示例，帮助开发者理解规则。");
            _output.WriteLine("");
            foreach (var suggestion in suggestions.Take(5))
            {
                _output.WriteLine(suggestion);
            }
            if (suggestions.Count > 5)
            {
                _output.WriteLine($"  ... 还有 {suggestions.Count - 5} 个建议");
            }
            _output.WriteLine("");
            _output.WriteLine("注意：这是建议，不是强制规则。");
        }

        // ✅ 永远通过 - Heuristics 不应该失败构建
        true.Should().BeTrue();
    }

    [Fact(DisplayName = "Heuristics: 文档建议保持简洁")]
    public void Documents_Should_Be_Concise()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        if (repoRoot == null) return;

        var suggestions = new List<string>();

        // 扫描所有文档
        var docsDir = Path.Combine(repoRoot, "docs");
        if (!Directory.Exists(docsDir)) return;

        var mdFiles = Directory.GetFiles(docsDir, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("/templates/", StringComparison.OrdinalIgnoreCase))
            .Take(20);

        foreach (var file in mdFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);

            var lineCount = content.Split('\n').Length;

            // ADR 建议不超过 500 行
            if (file.Contains("/adr/", StringComparison.OrdinalIgnoreCase) && lineCount > 500)
            {
                suggestions.Add($"  💡 {relativePath} - ADR 较长 ({lineCount} 行)，建议拆分为多个 ADR");
            }

            // README 建议不超过 300 行
            if (Path.GetFileName(file).Equals("README.md", StringComparison.OrdinalIgnoreCase) && lineCount > 300)
            {
                suggestions.Add($"  💡 {relativePath} - README 较长 ({lineCount} 行)，建议精简或拆分");
            }
        }

        if (suggestions.Any())
        {
            _output.WriteLine("⚠️ Heuristics 建议：以下文档可考虑精简");
            _output.WriteLine("");
            _output.WriteLine("过长的文档可能影响可读性，建议拆分或精简。");
            _output.WriteLine("");
            foreach (var suggestion in suggestions.Take(5))
            {
                _output.WriteLine(suggestion);
            }
            _output.WriteLine("");
            _output.WriteLine("注意：这是建议，不是强制规则。");
        }

        // ✅ 永远通过 - Heuristics 不应该失败构建
        true.Should().BeTrue();
    }

}
