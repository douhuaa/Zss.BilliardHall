using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation.ClauseExecutors;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation.Tests;

/// <summary>
/// ClauseExecutor 单元测试
/// </summary>
public sealed class ClauseExecutor_Tests
{
    private ArchitectureClauseDefinition CreateTestClause(ClauseExecutionType executionType)
    {
        return new ArchitectureClauseDefinition(
            ArchitectureRuleId.Clause(999, 1, 1),
            "测试条件",
            "测试执行要求",
            executionType);
    }

    #region ConventionExecutor Tests

    [Fact(DisplayName = "ConventionExecutor 应该返回正确的执行类型")]
    public void ConventionExecutor_Should_Return_Correct_SupportedType()
    {
        // Arrange
        var executor = new ConventionExecutor();

        // Assert
        executor.SupportedType.Should().Be(ClauseExecutionType.Convention);
    }

    [Fact(DisplayName = "ConventionExecutor 应该生成 Convention 断言代码")]
    public void ConventionExecutor_Should_Generate_Convention_Assertion_Code()
    {
        // Arrange
        var executor = new ConventionExecutor();
        var clause = CreateTestClause(ClauseExecutionType.Convention);
        var indent = "    ";

        // Act
        var code = executor.GenerateAssertionCode(clause, indent);

        // Assert
        code.Should().NotBeNullOrWhiteSpace();
        code.Should().Contain("Convention 类型");
        code.Should().Contain("NetArchTest");
        code.Should().Contain("ExecuteConventionTest");
    }

    [Fact(DisplayName = "ConventionExecutor 应该对 null clause 抛出异常")]
    public void ConventionExecutor_Should_Throw_For_Null_Clause()
    {
        // Arrange
        var executor = new ConventionExecutor();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            executor.GenerateAssertionCode(null!, "    "));
    }

    #endregion

    #region StaticAnalysisExecutor Tests

    [Fact(DisplayName = "StaticAnalysisExecutor 应该返回正确的执行类型")]
    public void StaticAnalysisExecutor_Should_Return_Correct_SupportedType()
    {
        // Arrange
        var executor = new StaticAnalysisExecutor();

        // Assert
        executor.SupportedType.Should().Be(ClauseExecutionType.StaticAnalysis);
    }

    [Fact(DisplayName = "StaticAnalysisExecutor 应该生成 StaticAnalysis 断言代码")]
    public void StaticAnalysisExecutor_Should_Generate_StaticAnalysis_Assertion_Code()
    {
        // Arrange
        var executor = new StaticAnalysisExecutor();
        var clause = CreateTestClause(ClauseExecutionType.StaticAnalysis);
        var indent = "    ";

        // Act
        var code = executor.GenerateAssertionCode(clause, indent);

        // Assert
        code.Should().NotBeNullOrWhiteSpace();
        code.Should().Contain("StaticAnalysis 类型");
        code.Should().Contain("Roslyn Analyzer");
    }

    #endregion

    #region RuntimeExecutor Tests

    [Fact(DisplayName = "RuntimeExecutor 应该返回正确的执行类型")]
    public void RuntimeExecutor_Should_Return_Correct_SupportedType()
    {
        // Arrange
        var executor = new RuntimeExecutor();

        // Assert
        executor.SupportedType.Should().Be(ClauseExecutionType.Runtime);
    }

    [Fact(DisplayName = "RuntimeExecutor 应该生成 Runtime 断言代码")]
    public void RuntimeExecutor_Should_Generate_Runtime_Assertion_Code()
    {
        // Arrange
        var executor = new RuntimeExecutor();
        var clause = CreateTestClause(ClauseExecutionType.Runtime);
        var indent = "    ";

        // Act
        var code = executor.GenerateAssertionCode(clause, indent);

        // Assert
        code.Should().NotBeNullOrWhiteSpace();
        code.Should().Contain("Runtime 类型");
        code.Should().Contain("运行时验证");
    }

    #endregion

    #region DocumentationExecutor Tests

    [Fact(DisplayName = "DocumentationExecutor 应该返回正确的执行类型")]
    public void DocumentationExecutor_Should_Return_Correct_SupportedType()
    {
        // Arrange
        var executor = new DocumentationExecutor();

        // Assert
        executor.SupportedType.Should().Be(ClauseExecutionType.Documentation);
    }

    [Fact(DisplayName = "DocumentationExecutor 应该生成 Documentation 断言代码")]
    public void DocumentationExecutor_Should_Generate_Documentation_Assertion_Code()
    {
        // Arrange
        var executor = new DocumentationExecutor();
        var clause = CreateTestClause(ClauseExecutionType.Documentation);
        var indent = "    ";

        // Act
        var code = executor.GenerateAssertionCode(clause, indent);

        // Assert
        code.Should().NotBeNullOrWhiteSpace();
        code.Should().Contain("Documentation 类型");
        code.Should().Contain("文档验证");
    }

    #endregion

    #region ManualReviewExecutor Tests

    [Fact(DisplayName = "ManualReviewExecutor 应该返回正确的执行类型")]
    public void ManualReviewExecutor_Should_Return_Correct_SupportedType()
    {
        // Arrange
        var executor = new ManualReviewExecutor();

        // Assert
        executor.SupportedType.Should().Be(ClauseExecutionType.ManualReview);
    }

    [Fact(DisplayName = "ManualReviewExecutor 应该生成 ManualReview 断言代码")]
    public void ManualReviewExecutor_Should_Generate_ManualReview_Assertion_Code()
    {
        // Arrange
        var executor = new ManualReviewExecutor();
        var clause = CreateTestClause(ClauseExecutionType.ManualReview);
        var indent = "    ";

        // Act
        var code = executor.GenerateAssertionCode(clause, indent);

        // Assert
        code.Should().NotBeNullOrWhiteSpace();
        code.Should().Contain("ManualReview 类型");
        code.Should().Contain("人工审查");
    }

    #endregion

    #region Indent Handling Tests

    [Theory(DisplayName = "所有 Executor 应该正确处理缩进")]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData("        ")]
    [InlineData("\t")]
    public void All_Executors_Should_Handle_Indent_Correctly(string indent)
    {
        // Arrange
        var executors = new IClauseExecutor[]
        {
            new ConventionExecutor(),
            new StaticAnalysisExecutor(),
            new RuntimeExecutor(),
            new DocumentationExecutor(),
            new ManualReviewExecutor()
        };

        // Act & Assert
        foreach (var executor in executors)
        {
            var clause = CreateTestClause(executor.SupportedType);
            var code = executor.GenerateAssertionCode(clause, indent);

            code.Should().NotBeNull($"{executor.GetType().Name} 应该返回代码");

            // 如果有缩进，验证生成的代码包含缩进
            if (!string.IsNullOrEmpty(indent))
            {
                code.Should().Contain(indent,
                    $"{executor.GetType().Name} 生成的代码应该包含缩进");
            }
        }
    }

    #endregion
}
