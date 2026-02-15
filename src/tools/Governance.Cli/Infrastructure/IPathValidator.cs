namespace Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

/// <summary>
/// 路径验证器接口，用于验证文件路径的安全性
/// </summary>
public interface IPathValidator
{
    /// <summary>
    /// 验证文件路径是否安全
    /// </summary>
    /// <param name="path">要验证的路径</param>
    /// <param name="errorMessage">如果验证失败，返回错误消息</param>
    /// <returns>true 如果路径安全</returns>
    bool IsPathSafe(string path, out string errorMessage);
}
