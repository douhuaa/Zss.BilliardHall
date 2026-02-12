using System.Text.RegularExpressions;
using Zss.BilliardHall.Generators.Interfaces;

namespace Zss.BilliardHall.Generators.Implementations;

/// <summary>
/// 命令验证器实现
/// </summary>
public sealed class CommandValidator : ICommandValidator
{
    // 匹配危险命令模式，但排除转义序列
    private static readonly Regex DangerousCommandPattern = new(
        @"(?<!\\)(rm\s+-rf|del\s+/[sqf]|format\s|shutdown|reboot)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// 验证命令字符串是否有效（不包含危险模式）
    /// </summary>
    public bool IsValidCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return false;

        try
        {
            return !DangerousCommandPattern.IsMatch(command);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// 清理命令字符串（移除危险字符）
    /// </summary>
    public string SanitizeCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return string.Empty;

        // 移除换行符和控制字符
        var sanitized = command
            .Replace("\r", string.Empty)
            .Replace("\n", " ");

        return sanitized.Trim();
    }
}
