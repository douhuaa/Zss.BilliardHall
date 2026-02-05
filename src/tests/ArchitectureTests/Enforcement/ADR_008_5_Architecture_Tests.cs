namespace Zss.BilliardHall.Tests.ArchitectureTests.Enforcement;

/// <summary>
/// ADR-008_5: Skills 判断性语言规范（Rule）
/// 执行 ADR-008 对 Skills 输出语言的约束
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-008_5_1: Skills 不得输出判断性结论
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-008-documentation-governance-constitution.md
/// - 来源决策: ADR-008 决策 3.2
///
/// 执法说明：
/// - 失败 = CI 阻断
/// - Skills 只能输出事实，不得输出判断
/// </summary>
public sealed class ADR_008_5_Architecture_Tests
{
    /// <summary>
    /// ADR-008_5_1: Skills 不得输出判断性结论
    /// 验证 Skills 只输出事实，不输出判断性结论（§ADR-008_5_1）
    /// </summary>
    [Fact(DisplayName = "ADR-008_5_1: Skills 不得输出判断性结论")]
    public void ADR_008_5_1_Skills_Must_Not_Output_Judgments()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var skillsDir = Path.Combine(repoRoot, ".github/skills");

        if (!Directory.Exists(skillsDir))
        {
            // Skills 目录可选，如果不存在则测试通过
            return;
        }

        // 判断性词汇（ADR-008 明确禁止 Skills 使用）
        var forbiddenJudgments = new[]
        {
            "违规", "不符合", "应当", "建议", "推荐",
            "正确", "错误", "合规", "不合规", "必须修复"
        };

        var violations = new List<string>();
        var skillFiles = Directory.GetFiles(skillsDir, "*.skill.md", SearchOption.AllDirectories);

        foreach (var file in skillFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);

            // 移除 frontmatter 和代码块
            var contentWithoutFrontmatter = RemoveFrontmatter(content);
            var contentWithoutCodeBlocks = RemoveCodeBlocks(contentWithoutFrontmatter);

            // 检查输出结果示例中是否包含判断词
            var outputSectionMatch = Regex.Match(contentWithoutCodeBlocks, @"###?\s*输出结果.*?(?=###|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            if (outputSectionMatch.Success)
            {
                var outputSection = outputSectionMatch.Value;

                foreach (var judgment in forbiddenJudgments)
                {
                    if (outputSection.Contains(judgment))
                    {
                        violations.Add($"  • {relativePath} - 输出示例包含判断词 '{judgment}'");
                    }
                }
            }
        }

        violations.Should().BeEmpty(string.Join("\n", new[]
            {
                "❌ Enforcement 违规：以下 Skills 输出了判断性结论",
                "",
                "根据 ADR-008 决策 3.2：Skills 只能输出事实，不得输出判断。",
                ""
            }
            .Concat(violations)
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. Skills 输出格式：'发现引用：模块 A → 模块 B'（事实）",
                "  2. 禁止输出：'违规：模块 A 非法引用模块 B'（判断）",
                "  3. 将判断逻辑移至 Agent，由 Agent 根据 ADR 做出裁决",
                "",
                "正确示例：",
                "  ✅ { \"type\": \"DirectReference\", \"source\": \"Orders\", \"target\": \"Members\" }",
                "  ❌ { \"violation\": \"非法引用\", \"severity\": \"High\" }",
                "",
                "参考：docs/adr/constitutional/ADR-008-documentation-governance-constitution.md 决策 3.2"
            })));
    }

    private static string RemoveCodeBlocks(string content)
    {
        // 移除 Markdown 代码块
        return Regex.Replace(content, @"```[\s\S]*?```", string.Empty);
    }

    private static string RemoveFrontmatter(string content)
    {
        // 移除 YAML frontmatter (--- ... ---)
        return Regex.Replace(content, @"^---[\s\S]*?---\s*", string.Empty);
    }

}
