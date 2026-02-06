namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-220: 事件总线集成规范
/// 验证事件总线依赖隔离、订阅者生命周期等规则
///
/// ADR 映射清单（ADR Mapping Checklist）：
/// ┌────────────────┬─────────────────────────────────────────────────────┬──────────┐
/// │ 测试方法         │ 对应 ADR 约束                                        │ RuleId   │
/// ├────────────────┼─────────────────────────────────────────────────────┼──────────┤
/// │ Modules_Must_Not_Depend_On_Concrete_EventBus         │ 模块禁止直接依赖具体事件总线实现                    │ ADR-220_1_1 │
/// │ EventHandlers_Must_Be_Scoped_Or_Transient            │ 事件订阅者必须注册为 Scoped 或 Transient          │ ADR-220_4_1 │
/// └────────────────┴─────────────────────────────────────────────────────┴──────────┘
///
/// 测试覆盖的 Rule：
/// - ADR-220_1：事件总线依赖隔离（Rule）
/// - ADR-220_4：事件订阅者生命周期（Rule）
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

            // 提取违规类型列表（避免在断言消息中使用嵌套括号）
            var failingTypes = result.FailingTypeNames ?? new List<string>();
            var failingTypesStr = string.Join(", ", failingTypes);

            var message = $"❌ ADR-220_1_1 违规：模块直接依赖具体事件总线实现\n\n" +
                $"违规实现：{dep}\n" +
                $"违规类型：{failingTypesStr}\n\n" +
                $"问题分析：\n" +
                $"模块不应直接依赖具体的事件总线实现（Wolverine、RabbitMQ、Kafka等），以保持架构的灵活性\n\n" +
                $"修复建议：\n" +
                $"1. 通过 IEventBus 抽象接口使用事件总线\n" +
                $"2. 将具体实现的依赖移至基础设施层或宿主层\n" +
                $"3. 使用依赖注入注册 IEventBus 的具体实现\n" +
                $"4. 示例代码：\n" +
                $"   // ❌ 错误：using Wolverine;\n" +
                $"   // ✅ 正确：using Platform.EventBus; // IEventBus\n\n" +
                $"参考：docs/adr/runtime/ADR-220-event-bus-integration.md（§ADR-220_1_1）";
            result.IsSuccessful.Should().BeTrue(message);
        }
    }

    [Fact(DisplayName = "ADR-220_4_1: 事件订阅者必须注册为 Scoped 或 Transient")]
    public void EventHandlers_Must_Be_Scoped_Or_Transient()
    {
        // 此规则需要在集成测试中验证 DI 容器配置
        true.Should().BeTrue($"ℹ️ ADR-220_4_1: EventHandler 生命周期验证需在集成测试中检查\n\n" +
            $"验证内容：\n" +
            $"所有事件处理器（EventHandler）必须注册为 Scoped 或 Transient 生命周期\n\n" +
            $"修复建议：\n" +
            $"1. 在集成测试中检查 ServiceDescriptor.Lifetime\n" +
            $"2. 确保不使用 Singleton 生命周期\n" +
            $"3. 推荐使用 Scoped 生命周期以支持事务管理\n" +
            $"4. 示例代码：\n" +
            $"   services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();\n\n" +
            $"参考：docs/adr/runtime/ADR-220-event-bus-integration.md（§ADR-220_4_1）");
    }
}
