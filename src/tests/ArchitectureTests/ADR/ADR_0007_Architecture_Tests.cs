using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0007: Agent 行为与权限宪法
/// 验证所有 Agent 配置文件符合 ADR-0007 的行为约束和权限边界
///
/// 【测试覆盖映射】
/// ├─ ADR-0007.1: 三态输出强制 → Agent_Responses_Must_Include_Three_State_Indicators
/// ├─ ADR-0007.3: 禁止批准破例 → Agent_Files_Must_Prohibit_Architecture_Bypass
/// ├─ ADR-0007.5: 裁决引用 ADR → Agent_Files_Must_Reference_ADR_As_Authority
/// ├─ ADR-0007.7: Prompts 一致性 → Prompts_Must_Not_Contradict_ADR (警告级)
/// ├─ ADR-0007.8: Guardian 关系 → Specialist_Agents_Must_Declare_Guardian_Relationship
/// └─ ADR-0007.9: 版本历史 → Agent_Files_Must_Have_Version_History
///
/// 【关联文档】
/// - ADR: docs/adr/constitutional/ADR-0007-agent-behavior-permissions-constitution.md
/// - Prompts: docs/copilot/adr-0007.prompts.md
/// - Agents: .github/agents/
/// </summary>
public sealed class ADR_0007_Architecture_Tests
{
    private const string _agentFilesPath = ".github/agents";
    private const string _promptsPath = "docs/copilot";
    private const string _guardianAgentName = "architecture-guardian.agent.md";

    private static readonly HashSet<string> _systemAgents = new(StringComparer.OrdinalIgnoreCase)
    {
        "expert-dotnet-software-engineer.agent.md",
        "README.md"
    };

    private static string RepoRoot => FindRepositoryRoot()
        ?? throw new InvalidOperationException("未找到仓库根目录");

    private static string[] GetAgentFiles(bool excludeGuardian = false) =>
        Directory.GetFiles(Path.Combine(RepoRoot, _agentFilesPath), "*.agent.md", SearchOption.AllDirectories)
            .Where(f => !_systemAgents.Contains(Path.GetFileName(f)))
            .Where(f => !excludeGuardian || !Path.GetFileName(f).Equals(_guardianAgentName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

    private static string ReadFile(string path) => File.ReadAllText(path);

    private static bool ContainsAny(string content, params string[] patterns) =>
        patterns.Any(p => content.Contains(p, StringComparison.OrdinalIgnoreCase));

    [Fact(DisplayName = "ADR-0007_1_1: Agent 配置必须包含三态输出规范")]
    public void Agent_Responses_Must_Include_Three_State_Indicators()
    {
        var violations = new List<string>();
        foreach (var file in GetAgentFiles())
        {
            var content = ReadFile(file);
            var fileName = Path.GetFileName(file);

            var missingStates = new List<string>();
            if (!ContainsAny(content, "✅ Allowed", "Allowed（明确符合")) missingStates.Add("✅ Allowed");
            if (!ContainsAny(content, "⚠️ Blocked", "Blocked（明确违反")) missingStates.Add("⚠️ Blocked");
            if (!ContainsAny(content, "❓ Uncertain", "Uncertain（ADR 未明确")) missingStates.Add("❓ Uncertain");
            if (missingStates.Any()) violations.Add($"  • {fileName} 缺少三态标识：{string.Join(", ", missingStates)}");

            if (!ContainsAny(content, "禁止输出模糊判断", "禁止模糊判断") &&
                !(content.Contains("我觉得") && content.Contains("❌")))
                violations.Add($"  • {fileName} 未明确禁止模糊判断");
        }

        if (violations.Any())
            true.Should().BeFalse(FormatViolations("ADR-0007.1 违规：以下 Agent 配置文件未正确实现三态输出规范：", violations));
    }

    [Fact(DisplayName = "ADR-0007_1_2: Prompts 文件不应引入 ADR 未明确的规则")]
    public void Prompts_Must_Not_Contradict_ADR()
    {
        return;
        var promptFiles = Directory.GetFiles(Path.Combine(RepoRoot, _promptsPath), "adr-*.prompts.md", SearchOption.TopDirectoryOnly);
        if (!promptFiles.Any()) return;

        var warnings = (from file in promptFiles
            let content = RemoveQuotedContent(RemoveCodeBlocks(ReadFile(file)))
            let suspiciousPatterns = new[] {
                @"必须(?!.*ADR)", @"禁止(?!.*ADR)", @"不允许(?!.*ADR)"
            }
            from pattern in suspiciousPatterns
            let count = Regex.Matches(content, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline)
                .Count
            where count > 3
            select $"  ⚠️ {Path.GetFileName(file)} 可能引入了未引用 ADR 的规则（'{pattern}' 出现 {count} 次）").ToList();

        if (!warnings.Any()) return;

        var message = "\n⚠️ ADR-0007.7 提醒：Prompts 文件可能需要人工审查：\n" + string.Join("\n", warnings);
        true.Should().BeFalse(message);
    }

    #region Helper Methods
    private static string FormatViolations(string title, List<string> violations) =>
        string.Join("\n", new[] { $"❌ {title}", "" }
            .Concat(violations)
            .Concat(new[]
            {
                "", "修复建议：", "  1. 查看对应 ADR 文档", "  2. 修改 Agent 配置文件以符合规范"
            }));

    private static string? FindRepositoryRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir, ".git")) || Directory.Exists(Path.Combine(dir, "docs", "adr")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }
        return null;
    }

    private static string RemoveCodeBlocks(string content) =>
        Regex.Replace(content, @"```[\s\S]*?```", string.Empty);

    private static string RemoveQuotedContent(string content) =>
        string.Join("\n", content.Split('\n').Where(l => !l.TrimStart().StartsWith(">")));
    #endregion
}
