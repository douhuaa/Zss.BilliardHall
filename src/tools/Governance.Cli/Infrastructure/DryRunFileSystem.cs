namespace Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

/// <summary>
/// Dry-run 模式的文件系统实现，只输出到控制台，不写入文件
/// </summary>
public sealed class DryRunFileSystem : IFileSystem
{
    private readonly IFileSystem _innerFileSystem;

    public DryRunFileSystem(IFileSystem innerFileSystem)
    {
        _innerFileSystem = innerFileSystem ?? throw new ArgumentNullException(nameof(innerFileSystem));
    }

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[DRY-RUN] 读取文件: {path}");
        return _innerFileSystem.ReadAllTextAsync(path, cancellationToken);
    }

    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[DRY-RUN] 将写入文件: {path}");
        Console.WriteLine("--- 内容预览 (前100行) ---");
        var lines = content.Split('\n').Take(100);
        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }
        Console.WriteLine("--- 内容预览结束 ---");
        return Task.CompletedTask;
    }

    public bool FileExists(string path)
    {
        return _innerFileSystem.FileExists(path);
    }

    public bool DirectoryExists(string path)
    {
        return _innerFileSystem.DirectoryExists(path);
    }

    public void CreateDirectory(string path)
    {
        Console.WriteLine($"[DRY-RUN] 将创建目录: {path}");
    }

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return _innerFileSystem.GetFiles(path, searchPattern, searchOption);
    }

    public string[] GetDirectories(string path)
    {
        return _innerFileSystem.GetDirectories(path);
    }
}
