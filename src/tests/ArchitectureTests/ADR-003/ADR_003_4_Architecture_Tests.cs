using NetArchTest.Rules;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_003;

/// <summary>
/// ADR-003.4: Modules 命名空间约束
/// 验证 Module 类型应在 Zss.BilliardHall.Modules.{ModuleName} 命名空间
/// </summary>
public sealed class ADR_003_4_Architecture_Tests
{
    private const string BaseNamespace = "Zss.BilliardHall";

    /// <summary>
    /// ADR-003_4_1: Module 类型应在 Zss.BilliardHall.Modules.{ModuleName} 命名空间
    /// </summary>
    [Theory(DisplayName = "ADR-003_4_1: Module 类型应在 Zss.BilliardHall.Modules.{ModuleName} 命名空间")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_003_4_1_Module_Types_Should_Have_Module_Namespace(Assembly moduleAssembly)
    {
        var moduleName = moduleAssembly.GetName()
            .Name!
            .Split('.')
            .Last();
        var types = Types
            .InAssembly(moduleAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<"))
            .ToList();

        foreach (var type in types)
        {
            (type.Namespace?.StartsWith($"{BaseNamespace}.Modules.{moduleName}") == true).Should().BeTrue($"❌ ADR-003_4_1 违规: Module 程序集中的类型应在 {BaseNamespace}.Modules.{{ModuleName}} 命名空间下\n\n" +
            $"模块名: {moduleName}\n" +
            $"违规类型: {type.FullName}\n" +
            $"当前命名空间: {type.Namespace}\n" +
            $"期望命名空间前缀: {BaseNamespace}.Modules.{moduleName}\n\n" +
            $"修复建议：\n" +
            $"1. 确保类型定义在正确的命名空间中\n" +
            $"2. 模块 {moduleName} 的所有代码都应该在 {BaseNamespace}.Modules.{moduleName} 命名空间下\n" +
            $"3. 如果是子命名空间，应该是 {BaseNamespace}.Modules.{moduleName}.* 格式（如 .Domain, .UseCases）\n\n" +
            $"参考: docs/copilot/adr-003.prompts.md (场景 2)");
        }
    }
}
