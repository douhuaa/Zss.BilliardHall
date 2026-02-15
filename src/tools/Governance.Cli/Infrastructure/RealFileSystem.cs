namespace Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

/// <summary>
/// 真实文件系统实现
/// </summary>
public sealed class RealFileSystem : IFileSystem
{
    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.ReadAllTextAsync(path, cancellationToken);
    }

    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        return File.WriteAllTextAsync(path, content, cancellationToken);
    }

    public bool FileExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.Exists(path);
    }

    public bool DirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Directory.Exists(path);
    }

    public void CreateDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Directory.CreateDirectory(path);
    }

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        return Directory.GetFiles(path, searchPattern, searchOption);
    }
}
