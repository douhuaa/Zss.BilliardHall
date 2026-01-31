using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907.2: 测试目录组织校验
/// 验证测试目录必须按 ADR 编号分组（§2.2）
/// 
/// 测试细则：
/// - 细则 1: ArchitectureTests 目录必须存在
/// - 细则 2: 测试目录必须按 ADR 编号组织
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md
/// - Prompts: docs/copilot/adr-0907.prompts.md
/// </summary>
public sealed class ADR_0907_2_Tests
{
    /// <summary>
    /// 细则 1: 验证 ArchitectureTests 目录存在
    /// </summary>
    [Fact(DisplayName = "ADR-907.2.1: ArchitectureTests 目录必须存在")]
    public void ArchitectureTests_Directory_Must_Exist()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath);

        Directory.Exists(testsDirectory).Should().BeTrue(
            $"❌ ADR-907.2.1 违规：ArchitectureTests 目录不存在\n\n" +
            $"预期路径：{testsDirectory}\n\n" +
            $"修复建议：\n" +
            $"  1. 创建 ArchitectureTests 测试目录\n" +
            $"  2. 按 ADR 编号创建子目录：/ADR-XXXX/\n" +
            $"  3. 每个 ADR 的测试文件放在对应子目录中\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.2");
    }

    /// <summary>
    /// 细则 2: 验证测试目录按 ADR 编号组织
    /// </summary>
    [Fact(DisplayName = "ADR-907.2.2: 测试目录必须按 ADR 编号组织")]
    public void Test_Directory_Must_Be_Organized_By_ADR_Number()
    {
        var repoRoot = ADR_0907_TestHelpers.FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testsDirectory = Path.Combine(repoRoot, ADR_0907_TestHelpers.AdrTestsPath);

        if (!Directory.Exists(testsDirectory))
        {
            // Skip if directory doesn't exist - covered by细则 1
            return;
        }

        // 验证至少存在按 ADR 编号组织的目录
        var adrDirectories = Directory.GetDirectories(testsDirectory, "ADR-*", SearchOption.TopDirectoryOnly);
        var hasAdrDirectory = adrDirectories.Length > 0 || Directory.Exists(Path.Combine(testsDirectory, "ADR"));
        
        hasAdrDirectory.Should().BeTrue(
            $"❌ ADR-907.2.2 违规：未找到按 ADR 编号组织的测试目录\n\n" +
            $"当前路径：{testsDirectory}\n" +
            $"预期格式：ADR-XXXX/ 或 ADR/ADR_XXXX_Architecture_Tests.cs\n\n" +
            $"修复建议：\n" +
            $"  1. 为每个 ADR 创建独立子目录：/ADR-0001/, /ADR-0907/ 等\n" +
            $"  2. 或使用集中式 /ADR/ 目录存放所有测试\n\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md §2.2");
    }
}
