using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.1: ArchitectureTests 项目存在性校验
/// 验证解决方案中必须存在独立的 ArchitectureTests 项目（§2.1）
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_1_Tests
{
    [Fact(DisplayName = "ADR-907.1: ArchitectureTests 项目必须存在")]
    public void ArchitectureTests_Project_Must_Exist()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var projectPath = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsProjectPath);

        File.Exists(projectPath).Should().BeTrue(
            $"❌ ADR-907.1 违规：ArchitectureTests 项目不存在\n\n" +
            $"预期路径：{projectPath}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建独立的 ArchitectureTests 测试项目\n" +
            $"  2. 项目命名格式：<SolutionName>.Tests.Architecture\n" +
            $"  3. 确保项目位于 src/tests 目录下\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.1");
    }
}
