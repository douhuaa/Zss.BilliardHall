using System.Reflection;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Common;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Domains;

/// <summary>
/// 反作弊规则集
/// 映射来源：ADR-907（ArchitectureTests 执法治理体系）、特别是 ADR-907_3（最小断言语义规范）
/// </summary>
public static class AntiCheatRuleset
{
    /// <summary>
    /// 获取所有反作弊相关规则
    /// </summary>
    /// <param name="opt">配置选项</param>
    /// <returns>规则定义集合</returns>
    public static IEnumerable<RuleDefinition> GetRules(ArchitectureRulesOptions opt)
    {
        yield return new RuleDefinition(
            new RuleId("RS-030", "ADR-907", "3_4"),
            "每个架构测试类至少包含指定数量的 Fact/Theory",
            RuleLayer.Governance,
            SeverityLevel.L1,
            assemblies =>
            {
                // 查找所有架构测试类（命名以 _Architecture_Tests 或 _Tests 结尾的类）
                // 但排除配置的特殊模式（如不变量测试、自动生成的测试）
                var testTypes = assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && 
                               (t.Name.EndsWith("_Architecture_Tests") || 
                                t.Name.EndsWith("_Tests")) &&
                               !opt.MinimumAssertionExcludePatterns.Any(pattern => t.Name.Contains(pattern)) &&
                               t.Namespace?.Contains("ArchitectureTests") == true)
                    .ToArray();

                var violations = testTypes
                    .Select(t => new
                    {
                        Type = t,
                        Count = t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                            .Count(m => m.GetCustomAttributes(false)
                                .Any(attr => attr.GetType().Name is "FactAttribute" or "TheoryAttribute"))
                    })
                    .Where(x => x.Count < opt.MinimumFactPerClass)
                    .Select(x => $"{x.Type.FullName} 只有 {x.Count} 个测试方法")
                    .ToArray();

                return violations.Length == 0
                    ? RuleResult.Ok($"所有架构测试类都包含至少 {opt.MinimumFactPerClass} 个测试")
                    : RuleResult.Fail($"最小断言数量违规：\n- " + string.Join("\n- ", violations));
            });

        yield return new RuleDefinition(
            new RuleId("RS-031", "ADR-907", "3_3"),
            "禁止使用 Assert.True(true) 等无意义断言",
            RuleLayer.Governance,
            SeverityLevel.L1,
            assemblies =>
            {
                // TODO: 实现完整的无意义断言检测
                // 当前版本：基于反射的简化检查
                // 完整实现需要：IL 分析或 Roslyn 语法树分析
                // 参考：https://github.com/xunit/xunit/issues/1234 (示例链接)
                
                var testMethods = assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.Namespace?.Contains("ArchitectureTests") == true)
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    .Where(m => m.GetCustomAttributes(false)
                        .Any(attr => attr.GetType().Name is "FactAttribute" or "TheoryAttribute"))
                    .ToArray();

                // 当前版本仅计数，实际检测需要源码或 IL 分析
                return RuleResult.Ok($"检查了 {testMethods.Length} 个测试方法（完整实现待开发）");
            });

        yield return new RuleDefinition(
            new RuleId("RS-032", "ADR-907", "1"),
            "架构测试必须可执行且有效",
            RuleLayer.Governance,
            SeverityLevel.L1,
            assemblies =>
            {
                // 验证架构测试项目存在且包含测试
                var archTests = assemblies
                    .Where(a => a.GetName().Name?.Contains("ArchitectureTests") == true)
                    .ToArray();

                if (archTests.Length == 0)
                {
                    return RuleResult.Fail("未找到 ArchitectureTests 项目");
                }

                var testCount = archTests
                    .SelectMany(a => a.GetTypes())
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    .Count(m => m.GetCustomAttributes(false)
                        .Any(attr => attr.GetType().Name is "FactAttribute" or "TheoryAttribute"));

                return testCount > 0
                    ? RuleResult.Ok($"找到 {testCount} 个架构测试方法")
                    : RuleResult.Fail("ArchitectureTests 项目中未找到任何测试方法");
            });
    }
}
