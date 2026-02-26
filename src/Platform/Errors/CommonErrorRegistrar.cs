using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Zss.BilliardHall.Platform.Errors;

/// <summary>
/// 平台公共错误码注册器，注册 COMMON_* 描述符
/// </summary>
public sealed class CommonErrorRegistrar : IErrorRegistrar
{
    public void Register(IErrorRegistry registry)
    {
        registry.Register(new ErrorDescriptor(
            CommonErrorCodes.ValidationFailed,
            StatusCodes.Status400BadRequest,
            "验证失败",
            LogLevel.Warning));

        registry.Register(new ErrorDescriptor(
            CommonErrorCodes.UnknownError,
            StatusCodes.Status500InternalServerError,
            "服务器内部错误",
            LogLevel.Error));
    }
}
