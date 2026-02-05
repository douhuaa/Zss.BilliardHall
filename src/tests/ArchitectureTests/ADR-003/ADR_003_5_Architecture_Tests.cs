using NetArchTest.Rules;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;

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
            (type.Namespace?.StartsWith(expectedNamespace) == true).Should().BeTrue($"❌ ADR-003_5_1 违规: Host 程序集中的类型应在 {BaseNamespace}.Host.{{HostName}} 命名空间下\n\n" +
            $"Host 名: {hostName}\n" +
            $"违规类型: {type.FullName}\n" +
            $"当前命名空间: {type.Namespace}\n" +
            $"期望命名空间前缀: {expectedNamespace}\n\n" +
            $"修复建议：\n" +
            $"1. 确保类型定义在正确的命名空间中\n" +
            $"2. Host {hostName} 的所有代码都应该在 {expectedNamespace} 命名空间下\n" +
            $"3. 如果是子命名空间，应该是 {expectedNamespace}.* 格式\n\n" +
            $"参考: docs/copilot/adr-003.prompts.md (场景 1, 场景 2)");
        }
    }
}
