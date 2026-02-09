using System.Reflection;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Common;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Domains;

/// <summary>
/// 命名与测试组织规则集
/// 映射来源：ADR-122（测试代码组织与命名规范）
/// </summary>
public static class NamingRuleset
{
    /// <summary>
    /// 获取所有命名相关规则
    /// </summary>
    /// <param name="opt">配置选项</param>
    /// <returns>规则定义集合</returns>
    public static IEnumerable<RuleDefinition> GetRules(ArchitectureRulesOptions opt)
    {
        yield return new RuleDefinition(
            new RuleId("RS-001", "ADR-122", "1_2"),
            "架构测试必须在专用项目中",
            RuleLayer.Governance,
            SeverityLevel.L1,
            assemblies =>
            {
                var exists = AppDomain.CurrentDomain.GetAssemblies()
                    .Any(a => a.GetName().Name == opt.TestsProjectName);
                return exists
                    ? RuleResult.Ok($"项目 {opt.TestsProjectName} 存在")
                    : RuleResult.Fail($"未找到项目 {opt.TestsProjectName}");
            });

        yield return new RuleDefinition(
            new RuleId("RS-002", "ADR-122", "1_1"),
            "测试类必须以 'Tests' 结尾",
            RuleLayer.Enforcement,
            SeverityLevel.L1,
            assemblies =>
            {
                bool Filter(Type t) => 
                    t.Namespace?.Contains(".Tests") == true && 
                    t.IsClass &&
                    !t.Name.Contains("Example"); // 排除示例类
                bool NameRule(Type t) => t.Name.EndsWith("Tests") || !HasTestMethods(t);
                const string Hint = "请将测试类重命名为 {TypeName}Tests 格式";
                return RuleAdapters.NamesShouldMatch(assemblies, Filter, NameRule, Hint);
            });

        yield return new RuleDefinition(
            new RuleId("RS-003", "ADR-122", "1_3"),
            "测试项目必须遵循命名约定",
            RuleLayer.Enforcement,
            SeverityLevel.L1,
            assemblies =>
            {
                var testAssemblies = assemblies
                    .Where(a => a.GetName().Name?.Contains("Tests") == true)
                    .Select(a => a.GetName().Name!)
                    .ToArray();

                var violations = testAssemblies
                    .Where(name => !name.EndsWith(".Tests") && name != "ArchitectureTests")
                    .ToArray();

                return violations.Length == 0
                    ? RuleResult.Ok()
                    : RuleResult.Fail($"测试项目命名不符合规范：\n- " + string.Join("\n- ", violations) +
                                    "\n请使用 {Module}.Tests 或 ArchitectureTests 格式");
            });
    }

    /// <summary>
    /// 检查类型是否包含测试方法（Fact 或 Theory）
    /// </summary>
    private static bool HasTestMethods(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Any(m => m.GetCustomAttributes(false)
                .Any(attr => attr.GetType().Name is "FactAttribute" or "TheoryAttribute"));
    }
}
