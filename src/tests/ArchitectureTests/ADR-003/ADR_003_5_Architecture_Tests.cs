namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_003;

/// <summary>
/// ADR-003.5: Host 命名空间约束
/// 验证 Host 类型应在 Zss.BilliardHall.Host.{HostName} 命名空间
/// </summary>
public sealed class ADR_003_5_Architecture_Tests
{
    private const string BaseNamespace = "Zss.BilliardHall";

    /// <summary>
    /// ADR-003_5_1: Host 类型应在 Zss.BilliardHall.Host.{HostName} 命名空间
    /// </summary>
    [Theory(DisplayName = "ADR-003_5_1: Host 类型应在 Zss.BilliardHall.Host.{HostName} 命名空间")]
    [ClassData(typeof(HostAssemblyData))]
    public void ADR_003_5_1_Host_Types_Should_Have_Host_Namespace(Assembly hostAssembly)
    {
        var hostName = hostAssembly.GetName()
            .Name!
            .Split('.')
            .Last();
        var types = Types
            .InAssembly(hostAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<"))
            .Where(t => t.Name != "Program") // 排除顶级语句生成的 Program 类
            .ToList();

        foreach (var type in types)
        {
            var expectedNamespace = $"{BaseNamespace}.Host.{hostName}";
            (type.Namespace?.StartsWith(expectedNamespace) == true).Should().BeTrue(
                Build(
                    ruleId: "ADR-003_5_1",
                    summary: $"Host 程序集中的类型应在 {BaseNamespace}.Host.{{HostName}} 命名空间下",
                    currentState: $"Host 名: {hostName}\n违规类型: {type.FullName}\n当前命名空间: {type.Namespace}\n期望命名空间前缀: {expectedNamespace}",
                    remediationSteps: new[]
                    {
                        "确保类型定义在正确的命名空间中",
                        $"Host {hostName} 的所有代码都应该在 {expectedNamespace} 命名空间下",
                        $"如果是子命名空间，应该是 {expectedNamespace}.* 格式"
                    },
                    adrReference: "docs/copilot/adr-003.prompts.md (场景 1, 场景 2)"));
        }
    }
}
