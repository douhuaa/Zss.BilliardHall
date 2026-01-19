using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// 垂直切片架构测试
/// 确保模块采用用例驱动的垂直切片组织方式
/// </summary>
public class VerticalSliceArchitectureTests
{
    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Organize_By_Features_Not_Technical_Layers(Assembly moduleAssembly)
    {
        // 模块应该按功能组织，而不是技术分层
        // 已在 ModuleIsolationTests 中检查禁止的命名空间
        var result = Types.InAssembly(moduleAssembly)
            .That().ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
            .ShouldNot().ResideInNamespaceContaining(".Application")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"模块 {moduleAssembly.GetName().Name} 应采用垂直切片架构，避免传统分层命名空间。" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。" +
            $"修复建议：按用例/功能组织代码（如 CreateMember, GetMemberById），而不是按技术层（Application/Domain/Infrastructure）。");
    }

    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void CommandHandlers_Should_Be_Self_Contained(Assembly moduleAssembly)
    {
        // Command Handler 应该是自包含的，不依赖横向的 Service
        var commandHandlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("CommandHandler")
            .Or().HaveNameEndingWith("Command.Handler")
            .GetTypes();

        // 检查是否有 Handler 依赖了 *Service
        foreach (var handler in commandHandlers)
        {
            var dependencies = handler.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.Name.EndsWith("Service") && 
                           t.Namespace?.StartsWith("Zss.BilliardHall.Modules") == true)
                .ToList();

            Assert.True(dependencies.Count == 0,
                $"Command Handler {handler.FullName} 依赖了横向 Service: {string.Join(", ", dependencies.Select(d => d.Name))}。" +
                $"修复建议：在垂直切片架构中，Handler 应直接包含业务逻辑，避免抽象为横向 Service。" +
                $"如需复用，考虑使用领域事件、辅助方法或领域服务（Domain Service）。");
        }
    }

    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void QueryHandlers_Should_Be_Self_Contained(Assembly moduleAssembly)
    {
        // Query Handler 应该是自包含的，不依赖横向的 Service
        var queryHandlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("QueryHandler")
            .Or().HaveNameEndingWith("Query.Handler")
            .GetTypes();

        // 检查是否有 Handler 依赖了 *Service
        foreach (var handler in queryHandlers)
        {
            var dependencies = handler.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.Name.EndsWith("Service") && 
                           t.Namespace?.StartsWith("Zss.BilliardHall.Modules") == true)
                .ToList();

            Assert.True(dependencies.Count == 0,
                $"Query Handler {handler.FullName} 依赖了横向 Service: {string.Join(", ", dependencies.Select(d => d.Name))}。" +
                $"修复建议：在垂直切片架构中，Handler 应直接查询数据，避免抽象为横向 Service。");
        }
    }

    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Have_Shared_Folders(Assembly moduleAssembly)
    {
        // 模块不应有 Shared/Common 文件夹（这会导致横向抽象）
        var result = Types.InAssembly(moduleAssembly)
            .That().ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
            .ShouldNot().ResideInNamespaceContaining(".Shared")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"模块 {moduleAssembly.GetName().Name} 不应包含 Shared/Common 命名空间。" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。" +
            $"修复建议：避免在模块内创建 Shared/Common 文件夹。" +
            $"如需在切片间共享逻辑，考虑：1) 复制代码（DRY 不是最高原则）；2) 提取到 Platform；3) 使用领域事件解耦。");
    }

    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Should_Not_Call_Other_Handlers_Directly(Assembly moduleAssembly)
    {
        // Handler 不应直接调用其他 Handler（应通过 Mediator/EventBus）
        var handlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            // 检查构造函数参数中是否注入了其他 Handler
            var dependencies = handler.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.Name.EndsWith("Handler") && 
                           t.Namespace?.StartsWith("Zss.BilliardHall.Modules") == true)
                .ToList();

            Assert.True(dependencies.Count == 0,
                $"Handler {handler.FullName} 直接依赖了其他 Handler: {string.Join(", ", dependencies.Select(d => d.Name))}。" +
                $"修复建议：Handler 之间不应直接调用。如需组合逻辑，使用 Mediator/EventBus 进行解耦，" +
                $"或将共享逻辑提取为独立的领域服务或辅助方法。");
        }
    }

    // 新增：Handlers 不应依赖 ASP.NET Core 类型/包
    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Should_Not_Depend_On_AspNet(Assembly moduleAssembly)
    {
        // 通过 NetArchTest 检查程序集依赖
        var forbiddenDeps = new[] { "Microsoft.AspNetCore", "Microsoft.AspNetCore.Http", "Microsoft.AspNetCore.Mvc" };
        var result = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Handler")
            .ShouldNot().HaveDependencyOnAny(forbiddenDeps)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"模块 {moduleAssembly.GetName().Name} 中的 Handler 不应依赖 ASP.NET Core 包/程序集。违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? System.Array.Empty<string>())}。");

        // 额外的反射检查：构造函数参数或属性类型来自 ASP.NET 命名空间
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
                $"Handler {handler.FullName} 的构造函数参数依赖 ASP.NET 类型: {string.Join(", ", ctorDeps.Select(t => t.FullName))}。请把 Web/HTTP 相关类型保持在 WebHost 层。");

            // 可选：检查公共属性（如果团队使用属性注入）
            var propDeps = handler.GetProperties()
                .Select(p => p.PropertyType)
                .Where(t => t?.Namespace != null && forbiddenNamespaces.Any(ns => t.Namespace.StartsWith(ns)))
                .ToList();

            Assert.True(propDeps.Count == 0,
                $"Handler {handler.FullName} 的公共属性依赖 ASP.NET 类型: {string.Join(", ", propDeps.Select(t => t.FullName))}。请把 Web/HTTP 相关类型保持在 WebHost 层。");
        }
    }
}
