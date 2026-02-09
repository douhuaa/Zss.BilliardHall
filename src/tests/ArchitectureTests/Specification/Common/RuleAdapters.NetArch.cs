using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Common;

/// <summary>
/// NetArchTest 适配器：将 NetArchTest.Rules 的验证逻辑适配到统一的 RuleResult 模型
/// </summary>
public static class RuleAdapters
{
    /// <summary>
    /// 验证命名规则：检查类型名称是否符合指定规则
    /// </summary>
    /// <param name="assemblies">要检查的程序集</param>
    /// <param name="filter">类型过滤条件</param>
    /// <param name="nameRule">命名规则检查函数</param>
    /// <param name="failHint">失败时的提示信息</param>
    /// <returns>规则执行结果</returns>
    public static RuleResult NamesShouldMatch(
        Assembly[] assemblies,
        Func<Type, bool> filter,
        Func<Type, bool> nameRule,
        string failHint)
    {
        var violations = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(filter)
            .Where(t => !nameRule(t))
            .Select(t => t.FullName ?? t.Name)
            .ToArray();

        return violations.Length == 0
            ? RuleResult.Ok()
            : RuleResult.Fail($"命名规则违规：\n- " + string.Join("\n- ", violations) + $"\n{failHint}");
    }

    /// <summary>
    /// 验证依赖规则：检查类型依赖是否符合指定规则
    /// </summary>
    /// <param name="assemblies">要检查的程序集</param>
    /// <param name="filter">类型过滤条件</param>
    /// <param name="dependencyRule">依赖规则检查函数</param>
    /// <param name="failHint">失败时的提示信息</param>
    /// <returns>规则执行结果</returns>
    public static RuleResult DependenciesShouldMatch(
        Assembly[] assemblies,
        Func<Type, bool> filter,
        Func<Type, bool> dependencyRule,
        string failHint)
    {
        var violations = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(filter)
            .Where(t => !dependencyRule(t))
            .Select(t => t.FullName ?? t.Name)
            .ToArray();

        return violations.Length == 0
            ? RuleResult.Ok()
            : RuleResult.Fail($"依赖规则违规：\n- " + string.Join("\n- ", violations) + $"\n{failHint}");
    }

    /// <summary>
    /// 验证存在性规则：检查特定类型是否存在
    /// </summary>
    /// <param name="assemblies">要检查的程序集</param>
    /// <param name="predicate">存在性条件</param>
    /// <param name="successMsg">成功消息</param>
    /// <param name="failMsg">失败消息</param>
    /// <returns>规则执行结果</returns>
    public static RuleResult ShouldExist(
        Assembly[] assemblies,
        Func<Type, bool> predicate,
        string successMsg,
        string failMsg)
    {
        var exists = assemblies
            .SelectMany(a => a.GetTypes())
            .Any(predicate);

        return exists
            ? RuleResult.Ok(successMsg)
            : RuleResult.Fail(failMsg);
    }

    /// <summary>
    /// 验证计数规则：检查符合条件的类型数量
    /// </summary>
    /// <param name="assemblies">要检查的程序集</param>
    /// <param name="filter">类型过滤条件</param>
    /// <param name="countRule">计数规则检查函数（输入：实际数量）</param>
    /// <param name="failHint">失败时的提示信息</param>
    /// <returns>规则执行结果</returns>
    public static RuleResult CountShouldMatch(
        Assembly[] assemblies,
        Func<Type, bool> filter,
        Func<int, bool> countRule,
        string failHint)
    {
        var types = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(filter)
            .ToArray();

        var count = types.Length;
        var passed = countRule(count);

        return passed
            ? RuleResult.Ok($"找到 {count} 个符合条件的类型")
            : RuleResult.Fail($"类型数量不符合要求：实际 {count} 个\n{failHint}");
    }
}
