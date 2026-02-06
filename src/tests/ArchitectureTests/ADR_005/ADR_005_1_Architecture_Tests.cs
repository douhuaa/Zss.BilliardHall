namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_005;

/// <summary>
/// 验证 ADR-005_1_1：编号格式强制要求
/// 验证 ADR-005_1_2：Decision 章节结构要求
/// </summary>
public sealed class ADR_005_1_Architecture_Tests
{
    [Theory(DisplayName = "ADR-005_1_1: 每个业务用例必须唯一 Handler")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_005_1_1_Each_Use_Case_Must_Have_Unique_Handler(Assembly moduleAssembly)
    {
        var handlers = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            var isCommandHandler = handler.Name.Contains("Command");
            var isQueryHandler = handler.Name.Contains("Query");
            var isEventHandler = handler.Name.Contains("Event");

            var message = AssertionMessageBuilder.BuildWithAnalysis(
                ruleId: "ADR-005_1_1",
                summary: "Handler 命名不清晰",
                currentState: $"违规类型: {handler.FullName}",
                problemAnalysis: "Handler 命名未明确表达业务意图（Command/Query/Event）",
                remediationSteps: new[]
                {
                    "将 Handler 重命名为 *CommandHandler（如 CreateOrderCommandHandler）",
                    "或重命名为 *QueryHandler（如 GetOrderByIdQueryHandler）",
                    "或重命名为 *EventHandler（如 OrderCreatedEventHandler）"
                },
                adrReference: "docs/adr/constitutional/ADR-005-Business-Logic-Layering.md");
            (isCommandHandler || isQueryHandler || isEventHandler).Should().BeTrue(message);
        }
    }

    [Theory(DisplayName = "ADR-005_1_2: Endpoint 仅做请求适配")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_005_1_2_Endpoint_Only_For_Request_Adaptation(Assembly moduleAssembly)
    {
        var endpoints = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Endpoint")
            .Or()
            .HaveNameEndingWith("Controller")
            .GetTypes();

        foreach (var endpoint in endpoints)
        {
            // 检查构造函数依赖数量
            var constructorParams = endpoint
                .GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => !t.Namespace?.StartsWith("Microsoft.Extensions") == true)
                .Where(t => !t.Namespace?.StartsWith("System") == true)
                .ToList();

            var message = AssertionMessageBuilder.BuildWithAnalysis(
                ruleId: "ADR-005_1_2",
                summary: "Endpoint/Controller 包含过多依赖",
                currentState: $"违规类型: {endpoint.FullName}\n构造函数依赖数量: {constructorParams.Count} 个（超过建议的 5 个）",
                problemAnalysis: "Endpoint/Controller 注入过多业务依赖，可能包含业务逻辑",
                remediationSteps: new[]
                {
                    "Endpoint 应只注入 IMessageBus 或类似的协调服务",
                    "将业务逻辑移到 Handler 中实现",
                    "Endpoint 只负责：接收请求 → 映射 Command/Query → 转发给 Handler → 返回响应"
                },
                adrReference: "docs/adr/constitutional/ADR-005-Business-Logic-Layering.md");
            (constructorParams.Count > 5).Should().BeFalse(message);
        }
    }
}
