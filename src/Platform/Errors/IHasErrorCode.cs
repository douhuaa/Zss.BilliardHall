namespace Zss.BilliardHall.Platform.Errors;

/// <summary>
/// 标记接口：表示该异常携带稳定的业务错误码
/// </summary>
public interface IHasErrorCode
{
    /// <summary>稳定的错误码，作为唯一语义源</summary>
    string ErrorCode { get; }
}
