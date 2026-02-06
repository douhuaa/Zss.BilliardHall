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
            ? GetAbsolutePath(Path.Combine(TestConstants.AdrDocsPath, subfolder))
            : GetAbsolutePath(TestConstants.AdrDocsPath);

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

    /// <summary>
    /// 检查文件内容是否包含所有指定的关键词
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="keywords">关键词列表</param>
    /// <param name="ignoreCase">是否忽略大小写，默认为 false</param>
    /// <returns>如果所有关键词都存在返回 true，否则返回 false</returns>
    public static bool FileContainsAllKeywords(string filePath, IEnumerable<string> keywords, bool ignoreCase = false)
    {
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
    public static bool FileContainsAnyKeyword(string filePath, IEnumerable<string> keywords, bool ignoreCase = false)
    {
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
    public static IEnumerable<string> GetMissingKeywords(string filePath, IEnumerable<string> requiredKeywords, bool ignoreCase = false)
    {
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
    public static bool FileContainsTable(string filePath, string? headerPattern = null)
    {
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
}
