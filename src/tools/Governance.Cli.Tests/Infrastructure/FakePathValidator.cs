using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Tests.Infrastructure;

/// <summary>
/// 可配置的路径验证器，用于测试特定的安全场景
/// </summary>
public sealed class FakePathValidator : IPathValidator
{
    private readonly Func<string, (bool isValid, string errorMessage)> _validationFunc;

    public FakePathValidator(Func<string, (bool, string)> validationFunc)
    {
        _validationFunc = validationFunc ?? throw new ArgumentNullException(nameof(validationFunc));
    }

    /// <summary>
    /// 创建一个拒绝特定路径的验证器
    /// </summary>
    public static FakePathValidator RejectPath(string pathToReject, string errorMessage = "路径不安全")
    {
        return new FakePathValidator(path => 
            path == pathToReject 
                ? (false, errorMessage) 
                : (true, string.Empty));
    }

    /// <summary>
    /// 创建一个仅接受特定路径的验证器
    /// </summary>
    public static FakePathValidator AcceptOnlyPath(string pathToAccept, string errorMessage = "路径不在允许列表中")
    {
        return new FakePathValidator(path =>
            path == pathToAccept
                ? (true, string.Empty)
                : (false, errorMessage));
    }

    public bool IsPathSafe(string path, out string errorMessage)
    {
        var (isValid, error) = _validationFunc(path);
        errorMessage = error;
        return isValid;
    }
}
