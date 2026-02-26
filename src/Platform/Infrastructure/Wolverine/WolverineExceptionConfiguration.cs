using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;

namespace Zss.BilliardHall.Platform.Infrastructure;

/// <summary>
/// Wolverine pipeline 层异常处理扩展方法
/// </summary>
public static class WolverineExceptionConfiguration
{
    /// <summary>
    /// 注册 Wolverine pipeline 层的异常语义化处理，包含：
    /// - 将 FluentValidation 失败转换为 <see cref="Exceptions.ValidationException"/>（PlatformValidationFailureAction）
    /// - 若 <see cref="ExceptionTransformMiddleware"/> 未能转换 PostgresException，
    ///   Wolverine 记录错误日志后丢弃消息，不重试也不入死信队列
    /// - 注册 <see cref="ExceptionTransformMiddleware"/> Wolverine 中间件，
    ///   将 PostgresException 转换为 DomainException（由各模块注册 IPostgresExceptionTransformer 扩展）
    /// </summary>
    public static WolverineOptions ConfigureExceptionTransforms(this WolverineOptions options)
    {
        options.Services.AddSingleton(typeof(IFailureAction<>), typeof(PlatformValidationFailureAction<>));

        // Discard：若 ExceptionTransformMiddleware 未处理 PostgresException，
        // Wolverine 记录错误日志后丢弃消息，不重试也不入死信队列。
        options.OnException<PostgresException>().Discard();

        options.Policies.AddMiddleware<ExceptionTransformMiddleware>();

        return options;
    }
}
