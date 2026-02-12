namespace Zss.BilliardHall.AdrDecisionGenerator;

/// <summary>
/// YAML Front Matter 解析器
/// 职责：统一解析 Markdown 文件的 Front Matter，提供快速和完整解析两种模式
/// 设计原则：Single Responsibility Principle (SRP) - 专注于 Front Matter 解析
/// </summary>
public static class FrontMatterParser
{
    private static readonly Regex FrontMatterPattern = new(@"^---\s*\r?\n(.*?)\r?\n---\s*\r?\n", 
        RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>
    /// 提取原始 Front Matter 文本（包括 --- 分隔符）
    /// 适用于需要保留原始格式的场景，如文档合并
    /// </summary>
    /// <param name="content">Markdown 文本内容</param>
    /// <returns>原始 Front Matter 文本，如果不存在则返回 null</returns>
    public static string? ExtractRawFrontMatter(string content)
    {
        if (!content.StartsWith("---"))
            return null;

        var lines = content.Split('\n');
        var endIndex = -1;

        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "---")
            {
                endIndex = i;
                break;
            }
        }

        if (endIndex == -1)
            return null;

        return string.Join('\n', lines.Take(endIndex + 1));
    }

    /// <summary>
    /// 从文本中解析完整的 Front Matter
    /// 返回所有相关字段：adr, type, status, level, date
    /// </summary>
    /// <param name="text">Markdown 文本内容</param>
    /// <returns>解析结果，包含所有可能的 Front Matter 字段</returns>
    public static FrontMatterData ParseFromText(string text)
    {
        var match = FrontMatterPattern.Match(text);
        if (!match.Success)
        {
            return FrontMatterData.Empty;
        }

        var frontMatterText = match.Groups[1].Value;
        return ParseYamlKeyValues(frontMatterText, includeAllFields: true);
    }

    /// <summary>
    /// 从文件快速解析 Front Matter（仅读取前 N 行，优化性能）
    /// 适用于文件过滤场景，只提取判断所需的关键字段
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="maxLinesToRead">最多读取的行数（默认50行）</param>
    /// <returns>解析结果，包含关键字段</returns>
    public static FrontMatterData ParseFromFileQuick(string filePath, int maxLinesToRead = 50)
    {
        try
        {
            var lines = File.ReadLines(filePath).Take(maxLinesToRead).ToList();

            if (lines.Count == 0 || !lines[0].Trim().StartsWith("---"))
            {
                return FrontMatterData.Empty;
            }

            // 查找结束标记
            var endIndex = -1;
            for (int i = 1; i < lines.Count; i++)
            {
                if (lines[i].Trim() == "---")
                {
                    endIndex = i;
                    break;
                }
            }

            if (endIndex == -1)
            {
                return FrontMatterData.Empty;
            }

            // 提取 Front Matter 内容
            var frontMatterText = string.Join(Environment.NewLine, lines.Skip(1).Take(endIndex - 1));
            return ParseYamlKeyValues(frontMatterText, includeAllFields: false);
        }
        catch
        {
            return FrontMatterData.Empty;
        }
    }

    /// <summary>
    /// 解析简单的 YAML 键值对
    /// 支持格式：key: value 或 key: "value" 或 key: 'value'
    /// </summary>
    private static FrontMatterData ParseYamlKeyValues(string yamlText, bool includeAllFields)
    {
        var lines = yamlText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        string? adrField = null;
        string? typeField = null;
        string? statusField = null;
        string? levelField = null;
        string? dateField = null;

        foreach (var line in lines)
        {
            var colonIndex = line.IndexOf(':');
            if (colonIndex <= 0) continue;

            var key = line.Substring(0, colonIndex).Trim().ToLowerInvariant();
            var value = line.Substring(colonIndex + 1).Trim().Trim('"', '\'');

            switch (key)
            {
                case "adr":
                    adrField = value;
                    break;
                case "type":
                    typeField = value;
                    break;
                case "status":
                    if (includeAllFields) statusField = value;
                    break;
                case "level":
                    if (includeAllFields) levelField = value;
                    break;
                case "date":
                    if (includeAllFields) dateField = value;
                    break;
            }

            // 快速模式：找到关键字段后提前退出
            if (!includeAllFields && adrField != null && typeField != null)
            {
                break;
            }
        }

        return new FrontMatterData(
            HasFrontMatter: true,
            AdrField: adrField,
            TypeField: typeField,
            StatusField: statusField,
            LevelField: levelField,
            DateField: dateField
        );
    }
}

/// <summary>
/// Front Matter 解析结果
/// 不可变数据对象 (Immutable Data Object)
/// 
/// 重构说明（2026-02-09）：
/// 改为 record 类型提升简洁性和不可变性
/// </summary>
public sealed record FrontMatterData(
    bool HasFrontMatter,
    string? AdrField,
    string? TypeField,
    string? StatusField,
    string? LevelField,
    string? DateField)
{
    public static readonly FrontMatterData Empty = new(false, null, null, null, null, null);
}
