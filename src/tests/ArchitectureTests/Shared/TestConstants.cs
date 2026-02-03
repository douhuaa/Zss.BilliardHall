namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 测试常量定义
/// 集中管理测试中使用的魔法字符串和常量，避免散布在各个测试文件中
/// </summary>
public static class TestConstants
{
    /// <summary>
    /// 架构测试的命名空间前缀
    /// </summary>
    public const string ArchitectureTestNamespace = "Zss.BilliardHall.Tests.ArchitectureTests";

    /// <summary>
    /// ADR 架构测试的命名空间
    /// </summary>
    public const string AdrTestNamespace = "Zss.BilliardHall.Tests.ArchitectureTests.ADR";

    /// <summary>
    /// ADR 测试文件名模式（正则表达式）
    /// 匹配格式：ADR_001_Architecture_Tests
    /// </summary>
    public const string AdrTestPattern = @"ADR_(\d{4})_Architecture_Tests";

    /// <summary>
    /// 模块命名空间前缀
    /// </summary>
    public const string ModuleNamespace = "Zss.BilliardHall.Modules";

    /// <summary>
    /// Platform 命名空间
    /// </summary>
    public const string PlatformNamespace = "Zss.BilliardHall.Platform";

    /// <summary>
    /// Platform Bootstrapper 类型全名
    /// </summary>
    public const string PlatformBootstrapperTypeName = "Zss.BilliardHall.Platform.PlatformBootstrapper";

    /// <summary>
    /// BuildingBlocks 命名空间
    /// </summary>
    public const string BuildingBlocksNamespace = "Zss.BilliardHall.BuildingBlocks";

    /// <summary>
    /// Host 命名空间前缀
    /// </summary>
    public const string HostNamespace = "Zss.BilliardHall.Host";

    /// <summary>
    /// ADR 文件命名模式（正则表达式）
    /// 匹配格式：ADR-001-description.md
    /// </summary>
    public const string AdrFilePattern = @"^ADR-\d{4}[^/\\]*\.md$";

    /// <summary>
    /// ADR 编号模式（正则表达式）
    /// 匹配格式：ADR-001
    /// </summary>
    public const string AdrIdPattern = @"^ADR-\d{4}$";

    /// <summary>
    /// 构建配置（Debug/Release）
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
}
