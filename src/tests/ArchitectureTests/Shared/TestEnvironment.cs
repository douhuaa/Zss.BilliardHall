namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 共享测试环境工具类
/// 提供仓库根目录、ADR 路径等常用路径，避免在每个测试中重复查找逻辑
/// </summary>
public static class TestEnvironment
{
    private static readonly Lazy<string> _repositoryRoot =
        new(() => FindRepositoryRootCore() ?? throw new InvalidOperationException(
            "未找到仓库根目录。请确保在解决方案根目录或其子目录中运行测试。"),
            LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 获取仓库根目录（缓存）
    /// </summary>
    /// <exception cref="InvalidOperationException">未找到仓库根目录</exception>
    public static string RepositoryRoot => _repositoryRoot.Value;

    /// <summary>
    /// 获取 ADR 文档目录路径
    /// </summary>
    public static string AdrPath => Path.Combine(RepositoryRoot, "docs", "adr");

    /// <summary>
    /// 获取 Agent 文件目录路径
    /// </summary>
    public static string AgentFilesPath => Path.Combine(RepositoryRoot, ".github", "agents");

    /// <summary>
    /// 获取 Copilot 指令目录路径
    /// </summary>
    public static string CopilotInstructionsPath => Path.Combine(RepositoryRoot, ".github", "instructions");

    /// <summary>
    /// 获取源代码根目录
    /// </summary>
    public static string SourceRoot => Path.Combine(RepositoryRoot, "src");

    /// <summary>
    /// 获取模块目录路径
    /// </summary>
    public static string ModulesPath => Path.Combine(SourceRoot, "Modules");

    /// <summary>
    /// 获取 Host 目录路径
    /// </summary>
    public static string HostPath => Path.Combine(SourceRoot, "Host");

    /// <summary>
    /// 获取构建配置（Debug/Release）
    /// </summary>
    public static string BuildConfiguration =>
        Environment.GetEnvironmentVariable("Configuration") ?? "Debug";

    /// <summary>
    /// 支持的目标框架（按优先级排序）
    /// </summary>
    public static readonly string[] SupportedTargetFrameworks =
    {
        "net10.0",
        "net8.0",
        "net7.0",
        "net6.0",
        "net5.0"
    };

    /// <summary>
    /// 查找仓库根目录的核心逻辑
    /// 通过查找 Zss.BilliardHall.slnx 文件来确定根目录
    /// </summary>
    private static string? FindRepositoryRootCore()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        Debug.WriteLine($"[TestEnvironment] 开始查找仓库根目录，起始路径: {current.FullName}");

        while (current != null)
        {
            var slnxPath = Path.Combine(current.FullName, "Zss.BilliardHall.slnx");

            if (File.Exists(slnxPath))
            {
                Debug.WriteLine($"[TestEnvironment] 找到仓库根目录: {current.FullName}");
                return current.FullName;
            }

            current = current.Parent;
        }

        Debug.WriteLine("[TestEnvironment] 未找到仓库根目录");
        return null;
    }

    /// <summary>
    /// 验证必需的目录是否存在
    /// </summary>
    public static void ValidateEnvironment()
    {
        if (!Directory.Exists(AdrPath))
        {
            throw new DirectoryNotFoundException(
                $"ADR 目录不存在: {AdrPath}\n" +
                $"仓库根目录: {RepositoryRoot}");
        }
    }
}
