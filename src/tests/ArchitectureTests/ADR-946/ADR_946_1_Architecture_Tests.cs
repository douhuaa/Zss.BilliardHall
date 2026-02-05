namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_946;

/// <summary>
/// ADR-946_1: 标题级别语义约束
/// 验证 ADR 文档中标题层级与机器可解析语义之间的强制对应关系
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-946_1_1: 标题级别即语义级别
/// - ADR-946_1_2: 关键语义块标题约束
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-946-adr-heading-level-semantic-constraint.md
/// </summary>
public sealed class ADR_946_1_Architecture_Tests
{
    /// <summary>
    /// ADR-946_1_1: 标题级别即语义级别
    /// 验证每个 ADR 文件必须且仅有一个 # 标题，语义块使用 ## 级别（§ADR-946_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-946_1_1: ADR 文件必须有且仅有一个 # 标题")]
    public void ADR_946_1_1_ADR_Must_Have_Exactly_One_H1_Title()
    {
        var adrFiles = FileSystemTestHelper.GetAdrFiles();

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var lines = content.Split('\n');

            // 计算 # 标题数量（必须在行首，不能在代码块中）
            var h1Count = 0;
            var inCodeBlock = false;

            foreach (var line in lines)
            {
                var trimmed = line.TrimStart();

                // 检测代码块
                if (trimmed.StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }

                // 只在非代码块中统计 # 标题
                if (!inCodeBlock && Regex.IsMatch(line, @"^#\s+"))
                {
                    h1Count++;
                }
            }

            if (h1Count != 1)
            {
                violations.Add($"{Path.GetFileName(adrFile)}: 发现 {h1Count} 个 # 标题，应该有且仅有 1 个");
            }
        }

        var message = AssertionMessageBuilder.BuildFormatViolationMessage(
            ruleId: "ADR-946_1_1",
            summary: "以下 ADR 文件违反标题级别即语义级别规则",
            violations: violations,
            remediationSteps: new[]
            {
                "确保每个 ADR 文件有且仅有一个 # 级别标题（文档标题）",
                "所有语义块（如 Decision、Relationships）使用 ## 级别",
                "检查是否有误将语义块标题设置为 # 级别"
            },
            adrReference: TestConstants.Adr946Path);

        violations.Should().BeEmpty(message);
    }

    /// <summary>
    /// ADR-946_1_2: 关键语义块标题约束
    /// 验证关键语义块（Relationships、Decision、Enforcement、Glossary）必须使用 ## 级别且不能重复（§ADR-946_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-946_1_2: 关键语义块标题必须使用 ## 级别且不能重复")]
    public void ADR_946_1_2_Key_Semantic_Blocks_Must_Use_H2_And_Not_Duplicate()
    {
        var adrFiles = FileSystemTestHelper.GetAdrFiles();

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var lines = content.Split('\n');

            var inCodeBlock = false;
            var semanticBlockCounts = new Dictionary<string, int>();

            foreach (var line in lines)
            {
                var trimmed = line.TrimStart();

                // 检测代码块
                if (trimmed.StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }

                // 只在非代码块中检查语义块标题
                if (!inCodeBlock)
                {
                    foreach (var semanticHeading in TestConstants.KeySemanticHeadings)
                    {
                        // 检查是否是 ## 级别的关键语义块
                        // 使用严格匹配：## <语义块名称>（中文或英文，可能带括号）
                        var pattern = $@"^##\s+{Regex.Escape(semanticHeading)}(?:\s*（.*?）|\s*\(.*?\))?(?:\s|$)";
                        if (Regex.IsMatch(line, pattern))
                        {
                            if (!semanticBlockCounts.ContainsKey(semanticHeading))
                            {
                                semanticBlockCounts[semanticHeading] = 0;
                            }
                            semanticBlockCounts[semanticHeading]++;
                        }

                        // 检查是否在模板/示例中错误使用了 ## 级别（不带 Example 等后缀）
                        var wrongPattern = $@"^##\s+{Regex.Escape(semanticHeading)}\s*Example";
                        if (Regex.IsMatch(line, wrongPattern))
                        {
                            violations.Add($"{Path.GetFileName(adrFile)}: 在模板/示例中使用了 ## 级别的 '{semanticHeading} Example'，应使用 ### 或更低级别");
                        }
                    }
                }
            }

            // 检查重复的语义块
            foreach (var kvp in semanticBlockCounts)
            {
                if (kvp.Value > 1)
                {
                    violations.Add($"{Path.GetFileName(adrFile)}: 关键语义块 '{kvp.Key}' 出现了 {kvp.Value} 次，只能出现 1 次");
                }
            }
        }

        var message = AssertionMessageBuilder.BuildFormatViolationMessage(
            ruleId: "ADR-946_1_2",
            summary: "以下 ADR 文件违反关键语义块标题约束",
            violations: violations,
            remediationSteps: new[]
            {
                "确保关键语义块（Relationships、Decision、Enforcement、Glossary）使用 ## 级别",
                "确保每个关键语义块在文件中只出现一次",
                "模板/示例中的语义块应使用 ### 或更低级别，并带有 'Example' 等标识"
            },
            adrReference: TestConstants.Adr946Path);

        violations.Should().BeEmpty(message);
    }
}
