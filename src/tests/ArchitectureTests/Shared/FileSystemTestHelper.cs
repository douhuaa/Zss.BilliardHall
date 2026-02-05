using FluentAssertions;

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
}
