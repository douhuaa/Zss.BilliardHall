namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 验证 ADR-007_1：Agent 根本定位（Rule）
/// 验证 ADR-007_1_1：Agent 定位规则
/// 验证 ADR-007_1_2：权威边界
/// 验证 ADR-007_1_3：判定规则
/// </summary>
public sealed class ADR_007_1_Architecture_Tests
{
    private const string _agentFilesPath = ".github/agents";
    private static readonly HashSet<string> _systemAgents = new(StringComparer.OrdinalIgnoreCase)
    {
        "expert-dotnet-software-engineer.agent.md",
        "README.md"
    };

    private static string RepoRoot => TestEnvironment.RepositoryRoot;

    private static string[] GetAgentFiles(bool excludeGuardian = false)
    {
        var agentPath = Path.Combine(RepoRoot, _agentFilesPath);
        if (!Directory.Exists(agentPath)) return Array.Empty<string>();
        
        return Directory.GetFiles(agentPath, "*.agent.md", SearchOption.AllDirectories)
            .Where(f => !_systemAgents.Contains(Path.GetFileName(f)))
            .Where(f => !excludeGuardian || !Path.GetFileName(f).Equals("architecture-guardian.agent.md", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    [Fact(DisplayName = "ADR-007_1_1: Agent 定位必须为工具")]
    public void ADR_007_1_1_Agent_Positioning_Must_Be_Tool()
    {
        var agentFiles = GetAgentFiles();
        if (agentFiles.Length == 0) return; // No agent files to check

        var violations = new List<string>();

        foreach (var file in agentFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);

            // Agent 不应声称自己是决策者或权威
            var forbiddenPhrases = new[]
            {
                "我批准", "我裁决", "我决定",
                "作为最终决策者", "拥有裁决权"
            };

            foreach (var phrase in forbiddenPhrases)
            {
                if (content.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add($"  • {fileName} 包含禁止的定位词汇: '{phrase}'");
                }
            }
        }

        violations.Should().BeEmpty(
            $"❌ ADR-007_1_1 违规：以下 Agent 文件违反了定位规则\n\n" +
            string.Join("\n", violations) + "\n\n" +
            "修复建议：\n" +
            "1. Agent 应定位为工具，而非决策者\n" +
            "2. 移除所有声称拥有裁决权的表述\n" +
            "3. 参考 ADR-007_1_1 Agent 定位规则");
    }

    [Fact(DisplayName = "ADR-007_1_2: Agent 必须声明权威边界")]
    public void ADR_007_1_2_Agent_Must_Declare_Authority_Boundary()
    {
        var agentFiles = GetAgentFiles();
        if (agentFiles.Length == 0) return;

        var violations = new List<string>();

        foreach (var file in agentFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);

            // Agent 应声明冲突时以 ADR 为准
            var hasAuthorityDeclaration = 
                content.Contains("以 ADR", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("ADR 为准", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("ADR 正文", StringComparison.OrdinalIgnoreCase);

            if (!hasAuthorityDeclaration)
            {
                violations.Add($"  • {fileName} 缺少权威边界声明");
            }
        }

        violations.Should().BeEmpty(
            $"❌ ADR-007_1_2 违规：以下 Agent 文件缺少权威边界声明\n\n" +
            string.Join("\n", violations) + "\n\n" +
            "修复建议：\n" +
            "1. 在 Agent 配置中声明'冲突时以 ADR 正文为准'\n" +
            "2. 明确 Agent 配置不承担宪法责任\n" +
            "3. 参考 ADR-007_1_2 权威边界");
    }

    [Fact(DisplayName = "ADR-007_1_3: Agent 禁止批准架构破例")]
    public void ADR_007_1_3_Agent_Must_Not_Approve_Architecture_Bypass()
    {
        var agentFiles = GetAgentFiles();
        if (agentFiles.Length == 0) return;

        var violations = new List<string>();

        foreach (var file in agentFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);

            // Agent 不应建议绕过架构测试或批准破例
            var forbiddenActions = new[]
            {
                "批准破例", "绕过测试", "跳过验证",
                "可以不遵守", "这次可以"
            };

            foreach (var action in forbiddenActions)
            {
                if (content.Contains(action, StringComparison.OrdinalIgnoreCase) &&
                    !content.Contains($"禁止{action}", StringComparison.OrdinalIgnoreCase) &&
                    !content.Contains($"不得{action}", StringComparison.OrdinalIgnoreCase) &&
                    !content.Contains($"❌ {action}", StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add($"  • {fileName} 可能允许: '{action}'");
                }
            }
        }

        violations.Should().BeEmpty(
            $"❌ ADR-007_1_3 违规：以下 Agent 文件违反判定规则\n\n" +
            string.Join("\n", violations) + "\n\n" +
            "修复建议：\n" +
            "1. 移除所有批准破例的表述\n" +
            "2. 移除所有建议绕过测试的表述\n" +
            "3. Agent 只能引用 ADR 并引导查阅\n" +
            "4. 参考 ADR-007_1_3 判定规则");
    }
}
