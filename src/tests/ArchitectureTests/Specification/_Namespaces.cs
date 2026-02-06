namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

/// <summary>
/// 命名空间规范定义
/// 定义系统中使用的标准命名空间，作为架构测试的规范源
/// </summary>
public sealed class _Namespaces
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static readonly _Namespaces Instance = new();

    /// <summary>
    /// 架构测试的命名空间前缀
    /// </summary>
    public string ArchitectureTests => "Zss.BilliardHall.Tests.ArchitectureTests";

    /// <summary>
    /// ADR 架构测试的命名空间
    /// </summary>
    public string AdrTests => "Zss.BilliardHall.Tests.ArchitectureTests.ADR";

    /// <summary>
    /// 模块命名空间前缀
    /// </summary>
    public string Modules => "Zss.BilliardHall.Modules";

    /// <summary>
    /// Platform 命名空间
    /// </summary>
    public string Platform => "Zss.BilliardHall.Platform";

    /// <summary>
    /// BuildingBlocks 命名空间
    /// </summary>
    public string BuildingBlocks => "Zss.BilliardHall.BuildingBlocks";

    /// <summary>
    /// Host 命名空间前缀
    /// </summary>
    public string Host => "Zss.BilliardHall.Host";

    /// <summary>
    /// Platform Bootstrapper 类型全名
    /// </summary>
    public string PlatformBootstrapperType => "Zss.BilliardHall.Platform.PlatformBootstrapper";

    private _Namespaces() { }
}
