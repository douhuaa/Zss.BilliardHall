namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 文件系统测试辅助类
/// 提供统一的文件和目录操作方法，避免在测试中重复实现相同功能
/// </summary>
public static class FileSystemTestHelper
{
    /// <summary>
    /// 断言文件存在，如果不存在则抛出带有详细信息的异常
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="failureMessage">失败时的错误消息</param>
    public static void AssertFileExists(string filePath, string failureMessage)
    {
        File.Exists(filePath).Should().BeTrue(failureMessage);
    }

    /// <summary>
    /// 断言目录存在，如果不存在则抛出带有详细信息的异常
    /// </summary>
    /// <param name="directoryPath">目录路径（绝对路径）</param>
    /// <param name="failureMessage">失败时的错误消息</param>
    public static void AssertDirectoryExists(string directoryPath, string failureMessage)
    {
        Directory.Exists(directoryPath).Should().BeTrue(failureMessage);
    }

    /// <summary>
    /// 安全读取文件内容
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <returns>文件内容字符串</returns>
    /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
    public static string ReadFileContent(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"文件不存在: {filePath}", filePath);
        }

        return File.ReadAllText(filePath);
    }

    /// <summary>
    /// 获取目录中的文件列表
    /// </summary>
    /// <param name="directoryPath">目录路径（绝对路径）</param>
    /// <param name="searchPattern">搜索模式（如 "*.cs"、"*.md"），默认为 "*"</param>
    /// <param name="searchOption">搜索选项，默认为 TopDirectoryOnly</param>
    /// <returns>文件路径列表</returns>
    public static IReadOnlyList<string> GetFilesInDirectory(
        string directoryPath,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
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
    public static IReadOnlyList<string> GetSubdirectories(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return Array.Empty<string>();
        }

        return Directory.GetDirectories(directoryPath);
    }

    /// <summary>
    /// 断言文件内容包含指定文本
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="expectedContent">期望包含的内容</param>
    /// <param name="failureMessage">失败时的错误消息</param>
    public static void AssertFileContains(string filePath, string expectedContent, string failureMessage)
    {
        var content = ReadFileContent(filePath);
        content.Should().Contain(expectedContent, failureMessage);
    }

    /// <summary>
    /// 断言文件内容长度大于指定值
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="minLength">最小长度</param>
    /// <param name="failureMessage">失败时的错误消息</param>
    public static void AssertFileContentLength(string filePath, int minLength, string failureMessage)
    {
        var content = ReadFileContent(filePath);
        content.Length.Should().BeGreaterThan(minLength, failureMessage);
    }

    /// <summary>
    /// 获取相对于仓库根目录的相对路径
    /// </summary>
    /// <param name="fullPath">完整路径</param>
    /// <returns>相对路径</returns>
    public static string GetRelativePath(string fullPath)
    {
        return Path.GetRelativePath(TestEnvironment.RepositoryRoot, fullPath);
    }

    /// <summary>
    /// 组合仓库根目录和相对路径，返回绝对路径
    /// </summary>
    /// <param name="relativePath">相对于仓库根目录的路径</param>
    /// <returns>绝对路径</returns>
    public static string GetAbsolutePath(string relativePath)
    {
        return Path.Combine(TestEnvironment.RepositoryRoot, relativePath);
    }

    /// <summary>
    /// 获取指定目录下所有 ADR 文档文件
    /// </summary>
    /// <param name="subfolder">子文件夹路径（相对于 ADR 根目录），为 null 则搜索整个 ADR 目录</param>
    /// <param name="excludeReadme">是否排除 README.md 文件，默认为 true</param>
    /// <param name="excludeTimeline">是否排除 TIMELINE 文件，默认为 true</param>
    /// <param name="excludeChecklist">是否排除 CHECKLIST 文件，默认为 true</param>
    /// <returns>ADR 文件路径列表</returns>
    public static IEnumerable<string> GetAdrFiles(
        string? subfolder = null,
        bool excludeReadme = true,
        bool excludeTimeline = true,
        bool excludeChecklist = true)
    {
        var adrPath = subfolder != null
            ? GetAbsolutePath(Path.Combine(TestConstants.AdrDocsPath, subfolder))
            : GetAbsolutePath(TestConstants.AdrDocsPath);

        if (!Directory.Exists(adrPath))
        {
            return Enumerable.Empty<string>();
        }

        var files = Directory.GetFiles(adrPath, "*.md", SearchOption.AllDirectories);

        if (excludeReadme)
        {
            files = files.Where(f => !f.Contains("README", StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        if (excludeTimeline)
        {
            files = files.Where(f => !f.Contains("TIMELINE", StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        if (excludeChecklist)
        {
            files = files.Where(f => !f.Contains("CHECKLIST", StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        return files.Where(f => Path.GetFileName(f).StartsWith("ADR-", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取指定目录下所有 Agent 配置文件
    /// </summary>
    /// <param name="includeSystemAgents">是否包含系统 Agent（如 expert-dotnet-software-engineer），默认为 false</param>
    /// <param name="excludeGuardian">是否排除 architecture-guardian，默认为 false</param>
    /// <returns>Agent 文件路径列表</returns>
    public static IEnumerable<string> GetAgentFiles(bool includeSystemAgents = false, bool excludeGuardian = false)
    {
        var agentPath = GetAbsolutePath(TestConstants.AgentFilesPath);

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
    /// 检查文件内容是否匹配正则表达式模式
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="pattern">正则表达式模式</param>
    /// <returns>如果匹配返回 true，否则返回 false</returns>
    public static bool FileContentMatches(string filePath, string pattern)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        var content = File.ReadAllText(filePath);
        return Regex.IsMatch(content, pattern);
    }

    /// <summary>
    /// 获取文件中匹配正则表达式的所有行
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="pattern">正则表达式模式</param>
    /// <returns>匹配的行列表</returns>
    public static IEnumerable<string> GetMatchingLines(string filePath, string pattern)
    {
        if (!File.Exists(filePath))
        {
            return Enumerable.Empty<string>();
        }

        var content = File.ReadAllText(filePath);
        var lines = content.Split('\n');

        return lines.Where(line => Regex.IsMatch(line, pattern));
    }

    /// <summary>
    /// 统计文件中特定模式出现的次数（不在代码块中）
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="pattern">正则表达式模式</param>
    /// <param name="excludeCodeBlocks">是否排除代码块中的匹配，默认为 true</param>
    /// <returns>匹配次数</returns>
    public static int CountPatternOccurrences(string filePath, string pattern, bool excludeCodeBlocks = true)
    {
        if (!File.Exists(filePath))
        {
            return 0;
        }

        var content = File.ReadAllText(filePath);
        var lines = content.Split('\n');

        var count = 0;
        var inCodeBlock = false;

        foreach (var line in lines)
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
}
