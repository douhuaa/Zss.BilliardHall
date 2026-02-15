namespace Zss.BilliardHall.Tests.SharedTestHelpers.FileSystem;

/// <summary>
/// 文件内容分析辅助类
/// 提供文件内容的分析功能，如关键词检查、模式匹配、表格检测等
/// 
/// 设计原则：
/// - 专注于内容分析，不涉及断言
/// - 优化性能：避免重复读取文件
/// - 支持流式处理大文件
/// 
/// 重构说明：
/// 从 FileSystemTestHelper 中提取出内容分析相关功能
/// </summary>
public static class FileContentAnalyzer
{
    /// <summary>
    /// 检查文件内容是否包含所有指定的关键词
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="keywords">关键词列表</param>
    /// <param name="ignoreCase">是否忽略大小写，默认为 false</param>
    /// <returns>如果所有关键词都存在返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentException">文件路径或关键词列表为空时抛出</exception>
    public static bool FileContainsAllKeywords(string filePath, IEnumerable<string> keywords, bool ignoreCase = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (keywords == null || !keywords.Any())
        {
            throw new ArgumentException("关键词列表不能为空", nameof(keywords));
        }

        if (!File.Exists(filePath))
        {
            return false;
        }

        var content = File.ReadAllText(filePath);
        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        return keywords.All(keyword => content.Contains(keyword, comparison));
    }

    /// <summary>
    /// 检查文件内容是否包含任一指定的关键词
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="keywords">关键词列表</param>
    /// <param name="ignoreCase">是否忽略大小写，默认为 false</param>
    /// <returns>如果任一关键词存在返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentException">文件路径或关键词列表为空时抛出</exception>
    public static bool FileContainsAnyKeyword(string filePath, IEnumerable<string> keywords, bool ignoreCase = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (keywords == null || !keywords.Any())
        {
            throw new ArgumentException("关键词列表不能为空", nameof(keywords));
        }

        if (!File.Exists(filePath))
        {
            return false;
        }

        var content = File.ReadAllText(filePath);
        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        return keywords.Any(keyword => content.Contains(keyword, comparison));
    }

    /// <summary>
    /// 获取文件中缺失的关键词列表
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="requiredKeywords">必需的关键词列表</param>
    /// <param name="ignoreCase">是否忽略大小写，默认为 false</param>
    /// <returns>缺失的关键词列表</returns>
    /// <exception cref="ArgumentException">文件路径或关键词列表为空时抛出</exception>
    public static IEnumerable<string> GetMissingKeywords(string filePath, IEnumerable<string> requiredKeywords, bool ignoreCase = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (requiredKeywords == null || !requiredKeywords.Any())
        {
            throw new ArgumentException("关键词列表不能为空", nameof(requiredKeywords));
        }

        if (!File.Exists(filePath))
        {
            return requiredKeywords;
        }

        var content = File.ReadAllText(filePath);
        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        return requiredKeywords.Where(keyword => !content.Contains(keyword, comparison));
    }

    /// <summary>
    /// 检查文件是否包含表格（Markdown 格式）
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="headerPattern">表格标题行的模式（可选）</param>
    /// <returns>如果包含表格返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentException">文件路径为空时抛出</exception>
    public static bool FileContainsTable(string filePath, string? headerPattern = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            return false;
        }

        var content = File.ReadAllText(filePath);
        var lines = content.Split('\n');

        for (int i = 0; i < lines.Length - 1; i++)
        {
            var currentLine = lines[i].Trim();
            var nextLine = lines[i + 1].Trim();

            // Markdown 表格格式：标题行 + 分隔行
            if (currentLine.Contains('|') && nextLine.StartsWith("|") && nextLine.Contains("---"))
            {
                if (string.IsNullOrEmpty(headerPattern))
                {
                    return true;
                }

                if (currentLine.Contains(headerPattern, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 统计文件中特定模式出现的次数（支持排除代码块）
    /// 使用流式读取优化大文件性能
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="pattern">正则表达式模式</param>
    /// <param name="excludeCodeBlocks">是否排除代码块中的匹配，默认为 true</param>
    /// <returns>匹配次数</returns>
    /// <exception cref="ArgumentException">文件路径或模式为空时抛出</exception>
    public static int CountPatternOccurrences(string filePath, string pattern, bool excludeCodeBlocks = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("模式不能为空", nameof(pattern));
        }

        if (!File.Exists(filePath))
        {
            return 0;
        }

        var count = 0;
        var inCodeBlock = false;

        // 使用流式读取优化性能
        using var reader = new StreamReader(filePath);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.TrimStart();

            if (excludeCodeBlocks && trimmed.StartsWith("```"))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (!excludeCodeBlocks || !inCodeBlock)
            {
                if (Regex.IsMatch(line, pattern))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// 检查文件内容是否匹配正则表达式模式
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="pattern">正则表达式模式</param>
    /// <returns>如果匹配返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentException">文件路径或模式为空时抛出</exception>
    public static bool FileContentMatches(string filePath, string pattern)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("模式不能为空", nameof(pattern));
        }

        if (!File.Exists(filePath))
        {
            return false;
        }

        var content = File.ReadAllText(filePath);
        return Regex.IsMatch(content, pattern);
    }

    /// <summary>
    /// 获取文件中匹配正则表达式的所有行
    /// 使用流式读取优化性能
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="pattern">正则表达式模式</param>
    /// <returns>匹配的行列表</returns>
    /// <exception cref="ArgumentException">文件路径或模式为空时抛出</exception>
    public static IEnumerable<string> GetMatchingLines(string filePath, string pattern)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("模式不能为空", nameof(pattern));
        }

        if (!File.Exists(filePath))
        {
            return Enumerable.Empty<string>();
        }

        var matchingLines = new List<string>();

        // 使用流式读取优化性能
        using var reader = new StreamReader(filePath);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (Regex.IsMatch(line, pattern))
            {
                matchingLines.Add(line);
            }
        }

        return matchingLines;
    }
}
