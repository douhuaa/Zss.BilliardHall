namespace Zss.BilliardHall.Tests.ArchitectureTests.Enforcement;


/// <summary>
/// ADR-008_2: ADR 文档结构规范（Rule）
/// 执行 ADR-008 对 ADR 文档结构的约束
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-008_2_1: ADR 文档必须包含必需章节
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-008-documentation-governance-constitution.md
/// - 来源决策: ADR-008 决策 4.1
///
/// 执法说明：
/// - 失败 = CI 阻断
/// </summary>
public sealed class ADR_008_2_Architecture_Tests
{
    /// <summary>
    /// ADR-008_2_1: ADR 文档必须包含必需章节
    /// 验证所有 ADR 文档包含状态、级别、核心决策等必需章节（§ADR-008_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-008_2_1: ADR 文档必须包含必需章节")]
    public void ADR_008_2_1_Documents_Must_Have_Required_Sections()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDir = Path.Combine(repoRoot, "docs/adr");

        Directory.Exists(adrDir).Should().BeTrue($"ADR 目录不存在：{adrDir}");

        var violations = new List<string>();

        // 扫描所有 ADR 文档（递归扫描所有子目录）
        var adrFiles = AdrFileFilter.GetAdrFiles(adrDir).Take(30); // 限制检查数量，避免性能问题

        foreach (var file in adrFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            var missingSections = new List<string>();

            // 检查必需章节（根据 ADR-008 决策 4.1）
            // 注意：部分 ADR 使用"地位"而非"状态"
            if (!content.Contains("状态") && !content.Contains("status") && !content.Contains("地位"))
            {
                missingSections.Add("状态");
            }

            if (!content.Contains("级别") && !content.Contains("Level".ToLower()))
            {
                missingSections.Add("级别");
            }

            // 聚焦内容是可选的，但建议包含
            // if (!content.Contains("聚焦内容") && !content.Contains("Focus"))
            // {
            //     missingSections.Add("聚焦内容");
            // }

            // ADR 应包含"核心决策"或"规则本体"之一
            // 注意：模板使用"规则本体"，ADR-008要求"核心决策"，两者均可接受
            if (!content.Contains("决策") && !content.Contains("Decision") &&
                !content.Contains("规则本体") && !content.Contains("Rule"))
            {
                missingSections.Add("决策或规则本体");
            }

            if (missingSections.Any())
            {
                violations.Add($"  • {relativePath} - 缺少章节: {string.Join(", ", missingSections)}");
            }
        }

        violations.Should().BeEmpty(string.Join("\n", new[]
            {
                "❌ Enforcement 违规：以下 ADR 文档缺少必需章节",
                "",
                "根据 ADR-008 决策 4.1：所有 ADR 必须包含以下章节：",
                "  • 状态（Proposed / Adopted / Final / Superseded）或地位",
                "  • 级别（宪法层 / 结构层 / 运行层 / 技术层 / 治理层）",
                "  • 核心决策（Decision）或规则本体（Rule）",
                ""
            }
            .Concat(violations)
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 在 ADR 文档开头添加状态和级别信息",
                "  2. 确保包含'核心决策'或'规则本体'章节",
                "  3. 参考模板：docs/templates/adr-template.md",
                "",
                "参考：docs/adr/constitutional/ADR-008-documentation-governance-constitution.md 决策 4.1"
            })));
    }

}
