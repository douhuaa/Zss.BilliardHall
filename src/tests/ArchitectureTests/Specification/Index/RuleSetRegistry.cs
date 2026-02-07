namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Index;

using System.Text.RegularExpressions;
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
    /// ADR 编号格式验证正则表达式
    /// 匹配格式：
    /// - "ADR-" 后跟 1-3 位数字（如 ADR-001, ADR-1）
    /// - "ADR" 后跟 1-3 位数字（如 ADR001, ADR1）
    /// - 纯 1-3 位数字（如 001, 1）
    /// 不匹配：ADR0001（4位数字）, ADR.001（错误分隔符）
    /// </summary>
    private static readonly Regex AdrPattern = new(@"^(ADR-?)?\d{1,3}$", RegexOptions.IgnoreCase);

    /// <summary>
    /// 所有已注册的规则集
    /// Key: ADR 编号
    /// Value: 规则集定义
    /// </summary>
    public static IReadOnlyDictionary<int, ArchitectureRuleSet> All => LazyRegistry.Value;

    /// <summary>
    /// 根据 ADR 编号获取规则集（宽容模式）
    /// </summary>
    /// <param name="adrNumber">ADR 编号</param>
    /// <returns>规则集，如果不存在则返回 null</returns>
    public static ArchitectureRuleSet? Get(int adrNumber)
    {
        return All.TryGetValue(adrNumber, out var ruleSet) ? ruleSet : null;
    }

    /// <summary>
    /// 根据 ADR 编号获取规则集（严格模式）
    /// 
    /// 与 Get() 的区别：
    /// - Get()：宽容模式，不存在时返回 null（适用于探索性查询）
    /// - GetStrict()：严格模式，不存在时抛出异常（适用于测试/CI/Analyzer）
    /// 
    /// 设计原则：
    /// - RuleId 是裁决系统的"法律编号"，不是用户输入
    /// - 在测试、CI、Analyzer 场景中，无效 RuleId = 架构错误
    /// - 显式失败优于静默返回 null
    /// </summary>
    /// <param name="adrNumber">ADR 编号</param>
    /// <returns>规则集</returns>
    /// <exception cref="InvalidOperationException">当规则集不存在时抛出</exception>
    public static ArchitectureRuleSet GetStrict(int adrNumber)
    {
        if (All.TryGetValue(adrNumber, out var ruleSet))
        {
            return ruleSet;
        }

        throw new InvalidOperationException(
            $"无效的 ADR 编号：{adrNumber}。" +
            $"该 ADR 规则集不存在或尚未注册。" +
            $"可用的 ADR 编号：{string.Join(", ", GetAllAdrNumbers())}");
    }

    /// <summary>
    /// 根据 ADR 编号字符串获取规则集（宽容模式）
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
    /// 根据 ADR 编号字符串获取规则集（严格模式）
    /// 
    /// 与 Get() 的区别：
    /// - Get()：宽容模式，格式错误或不存在时返回 null
    /// - GetStrict()：严格模式，格式错误或不存在时抛出异常
    /// 
    /// 支持的格式：
    /// - "ADR-001" 或 "ADR-1"（推荐格式）
    /// - "001" 或 "1"（简化格式）
    /// - "ADR001"（无分隔符格式，仅限 1-3 位数字）
    /// 
    /// 不支持的格式：
    /// - "ADR0001"（4位数字）
    /// - "0001"（4位数字）
    /// - "ADR.001"（错误分隔符）
    /// - 其他非标准格式
    /// 
    /// 设计原则：
    /// - RuleId 是裁决系统的"法律编号"，不是用户输入
    /// - 在测试、CI、Analyzer 场景中，格式错误 = 架构错误
    /// - 显式失败优于静默返回 null
    /// - 格式验证通过正则表达式严格匹配，确保代码行为与注释一致
    /// </summary>
    /// <param name="adrId">ADR 编号字符串</param>
    /// <returns>规则集</returns>
    /// <exception cref="ArgumentException">当 adrId 为空或格式错误时抛出</exception>
    /// <exception cref="InvalidOperationException">当规则集不存在时抛出</exception>
    public static ArchitectureRuleSet GetStrict(string adrId)
    {
        if (string.IsNullOrWhiteSpace(adrId))
        {
            throw new ArgumentException(
                "ADR 编号不能为空或仅包含空白字符。",
                nameof(adrId));
        }

        // 使用正则表达式严格验证格式
        if (!AdrPattern.IsMatch(adrId))
        {
            throw new ArgumentException(
                $"无效的 ADR 编号格式：'{adrId}'。" +
                $"支持的格式：'ADR-001', 'ADR-1', '001', '1', 'ADR001'（仅 1-3 位数字）。" +
                $"不支持 4 位数字格式如 'ADR0001' 或 '0001'。",
                nameof(adrId));
        }

        // 提取数字部分
        var normalized = adrId.Replace("ADR-", "", StringComparison.OrdinalIgnoreCase)
                               .Replace("ADR", "", StringComparison.OrdinalIgnoreCase)
                               .Trim();

        // 此时 normalized 必定可以解析为 int（因为已通过正则验证）
        var adrNumber = int.Parse(normalized);

        return GetStrict(adrNumber);
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
