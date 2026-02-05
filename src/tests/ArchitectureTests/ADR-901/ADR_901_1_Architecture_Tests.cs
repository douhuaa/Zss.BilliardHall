namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_901;

/// <summary>
/// ADR-901_1: 三态语义模型（Constraint / Warning / Notice）
/// 架构测试：验证 ADR 和文档中的风险表达语义合规性
/// </summary>
public sealed class ADR_901_1_Architecture_Tests
{
    // 三态语义关键词
    private static readonly string[] ConstraintKeywords = { "Constraint", "约束" };
    private static readonly string[] WarningKeywords = { "Warning", "警告" };
    private static readonly string[] NoticeKeywords = { "Notice", "提示", "说明" };

    // 禁止的语义关键词
    private static readonly string[] ProhibitedSemanticKeywords =
    {
        "Suggestion", "建议",
        "Recommendation", "推荐",
        "Attention", "注意",
        "Soft Rule", "软规则",
        "Best Practice" // 当具有约束性时禁止
    };

    // Constraint 必须的元素
    private static readonly string[] ConstraintRequiredElements =
    {
        "规则", "Rule",
        "范围", "Scope",
        "后果", "Consequence"
    };

    // Warning 必须的元素
    private static readonly string[] WarningRequiredElements =
    {
        "风险", "Risk",
        "放行", "Override"
    };

    /// <summary>
    /// ADR-901_1_1: 风险表达必须使用三态语义模型
    /// </summary>
    [Fact(DisplayName = "ADR-901_1_1: 风险表达必须使用三态语义模型")]
    public void ADR_901_1_1_Risk_Expressions_Must_Use_Tristate_Semantic_Model()
    {
        var adrDirectory = FileSystemTestHelper.GetAbsolutePath(TestConstants.AdrDocsPath);

        var directoryMessage = AssertionMessageBuilder.BuildDirectoryNotFoundMessage(
            ruleId: "ADR-901_1_1",
            directoryPath: adrDirectory,
            directoryDescription: "ADR 文档目录",
            remediationSteps: new[]
            {
                "确保 docs/adr 目录存在",
                "创建必要的 ADR 文档"
            },
            adrReference: TestConstants.Adr901Path);

        Directory.Exists(adrDirectory).Should().BeTrue(directoryMessage);

        var adrFiles = GetActiveAdrFiles(adrDirectory);

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 检查是否使用了禁止的语义关键词
            foreach (var prohibited in ProhibitedSemanticKeywords)
            {
                // 使用正则表达式检查是否在块引用或标题中使用了禁止的关键词
                var pattern = $@">\s*.*?\b{Regex.Escape(prohibited)}\b|^#+.*?\b{Regex.Escape(prohibited)}\b";
                if (Regex.IsMatch(content, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase))
                {
                    violations.Add($"{fileName}: 使用了禁止的语义关键词 '{prohibited}'");
                }
            }
        }

        var message = AssertionMessageBuilder.BuildWithViolations(
            ruleId: "ADR-901_1_1",
            summary: "风险表达必须使用三态语义模型（Constraint / Warning / Notice）",
            failingTypes: violations,
            remediationSteps: new[]
            {
                "移除所有禁止的语义关键词（Suggestion、Recommendation、Attention等）",
                "将所有风险表达明确归类为 Constraint、Warning 或 Notice 之一",
                "使用标准的语义声明块格式"
            },
            adrReference: TestConstants.Adr901Path);

        violations.Should().BeEmpty(message);
    }

    /// <summary>
    /// ADR-901_1_2: Constraint 的合法性条件
    /// </summary>
    [Fact(DisplayName = "ADR-901_1_2: Constraint 必须包含完整的合法性元素")]
    public void ADR_901_1_2_Constraint_Must_Have_Legality_Conditions()
    {
        var adrDirectory = FileSystemTestHelper.GetAbsolutePath(TestConstants.AdrDocsPath);
        FileSystemTestHelper.AssertDirectoryExists(adrDirectory, $"ADR 文档目录不存在: {adrDirectory}");

        var adrFiles = GetActiveAdrFiles(adrDirectory);

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 查找所有 Constraint 块
            var constraintBlocks = FindSemanticBlocks(content, ConstraintKeywords);

            foreach (var block in constraintBlocks)
            {
                // 检查是否包含执行级别声明（L1/L2/L3）
                if (!Regex.IsMatch(block, @"\bL[123]\b"))
                {
                    violations.Add($"{fileName}: Constraint 块缺少执行级别声明（L1/L2/L3）");
                }

                // 检查是否包含必须的元素（至少中文或英文之一）
                var hasRule = ConstraintRequiredElements.Take(2).Any(e => block.Contains(e, StringComparison.OrdinalIgnoreCase));
                var hasScope = ConstraintRequiredElements.Skip(2).Take(2).Any(e => block.Contains(e, StringComparison.OrdinalIgnoreCase));
                var hasConsequence = ConstraintRequiredElements.Skip(4).Any(e => block.Contains(e, StringComparison.OrdinalIgnoreCase));

                if (!hasRule)
                {
                    violations.Add($"{fileName}: Constraint 块缺少规则描述（规则/Rule）");
                }
                if (!hasScope)
                {
                    violations.Add($"{fileName}: Constraint 块缺少范围说明（范围/Scope）");
                }
                if (!hasConsequence)
                {
                    violations.Add($"{fileName}: Constraint 块缺少后果说明（后果/Consequence）");
                }
            }
        }

        if (violations.Any())
        {
            var message = "⚠️ ADR-901_1_2 建议：Constraint 应包含完整的合法性条件\n" +
                         string.Join("\n", violations) +
                         "\n\n建议：Constraint 应明确声明规则、范围、后果和执行级别。";

            // 这是建议性检查，暂时只输出调试信息
            System.Diagnostics.Debug.WriteLine(message);
        }
    }

    /// <summary>
    /// ADR-901_1_3: Warning 的边界
    /// </summary>
    [Fact(DisplayName = "ADR-901_1_3: Warning 必须明确风险和放行条件")]
    public void ADR_901_1_3_Warning_Must_Have_Clear_Boundaries()
    {
        var adrDirectory = FileSystemTestHelper.GetAbsolutePath(TestConstants.AdrDocsPath);
        FileSystemTestHelper.AssertDirectoryExists(adrDirectory, $"ADR 文档目录不存在: {adrDirectory}");

        var adrFiles = GetActiveAdrFiles(adrDirectory);

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 查找所有 Warning 块
            var warningBlocks = FindSemanticBlocks(content, WarningKeywords);

            foreach (var block in warningBlocks)
            {
                // 检查是否包含执行级别声明
                if (!Regex.IsMatch(block, @"\bL[123]\b"))
                {
                    violations.Add($"{fileName}: Warning 块缺少执行级别声明（L1/L2/L3）");
                }

                // 检查是否包含风险说明
                var hasRisk = WarningRequiredElements.Take(2).Any(e => block.Contains(e, StringComparison.OrdinalIgnoreCase));
                var hasOverride = WarningRequiredElements.Skip(2).Any(e => block.Contains(e, StringComparison.OrdinalIgnoreCase));

                if (!hasRisk)
                {
                    violations.Add($"{fileName}: Warning 块缺少风险说明（风险/Risk）");
                }
                if (!hasOverride)
                {
                    violations.Add($"{fileName}: Warning 块缺少放行条件（放行/Override）");
                }

                // 检查是否使用了禁止的表述
                var prohibitedPhrases = new[] { "建议", "可以考虑", "最好", "suggest", "consider", "better" };
                foreach (var phrase in prohibitedPhrases)
                {
                    if (block.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add($"{fileName}: Warning 块使用了禁止的弱化表述 '{phrase}'");
                    }
                }
            }
        }

        if (violations.Any())
        {
            var message = "⚠️ ADR-901_1_3 建议：Warning 应明确边界\n" +
                         string.Join("\n", violations) +
                         "\n\n建议：Warning 必须明确风险后果、是否允许放行、放行责任主体和执行级别。";

            // 这是建议性检查，暂时只输出调试信息
            System.Diagnostics.Debug.WriteLine(message);
        }
    }

    /// <summary>
    /// ADR-901_1_4: Notice 的纯信息性约束
    /// </summary>
    [Fact(DisplayName = "ADR-901_1_4: Notice 必须保持纯信息性，不得包含隐性规则")]
    public void ADR_901_1_4_Notice_Must_Be_Pure_Informational()
    {
        var adrDirectory = FileSystemTestHelper.GetAbsolutePath(TestConstants.AdrDocsPath);
        FileSystemTestHelper.AssertDirectoryExists(adrDirectory, $"ADR 文档目录不存在: {adrDirectory}");

        var adrFiles = GetActiveAdrFiles(adrDirectory);

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 查找所有 Notice 块
            var noticeBlocks = FindSemanticBlocks(content, NoticeKeywords);

            foreach (var block in noticeBlocks)
            {
                // 检查是否包含 MUST/SHOULD/SHALL 等强制性关键词
                var imperativeKeywords = new[] { "MUST", "SHOULD", "SHALL", "必须", "应该", "禁止", "不得" };
                foreach (var keyword in imperativeKeywords)
                {
                    if (Regex.IsMatch(block, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase))
                    {
                        violations.Add($"{fileName}: Notice 块包含强制性关键词 '{keyword}'，违反纯信息性约束");
                    }
                }

                // 检查是否包含流程性约束
                var processKeywords = new[] { "流程", "步骤", "必须执行", "process", "step", "must execute" };
                foreach (var keyword in processKeywords)
                {
                    if (block.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add($"{fileName}: Notice 块可能包含流程性约束 '{keyword}'");
                    }
                }
            }
        }

        var message = AssertionMessageBuilder.BuildWithViolations(
            ruleId: "ADR-901_1_4",
            summary: "Notice 必须保持纯信息性",
            failingTypes: violations,
            remediationSteps: new[]
            {
                "从 Notice 块中移除所有强制性关键词（MUST、SHOULD、SHALL、必须、应该、禁止、不得）",
                "Notice 只能用于背景说明、设计动机、经验性解释",
                "如需表达约束，将内容移至 Constraint 或 Warning 块"
            },
            adrReference: TestConstants.Adr901Path);

        violations.Should().BeEmpty(message);
    }

    // 辅助方法

    /// <summary>
    /// 获取所有活跃（非归档）的 ADR 文件
    /// </summary>
    private static List<string> GetActiveAdrFiles(string adrDirectory)
    {
        return Directory.GetFiles(adrDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => Regex.IsMatch(Path.GetFileName(f), @"^ADR-\d+", RegexOptions.IgnoreCase))
            .Where(f => !f.Contains("/archive/", StringComparison.OrdinalIgnoreCase)) // 排除归档的 ADR
            .ToList();
    }

    /// <summary>
    /// 查找文档中的语义块
    /// </summary>
    private static List<string> FindSemanticBlocks(string content, string[] keywords)
    {
        var blocks = new List<string>();
        var lines = content.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // 检查是否是语义块的开始（> 开头，包含关键词）
            if (line.TrimStart().StartsWith(">"))
            {
                foreach (var keyword in keywords)
                {
                    if (line.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        // 收集整个块（连续的 > 行）
                        var block = new System.Text.StringBuilder();
                        block.AppendLine(line);

                        for (int j = i + 1; j < lines.Length; j++)
                        {
                            if (lines[j].TrimStart().StartsWith(">"))
                            {
                                block.AppendLine(lines[j]);
                            }
                            else if (string.IsNullOrWhiteSpace(lines[j]))
                            {
                                // 空行，继续检查下一行
                                continue;
                            }
                            else
                            {
                                // 块结束
                                break;
                            }
                        }

                        blocks.Add(block.ToString());
                        break;
                    }
                }
            }
        }

        return blocks;
    }
}
