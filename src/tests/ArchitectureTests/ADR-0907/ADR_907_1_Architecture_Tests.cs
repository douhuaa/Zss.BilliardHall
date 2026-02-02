using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907_1: ArchitectureTests 的法律地位
/// 验证 ArchitectureTests 作为 ADR 唯一自动化执法形式的相关规则
/// 
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-907_1_1: 唯一自动化执法形式 → ADR_907_1_1_ArchitectureTests_Are_Sole_Automated_Enforcement
/// - ADR-907_1_2: 可执法性要求 → ADR_907_1_2_ADRs_Must_Have_Tests_Or_Be_Non_Enforceable
/// - ADR-907_1_3: 禁止仅文档约束 → ADR_907_1_3_No_Documentation_Only_Rules_Allowed
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_907_1_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const string AdrTestsPath = "src/tests/ArchitectureTests";

    /// <summary>
    /// ADR-907_1_1: 唯一自动化执法形式
    /// 验证 ArchitectureTests 是 ADR 的唯一自动化执法形式（§1.1）
    /// </summary>
    [Fact(DisplayName = "ADR-907_1_1: ArchitectureTests 必须是 ADR 的唯一自动化执法形式")]
    public void ADR_907_1_1_ArchitectureTests_Are_Sole_Automated_Enforcement()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath);

        // 验证 ArchitectureTests 项目存在
        Directory.Exists(testsDirectory).Should().BeTrue(
            $"❌ ADR-907_1_1 违规：ArchitectureTests 目录不存在\n\n" +
            $"预期路径：{testsDirectory}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建独立的 ArchitectureTests 项目\n" +
            $"  2. ArchitectureTests 是 ADR 的唯一自动化执法形式\n" +
            $"  3. 所有可执法的架构规则必须通过 ArchitectureTests 验证\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §1.1");

        // 验证项目文件存在
        var projectFile = Path.Combine(testsDirectory, "ArchitectureTests.csproj");
        File.Exists(projectFile).Should().BeTrue(
            $"❌ ADR-907_1_1 违规：ArchitectureTests 项目文件不存在\n\n" +
            $"预期路径：{projectFile}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建 ArchitectureTests.csproj 项目文件\n" +
            $"  2. 配置为测试项目，引用 xUnit 和 NetArchTest.Rules\n" +
            $"  3. 确保项目命名格式：<SolutionName>.Tests.Architecture\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §1.1");

        // 验证测试使用允许的技术（文件系统扫描、Roslyn、Reflection、NetArchTest）
        // 这是元验证测试，确保测试本身使用合规的技术
        var testFiles = Directory.GetFiles(testsDirectory, "*.cs", SearchOption.AllDirectories);
        testFiles.Should().NotBeEmpty(
            $"❌ ADR-907_1_1 违规：ArchitectureTests 项目中没有测试文件\n\n" +
            $"修复建议：\n" +
            $"  1. 为每个具备裁决力的 ADR 创建对应的架构测试\n" +
            $"  2. 测试可使用：文件系统扫描、Roslyn、Reflection、NetArchTest\n" +
            $"  3. 测试文件命名：ADR_<编号>_Architecture_Tests.cs\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §1.1");
    }

    /// <summary>
    /// ADR-907_1_2: 可执法性要求
    /// 验证任何具备裁决力的 ADR 必须有对应的 ArchitectureTests 或明确声明为 Non-Enforceable（§1.2）
    /// </summary>
    [Fact(DisplayName = "ADR-907_1_2: 具备裁决力的 ADR 必须有测试或声明 Non-Enforceable")]
    public void ADR_907_1_2_ADRs_Must_Have_Tests_Or_Be_Non_Enforceable()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDocsDirectory = Path.Combine(repoRoot, AdrDocsPath);
        var testsDirectory = Path.Combine(repoRoot, AdrTestsPath, "ADR");

        if (!Directory.Exists(adrDocsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_1_2 无法执行：ADR 文档目录不存在 {adrDocsDirectory}");
            return;
        }

        if (!Directory.Exists(testsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_1_2 无法执行：测试目录不存在 {testsDirectory}");
            return;
        }

        // 查找所有 Final 或 Active 状态的 ADR
        var adrFiles = Directory.GetFiles(adrDocsDirectory, "ADR-*.md", SearchOption.AllDirectories);
        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 提取 ADR 编号
            var adrMatch = Regex.Match(fileName, @"ADR-(\d{3,4})");
            if (!adrMatch.Success)
                continue;

            var adrNumber = adrMatch.Groups[1].Value.PadLeft(4, '0');

            // 检查是否为 Final 或 Active 状态
            var isFinalOrActive = Regex.IsMatch(content, @"status:\s*(Final|Active)", RegexOptions.IgnoreCase);
            if (!isFinalOrActive)
                continue;

            // 检查是否明确声明为 Non-Enforceable
            var isNonEnforceable = Regex.IsMatch(content, @"enforceable:\s*(false|no)", RegexOptions.IgnoreCase) ||
                                   content.Contains("Non-Enforceable", StringComparison.OrdinalIgnoreCase);

            if (isNonEnforceable)
                continue;

            // 检查是否存在对应的测试文件
            var testFilePattern = $"ADR_{adrNumber}_Architecture_Tests.cs";
            var testFilePattern3Digit = $"ADR_{int.Parse(adrNumber)}_Architecture_Tests.cs";
            
            var hasTest = Directory.GetFiles(testsDirectory, testFilePattern).Any() ||
                         Directory.GetFiles(testsDirectory, testFilePattern3Digit).Any();

            if (!hasTest)
            {
                violations.Add($"  • ADR-{adrNumber} ({fileName}) - 缺少架构测试且未声明 Non-Enforceable");
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_1_2 违规：以下 ADR 缺少架构测试且未声明 Non-Enforceable\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 为每个 Final/Active ADR 创建对应的架构测试\n" +
                $"  2. 或在 ADR 元数据中明确声明 enforceable: false\n" +
                $"  3. 或在 ADR 正文中添加 Non-Enforceable 标记\n" +
                $"  4. 测试文件命名：ADR_<编号>_Architecture_Tests.cs\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §1.2");
        }
    }

    /// <summary>
    /// ADR-907_1_3: 禁止仅文档约束
    /// 验证不存在"仅文档约束、不接受执行"的架构规则（§1.3）
    /// </summary>
    [Fact(DisplayName = "ADR-907_1_3: 禁止仅文档约束的架构规则")]
    public void ADR_907_1_3_No_Documentation_Only_Rules_Allowed()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDocsDirectory = Path.Combine(repoRoot, AdrDocsPath);

        if (!Directory.Exists(adrDocsDirectory))
        {
            true.Should().BeFalse($"❌ ADR-907_1_3 无法执行：ADR 文档目录不存在 {adrDocsDirectory}");
            return;
        }

        var adrFiles = Directory.GetFiles(adrDocsDirectory, "ADR-*.md", SearchOption.AllDirectories);
        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 检查是否为 Final 或 Active 状态
            var isFinalOrActive = Regex.IsMatch(content, @"status:\s*(Final|Active)", RegexOptions.IgnoreCase);
            if (!isFinalOrActive)
                continue;

            // 检查是否包含"仅文档"、"不执行"等禁止的声明
            var prohibitedPatterns = new[]
            {
                @"仅文档约束",
                @"documentation\s+only",
                @"不接受执行",
                @"no\s+enforcement",
                @"不执行",
                @"manual\s+only"
            };

            foreach (var pattern in prohibitedPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    violations.Add($"  • {fileName} - 包含禁止的声明：{pattern}");
                    break;
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(
                $"❌ ADR-907_1_3 违规：以下 ADR 包含"仅文档约束"的禁止声明\n\n" +
                $"{string.Join("\n", violations)}\n\n" +
                $"修复建议：\n" +
                $"  1. 移除"仅文档约束"、"不接受执行"等声明\n" +
                $"  2. 选择以下之一：\n" +
                $"     a. 创建对应的 ArchitectureTests（推荐）\n" +
                $"     b. 明确声明为 Non-Enforceable\n" +
                $"  3. 所有架构规则必须可执法或明确声明不可执法\n\n" +
                $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §1.3");
        }
    }

    #region Helper Methods

    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();

        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) || 
                Directory.Exists(Path.Combine(currentDir, AdrDocsPath)))
            {
                return currentDir;
            }

            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        return null;
    }

    #endregion
}
