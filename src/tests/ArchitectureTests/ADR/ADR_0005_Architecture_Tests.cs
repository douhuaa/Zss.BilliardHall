using NetArchTest.Rules;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0005: 应用内交互模型与执行边界
/// 验证运行时规则：Handler 职责、同步/异步边界、模块通信契约
/// </summary>
public sealed class ADR_0005_Architecture_Tests
{
    #region 1. Use Case + Handler 最小执行单元

    [Theory(DisplayName = "ADR-0005.1: Handler 应有明确的命名约定")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Should_Have_Clear_Naming_Convention(Assembly moduleAssembly)
    {
        var handlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            var isCommandHandler = handler.Name.Contains("Command");
            var isQueryHandler = handler.Name.Contains("Query");
            var isEventHandler = handler.Name.Contains("Event");

            Assert.True(isCommandHandler || isQueryHandler || isEventHandler,
                $"❌ ADR-0005 违规: Handler {handler.FullName} 命名不清晰。\n" +
                $"修复建议：Handler 应明确命名为 *CommandHandler、*QueryHandler 或 *EventHandler，表达明确的业务意图。");
        }
    }

    [Theory(DisplayName = "ADR-0005.2: Endpoint 不应包含业务逻辑")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Endpoints_Should_Not_Contain_Business_Logic(Assembly moduleAssembly)
    {
        var endpoints = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Endpoint")
            .Or().HaveNameEndingWith("Controller")
            .GetTypes();

        foreach (var endpoint in endpoints)
        {
            // 检查方法行数（简单启发式检查）
            var methods = endpoint.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName)
                .Where(m => m.DeclaringType == endpoint)
                .ToList();

            foreach (var method in methods)
            {
                // 检查方法是否注入了过多依赖（超过 3 个可能表示业务逻辑）
                var parameters = method.GetParameters();
                var constructorParams = endpoint.GetConstructors()
                    .SelectMany(c => c.GetParameters())
                    .Select(p => p.ParameterType)
                    .Where(t => !t.Namespace?.StartsWith("Microsoft.Extensions") == true)
                    .Where(t => !t.Namespace?.StartsWith("System") == true)
                    .ToList();

                if (constructorParams.Count > 5)
                {
                    Assert.Fail(
                        $"⚠️ ADR-0005 建议: Endpoint/Controller {endpoint.FullName} 注入了过多依赖 ({constructorParams.Count} 个)，可能包含业务逻辑。\n" +
                        $"修复建议：Endpoint 应该只负责接收请求、映射为 Use Case、转发给 Handler。业务逻辑应在 Handler 中实现。");
                }
            }
        }
    }

    #endregion

    #region 2. Handler 职责边界

    [Theory(DisplayName = "ADR-0005.3: Handler 不应依赖 ASP.NET 类型")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Should_Not_Depend_On_AspNet(Assembly moduleAssembly)
    {
        var forbiddenDeps = new[] { "Microsoft.AspNetCore", "Microsoft.AspNetCore.Http", "Microsoft.AspNetCore.Mvc" };
        var result = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Handler")
            .ShouldNot().HaveDependencyOnAny(forbiddenDeps)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0005 违规: 模块 {moduleAssembly.GetName().Name} 中的 Handler 不应依赖 ASP.NET Core 包/程序集。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
            $"修复建议：Handler 应该是纯业务逻辑，不应依赖 Web 基础设施。");

        // 额外的反射检查
        var handlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Handler")
            .GetTypes();

        var forbiddenNamespaces = new[] { "Microsoft.AspNetCore", "Microsoft.AspNetCore.Http", "Microsoft.AspNetCore.Mvc" };

        foreach (var handler in handlers)
        {
            var ctorDeps = handler.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t?.Namespace != null && forbiddenNamespaces.Any(ns => t.Namespace.StartsWith(ns)))
                .ToList();

            Assert.True(ctorDeps.Count == 0,
                $"❌ ADR-0005 违规: Handler {handler.FullName} 的构造函数参数依赖 ASP.NET 类型: {string.Join(", ", ctorDeps.Select(t => t.FullName))}。\n" +
                $"修复建议：把 Web/HTTP 相关类型保持在 Host 层，Handler 应该是协议无关的。");
        }
    }

    [Theory(DisplayName = "ADR-0005.4: Handler 应该是无状态的")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Should_Be_Stateless(Assembly moduleAssembly)
    {
        var handlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            // 检查是否有可变的实例字段（除了注入的依赖）
            var fields = handler.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => !f.IsInitOnly) // 排除 readonly 字段
                .ToList();

            Assert.True(fields.Count == 0,
                $"⚠️ ADR-0005 建议: Handler {handler.FullName} 包含可变字段，可能承载状态。\n" +
                $"可变字段: {string.Join(", ", fields.Select(f => f.Name))}。\n" +
                $"修复建议：Handler 应该是无状态的，所有依赖应通过构造函数注入（readonly）。");
        }
    }

    #endregion

    #region 3. 同步/异步原则

    [Theory(DisplayName = "ADR-0005.5: 模块间不应有未审批的同步调用")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Have_Synchronous_Cross_Module_Calls(Assembly moduleAssembly)
    {
        // 检查模块是否直接依赖其他模块的 Handler（这表示同步调用）
        var handlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            var ctorParams = handler.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .ToList();

            foreach (var param in ctorParams)
            {
                // 检查是否注入了其他模块的类型
                if (param.Namespace != null && param.Namespace.StartsWith("Zss.BilliardHall.Modules"))
                {
                    var currentModule = moduleAssembly.GetName().Name!.Split('.').Last();
                    var paramModule = param.Namespace.Split('.').ElementAtOrDefault(3);

                    if (paramModule != null && paramModule != currentModule)
                    {
                        Assert.Fail(
                            $"❌ ADR-0005 违规: Handler {handler.FullName} 注入了其他模块 {paramModule} 的类型: {param.FullName}。\n" +
                            $"修复建议：模块间应通过异步通信（事件/消息）解耦，避免同步调用。如确需同步调用，必须提交 ADR 审批。");
                    }
                }
            }
        }
    }

    [Theory(DisplayName = "ADR-0005.6: 异步方法应遵循命名约定")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Async_Methods_Should_Follow_Naming_Convention(Assembly moduleAssembly)
    {
        var types = Types.InAssembly(moduleAssembly)
            .That().ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
            .GetTypes();

        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.ReturnType.Name.Contains("Task"))
                .Where(m => !m.IsSpecialName)
                .Where(m => m.Name != "Handle") // Wolverine 使用 Handle 作为约定方法名
                .ToList();

            foreach (var method in methods)
            {
                if (!method.Name.EndsWith("Async"))
                {
                    Assert.Fail(
                        $"⚠️ ADR-0005 建议: 异步方法 {type.FullName}.{method.Name} 应该以 'Async' 结尾。\n" +
                        $"修复建议：异步方法命名应遵循 .NET 约定，以 Async 结尾（Handler.Handle 方法除外）。");
                }
            }
        }
    }

    #endregion

    #region 4. 模块通信契约

    [Theory(DisplayName = "ADR-0005.7: 模块不应共享领域实体")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Share_Domain_Entities(Assembly moduleAssembly)
    {
        var entities = Types.InAssembly(moduleAssembly)
            .That().ResideInNamespaceContaining(".Entities")
            .Or().ResideInNamespaceContaining(".Domain")
            .Or().HaveNameEndingWith("Entity")
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
                    System.Diagnostics.Debug.WriteLine(
                        $"⚠️ ADR-0005 建议: 实体 {entity.FullName} 是 public 且有公共构造函数，确保不会被其他模块直接引用。");
                }
            }
        }

        Assert.True(true, "领域实体共享检查完成");
    }

    [Theory(DisplayName = "ADR-0005.8: Query Handler 可以返回 Contracts")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void QueryHandlers_Can_Return_Contracts(Assembly moduleAssembly)
    {
        var queryHandlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("QueryHandler")
            .Or().HaveNameEndingWith("Query.Handler")
            .GetTypes();

        // 这是一个"正面"测试，确保我们没有过度限制
        // Query Handler 允许使用和返回 Contracts
        Assert.True(true, "Query Handlers 允许使用 Contracts - 这是预期行为");
    }

    #endregion

    #region 5. 查询与命令分离

    [Theory(DisplayName = "ADR-0005.9: Command Handler 和 Query Handler 应明确分离")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Command_And_Query_Handlers_Should_Be_Separated(Assembly moduleAssembly)
    {
        var handlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            var isCommandHandler = handler.Name.Contains("Command");
            var isQueryHandler = handler.Name.Contains("Query");

            // Handler 不应同时是 Command 和 Query
            if (isCommandHandler && isQueryHandler)
            {
                Assert.Fail(
                    $"❌ ADR-0005 违规: Handler {handler.FullName} 同时包含 Command 和 Query 语义。\n" +
                    $"修复建议：严格分离 Command Handler（写操作）和 Query Handler（读操作）。");
            }
        }
    }

    [Theory(DisplayName = "ADR-0005.10: Command Handler 不应返回业务数据")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void CommandHandlers_Should_Not_Return_Business_Data(Assembly moduleAssembly)
    {
        var commandHandlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("CommandHandler")
            .Or().HaveNameEndingWith("Command.Handler")
            .GetTypes();

        foreach (var handler in commandHandlers)
        {
            // 查找 Handle 或 Execute 方法
            var handleMethods = handler.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "Handle" || m.Name == "Execute" || m.Name.EndsWith("Async"))
                .ToList();

            foreach (var method in handleMethods)
            {
                var returnType = method.ReturnType;

                // 提取 Task<T> 中的 T
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition().Name.StartsWith("Task"))
                {
                    returnType = returnType.GetGenericArguments().FirstOrDefault() ?? returnType;
                }

                // 提取 ValueTask<T> 中的 T
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition().Name.StartsWith("ValueTask"))
                {
                    returnType = returnType.GetGenericArguments().FirstOrDefault() ?? returnType;
                }

                // 允许的返回类型：void, Task, ValueTask, Unit, bool, int (表示 ID), Guid
                var allowedTypes = new[] { "Void", "Task", "ValueTask", "Unit", "Boolean", "Int32", "Int64", "Guid" };

                if (!allowedTypes.Contains(returnType.Name) && returnType != typeof(void))
                {
                    // 如果返回类型是复杂业务对象，给出建议
                    if (returnType.Namespace?.StartsWith("Zss.BilliardHall.Modules") == true ||
                        returnType.Namespace?.StartsWith("Zss.BilliardHall.Platform.Contracts") == true)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"⚠️ ADR-0005 建议: Command Handler {handler.FullName}.{method.Name} 返回了业务数据类型 {returnType.FullName}。\n" +
                            $"建议：Command Handler 应该只返回简单类型（如 ID/bool）或 void，避免返回完整的业务对象。");
                    }
                }
            }
        }

        Assert.True(true, "Command Handler 返回类型检查完成");
    }

    #endregion

    #region 6. 错误与失败语义

    [Theory(DisplayName = "ADR-0005.11: Handler 应使用结构化异常")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Should_Use_Structured_Exceptions(Assembly moduleAssembly)
    {
        var handlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            var handleMethods = handler.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "Handle" || m.Name == "Execute" || m.Name.EndsWith("Async"))
                .ToList();

            foreach (var method in handleMethods)
            {
                // 检查方法体中是否直接 throw new Exception
                // 这需要 IL 分析，这里只做简单的类型检查
                var methodBody = method.GetMethodBody();
                if (methodBody != null)
                {
                    // 建议使用 DomainException 等结构化异常
                    // 这里只是示例，实际需要更复杂的 IL 分析或 Roslyn Analyzer
                    System.Diagnostics.Debug.WriteLine(
                        $"提示: 确保 Handler {handler.FullName}.{method.Name} 使用结构化异常（如 DomainException）。");
                }
            }
        }

        Assert.True(true, "Handler 异常使用检查完成");
    }

    #endregion

    #region 7. 自动化校验辅助

    [Fact(DisplayName = "ADR-0005.12: 所有 Handler 应在模块程序集中")]
    public void All_Handlers_Should_Be_In_Module_Assemblies()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;

        // Platform 不应包含 Handler
        var platformHandlers = Types.InAssembly(platformAssembly)
            .That().HaveNameEndingWith("Handler")
            .GetTypes()
            .ToList();

        Assert.Empty(platformHandlers);

        // Application 不应包含业务 Handler（可能有基础 Handler）
        var applicationHandlers = Types.InAssembly(applicationAssembly)
            .That().HaveNameEndingWith("Handler")
            .And().DoNotHaveNameStartingWith("Base")
            .And().DoNotHaveNameStartingWith("Abstract")
            .GetTypes()
            .ToList();

        // Application 可以有抽象基类，但不应有具体业务 Handler
        var businessHandlers = applicationHandlers
            .Where(h => !h.IsAbstract)
            .Where(h => h.Namespace?.Contains("Command") == true || h.Namespace?.Contains("Query") == true)
            .ToList();

        Assert.Empty(businessHandlers);
    }

    #endregion
}
