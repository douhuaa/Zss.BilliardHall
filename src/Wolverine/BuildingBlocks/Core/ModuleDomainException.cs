namespace Zss.BilliardHall.BuildingBlocks.Core;

/// <summary>
/// 模块领域异常基类，表达 Bounded Context 和业务语义
/// Module domain exception base class, expressing Bounded Context and business semantics
/// </summary>
public abstract class ModuleDomainException : Exception
{
    /// <summary>
    /// 错误描述符
    /// Error descriptor
    /// </summary>
    public ErrorDescriptor ErrorDescriptor { get; }

    /// <summary>
    /// 错误码
    /// Error code
    /// </summary>
    public string Code => ErrorDescriptor.Code;

    /// <summary>
    /// 错误类别
    /// Error category
    /// </summary>
    public ErrorCategory Category => ErrorDescriptor.Category;

    /// <summary>
    /// 所属模块
    /// Module
    /// </summary>
    public string Module => ErrorDescriptor.Module;

    protected ModuleDomainException(ErrorDescriptor errorDescriptor)
        : base(errorDescriptor.FormatMessage())
    {
        ErrorDescriptor = errorDescriptor;
    }

    protected ModuleDomainException(ErrorDescriptor errorDescriptor, Exception innerException)
        : base(errorDescriptor.FormatMessage(), innerException)
    {
        ErrorDescriptor = errorDescriptor;
    }
}
