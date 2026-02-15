using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Tests.Infrastructure;

public sealed class InMemoryFileSystem : IFileSystem
{
    private readonly Dictionary<string, string> _files = new();
    private readonly HashSet<string> _directories = new();

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!_files.ContainsKey(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        return Task.FromResult(_files[path]);
    }

    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);

        _files[path] = content;
        return Task.CompletedTask;
    }

    public bool FileExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return _files.ContainsKey(path);
    }

    public bool DirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return _directories.Contains(path) || path == "/";
    }

    public void CreateDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _directories.Add(path);
    }

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        // 简单实现：返回所有匹配的文件
        return _files.Keys
            .Where(f => f.StartsWith(path))
            .ToArray();
    }

    // 测试辅助方法
    public void AddFile(string path, string content)
    {
        _files[path] = content;
    }

    public string GetFileContent(string path)
    {
        return _files.TryGetValue(path, out var content) ? content : string.Empty;
    }

    public IReadOnlyDictionary<string, string> GetAllFiles() => _files;
}
