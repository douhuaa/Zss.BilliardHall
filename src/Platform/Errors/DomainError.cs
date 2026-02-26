namespace Zss.BilliardHall.Platform.Errors;

public class DomainError(string code) : Exception(code)
{
    public string Code { get; } = code;
}

