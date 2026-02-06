namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_952;

/// <summary>
/// ADR-952_1: 层级定义与权威关系（Rule）
/// 验证 ADR、Standard、Best Practice 的三层架构和权威关系
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-952_1_1: 三层架构定义
/// - ADR-952_1_2: 层级定义表格
/// - ADR-952_1_3: 核心原则
/// - ADR-952_1_4: 判定
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-952-engineering-standard-adr-boundary.md
/// </summary>
public sealed class ADR_952_1_Architecture_Tests
{
    /// <summary>
    /// ADR-952_1_1: 三层架构定义
    /// 验证工程标准文档符合 L1 ADR、L2 Standard、L3 Best Practice 的层级定义（§ADR-952_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-952_1_1: 工程标准必须符合三层架构定义")]
    public void ADR_952_1_1_Engineering_Standards_Must_Follow_Three_Layer_Architecture()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var standardsDirectory = Path.Combine(repoRoot, "docs/engineering-standards");

        if (!Directory.Exists(standardsDirectory))
        {
            Console.WriteLine("⚠️ ADR-952_1_1 提示：docs/engineering-standards/ 目录尚未创建，跳过测试。");
            return;
        }

        var standardFiles = Directory.GetFiles(standardsDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (standardFiles.Count == 0)
        {
            Console.WriteLine("⚠️ ADR-952_1_1 提示：暂无工程标准文档，跳过测试。");
            return;
        }

        var violations = new List<string>();

        foreach (var standardFile in standardFiles)
        {
            var fileName = Path.GetFileName(standardFile);
            var content = File.ReadAllText(standardFile);

            // Standard 文档不应使用"必须"、"禁止"等强制性语言引入新约束
            // 除非明确基于某个 ADR
            var hasAdrReference = Regex.IsMatch(content, @"基于\s*ADR|Based\s*on\s*ADR", RegexOptions.IgnoreCase);

            if (!hasAdrReference)
            {
                // 检查是否使用了强制性语言
                var hasMandatoryLanguage = Regex.IsMatch(content,
                    @"(必须|禁止|不得|强制|Mandatory|Must|Shall|Forbidden)",
                    RegexOptions.IgnoreCase);

                if (hasMandatoryLanguage)
                {
                    violations.Add($"{fileName}: 使用强制性语言但未明确基于 ADR");
                }
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-952_1_1：以下工程标准文档违反三层架构定义：\n{string.Join("\n", violations)}");
    }

}
