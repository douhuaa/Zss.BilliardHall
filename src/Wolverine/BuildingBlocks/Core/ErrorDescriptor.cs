namespace Zss.BilliardHall.BuildingBlocks.Core;

/// <summary>
/// 错误描述符，支持本地化和结构化错误信息
/// Error descriptor supporting localization and structured error information
/// </summary>
public sealed record ErrorDescriptor
{
    /// <summary>
    /// 错误码：格式 {Module}:{Category}.{Specific}
    /// 例如: Members:Validation.InvalidTopUpAmount, Members:Business.InsufficientBalance
    /// Error code: format {Module}:{Category}.{Specific}
    /// Example: Members:Validation.InvalidTopUpAmount, Members:Business.InsufficientBalance
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 错误类别（NotFound, Validation, Business, Conflict, Forbidden）
    /// Error category
    /// </summary>
    public ErrorCategory Category { get; }

    /// <summary>
    /// 所属模块/领域上下文（Members, Tables, Sessions）
    /// Module/Bounded Context
    /// </summary>
    public string Module { get; }

    /// <summary>
    /// 默认错误消息（中文）
    /// Default error message (Chinese)
    /// </summary>
    public string DefaultMessage { get; }

    /// <summary>
    /// 附加上下文数据（用于消息模板参数）
    /// Additional context data (for message template parameters)
    /// </summary>
    public IReadOnlyDictionary<string, object>? Context { get; init; }

    public ErrorDescriptor(
        string code,
        ErrorCategory category,
        string module,
        string defaultMessage,
        IReadOnlyDictionary<string, object>? context = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("错误码不能为空", nameof(code));

        if (string.IsNullOrWhiteSpace(module))
            throw new ArgumentException("模块名不能为空", nameof(module));

        if (string.IsNullOrWhiteSpace(defaultMessage))
            throw new ArgumentException("默认消息不能为空", nameof(defaultMessage));

        // 验证错误码格式：{Module}:{Category}.{Specific}
        if (!code.StartsWith($"{module}:"))
            throw new ArgumentException(
                $"错误码必须以模块名开头: {module}:，实际: {code}",
                nameof(code));

        Code = code;
        Category = category;
        Module = module;
        DefaultMessage = defaultMessage;
        Context = context;
    }

    /// <summary>
    /// 格式化消息（替换模板参数）
    /// Format message (replace template parameters)
    /// </summary>
    public string FormatMessage()
    {
        if (Context == null || Context.Count == 0)
            return DefaultMessage;

        var result = DefaultMessage;
        
        // 优化：避免在循环中多次调用 String.Replace
        if (Context.Count <= 3)
        {
            // 少量参数：直接替换
            foreach (var (key, value) in Context)
            {
                result = result.Replace($"{{{key}}}", value?.ToString() ?? string.Empty);
            }
        }
        else
        {
            // 多参数：使用 StringBuilder 优化性能
            var sb = new System.Text.StringBuilder(result);
            foreach (var (key, value) in Context)
            {
                sb.Replace($"{{{key}}}", value?.ToString() ?? string.Empty);
            }
            result = sb.ToString();
        }
        
        return result;
    }
}

/// <summary>
/// 错误类别枚举
/// Error category enumeration
/// </summary>
public enum ErrorCategory
{
    /// <summary>资源未找到</summary>
    NotFound,

    /// <summary>输入验证失败</summary>
    Validation,

    /// <summary>业务规则违反</summary>
    Business,

    /// <summary>资源冲突</summary>
    Conflict,

    /// <summary>权限不足</summary>
    Forbidden,

    /// <summary>无效状态</summary>
    InvalidStatus
}
