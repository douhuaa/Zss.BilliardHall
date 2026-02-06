namespace Zss.BilliardHall.Tests.ArchitectureTests.Enforcement;

/// <summary>
/// 架构测试：强制使用 AdrFileFilter 而不是 Directory.GetFiles 查找 ADR 文件
/// 
/// 目的：防止开发者疏忽地使用 Directory.GetFiles 直接查找 ADR Markdown 文件
/// 这会导致过滤逻辑不一致，可能包含 TEMPLATE、CHECKLIST、guide 等非 ADR 文件
/// 
/// 相关：
/// - AdrFileFilter.cs：统一的 ADR 文件过滤器
/// - 问题修复：解决 Directory.GetFiles 与 AdrFileFilter 混用问题
/// </summary>
public sealed class ADR_File_Filter_Usage_Tests
{
    // 允许使用 Directory.GetFiles 的文件（白名单）
    private static readonly string[] AllowedFiles = new[]
    {
        "AdrFileFilter.cs",           // AdrFileFilter 内部实现
        "Program.cs",                 // AdrParserCli 独立工具  
        "ADR_File_Filter_Usage_Tests.cs", // 本测试文件自身（需要扫描测试文件）
        "ADR_907_2_Architecture_Tests.cs",  // 查找测试文件 (.cs)
        "ADR_907_3_Architecture_Tests.cs",  // 查找测试文件 (.cs)
        "ADR_907_4_Architecture_Tests.cs",  // 查找测试文件 (.cs)
        "ADR_950_1_Architecture_Tests.cs",  // 查找 guide 文档
        "ADR_950_2_Architecture_Tests.cs",  // 查找 guide 文档
        "ADR_951_2_Architecture_Tests.cs",  // 查找案例文档（cases/*.md）
        "ADR_951_3_Architecture_Tests.cs",  // 查找案例文档
        "ADR_951_4_Architecture_Tests.cs",  // 查找案例文档
    };

    /// <summary>
    /// 验证：测试代码不得直接使用 Directory.GetFiles 查找 ADR Markdown 文件
    /// 
    /// 规则：
    /// 1. 禁止使用 Directory.GetFiles(..., "*.md", ...) 查找 ADR 文件
    /// 2. 禁止使用 Directory.GetFiles(..., "ADR-*.md", ...) 
    /// 3. 必须使用 AdrFileFilter.GetAdrFiles() 替代
    /// 
    /// 例外：
    /// - AdrFileFilter.cs 本身（实现者）
    /// - AdrParserCli/Program.cs（独立 CLI 工具）
    /// - 查找测试文件 (.cs) 的场景
    /// </summary>
    [Fact(DisplayName = "测试代码必须使用 AdrFileFilter 查找 ADR 文件")]
    public void Test_Code_Must_Use_AdrFileFilter_For_ADR_Files()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, "src/tests/ArchitectureTests");

        if (!Directory.Exists(testsDirectory))
        {
            throw new DirectoryNotFoundException($"测试目录不存在: {testsDirectory}");
        }

        var violations = new List<string>();

        // 扫描所有测试文件
        var testFiles = Directory.GetFiles(testsDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => !AllowedFiles.Any(allowed => f.EndsWith(allowed, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetRelativePath(repoRoot, testFile);

            // 检测模式：Directory.GetFiles 查找 ADR 文档 (adr 目录下的 .md 文件)
            var problematicPatterns = new[]
            {
                // Directory.GetFiles(adrDirectory, "*.md", ...)  
                // 只检测变量名包含 "adr" 的情况
                (@"Directory\.GetFiles\s*\(\s*[^,]*adr[^,]*,\s*""\*\.md""", "在 adr 相关目录使用 \"*.md\" 模式"),
                
                // Directory.GetFiles(..., "ADR-*.md", ...)
                (@"Directory\.GetFiles\s*\([^)]*""ADR-[^""]*\.md""", "使用 \"ADR-*.md\" 模式"),
            };

            foreach (var (pattern, description) in problematicPatterns)
            {
                var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    var lines = content.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (Regex.IsMatch(lines[i], pattern, RegexOptions.IgnoreCase))
                        {
                            violations.Add($"  • {fileName}:{i + 1}");
                            violations.Add($"    问题：{description}");
                            violations.Add($"    代码：{lines[i].Trim()}");
                            break; // 每个文件只报告一次
                        }
                    }
                }
            }
        }

        violations.Should().BeEmpty(string.Join("\n", new[]
        {
            "❌ 架构违规：以下测试文件直接使用 Directory.GetFiles 查找 ADR 文件",
            "",
            "这会导致：",
            "  1. 过滤逻辑不一致",
            "  2. 可能包含 TEMPLATE、CHECKLIST、guide、proposal 等非 ADR 文件",
            "  3. 无法利用 YAML Front Matter 精确识别文档类型",
            "",
            "违规位置：",
        }
        .Concat(violations)
        .Concat(new[]
        {
            "",
            "修复建议：",
            "  1. 使用 AdrFileFilter.GetAdrFiles(directory) 替代 Directory.GetFiles",
            "  2. AdrFileFilter 类已通过 GlobalUsings 全局可用",
            "",
            "示例：",
            "  // ❌ 错误",
            "  var files = Directory.GetFiles(adrDir, \"*.md\", SearchOption.AllDirectories);",
            "",
            "  // ✅ 正确",
            "  var files = AdrFileFilter.GetAdrFiles(adrDir);",
            "",
            "参考：",
            "  - AdrFileFilter.cs：统一的 ADR 文件过滤器",
            "  - FileSystemTestHelper.cs：已更新使用 AdrFileFilter",
        })));
    }

    /// <summary>
    /// 验证：AdrFileFilter.GetAdrFiles 在测试代码中被广泛使用
    /// 
    /// 目的：确保新代码遵循最佳实践
    /// </summary>
    [Fact(DisplayName = "AdrFileFilter 应该在测试代码中被使用")]
    public void AdrFileFilter_Should_Be_Used_In_Tests()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, "src/tests/ArchitectureTests");

        if (!Directory.Exists(testsDirectory))
        {
            throw new DirectoryNotFoundException($"测试目录不存在: {testsDirectory}");
        }

        // 统计使用 AdrFileFilter.GetAdrFiles 的文件数量
        var testFiles = Directory.GetFiles(testsDirectory, "*.cs", SearchOption.AllDirectories);
        var filesUsingAdrFilter = testFiles
            .Where(f => File.ReadAllText(f).Contains("AdrFileFilter.GetAdrFiles"))
            .Count();

        // 至少应该有几个文件使用了 AdrFileFilter
        filesUsingAdrFilter.Should().BeGreaterThanOrEqualTo(3, 
            "应该有多个测试文件使用 AdrFileFilter.GetAdrFiles，" +
            "这表明团队正在遵循统一的 ADR 文件查找最佳实践");

        Console.WriteLine($"✅ 检测到 {filesUsingAdrFilter} 个测试文件正在使用 AdrFileFilter.GetAdrFiles");
    }
}
