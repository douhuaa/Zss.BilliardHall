using NetArchTest.Rules;
using FluentAssertions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-120: 领域事件命名规范
/// 验证领域事件命名、命名空间组织、版本演进和模块隔离约束
/// 
/// 约束映射（对应 ADR-120 快速参考表）：
/// ┌──────────────┬────────────────────────────────────────────────────────┬─────────┬────────────────────────────────────────────────────┐
/// │ 约束编号     │ 描述                                                    │ 层级    │ 测试方法                                            │
/// ├──────────────┼────────────────────────────────────────────────────────┼─────────┼────────────────────────────────────────────────────┤
/// │ ADR-120.1    │ 事件类型必须以 `Event` 后缀结尾                          │ L1      │ Event_Types_Should_End_With_Event_Suffix           │
/// │ ADR-120.2    │ 事件名称必须使用动词过去式                                │ L1      │ Event_Names_Should_Use_Past_Tense_Verbs            │
/// │ ADR-120.3    │ 事件必须在模块的 `Events` 命名空间下                      │ L1      │ Events_Should_Be_In_Events_Namespace               │
/// │ ADR-120.4    │ 事件处理器必须以 `Handler` 后缀结尾                       │ L1      │ Event_Handlers_Should_End_With_Handler_Suffix      │
/// │ ADR-120.5    │ 事件不得包含领域实体类型                                  │ L1      │ Events_Should_Not_Contain_Domain_Entities          │
/// │ ADR-120.6    │ 事件不得包含业务方法                                      │ L1      │ Events_Should_Not_Contain_Business_Methods         │
/// └──────────────┴────────────────────────────────────────────────────────┴─────────┴────────────────────────────────────────────────────┘
/// 
/// 注：ADR-120.7（文件名与类型名一致）已移除，因其耦合仓库物理结构，属于代码组织习惯而非架构约束。
/// </summary>
public sealed class ADR_120_Architecture_Tests
{

    #region 1. 事件命名规则 (ADR-120.1, ADR-120.2)

    [Theory(DisplayName = "ADR-120_1_1: 事件类型必须以 Event 后缀结尾")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Event_Types_Should_End_With_Event_Suffix(Assembly moduleAssembly)
    {
        // 查找所有在 Events 命名空间或继承自常见事件基类的类型
        var eventTypes = Types
            .InAssembly(moduleAssembly)
            .That()
            .ResideInNamespaceContaining(".Events")
            .And()
            .AreClasses()
            .GetTypes()
            .Where(t => !t.IsAbstract) // 排除抽象基类
            .ToList();

        foreach (var eventType in eventTypes)
        {
            eventType.Name.EndsWith("Event").Should().BeTrue($"❌ ADR-120_1_1 违规: 事件类型缺少 'Event' 后缀\n\n" +
            $"违规类型: {eventType.FullName}\n\n" +
            $"问题分析:\n" +
            $"所有领域事件类型必须以 'Event' 后缀结尾，以明确标识其为事件\n\n" +
            $"修复建议：\n" +
            $"1. 将类型重命名为 {eventType.Name}Event\n" +
            $"2. 确保命名遵循模式: {{AggregateRoot}}{{Action}}Event\n" +
            $"3. 示例：OrderCreatedEvent, MemberUpgradedEvent\n\n" +
            $"参考：docs/adr/structure/ADR-120-domain-event-naming-convention.md（基本命名规则）");
        }
    }

    [Theory(DisplayName = "ADR-120_1_2: 事件名称必须使用动词过去式")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Event_Names_Should_Use_Past_Tense_Verbs(Assembly moduleAssembly)
    {
        var eventTypes = Types
            .InAssembly(moduleAssembly)
            .That()
            .ResideInNamespaceContaining(".Events")
            .And()
            .HaveNameEndingWith("Event")
            .GetTypes()
            .Where(t => !t.IsAbstract)
            .ToList();

        // 禁止的违规模式：现在时/进行时/原形动词
        // 架构测试哲学：裁定明显错误，而不是证明正确
        var forbiddenPatterns = new[] {
            // 进行时 (-ing)
            new Regex(@"(\w+)ingEvent$", RegexOptions.Compiled),
            // 常见现在时动词原形
            new Regex(@"(Create|Update|Delete|Add|Remove|Send|Process|Execute|Start|Stop|Begin|End|Open|Close)Event$", RegexOptions.Compiled)
        };

        foreach (var eventType in eventTypes)
        {
            var eventName = eventType.Name;

            // 只检查是否匹配违规模式（明确的错误）
            foreach (var pattern in forbiddenPatterns)
            {
                if (pattern.IsMatch(eventName))
                {
                    true.Should().BeFalse($"❌ ADR-120_1_2 违规: 事件名称未使用动词过去式（本体语义约束，L1）\n\n" +
                                $"违规类型: {eventType.FullName}\n" +
                                $"命名模式: {eventName}\n\n" +
                                $"问题分析:\n" +
                                $"事件名称必须使用动词过去式，因为事件描述的是已发生的业务事实。\n" +
                                $"使用现在时或进行时会导致事件与命令混淆，造成概念污染。\n\n" +
                                $"修复建议：\n" +
                                $"1. 将动词改为过去式形式：\n" +
                                $"   - Creating → Created\n" +
                                $"   - Updating → Updated\n" +
                                $"   - Processing → Processed\n" +
                                $"2. 或使用明确的过去式动词：\n" +
                                $"   - Cancelled, Shipped, Paid, Upgraded, Suspended\n\n" +
                                $"参考：docs/adr/structure/ADR-120-domain-event-naming-convention.md（基本命名规则）");
                }
            }
        }
    }

    #endregion

    #region 2. 命名空间组织 (ADR-120.3)

    [Theory(DisplayName = "ADR-120_2_1: 事件必须在模块的 Events 命名空间下")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Events_Should_Be_In_Events_Namespace(Assembly moduleAssembly)
    {
        var eventTypes = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Event")
            .GetTypes()
            .Where(t => !t.IsAbstract)
            .Where(t => t.Namespace != null)
            .ToList();

        foreach (var eventType in eventTypes)
        {
            var ns = eventType.Namespace!;

            // 检查命名空间是否符合规范：Zss.BilliardHall.Modules.{ModuleName}.Events[.{SubNamespace}]
            var isValidNamespace = ns.Contains(".Events") && ns.StartsWith("Zss.BilliardHall.Modules.");

            isValidNamespace.Should().BeTrue($"❌ ADR-120_2_1 违规: 事件未在正确的命名空间下\n\n" +
            $"违规类型: {eventType.FullName}\n" +
            $"当前命名空间: {ns}\n" +
            $"期望命名空间: Zss.BilliardHall.Modules.{{ModuleName}}.Events[.{{SubNamespace}}]\n\n" +
            $"问题分析:\n" +
            $"领域事件必须组织在模块的 Events 命名空间下，以保持清晰的结构\n\n" +
            $"修复建议：\n" +
            $"1. 将事件移动到正确的命名空间：\n" +
            $"   namespace Zss.BilliardHall.Modules.Orders.Events;\n" +
            $"2. 或使用子命名空间分组：\n" +
            $"   namespace Zss.BilliardHall.Modules.Orders.Events.OrderLifecycle;\n" +
            $"3. 禁止的命名空间模式：\n" +
            $"   - Zss.BilliardHall.Events (不在模块内)\n" +
            $"   - Zss.BilliardHall.Modules.Orders.Domain.Events (非标准)\n\n" +
            $"参考：docs/adr/structure/ADR-120-domain-event-naming-convention.md（命名空间组织规则）");
        }
    }

    #endregion

    #region 3. 事件处理器命名 (ADR-120.4)

    [Theory(DisplayName = "ADR-120_3_1: 事件处理器必须以 Handler 后缀结尾")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Event_Handlers_Should_End_With_Handler_Suffix(Assembly moduleAssembly)
    {
        // 语义化检测：查找实现了事件处理接口的类型
        // 而不是基于命名约定猜测（避免误杀 EventHandlerRegistry、EventHandlerMetrics 等）
        var allTypes = moduleAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        var eventHandlerTypes = new List<Type>();

        foreach (var type in allTypes)
        {
            // 检查是否实现了事件处理接口（IEventHandler<T>、IHandle<T> 等）
            var interfaces = type.GetInterfaces();
            var isEventHandler = interfaces.Any(i => i.IsGenericType &&
                                                     (i
                                                          .GetGenericTypeDefinition()
                                                          .Name
                                                          .Contains("IEventHandler") ||
                                                      i
                                                          .GetGenericTypeDefinition()
                                                          .Name
                                                          .Contains("IHandle")) &&
                                                     i
                                                         .GetGenericArguments()
                                                         .Any(arg => arg.Name.EndsWith("Event")));

            // 或者：位于明确的事件处理命名空间（精确匹配，不是模糊包含）
            var isInEventHandlersNamespace = type.Namespace != null && (type.Namespace.EndsWith(".EventHandlers") || type.Namespace.Contains(".EventHandlers."));

            if (isEventHandler || isInEventHandlersNamespace)
            {
                eventHandlerTypes.Add(type);
            }
        }

        foreach (var handlerType in eventHandlerTypes)
        {
            handlerType.Name.EndsWith("Handler").Should().BeTrue($"❌ ADR-120_3_1 违规: 事件处理器缺少 'Handler' 后缀\n\n" +
            $"违规类型: {handlerType.FullName}\n\n" +
            $"问题分析:\n" +
            $"所有事件处理器必须以 'Handler' 后缀结尾\n\n" +
            $"修复建议：\n" +
            $"1. 基础模式：{{EventName}}Handler\n" +
            $"   - OrderCreatedEventHandler\n" +
            $"2. 扩展模式（多订阅场景）：{{EventName}}{{Purpose}}Handler\n" +
            $"   - OrderPaidEventAddPointsHandler\n" +
            $"   - OrderPaidEventGenerateInvoiceHandler\n" +
            $"3. 禁止使用 Processor、Service 等后缀\n\n" +
            $"参考：docs/adr/structure/ADR-120-domain-event-naming-convention.md（事件处理器命名规则）");
        }
    }

    #endregion

    #region 4. 模块隔离约束 (ADR-120.5, ADR-120.6)

    [Theory(DisplayName = "ADR-120_4_1: 事件不得包含领域实体类型")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Events_Should_Not_Contain_Domain_Entities(Assembly moduleAssembly)
    {
        var eventTypes = Types
            .InAssembly(moduleAssembly)
            .That()
            .ResideInNamespaceContaining(".Events")
            .And()
            .HaveNameEndingWith("Event")
            .GetTypes()
            .Where(t => !t.IsAbstract)
            .ToList();

        foreach (var eventType in eventTypes)
        {
            // 检查构造函数参数和公共属性
            var properties = eventType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var ctorParams = eventType
                .GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .Distinct()
                .ToList();

            var propertyTypes = properties
                .Select(p => p.PropertyType)
                .Distinct()
                .ToList();
            var allTypes = ctorParams
                .Concat(propertyTypes)
                .Distinct()
                .ToList();

            foreach (var type in allTypes)
            {
                var actualType = type;

                // 提取泛型参数（如 List<T> 中的 T）
                if (actualType.IsGenericType)
                {
                    actualType = actualType
                                     .GetGenericArguments()
                                     .FirstOrDefault() ??
                                 actualType;
                }

                // 检查是否为领域实体（通常在 Domain 或 Entities 命名空间）
                if (actualType.Namespace != null)
                {
                    var isDomainEntity = actualType.Namespace.Contains(".Domain") || actualType.Namespace.Contains(".Entities") || actualType.Name.EndsWith("Entity") || actualType.Name.EndsWith("Aggregate") || actualType.Name.EndsWith("ValueObject");

                    if (isDomainEntity)
                    {
                        true.Should().BeFalse($"❌ ADR-120_4_1 违规: 事件包含领域实体类型\n\n" +
                                    $"违规事件: {eventType.FullName}\n" +
                                    $"领域实体类型: {actualType.FullName}\n\n" +
                                    $"问题分析:\n" +
                                    $"事件不得包含领域实体（Entity/Aggregate/ValueObject），只能包含原始类型和 DTO\n\n" +
                                    $"修复建议：\n" +
                                    $"1. 将领域实体转换为简单 DTO：\n" +
                                    $"   - public record OrderItemDto(string ProductId, int Quantity, decimal Price);\n" +
                                    $"2. 或使用原始类型：\n" +
                                    $"   - Guid OrderId（而非 Order 对象）\n" +
                                    $"   - string MemberId（而非 Member 对象）\n" +
                                    $"3. 事件应该是不可变的数据快照，不包含行为\n\n" +
                                    $"参考：docs/adr/structure/ADR-120-domain-event-naming-convention.md（模块隔离约束）");
                    }
                }
            }
        }
    }

    [Theory(DisplayName = "ADR-120_4_2: 事件不得包含业务方法")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Events_Should_Not_Contain_Business_Methods(Assembly moduleAssembly)
    {
        var eventTypes = Types
            .InAssembly(moduleAssembly)
            .That()
            .ResideInNamespaceContaining(".Events")
            .And()
            .HaveNameEndingWith("Event")
            .GetTypes()
            .Where(t => !t.IsAbstract)
            .ToList();

        foreach (var eventType in eventTypes)
        {
            // 获取所有公共实例方法（排除属性的 get/set 和 Object 继承的方法）
            var methods = eventType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName) // 排除属性访问器和事件访问器
                .Where(m => m.DeclaringType == eventType) // 只检查当前类型声明的方法
                .Where(m => m.DeclaringType != typeof(object)) // 排除 Object 方法
                .ToList();

            // Record 类型会自动生成一些方法（如 Equals, GetHashCode, ToString, PrintMembers），需要排除
            var businessMethods = methods
                .Where(m => m.Name != "Equals")
                .Where(m => m.Name != "GetHashCode")
                .Where(m => m.Name != "ToString")
                .Where(m => m.Name != "PrintMembers")
                .Where(m => m.Name != "Deconstruct")
                .Where(m => !m.Name.StartsWith("<")) // 排除编译器生成的方法
                .ToList();

            if (businessMethods.Any())
            {
                true.Should().BeFalse($"❌ ADR-120_4_2 违规: 事件包含业务方法\n\n" +
                            $"违规事件: {eventType.FullName}\n" +
                            $"业务方法: {string.Join(", ", businessMethods.Select(m => m.Name))}\n\n" +
                            $"问题分析:\n" +
                            $"事件应该是纯数据对象（Data Object），不应包含业务逻辑或判断方法\n\n" +
                            $"修复建议：\n" +
                            $"1. 移除事件中的所有方法，只保留属性\n" +
                            $"2. 使用 record 类型定义事件（自动不可变）：\n" +
                            $"   - public record OrderCreatedEvent(Guid OrderId, DateTime CreatedAt);\n" +
                            $"3. 业务逻辑应该在领域模型或处理器中实现：\n" +
                            $"   - 判断逻辑 → 领域模型方法\n" +
                            $"   - 协调逻辑 → Handler\n\n" +
                            $"参考：docs/adr/structure/ADR-120-domain-event-naming-convention.md（模块隔离约束）");
            }
        }
    }

    #endregion

}
