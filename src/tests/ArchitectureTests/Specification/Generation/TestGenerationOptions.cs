namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation;

/// <summary>
/// 测试生成配置选项
/// </summary>
public sealed class TestGenerationOptions
{
    /// <summary>
    /// 生成的测试类命名空间
    /// </summary>
    public string Namespace { get; init; } = "Zss.BilliardHall.Tests.ArchitectureTests.Generated";

    /// <summary>
    /// 测试类名后缀
    /// 默认：_Tests
    /// </summary>
    public string TestClassSuffix { get; init; } = "_Tests";

    /// <summary>
    /// 是否包含详细注释
    /// </summary>
    public bool IncludeComments { get; init; } = true;

    /// <summary>
    /// 是否包含示例实现
    /// </summary>
    public bool IncludeExampleImplementation { get; init; } = true;

    /// <summary>
    /// 是否生成 Theory 测试（使用 MemberData）
    /// 如果为 false，则生成独立的 Fact 方法
    /// </summary>
    public bool UseTheoryPattern { get; init; } = true;

    /// <summary>
    /// 代码缩进字符串
    /// 默认：4个空格
    /// </summary>
    public string IndentString { get; init; } = "    ";

    /// <summary>
    /// 创建默认选项
    /// </summary>
    public static TestGenerationOptions Default => new();
}
