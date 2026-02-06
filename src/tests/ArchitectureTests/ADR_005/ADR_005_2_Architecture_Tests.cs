namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_005;

/// <summary>
/// 验证 ADR-005_2_1：Handler 不得持有业务状态
/// 验证 ADR-005_2_2：Handler 禁止作为跨模块粘合层
/// 验证 ADR-005_2_3：Handler 不允许返回领域实体
/// </summary>
public sealed class ADR_005_2_Architecture_Tests
{
    [Theory(DisplayName = "ADR-005_2_1: Handler 不得持有业务状态")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_005_2_1_Handler_Must_Not_Hold_Business_State(Assembly moduleAssembly)
    {
        var handlers = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            // 检查是否有可变的实例字段（除了注入的依赖）
            var fields = handler
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => !f.IsInitOnly) // 排除 readonly 字段
                .ToList();

            var message = AssertionMessageBuilder.BuildWithAnalysis(
                ruleId: "ADR-005_2_1",
                summary: "Handler 包含可变字段",
                currentState: $"违规 Handler: {handler.FullName}\n可变字段: {string.Join(", ", fields.Select(f => f.Name))}",
                problemAnalysis: "Handler 包含非 readonly 字段，可能维护长期状态",
                remediationSteps: new[]
                {
                    "将所有依赖字段标记为 readonly",
                    "通过构造函数注入依赖，而非在字段中维护状态",
                    "Handler 应该是短生命周期、无状态、可重入的"
                },
                adrReference: "docs/adr/constitutional/ADR-005-Business-Logic-Layering.md");
            (fields.Count == 0).Should().BeTrue(message);
        }
    }

    [Theory(DisplayName = "ADR-005_2_2: Handler 禁止作为跨模块粘合层")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_005_2_2_Handler_Must_Not_Be_Cross_Module_Glue_Layer(Assembly moduleAssembly)
    {
        // 检查模块是否直接依赖其他模块的 Handler（这表示同步调用）
        var handlers = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            var ctorParams = handler
                .GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .ToList();

            foreach (var param in ctorParams)
            {
                // 检查是否注入了其他模块的类型
                if (param.Namespace != null && param.Namespace.StartsWith("Zss.BilliardHall.Modules"))
                {
                    var currentModule = moduleAssembly.GetName()
                        .Name!
                        .Split('.')
                        .Last();
                    var paramModule = param
                        .Namespace
                        .Split('.')
                        .ElementAtOrDefault(3);

                    if (paramModule != null && paramModule != currentModule)
                    {
                        var message = AssertionMessageBuilder.BuildWithAnalysis(
                            ruleId: "ADR-005_2_2",
                            summary: "Handler 注入了其他模块的类型",
                            currentState: $"违规 Handler: {handler.FullName}\n当前模块: {currentModule}\n依赖模块: {paramModule}\n依赖类型: {param.FullName}",
                            problemAnalysis: "模块间直接注入依赖表示同步调用，违反模块隔离原则",
                            remediationSteps: new[]
                            {
                                "使用异步事件通信: await _eventBus.Publish(new SomeEvent(...))",
                                "或通过契约查询: var dto = await _queryBus.Send(new GetSomeData(...))",
                                "如确需同步调用，必须提交 ADR 破例审批"
                            },
                            adrReference: "docs/adr/constitutional/ADR-005-Business-Logic-Layering.md");
                        true.Should().BeFalse(message);
                    }
                }
            }
        }
    }

    [Theory(DisplayName = "ADR-005_2_3: Handler 不允许返回领域实体")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_005_2_3_Handler_Must_Not_Return_Domain_Entity(Assembly moduleAssembly)
    {
        var handlers = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
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

                // 检查是否返回领域实体
                if (returnType.Namespace != null &&
                    (returnType.Namespace.Contains(".Entities") ||
                     returnType.Namespace.Contains(".Domain") ||
                     returnType.Name.EndsWith("Entity") ||
                     returnType.Name.EndsWith("Aggregate") ||
                     returnType.Name.EndsWith("ValueObject")))
                {
                    var message = AssertionMessageBuilder.BuildWithAnalysis(
                        ruleId: "ADR-005_2_3",
                        summary: "Handler 返回领域实体",
                        currentState: $"违规 Handler: {handler.FullName}\n违规方法: {method.Name}\n返回类型: {returnType.FullName}",
                        problemAnalysis: "Handler 不应返回领域实体（Entity/Aggregate/ValueObject）",
                        remediationSteps: new[]
                        {
                            "创建对应的 DTO 或投影类型",
                            "在 Handler 中映射领域实体到 DTO",
                            "返回 DTO 而非领域实体"
                        },
                        adrReference: "docs/adr/constitutional/ADR-005-Business-Logic-Layering.md");
                    true.Should().BeFalse(message);
                }
            }
        }
    }
}
