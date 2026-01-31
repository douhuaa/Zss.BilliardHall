using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Enforcement;

/// <summary>
/// 文档权威声明检查 - Enforcement 层测试
/// 
/// 【定位】：执行 ADR-0008 的具体约束
/// 【来源】：ADR-0008 决策 3.1
/// 【执法】：失败 = CI 阻断
/// 
/// 本测试类检查：
/// 1. Instructions/Agents 必须声明权威依据
/// 2. 必须包含冲突裁决规则
/// 
/// 【关联文档】
/// - ADR: docs/adr/constitutional/ADR-0008-documentation-governance-constitution.md
/// - 来源决策: ADR-0008 决策 3.1
/// </summary>
public sealed class DocumentationAuthorityDeclarationTests
{
    [Fact(DisplayName = "Instructions/Agents 必须声明权威依据")]
    public void Instructions_And_Agents_Must_Declare_Authority_Basis()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var violations = new List<string>();

        // 检查 Instructions 文件
        var instructionsDir = Path.Combine(repoRoot, ".github/instructions");
        if (Directory.Exists(instructionsDir))
        {
            var instructionFiles = Directory.GetFiles(instructionsDir, "*.instructions.md", SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase));

            foreach (var file in instructionFiles)
            {
                var content = File.ReadAllText(file);
                var relativePath = Path.GetRelativePath(repoRoot, file);

                var hasAuthorityDeclaration = 
                    content.Contains("权威声明") || 
                    content.Contains("权威依据") ||
                    (content.Contains("基于") && content.Contains("ADR")) ||
                    content.Contains("服从") && content.Contains("ADR");

                var hasConflictResolution = 
                    (content.Contains("冲突") && content.Contains("ADR") && content.Contains("为准")) ||
                    content.Contains("以 ADR 正文为准") ||
                    content.Contains("ADR 正文为唯一");

                if (!hasAuthorityDeclaration)
                {
                    violations.Add($"  • Instructions: {relativePath} - 缺少权威声明");
                }

                if (!hasConflictResolution)
                {
                    violations.Add($"  • Instructions: {relativePath} - 缺少冲突裁决规则");
                }
            }
        }

        // 检查 Agents 文件
        var agentsDir = Path.Combine(repoRoot, ".github/agents");
        if (Directory.Exists(agentsDir))
        {
            var agentFiles = Directory.GetFiles(agentsDir, "*.agent.md", SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
                .Where(f => !f.Contains("expert-dotnet", StringComparison.OrdinalIgnoreCase)); // 排除系统 Agent

            foreach (var file in agentFiles)
            {
                var content = File.ReadAllText(file);
                var relativePath = Path.GetRelativePath(repoRoot, file);

                var hasAuthorityDeclaration = 
                    content.Contains("权威声明") || 
                    (content.Contains("ADR") && (content.Contains("唯一裁决") || content.Contains("唯一权威") || content.Contains("正文为准")));

                var declaresAdrBasis = 
                    Regex.IsMatch(content, @"ADR[-\s]*\d+", RegexOptions.IgnoreCase);

                if (!hasAuthorityDeclaration)
                {
                    violations.Add($"  • Agent: {relativePath} - 缺少 ADR 权威声明");
                }

                if (!declaresAdrBasis)
                {
                    violations.Add($"  • Agent: {relativePath} - 未引用任何 ADR 编号");
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(string.Join("\n", new[]
            {
                "❌ Enforcement 违规：以下治理级文档未正确声明权威依据",
                "",
                "根据 ADR-0008 决策 3.1：Instructions/Agents 必须显式声明所服从的 ADR。",
                ""
            }
            .Concat(violations)
            .Concat(new[]
            {
                "",
                "修复建议：",
                "  1. 在文档开头添加'权威声明'或'权威依据'章节",
                "  2. 明确列出所服从的 ADR 编号（如：本文档基于 ADR-0001, ADR-0005）",
                "  3. 添加冲突裁决规则：'若与 ADR 冲突，以 ADR 正文为准'",
                "",
                "示例：",
                "  ## 权威声明",
                "  ",
                "  本文档服从以下 ADR：",
                "  - ADR-0001: 模块化单体架构",
                "  - ADR-0005: Handler 模式",
                "  ",
                "  > ⚖️ 若本文档与 ADR 正文冲突，以 ADR 正文为准。",
                "",
                "参考：docs/adr/constitutional/ADR-0008-documentation-governance-constitution.md 决策 3.1"
            })));
        }
    }

    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) || 
                Directory.Exists(Path.Combine(currentDir, "docs", "adr")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return null;
    }
}
