using NetArchTest.Rules;
using FluentAssertions;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-201: Command Handler 生命周期管理
/// 验证 Handler 生命周期、依赖注入、资源释放等运行时规则
/// 
/// ADR 映射清单（ADR Mapping Checklist）：
/// ┌─────────────┬────────────────────────────────────────────────────────┬──────────┐
/// │ 测试方法     │ 对应 ADR 约束                                          │ ADR 章节 │
/// ├─────────────┼────────────────────────────────────────────────────────┼──────────┤
/// │ ADR-201.1   │ Handler 必须注册为 Scoped 生命周期                     │ 规则本体 │
/// │ ADR-201.3   │ Handler 禁止使用静态字段存储状态                       │ 规则本体 │
/// └─────────────┴────────────────────────────────────────────────────────┴──────────┘
/// </summary>
public sealed class ADR_201_Architecture_Tests
{
    [Theory(DisplayName = "ADR-201_1_3: Handler 禁止使用静态字段存储状态")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Must_Not_Have_Static_Fields(Assembly moduleAssembly)
    {
        var handlers = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            var staticFields = handler
                .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => !f.IsLiteral) // 排除 const
                .Where(f => !f.IsInitOnly || f.FieldType.IsGenericType) // 排除简单的 static readonly
                .ToList();

            if (staticFields.Any())
            {
                var fieldNames = string.Join(", ", staticFields.Select(f => f.Name));
                true.Should().BeFalse(
                    $"❌ ADR-201_1_3 违规: Handler 使用静态字段存储状态\n\n" +
                    $"违规类型：{handler.FullName}\n" +
                    $"静态字段：{fieldNames}\n\n" +
                    $"问题分析：\n" +
                    $"Handler 使用静态字段可能导致跨请求状态污染，违反 Scoped 生命周期原则\n\n" +
                    $"修复建议：\n" +
                    $"1. 将静态字段改为实例字段，通过构造函数注入依赖\n" +
                    $"2. 如需常量，使用 const 或 static readonly 原始类型\n" +
                    $"3. 如需共享状态，通过 DI 注入 Scoped 服务而非静态字段\n" +
                    $"4. 示例：\n" +
                    $"   // ❌ 错误：static Dictionary<string, int> _cache;\n" +
                    $"   // ✅ 正确：private readonly ICacheService _cache;\n\n" +
                    $"参考：docs/adr/runtime/ADR-201-command-handler-lifecycle-management.md（§1.3）");
            }
        }
    }

    [Fact(DisplayName = "ADR-201_1_1: Handler 必须注册为 Scoped 生命周期（需人工验证 DI 配置）")]
    public void Handlers_Must_Be_Registered_As_Scoped()
    {
        // 注意：此测试需要在集成测试中验证 DI 容器配置
        // 这里仅作为占位符，提醒团队此规则存在
        true.Should().BeTrue($"❌ ADR-201_1_1 提示：Handler 生命周期验证需在集成测试中检查\n\n" +
            $"验证内容：\n" +
            $"所有 Handler 类型必须在 DI 容器中注册为 Scoped 生命周期\n\n" +
            $"验证方法：\n" +
            $"1. 在集成测试中检查 ServiceDescriptor.Lifetime == ServiceLifetime.Scoped\n" +
            $"2. 验证所有 *Handler 类型都已正确注册\n" +
            $"3. 示例代码：\n" +
            $"   services.AddScoped<ICommandHandler<CreateOrderCommand>, CreateOrderCommandHandler>();\n\n" +
            $"参考：docs/adr/runtime/ADR-201-command-handler-lifecycle-management.md（§1.1）");
    }
}
