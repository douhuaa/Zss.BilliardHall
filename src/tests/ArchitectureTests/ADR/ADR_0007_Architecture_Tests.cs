using System.Text.RegularExpressions;
using Xunit;

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
    private const string AgentFilesPath = ".github/agents";
    private const string PromptsPath = "docs/copilot";
    private const string GuardianAgentName = "architecture-guardian.agent.md";
    
    // 排除系统级 Agent（GitHub Copilot Workspace 提供的预置 Agent）
    private static readonly HashSet<string> SystemAgents = new(StringComparer.OrdinalIgnoreCase)
    {
        "expert-dotnet-software-engineer.agent.md",
        "README.md"
    };

    [Fact(DisplayName = "ADR-0007.1: Agent 配置必须包含三态输出规范")]
    public void Agent_Responses_Must_Include_Three_State_Indicators()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var agentFilesDir = Path.Combine(repoRoot, AgentFilesPath);
        Assert.True(Directory.Exists(agentFilesDir), $"未找到 Agent 配置目录：{AgentFilesPath}");

        var agentFiles = Directory.GetFiles(agentFilesDir, "*.agent.md", SearchOption.AllDirectories)
            .Where(f => !SystemAgents.Contains(Path.GetFileName(f)))
            .ToArray();
        Assert.NotEmpty(agentFiles);

        var violations = new List<string>();

        foreach (var agentFile in agentFiles)
        {
            var content = File.ReadAllText(agentFile);
            var fileName = Path.GetFileName(agentFile);

            var hasAllowedState = content.Contains("✅ Allowed") || content.Contains("Allowed（明确符合");
            var hasBlockedState = content.Contains("⚠️ Blocked") || content.Contains("Blocked（明确违反");
            var hasUncertainState = content.Contains("❓ Uncertain") || content.Contains("Uncertain（ADR 未明确");

            if (!hasAllowedState || !hasBlockedState || !hasUncertainState)
            {
                var missingStates = new List<string>();
                if (!hasAllowedState) missingStates.Add("✅ Allowed");
                if (!hasBlockedState) missingStates.Add("⚠️ Blocked");
                if (!hasUncertainState) missingStates.Add("❓ Uncertain");
                violations.Add($"  • {fileName} 缺少三态标识：{string.Join(", ", missingStates)}");
            }

            var prohibitsFuzzyJudgment = content.Contains("禁止输出模糊判断") || content.Contains("禁止模糊判断") || (content.Contains("我觉得") && content.Contains("❌"));
            if (!prohibitsFuzzyJudgment)
            {
                violations.Add($"  • {fileName} 未明确禁止模糊判断");
            }
        }

        if (violations.Any())
        {
            Assert.Fail(string.Join("\n", new[] { "❌ ADR-0007.1 违规：以下 Agent 配置文件未正确实现三态输出规范：", "" }.Concat(violations).Concat(new[] { "", "修复建议：", "  1. 在 Agent 配置文件中添加三态输出章节", "  2. 定义 ✅ Allowed、⚠️ Blocked、❓ Uncertain 三种响应状态", "  3. 明确禁止模糊判断", "", "参考：docs/adr/constitutional/ADR-0007-agent-behavior-permissions-constitution.md" })));
        }
    }

    [Fact(DisplayName = "ADR-0007.3: Agent 配置必须禁止批准破例和绕过测试")]
    public void Agent_Files_Must_Prohibit_Architecture_Bypass()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var agentFilesDir = Path.Combine(repoRoot, AgentFilesPath);
        var agentFiles = Directory.GetFiles(agentFilesDir, "*.agent.md", SearchOption.AllDirectories)
            .Where(f => !SystemAgents.Contains(Path.GetFileName(f)))
            .ToArray();
        var violations = new List<string>();

        foreach (var agentFile in agentFiles)
        {
            var content = File.ReadAllText(agentFile);
            var fileName = Path.GetFileName(agentFile);

            var prohibitsException = content.Contains("禁止") && (content.Contains("破例") || content.Contains("例外") || content.Contains("Exception"));
            var prohibitsBypass = content.Contains("禁止") && (content.Contains("绕过") || content.Contains("bypass") || content.Contains("跳过"));

            if (!prohibitsException && !prohibitsBypass)
            {
                violations.Add($"  • {fileName} 未明确禁止批准破例或绕过测试");
            }
        }

        if (violations.Any())
        {
            Assert.Fail(string.Join("\n", new[] { "❌ ADR-0007.3 违规：以下 Agent 配置文件未明确禁止批准破例：", "" }.Concat(violations).Concat(new[] { "", "修复建议：", "  1. 在 Agent 的'禁止行为'或'权限边界'章节中明确禁止批准架构破例", "  2. 明确禁止建议绕过架构测试", "", "参考：docs/adr/constitutional/ADR-0007-agent-behavior-permissions-constitution.md" })));
        }
    }

    [Fact(DisplayName = "ADR-0007.5: Agent 配置必须声明 ADR 正文为唯一权威")]
    public void Agent_Files_Must_Reference_ADR_As_Authority()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var agentFilesDir = Path.Combine(repoRoot, AgentFilesPath);
        var agentFiles = Directory.GetFiles(agentFilesDir, "*.agent.md", SearchOption.AllDirectories)
            .Where(f => !SystemAgents.Contains(Path.GetFileName(f)))
            .ToArray();
        var violations = new List<string>();

        foreach (var agentFile in agentFiles)
        {
            var content = File.ReadAllText(agentFile);
            var fileName = Path.GetFileName(agentFile);

            var hasAuthorityDeclaration = content.Contains("权威声明") || content.Contains("Authority");
            var recognizesAdrAuthority = content.Contains("ADR 正文") && (content.Contains("唯一裁决") || content.Contains("最高权威") || content.Contains("唯一权威"));
            var recognizesPromptsRole = content.Contains("Prompts") && (content.Contains("仅作为") || content.Contains("仅供参考") || content.Contains("不作为判定"));

            if (!hasAuthorityDeclaration || !recognizesAdrAuthority)
            {
                violations.Add($"  • {fileName} 未明确声明 ADR 正文的唯一权威地位");
            }

            if (!recognizesPromptsRole && content.Contains("Prompts"))
            {
                violations.Add($"  • {fileName} 未明确 Prompts 的辅助定位");
            }
        }

        if (violations.Any())
        {
            Assert.Fail(string.Join("\n", new[] { "❌ ADR-0007.5 违规：以下 Agent 配置文件未正确声明权威关系：", "" }.Concat(violations).Concat(new[] { "", "修复建议：", "  1. 在 Agent 配置文件开头添加'权威声明'章节", "  2. 明确声明'ADR 正文为唯一裁决依据'", "  3. 明确 Prompts 仅作为示例和解释辅助", "", "参考：docs/adr/constitutional/ADR-0007-agent-behavior-permissions-constitution.md" })));
        }
    }

    [Fact(DisplayName = "ADR-0007.8: 专业 Agent 必须声明向 Guardian 报告")]
    public void Specialist_Agents_Must_Declare_Guardian_Relationship()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var agentFilesDir = Path.Combine(repoRoot, AgentFilesPath);
        var agentFiles = Directory.GetFiles(agentFilesDir, "*.agent.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals(GuardianAgentName, StringComparison.OrdinalIgnoreCase))
            .Where(f => !SystemAgents.Contains(Path.GetFileName(f)))
            .ToList();

        if (!agentFiles.Any()) return;

        var violations = new List<string>();

        foreach (var agentFile in agentFiles)
        {
            var content = File.ReadAllText(agentFile);
            var fileName = Path.GetFileName(agentFile);

            var declaresGuardianRelationship = content.Contains("与 Guardian 的关系") || content.Contains("向 Guardian 报告") || content.Contains("architecture-guardian");
            var declaresAdr0007Implementation = content.Contains("ADR-0007") && (content.Contains("实例化实现") || content.Contains("Implementation"));

            if (!declaresGuardianRelationship)
            {
                violations.Add($"  • {fileName} 未声明与 Guardian 的关系");
            }

            if (!declaresAdr0007Implementation)
            {
                violations.Add($"  • {fileName} 未声明实现 ADR-0007");
            }
        }

        if (violations.Any())
        {
            Assert.Fail(string.Join("\n", new[] { "❌ ADR-0007.8 违规：以下专业 Agent 配置文件未正确声明 Guardian 关系：", "" }.Concat(violations).Concat(new[] { "", "修复建议：", "  1. 在专业 Agent 配置中添加'与 Guardian 的关系'章节", "  2. 明确声明向 Architecture Guardian 报告", "  3. 添加'本 Agent 是 ADR-0007 的实例化实现'声明", "", "参考：.github/agents/module-boundary-checker.agent.md" })));
        }
    }

    [Fact(DisplayName = "ADR-0007.9: Agent 配置文件必须有版本历史")]
    public void Agent_Files_Must_Have_Version_History()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var agentFilesDir = Path.Combine(repoRoot, AgentFilesPath);
        var agentFiles = Directory.GetFiles(agentFilesDir, "*.agent.md", SearchOption.AllDirectories)
            .Where(f => !SystemAgents.Contains(Path.GetFileName(f)))
            .ToList();
        var violations = new List<string>();

        foreach (var agentFile in agentFiles)
        {
            var content = File.ReadAllText(agentFile);
            var fileName = Path.GetFileName(agentFile);

            var hasVersionHistory = content.Contains("版本历史") || content.Contains("Version History") || content.Contains("## 版本") || content.Contains("## Version");
            var hasVersionMetadata = content.Contains("version:") || content.Contains("版本：");

            if (!hasVersionHistory || !hasVersionMetadata)
            {
                var missing = new List<string>();
                if (!hasVersionHistory) missing.Add("版本历史章节");
                if (!hasVersionMetadata) missing.Add("版本元数据");
                violations.Add($"  • {fileName} 缺少：{string.Join("、", missing)}");
            }
        }

        if (violations.Any())
        {
            Assert.Fail(string.Join("\n", new[] { "❌ ADR-0007.9 违规：以下 Agent 配置文件缺少版本历史：", "" }.Concat(violations).Concat(new[] { "", "修复建议：", "  1. 在 Agent 配置文件的 frontmatter 中添加 version 字段", "  2. 在文件末尾添加'版本历史'章节", "  3. 记录每次变更的版本号、日期和变更说明", "", "参考：.github/agents/architecture-guardian.agent.md" })));
        }
    }

    [Fact(DisplayName = "ADR-0007.8: Guardian 配置必须列出所有专业 Agent")]
    public void Guardian_Must_List_All_Specialist_Agents()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var agentFilesDir = Path.Combine(repoRoot, AgentFilesPath);
        var guardianFile = Path.Combine(agentFilesDir, GuardianAgentName);
        Assert.True(File.Exists(guardianFile), $"未找到 Guardian 配置文件：{GuardianAgentName}");

        var guardianContent = File.ReadAllText(guardianFile);
        var specialistAgents = Directory.GetFiles(agentFilesDir, "*.agent.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals(GuardianAgentName, StringComparison.OrdinalIgnoreCase))
            .Where(f => !SystemAgents.Contains(Path.GetFileName(f)))
            .Select(f => {
                var nameWithoutMd = Path.GetFileNameWithoutExtension(f); // "xxx.agent"
                // 移除 .agent 后缀得到纯名称
                return nameWithoutMd?.EndsWith(".agent", StringComparison.OrdinalIgnoreCase) == true 
                    ? nameWithoutMd.Substring(0, nameWithoutMd.Length - 6) 
                    : nameWithoutMd;
            })
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();

        if (!specialistAgents.Any()) return;

        var missingAgents = new List<string>();

        foreach (var agentName in specialistAgents)
        {
            if (!guardianContent.Contains(agentName!, StringComparison.OrdinalIgnoreCase))
            {
                missingAgents.Add($"  • {agentName}");
            }
        }

        if (missingAgents.Any())
        {
            Assert.Fail(string.Join("\n", new[] { "❌ ADR-0007.8 违规：Guardian 配置文件未列出以下专业 Agent：", "" }.Concat(missingAgents).Concat(new[] { "", "修复建议：", "  1. 在 Guardian 的'专业 Agent 协调'或类似章节中列出所有专业 Agent", "  2. 说明 Guardian 与每个专业 Agent 的协作关系", "", $"参考：{GuardianAgentName}" })));
        }
    }

    [Fact(DisplayName = "ADR-0007.7: Prompts 文件不应引入 ADR 未明确的规则")]
    public void Prompts_Must_Not_Contradict_ADR()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var promptsDir = Path.Combine(repoRoot, PromptsPath);

        if (!Directory.Exists(promptsDir)) return;

        var promptFiles = Directory.GetFiles(promptsDir, "adr-*.prompts.md", SearchOption.TopDirectoryOnly);
        if (promptFiles.Length == 0) return;

        var warnings = new List<string>();

        foreach (var promptFile in promptFiles)
        {
            var content = File.ReadAllText(promptFile);
            var fileName = Path.GetFileName(promptFile);

            var suspiciousPatterns = new[] { @"必须(?!.*ADR)", @"禁止(?!.*ADR)", @"不允许(?!.*ADR)" };
            var contentWithoutCodeBlocks = RemoveCodeBlocks(content);
            var contentWithoutQuotes = RemoveQuotedContent(contentWithoutCodeBlocks);

            foreach (var pattern in suspiciousPatterns)
            {
                var matches = Regex.Matches(contentWithoutQuotes, pattern);
                if (matches.Count > 3)
                {
                    warnings.Add($"  ⚠️ {fileName} 可能引入了未引用 ADR 的规则（'{pattern}' 出现 {matches.Count} 次）");
                }
            }
        }

        if (warnings.Any())
        {
            Console.WriteLine(string.Join("\n", new[] { "", "⚠️ ADR-0007.7 提醒：以下 Prompts 文件可能需要人工审查：", "" }.Concat(warnings).Concat(new[] { "", "建议：", "  • 确保 Prompts 中的规则都引用了对应的 ADR 编号", "  • Prompts 应该是'场景示例'，而非'新规则'", "", "注意：这是启发式检查，最终需要人工审查", "" })));
        }
    }

    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) || Directory.Exists(Path.Combine(currentDir, "docs", "adr")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return null;
    }

    private static string RemoveCodeBlocks(string content)
    {
        var pattern = @"```[\s\S]*?```";
        return Regex.Replace(content, pattern, string.Empty);
    }

    private static string RemoveQuotedContent(string content)
    {
        var lines = content.Split('\n');
        var nonQuotedLines = lines.Where(line => !line.TrimStart().StartsWith(">"));
        return string.Join("\n", nonQuotedLines);
    }
}
