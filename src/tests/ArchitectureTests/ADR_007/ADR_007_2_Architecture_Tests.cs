namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

/// <summary>
/// 验证 ADR-007_2：三态输出规则（Rule）
/// 验证 ADR-007_2_1：三态标识要求
/// 验证 ADR-007_2_2：关键原则
/// 验证 ADR-007_2_3：判定规则
/// </summary>
public sealed class ADR_007_2_Architecture_Tests
{
    [Fact(DisplayName = "ADR-007_2_1: Agent 响应必须包含三态标识")]
    public void ADR_007_2_1_Agent_Responses_Must_Include_Three_State_Indicators()
    {
        var agentFiles = FileSystemTestHelper.GetAgentFiles(
            includeSystemAgents: false,
            excludeGuardian: false);

        if (!agentFiles.Any()) return;

        var violations = new List<string>();

        foreach (var file in agentFiles)
        {
            var fileName = Path.GetFileName(file);
            var missingStates = new List<string>();

            // 使用常量检查三态标识
            foreach (var indicator in TestConstants.ThreeStateIndicators)
            {
                var hasIndicator = FileSystemTestHelper.FileContainsAnyKeyword(
                    file,
                    new[] { indicator, indicator.Split(' ')[1] }, // 检查完整形式和简写形式
                    ignoreCase: true);

                if (!hasIndicator)
                {
                    missingStates.Add(indicator);
                }
            }

            if (missingStates.Count >= 2) // 如果缺少2个或以上，说明没有三态规范
            {
                violations.Add($"{fileName} 缺少三态标识: {string.Join(", ", missingStates)}");
            }
        }

        var message = AssertionMessageBuilder.BuildWithViolations(
            ruleId: "ADR-007_2_1",
            summary: "以下 Agent 文件未实现三态输出规范",
            failingTypes: violations,
            remediationSteps: new[]
            {
                "确保 Agent 响应明确标识 ✅ Allowed、⚠️ Blocked 或 ❓ Uncertain",
                "在 Agent 配置中定义三态输出规范",
                "每种判定结果都应使用相应的标识"
            },
            adrReference: TestConstants.Adr007Path);

        violations.Should().BeEmpty(message);
    }

    [Fact(DisplayName = "ADR-007_2_2: 无法确认时必须假定禁止")]
    public void ADR_007_2_2_Must_Assume_Forbidden_When_Uncertain()
    {
        var agentFiles = FileSystemTestHelper.GetAgentFiles(
            includeSystemAgents: false,
            excludeGuardian: false);

        if (!agentFiles.Any()) return;

        var violations = new List<string>();

        // 检查是否声明了"默认禁止"原则的关键词
        var defaultForbiddenKeywords = new[]
        {
            "默认禁止",
            "假定禁止",
            "Uncertain 时禁止"
        };

        foreach (var file in agentFiles)
        {
            var fileName = Path.GetFileName(file);

            // 检查是否包含任一关键词
            var hasDefaultForbiddenPrinciple = FileSystemTestHelper.FileContainsAnyKeyword(
                file,
                defaultForbiddenKeywords,
                ignoreCase: true);

            if (!hasDefaultForbiddenPrinciple)
            {
                violations.Add($"{fileName} 未声明'无法确认时默认禁止'原则");
            }
        }

        var message = AssertionMessageBuilder.BuildWithViolations(
            ruleId: "ADR-007_2_2",
            summary: "以下 Agent 文件未声明关键原则",
            failingTypes: violations,
            remediationSteps: new[]
            {
                "在 Agent 配置中明确声明：当无法确认 ADR 明确允许某行为时，必须假定该行为被禁止",
                "使用 Uncertain 状态标识不确定的情况",
                "引导用户查阅相关 ADR 文档或咨询架构师"
            },
            adrReference: TestConstants.Adr007Path);

        violations.Should().BeEmpty(message);
    }

    [Fact(DisplayName = "ADR-007_2_3: 禁止输出模糊判断")]
    public void ADR_007_2_3_Must_Not_Output_Ambiguous_Judgments()
    {
        var agentFiles = FileSystemTestHelper.GetAgentFiles(
            includeSystemAgents: false,
            excludeGuardian: false);

        if (!agentFiles.Any()) return;

        var violations = new List<string>();

        // 检查是否禁止模糊判断的关键词
        var fuzzyProhibitionKeywords = new[]
        {
            "禁止模糊",
            "不得使用模糊词汇",
            "禁止'应该没问题'",
            "禁止'可能可以'"
        };

        foreach (var file in agentFiles)
        {
            var fileName = Path.GetFileName(file);

            var hasFuzzyJudgmentProhibition = FileSystemTestHelper.FileContainsAnyKeyword(
                file,
                fuzzyProhibitionKeywords,
                ignoreCase: true);

            if (!hasFuzzyJudgmentProhibition)
            {
                violations.Add($"{fileName} 未明确禁止模糊判断");
            }
        }

        var message = AssertionMessageBuilder.BuildWithViolations(
            ruleId: "ADR-007_2_3",
            summary: "以下 Agent 文件未明确禁止模糊判断",
            failingTypes: violations,
            remediationSteps: new[]
            {
                "禁止输出'应该没问题'、'可能可以'、'试试看'等模糊词汇",
                "必须使用明确的三态标识（✅ Allowed / ⚠️ Blocked / ❓ Uncertain）",
                "Uncertain 时应引导查阅 ADR 或咨询架构师，而非提供模糊建议"
            },
            adrReference: TestConstants.Adr007Path);

        violations.Should().BeEmpty(message);
    }
}
