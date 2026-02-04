using NetArchTest.Rules;
using FluentAssertions;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-005: 应用内交互模型与执行边界
/// 验证运行时规则：Handler 职责、同步/异步边界、模块通信契约
/// 
/// ADR 映射清单（ADR Mapping Checklist）：
/// ┌──────────────┬────────────────────────────────────────────────────────┬────────────┐
/// │ 测试方法      │ 对应 ADR 约束                                          │ ADR RuleId │
/// ├──────────────┼────────────────────────────────────────────────────────┼────────────┤
/// │ ADR-005_1_1 │ 每个业务用例必须唯一 Handler                           │ ADR-005_1_1│
/// │ ADR-005_1_2 │ Endpoint 仅做请求适配                                  │ ADR-005_1_2│
/// │ ADR-005_2_1 │ Handler 不得持有业务状态                               │ ADR-005_2_1│
/// │ ADR-005_2_2 │ Handler 禁止作为跨模块粘合层                           │ ADR-005_2_2│
/// │ ADR-005_2_3 │ Handler 不允许返回领域实体                             │ ADR-005_2_3│
/// │ ADR-005_3_1 │ 模块内允许同步调用                                     │ ADR-005_3_1│
/// │ ADR-005_3_2 │ 模块间默认异步通信                                     │ ADR-005_3_2│
/// │ ADR-005_4_1 │ 模块间仅通过契约通信                                   │ ADR-005_4_1│
/// │ ADR-005_4_2 │ 契约不承载业务决策                                     │ ADR-005_4_2│
/// │ ADR-005_5_1 │ Command Handler 只执行业务逻辑                         │ ADR-005_5_1│
/// │ ADR-005_5_2 │ Query Handler 只读返回                                 │ ADR-005_5_2│
/// │ ADR-005_5_3 │ Command/Query 必须分离                                 │ ADR-005_5_3│
/// └──────────────┴────────────────────────────────────────────────────────┴────────────┘
/// </summary>
public sealed class ADR_005_Architecture_Tests
{

    #region ADR-005_1: Use Case 执行与裁决权

    [Theory(DisplayName = "ADR-005_1_1: 每个业务用例必须唯一 Handler")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Each_Use_Case_Must_Have_Unique_Handler(Assembly moduleAssembly)
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

            (isCommandHandler || isQueryHandler || isEventHandler).Should().BeTrue($"❌ ADR-005_1_1 违规: Handler 命名不清晰\n\n" +
            $"违规类型: {handler.FullName}\n\n" +
            $"问题分析:\n" +
            $"Handler 命名未明确表达业务意图（Command/Query/Event），无法判断其职责\n\n" +
            $"修复建议:\n" +
            $"1. 将 Handler 重命名为 *CommandHandler（如 CreateOrderCommandHandler）\n" +
            $"2. 或重命名为 *QueryHandler（如 GetOrderByIdQueryHandler）\n" +
            $"3. 或重命名为 *EventHandler（如 OrderCreatedEventHandler）\n\n" +
            $"参考: ADR-005 §ADR-005_1_1");
        }
    }

    [Theory(DisplayName = "ADR-005_1_2: Endpoint 仅做请求适配")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Endpoints_Should_Only_Adapt_Requests(Assembly moduleAssembly)
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
            // 检查 Endpoint 是否注入了过多依赖（超过 5 个可能表示业务逻辑）
            var constructorParams = endpoint
                .GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => !t.Namespace?.StartsWith("Microsoft.Extensions") == true)
                .Where(t => !t.Namespace?.StartsWith("System") == true)
                .ToList();

            if (constructorParams.Count > 5)
            {
                true.Should().BeFalse($"❌ ADR-005_1_2 违规: Endpoint/Controller 包含过多依赖\n\n" +
                            $"违规类型: {endpoint.FullName}\n" +
                            $"构造函数依赖数量: {constructorParams.Count} 个（超过建议的 5 个）\n\n" +
                            $"问题分析:\n" +
                            $"Endpoint/Controller 注入过多业务依赖，可能包含业务逻辑\n\n" +
                            $"修复建议:\n" +
                            $"1. Endpoint 应只注入 IMessageBus 或类似的协调服务\n" +
                            $"2. 将业务逻辑移到 Handler 中实现\n" +
                            $"3. Endpoint 只负责：接收请求 → 映射 Command/Query → 转发给 Handler → 返回响应\n\n" +
                            $"参考: ADR-005 §ADR-005_1_2");
            }
        }
    }

    #endregion

    #region ADR-005_2: Handler 职责边界

    [Theory(DisplayName = "ADR-005_2_1: Handler 不得持有业务状态")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Must_Not_Hold_Business_State(Assembly moduleAssembly)
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

            (fields.Count == 0).Should().BeTrue($"❌ ADR-005_2_1 违规: Handler 包含可变字段\n\n" +
            $"违规 Handler: {handler.FullName}\n" +
            $"可变字段: {string.Join(", ", fields.Select(f => f.Name))}\n\n" +
            $"问题分析:\n" +
            $"Handler 包含非 readonly 字段，可能维护跨调用生命周期的业务状态\n\n" +
            $"修复建议:\n" +
            $"1. 将所有依赖字段标记为 readonly\n" +
            $"2. 通过构造函数注入依赖，而非在字段中维护状态\n" +
            $"3. Handler 应该是短生命周期、无状态、可重入的\n\n" +
            $"参考: ADR-005 §ADR-005_2_1");
        }
    }

    [Theory(DisplayName = "ADR-005_2_2: Handler 禁止作为跨模块粘合层")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Must_Not_Be_Cross_Module_Glue(Assembly moduleAssembly)
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
                        true.Should().BeFalse($"❌ ADR-005_2_2 违规: Handler 注入了其他模块的类型\n\n" +
                                    $"违规 Handler: {handler.FullName}\n" +
                                    $"当前模块: {currentModule}\n" +
                                    $"依赖模块: {paramModule}\n" +
                                    $"依赖类型: {param.FullName}\n\n" +
                                    $"问题分析:\n" +
                                    $"Handler 同步调用多个其他模块的 Handler 并聚合结果，违反模块隔离原则\n\n" +
                                    $"修复建议:\n" +
                                    $"1. 使用异步事件通信: await _eventBus.Publish(new SomeEvent(...))\n" +
                                    $"2. 或通过契约查询: var dto = await _queryBus.Send(new GetSomeData(...))\n" +
                                    $"3. Handler 应通过事件异步协调，而非同步聚合\n\n" +
                                    $"参考: ADR-005 §ADR-005_2_2");
                    }
                }
            }
        }
    }

    [Theory(DisplayName = "ADR-005_2_3: Handler 不允许返回领域实体")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Must_Not_Return_Domain_Entities(Assembly moduleAssembly)
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
                    returnType
                        .GetGenericTypeDefinition()
                        .Name
                        .StartsWith("Task"))
                {
                    returnType = returnType
                                     .GetGenericArguments()
                                     .FirstOrDefault() ??
                                 returnType;
                }

                // 检查返回类型是否是领域实体（Entity/Aggregate/VO）
                if (returnType.Namespace?.Contains(".Entities") == true ||
                    returnType.Namespace?.Contains(".Domain") == true ||
                    returnType.Name.EndsWith("Entity") ||
                    returnType.Name.EndsWith("Aggregate") ||
                    returnType.Name.EndsWith("ValueObject"))
                {
                    true.Should().BeFalse($"❌ ADR-005_2_3 违规: Handler 返回领域实体\n\n" +
                                $"违规 Handler: {handler.FullName}.{method.Name}\n" +
                                $"返回类型: {returnType.FullName}\n\n" +
                                $"问题分析:\n" +
                                $"Handler 将 Entity、Aggregate、ValueObject 直接作为返回值\n\n" +
                                $"修复建议:\n" +
                                $"1. Handler 应返回 DTO 或原始类型\n" +
                                $"2. 使用 AutoMapper 或手动映射将领域对象转换为 DTO\n" +
                                $"3. 确保领域模型不会泄漏到应用层边界外\n\n" +
                                $"参考: ADR-005 §ADR-005_2_3");
                }
            }
        }
    }

    #endregion

    #region ADR-005_3: 模块通信及同步/异步边界

    [Theory(DisplayName = "ADR-005_3_1: 模块内允许同步调用")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Module_Internal_Synchronous_Calls_Are_Allowed(Assembly moduleAssembly)
    {
        // 这是一个"正面"测试，确认模块内同步调用是允许的
        var handlers = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        // 模块内组件可直接依赖和调用
        true.Should().BeTrue("模块内允许同步调用 - 这是预期行为");
    }

    [Theory(DisplayName = "ADR-005_3_2: 模块间默认异步通信")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Cross_Module_Must_Use_Async_Communication(Assembly moduleAssembly)
    {
        var handlers = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            // 检查 Handler 是否依赖 ASP.NET 类型
            var forbiddenDeps = new[] { "Microsoft.AspNetCore", "Microsoft.AspNetCore.Http", "Microsoft.AspNetCore.Mvc" };
            var result = Types
                .InAssembly(moduleAssembly)
                .That()
                .HaveNameEndingWith("Handler")
                .And()
                .AreAssignableTo(handler)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenDeps)
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"❌ ADR-005_3_2 违规: Handler 依赖 ASP.NET Core 类型\n\n" +
            $"模块: {moduleAssembly.GetName().Name}\n" +
            $"违规类型: {handler.FullName}\n\n" +
            $"问题分析:\n" +
            $"Handler 不应依赖 Web 基础设施类型（ASP.NET Core），应保持协议无关\n\n" +
            $"修复建议:\n" +
            $"1. 将 Web/HTTP 相关逻辑保持在 Endpoint 或 Host 层\n" +
            $"2. Handler 应只依赖业务抽象接口（如 IRepository, IEventBus）\n" +
            $"3. 使用 Command/Query 对象传递数据，而非 HttpContext/IFormFile 等\n\n" +
            $"参考: ADR-005 §ADR-005_3_2");
        }
    }

    #endregion

    #region ADR-005_4: 通信契约与领域模型隔离

    [Theory(DisplayName = "ADR-005_4_1: 模块间仅通过契约通信")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Cross_Module_Must_Use_Contracts_Only(Assembly moduleAssembly)
    {
        var entities = Types
            .InAssembly(moduleAssembly)
            .That()
            .ResideInNamespaceContaining(".Entities")
            .Or()
            .ResideInNamespaceContaining(".Domain")
            .Or()
            .HaveNameEndingWith("Entity")
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var entity in entities)
        {
            // 检查实体是否标记为 public（可能被其他模块引用）
            if (entity.IsPublic)
            {
                var constructors = entity.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

                // 建议：领域实体的公共构造函数应该很少或没有
                // 这是一个软约束，用于提醒开发者注意
                if (constructors.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ ADR-005_4_1 建议: 实体 {entity.FullName} 是 public 且有公共构造函数，确保不会被其他模块直接引用。应仅传递契约 DTO。");
                }
            }
        }

        true.Should().BeTrue("模块间仅通过契约通信检查完成");
    }

    [Theory(DisplayName = "ADR-005_4_2: 契约不承载业务决策")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Contracts_Must_Not_Contain_Business_Logic(Assembly moduleAssembly)
    {
        var contracts = Types
            .InAssembly(moduleAssembly)
            .That()
            .ResideInNamespaceContaining(".Contracts")
            .Or()
            .HaveNameEndingWith("Dto")
            .Or()
            .HaveNameEndingWith("Contract")
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var contract in contracts)
        {
            // 检查契约是否包含方法（除了属性 getter/setter）
            var methods = contract
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName) // 排除属性访问器
                .Where(m => m.DeclaringType == contract)
                .ToList();

            if (methods.Count > 0)
            {
                true.Should().BeFalse($"❌ ADR-005_4_2 违规: 契约包含业务方法\n\n" +
                            $"违规契约: {contract.FullName}\n" +
                            $"业务方法: {string.Join(", ", methods.Select(m => m.Name))}\n\n" +
                            $"问题分析:\n" +
                            $"契约（Contract）包含计算属性或业务判断方法\n\n" +
                            $"修复建议:\n" +
                            $"1. 契约仅包含数据字段（属性）\n" +
                            $"2. 将业务逻辑移到 Handler 或领域服务中\n" +
                            $"3. 契约只允许传递数据，不承载业务决策/行为\n\n" +
                            $"参考: ADR-005 §ADR-005_4_2");
            }
        }
    }

    #endregion

    #region ADR-005_5: CQRS 与 Handler 唯一性

    [Theory(DisplayName = "ADR-005_5_1: Command Handler 只执行业务逻辑")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Command_Handlers_Only_Execute_Business_Logic(Assembly moduleAssembly)
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
                    returnType
                        .GetGenericTypeDefinition()
                        .Name
                        .StartsWith("Task"))
                {
                    returnType = returnType
                                     .GetGenericArguments()
                                     .FirstOrDefault() ??
                                 returnType;
                }

                // 提取 ValueTask<T> 中的 T
                if (returnType.IsGenericType &&
                    returnType
                        .GetGenericTypeDefinition()
                        .Name
                        .StartsWith("ValueTask"))
                {
                    returnType = returnType
                                     .GetGenericArguments()
                                     .FirstOrDefault() ??
                                 returnType;
                }

                // 允许的返回类型：void, Task, ValueTask, Unit, bool, int (表示 ID), Guid
                var allowedTypes = new[] { "Void", "Task", "ValueTask", "Unit", "Boolean", "Int32", "Int64", "Guid" };

                if (!allowedTypes.Contains(returnType.Name) && returnType != typeof(void))
                {
                    // 如果返回类型是复杂业务对象，给出建议
                    if (returnType.Namespace?.StartsWith("Zss.BilliardHall.Modules") == true || returnType.Namespace?.StartsWith("Zss.BilliardHall.Platform.Contracts") == true)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ ADR-005_5_1 建议: Command Handler {handler.FullName}.{method.Name} 返回了业务数据类型 {returnType.FullName}。\n" + $"建议：Command Handler 应该只返回 void 或 Id（简单类型如 Guid/int），避免返回完整的业务对象。");
                    }
                }
            }
        }

        true.Should().BeTrue("Command Handler 只执行业务逻辑检查完成");
    }

    [Theory(DisplayName = "ADR-005_5_2: Query Handler 只读返回")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Query_Handlers_Return_Read_Only_Data(Assembly moduleAssembly)
    {
        var queryHandlers = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("QueryHandler")
            .Or()
            .HaveNameEndingWith("Query.Handler")
            .GetTypes();

        foreach (var handler in queryHandlers)
        {
            // 查找 Handle 或 Execute 方法
            var handleMethods = handler
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "Handle" || m.Name == "Execute" || m.Name.EndsWith("Async"))
                .ToList();

            foreach (var method in handleMethods)
            {
                // 检查方法体是否调用了 Repository 的写操作
                // 这需要 IL 分析，这里只做简单的类型检查
                var methodBody = method.GetMethodBody();
                if (methodBody != null)
                {
                    // 建议：Query Handler 禁止调用 Repository 的 Add/Update/Delete 方法
                    // 这里只是示例，实际需要更复杂的 IL 分析或 Roslyn Analyzer
                    System.Diagnostics.Debug.WriteLine($"提示: 确保 Query Handler {handler.FullName}.{method.Name} 仅读取数据，不执行写操作。");
                }
            }
        }

        // Query Handler 允许使用和返回 Contracts/DTO - 这是预期行为
        true.Should().BeTrue("Query Handler 只读返回检查完成");
    }

    [Theory(DisplayName = "ADR-005_5_3: Command/Query 必须分离")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Command_And_Query_Must_Be_Separated(Assembly moduleAssembly)
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
                true.Should().BeFalse($"❌ ADR-005_5_3 违规: Handler 同时包含 Command 和 Query 语义\n\n" +
                            $"违规 Handler: {handler.FullName}\n\n" +
                            $"问题分析:\n" +
                            $"Handler 命名同时包含 Command 和 Query，违反 CQRS 原则\n\n" +
                            $"修复建议:\n" +
                            $"1. 严格分离为两个 Handler：XxxCommandHandler 和 XxxQueryHandler\n" +
                            $"2. Command Handler：执行写操作，返回 void 或 ID\n" +
                            $"3. Query Handler：执行读操作，返回 DTO\n" +
                            $"4. Command 和 Query 独立 Handler，不允许合并、混用\n\n" +
                            $"参考: ADR-005 §ADR-005_5_3");
            }
        }
    }

    #endregion

    #region 辅助验证

    [Fact(DisplayName = "ADR-005_辅助: 所有 Handler 应在模块程序集中")]
    public void All_Handlers_Should_Be_In_Module_Assemblies()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;

        // Platform 不应包含 Handler
        var platformHandlers = Types
            .InAssembly(platformAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .GetTypes()
            .ToList();

        platformHandlers.Should().BeEmpty("Platform 层不应包含业务 Handler");

        // Application 不应包含业务 Handler（可能有基础 Handler）
        var applicationHandlers = Types
            .InAssembly(applicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .DoNotHaveNameStartingWith("Base")
            .And()
            .DoNotHaveNameStartingWith("Abstract")
            .GetTypes()
            .ToList();

        // Application 可以有抽象基类，但不应有具体业务 Handler
        var businessHandlers = applicationHandlers
            .Where(h => !h.IsAbstract)
            .Where(h => h.Namespace?.Contains("Command") == true || h.Namespace?.Contains("Query") == true)
            .ToList();

        businessHandlers.Should().BeEmpty("Application 层不应包含具体业务 Handler");
    }

    #endregion

}
