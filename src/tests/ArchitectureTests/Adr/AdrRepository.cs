using System.Text.RegularExpressions;

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
