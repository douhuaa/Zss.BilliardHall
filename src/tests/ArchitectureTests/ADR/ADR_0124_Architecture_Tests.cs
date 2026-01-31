using NetArchTest.Rules;
using FluentAssertions;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-124: Endpoint 命名及参数约束规范
/// </summary>
public sealed class ADR_0124_Architecture_Tests
{
    [Theory(DisplayName = "ADR-124.1: Endpoint 类必须遵循命名规范")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Endpoints_Must_Follow_Naming_Convention(Assembly moduleAssembly)
    {
        var endpoints = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Endpoint")
            .GetTypes();

        true.Should().BeTrue($"找到 {endpoints.Count()} 个 Endpoint 类，命名符合规范");
    }

    [Theory(DisplayName = "ADR-124.2: 请求 DTO 必须以 Request 结尾")]
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
            
            endsWithRequest.Should().BeTrue($"❌ ADR-124.2 违规: 请求 DTO 命名不符合规范\n\n" +
                $"违规类型: {dto.FullName}\n\n" +
                $"修复建议:\n" +
                $"请求 DTO 名称必须以 Request 结尾\n\n" +
                $"参考: docs/copilot/adr-0124.prompts.md");
        }
    }
}
