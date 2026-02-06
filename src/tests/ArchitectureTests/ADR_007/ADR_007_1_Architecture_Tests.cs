namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

/// <summary>
/// 验证 ADR-007_1：Agent 根本定位（Rule）
/// 验证 ADR-007_1_1：Agent 定位规则
/// 验证 ADR-007_1_2：权威边界
/// 验证 ADR-007_1_3：判定规则
/// </summary>
public sealed class ADR_007_1_Architecture_Tests
{
    [Fact(DisplayName = "ADR-007_1_1: Agent 定位必须为工具")]
    public void ADR_007_1_1_Agent_Positioning_Must_Be_Tool()
    {
        var agentFiles = FileSystemTestHelper.GetAgentFiles(
            includeSystemAgents: false,
            excludeGuardian: false);

        if (!agentFiles.Any()) return;

        var violations = new List<string>();

        // Agent 不应声称自己是决策者或权威的禁止词汇
        var forbiddenPhrases = new[]
        {
            "我批准", "我裁决", "我决定",
            "作为最终决策者", "拥有裁决权"
        };

        foreach (var file in agentFiles)
        {
            var fileName = Path.GetFileName(file);

            foreach (var phrase in forbiddenPhrases)
            {
                if (FileSystemTestHelper.FileContainsAnyKeyword(file, new[] { phrase }, ignoreCase: true))
                {
                    violations.Add($"{fileName} 包含禁止的定位词汇: '{phrase}'");
                    break; // 每个文件只报告一次
                }
            }
        }

        var message = AssertionMessageBuilder.BuildWithViolations(
            ruleId: "ADR-007_1_1",
            summary: "以下 Agent 文件违反了定位规则",
            failingTypes: violations,
            remediationSteps: new[]
            {
                "Agent 应定位为工具，而非决策者",
                "移除所有声称拥有裁决权的表述",
                "确保 Agent 配置明确引用 ADR 作为权威来源"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr007);

        violations.Should().BeEmpty(message);
    }

    [Fact(DisplayName = "ADR-007_1_2: Agent 必须声明权威边界")]
    public void ADR_007_1_2_Agent_Must_Declare_Authority_Boundary()
    {
        var agentFiles = FileSystemTestHelper.GetAgentFiles(
            includeSystemAgents: false,
            excludeGuardian: false);

        if (!agentFiles.Any()) return;

        var violations = new List<string>();

        // Agent 应声明冲突时以 ADR 为准的关键词
        var authorityKeywords = new[]
        {
            "以 ADR",
            "ADR 为准",
            "ADR 正文"
        };

        foreach (var file in agentFiles)
        {
            var fileName = Path.GetFileName(file);

            var hasAuthorityDeclaration = FileSystemTestHelper.FileContainsAnyKeyword(
                file,
                authorityKeywords,
                ignoreCase: true);

            if (!hasAuthorityDeclaration)
            {
                violations.Add($"{fileName} 缺少权威边界声明");
            }
        }

        var message = AssertionMessageBuilder.BuildWithViolations(
            ruleId: "ADR-007_1_2",
            summary: "以下 Agent 文件缺少权威边界声明",
            failingTypes: violations,
            remediationSteps: new[]
            {
                "在 Agent 配置中声明'冲突时以 ADR 正文为准'",
                "明确 Agent 配置不承担宪法责任",
                "确保 Agent 只是工具，权威在 ADR"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr007);

        violations.Should().BeEmpty(message);
    }

    [Fact(DisplayName = "ADR-007_1_3: Agent 禁止批准架构破例")]
    public void ADR_007_1_3_Agent_Must_Not_Approve_Architecture_Bypass()
    {
        var agentFiles = FileSystemTestHelper.GetAgentFiles(
            includeSystemAgents: false,
            excludeGuardian: false);

        if (!agentFiles.Any()) return;

        var violations = new List<string>();

        // Agent 不应建议绕过架构测试或批准破例的禁止行为
        var forbiddenActions = new[]
        {
            "批准破例", "绕过测试", "跳过验证",
            "可以不遵守", "这次可以"
        };

        foreach (var file in agentFiles)
        {
            var content = FileSystemTestHelper.ReadFileContent(file);
            var fileName = Path.GetFileName(file);

            foreach (var action in forbiddenActions)
            {
                // 检查是否包含禁止行为，但排除明确禁止的表述
                if (content.Contains(action, StringComparison.OrdinalIgnoreCase) &&
                    !content.Contains($"禁止{action}", StringComparison.OrdinalIgnoreCase) &&
                    !content.Contains($"不得{action}", StringComparison.OrdinalIgnoreCase) &&
                    !content.Contains($"❌ {action}", StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add($"{fileName} 可能允许: '{action}'");
                    break; // 每个文件只报告一次
                }
            }
        }

        var message = AssertionMessageBuilder.BuildWithViolations(
            ruleId: "ADR-007_1_3",
            summary: "以下 Agent 文件违反判定规则",
            failingTypes: violations,
            remediationSteps: new[]
            {
                "移除所有批准破例的表述",
                "移除所有建议绕过测试的表述",
                "Agent 只能引用 ADR 并引导查阅",
                "不得提供绕过架构约束的建议"
            },
            adrReference: ArchitectureTestSpecification.Adr.KnownDocuments.Adr007);

        violations.Should().BeEmpty(message);
    }
}
