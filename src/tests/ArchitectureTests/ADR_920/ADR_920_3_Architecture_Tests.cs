namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_920;

/// <summary>
/// ADR-920_3: 示例类型边界管理（Rule）
/// 验证示例、测试、PoC的边界清晰，存放位置正确
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-920_3_1: 示例 vs 测试 vs PoC
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-920-examples-governance-constitution.md
/// </summary>
public sealed class ADR_920_3_Architecture_Tests
{
    /// <summary>
    /// ADR-920_3_1: 示例 vs 测试 vs PoC
    /// 验证示例目录必须有责任人和目的说明（§ADR-920_3_1）
    /// </summary>
    [Fact(DisplayName = "ADR-920_3_1: 示例目录必须有责任人和目的说明")]
    public void ADR_920_3_1_Example_Directories_Must_Have_Owner_And_Purpose()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
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
        var purposePattern = @"(\*\*目的\*\*|Purpose)[:：]\s*\w+";
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

            var content = FileSystemTestHelper.ReadFileContent(readmePath);

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

        violations.Should().BeEmpty(string.Join("\n", new[]
            {
                "❌ ADR-920_3_1 违规：以下示例目录缺少必需的维护信息",
                "",
                "根据 ADR-920_3_1：示例 vs 测试 vs PoC。",
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
                "- **适用 ADR**：ADR-001, ADR-005",
                "```",
                "",
                "核心原则：没有责任人 = 没人维护 = 示例腐化",
                "",
                "参考：docs/adr/governance/ADR-920-examples-governance-constitution.md §ADR-920_3_1"
            })));
    }

}
