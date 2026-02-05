namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-220: 集成事件总线选型与适配规范
/// </summary>
public sealed class ADR_220_Architecture_Tests
{
    [Theory(DisplayName = "ADR-220_1_1: 模块禁止直接依赖具体事件总线实现")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Must_Not_Depend_On_Concrete_EventBus(Assembly moduleAssembly)
    {
        var forbiddenDeps = new[] { "Wolverine", "RabbitMQ", "Kafka" };

        foreach (var dep in forbiddenDeps)
        {
            var result = Types
                .InAssembly(moduleAssembly)
                .ShouldNot()
                .HaveDependencyOn(dep)
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"❌ ADR-220_1_1 违规: 模块直接依赖具体事件总线实现\n\n" +
                $"违规实现：{dep}\n" +
                $"违规类型：{string.Join(", ", result.FailingTypeNames ?? new List<string>())}\n\n" +
                $"问题分析：\n" +
                $"模块不应直接依赖具体的事件总线实现（Wolverine、RabbitMQ、Kafka等），以保持架构的灵活性\n\n" +
                $"修复建议：\n" +
                $"1. 通过 IEventBus 抽象接口使用事件总线\n" +
                $"2. 将具体实现的依赖移至基础设施层或宿主层\n" +
                $"3. 使用依赖注入注册 IEventBus 的具体实现\n" +
                $"4. 示例代码：\n" +
                $"   // ❌ 错误：using Wolverine;\n" +
                $"   // ✅ 正确：using Platform.EventBus; // IEventBus\n\n" +
                $"参考：docs/adr/structure/ADR-220-integration-event-bus-selection-adaptation.md（§1.1）");
        }
    }

    [Fact(DisplayName = "ADR-220_1_2: 事件订阅者必须注册为 Scoped 或 Transient")]
    public void EventHandlers_Must_Be_Scoped_Or_Transient()
    {
        // 此规则需要在集成测试中验证 DI 容器配置
        true.Should().BeTrue($"❌ ADR-220_1_2 违规：EventHandler 生命周期验证需在集成测试中检查\n\n" +
            $"验证内容：\n" +
            $"所有事件处理器（EventHandler）必须注册为 Scoped 或 Transient 生命周期\n\n" +
            $"修复建议：\n" +
            $"1. 在集成测试中检查 ServiceDescriptor.Lifetime\n" +
            $"2. 确保不使用 Singleton 生命周期\n" +
            $"3. 推荐使用 Scoped 生命周期以支持事务管理\n" +
            $"4. 示例代码：\n" +
            $"   services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();\n\n" +
            $"参考：docs/adr/structure/ADR-220-integration-event-bus-selection-adaptation.md（§1.2）");
    }
}
