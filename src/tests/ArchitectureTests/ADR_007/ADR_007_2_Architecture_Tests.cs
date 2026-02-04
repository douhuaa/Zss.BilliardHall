namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 验证 ADR-007_2：三态输出规则（Rule）
/// 验证 ADR-007_2_1：三态标识要求
/// 验证 ADR-007_2_2：关键原则
/// 验证 ADR-007_2_3：判定规则
/// </summary>
public sealed class ADR_007_2_Architecture_Tests
{
    private const string _agentFilesPath = ".github/agents";
    private static readonly HashSet<string> _systemAgents = new(StringComparer.OrdinalIgnoreCase)
    {
        "expert-dotnet-software-engineer.agent.md",
        "README.md"
    };

    private static string RepoRoot => TestEnvironment.RepositoryRoot;

    private static string[] GetAgentFiles()
    {
        var agentPath = Path.Combine(RepoRoot, _agentFilesPath);
        if (!Directory.Exists(agentPath)) return Array.Empty<string>();
        
        return Directory.GetFiles(agentPath, "*.agent.md", SearchOption.AllDirectories)
            .Where(f => !_systemAgents.Contains(Path.GetFileName(f)))
            .ToArray();
    }

    [Fact(DisplayName = "ADR-007_2_1: Agent 响应必须包含三态标识")]
    public void ADR_007_2_1_Agent_Responses_Must_Include_Three_State_Indicators()
    {
        var agentFiles = GetAgentFiles();
        if (agentFiles.Length == 0) return;

        var violations = new List<string>();

        foreach (var file in agentFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);

            var missingStates = new List<string>();
            
            // 检查是否提及三态输出
            if (!content.Contains("✅", StringComparison.OrdinalIgnoreCase) &&
                !content.Contains("Allowed", StringComparison.OrdinalIgnoreCase))
            {
                missingStates.Add("✅ Allowed");
            }

            if (!content.Contains("⚠️", StringComparison.OrdinalIgnoreCase) &&
                !content.Contains("Blocked", StringComparison.OrdinalIgnoreCase))
            {
                missingStates.Add("⚠️ Blocked");
            }

            if (!content.Contains("❓", StringComparison.OrdinalIgnoreCase) &&
                !content.Contains("Uncertain", StringComparison.OrdinalIgnoreCase))
            {
                missingStates.Add("❓ Uncertain");
            }

            if (missingStates.Count >= 2) // 如果缺少2个或以上，说明没有三态规范
            {
                violations.Add($"  • {fileName} 缺少三态标识: {string.Join(", ", missingStates)}");
            }
        }

        violations.Should().BeEmpty(
            $"❌ ADR-007_2_1 违规：以下 Agent 文件未实现三态输出规范\n\n" +
            string.Join("\n", violations) + "\n\n" +
            "修复建议：\n" +
            "1. 确保 Agent 响应明确标识 ✅ Allowed、⚠️ Blocked 或 ❓ Uncertain\n" +
            "2. 在 Agent 配置中定义三态输出规范\n" +
            "3. 参考 ADR-007_2_1 三态标识要求");
    }

    [Fact(DisplayName = "ADR-007_2_2: 无法确认时必须假定禁止")]
    public void ADR_007_2_2_Must_Assume_Forbidden_When_Uncertain()
    {
        var agentFiles = GetAgentFiles();
        if (agentFiles.Length == 0) return;

        var violations = new List<string>();

        foreach (var file in agentFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);

            // 检查是否声明了"默认禁止"原则
            var hasDefaultForbiddenPrinciple = 
                content.Contains("默认禁止", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("假定禁止", StringComparison.OrdinalIgnoreCase) ||
                (content.Contains("Uncertain", StringComparison.OrdinalIgnoreCase) &&
                 content.Contains("禁止", StringComparison.OrdinalIgnoreCase));

            if (!hasDefaultForbiddenPrinciple)
            {
                violations.Add($"  • {fileName} 未声明'无法确认时默认禁止'原则");
            }
        }

        violations.Should().BeEmpty(
            $"❌ ADR-007_2_2 违规：以下 Agent 文件未声明关键原则\n\n" +
            string.Join("\n", violations) + "\n\n" +
            "修复建议：\n" +
            "1. 在 Agent 配置中明确声明：当无法确认 ADR 明确允许某行为时，必须假定该行为被禁止\n" +
            "2. 使用 Uncertain 状态标识不确定的情况\n" +
            "3. 参考 ADR-007_2_2 关键原则");
    }

    [Fact(DisplayName = "ADR-007_2_3: 禁止输出模糊判断")]
    public void ADR_007_2_3_Must_Not_Output_Ambiguous_Judgments()
    {
        var agentFiles = GetAgentFiles();
        if (agentFiles.Length == 0) return;

        var violations = new List<string>();

        foreach (var file in agentFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);

            // 检查是否禁止模糊判断
            var hasFuzzyJudgmentProhibition = 
                content.Contains("禁止模糊", StringComparison.OrdinalIgnoreCase) ||
                (content.Contains("应该没问题", StringComparison.OrdinalIgnoreCase) &&
                 content.Contains("❌", StringComparison.OrdinalIgnoreCase)) ||
                (content.Contains("可能可以", StringComparison.OrdinalIgnoreCase) &&
                 content.Contains("❌", StringComparison.OrdinalIgnoreCase));

            if (!hasFuzzyJudgmentProhibition)
            {
                violations.Add($"  • {fileName} 未明确禁止模糊判断");
            }
        }

        violations.Should().BeEmpty(
            $"❌ ADR-007_2_3 违规：以下 Agent 文件未明确禁止模糊判断\n\n" +
            string.Join("\n", violations) + "\n\n" +
            "修复建议：\n" +
            "1. 禁止输出'应该没问题'、'可能可以'、'试试看'等模糊词汇\n" +
            "2. 必须使用明确的三态标识\n" +
            "3. Uncertain 时应引导查阅 ADR 或咨询架构师\n" +
            "4. 参考 ADR-007_2_3 判定规则");
    }
}
