namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// ADR 文件过滤器
/// 提供统一的文件过滤逻辑，使用 YAML Front Matter 识别文档类型
/// 
/// **最佳实践**：
/// - 所有测试代码应使用 AdrFileFilter.GetAdrFiles() 而不是 Directory.GetFiles()
/// - 这确保过滤逻辑统一，通过 YAML Front Matter 精确识别真正的 ADR 文档
/// - 避免将 TEMPLATE、CHECKLIST、guide、proposal 等非 ADR 文档误认为 ADR
/// 
/// **使用场景**：
/// - 用于整个架构测试项目中需要查找和处理 ADR 文档的场景
/// - 替代直接使用 Directory.GetFiles() 的简单文件名匹配
/// 
/// **重构说明**：
/// - 现在委托给 AdrDocumentClassifier 进行文档分类
/// - 委托给 FrontMatterParser 进行 Front Matter 解析
/// - 保持高性能的快速过滤特性
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
    /// 委托给 AdrDocumentClassifier 统一处理
    /// </summary>
    public static bool IsAdrDocument(string filePath)
    {
        // 检查文件名是否匹配 ADR 模式（性能优化：提前过滤）
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        if (!AdrFilePattern.IsMatch(fileNameWithoutExt))
        {
            return false;
        }

        // 委托给统一的文档分类器
        return AdrDocumentClassifier.IsAdrDocument(filePath);
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
