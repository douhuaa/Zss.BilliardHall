using NetArchTest.Rules;
using FluentAssertions;
using System.Reflection;
using static Zss.BilliardHall.Tests.ArchitectureTests.Shared.AssertionMessageBuilder;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_005;

/// <summary>
/// 验证 ADR-005_3_1：模块内允许同步调用
/// 验证 ADR-005_3_2：模块间默认异步通信
/// </summary>
public sealed class ADR_005_3_Architecture_Tests
{
    [Theory(DisplayName = "ADR-005_3_1: 模块内允许同步调用")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_005_3_1_Intra_Module_Synchronous_Call_Allowed(Assembly moduleAssembly)
    {
        // 这是一个正面测试，验证模块内同步调用是允许的
        var types = Types
            .InAssembly(moduleAssembly)
            .That()
            .ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
            .GetTypes();

        // 模块内同步调用是允许的，这个测试主要是文档化这个规则
        types.Should().NotBeEmpty("模块应包含类型，模块内同步调用是允许的");
    }

    [Theory(DisplayName = "ADR-005_3_2: 模块间默认异步通信")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_005_3_2_Inter_Module_Communication_Must_Be_Async(Assembly moduleAssembly)
    {
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
                // 检查是否注入了其他模块的 Handler（表示未审批的跨模块同步调用）
                if (param.Namespace != null &&
                    param.Namespace.StartsWith("Zss.BilliardHall.Modules") &&
                    param.Name.EndsWith("Handler"))
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
                        true.Should().BeFalse(Build(
                            ruleId: "ADR-005_3_2",
                            violation: "未审批的跨模块同步调用",
                            evidence: new[]
                            {
                                $"违规 Handler: {handler.FullName}",
                                $"当前模块: {currentModule}",
                                $"依赖模块: {paramModule}",
                                $"依赖类型: {param.FullName}"
                            },
                            analysis: "模块间默认只能异步通信（领域事件/集成事件）",
                            remediation: new[]
                            {
                                "1. 使用异步事件: await _eventBus.Publish(new SomeEvent(...))",
                                "2. 如确需同步调用，必须提交 ADR 破例审批",
                                "3. 通过契约查询而非直接依赖: var dto = await _queryBus.Send(new GetSomeData(...))"
                            },
                            reference: "ADR-005_3_2 - 模块间默认异步通信"));
                    }
                }
            }
        }
    }
}
