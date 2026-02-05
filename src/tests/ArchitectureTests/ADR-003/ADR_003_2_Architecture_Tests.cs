namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_003;

/// <summary>
/// ADR-003.2: Platform 命名空间约束
/// 验证 Platform 类型应在 Zss.BilliardHall.Platform 命名空间
/// </summary>
public sealed class ADR_003_2_Architecture_Tests
{
    private const string BaseNamespace = "Zss.BilliardHall";

    /// <summary>
    /// ADR-003_2_1: Platform 类型应在 Zss.BilliardHall.Platform 命名空间
    /// </summary>
    [Fact(DisplayName = "ADR-003_2_1: Platform 类型应在 Zss.BilliardHall.Platform 命名空间")]
    public void ADR_003_2_1_Platform_Types_Should_Have_Platform_Namespace()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var types = Types
            .InAssembly(platformAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<"))
            .ToList();

        foreach (var type in types)
        {
            (type.Namespace?.StartsWith($"{BaseNamespace}.Platform") == true).Should().BeTrue(
                Build(
                    ruleId: "ADR-003_2_1",
                    summary: $"Platform 程序集中的类型应在 {BaseNamespace}.Platform 命名空间下",
                    currentState: $"违规类型: {type.FullName}\n当前命名空间: {type.Namespace}\n期望命名空间前缀: {BaseNamespace}.Platform",
                    remediationSteps: new[]
                    {
                        "确保类型定义在正确的命名空间中",
                        $"Platform 层的所有代码都应该在 {BaseNamespace}.Platform 命名空间下",
                        $"如果是子命名空间，应该是 {BaseNamespace}.Platform.* 格式"
                    },
                    adrReference: "docs/copilot/adr-003.prompts.md (场景 2)"));
        }
    }
}
