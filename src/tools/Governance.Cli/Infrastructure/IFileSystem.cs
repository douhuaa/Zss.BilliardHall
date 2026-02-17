namespace Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

/// <summary>
/// 文件系统操作抽象，支持测试和 dry-run 模式
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// 读取文件内容
    /// </summary>
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入文件内容
    /// </summary>
    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    bool FileExists(string path);

    /// <summary>
    /// 检查目录是否存在
    /// </summary>
    bool DirectoryExists(string path);

    /// <summary>
    /// 创建目录
    /// </summary>
    void CreateDirectory(string path);

    /// <summary>
    /// 获取目录下的所有文件
    /// </summary>
    string[] GetFiles(string path, string searchPattern, SearchOption searchOption);

    /// <summary>
    /// 获取目录下的所有子目录
    /// </summary>
    string[] GetDirectories(string path);
}
