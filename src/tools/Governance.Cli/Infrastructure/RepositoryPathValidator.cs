namespace Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

/// <summary>
/// 仓库路径验证器，确保路径在仓库根目录下
/// </summary>
public sealed class RepositoryPathValidator : IPathValidator
{
    public bool IsPathSafe(string directoryPath, out string errorMessage)
    {
        errorMessage = string.Empty;

        try
        {
            // 获取完整路径
            var fullPath = Path.GetFullPath(directoryPath);
            
            // 获取仓库根目录
            var currentDir = Directory.GetCurrentDirectory();
            var repoRoot = Path.GetFullPath(currentDir);

            // 检查路径是否在仓库根目录下
            if (!fullPath.StartsWith(repoRoot, StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = $"目标路径必须在仓库根目录下。当前路径: {fullPath}";
                return false;
            }

            // 检查是否包含可疑的路径遍历模式
            if (directoryPath.Contains("..") || directoryPath.Contains("~"))
            {
                errorMessage = "路径不能包含 '..' 或 '~' 字符";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"路径验证失败: {ex.Message}";
            return false;
        }
    }
}
