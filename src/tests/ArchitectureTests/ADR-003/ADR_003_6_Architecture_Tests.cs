using NetArchTest.Rules;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_003;

/// <summary>
/// ADR-003.6: Directory.Build.props 约束
/// 验证 Directory.Build.props 应存在于仓库根目录并定义 BaseNamespace
/// </summary>
public sealed class ADR_003_6_Architecture_Tests
{
    private const string BaseNamespace = "Zss.BilliardHall";

    /// <summary>
    /// ADR-003_6_1: Directory.Build.props 应存在于仓库根目录
    /// </summary>
    [Fact(DisplayName = "ADR-003_6_1: Directory.Build.props 应存在于仓库根目录")]
    public void ADR_003_6_1_Directory_Build_Props_Should_Exist_At_Repository_Root()
    {
        var root = TestEnvironment.RepositoryRoot;
        var directoryBuildPropsPath = Path.Combine(root, "Directory.Build.props");

        File.Exists(directoryBuildPropsPath).Should().BeTrue($"❌ ADR-003_6_1 违规: Directory.Build.props 文件应存在于仓库根目录\n\n" +
        $"期望路径: {directoryBuildPropsPath}\n" +
        $"当前状态: 文件不存在\n\n" +
        $"修复建议：\n" +
        $"1. 在仓库根目录创建 Directory.Build.props 文件\n" +
        $"2. 在文件中定义 BaseNamespace（CompanyNamespace + ProductNamespace）\n" +
        $"3. 参考其他项目的 Directory.Build.props 模板\n\n" +
        $"参考: docs/copilot/adr-003.prompts.md (场景 1)");
    }

    /// <summary>
    /// ADR-003_6_2: Directory.Build.props 应定义 BaseNamespace
    /// </summary>
    [Fact(DisplayName = "ADR-003_6_2: Directory.Build.props 应定义 BaseNamespace")]
    public void ADR_003_6_2_Directory_Build_Props_Should_Define_Base_Namespace()
    {
        var root = TestEnvironment.RepositoryRoot;
        var directoryBuildPropsPath = Path.Combine(root, "Directory.Build.props");

        if (!File.Exists(directoryBuildPropsPath))
        {
            true.Should().BeFalse($"❌ ADR-003_6_2 违规: Directory.Build.props 文件不存在");
        }

        var content = File.ReadAllText(directoryBuildPropsPath);

        (content.Contains("CompanyNamespace") || content.Contains("ProductNamespace") || content.Contains("BaseNamespace")).Should().BeTrue($"❌ ADR-003_6_2 违规: Directory.Build.props 应定义 BaseNamespace 相关属性\n\n" +
        $"文件路径: {directoryBuildPropsPath}\n" +
        $"当前状态: 未找到 CompanyNamespace/ProductNamespace/BaseNamespace 定义\n\n" +
        $"修复建议：\n" +
        $"1. 在 Directory.Build.props 中添加 BaseNamespace 定义\n" +
        $"2. 使用以下格式:\n" +
        $"   <CompanyNamespace>Zss</CompanyNamespace>\n" +
        $"   <ProductNamespace>BilliardHall</ProductNamespace>\n" +
        $"   <BaseNamespace>$(CompanyNamespace).$(ProductNamespace)</BaseNamespace>\n" +
        $"3. 确保所有项目都依赖这个统一的 BaseNamespace\n\n" +
        $"参考: docs/copilot/adr-003.prompts.md (场景 1, FAQ Q2)");
    }
}
