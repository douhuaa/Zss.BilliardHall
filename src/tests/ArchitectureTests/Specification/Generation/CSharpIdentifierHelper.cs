using System.Text.RegularExpressions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation;

/// <summary>
/// C# 标识符辅助工具
/// 用于清理和规范化 C# 标识符（类名、方法名等）
/// </summary>
public static class CSharpIdentifierHelper
{
    private static readonly Regex ConsecutiveUnderscores = new("_{2,}", RegexOptions.Compiled);

    /// <summary>
    /// 将字符串转换为有效的 C# 标识符
    /// 规则：
    /// - 移除或替换非法字符
    /// - 确保以字母或下划线开头
    /// - 保留驼峰命名和下划线
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>有效的 C# 标识符</returns>
    public static string ToValidIdentifier(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("输入不能为空", nameof(input));
        }

        // 替换常见的特殊字符
        var identifier = input
            .Replace(" ", "_")
            .Replace("-", "_")
            .Replace("/", "_")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("<", "")
            .Replace(">", "")
            .Replace(".", "_")
            .Replace(",", "_")
            .Replace(";", "_")
            .Replace(":", "_")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("&", "And")
            .Replace("|", "Or")
            .Replace("!", "Not")
            .Replace("?", "")
            .Replace("*", "")
            .Replace("+", "Plus")
            .Replace("=", "Equals")
            .Replace("@", "At");

        // 使用正则表达式移除连续的下划线
        identifier = ConsecutiveUnderscores.Replace(identifier, "_");

        // 移除首尾下划线
        identifier = identifier.Trim('_');

        // 如果结果为空或只有下划线，返回默认值
        if (string.IsNullOrWhiteSpace(identifier) || identifier == "_")
        {
            return "Unnamed";
        }

        // 确保以字母或下划线开头
        if (!char.IsLetter(identifier[0]) && identifier[0] != '_')
        {
            identifier = "_" + identifier;
        }

        return identifier;
    }

    /// <summary>
    /// 将字符串转换为 Pascal 命名格式（首字母大写）
    /// </summary>
    public static string ToPascalCase(string input)
    {
        var identifier = ToValidIdentifier(input);
        
        if (string.IsNullOrEmpty(identifier))
        {
            return identifier;
        }

        // 如果第一个字符是下划线，保留它
        if (identifier[0] == '_')
        {
            return identifier;
        }

        return char.ToUpper(identifier[0]) + identifier.Substring(1);
    }

    /// <summary>
    /// 检查字符串是否为有效的 C# 标识符
    /// </summary>
    public static bool IsValidIdentifier(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        // 第一个字符必须是字母或下划线
        if (!char.IsLetter(input[0]) && input[0] != '_')
        {
            return false;
        }

        // 其余字符必须是字母、数字或下划线
        for (int i = 1; i < input.Length; i++)
        {
            if (!char.IsLetterOrDigit(input[i]) && input[i] != '_')
            {
                return false;
            }
        }

        return true;
    }
}
