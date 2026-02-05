using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_907_A;

/// <summary>
/// ADR-907-A_1: 对齐标准与格式规范
/// 验证 ADR 文档向 Rule/Clause 双层编号体系的对齐要求
///
/// 测试覆盖映射（严格遵循 ADR-907-A v1.2 Rule/Clause 体系）：
/// - ADR-907-A_1_1: 编号格式强制要求 → ADR_907_A_1_1_RuleId_Format_Must_Comply
/// - ADR-907-A_1_2: Decision 章节结构要求 → ADR_907_A_1_2_Decision_Chapter_Structure_Required
/// - ADR-907-A_1_3: Enforcement 章节强制要求 → ADR_907_A_1_3_Enforcement_Chapter_Mandatory
/// - ADR-907-A_1_4: Front Matter 版本更新要求 → ADR_907_A_1_4_Front_Matter_Version_Update
/// - ADR-907-A_1_5: History 章节记录要求 → ADR_907_A_1_5_History_Chapter_Record
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md
/// </summary>
public sealed class ADR_907_A_1_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";

    /// <summary>
    /// ADR-907-A_1_1: 编号格式强制要求
    /// 验证所有 ADR 使用 ADR-XXX_<Rule>_<Clause> 格式
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_1_1: ADR 编号必须符合 Rule/Clause 格式")]
    public void ADR_907_A_1_1_RuleId_Format_Must_Comply()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        var adrFiles = Directory.GetFiles(adrDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\archive\\") && !f.Contains("\\README.md"))
            .ToArray();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 检查是否包含 RuleId 格式声明
            var hasRuleIdDeclaration = content.Contains("ADR-XXX_<Rule>_<Clause>") ||
                                     content.Contains("ADR-XXX_Y_Z");

            hasRuleIdDeclaration.Should().BeTrue(
                $"❌ ADR-907-A_1_1 违规：{fileName} 未声明 RuleId 格式规范\n\n" +
                $"修复建议：\n" +
                $"  在 Decision 章节添加统一铁律：\n" +
                $"  '🔒 统一铁律：ADR-XXX 中，所有可执法条款必须具备稳定 RuleId，格式为：ADR-XXX_<Rule>_<Clause>'\n\n" +
                $"参考：docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md §1.1");

            // 检查是否使用了旧格式
            var hasOldFormat = Regex.IsMatch(content, @"ADR-\d+\.\d+:L\d+");
            hasOldFormat.Should().BeFalse(
                $"❌ ADR-907-A_1_1 违规：{fileName} 仍使用旧编号格式 ADR-X.Y:LZ\n\n" +
                $"修复建议：\n" +
                $"  转换为新格式：ADR-X_Y_Z\n\n" +
                $"参考：docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md §1.1");
        }
    }

    /// <summary>
    /// ADR-907-A_1_2: Decision 章节结构要求
    /// 验证 Decision 章节包含必要结构元素
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_1_2: Decision 章节必须符合标准结构")]
    public void ADR_907_A_1_2_Decision_Chapter_Structure_Required()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        var adrFiles = Directory.GetFiles(adrDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\archive\\") && !f.Contains("\\README.md"))
            .ToArray();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 检查 Decision 章节
            content.Should().Contain("## Decision",
                $"❌ ADR-907-A_1_2 违规：{fileName} 缺少 Decision 章节\n\n" +
                $"修复建议：\n" +
                $"  添加 Decision 章节\n\n" +
                $"参考：docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md §1.2");

            // 检查唯一裁决声明
            content.Should().Contain("唯一裁决",
                $"❌ ADR-907-A_1_2 违规：{fileName} 未声明 Decision 为唯一裁决来源\n\n" +
                $"修复建议：\n" +
                $"  在 Decision 章节开头添加：'⚠️ 本节为唯一裁决来源，所有条款具备执行级别。'\n\n" +
                $"参考：docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md §1.2");

            // 检查统一铁律
            content.Should().Contain("统一铁律",
                $"❌ ADR-907-A_1_2 违规：{fileName} 缺少统一铁律声明\n\n" +
                $"修复建议：\n" +
                $"  添加统一铁律部分\n\n" +
                $"参考：docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md §1.2");
        }
    }

    /// <summary>
    /// ADR-907-A_1_3: Enforcement 章节强制要求
    /// 验证 Enforcement 章节的存在性和完整性
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_1_3: 必须包含完整的 Enforcement 章节")]
    public void ADR_907_A_1_3_Enforcement_Chapter_Mandatory()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        var adrFiles = Directory.GetFiles(adrDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\archive\\") && !f.Contains("\\README.md"))
            .ToArray();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 检查 Enforcement 章节
            content.Should().Contain("## Enforcement",
                $"❌ ADR-907-A_1_3 违规：{fileName} 缺少 Enforcement 章节\n\n" +
                $"修复建议：\n" +
                $"  添加 Enforcement 章节及执法模型表格\n\n" +
                $"参考：docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md §1.3");

            // 检查执法映射表格
            content.Should().Contain("| 规则编号 |",
                $"❌ ADR-907-A_1_3 违规：{fileName} 缺少执法映射表格\n\n" +
                $"修复建议：\n" +
                $"  添加标准执法映射表格：\n" +
                $"  | 规则编号 | 执行级 | 执法方式 | Decision 映射 |\n\n" +
                $"参考：docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md §1.3");
        }
    }

    /// <summary>
    /// ADR-907-A_1_4: Front Matter 版本更新要求
    /// 验证对齐后的 ADR 更新了版本和日期
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_1_4: 对齐后必须更新 Front Matter")]
    public void ADR_907_A_1_4_Front_Matter_Version_Update()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        var adrFiles = Directory.GetFiles(adrDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\archive\\") && !f.Contains("\\README.md"))
            .ToArray();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 检查 Front Matter
            content.Should().Contain("version:",
                $"❌ ADR-907-A_1_4 违规：{fileName} 缺少 version 字段\n\n" +
                $"修复建议：\n" +
                $"  在 Front Matter 中添加 version 字段\n\n" +
                $"参考：docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md §1.4");

            content.Should().Contain("date:",
                $"❌ ADR-907-A_1_4 违规：{fileName} 缺少 date 字段\n\n" +
                $"修复建议：\n" +
                $"  在 Front Matter 中添加 date 字段\n\n" +
                $"参考：docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md §1.4");
        }
    }

    /// <summary>
    /// ADR-907-A_1_5: History 章节记录要求
    /// 验证对齐记录在 History 章节中
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_1_5: 对齐必须记录在 History 中")]
    public void ADR_907_A_1_5_History_Chapter_Record()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        var adrFiles = Directory.GetFiles(adrDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\archive\\") && !f.Contains("\\README.md"))
            .ToArray();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 检查 History 章节
            content.Should().Contain("## History",
                $"❌ ADR-907-A_1_5 违规：{fileName} 缺少 History 章节\n\n" +
                $"修复建议：\n" +
                $"  添加 History 章节\n\n" +
                $"参考：docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md §1.5");

            // 检查对齐记录（如果有对齐）
            if (content.Contains("Rule/Clause") || content.Contains("对齐"))
            {
                content.Should().Contain("对齐 ADR-907",
                    $"❌ ADR-907-A_1_5 违规：{fileName} 缺少对齐记录\n\n" +
                    $"修复建议：\n" +
                    $"  在 History 中添加对齐记录：\n" +
                    $"  '| X.0 | 2026-XX-XX | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |'\n\n" +
                    $"参考：docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md §1.5");
            }
        }
    }
}
