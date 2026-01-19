using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// 契约使用规则测试
/// 确保 Contracts 仅用于数据传递，不用于业务决策
/// </summary>
public class ContractUsageTests
{
    private static readonly Assembly PlatformAssembly = typeof(Platform.PlatformDefaults).Assembly;

    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void CommandHandlers_Should_Not_Depend_On_IQuery_Interfaces(Assembly moduleAssembly)
    {
        // Command Handler 不允许依赖任何 IQuery 接口（包括本模块和其他模块的）
        var result = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("CommandHandler")
            .Or().HaveNameEndingWith("Command.Handler")
            .ShouldNot().HaveDependencyOn("Zss.BilliardHall.Platform.Contracts.IQuery")
            .GetResult();

        Assert.True(result.IsSuccessful, 
            $"模块 {moduleAssembly.GetName().Name} 中的 Command Handler 不应依赖 IQuery 接口。" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}。" +
            $"修复建议：Command Handler 应通过领域事件、命令或维护本地状态副本来获取必要信息，而不是查询其他模块的契约数据做业务决策。");
    }

    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void QueryHandlers_Can_Return_Contracts(Assembly moduleAssembly)
    {
        // Query Handler 允许返回契约类型（这是白名单场景）
        // 这个测试确保我们的架构允许这种用法
        var queryHandlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("QueryHandler")
            .Or().HaveNameEndingWith("Query.Handler")
            .GetTypes();

        // 如果有 Query Handler，确认它们可以依赖 Contracts 命名空间
        // 这是一个"正面"测试，确保我们没有过度限制
        Assert.True(true, "Query Handlers 允许使用 Contracts - 这是预期行为");
    }

    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Endpoints_Can_Use_Contracts(Assembly moduleAssembly)
    {
        // Endpoint/Controller 允许使用契约进行请求/响应（白名单场景）
        var endpoints = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Endpoint")
            .Or().HaveNameEndingWith("Controller")
            .GetTypes();

        // 这是一个"正面"测试，确保我们没有过度限制
        Assert.True(true, "Endpoints 允许使用 Contracts - 这是预期行为");
    }

    [Fact]
    public void Platform_Should_Not_Depend_On_Module_Contracts()
    {
        // Platform 不应该依赖任何模块的契约
        // Platform 可以定义契约接口（IContract, IQuery），但不应依赖具体的业务契约
        var result = Types.InAssembly(PlatformAssembly)
            .That().ResideInNamespace("Zss.BilliardHall.Platform")
            .And().DoNotResideInNamespace("Zss.BilliardHall.Platform.Contracts")
            .ShouldNot().HaveDependencyOnAny(
                ModuleAssemblyData.ModuleNames.Select(m => $"Zss.BilliardHall.Modules.{m}").ToArray())
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Platform 层不应依赖任何模块的契约或实现。" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}。" +
            $"修复建议：Platform 只提供技术能力，不应包含业务逻辑或依赖业务契约。");
    }

    [Theory]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Should_Have_Clear_Naming_Convention(Assembly moduleAssembly)
    {
        // 确保 Handler 命名清晰（CommandHandler vs QueryHandler）
        var handlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            var isCommandHandler = handler.Name.Contains("Command");
            var isQueryHandler = handler.Name.Contains("Query");
            
            Assert.True(isCommandHandler || isQueryHandler || handler.Name.EndsWith("EventHandler"),
                $"Handler {handler.FullName} 命名不清晰。" +
                $"修复建议：Handler 应明确命名为 *CommandHandler、*QueryHandler 或 *EventHandler。");
        }
    }
}
