namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation;

/// <summary>
/// GeneratedTestCode 单元测试
/// </summary>
public sealed class GeneratedTestCode_Tests
{
    [Fact(DisplayName = "构造函数应该创建有效的实例")]
    public void Constructor_Should_Create_Valid_Instance()
    {
        // Act
        var result = new GeneratedTestCode(
            "TestClass",
            "public class TestClass { }",
            "Test.Namespace",
            1);

        // Assert
        result.ClassName.Should().Be("TestClass");
        result.SourceCode.Should().Be("public class TestClass { }");
        result.Namespace.Should().Be("Test.Namespace");
        result.TestMethodCount.Should().Be(1);
    }

    [Fact(DisplayName = "Validate 应该验证类名不为空")]
    public void Validate_Should_Check_ClassName_Not_Empty()
    {
        // Arrange
        var result = new GeneratedTestCode(
            "",
            "public class Test { }",
            "Test.Namespace",
            1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => result.Validate());
    }

    [Fact(DisplayName = "Validate 应该验证源代码不为空")]
    public void Validate_Should_Check_SourceCode_Not_Empty()
    {
        // Arrange
        var result = new GeneratedTestCode(
            "TestClass",
            "",
            "Test.Namespace",
            1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => result.Validate());
    }

    [Fact(DisplayName = "Validate 应该验证命名空间不为空")]
    public void Validate_Should_Check_Namespace_Not_Empty()
    {
        // Arrange
        var result = new GeneratedTestCode(
            "TestClass",
            "public class Test { }",
            "",
            1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => result.Validate());
    }

    [Fact(DisplayName = "Validate 应该验证测试方法数量大于0")]
    public void Validate_Should_Check_TestMethodCount_Greater_Than_Zero()
    {
        // Arrange
        var result = new GeneratedTestCode(
            "TestClass",
            "public class Test { }",
            "Test.Namespace",
            0);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => result.Validate());
    }

    [Fact(DisplayName = "Validate 对有效数据应该不抛出异常")]
    public void Validate_Should_Not_Throw_For_Valid_Data()
    {
        // Arrange
        var result = new GeneratedTestCode(
            "TestClass",
            "public class Test { }",
            "Test.Namespace",
            5);

        // Act & Assert
        var exception = Record.Exception(() => result.Validate());
        exception.Should().BeNull();
    }
}
