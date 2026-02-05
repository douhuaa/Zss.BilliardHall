using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_940;

/// <summary>
/// ADR-940_1: 关系声明章节要求（Rule）
/// 验证所有 ADR 必须包含关系声明章节
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-940_1_1: 每个 ADR 必须包含关系声明章节
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-940-adr-relationship-traceability-management.md
/// </summary>
public sealed class ADR_940_1_Architecture_Tests
{
    /// <summary>
    /// ADR-940_1_1: 每个 ADR 必须包含关系声明章节
    /// 验证所有 ADR 文档包含 Relationships 章节（§ADR-940_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-940_1_1: 每个 ADR 必须包含关系声明章节")]
    public void ADR_940_1_1_Every_ADR_Must_Have_Relationships_Section()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var violations = new List<string>();

        // 扫描所有 ADR 目录
        var adrDirectories = new[]
        {
            Path.Combine(repoRoot, "docs/adr/constitutional"),
            Path.Combine(repoRoot, "docs/adr/governance"),
            Path.Combine(repoRoot, "docs/adr/runtime"),
            Path.Combine(repoRoot, "docs/adr/structure"),
            Path.Combine(repoRoot, "docs/adr/technical")
        };

        foreach (var dir in adrDirectories)
        {
            if (!Directory.Exists(dir))
                continue;

            var adrFiles = Directory.GetFiles(dir, "ADR-*.md", SearchOption.TopDirectoryOnly)
                .Where(f => !Path.GetFileName(f).StartsWith("ADR-RELATIONSHIP-MAP", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var file in adrFiles)
            {
                var content = File.ReadAllText(file);
                var relativePath = Path.GetRelativePath(repoRoot, file);

                // 检查是否包含 Relationships 章节
                var hasRelationshipsSection = content.Contains("## Relationships", StringComparison.Ordinal) ||
                                             content.Contains("## 关系声明", StringComparison.Ordinal);

                if (!hasRelationshipsSection)
                {
                    violations.Add($"  • {relativePath}");
                }
            }
        }

        if (violations.Any())
        {
            true.Should().BeFalse(string.Join("\n", new[]
            {
                "❌ ADR-940_1_1 违规：以下 ADR 文档缺少 Relationships 章节",
                "",
                "根据 ADR-940_1_1：每个 ADR 必须包含关系声明章节。",
                ""
            }
            .Concat(violations)
            .Concat(new[]
            {
                "",
                "修复建议：在 Decision 章节之后添加 Relationships 章节：",
                "",
                "```markdown",
                "## Relationships（关系声明）",
                "",
                "### Depends On",
                "- [ADR-XXX：标题](./ADR-XXX.md)",
                "",
                "### Depended By",
                "- [ADR-YYY：标题](./ADR-YYY.md)",
                "",
                "### Supersedes",
                "- 无",
                "",
                "### Superseded By",
                "- 无",
                "",
                "### Related",
                "- [ADR-ZZZ：标题](./ADR-ZZZ.md)",
                "```",
                "",
                "参考：docs/adr/governance/ADR-940-adr-relationship-traceability-management.md §ADR-940_1_1"
            })));
        }
    }

    }
    }
}
