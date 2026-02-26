using FluentValidation.Results;
using Wolverine.FluentValidation;
using Zss.BilliardHall.Platform.Errors;
using PlatformValidationException = Zss.BilliardHall.Platform.Exceptions.ValidationException;

namespace Zss.BilliardHall.Platform.Infrastructure;

/// <summary>
/// 自定义 FluentValidation 失败处理器，替代 Wolverine.FluentValidation 默认实现。
/// 将验证失败直接转换为 <see cref="PlatformValidationException"/>，
/// 使异常语义化在 Wolverine pipeline 层完成，Web 层只需映射 ProblemDetails。
/// </summary>
/// <typeparam name="T">被验证的消息类型</typeparam>
public sealed class PlatformValidationFailureAction<T> : IFailureAction<T>
{
    public void Throw(T message, IReadOnlyList<ValidationFailure> failures)
    {
        var errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

        throw new PlatformValidationException(PlatformErrorMessages.ValidationFailure, errors);
    }
}
