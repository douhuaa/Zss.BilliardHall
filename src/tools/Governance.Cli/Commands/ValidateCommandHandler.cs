using Zss.BilliardHall.Specification;
using Zss.BilliardHall.Specification.Index;
using Zss.BilliardHall.Specification.Rules;

namespace Zss.BilliardHall.Tools.Governance.Cli.Commands;

/// <summary>
/// 校验 RuleSetRegistry 注册完整性与 RuleId 格式
/// </summary>
public sealed class ValidateCommandHandler
{
    public Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine("🔍 开始校验 RuleSetRegistry...\n");

            var allRuleSets = RuleSetRegistry.GetAllRuleSets().ToList();
            Console.WriteLine($"📊 共找到 {allRuleSets.Count} 个 RuleSet");

            var totalRules = 0;
            var totalClauses = 0;
            var validationErrors = new List<string>();

            foreach (var ruleSet in allRuleSets)
            {
                Console.WriteLine($"\n📖 校验 ADR-{ruleSet.AdrNumber:D3}");

                // 校验所有 Rules
                foreach (var rule in ruleSet.Rules)
                {
                    totalRules++;

                    // 校验 Rule 内容
                    if (string.IsNullOrWhiteSpace(rule.Summary))
                    {
                        validationErrors.Add($"{rule.Id}: Summary 为空");
                    }
                }

                // 校验所有 Clauses
                foreach (var clause in ruleSet.Clauses)
                {
                    totalClauses++;

                    // 校验 Clause 内容
                    if (string.IsNullOrWhiteSpace(clause.Condition))
                    {
                        validationErrors.Add($"{clause.Id}: Condition 为空");
                    }

                    if (string.IsNullOrWhiteSpace(clause.Enforcement))
                    {
                        validationErrors.Add($"{clause.Id}: Enforcement 为空");
                    }
                }

                Console.WriteLine($"   ✅ Rules: {ruleSet.RuleCount}, Clauses: {ruleSet.ClauseCount}");
            }

            // 打印统计信息
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("📊 统计信息:");
            Console.WriteLine($"   RuleSet 总数: {allRuleSets.Count}");
            Console.WriteLine($"   Rule 总数: {totalRules}");
            Console.WriteLine($"   Clause 总数: {totalClauses}");

            // 校验 AdrRuleIndex 完整性
            Console.WriteLine("\n🔍 校验 AdrRuleIndex 完整性...");
            var indexErrors = ValidateAdrRuleIndex(allRuleSets);
            validationErrors.AddRange(indexErrors);

            // 打印错误
            if (validationErrors.Count > 0)
            {
                Console.WriteLine("\n❌ 发现以下问题:");
                foreach (var error in validationErrors)
                {
                    Console.WriteLine($"   • {error}");
                }
                return Task.FromResult(1);
            }

            Console.WriteLine("\n✅ 校验通过！所有 RuleSet、Rule 和 Clause 格式正确。");
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 校验失败: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   内部异常: {ex.InnerException.Message}");
            }
            return Task.FromResult(1);
        }
    }

    private static List<string> ValidateAdrRuleIndex(List<ArchitectureRuleSet> ruleSets)
    {
        var errors = new List<string>();

        foreach (var ruleSet in ruleSets)
        {
            foreach (var rule in ruleSet.Rules)
            {
                // 校验 Rule 是否可以通过 Index 查询到
                var foundRule = AdrRuleIndex.GetRule(rule.Id);
                if (foundRule == null)
                {
                    errors.Add($"AdrRuleIndex: Rule '{rule.Id}' 不存在于索引中");
                }
            }

            foreach (var clause in ruleSet.Clauses)
            {
                // 校验 Clause 是否可以通过 Index 查询到
                var foundClause = AdrRuleIndex.GetClause(clause.Id);
                if (foundClause == null)
                {
                    errors.Add($"AdrRuleIndex: Clause '{clause.Id}' 不存在于索引中");
                }
            }
        }

        if (errors.Count == 0)
        {
            Console.WriteLine("   ✅ AdrRuleIndex 索引完整");
        }

        return errors;
    }
}
