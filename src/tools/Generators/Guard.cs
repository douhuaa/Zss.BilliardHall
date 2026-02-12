namespace Zss.BilliardHall.Generators;
using Zss.BilliardHall.Specification.Rules;
using Zss.BilliardHall.Specification.Language.RuleIdLanguage;

/// <summary>
/// 参数验证辅助类
/// 用于早期返回与参数检查，提升代码可读性
/// </summary>
public static class Guard
{
    /// <summary>
    /// 确保参数不为 null
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="value">参数值</param>
    /// <param name="parameterName">参数名</param>
    /// <returns>非 null 的参数值</returns>
    /// <exception cref="ArgumentNullException">当参数为 null 时</exception>
    public static T NotNull<T>(T? value, string parameterName) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }
        return value;
    }

    /// <summary>
    /// 确保字符串参数不为 null 或空白
    /// </summary>
    /// <param name="value">字符串值</param>
    /// <param name="parameterName">参数名</param>
    /// <returns>非空字符串</returns>
    /// <exception cref="ArgumentException">当字符串为 null 或空白时</exception>
    public static string NotNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("参数不能为 null 或空白", parameterName);
        }
        return value;
    }

    /// <summary>
    /// 确保字符串参数不为 null 或空
    /// </summary>
    /// <param name="value">字符串值</param>
    /// <param name="parameterName">参数名</param>
    /// <returns>非空字符串</returns>
    /// <exception cref="ArgumentException">当字符串为 null 或空时</exception>
    public static string NotNullOrEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("参数不能为 null 或空", parameterName);
        }
        return value;
    }

    /// <summary>
    /// 确保集合不为 null 或空
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="value">集合</param>
    /// <param name="parameterName">参数名</param>
    /// <returns>非空集合</returns>
    /// <exception cref="ArgumentException">当集合为 null 或空时</exception>
    public static IEnumerable<T> NotNullOrEmpty<T>(IEnumerable<T>? value, string parameterName)
    {
        if (value is null || !value.Any())
        {
            throw new ArgumentException("集合不能为 null 或空", parameterName);
        }
        return value;
    }

    /// <summary>
    /// 确保数值在指定范围内
    /// </summary>
    /// <param name="value">数值</param>
    /// <param name="min">最小值（含）</param>
    /// <param name="max">最大值（含）</param>
    /// <param name="parameterName">参数名</param>
    /// <returns>范围内的数值</returns>
    /// <exception cref="ArgumentOutOfRangeException">当数值不在范围内时</exception>
    public static int InRange(int value, int min, int max, string parameterName)
    {
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"值必须在 [{min}, {max}] 范围内");
        }
        return value;
    }

    /// <summary>
    /// 确保数值大于指定值
    /// </summary>
    /// <param name="value">数值</param>
    /// <param name="minValue">最小值（不含）</param>
    /// <param name="parameterName">参数名</param>
    /// <returns>符合条件的数值</returns>
    /// <exception cref="ArgumentOutOfRangeException">当数值不符合条件时</exception>
    public static int GreaterThan(int value, int minValue, string parameterName)
    {
        if (value <= minValue)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"值必须大于 {minValue}");
        }
        return value;
    }

    /// <summary>
    /// 确保数值大于或等于指定值
    /// </summary>
    /// <param name="value">数值</param>
    /// <param name="minValue">最小值（含）</param>
    /// <param name="parameterName">参数名</param>
    /// <returns>符合条件的数值</returns>
    /// <exception cref="ArgumentOutOfRangeException">当数值不符合条件时</exception>
    public static int GreaterThanOrEqual(int value, int minValue, string parameterName)
    {
        if (value < minValue)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"值必须大于或等于 {minValue}");
        }
        return value;
    }
}
