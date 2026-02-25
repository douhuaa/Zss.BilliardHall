using System.Runtime.ExceptionServices;
using Zss.BilliardHall.Platform.Contracts;

namespace Zss.BilliardHall.Application.Infrastructure;

/// <summary>
/// Wolverine 异常翻译中间件
/// 在 Wolverine handler pipeline 中将技术异常（如 PostgresException）翻译为领域异常，
/// 确保翻译在事务提交后、异常冒泡到 Web/Worker 层之前完成。
/// Web/Worker 层只需处理结构化的 DomainException。
/// </summary>
public sealed class ExceptionTranslationMiddleware(IEnumerable<IExceptionTranslator> translators)
{
    public async Task BeforeAsync(Func<Task> next)
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            foreach (var translator in translators)
            {
                var translated = translator.Translate(ex);
                if (translated is not null)
                    ExceptionDispatchInfo.Capture(translated).Throw();
            }

            throw;
        }
    }
}
