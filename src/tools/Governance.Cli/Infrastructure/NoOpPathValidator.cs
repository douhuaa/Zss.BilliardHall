namespace Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

/// <summary>
/// 无操作路径验证器，总是返回 true（用于测试）
/// </summary>
public sealed class NoOpPathValidator : IPathValidator
{
    public bool IsPathSafe(string path, out string errorMessage)
    {
        errorMessage = string.Empty;
        return true;
    }
}
