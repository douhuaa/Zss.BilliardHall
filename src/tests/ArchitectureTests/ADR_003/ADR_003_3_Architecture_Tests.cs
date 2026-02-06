namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_003;

/// <summary>
/// ADR-003.3: Application 命名空间约束
/// 验证 Application 类型应在 Zss.BilliardHall.Application 命名空间
/// </summary>
public sealed class ADR_003_3_Architecture_Tests
{
    private const string BaseNamespace = "Zss.BilliardHall";

    /// <summary>
    /// ADR-003_3_1: Application 类型应在 Zss.BilliardHall.Application 命名空间
    /// </summary>
    [Fact(DisplayName = "ADR-003_3_1: Application 类型应在 Zss.BilliardHall.Application 命名空间")]
    public void ADR_003_3_1_Application_Types_Should_Have_Application_Namespace()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var types = Types
            .InAssembly(applicationAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<"))
            .ToList();

        foreach (var type in types)
        {
            (type.Namespace?.StartsWith($"{BaseNamespace}.Application") == true).Should().BeTrue(
                Build(
                    ruleId: "ADR-003_3_1",
                    summary: $"Application 程序集中的类型应在 {BaseNamespace}.Application 命名空间下",
                    currentState: $"违规类型: {type.FullName}\n当前命名空间: {type.Namespace}\n期望命名空间前缀: {BaseNamespace}.Application",
                    remediationSteps: new[]
                    {
                        "确保类型定义在正确的命名空间中",
                        $"Application 层的所有代码都应该在 {BaseNamespace}.Application 命名空间下",
                        $"如果是子命名空间，应该是 {BaseNamespace}.Application.* 格式"
                    },
                    adrReference: "docs/copilot/adr-003.prompts.md (场景 2)"));
        }
    }
}
