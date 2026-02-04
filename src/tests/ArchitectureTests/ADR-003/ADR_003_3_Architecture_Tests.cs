using NetArchTest.Rules;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;

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
            (type.Namespace?.StartsWith($"{BaseNamespace}.Application") == true).Should().BeTrue($"❌ ADR-003_3_1 违规: Application 程序集中的类型应在 {BaseNamespace}.Application 命名空间下\n\n" +
            $"违规类型: {type.FullName}\n" +
            $"当前命名空间: {type.Namespace}\n" +
            $"期望命名空间前缀: {BaseNamespace}.Application\n\n" +
            $"修复建议:\n" +
            $"1. 确保类型定义在正确的命名空间中\n" +
            $"2. Application 层的所有代码都应该在 {BaseNamespace}.Application 命名空间下\n" +
            $"3. 如果是子命名空间，应该是 {BaseNamespace}.Application.* 格式\n\n" +
            $"参考: docs/copilot/adr-003.prompts.md (场景 2)");
        }
    }
}
