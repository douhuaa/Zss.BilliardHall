namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.FileSystem;

/// <summary>
/// 文件系统测试辅助类（向后兼容桥接类）
/// 
/// ⚠️ 此类已重构为三个专用类，请使用新的辅助类：
/// - FileAssertionHelper：文件和目录断言
/// - FileContentAnalyzer：内容分析（关键词、模式、表格）
/// - FileSearchHelper：文件搜索和路径操作
/// 
/// 此类保留作为向后兼容性桥接，但应被视为已废弃。
/// 新代码应直接使用上述专用辅助类。
/// 
/// 重构说明：
/// - 原 15+ 个方法已拆分到三个专用类中
/// - 提升了单一职责原则（SRP）遵循度
/// - 优化了性能（使用流式读取）
/// - 添加了参数验证
/// </summary>
[Obsolete("使用 FileAssertionHelper、FileContentAnalyzer 或 FileSearchHelper 代替。此类仅为向后兼容保留。", false)]
public static class FileSystemTestHelper
{
    /// <summary>
    /// 断言文件存在，如果不存在则抛出带有详细信息的异常
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="failureMessage">失败时的错误消息</param>
    [Obsolete("使用 FileAssertionHelper.AssertFileExists 代替", false)]
    public static void AssertFileExists(string filePath, string failureMessage)
    {
        FileAssertionHelper.AssertFileExists(filePath, failureMessage);
    }


    /// <summary>
    /// 断言目录存在，如果不存在则抛出带有详细信息的异常
    /// </summary>
    /// <param name="directoryPath">目录路径（绝对路径）</param>
    /// <param name="failureMessage">失败时的错误消息</param>
    [Obsolete("使用 FileAssertionHelper.AssertDirectoryExists 代替", false)]
    public static void AssertDirectoryExists(string directoryPath, string failureMessage)
    {
        FileAssertionHelper.AssertDirectoryExists(directoryPath, failureMessage);
    }

    /// <summary>
    /// 安全读取文件内容
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <returns>文件内容字符串</returns>
    /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
    [Obsolete("使用 FileSearchHelper.ReadFileContent 代替", false)]
    public static string ReadFileContent(string filePath)
    {
        return FileSearchHelper.ReadFileContent(filePath);
    }

    /// <summary>
    /// 获取目录中的文件列表
    /// </summary>
    /// <param name="directoryPath">目录路径（绝对路径）</param>
    /// <param name="searchPattern">搜索模式（如 "*.cs"、"*.md"），默认为 "*"</param>
    /// <param name="searchOption">搜索选项，默认为 TopDirectoryOnly</param>
    /// <returns>文件路径列表</returns>
    [Obsolete("使用 FileSearchHelper.GetFilesInDirectory 代替", false)]
    public static IReadOnlyList<string> GetFilesInDirectory(
        string directoryPath,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return FileSearchHelper.GetFilesInDirectory(directoryPath, searchPattern, searchOption);
    }

    /// <summary>
    /// 获取目录的子目录列表
    /// </summary>
    /// <param name="directoryPath">目录路径（绝对路径）</param>
    /// <returns>子目录路径列表</returns>
    [Obsolete("使用 FileSearchHelper.GetSubdirectories 代替", false)]
    public static IReadOnlyList<string> GetSubdirectories(string directoryPath)
    {
        return FileSearchHelper.GetSubdirectories(directoryPath);
    }

    /// <summary>
    /// 断言文件内容包含指定文本
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="expectedContent">期望包含的内容</param>
    /// <param name="failureMessage">失败时的错误消息</param>
    [Obsolete("使用 FileAssertionHelper.AssertFileContains 代替", false)]
    public static void AssertFileContains(string filePath, string expectedContent, string failureMessage)
    {
        FileAssertionHelper.AssertFileContains(filePath, expectedContent, failureMessage);
    }

    /// <summary>
    /// 断言文件内容长度大于指定值
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="minLength">最小长度</param>
    /// <param name="failureMessage">失败时的错误消息</param>
    [Obsolete("使用 FileAssertionHelper.AssertFileContentLength 代替", false)]
    public static void AssertFileContentLength(string filePath, int minLength, string failureMessage)
    {
        FileAssertionHelper.AssertFileContentLength(filePath, minLength, failureMessage);
    }

    /// <summary>
    /// 获取相对于仓库根目录的相对路径
    /// </summary>
    /// <param name="fullPath">完整路径</param>
    /// <returns>相对路径</returns>
    [Obsolete("使用 FileSearchHelper.GetRelativePath 代替", false)]
    public static string GetRelativePath(string fullPath)
    {
        return FileSearchHelper.GetRelativePath(fullPath);
    }

    /// <summary>
    /// 组合仓库根目录和相对路径，返回绝对路径
    /// </summary>
    /// <param name="relativePath">相对于仓库根目录的路径</param>
    /// <returns>绝对路径</returns>
    [Obsolete("使用 FileSearchHelper.GetAbsolutePath 代替", false)]
    public static string GetAbsolutePath(string relativePath)
    {
        return FileSearchHelper.GetAbsolutePath(relativePath);
    }

    /// <summary>
    /// 获取指定目录下所有 ADR 文档文件
    /// 使用 AdrFileFilter 统一过滤逻辑（通过 YAML Front Matter 识别真正的 ADR）
    /// </summary>
    /// <param name="subfolder">子文件夹路径（相对于 ADR 根目录），为 null 则搜索整个 ADR 目录</param>
    /// <param name="excludeReadme">已废弃：README 文件由 AdrFileFilter 自动排除</param>
    /// <param name="excludeTimeline">是否排除 TIMELINE 文件，默认为 true</param>
    /// <param name="excludeChecklist">已废弃：CHECKLIST 文件由 AdrFileFilter 自动排除</param>
    /// <returns>ADR 文件路径列表</returns>
    [Obsolete("使用 FileSearchHelper.GetAdrFiles 代替", false)]
    public static IEnumerable<string> GetAdrFiles(
        string? subfolder = null,
        bool excludeReadme = true,
        bool excludeTimeline = true,
        bool excludeChecklist = true)
    {
        return FileSearchHelper.GetAdrFiles(subfolder, excludeReadme, excludeTimeline, excludeChecklist);
    }

    /// <summary>
    /// 获取指定目录下所有 Agent 配置文件
    /// </summary>
    /// <param name="includeSystemAgents">是否包含系统 Agent（如 expert-dotnet-software-engineer），默认为 false</param>
    /// <param name="excludeGuardian">是否排除 architecture-guardian，默认为 false</param>
    /// <returns>Agent 文件路径列表</returns>
    [Obsolete("使用 FileSearchHelper.GetAgentFiles 代替", false)]
    public static IEnumerable<string> GetAgentFiles(bool includeSystemAgents = false, bool excludeGuardian = false)
    {
        return FileSearchHelper.GetAgentFiles(includeSystemAgents, excludeGuardian);
    }

    /// <summary>
    /// 检查文件内容是否匹配正则表达式模式
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="pattern">正则表达式模式</param>
    /// <returns>如果匹配返回 true，否则返回 false</returns>
    [Obsolete("使用 FileContentAnalyzer.FileContentMatches 代替", false)]
    public static bool FileContentMatches(string filePath, string pattern)
    {
        return FileContentAnalyzer.FileContentMatches(filePath, pattern);
    }

    /// <summary>
    /// 获取文件中匹配正则表达式的所有行
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="pattern">正则表达式模式</param>
    /// <returns>匹配的行列表</returns>
    [Obsolete("使用 FileContentAnalyzer.GetMatchingLines 代替", false)]
    public static IEnumerable<string> GetMatchingLines(string filePath, string pattern)
    {
        return FileContentAnalyzer.GetMatchingLines(filePath, pattern);
    }

    /// <summary>
    /// 统计文件中特定模式出现的次数（不在代码块中）
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="pattern">正则表达式模式</param>
    /// <param name="excludeCodeBlocks">是否排除代码块中的匹配，默认为 true</param>
    /// <returns>匹配次数</returns>
    [Obsolete("使用 FileContentAnalyzer.CountPatternOccurrences 代替（已优化为流式读取）", false)]
    public static int CountPatternOccurrences(string filePath, string pattern, bool excludeCodeBlocks = true)
    {
        return FileContentAnalyzer.CountPatternOccurrences(filePath, pattern, excludeCodeBlocks);
    }

    /// <summary>
    /// 检查文件内容是否包含所有指定的关键词
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="keywords">关键词列表</param>
    /// <param name="ignoreCase">是否忽略大小写，默认为 false</param>
    /// <returns>如果所有关键词都存在返回 true，否则返回 false</returns>
    [Obsolete("使用 FileContentAnalyzer.FileContainsAllKeywords 代替", false)]
    public static bool FileContainsAllKeywords(string filePath, IEnumerable<string> keywords, bool ignoreCase = false)
    {
        return FileContentAnalyzer.FileContainsAllKeywords(filePath, keywords, ignoreCase);
    }

    /// <summary>
    /// 检查文件内容是否包含任一指定的关键词
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="keywords">关键词列表</param>
    /// <param name="ignoreCase">是否忽略大小写，默认为 false</param>
    /// <returns>如果任一关键词存在返回 true，否则返回 false</returns>
    [Obsolete("使用 FileContentAnalyzer.FileContainsAnyKeyword 代替", false)]
    public static bool FileContainsAnyKeyword(string filePath, IEnumerable<string> keywords, bool ignoreCase = false)
    {
        return FileContentAnalyzer.FileContainsAnyKeyword(filePath, keywords, ignoreCase);
    }

    /// <summary>
    /// 获取文件中缺失的关键词列表
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="requiredKeywords">必需的关键词列表</param>
    /// <param name="ignoreCase">是否忽略大小写，默认为 false</param>
    /// <returns>缺失的关键词列表</returns>
    [Obsolete("使用 FileContentAnalyzer.GetMissingKeywords 代替", false)]
    public static IEnumerable<string> GetMissingKeywords(string filePath, IEnumerable<string> requiredKeywords, bool ignoreCase = false)
    {
        return FileContentAnalyzer.GetMissingKeywords(filePath, requiredKeywords, ignoreCase);
    }

    /// <summary>
    /// 检查文件是否包含表格（Markdown 格式）
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="headerPattern">表格标题行的模式（可选）</param>
    /// <returns>如果包含表格返回 true，否则返回 false</returns>
    [Obsolete("使用 FileContentAnalyzer.FileContainsTable 代替", false)]
    public static bool FileContainsTable(string filePath, string? headerPattern = null)
    {
        return FileContentAnalyzer.FileContainsTable(filePath, headerPattern);
    }
}
