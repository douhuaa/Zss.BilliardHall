namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR 仓库 - 负责扫描和加载所有 ADR 文档
/// </summary>
public sealed class AdrRepository
{
    private static readonly Regex AdrFilePattern = new(@"ADR-\d{3,4}", RegexOptions.Compiled);
    
    private readonly string _adrRoot;

    public AdrRepository(string adrRoot)
    {
        _adrRoot = adrRoot ?? throw new ArgumentNullException(nameof(adrRoot));
    }

    /// <summary>
    /// 加载所有 ADR 文档
    /// 排除：
    /// - README.md
    /// - proposals 目录
    /// - 无效的 ADR 编号格式
    /// - 通过 Front Matter 标记为非 ADR 的文档（如 checklist, guide）
    /// </summary>
    public IReadOnlyList<AdrDocument> LoadAll()
    {
        if (!Directory.Exists(_adrRoot))
        {
            throw new DirectoryNotFoundException($"ADR 目录不存在: {_adrRoot}");
        }

        return Directory
            .EnumerateFiles(_adrRoot, "ADR-*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("/proposals/", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("\\proposals\\", StringComparison.OrdinalIgnoreCase))
            .Select(ParseFile)
            .Where(adr => adr is not null && adr.IsAdr)  // 使用 IsAdr 属性过滤
            .Cast<AdrDocument>()
            .ToList();
    }

    /// <summary>
    /// 加载所有匹配 ADR-*.md 模式的文档（包括非 ADR 文档）
    /// 用于需要检查所有文档的场景
    /// </summary>
    public IReadOnlyList<AdrDocument> LoadAllFiles()
    {
        if (!Directory.Exists(_adrRoot))
        {
            throw new DirectoryNotFoundException($"ADR 目录不存在: {_adrRoot}");
        }

        return Directory
            .EnumerateFiles(_adrRoot, "ADR-*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("/proposals/", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("\\proposals\\", StringComparison.OrdinalIgnoreCase))
            .Select(ParseFile)
            .Where(adr => adr is not null)
            .Cast<AdrDocument>()
            .ToList();
    }

    /// <summary>
    /// 解析单个 ADR 文件
    /// </summary>
    private static AdrDocument? ParseFile(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var match = AdrFilePattern.Match(fileName);

        if (!match.Success)
            return null;

        var adrId = match.Value;
        return AdrParser.Parse(adrId, filePath);
    }
}
