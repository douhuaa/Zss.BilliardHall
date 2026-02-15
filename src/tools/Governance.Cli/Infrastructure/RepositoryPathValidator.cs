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
            
            // 获取仓库根目录（规范化为带目录分隔符的前缀）
            var currentDir = Directory.GetCurrentDirectory();
            var repoRoot = Path.GetFullPath(currentDir);
            var repoRootWithSeparator = repoRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) 
                                        + Path.DirectorySeparatorChar;

            // 检查路径是否在仓库根目录下或就是仓库根目录
            // 使用带分隔符的前缀匹配避免 /repo 和 /repo2 的碰撞
            var isInRepo = fullPath.Equals(repoRoot, StringComparison.OrdinalIgnoreCase) ||
                          fullPath.StartsWith(repoRootWithSeparator, StringComparison.OrdinalIgnoreCase);

            if (!isInRepo)
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
