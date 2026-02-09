using System.Reflection;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Common;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Domains;

/// <summary>
/// 仓储模式规则集
/// 映射来源：ADR-123（Repository 模式规范）
/// </summary>
public static class RepositoryRuleset
{
    /// <summary>
    /// 获取所有仓储相关规则
    /// </summary>
    /// <returns>规则定义集合</returns>
    public static IEnumerable<RuleDefinition> GetRules()
    {
        yield return new RuleDefinition(
            new RuleId("RS-020", "ADR-123", "3"),
            "Repository 接口以 'I' 开头，后缀 'Repository'",
            RuleLayer.Enforcement,
            SeverityLevel.L1,
            assemblies =>
            {
                bool Filter(Type t) =>
                    t.IsInterface &&
                    t.Name.Contains("Repository");

                bool NameRule(Type t) =>
                    t.Name.StartsWith("I") && t.Name.EndsWith("Repository");

                const string Hint = "Repository 接口应命名为 'IUserRepository' 格式";
                return RuleAdapters.NamesShouldMatch(assemblies, Filter, NameRule, Hint);
            });

        yield return new RuleDefinition(
            new RuleId("RS-021", "ADR-123", "2"),
            "Repository 接口必须在领域层",
            RuleLayer.Enforcement,
            SeverityLevel.L1,
            assemblies =>
            {
                bool Filter(Type t) =>
                    t.IsInterface &&
                    t.Name.EndsWith("Repository") &&
                    t.Name.StartsWith("I") &&
                    t.Namespace != null &&
                    !t.Namespace.Contains(".Domain");

                var violations = assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(Filter)
                    .Select(t => $"{t.FullName} (应在 *.Domain 命名空间下)")
                    .ToArray();

                return violations.Length == 0
                    ? RuleResult.Ok()
                    : RuleResult.Fail($"Repository 接口位置不正确：\n- " + string.Join("\n- ", violations));
            });

        yield return new RuleDefinition(
            new RuleId("RS-022", "ADR-123", "1"),
            "Repository 实现类应以 'Repository' 结尾",
            RuleLayer.Enforcement,
            SeverityLevel.L1,
            assemblies =>
            {
                bool Filter(Type t) =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.GetInterfaces().Any(i => i.Name.EndsWith("Repository")) &&
                    !t.Name.EndsWith("Repository");

                var violations = assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(Filter)
                    .Select(t => $"{t.FullName}")
                    .ToArray();

                return violations.Length == 0
                    ? RuleResult.Ok()
                    : RuleResult.Fail($"Repository 实现类命名不符合规范：\n- " +
                                    string.Join("\n- ", violations) +
                                    "\n实现类应命名为 'UserRepository' 格式");
            });

        yield return new RuleDefinition(
            new RuleId("RS-023", "ADR-123", "4"),
            "Repository 实现应在基础设施层",
            RuleLayer.Enforcement,
            SeverityLevel.L1,
            assemblies =>
            {
                bool Filter(Type t) =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.Name.EndsWith("Repository") &&
                    !t.Name.StartsWith("I") &&
                    t.Namespace != null &&
                    t.GetInterfaces().Any(i => i.Name.EndsWith("Repository")) &&
                    !t.Namespace.Contains(".Infrastructure");

                var violations = assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(Filter)
                    .Select(t => $"{t.FullName} (应在 *.Infrastructure 命名空间下)")
                    .ToArray();

                return violations.Length == 0
                    ? RuleResult.Ok()
                    : RuleResult.Fail($"Repository 实现类位置不正确：\n- " + string.Join("\n- ", violations));
            });
    }
}
