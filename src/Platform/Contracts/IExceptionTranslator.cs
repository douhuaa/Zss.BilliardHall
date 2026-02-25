namespace Zss.BilliardHall.Platform.Contracts;

/// <summary>
/// 异常翻译器契约：将技术层异常翻译为领域语义异常
/// </summary>
/// <remarks>
/// 用途：在 Web 层映射器之前，将 Marten/Npgsql 等基础设施异常
/// 翻译为 DomainException/InfrastructureException，
/// 确保 ExceptionProblemDetailsMapper 不依赖具体基础设施类型。
/// </remarks>
public interface IExceptionTranslator
{
    /// <summary>
    /// 尝试将异常翻译为语义更丰富的异常
    /// </summary>
    /// <param name="ex">原始异常</param>
    /// <returns>翻译后的异常；如果不适用则返回 null</returns>
    Exception? Translate(Exception ex);
}
