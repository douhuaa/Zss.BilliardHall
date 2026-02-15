namespace Zss.BilliardHall.Tests.SharedTestHelpers.Adr;

/// <summary>
/// ADR 分类器
/// 根据 ADR 编号确定其所属分类
/// 
/// 设计原则：
/// - 单一职责：专注于 ADR 分类逻辑
/// - 可配置：支持外部配置分类规则（未来扩展）
/// - 可测试：纯函数，易于单元测试
/// 
/// 重构说明：
/// 从 AdrRelationshipMapGenerator 中提取出分类逻辑
/// </summary>
public static class AdrCategoryClassifier
{
    /// <summary>
    /// ADR 编号正则表达式模式
    /// </summary>
    private static readonly Regex AdrIdPattern = new(@"ADR-(\d{3,4})", RegexOptions.Compiled);

    /// <summary>
    /// ADR 分类定义（按编号范围）
    /// </summary>
    private static readonly (int MinNumber, int MaxNumber, string Category)[] Categories =
    {
        (0, 0, "治理（Governance）"),
        (1, 99, "宪法（Constitutional）"),
        (100, 199, "结构（Structure）"),
        (200, 299, "运行时（Runtime）"),
        (300, 399, "技术（Technical）"),
        (400, 9999, "治理（Governance）")
    };

    /// <summary>
    /// 根据 ADR 编号确定分类
    /// </summary>
    /// <param name="adrId">ADR 编号（格式：ADR-XXX 或 ADR-XXXX）</param>
    /// <returns>ADR 所属分类</returns>
    /// <exception cref="ArgumentException">当 ADR 编号格式不正确时抛出</exception>
    public static string GetCategory(string adrId)
    {
        if (string.IsNullOrWhiteSpace(adrId))
        {
            throw new ArgumentException("ADR 编号不能为空", nameof(adrId));
        }

        var match = AdrIdPattern.Match(adrId);
        if (!match.Success)
        {
            throw new ArgumentException($"无效的 ADR 编号格式: {adrId}。期望格式：ADR-XXX 或 ADR-XXXX", nameof(adrId));
        }

        var number = int.Parse(match.Groups[1].Value);
        return GetCategoryByNumber(number);
    }

    /// <summary>
    /// 根据 ADR 编号（数字）确定分类
    /// </summary>
    /// <param name="number">ADR 编号（数字部分）</param>
    /// <returns>ADR 所属分类</returns>
    public static string GetCategoryByNumber(int number)
    {
        if (number < 0)
        {
            throw new ArgumentException("ADR 编号不能为负数", nameof(number));
        }

        foreach (var (minNumber, maxNumber, category) in Categories)
        {
            if (number >= minNumber && number <= maxNumber)
            {
                return category;
            }
        }

        return "其他";
    }

    /// <summary>
    /// 尝试根据 ADR 编号确定分类（不抛出异常）
    /// </summary>
    /// <param name="adrId">ADR 编号</param>
    /// <param name="category">输出参数：分类结果</param>
    /// <returns>如果成功解析返回 true，否则返回 false</returns>
    public static bool TryGetCategory(string adrId, out string category)
    {
        category = "其他";

        if (string.IsNullOrWhiteSpace(adrId))
        {
            return false;
        }

        var match = AdrIdPattern.Match(adrId);
        if (!match.Success)
        {
            return false;
        }

        var number = int.Parse(match.Groups[1].Value);
        category = GetCategoryByNumber(number);
        return true;
    }

    /// <summary>
    /// 获取所有支持的分类列表（去重）
    /// </summary>
    /// <returns>分类名称列表</returns>
    public static IReadOnlyList<string> GetAllCategories()
    {
        return Categories
            .Select(c => c.Category)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// 判断 ADR 编号是否属于特定分类
    /// </summary>
    /// <param name="adrId">ADR 编号</param>
    /// <param name="expectedCategory">期望的分类</param>
    /// <returns>如果属于该分类返回 true，否则返回 false</returns>
    public static bool IsInCategory(string adrId, string expectedCategory)
    {
        if (!TryGetCategory(adrId, out var actualCategory))
        {
            return false;
        }

        return string.Equals(actualCategory, expectedCategory, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 判断 ADR 编号是否属于宪法层（Constitutional）
    /// </summary>
    /// <param name="adrId">ADR 编号</param>
    /// <returns>如果属于宪法层返回 true，否则返回 false</returns>
    public static bool IsConstitutional(string adrId)
    {
        return IsInCategory(adrId, "宪法（Constitutional）");
    }

    /// <summary>
    /// 判断 ADR 编号是否属于治理层（Governance）
    /// </summary>
    /// <param name="adrId">ADR 编号</param>
    /// <returns>如果属于治理层返回 true，否则返回 false</returns>
    public static bool IsGovernance(string adrId)
    {
        return IsInCategory(adrId, "治理（Governance）");
    }
}
