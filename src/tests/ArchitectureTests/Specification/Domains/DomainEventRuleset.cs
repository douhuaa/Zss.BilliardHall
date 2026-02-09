using System.Reflection;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Common;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Domains;

/// <summary>
/// 领域事件规则集
/// 映射来源：ADR-120（领域事件命名规范）、ADR-210（事件溯源模式）
/// </summary>
public static class DomainEventRuleset
{
    /// <summary>
    /// 获取所有领域事件相关规则
    /// </summary>
    /// <returns>规则定义集合</returns>
    public static IEnumerable<RuleDefinition> GetRules()
    {
        yield return new RuleDefinition(
            new RuleId("RS-010", "ADR-120", "1_1"),
            "领域事件名称必须以 'Event' 结尾",
            RuleLayer.Enforcement,
            SeverityLevel.L1,
            assemblies =>
            {
                bool Filter(Type t) =>
                    t.Namespace?.Contains(".Domain.Events") == true &&
                    t.IsClass &&
                    !t.IsAbstract;

                bool NameRule(Type t) => t.Name.EndsWith("Event");

                const string Hint = "确保领域事件类名以 'Event' 结尾，并位于 *.Domain.Events 命名空间下";
                return RuleAdapters.NamesShouldMatch(assemblies, Filter, NameRule, Hint);
            });

        yield return new RuleDefinition(
            new RuleId("RS-011", "ADR-120", "1_2"),
            "领域事件必须在正确的命名空间下",
            RuleLayer.Enforcement,
            SeverityLevel.L1,
            assemblies =>
            {
                bool Filter(Type t) =>
                    t.Name.EndsWith("Event") &&
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.Namespace != null &&
                    t.Namespace.StartsWith("Zss.BilliardHall") && // 只检查项目内的类型
                    !t.Namespace.Contains(".Domain.Events");

                var violations = assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(Filter)
                    .Select(t => $"{t.FullName} (应在 *.Domain.Events 命名空间下)")
                    .ToArray();

                return violations.Length == 0
                    ? RuleResult.Ok()
                    : RuleResult.Fail($"事件命名空间组织不正确：\n- " + string.Join("\n- ", violations));
            });

        yield return new RuleDefinition(
            new RuleId("RS-012", "ADR-120", "2_1"),
            "事件处理器名称必须以 'Handler' 结尾",
            RuleLayer.Enforcement,
            SeverityLevel.L1,
            assemblies =>
            {
                bool Filter(Type t) =>
                    t.Namespace?.Contains(".Handlers") == true &&
                    t.IsClass &&
                    !t.IsAbstract;

                bool NameRule(Type t) => t.Name.EndsWith("Handler");

                const string Hint = "确保事件处理器类名以 'Handler' 结尾";
                return RuleAdapters.NamesShouldMatch(assemblies, Filter, NameRule, Hint);
            });

        yield return new RuleDefinition(
            new RuleId("RS-013", "ADR-120", "3_1"),
            "领域事件不应包含领域实体引用",
            RuleLayer.Heuristics,
            SeverityLevel.L3,
            assemblies =>
            {
                // 这是一个启发式规则，检查事件是否直接依赖实体类型
                var eventTypes = assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.Namespace?.Contains(".Domain.Events") == true &&
                               t.Name.EndsWith("Event") &&
                               t.IsClass)
                    .ToArray();

                var warnings = new List<string>();

                foreach (var eventType in eventTypes)
                {
                    var properties = eventType.GetProperties();
                    foreach (var prop in properties)
                    {
                        var propTypeName = prop.PropertyType.Name;
                        // 检查属性类型是否可能是实体（启发式：避免常见实体命名）
                        if (propTypeName.EndsWith("Entity") || propTypeName.EndsWith("Aggregate"))
                        {
                            warnings.Add($"{eventType.Name}.{prop.Name} : {propTypeName}");
                        }
                    }
                }

                return warnings.Count == 0
                    ? RuleResult.Ok()
                    : RuleResult.Warning(warnings.ToArray());
            });
    }
}
