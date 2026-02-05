using NetArchTest.Rules;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;
using static Zss.BilliardHall.Tests.ArchitectureTests.Shared.AssertionMessageBuilder;

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

        File.Exists(directoryBuildPropsPath).Should().BeTrue(
            Build(
                ruleId: "ADR-003_6_1",
                summary: "Directory.Build.props 文件应存在于仓库根目录",
                currentState: $"期望路径: {directoryBuildPropsPath}\n文件状态: 不存在",
                remediationSteps: new[]
                {
                    "在仓库根目录创建 Directory.Build.props 文件",
                    "在文件中定义 BaseNamespace（CompanyNamespace + ProductNamespace）",
                    "参考其他项目的 Directory.Build.props 模板"
                },
                adrReference: "docs/copilot/adr-003.prompts.md (场景 1)"));
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
            true.Should().BeFalse(
                BuildSimple(
                    ruleId: "ADR-003_6_2",
                    summary: "Directory.Build.props 文件不存在",
                    currentState: $"文件路径: {directoryBuildPropsPath}",
                    remediation: "首先创建 Directory.Build.props 文件（参考 ADR-003_6_1）",
                    adrReference: "docs/copilot/adr-003.prompts.md (场景 1)"));
        }

        var content = File.ReadAllText(directoryBuildPropsPath);

        (content.Contains("CompanyNamespace") || content.Contains("ProductNamespace") || content.Contains("BaseNamespace")).Should().BeTrue(
            Build(
                ruleId: "ADR-003_6_2",
                summary: "Directory.Build.props 应定义 BaseNamespace 相关属性",
                currentState: $"文件路径: {directoryBuildPropsPath}\n检查结果: 未找到 CompanyNamespace/ProductNamespace/BaseNamespace 定义",
                remediationSteps: new[]
                {
                    "在 Directory.Build.props 中添加 BaseNamespace 定义",
                    "使用以下格式:\n   <CompanyNamespace>Zss</CompanyNamespace>\n   <ProductNamespace>BilliardHall</ProductNamespace>\n   <BaseNamespace>$(CompanyNamespace).$(ProductNamespace)</BaseNamespace>",
                    "确保所有项目都依赖这个统一的 BaseNamespace"
                },
                adrReference: "docs/copilot/adr-003.prompts.md (场景 1, FAQ Q2)"));
    }
}
