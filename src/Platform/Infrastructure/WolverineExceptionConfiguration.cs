using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;
using FluentValidation.Results;
using PlatformValidationException = Zss.BilliardHall.Platform.Exceptions.ValidationException;

namespace Zss.BilliardHall.Platform.Infrastructure;

/// <summary>
/// Wolverine 异常转换配置扩展
/// 职责：在 Wolverine pipeline 层面将技术异常转换为语义异常
/// 
/// 设计原则：
/// - 技术异常（FluentValidation.ValidationException）不出基础设施层
/// - 所有异常在到达 Web 层之前已经是语义异常
/// - Web 层只做 HTTP 映射（ProblemDetails），不感知具体技术栈
/// </summary>
public static class WolverineExceptionConfiguration
{
    /// <summary>
    /// 配置 Wolverine 异常转换策略
    /// 将技术层异常转换为平台语义异常
    /// </summary>
    /// <param name="options">Wolverine 配置选项</param>
    public static void ConfigureExceptionTransforms(this WolverineOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // 替换默认的 IFailureAction<T> 实现，使其抛出 PlatformValidationException 而非 FluentValidation.ValidationException
        options.Services.AddSingleton(typeof(IFailureAction<>), typeof(PlatformValidationFailureAction<>));

        // PostgresException 唯一约束冲突：丢弃消息（不重试），异常继续冒泡到 Web 层
        // 注意：具体的异常转换由各模块通过 IPostgresExceptionTransformer 注册
        options.OnException<PostgresException>()
            .Discard();
    }
}

/// <summary>
/// 自定义 FluentValidation 失败处理器
/// 职责：将 FluentValidation 验证失败转换为 Platform.Exceptions.ValidationException
/// 
/// 替代 Wolverine.FluentValidation.Internals.FailureAction 的默认行为，
/// 确保验证失败抛出的是语义异常而非技术异常
/// </summary>
/// <typeparam name="T">被验证的消息类型</typeparam>
public sealed class PlatformValidationFailureAction<T> : IFailureAction<T>
{
    public void Throw(T message, IReadOnlyList<ValidationFailure> failures)
    {
        var errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

        throw new PlatformValidationException("验证失败，请检查输入数据。", errors);
    }
}
