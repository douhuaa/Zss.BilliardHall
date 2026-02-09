namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Common;

/// <summary>
/// 架构规则配置选项
/// 用于配置规则集的运行时参数，支持 IOptionsSnapshot
/// </summary>
public sealed class ArchitectureRulesOptions
{
    /// <summary>
    /// 测试项目名称（默认：ArchitectureTests）
    /// </summary>
    public string TestsProjectName { get; set; } = "ArchitectureTests";

    /// <summary>
    /// 每个架构测试类的最小 Fact/Theory 数量（默认：3）
    /// 用于反作弊检查，确保测试类不是空壳
    /// </summary>
    public int MinimumFactPerClass { get; set; } = 3;

    /// <summary>
    /// 领域模块命名空间前缀（默认：Zss.BilliardHall.Modules）
    /// </summary>
    public string ModulesNamespacePrefix { get; set; } = "Zss.BilliardHall.Modules";

    /// <summary>
    /// 领域事件命名空间模式（默认：*.Domain.Events）
    /// </summary>
    public string DomainEventsNamespacePattern { get; set; } = "*.Domain.Events";

    /// <summary>
    /// 仓储接口命名空间模式（默认：*.Domain.Repositories）
    /// </summary>
    public string RepositoryNamespacePattern { get; set; } = "*.Domain.Repositories";

    /// <summary>
    /// 是否启用 Heuristics 层规则（默认：true）
    /// </summary>
    public bool EnableHeuristics { get; set; } = true;

    /// <summary>
    /// 排除的测试类命名模式（如示例类、辅助类等）
    /// </summary>
    public string[] ExcludedTestClassPatterns { get; set; } = new[] { "Example", "Helper", "Mock" };

    /// <summary>
    /// 最小断言检查时排除的测试类模式（如不变量测试、自动生成的测试）
    /// </summary>
    public string[] MinimumAssertionExcludePatterns { get; set; } = new[] { "Invariants", "Auto" };
}
