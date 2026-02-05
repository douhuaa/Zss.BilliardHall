namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_946;

/// <summary>
/// ADR-946_2: 模板与示例结构约束
/// 验证 ADR 文档中模板和示例避免解析歧义
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-946_2_1: 模板与示例必须避免解析歧义
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-946-adr-heading-level-semantic-constraint.md
/// </summary>
public sealed class ADR_946_2_Architecture_Tests
{
    // 关键语义块标题（在示例/模板中不能直接使用 ## 级别）
    private static readonly string[] KeySemanticHeadings = new[]
    {
        "Relationships",
        "Decision",
        "Enforcement",
        "Glossary",
        "Non-Goals",
        "Prohibited",
        "References",
        "History"
    };

    /// <summary>
    /// ADR-946_2_1: 模板与示例必须避免解析歧义
    /// 验证示例/模板使用英文、占位符或降级标题，不使用语义 ## 标题（§ADR-946_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-946_2_1: 模板与示例必须避免解析歧义")]
    public void ADR_946_2_1_Templates_And_Examples_Must_Avoid_Parsing_Ambiguity()
    {
        var adrFiles = FileSystemTestHelper.GetAdrFiles();

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var lines = content.Split('\n');

            var inCodeBlock = false;
            var lineNumber = 0;

            foreach (var line in lines)
            {
                lineNumber++;
                var trimmed = line.TrimStart();

                // 检测代码块边界
                if (trimmed.StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }

                // 在代码块外检查可能产生歧义的标题
                if (!inCodeBlock)
                {
                    foreach (var semanticHeading in KeySemanticHeadings)
                    {
                        // 检查是否在示例上下文中使用了 ## 级别的语义标题
                        // 允许：## Relationships Example, ## Decision Example, ### Relationships
                        // 不允许：在 Example/Template/示例 章节中直接使用 ## Relationships

                        // 检测可能的违规模式：
                        // 1. 代码块外的 ## <语义块> Template
                        // 2. 代码块外的 ## <语义块> 示例
                        var templatePattern = $@"^##\s+{Regex.Escape(semanticHeading)}\s+(Template|模板)";
                        if (Regex.IsMatch(line, templatePattern, RegexOptions.IgnoreCase))
                        {
                            violations.Add($"{Path.GetFileName(adrFile)}:{lineNumber} - 在模板中使用了 ## 级别的 '{semanticHeading}'，应使用 ### 或代码块");
                        }

                        var examplePattern = $@"^##\s+{Regex.Escape(semanticHeading)}\s+(Example|示例)";
                        if (Regex.IsMatch(line, examplePattern, RegexOptions.IgnoreCase))
                        {
                            violations.Add($"{Path.GetFileName(adrFile)}:{lineNumber} - 在示例中使用了 ## 级别的 '{semanticHeading}'，应使用 ### 或代码块");
                        }
                    }
                }
            }
        }

        var message = AssertionMessageBuilder.BuildFormatViolationMessage(
            ruleId: "ADR-946_2_1",
            summary: "以下 ADR 文件违反模板与示例结构约束",
            violations: violations,
            remediationSteps: new[]
            {
                "在模板/示例中使用 ### 级别标题而非 ## 级别",
                "或将示例内容放入代码块中",
                "或使用占位符（如 ## [Relationships]）",
                "避免机器解析时产生歧义"
            },
            adrReference: TestConstants.Adr946Path);

        violations.Should().BeEmpty(message);
    }
}
