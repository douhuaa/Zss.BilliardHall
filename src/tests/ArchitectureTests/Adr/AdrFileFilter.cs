namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR 文件过滤器
/// 提供统一的文件过滤逻辑，使用 YAML Front Matter 识别文档类型
/// </summary>
public static class AdrFileFilter
{
    private static readonly Regex AdrFilePattern = new(@"ADR-\d{3,4}", RegexOptions.Compiled);

    /// <summary>
    /// 获取所有正式的 ADR 文档
    /// 使用 YAML Front Matter 识别，排除 checklist、guide、template 等配套文档
    /// </summary>
    public static IEnumerable<string> GetAdrFiles(string directory, SearchOption searchOption = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(directory))
        {
            yield break;
        }

        var files = Directory.GetFiles(directory, "ADR-*.md", searchOption);
        
        foreach (var file in files)
        {
            if (IsAdrDocument(file))
            {
                yield return file;
            }
        }
    }

    /// <summary>
    /// 判断文件是否是正式的 ADR 文档
    /// 优先使用 Front Matter，回退到文件名判断
    /// </summary>
    public static bool IsAdrDocument(string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        // 快速排除明显的非 ADR 文件
        if (fileName.Equals("README.md", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 排除 proposals 目录
        if (filePath.Contains("/proposals/", StringComparison.OrdinalIgnoreCase) ||
            filePath.Contains("\\proposals\\", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 检查文件名是否匹配 ADR 模式
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        if (!AdrFilePattern.IsMatch(fileNameWithoutExt))
        {
            return false;
        }

        // 尝试从 Front Matter 判断
        try
        {
            var (hasFrontMatter, adrField, typeField) = ParseFrontMatterQuick(filePath);
            
            if (hasFrontMatter)
            {
                // 排除明确标记为非 ADR 的类型
                if (!string.IsNullOrEmpty(typeField))
                {
                    var lowerType = typeField.ToLowerInvariant();
                    if (lowerType == "checklist" || lowerType == "guide" || 
                        lowerType == "template" || lowerType == "proposal")
                    {
                        return false;
                    }
                }

                // 如果有 adr 字段，认为是正式 ADR
                if (!string.IsNullOrEmpty(adrField))
                {
                    return true;
                }

                // 有 Front Matter 且 type 为 adr 或未指定
                return typeField == null || typeField.Equals("adr", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // 解析失败，回退到文件名判断
        }

        // 回退：根据文件名判断（排除常见的非 ADR 关键字）
        return !fileName.Contains("checklist", StringComparison.OrdinalIgnoreCase) &&
               !fileName.Contains("guide", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 快速解析 Front Matter（仅提取关键字段）
    /// 返回: (hasFrontMatter, adrField, typeField)
    /// </summary>
    private static (bool, string?, string?) ParseFrontMatterQuick(string filePath)
    {
        // 只读取文件的前几行（Front Matter 通常在开头）
        const int maxLinesToRead = 50;
        var lines = File.ReadLines(filePath).Take(maxLinesToRead).ToList();

        if (lines.Count == 0 || !lines[0].Trim().StartsWith("---"))
        {
            return (false, null, null);
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
            return (false, null, null);
        }

        // 提取 adr 和 type 字段
        string? adrField = null;
        string? typeField = null;

        for (int i = 1; i < endIndex; i++)
        {
            var line = lines[i];
            var colonIndex = line.IndexOf(':');
            if (colonIndex <= 0) continue;

            var key = line.Substring(0, colonIndex).Trim().ToLowerInvariant();
            var value = line.Substring(colonIndex + 1).Trim().Trim('"', '\'');

            if (key == "adr")
            {
                adrField = value;
            }
            else if (key == "type")
            {
                typeField = value;
            }

            // 找到两个字段就可以返回了
            if (adrField != null && typeField != null)
            {
                break;
            }
        }

        return (true, adrField, typeField);
    }

    /// <summary>
    /// 应用标准的排除过滤器
    /// 排除 README、TEMPLATE、proposals 目录
    /// </summary>
    public static IEnumerable<string> ApplyStandardExclusions(IEnumerable<string> files)
    {
        return files
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .Where(f => !Path.GetFileName(f).Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("/proposals/", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("\\proposals\\", StringComparison.OrdinalIgnoreCase));
    }
}
