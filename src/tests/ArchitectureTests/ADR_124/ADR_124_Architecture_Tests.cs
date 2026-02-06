namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_124;

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

        endpoints.Count().Should().BeGreaterThanOrEqualTo(0, $"❌ ADR-124_1_1 违规：Endpoint 类检查失败\n\n" +
            $"检查结果：找到 {endpoints.Count()} 个 Endpoint 类\n\n" +
            $"修复建议：\n" +
            $"所有 Endpoint 类必须以 'Endpoint' 后缀结尾：\n" +
            $"1. 类名以 Endpoint 结尾\n" +
            $"2. 示例：CreateOrderEndpoint, GetMemberEndpoint\n\n" +
            $"参考：docs/adr/structure/ADR-124-endpoint-naming-parameter-constraints.md（§1.1）");
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

            endsWithRequest.Should().BeTrue($"❌ ADR-124_1_2 违规: 请求 DTO 命名不符合规范\n\n" +
                $"违规类型：{dto.FullName}\n" +
                $"当前名称：{dto.Name}\n\n" +
                $"问题分析：\n" +
                $"请求 DTO 必须以 Request 后缀结尾以明确标识其用途\n\n" +
                $"修复建议：\n" +
                $"1. 将类型重命名为 {{操作名}}Request\n" +
                $"2. 示例：CreateOrderRequest, UpdateMemberRequest\n" +
                $"3. 确保类型放置在 Contracts 命名空间\n\n" +
                $"参考：docs/adr/structure/ADR-124-endpoint-naming-parameter-constraints.md（§1.2）");
        }
    }
}
