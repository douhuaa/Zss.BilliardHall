using NetArchTest.Rules;
using FluentAssertions;
using System.Reflection;
using static Zss.BilliardHall.Tests.ArchitectureTests.Shared.AssertionMessageBuilder;

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

            (fields.Count == 0).Should().BeTrue(Build(
                ruleId: "ADR-005_2_1",
                violation: "Handler 包含可变字段",
                evidence: new[]
                {
                    $"违规 Handler: {handler.FullName}",
                    $"可变字段: {string.Join(", ", fields.Select(f => f.Name))}"
                },
                analysis: "Handler 包含非 readonly 字段，可能维护长期状态",
                remediation: new[]
                {
                    "1. 将所有依赖字段标记为 readonly",
                    "2. 通过构造函数注入依赖，而非在字段中维护状态",
                    "3. Handler 应该是短生命周期、无状态、可重入的"
                },
                reference: "ADR-005_2_1 - Handler 不得持有业务状态"));
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
                        true.Should().BeFalse(Build(
                            ruleId: "ADR-005_2_2",
                            violation: "Handler 注入了其他模块的类型",
                            evidence: new[]
                            {
                                $"违规 Handler: {handler.FullName}",
                                $"当前模块: {currentModule}",
                                $"依赖模块: {paramModule}",
                                $"依赖类型: {param.FullName}"
                            },
                            analysis: "模块间直接注入依赖表示同步调用，违反模块隔离原则",
                            remediation: new[]
                            {
                                "1. 使用异步事件通信: await _eventBus.Publish(new SomeEvent(...))",
                                "2. 或通过契约查询: var dto = await _queryBus.Send(new GetSomeData(...))",
                                "3. 如确需同步调用，必须提交 ADR 破例审批"
                            },
                            reference: "ADR-005_2_2 - Handler 禁止作为跨模块粘合层"));
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
                    true.Should().BeFalse(Build(
                        ruleId: "ADR-005_2_3",
                        violation: "Handler 返回领域实体",
                        evidence: new[]
                        {
                            $"违规 Handler: {handler.FullName}",
                            $"违规方法: {method.Name}",
                            $"返回类型: {returnType.FullName}"
                        },
                        analysis: "Handler 不应返回领域实体（Entity/Aggregate/ValueObject）",
                        remediation: new[]
                        {
                            "1. 创建对应的 DTO 或投影类型",
                            "2. 在 Handler 中映射领域实体到 DTO",
                            "3. 返回 DTO 而非领域实体"
                        },
                        reference: "ADR-005_2_3 - Handler 不允许返回领域实体"));
                }
            }
        }
    }
}
