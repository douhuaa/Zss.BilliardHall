namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_0907;

/// <summary>
/// ADR-907 测试的共享辅助类
/// 提供常量和辅助方法供各规则测试使用
/// </summary>
internal static class ADR_0907_TestHelpers
{
    public const string AdrDocsPath = "docs/adr";
    public const string AdrTestsPath = "src/tests/ArchitectureTests";
    public const string AdrTestsProjectPath = "src/tests/ArchitectureTests/ArchitectureTests.csproj";

    /// <summary>
    /// 查找仓库根目录
    /// </summary>
    public static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();

        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) || 
                Directory.Exists(Path.Combine(currentDir, AdrDocsPath)))
            {
                return currentDir;
            }

            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        return null;
    }
}
