namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// ADR 文档分类器
/// 职责：统一判断文件是否是正式的 ADR 文档
/// 设计原则：
/// - Single Responsibility Principle (SRP) - 专注于文档分类逻辑
/// - Open/Closed Principle (OCP) - 易于扩展新的分类规则
/// </summary>
public static class AdrDocumentClassifier
{
    /// <summary>
    /// 判断文件是否是正式的 ADR 文档
    /// 
    /// 判断规则（按优先级）：
    /// 1. 文件名快速排除：README、TEMPLATE
    /// 2. 目录排除：proposals
    /// 3. Front Matter 类型检查：排除 checklist、guide、template、proposal
    /// 4. ADR 字段检查：有 adr 字段视为正式 ADR
    /// 5. 文件名回退规则：排除包含 checklist、guide 关键字的文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="frontMatter">Front Matter 解析结果（可选，如已解析可传入以提高性能）</param>
    /// <returns>是否是正式的 ADR 文档</returns>
    public static bool IsAdrDocument(string filePath, FrontMatterData? frontMatter = null)
    {
        var fileName = Path.GetFileName(filePath);

        // 规则 1: 快速排除明显的非 ADR 文件
        if (IsExcludedByFileName(fileName))
        {
            return false;
        }

        // 规则 2: 排除 proposals 目录
        if (IsInProposalsDirectory(filePath))
        {
            return false;
        }

        // 规则 3-5: 使用 Front Matter 判断（如未提供则自动解析）
        frontMatter ??= FrontMatterParser.ParseFromFileQuick(filePath);
        return IsAdrByFrontMatter(frontMatter, fileName);
    }

    /// <summary>
    /// 基于 Front Matter 和文件名判断是否是 ADR
    /// （内部逻辑，供 AdrParser 和 AdrFileFilter 共享）
    /// </summary>
    public static bool IsAdrByFrontMatter(FrontMatterData frontMatter, string fileName)
    {
        if (frontMatter.HasFrontMatter)
        {
            // 规则 3: 排除明确标记为非 ADR 的类型
            if (IsExcludedByType(frontMatter.TypeField))
            {
                return false;
            }

            // 规则 4: 如果有 adr 字段，认为是正式 ADR
            if (!string.IsNullOrEmpty(frontMatter.AdrField))
            {
                return true;
            }

            // 有 Front Matter 且 type 为 adr 或未指定
            return frontMatter.TypeField == null || 
                   frontMatter.TypeField.Equals("adr", StringComparison.OrdinalIgnoreCase);
        }

        // 规则 5: 回退 - 根据文件名判断（排除常见的非 ADR 关键字）
        return !fileName.Contains("checklist", StringComparison.OrdinalIgnoreCase) &&
               !fileName.Contains("guide", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 检查文件名是否应被排除
    /// </summary>
    private static bool IsExcludedByFileName(string fileName)
    {
        return fileName.Equals("README.md", StringComparison.OrdinalIgnoreCase) ||
               fileName.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 检查文件是否在 proposals 目录中
    /// </summary>
    private static bool IsInProposalsDirectory(string filePath)
    {
        return filePath.Contains("/proposals/", StringComparison.OrdinalIgnoreCase) ||
               filePath.Contains("\\proposals\\", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 检查类型字段是否表示非 ADR 文档
    /// </summary>
    private static bool IsExcludedByType(string? typeField)
    {
        if (string.IsNullOrEmpty(typeField))
        {
            return false;
        }

        var lowerType = typeField.ToLowerInvariant();
        return lowerType == "checklist" || 
               lowerType == "guide" || 
               lowerType == "template" || 
               lowerType == "proposal";
    }
}
