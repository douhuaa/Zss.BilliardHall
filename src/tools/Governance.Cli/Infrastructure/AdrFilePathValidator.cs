namespace Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

/// <summary>
/// ADR 文件路径验证器，确保文件只能写入 docs/adr 目录
/// </summary>
public sealed class AdrFilePathValidator : IPathValidator
{
    public bool IsPathSafe(string filePath, out string errorMessage)
    {
        errorMessage = string.Empty;

        try
        {
            // 获取完整路径
            var fullPath = Path.GetFullPath(filePath);
            
            // 获取仓库根目录
            var currentDir = Directory.GetCurrentDirectory();
            var repoRoot = Path.GetFullPath(currentDir);
            
            // 构建允许的 ADR 目录路径
            var allowedPrefix = Path.Combine(repoRoot, "docs", "adr");
            var normalizedAllowedPrefix = Path.GetFullPath(allowedPrefix) + Path.DirectorySeparatorChar;
            var normalizedFullPath = fullPath + (Directory.Exists(fullPath) ? Path.DirectorySeparatorChar.ToString() : "");

            // 检查路径是否在允许的目录下
            if (!normalizedFullPath.StartsWith(normalizedAllowedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = $"目标路径必须在 docs/adr 目录下。当前路径: {fullPath}";
                return false;
            }

            // 检查是否包含可疑的路径遍历模式
            if (filePath.Contains("..") || filePath.Contains("~"))
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
