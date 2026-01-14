using System.Collections.ObjectModel;

namespace Zss.BilliardHall.BuildingBlocks.Core;

/// <summary>
/// 错误码构建器，简化错误描述符的创建
/// Error code builder to simplify error descriptor creation
/// </summary>
public sealed class ErrorCodeBuilder
{
    private readonly string _module;
    private string? _code;
    private ErrorCategory? _category;
    private string? _defaultMessage;
    private Dictionary<string, object>? _context;

    private ErrorCodeBuilder(string module)
    {
        _module = module;
    }

    /// <summary>
    /// 创建指定模块的错误码构建器
    /// Create an error code builder for the specified module
    /// </summary>
    public static ErrorCodeBuilder ForModule(string module) => new(module);

    /// <summary>
    /// 设置错误码
    /// Set error code
    /// </summary>
    public ErrorCodeBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    /// <summary>
    /// 设置错误类别
    /// Set error category
    /// </summary>
    public ErrorCodeBuilder WithCategory(ErrorCategory category)
    {
        _category = category;
        return this;
    }

    /// <summary>
    /// 设置默认消息
    /// Set default message
    /// </summary>
    public ErrorCodeBuilder WithMessage(string message)
    {
        _defaultMessage = message;
        return this;
    }

    /// <summary>
    /// 添加上下文数据
    /// Add context data
    /// </summary>
    public ErrorCodeBuilder AddContext(string key, object value)
    {
        _context ??= new Dictionary<string, object>();
        _context[key] = value;
        return this;
    }

    /// <summary>
    /// 构建错误描述符
    /// Build error descriptor
    /// </summary>
    public ErrorDescriptor Build()
    {
        if (string.IsNullOrWhiteSpace(_code))
            throw new InvalidOperationException("错误码不能为空");

        if (!_category.HasValue)
            throw new InvalidOperationException("错误类别不能为空");

        if (string.IsNullOrWhiteSpace(_defaultMessage))
            throw new InvalidOperationException("默认消息不能为空");

        var context = _context != null
            ? new ReadOnlyDictionary<string, object>(_context)
            : null;

        return new ErrorDescriptor(
            _code,
            _category.Value,
            _module,
            _defaultMessage,
            context);
    }
}
