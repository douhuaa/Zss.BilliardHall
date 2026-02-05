namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_003;

/// <summary>
/// ADR-003.1: 基础命名空间约束
/// 验证所有类型应以 BaseNamespace 开头
/// </summary>
public sealed class ADR_003_1_Architecture_Tests
{
    private const string BaseNamespace = "Zss.BilliardHall";

    /// <summary>
    /// ADR-003_1_1: 所有类型应以 BaseNamespace 开头
    /// </summary>
    [Fact(DisplayName = "ADR-003_1_1: 所有类型应以 BaseNamespace 开头")]
    public void ADR_003_1_1_All_Types_Should_Start_With_Base_Namespace()
    {
        foreach (var assembly in GetAllProjectAssemblies())
        {
            var types = Types
                .InAssembly(assembly)
                .GetTypes()
                .Where(t => !t.Name.StartsWith("<")) // 排除编译器生成的类型
                .Where(t => t.Name != "Program") // 排除顶级语句生成的 Program 类
                .ToList();

            foreach (var type in types)
            {
                (type.Namespace?.StartsWith(BaseNamespace) == true).Should().BeTrue(
                    Build(
                        ruleId: "ADR-003_1_1",
                        summary: "所有类型的命名空间都应以 BaseNamespace 开头",
                        currentState: $"程序集: {assembly.GetName().Name}\n违规类型: {type.FullName}\n当前命名空间: {type.Namespace}\n期望开头: {BaseNamespace}",
                        remediationSteps: new[]
                        {
                            "检查项目的 RootNamespace 是否由 Directory.Build.props 正确推导",
                            "确保项目目录结构符合规范（Platform/Application/Modules/Host/Tests）",
                            "删除项目文件中的手动 RootNamespace 设置"
                        },
                        adrReference: "docs/copilot/adr-003.prompts.md (场景 1, 场景 2)"));
            }
        }
    }

    #region Helper Methods

    private static IEnumerable<Assembly> GetAllProjectAssemblies()
    {
        yield return typeof(Platform.PlatformBootstrapper).Assembly;
        yield return typeof(Application.ApplicationBootstrapper).Assembly;

        foreach (var moduleAssembly in ModuleAssemblyData.ModuleAssemblies)
        {
            yield return moduleAssembly;
        }

        foreach (var hostAssembly in HostAssemblyData.HostAssemblies)
        {
            yield return hostAssembly;
        }
    }

    #endregion
}
