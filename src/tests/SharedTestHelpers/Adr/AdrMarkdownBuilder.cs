namespace Zss.BilliardHall.Tests.SharedTestHelpers.Adr;

/// <summary>
/// ADR Markdown 文档构建器
/// 用于在测试中创建符合规范的 ADR 文档内容，避免硬编码的测试数据
/// 
/// 重构说明（2026-02-09）：
/// - 统一方法命名为 With 前缀（提升 API 一致性）
/// - 添加 ADR 编号格式验证
/// </summary>
public sealed class AdrMarkdownBuilder
{
    private static readonly Regex AdrIdPattern = new(@"^ADR-\d{3,4}$", RegexOptions.Compiled);

    private string _id = "ADR-001";
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
    /// <param name="id">ADR 编号（格式：ADR-XXX 或 ADR-XXXX）</param>
    /// <exception cref="ArgumentException">当 ADR 编号格式不正确时抛出</exception>
    public AdrMarkdownBuilder WithId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("ADR 编号不能为空", nameof(id));
        }

        if (!AdrIdPattern.IsMatch(id))
        {
            throw new ArgumentException($"无效的 ADR 编号格式: {id}。期望格式：ADR-XXX 或 ADR-XXXX", nameof(id));
        }

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
    /// 添加依赖关系（统一方法名）
    /// </summary>
    /// <param name="adrIds">依赖的 ADR 编号列表</param>
    public AdrMarkdownBuilder WithDependsOn(params string[] adrIds)
    {
        _dependsOn.AddRange(adrIds);
        return this;
    }

    /// <summary>
    /// 添加依赖关系（向后兼容方法）
    /// </summary>
    /// <param name="adrIds">依赖的 ADR 编号列表</param>
    [Obsolete("使用 WithDependsOn 代替以保持命名一致性", false)]
    public AdrMarkdownBuilder DependsOn(params string[] adrIds)
    {
        return WithDependsOn(adrIds);
    }

    /// <summary>
    /// 添加被依赖关系（统一方法名）
    /// </summary>
    /// <param name="adrIds">被依赖的 ADR 编号列表</param>
    public AdrMarkdownBuilder WithDependedBy(params string[] adrIds)
    {
        _dependedBy.AddRange(adrIds);
        return this;
    }

    /// <summary>
    /// 添加被依赖关系（向后兼容方法）
    /// </summary>
    /// <param name="adrIds">被依赖的 ADR 编号列表</param>
    [Obsolete("使用 WithDependedBy 代替以保持命名一致性", false)]
    public AdrMarkdownBuilder DependedBy(params string[] adrIds)
    {
        return WithDependedBy(adrIds);
    }

    /// <summary>
    /// 添加替代关系（统一方法名）
    /// </summary>
    /// <param name="adrIds">替代的 ADR 编号列表</param>
    public AdrMarkdownBuilder WithSupersedes(params string[] adrIds)
    {
        _supersedes.AddRange(adrIds);
        return this;
    }

    /// <summary>
    /// 添加替代关系（向后兼容方法）
    /// </summary>
    /// <param name="adrIds">替代的 ADR 编号列表</param>
    [Obsolete("使用 WithSupersedes 代替以保持命名一致性", false)]
    public AdrMarkdownBuilder Supersedes(params string[] adrIds)
    {
        return WithSupersedes(adrIds);
    }

    /// <summary>
    /// 添加被替代关系（统一方法名）
    /// </summary>
    /// <param name="adrIds">被替代的 ADR 编号列表</param>
    public AdrMarkdownBuilder WithSupersededBy(params string[] adrIds)
    {
        _supersededBy.AddRange(adrIds);
        return this;
    }

    /// <summary>
    /// 添加被替代关系（向后兼容方法）
    /// </summary>
    /// <param name="adrIds">被替代的 ADR 编号列表</param>
    [Obsolete("使用 WithSupersededBy 代替以保持命名一致性", false)]
    public AdrMarkdownBuilder SupersededBy(params string[] adrIds)
    {
        return WithSupersededBy(adrIds);
    }

    /// <summary>
    /// 添加相关关系（统一方法名）
    /// </summary>
    /// <param name="adrIds">相关的 ADR 编号列表</param>
    public AdrMarkdownBuilder WithRelatedTo(params string[] adrIds)
    {
        _related.AddRange(adrIds);
        return this;
    }

    /// <summary>
    /// 添加相关关系（向后兼容方法）
    /// </summary>
    /// <param name="adrIds">相关的 ADR 编号列表</param>
    [Obsolete("使用 WithRelatedTo 代替以保持命名一致性", false)]
    public AdrMarkdownBuilder RelatedTo(params string[] adrIds)
    {
        return WithRelatedTo(adrIds);
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
