using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Zss.BilliardHall.Platform.Errors;

public sealed class PlatformErrorModule : IErrorModule
{
    public void Register()
    {
        ErrorRegistry.Register(new ErrorDescriptor(
            CommonErrorCodes.Unknown,
            "未知错误",
            StatusCodes.Status500InternalServerError,
            "https://api.zss.com/problems/common/unknown",
            LogLevel.Error));

        ErrorRegistry.Register(new ErrorDescriptor(
            CommonErrorCodes.Validation,
            "请求参数无效",
            StatusCodes.Status400BadRequest,
            "https://api.zss.com/problems/common/validation",
            LogLevel.Warning));

        ErrorRegistry.Register(new ErrorDescriptor(
            CommonErrorCodes.Unauthorized,
            "未授权访问",
            StatusCodes.Status401Unauthorized,
            "https://api.zss.com/problems/common/unauthorized",
            LogLevel.Information));

        ErrorRegistry.Register(new ErrorDescriptor(
            CommonErrorCodes.Forbidden,
            "权限不足",
            StatusCodes.Status403Forbidden,
            "https://api.zss.com/problems/common/forbidden",
            LogLevel.Warning));
    }
}

