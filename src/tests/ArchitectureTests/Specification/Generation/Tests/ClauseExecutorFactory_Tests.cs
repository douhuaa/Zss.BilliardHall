using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation.ClauseExecutors;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation.Tests;

/// <summary>
/// ClauseExecutorFactory 单元测试
/// </summary>
public sealed class ClauseExecutorFactory_Tests
{
    private readonly IClauseExecutorFactory _factory;

    public ClauseExecutorFactory_Tests()
    {
        _factory = new ClauseExecutorFactory();
    }

    [Theory(DisplayName = "GetExecutor 应该为所有 ClauseExecutionType 返回正确的执行器")]
    [InlineData(ClauseExecutionType.Convention, typeof(ConventionExecutor))]
    [InlineData(ClauseExecutionType.StaticAnalysis, typeof(StaticAnalysisExecutor))]
    [InlineData(ClauseExecutionType.Runtime, typeof(RuntimeExecutor))]
    [InlineData(ClauseExecutionType.Documentation, typeof(DocumentationExecutor))]
    [InlineData(ClauseExecutionType.ManualReview, typeof(ManualReviewExecutor))]
    public void GetExecutor_Should_Return_Correct_Executor_For_All_Types(
        ClauseExecutionType executionType,
        Type expectedType)
    {
        // Act
        var executor = _factory.GetExecutor(executionType);

        // Assert
        executor.Should().NotBeNull();
        executor.Should().BeOfType(expectedType);
        executor.SupportedType.Should().Be(executionType);
    }

    [Fact(DisplayName = "GetExecutor 应该为相同类型返回相同实例")]
    public void GetExecutor_Should_Return_Same_Instance_For_Same_Type()
    {
        // Act
        var executor1 = _factory.GetExecutor(ClauseExecutionType.Convention);
        var executor2 = _factory.GetExecutor(ClauseExecutionType.Convention);

        // Assert
        executor1.Should().BeSameAs(executor2);
    }

    [Fact(DisplayName = "GetExecutor 应该对所有枚举值返回执行器")]
    public void GetExecutor_Should_Return_Executor_For_All_Enum_Values()
    {
        // Arrange
        var allTypes = Enum.GetValues(typeof(ClauseExecutionType)).Cast<ClauseExecutionType>();

        // Act & Assert
        foreach (var executionType in allTypes)
        {
            var executor = _factory.GetExecutor(executionType);
            executor.Should().NotBeNull($"应该为 {executionType} 返回执行器");
            executor.SupportedType.Should().Be(executionType);
        }
    }
}
