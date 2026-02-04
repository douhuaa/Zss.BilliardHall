using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_004;

/// <summary>
/// ADR-004_1: CPM 基础设施约束（Rule）
/// 验证 Central Package Management (CPM) 基础设施配置正确
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-004_1_1: Directory.Packages.props 必须存在
/// - ADR-004_1_2: CPM 必须启用
/// - ADR-004_1_3: 传递依赖固定建议启用
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-004-Cpm-Final.md
/// </summary>
public sealed class ADR_004_1_Architecture_Tests
{
    /// <summary>
    /// ADR-004_1_1: Directory.Packages.props 必须存在
    /// 验证仓库根目录必须包含 Directory.Packages.props 文件（§ADR-004_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-004_1_1: Directory.Packages.props 应存在于仓库根目录")]
    public void ADR_004_1_1_Repository_Should_Have_Directory_Packages_Props()
    {
        var root = TestEnvironment.RepositoryRoot;
        var cpmFile = Path.Combine(root, "Directory.Packages.props");

        File.Exists(cpmFile).Should().BeTrue(
            $"❌ ADR-004_1_1 违规: 仓库根目录必须存在 Directory.Packages.props 文件以启用 Central Package Management (CPM)。\n\n" +
            $"预期路径: {cpmFile}\n\n" +
            $"修复建议:\n" +
            $"1. 在仓库根目录创建 Directory.Packages.props 文件\n" +
            $"2. 添加 <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>\n" +
            $"3. 添加 <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>\n\n" +
            $"参考: docs/adr/constitutional/ADR-004-Cpm-Final.md §ADR-004_1_1");
    }

    /// <summary>
    /// ADR-004_1_2: CPM 必须启用
    /// 验证 Directory.Packages.props 必须包含 ManagePackageVersionsCentrally=true（§ADR-004_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-004_1_2: CPM 应被启用")]
    public void ADR_004_1_2_CPM_Should_Be_Enabled()
    {
        var root = TestEnvironment.RepositoryRoot;
        var cpmFile = Path.Combine(root, "Directory.Packages.props");

        File.Exists(cpmFile).Should().BeTrue("Directory.Packages.props 文件不存在");

        var content = File.ReadAllText(cpmFile);

        content.Contains("ManagePackageVersionsCentrally").Should().BeTrue(
            $"❌ ADR-004_1_2 违规: Directory.Packages.props 必须包含 ManagePackageVersionsCentrally 设置。\n\n" +
            $"当前状态: 未找到 ManagePackageVersionsCentrally 配置\n\n" +
            $"修复建议:\n" +
            $"1. 在 Directory.Packages.props 中添加 <PropertyGroup> 节点\n" +
            $"2. 添加 <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>\n" +
            $"3. 重新构建项目验证配置生效\n\n" +
            $"参考: docs/adr/constitutional/ADR-004-Cpm-Final.md §ADR-004_1_2");

        content.Contains("true").Should().BeTrue(
            $"❌ ADR-004_1_2 违规: Directory.Packages.props 中的 ManagePackageVersionsCentrally 应该设置为 true。\n\n" +
            $"当前状态: ManagePackageVersionsCentrally 值不正确\n\n" +
            $"修复建议:\n" +
            $"1. 确保 <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>\n" +
            $"2. 检查拼写和大小写是否正确\n" +
            $"3. 删除所有项目文件中的手动 Version 属性\n\n" +
            $"参考: docs/adr/constitutional/ADR-004-Cpm-Final.md §ADR-004_1_2");
    }

    /// <summary>
    /// ADR-004_1_3: 传递依赖固定建议启用
    /// 建议启用 CentralPackageTransitivePinningEnabled 以固定传递依赖版本（§ADR-004_1_3）
    /// 注意：此条款为建议性质（L2），不会阻断构建
    /// </summary>
    [Fact(DisplayName = "ADR-004_1_3: CPM 应启用传递依赖固定")]
    public void ADR_004_1_3_CPM_Should_Enable_Transitive_Pinning()
    {
        var root = TestEnvironment.RepositoryRoot;
        var cpmFile = Path.Combine(root, "Directory.Packages.props");

        if (!File.Exists(cpmFile)) return;

        var content = File.ReadAllText(cpmFile);

        // 建议启用但不强制（L2 级别）
        if (content.Contains("CentralPackageTransitivePinningEnabled"))
        {
            content.Contains("<CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>").Should().BeTrue(
                $"⚠️ ADR-004_1_3 建议: 建议启用 CentralPackageTransitivePinningEnabled 以固定传递依赖版本。\n\n" +
                $"当前状态: CentralPackageTransitivePinningEnabled 未设置为 true\n\n" +
                $"修复建议:\n" +
                $"1. 在 Directory.Packages.props 中添加 <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>\n" +
                $"2. 这将确保所有传递依赖使用 CPM 中定义的版本\n" +
                $"3. 避免间接依赖升级导致的破坏性变更\n\n" +
                $"参考: docs/adr/constitutional/ADR-004-Cpm-Final.md §ADR-004_1_3");
        }
    }
}
