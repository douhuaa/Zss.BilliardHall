namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.FileSystem;

/// <summary>
/// 文件搜索辅助类
/// 提供文件和目录的搜索功能
/// 
/// 设计原则：
/// - 专注于文件系统的搜索和枚举操作
/// - 提供 ADR 和 Agent 文件的专用搜索方法
/// - 支持灵活的过滤和路径转换
/// 
/// 重构说明：
/// 从 FileSystemTestHelper 中提取出搜索相关功能
/// </summary>
public static class FileSearchHelper
{
    /// <summary>
    /// 获取目录中的文件列表
    /// </summary>
    /// <param name="directoryPath">目录路径（绝对路径）</param>
    /// <param name="searchPattern">搜索模式（如 "*.cs"、"*.md"），默认为 "*"</param>
    /// <param name="searchOption">搜索选项，默认为 TopDirectoryOnly</param>
    /// <returns>文件路径列表</returns>
    /// <exception cref="ArgumentException">目录路径为空时抛出</exception>
    public static IReadOnlyList<string> GetFilesInDirectory(
        string directoryPath,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("目录路径不能为空", nameof(directoryPath));
        }

        if (!Directory.Exists(directoryPath))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(directoryPath, searchPattern, searchOption);
    }

    /// <summary>
    /// 获取目录的子目录列表
    /// </summary>
    /// <param name="directoryPath">目录路径（绝对路径）</param>
    /// <returns>子目录路径列表</returns>
    /// <exception cref="ArgumentException">目录路径为空时抛出</exception>
    public static IReadOnlyList<string> GetSubdirectories(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("目录路径不能为空", nameof(directoryPath));
        }

        if (!Directory.Exists(directoryPath))
        {
            return Array.Empty<string>();
        }

        return Directory.GetDirectories(directoryPath);
    }

    /// <summary>
    /// 获取相对于仓库根目录的相对路径
    /// </summary>
    /// <param name="fullPath">完整路径</param>
    /// <returns>相对路径</returns>
    /// <exception cref="ArgumentException">路径为空时抛出</exception>
    public static string GetRelativePath(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            throw new ArgumentException("路径不能为空", nameof(fullPath));
        }

        return Path.GetRelativePath(TestEnvironment.RepositoryRoot, fullPath);
    }

    /// <summary>
    /// 组合仓库根目录和相对路径，返回绝对路径
    /// </summary>
    /// <param name="relativePath">相对于仓库根目录的路径</param>
    /// <returns>绝对路径</returns>
    /// <exception cref="ArgumentException">路径为空时抛出</exception>
    public static string GetAbsolutePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("相对路径不能为空", nameof(relativePath));
        }

        return Path.Combine(TestEnvironment.RepositoryRoot, relativePath);
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
    public static IEnumerable<string> GetAdrFiles(
        string? subfolder = null,
        bool excludeReadme = true,
        bool excludeTimeline = true,
        bool excludeChecklist = true)
    {
        var adrPath = subfolder != null
            ? GetAbsolutePath(Path.Combine(ArchitectureTestSpecification.Adr.Paths.Root, subfolder))
            : GetAbsolutePath(ArchitectureTestSpecification.Adr.Paths.Root);

        if (!Directory.Exists(adrPath))
        {
            return Enumerable.Empty<string>();
        }

        // 使用 AdrFileFilter 统一过滤 ADR 文件
        // AdrFileFilter 已处理：README、TEMPLATE、CHECKLIST、proposals 目录等
        var files = AdrFileFilter.GetAdrFiles(adrPath);

        // 额外的过滤选项（TIMELINE 不在 AdrFileFilter 中处理）
        if (excludeTimeline)
        {
            files = files.Where(f => !f.Contains("TIMELINE", StringComparison.OrdinalIgnoreCase));
        }

        return files;
    }

    /// <summary>
    /// 获取指定目录下所有 Agent 配置文件
    /// </summary>
    /// <param name="includeSystemAgents">是否包含系统 Agent（如 expert-dotnet-software-engineer），默认为 false</param>
    /// <param name="excludeGuardian">是否排除 architecture-guardian，默认为 false</param>
    /// <returns>Agent 文件路径列表</returns>
    public static IEnumerable<string> GetAgentFiles(bool includeSystemAgents = false, bool excludeGuardian = false)
    {
        var agentPath = GetAbsolutePath(ArchitectureTestSpecification.Adr.Paths.AgentFiles);

        if (!Directory.Exists(agentPath))
        {
            return Enumerable.Empty<string>();
        }

        var files = Directory.GetFiles(agentPath, "*.agent.md", SearchOption.AllDirectories);

        if (!includeSystemAgents)
        {
            var systemAgents = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "expert-dotnet-software-engineer.agent.md",
                "README.md"
            };
            files = files.Where(f => !systemAgents.Contains(Path.GetFileName(f))).ToArray();
        }

        if (excludeGuardian)
        {
            files = files.Where(f => !Path.GetFileName(f).Equals("architecture-guardian.agent.md", StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        return files;
    }

    /// <summary>
    /// 安全读取文件内容
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <returns>文件内容字符串</returns>
    /// <exception cref="ArgumentException">文件路径为空时抛出</exception>
    /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
    public static string ReadFileContent(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"文件不存在: {filePath}", filePath);
        }

        return File.ReadAllText(filePath);
    }
}
