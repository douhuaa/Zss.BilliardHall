namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// ADR Markdown 文档构建器
/// 用于在测试中创建符合规范的 ADR 文档内容，避免硬编码的测试数据
/// </summary>
public sealed class AdrMarkdownBuilder
{
    private string _id = "ADR-0001";
    private string _title = "测试 ADR 文档";
    private string _status = "Final";
    private string _level = "架构约束";
    private readonly List<string> _dependsOn = new();
    private readonly List<string> _dependedBy = new();
    private readonly List<string> _supersedes = new();
    private readonly List<string> _supersededBy = new();
    private readonly List<string> _related = new();
    private string _decision = "这是决策内容。";
    private string? _context;
    private string? _consequences;

    /// <summary>
    /// 设置 ADR 编号
    /// </summary>
    public AdrMarkdownBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// 设置 ADR 标题
    /// </summary>
    public AdrMarkdownBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// 设置状态
    /// </summary>
    public AdrMarkdownBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// 设置级别
    /// </summary>
    public AdrMarkdownBuilder WithLevel(string level)
    {
        _level = level;
        return this;
    }

    /// <summary>
    /// 添加依赖关系
    /// </summary>
    public AdrMarkdownBuilder DependsOn(params string[] adrIds)
    {
        _dependsOn.AddRange(adrIds);
        return this;
    }

    /// <summary>
    /// 添加被依赖关系
    /// </summary>
    public AdrMarkdownBuilder DependedBy(params string[] adrIds)
    {
        _dependedBy.AddRange(adrIds);
        return this;
    }

    /// <summary>
    /// 添加替代关系
    /// </summary>
    public AdrMarkdownBuilder Supersedes(params string[] adrIds)
    {
        _supersedes.AddRange(adrIds);
        return this;
    }

    /// <summary>
    /// 添加被替代关系
    /// </summary>
    public AdrMarkdownBuilder SupersededBy(params string[] adrIds)
    {
        _supersededBy.AddRange(adrIds);
        return this;
    }

    /// <summary>
    /// 添加相关关系
    /// </summary>
    public AdrMarkdownBuilder RelatedTo(params string[] adrIds)
    {
        _related.AddRange(adrIds);
        return this;
    }

    /// <summary>
    /// 设置决策内容
    /// </summary>
    public AdrMarkdownBuilder WithDecision(string decision)
    {
        _decision = decision;
        return this;
    }

    /// <summary>
    /// 设置背景内容
    /// </summary>
    public AdrMarkdownBuilder WithContext(string context)
    {
        _context = context;
        return this;
    }

    /// <summary>
    /// 设置后果内容
    /// </summary>
    public AdrMarkdownBuilder WithConsequences(string consequences)
    {
        _consequences = consequences;
        return this;
    }

    /// <summary>
    /// 构建 ADR Markdown 文档
    /// </summary>
    public string Build()
    {
        var builder = new System.Text.StringBuilder();

        // 标题
        builder.AppendLine($"# {_id}：{_title}");
        builder.AppendLine();

        // 状态和级别
        builder.AppendLine($"**状态**：{_status}");
        builder.AppendLine($"**级别**：{_level}");
        builder.AppendLine();

        // 关系声明
        builder.AppendLine("## 关系声明（Relationships）");
        builder.AppendLine();

        builder.AppendLine($"**依赖（Depends On）**：{FormatList(_dependsOn)}");
        builder.AppendLine($"**被依赖（Depended By）**：{FormatList(_dependedBy)}");
        builder.AppendLine($"**替代（Supersedes）**：{FormatList(_supersedes)}");
        builder.AppendLine($"**被替代（Superseded By）**：{FormatList(_supersededBy)}");
        builder.AppendLine($"**相关（Related）**：{FormatList(_related)}");
        builder.AppendLine();

        // 背景（可选）
        if (!string.IsNullOrEmpty(_context))
        {
            builder.AppendLine("## 背景（Context）");
            builder.AppendLine();
            builder.AppendLine(_context);
            builder.AppendLine();
        }

        // 决策
        builder.AppendLine("## 决策（Decision）");
        builder.AppendLine();
        builder.AppendLine(_decision);
        builder.AppendLine();

        // 后果（可选）
        if (!string.IsNullOrEmpty(_consequences))
        {
            builder.AppendLine("## 后果（Consequences）");
            builder.AppendLine();
            builder.AppendLine(_consequences);
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string FormatList(List<string> items)
    {
        return items.Count == 0 ? "无" : string.Join(", ", items);
    }

    /// <summary>
    /// 创建一个默认的 ADR 构建器
    /// </summary>
    public static AdrMarkdownBuilder CreateDefault()
    {
        return new AdrMarkdownBuilder();
    }

    /// <summary>
    /// 创建一个指定 ID 和标题的 ADR 构建器
    /// </summary>
    public static AdrMarkdownBuilder Create(string id, string title)
    {
        return new AdrMarkdownBuilder()
            .WithId(id)
            .WithTitle(title);
    }
}
