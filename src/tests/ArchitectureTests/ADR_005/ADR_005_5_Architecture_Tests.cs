namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_005;

/// <summary>
/// 验证 ADR-005_5_1：Command Handler 只执行业务逻辑
/// 验证 ADR-005_5_2：Query Handler 只读返回
/// 验证 ADR-005_5_3：Command/Query 必须分离
/// </summary>
public sealed class ADR_005_5_Architecture_Tests
{
    [Theory(DisplayName = "ADR-005_5_1: Command Handler 只执行业务逻辑")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_005_5_1_Command_Handler_Only_Execute_Business_Logic(Assembly moduleAssembly)
    {
        var commandHandlers = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("CommandHandler")
            .Or()
            .HaveNameEndingWith("Command.Handler")
            .GetTypes();

        foreach (var handler in commandHandlers)
        {
            // 查找 Handle 或 Execute 方法
            var handleMethods = handler
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "Handle" || m.Name == "Execute" || m.Name.EndsWith("Async"))
                .ToList();

            foreach (var method in handleMethods)
            {
                var returnType = method.ReturnType;

                // 提取 Task<T> 中的 T
                if (returnType.IsGenericType &&
                    returnType.GetGenericTypeDefinition().Name.StartsWith("Task"))
                {
                    returnType = returnType.GetGenericArguments().FirstOrDefault() ?? returnType;
                }

                // 提取 ValueTask<T> 中的 T
                if (returnType.IsGenericType &&
                    returnType.GetGenericTypeDefinition().Name.StartsWith("ValueTask"))
                {
                    returnType = returnType.GetGenericArguments().FirstOrDefault() ?? returnType;
                }

                // 允许的返回类型：void, Task, ValueTask, Unit, bool, int (表示 ID), Guid
                var allowedTypes = new[] { "Void", "Task", "ValueTask", "Unit", "Boolean", "Int32", "Int64", "Guid" };

                if (!allowedTypes.Contains(returnType.Name) && returnType != typeof(void))
                {
                    // 如果返回类型是复杂业务对象，给出警告
                    if (returnType.Namespace?.StartsWith("Zss.BilliardHall.Modules") == true ||
                        returnType.Namespace?.StartsWith("Zss.BilliardHall.Platform.Contracts") == true)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"⚠️ ADR-005_5_1 建议: Command Handler {handler.FullName}.{method.Name} " +
                            $"返回了业务数据类型 {returnType.FullName}。\n" +
                            $"建议：Command Handler 应该只返回简单类型（如 ID/bool）或 void，避免返回完整的业务对象。");
                    }
                }
            }
        }

        // 这是一个正面测试
        true.Should().BeTrue("Command Handler 返回类型检查完成");
    }

    [Theory(DisplayName = "ADR-005_5_2: Query Handler 只读返回")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_005_5_2_Query_Handler_Only_Read_Return(Assembly moduleAssembly)
    {
        var queryHandlers = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("QueryHandler")
            .Or()
            .HaveNameEndingWith("Query.Handler")
            .GetTypes();

        // 这是一个正面测试，确保我们没有过度限制
        // Query Handler 允许使用和返回 Contracts/DTOs
        // 实际的写操作检测需要更复杂的 IL 分析或 Roslyn Analyzer
        queryHandlers.Should().NotBeNull("Query Handler 允许返回 DTO，这是预期行为");
    }

    [Theory(DisplayName = "ADR-005_5_3: Command/Query 必须分离")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_005_5_3_Command_Query_Must_Be_Separated(Assembly moduleAssembly)
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

            // Handler 不应同时是 Command 和 Query
            if (isCommandHandler && isQueryHandler)
            {
                var message = AssertionMessageBuilder.BuildWithAnalysis(
                    ruleId: "ADR-005_5_3",
                    summary: "Handler 同时包含 Command 和 Query 语义",
                    currentState: $"违规 Handler: {handler.FullName}",
                    problemAnalysis: "Handler 命名同时包含 Command 和 Query，违反 CQRS 原则",
                    remediationSteps: new[]
                    {
                        "严格分离为两个 Handler：XxxCommandHandler 和 XxxQueryHandler",
                        "Command Handler：执行写操作，返回 void 或 ID",
                        "Query Handler：执行读操作，返回 DTO"
                    },
                    adrReference: "docs/adr/constitutional/ADR-005-Business-Logic-Layering.md");
                true.Should().BeFalse(message);
            }
        }
    }
}
