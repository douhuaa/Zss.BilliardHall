namespace Zss.BilliardHall.BuildingBlocks.Core;

public sealed class DomainResult
{
    public bool IsSuccess { get; }
    public ErrorCode? Error { get; }

    private DomainResult(bool isSuccess, ErrorCode? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static DomainResult Success() => new(true, null);

    public static DomainResult Fail(ErrorCode error)
        => new(false, error);

    public void EnsureSuccess()
    {
        if (!IsSuccess)
            throw new InvalidOperationException(
                $"Domain rule violated: {Error}");
    }

}
