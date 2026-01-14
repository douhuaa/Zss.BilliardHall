using Microsoft.AspNetCore.Http;

namespace Zss.BilliardHall.BuildingBlocks.Exceptions;

/// <summary>
/// 资源未找到异常
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string resourceName, object key)
        : base(new ErrorCode("Common:NotFound", StatusCodes.Status404NotFound, $"{resourceName} with key '{key}' was not found."))
    {
    }

    public NotFoundException(string message)
        : base(new ErrorCode("Common:NotFound", StatusCodes.Status404NotFound, message))
    {
    }
}
