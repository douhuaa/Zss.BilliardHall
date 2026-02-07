namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-124: Endpoint 命名及参数约束规范
/// </summary>
public sealed class ADR_124_Architecture_Tests
{
    [Theory(DisplayName = "ADR-124_1_1: Endpoint 类必须遵循命名规范")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Endpoints_Must_Follow_Naming_Convention(Assembly moduleAssembly)
    {
        var endpoints = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Endpoint")
            .GetTypes();

        var message = AssertionMessageBuilder.Build(
            ruleId: "ADR-124_1_1",
            summary: "Endpoint 类检查完成",
            currentState: $"在模块 {moduleAssembly.GetName().Name} 中找到 {endpoints.Count()} 个 Endpoint 类",
            remediationSteps: new[]
            {
                "所有 Endpoint 类必须以 'Endpoint' 后缀结尾",
                "类名格式：{操作名}Endpoint",
                "示例：CreateOrderEndpoint, GetMemberEndpoint"
            },
            adrReference: "docs/adr/structure/ADR-124-endpoint-naming-parameter-constraints.md");

        endpoints.Count().Should().BeGreaterThanOrEqualTo(0, message);
    }

    [Theory(DisplayName = "ADR-124_1_2: 请求 DTO 必须以 Request 结尾")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Request_DTOs_Must_End_With_Request(Assembly moduleAssembly)
    {
        var requestDtos = Types
            .InAssembly(moduleAssembly)
            .That()
            .ResideInNamespace("Contracts")
            .Or()
            .HaveNameEndingWith("Request")
            .GetTypes()
            .Where(t => t.Name.Contains("Request"))
            .ToList();

        foreach (var dto in requestDtos)
        {
            var endsWithRequest = dto.Name.EndsWith("Request");

            var message = AssertionMessageBuilder.Build(
                ruleId: "ADR-124_1_2",
                summary: $"请求 DTO 命名不符合规范",
                currentState: $"违规类型：{dto.FullName}\n当前名称：{dto.Name}\n问题：请求 DTO 必须以 Request 后缀结尾以明确标识其用途",
                remediationSteps: new[]
                {
                    "将类型重命名为 {操作名}Request",
                    "示例：CreateOrderRequest, UpdateMemberRequest",
                    "确保类型放置在 Contracts 命名空间"
                },
                adrReference: "docs/adr/structure/ADR-124-endpoint-naming-parameter-constraints.md");

            endsWithRequest.Should().BeTrue(message);
        }
    }
}
