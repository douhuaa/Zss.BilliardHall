namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.RuleIdLanguage;

/// <summary>
/// RuleId 语言规范解析器
/// 负责所有 RuleId 字符串到 ArchitectureRuleId 对象的转换
/// 
/// 这是 RuleId 语义解释的唯一入口点，确保：
/// 1. RuleId 语法的一致性
/// 2. Rule / Clause 边界的明确定义
/// 3. 合法 ID 的统一判定
/// 
/// 设计原则（对齐 ADR-905）：
/// - 严格模式（ParseStrict）：用于测试/CI/Analyzer，格式错误立即失败
/// - 宽容模式（TryParse）：用于探索性查询，格式错误返回 false
/// - 所有其他代码不应该自行解析 RuleId 字符串
/// </summary>
public static class RuleIdParser
{
    /// <summary>
    /// 解析 RuleId 字符串（严格模式）
    /// 
    /// 支持的格式：
    /// - "ADR-{编号}_{Rule}" → Rule 级别
    /// - "ADR-{编号}_{Rule}_{Clause}" → Clause 级别
    /// - "ADR-{编号}.{Rule}" → Rule 级别（兼容旧格式）
    /// - "ADR-{编号}.{Rule}.{Clause}" → Clause 级别（兼容旧格式）
    /// - "{编号}_{Rule}" → Rule 级别（简化格式）
    /// - "{编号}_{Rule}_{Clause}" → Clause 级别（简化格式）
    /// 
    /// 不支持的格式会抛出 ArgumentException
    /// </summary>
    /// <param name="ruleId">RuleId 字符串</param>
    /// <returns>ArchitectureRuleId 对象</returns>
    /// <exception cref="ArgumentException">当 ruleId 为空或格式错误时抛出</exception>
    public static ArchitectureRuleId ParseStrict(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
        {
            throw new ArgumentException(
                "RuleId 不能为空或仅包含空白字符。",
                nameof(ruleId));
        }

        if (TryParse(ruleId, out var result))
        {
            return result;
        }

        throw new ArgumentException(
            $"无效的 RuleId 格式：'{ruleId}'。" +
            $"支持的格式：" +
            $"'ADR-001_1'（Rule）, 'ADR-001_1_1'（Clause）, " +
            $"'ADR-001.1'（Rule，旧格式）, 'ADR-001.1.1'（Clause，旧格式）, " +
            $"'001_1'（Rule，简化格式）, '001_1_1'（Clause，简化格式）。",
            nameof(ruleId));
    }

    /// <summary>
    /// 尝试解析 RuleId 字符串（宽容模式）
    /// 
    /// 与 ParseStrict 的区别：
    /// - ParseStrict：格式错误时抛出异常（适用于测试/CI/Analyzer）
    /// - TryParse：格式错误时返回 false（适用于探索性查询）
    /// </summary>
    /// <param name="ruleId">RuleId 字符串</param>
    /// <param name="result">解析结果</param>
    /// <returns>true 如果解析成功，否则 false</returns>
    public static bool TryParse(string ruleId, out ArchitectureRuleId result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(ruleId))
        {
            return false;
        }

        // 移除 "ADR-" 或 "ADR" 前缀
        var normalized = ruleId
            .Replace("ADR-", "", StringComparison.OrdinalIgnoreCase)
            .Replace("ADR", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        // 尝试用下划线分隔（推荐格式：ADR-XXX_Y_Z）
        var underscoreParts = normalized.Split('_');
        if (underscoreParts.Length >= 2)
        {
            if (int.TryParse(underscoreParts[0], out var adr) &&
                int.TryParse(underscoreParts[1], out var rule))
            {
                // 检查是否有 Clause 部分
                if (underscoreParts.Length >= 3 && int.TryParse(underscoreParts[2], out var clause))
                {
                    result = ArchitectureRuleId.Clause(adr, rule, clause);
                    return true;
                }

                result = ArchitectureRuleId.Rule(adr, rule);
                return true;
            }
        }

        // 尝试用点号分隔（兼容旧格式：ADR-XXX.Y.Z）
        var dotParts = normalized.Split('.');
        if (dotParts.Length >= 2)
        {
            if (int.TryParse(dotParts[0], out var adr) &&
                int.TryParse(dotParts[1], out var rule))
            {
                // 检查是否有 Clause 部分
                if (dotParts.Length >= 3 && int.TryParse(dotParts[2], out var clause))
                {
                    result = ArchitectureRuleId.Clause(adr, rule, clause);
                    return true;
                }

                result = ArchitectureRuleId.Rule(adr, rule);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 验证 RuleId 格式是否正确
    /// </summary>
    /// <param name="ruleId">RuleId 字符串</param>
    /// <returns>true 如果格式正确</returns>
    public static bool IsValidRuleId(string ruleId)
    {
        return TryParse(ruleId, out _);
    }
}
