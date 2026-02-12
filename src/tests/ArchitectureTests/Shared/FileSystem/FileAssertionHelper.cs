namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.FileSystem;

/// <summary>
/// 文件和目录断言辅助类
/// 提供统一的文件/目录存在性断言方法
/// 
/// 设计原则：
/// - 专注于文件和目录的断言操作
/// - 使用 AssertionMessageBuilder 生成标准化错误消息
/// - 支持内容断言（包含、长度等）
/// 
/// 重构说明：
/// 从 FileSystemTestHelper 中提取出断言相关功能
/// </summary>
public static class FileAssertionHelper
{
    /// <summary>
    /// 断言文件存在，如果不存在则抛出带有详细信息的异常
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="failureMessage">失败时的错误消息</param>
    /// <exception cref="ArgumentException">文件路径为空时抛出</exception>
    public static void AssertFileExists(string filePath, string failureMessage)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        File.Exists(filePath).Should().BeTrue(failureMessage);
    }

    /// <summary>
    /// 断言目录存在，如果不存在则抛出带有详细信息的异常
    /// </summary>
    /// <param name="directoryPath">目录路径（绝对路径）</param>
    /// <param name="failureMessage">失败时的错误消息</param>
    /// <exception cref="ArgumentException">目录路径为空时抛出</exception>
    public static void AssertDirectoryExists(string directoryPath, string failureMessage)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("目录路径不能为空", nameof(directoryPath));
        }

        Directory.Exists(directoryPath).Should().BeTrue(failureMessage);
    }

    /// <summary>
    /// 断言文件内容包含指定文本
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="expectedContent">期望包含的内容</param>
    /// <param name="failureMessage">失败时的错误消息</param>
    /// <exception cref="ArgumentException">文件路径或期望内容为空时抛出</exception>
    /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
    public static void AssertFileContains(string filePath, string expectedContent, string failureMessage)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(expectedContent))
        {
            throw new ArgumentException("期望内容不能为空", nameof(expectedContent));
        }

        var content = SafeReadFileContent(filePath);
        content.Should().Contain(expectedContent, failureMessage);
    }

    /// <summary>
    /// 断言文件内容长度大于指定值
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <param name="minLength">最小长度</param>
    /// <param name="failureMessage">失败时的错误消息</param>
    /// <exception cref="ArgumentException">文件路径为空或最小长度为负数时抛出</exception>
    /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
    public static void AssertFileContentLength(string filePath, int minLength, string failureMessage)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (minLength < 0)
        {
            throw new ArgumentException("最小长度不能为负数", nameof(minLength));
        }

        var content = SafeReadFileContent(filePath);
        content.Length.Should().BeGreaterThan(minLength, failureMessage);
    }

    /// <summary>
    /// 安全读取文件内容（内部辅助方法）
    /// </summary>
    /// <param name="filePath">文件路径（绝对路径）</param>
    /// <returns>文件内容字符串</returns>
    /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
    private static string SafeReadFileContent(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"文件不存在: {filePath}", filePath);
        }

        return File.ReadAllText(filePath);
    }
}
