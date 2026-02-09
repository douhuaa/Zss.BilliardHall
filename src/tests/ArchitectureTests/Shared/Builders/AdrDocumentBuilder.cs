namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Builders;

/// <summary>
/// ADR 文档构建器
/// 用于在测试中快速创建 ADR 文档对象
/// 注意：AdrDocument 使用 init-only 属性，所以只能在初始化时设置
/// </summary>
public class AdrDocumentBuilder
{
    private static int _idCounter = 100; // 使用计数器而非随机数，确保测试可重复性
    
    private string _id = $"ADR-{Interlocked.Increment(ref _idCounter):D3}";
    private string _filePath = "/test/adr/ADR-XXX.md";
    private string? _adrField;
    private string? _type = "adr";
    private string? _status = "已接受";
    private string? _level;
    private bool _isAdr = true;
    private bool _hasFrontMatter = true;
    private readonly HashSet<string> _dependsOn = new();
    private readonly HashSet<string> _dependedBy = new();
    private readonly HashSet<string> _supersedes = new();
    private readonly HashSet<string> _supersededBy = new();
    private readonly HashSet<string> _related = new();

    public AdrDocumentBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public AdrDocumentBuilder WithFilePath(string filePath)
    {
        _filePath = filePath;
        return this;
    }

    public AdrDocumentBuilder WithAdrField(string adrField)
    {
        _adrField = adrField;
        return this;
    }

    public AdrDocumentBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public AdrDocumentBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public AdrDocumentBuilder WithLevel(string level)
    {
        _level = level;
        return this;
    }

    public AdrDocumentBuilder AsAdr(bool isAdr = true)
    {
        _isAdr = isAdr;
        return this;
    }

    public AdrDocumentBuilder WithFrontMatter(bool hasFrontMatter = true)
    {
        _hasFrontMatter = hasFrontMatter;
        return this;
    }

    public AdrDocumentBuilder AddDependsOn(params string[] adrIds)
    {
        foreach (var id in adrIds)
            _dependsOn.Add(id);
        return this;
    }

    public AdrDocumentBuilder AddDependedBy(params string[] adrIds)
    {
        foreach (var id in adrIds)
            _dependedBy.Add(id);
        return this;
    }

    public AdrDocumentBuilder AddSupersedes(params string[] adrIds)
    {
        foreach (var id in adrIds)
            _supersedes.Add(id);
        return this;
    }

    public AdrDocumentBuilder AddSupersededBy(params string[] adrIds)
    {
        foreach (var id in adrIds)
            _supersededBy.Add(id);
        return this;
    }

    public AdrDocumentBuilder AddRelated(params string[] adrIds)
    {
        foreach (var id in adrIds)
            _related.Add(id);
        return this;
    }

    public AdrDocument Build()
    {
        var doc = new AdrDocument
        {
            Id = _id,
            FilePath = _filePath,
            AdrField = _adrField,
            Type = _type,
            Status = _status,
            Level = _level,
            IsAdr = _isAdr,
            HasFrontMatter = _hasFrontMatter
        };

        // 填充关系集合
        foreach (var id in _dependsOn)
            doc.DependsOn.Add(id);
        foreach (var id in _dependedBy)
            doc.DependedBy.Add(id);
        foreach (var id in _supersedes)
            doc.Supersedes.Add(id);
        foreach (var id in _supersededBy)
            doc.SupersededBy.Add(id);
        foreach (var id in _related)
            doc.Related.Add(id);

        return doc;
    }
}
