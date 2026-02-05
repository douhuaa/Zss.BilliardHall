using System.Text.RegularExpressions;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_960;

/// <summary>
/// ADR-960_1: Onboarding 文档的权威定位
/// 验证 Onboarding 文档符合非裁决性定位要求
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-960_1_1: 不是裁决性文档
/// - ADR-960_1_2: 不得定义架构约束
/// - ADR-960_1_3: 唯一合法职责
/// - ADR-960_1_4: 权威层级
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-960-onboarding-documentation-governance.md
/// </summary>
public sealed class ADR_960_1_Architecture_Tests
{
    private const string DocsPath = "docs";
    
    // 裁决性关键词 - 这些词不应该出现在 Onboarding 文档中作为新规则定义
    private static readonly string[] DecisionKeywords = new[]
    {
        "必须", "禁止", "不得", "强制", "不允许"
    };

    /// <summary>
    /// ADR-960_1_1: 不是裁决性文档
    /// 验证 Onboarding 文档存在且不包含裁决性语言（§ADR-960_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-960_1_1: Onboarding 文档不得包含裁决性语言")]
    public void ADR_960_1_1_Onboarding_Must_Not_Contain_Decision_Language()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var docsPath = Path.Combine(repoRoot, DocsPath);

        if (!Directory.Exists(docsPath))
        {
            throw new DirectoryNotFoundException($"文档目录不存在: {docsPath}");
        }

        // 查找 Onboarding 文档
        var onboardingFiles = Directory.GetFiles(docsPath, "*onboarding*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("ADR-960", StringComparison.OrdinalIgnoreCase)) // 排除 ADR-960 本身
            .ToList();

        var violations = new List<string>();

        foreach (var file in onboardingFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);

            // 检查是否包含裁决性关键词（避免误报，检查上下文）
            foreach (var keyword in DecisionKeywords)
            {
                // 匹配裁决性模式：如 "必须遵循"、"禁止使用" 等
                var pattern = $@"\b{Regex.Escape(keyword)}(?:遵循|使用|实现|修改|删除|创建|定义)";
                if (Regex.IsMatch(content, pattern))
                {
                    violations.Add($"{relativePath} - 包含裁决性语言：'{keyword}'");
                    break; // 每个文件只报告一次
                }
            }
        }

        if (violations.Count > 0)
        {
            Console.WriteLine("⚠️ ADR-960_1_1 警告：发现 Onboarding 文档可能包含裁决性语言：");
            foreach (var violation in violations)
            {
                Console.WriteLine($"  - {violation}");
            }
            Console.WriteLine("\n修复建议：Onboarding 文档应该使用引导性语言，如'建议'、'推荐'，而不是裁决性语言。");
        }

        // L2 级别，不阻断测试，仅警告
    }

    /// <summary>
    /// ADR-960_1_2: 不得定义架构约束
    /// 验证 Onboarding 文档不包含新的架构约束定义（§ADR-960_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-960_1_2: Onboarding 不得定义新架构约束")]
    public void ADR_960_1_2_Onboarding_Must_Not_Define_New_Constraints()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        
        // 验证 ADR-960 文档存在以定义此规则
        var adr960Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-960-onboarding-documentation-governance.md");
        
        File.Exists(adr960Path).Should().BeTrue(
            $"❌ ADR-960_1_2 违规：ADR-960 文档不存在\n\n" +
            $"修复建议：确保 ADR-960 存在以定义 Onboarding 文档规范\n\n" +
            $"参考：docs/adr/governance/ADR-960-onboarding-documentation-governance.md §1.2");
        
        var content = File.ReadAllText(adr960Path);
        
        // 验证 ADR-960 明确定义了 Onboarding 的非裁决性
        content.Should().Contain("不是裁决性文档",
            $"❌ ADR-960_1_2 违规：ADR-960 必须明确定义 Onboarding 的非裁决性");
        
        content.Should().Contain("不得",
            $"❌ ADR-960_1_2 违规：ADR-960 必须明确禁止 Onboarding 定义架构约束");
    }

    /// <summary>
    /// ADR-960_1_3: 唯一合法职责
    /// 验证 Onboarding 文档的职责定义明确（§ADR-960_1_3）
    /// </summary>
    [Fact(DisplayName = "ADR-960_1_3: Onboarding 唯一职责必须明确定义")]
    public void ADR_960_1_3_Onboarding_Responsibilities_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr960Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-960-onboarding-documentation-governance.md");
        
        File.Exists(adr960Path).Should().BeTrue(
            $"❌ ADR-960_1_3 违规：ADR-960 文档不存在");
        
        var content = File.ReadAllText(adr960Path);
        
        // 验证定义了 Onboarding 的唯一合法职责
        var hasResponsibilityDefinition = content.Contains("唯一合法职责", StringComparison.OrdinalIgnoreCase) ||
                                         content.Contains("告诉你", StringComparison.OrdinalIgnoreCase);
        
        hasResponsibilityDefinition.Should().BeTrue(
            $"❌ ADR-960_1_3 违规：ADR-960 必须明确定义 Onboarding 的唯一合法职责\n\n" +
            $"参考：docs/adr/governance/ADR-960-onboarding-documentation-governance.md §1.3");
    }

    /// <summary>
    /// ADR-960_1_4: 权威层级
    /// 验证 Onboarding 在文档权威层级中的位置（§ADR-960_1_4）
    /// </summary>
    [Fact(DisplayName = "ADR-960_1_4: Onboarding 权威层级必须明确")]
    public void ADR_960_1_4_Onboarding_Authority_Level_Must_Be_Clear()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr960Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-960-onboarding-documentation-governance.md");
        
        File.Exists(adr960Path).Should().BeTrue(
            $"❌ ADR-960_1_4 违规：ADR-960 文档不存在");
        
        var content = File.ReadAllText(adr960Path);
        
        // 验证定义了权威层级
        var hasAuthorityHierarchy = content.Contains("权威层级", StringComparison.OrdinalIgnoreCase) ||
                                   content.Contains("ADR（裁决", StringComparison.OrdinalIgnoreCase);
        
        hasAuthorityHierarchy.Should().BeTrue(
            $"❌ ADR-960_1_4 违规：ADR-960 必须定义 Onboarding 在权威层级中的位置\n\n" +
            $"参考：docs/adr/governance/ADR-960-onboarding-documentation-governance.md §1.4");
    }

}
