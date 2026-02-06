namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Index;

using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0001;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0002;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0003;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0120;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0201;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0900;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0907;

/// <summary>
/// 规则集注册表
/// 架构测试、CLI、CI、Analyzer 的唯一规则集访问入口
/// 
/// 设计原则：
/// 1. 测试禁止直接 new RuleSet()，必须通过 Registry 获取
/// 2. 提供统一的规则集查询和访问接口
/// 3. 支持按 ADR 编号、范围、严重程度等维度查询
/// 4. 为未来的规则集验证、版本管理等功能预留扩展点
/// </summary>
public static class RuleSetRegistry
{
    private static readonly Lazy<IReadOnlyDictionary<int, ArchitectureRuleSet>> LazyRegistry =
        new(BuildRegistry);

    /// <summary>
    /// 所有已注册的规则集
    /// Key: ADR 编号
    /// Value: 规则集定义
    /// </summary>
    public static IReadOnlyDictionary<int, ArchitectureRuleSet> All => LazyRegistry.Value;

    /// <summary>
    /// 根据 ADR 编号获取规则集
    /// </summary>
    /// <param name="adrNumber">ADR 编号</param>
    /// <returns>规则集，如果不存在则返回 null</returns>
    public static ArchitectureRuleSet? Get(int adrNumber)
    {
        return All.TryGetValue(adrNumber, out var ruleSet) ? ruleSet : null;
    }

    /// <summary>
    /// 根据 ADR 编号字符串获取规则集
    /// </summary>
    /// <param name="adrId">ADR 编号字符串，格式如 "ADR-001" 或 "1"</param>
    /// <returns>规则集，如果不存在或格式错误则返回 null</returns>
    public static ArchitectureRuleSet? Get(string adrId)
    {
        if (string.IsNullOrWhiteSpace(adrId))
        {
            return null;
        }

        // 支持 "ADR-001" 格式
        var normalized = adrId.Replace("ADR-", "", StringComparison.OrdinalIgnoreCase)
                               .Replace("ADR", "", StringComparison.OrdinalIgnoreCase)
                               .Trim();

        if (int.TryParse(normalized, out var adrNumber))
        {
            return Get(adrNumber);
        }

        return null;
    }

    /// <summary>
    /// 检查指定 ADR 编号的规则集是否存在
    /// </summary>
    /// <param name="adrNumber">ADR 编号</param>
    /// <returns>true 如果规则集已注册</returns>
    public static bool Contains(int adrNumber)
    {
        return All.ContainsKey(adrNumber);
    }

    /// <summary>
    /// 获取所有已注册的 ADR 编号
    /// </summary>
    public static IEnumerable<int> GetAllAdrNumbers()
    {
        return All.Keys.OrderBy(x => x);
    }

    /// <summary>
    /// 获取所有已注册的规则集
    /// </summary>
    public static IEnumerable<ArchitectureRuleSet> GetAllRuleSets()
    {
        return All.Values.OrderBy(x => x.AdrNumber);
    }

    /// <summary>
    /// 按严重程度筛选规则集
    /// </summary>
    /// <param name="severity">严重程度</param>
    /// <returns>符合条件的规则集</returns>
    public static IEnumerable<ArchitectureRuleSet> GetBySeverity(RuleSeverity severity)
    {
        return All.Values
            .Where(rs => rs.Rules.Any(r => r.Severity == severity))
            .OrderBy(rs => rs.AdrNumber);
    }

    /// <summary>
    /// 按作用域筛选规则集
    /// </summary>
    /// <param name="scope">作用域</param>
    /// <returns>符合条件的规则集</returns>
    public static IEnumerable<ArchitectureRuleSet> GetByScope(RuleScope scope)
    {
        return All.Values
            .Where(rs => rs.Rules.Any(r => r.Scope == scope))
            .OrderBy(rs => rs.AdrNumber);
    }

    /// <summary>
    /// 获取宪法层 ADR 规则集（ADR-001 ~ ADR-008）
    /// </summary>
    public static IEnumerable<ArchitectureRuleSet> GetConstitutionalRuleSets()
    {
        return All.Values
            .Where(rs => rs.AdrNumber >= 1 && rs.AdrNumber <= 8)
            .OrderBy(rs => rs.AdrNumber);
    }

    /// <summary>
    /// 获取治理层 ADR 规则集（ADR-900 ~ ADR-999）
    /// </summary>
    public static IEnumerable<ArchitectureRuleSet> GetGovernanceRuleSets()
    {
        return All.Values
            .Where(rs => rs.AdrNumber >= 900 && rs.AdrNumber <= 999)
            .OrderBy(rs => rs.AdrNumber);
    }

    /// <summary>
    /// 获取运行时 ADR 规则集（ADR-201 ~ ADR-240）
    /// </summary>
    public static IEnumerable<ArchitectureRuleSet> GetRuntimeRuleSets()
    {
        return All.Values
            .Where(rs => rs.AdrNumber >= 201 && rs.AdrNumber <= 240)
            .OrderBy(rs => rs.AdrNumber);
    }

    /// <summary>
    /// 获取结构层 ADR 规则集（ADR-120 ~ ADR-124）
    /// </summary>
    public static IEnumerable<ArchitectureRuleSet> GetStructureRuleSets()
    {
        return All.Values
            .Where(rs => rs.AdrNumber >= 120 && rs.AdrNumber <= 124)
            .OrderBy(rs => rs.AdrNumber);
    }

    private static IReadOnlyDictionary<int, ArchitectureRuleSet> BuildRegistry()
    {
        var registry = new Dictionary<int, ArchitectureRuleSet>();

        // 注册所有 RuleSet
        // Constitutional ADRs
        Register(registry, Adr0001RuleSet.AdrNumber, Adr0001RuleSet.RuleSet);
        Register(registry, Adr0002RuleSet.AdrNumber, Adr0002RuleSet.RuleSet);
        Register(registry, Adr0003RuleSet.AdrNumber, Adr0003RuleSet.RuleSet);

        // Structure ADRs
        Register(registry, Adr0120RuleSet.AdrNumber, Adr0120RuleSet.RuleSet);

        // Runtime ADRs
        Register(registry, Adr0201RuleSet.AdrNumber, Adr0201RuleSet.RuleSet);

        // Governance ADRs
        Register(registry, Adr0900RuleSet.AdrNumber, Adr0900RuleSet.RuleSet);
        Register(registry, Adr0907RuleSet.AdrNumber, Adr0907RuleSet.RuleSet);

        return registry;
    }

    private static void Register(
        Dictionary<int, ArchitectureRuleSet> registry,
        int adrNumber,
        ArchitectureRuleSet ruleSet)
    {
        if (registry.ContainsKey(adrNumber))
        {
            throw new InvalidOperationException(
                $"规则集 ADR-{adrNumber:D3} 已注册, 不能重复注册");
        }

        registry.Add(adrNumber, ruleSet);
    }
}
