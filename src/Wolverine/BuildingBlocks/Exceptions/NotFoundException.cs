namespace Zss.BilliardHall.BuildingBlocks.Exceptions;

/// <summary>
/// 资源未找到异常
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string resourceName, object key)
        : base("NotFound", $"{resourceName} with key '{key}' was not found.")
    {
    }

    public NotFoundException(string message)
        : base("NotFound", message)
    {
    }
}
