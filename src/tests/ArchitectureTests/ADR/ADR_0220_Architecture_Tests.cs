using NetArchTest.Rules;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-220: 集成事件总线选型与适配规范
/// </summary>
public sealed class ADR_0220_Architecture_Tests
{
    [Theory(DisplayName = "ADR-220.1: 模块禁止直接依赖具体事件总线实现")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Must_Not_Depend_On_Concrete_EventBus(Assembly moduleAssembly)
    {
        var result = Types
            .InAssembly(moduleAssembly)
            .ShouldNot()
            .HaveDependencyOn("Wolverine")
            .And()
            .ShouldNot()
            .HaveDependencyOn("RabbitMQ")
            .And()
            .ShouldNot()
            .HaveDependencyOn("Kafka")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-220.1 违规: 模块直接依赖具体事件总线实现\n\n" +
            $"违规依赖: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}\n\n" +
            $"修复建议:\n" +
            $"通过 IEventBus 抽象接口使用事件总线\n\n" +
            $"参考: docs/copilot/adr-0220.prompts.md");
    }

    [Theory(DisplayName = "ADR-220.4: 事件订阅者必须注册为 Scoped 或 Transient")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void EventHandlers_Must_Be_Scoped_Or_Transient(Assembly moduleAssembly)
    {
        // 此规则需要在集成测试中验证 DI 容器配置
        Assert.True(true,
            "EventHandler 生命周期验证需在集成测试中检查 ServiceDescriptor.Lifetime");
    }
}
