namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation;

/// <summary>
/// 生成的测试代码
/// </summary>
/// <param name="ClassName">测试类名</param>
/// <param name="SourceCode">生成的完整源代码</param>
/// <param name="Namespace">命名空间</param>
/// <param name="TestMethodCount">生成的测试方法数量</param>
public sealed record GeneratedTestCode(
    string ClassName,
    string SourceCode,
    string Namespace,
    int TestMethodCount
)
{
    /// <summary>
    /// 验证生成的代码有效性
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ClassName))
        {
            throw new ArgumentException("类名不能为空", nameof(ClassName));
        }

        if (string.IsNullOrWhiteSpace(SourceCode))
        {
            throw new ArgumentException("源代码不能为空", nameof(SourceCode));
        }

        if (string.IsNullOrWhiteSpace(Namespace))
        {
            throw new ArgumentException("命名空间不能为空", nameof(Namespace));
        }

        if (TestMethodCount <= 0)
        {
            throw new ArgumentException("测试方法数量必须大于0", nameof(TestMethodCount));
        }
    }
}
