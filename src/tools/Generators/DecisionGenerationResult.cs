namespace Zss.BilliardHall.Generators;

/// <summary>
/// Decision 生成结果
/// 封装生成的文本内容
/// </summary>
public sealed record DecisionGenerationResult
{
    /// <summary>
    /// 生成的 Markdown 格式文本
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// 创建生成结果
    /// </summary>
    public DecisionGenerationResult(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        Content = content;
    }

    /// <summary>
    /// 隐式转换为字符串（便于向后兼容）
    /// </summary>
    public static implicit operator string(DecisionGenerationResult result) => result.Content;

    /// <summary>
    /// 从字符串隐式转换
    /// </summary>
    public static implicit operator DecisionGenerationResult(string content) => new(content);
}
