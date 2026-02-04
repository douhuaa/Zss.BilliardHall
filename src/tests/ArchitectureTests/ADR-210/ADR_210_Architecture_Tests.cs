using NetArchTest.Rules;
using FluentAssertions;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-210: 领域事件版本化与兼容性
/// </summary>
public sealed class ADR_210_Architecture_Tests
{
    [Theory(DisplayName = "ADR-210_1_1: 事件必须包含 SchemaVersion 属性")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Events_Must_Have_SchemaVersion_Property(Assembly moduleAssembly)
    {
        var eventTypes = Types
            .InAssembly(moduleAssembly)
            .That()
            .ResideInNamespace("DomainEvents")
            .Or()
            .HaveNameEndingWith("Event")
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var eventType in eventTypes)
        {
            var hasSchemaVersion = eventType.GetProperty("SchemaVersion") != null;
            
            hasSchemaVersion.Should().BeTrue($"❌ ADR-210_1_1 违规: 事件缺少 SchemaVersion 属性\n\n" +
                $"违规类型: {eventType.FullName}\n\n" +
                $"修复建议:\n" +
                $"添加 public string SchemaVersion {{ get; init; }} = \"1.0\";\n\n" +
                $"参考: docs/copilot/adr-210.prompts.md");
        }
    }
}
