using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.10: CI / Analyzer 自动注册校验
/// 验证所有 ArchitectureTests 必须被 Analyzer 自动发现并注册（§4.1）
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_10_Tests
{
    [Fact(DisplayName = "ADR-907.10: 架构测试必须可被 CI 发现")]
    public void Tests_Must_Be_Discoverable_By_CI()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var projectPath = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsProjectPath);

        File.Exists(projectPath).Should().BeTrue(
            $"❌ ADR-907.10 无法执行：项目文件不存在 {projectPath}");

        var projectContent = File.ReadAllText(projectPath);

        // 验证项目包含必要的测试框架引用
        var hasXunit = projectContent.Contains("xunit") || projectContent.Contains("Xunit");
        var hasNetArchTest = projectContent.Contains("NetArchTest");

        var warnings = new List<string>();
        if (!hasXunit)
        {
            warnings.Add("  • 缺少 xUnit 测试框架引用");
        }
        if (!hasNetArchTest)
        {
            warnings.Add("  • 缺少 NetArchTest.Rules 引用");
        }

        // 检查是否有 CI 配置文件
        var githubWorkflowsPath = Path.Combine(repoRoot, ".github", "workflows");
        var hasCiConfig = Directory.Exists(githubWorkflowsPath);

        if (!hasCiConfig)
        {
            warnings.Add("  • 缺少 CI 配置（.github/workflows）");
        }

        if (warnings.Any())
        {
            var message = string.Join("\n",
                new[]
                {
                    "",
                    "⚠️  ADR-907.10 提醒：架构测试的 CI 集成可能不完整：",
                    ""
                }
                .Concat(warnings)
                .Concat(new[]
                {
                    "",
                    "建议：",
                    "  • 确保项目引用了必要的测试框架",
                    "  • 配置 CI 管道自动运行架构测试",
                    "  • 测试失败应阻断合并",
                    "",
                    "参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §4.1",
                    ""
                }));

            Console.WriteLine(message);
        }
    }
}
