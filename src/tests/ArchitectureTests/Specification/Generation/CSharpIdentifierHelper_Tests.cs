namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation;

/// <summary>
/// CSharpIdentifierHelper 单元测试
/// </summary>
public sealed class CSharpIdentifierHelper_Tests
{
    [Theory(DisplayName = "ToValidIdentifier 应该转换特殊字符为下划线")]
    [InlineData("Hello World", "Hello_World")]
    [InlineData("test-case", "test_case")]
    [InlineData("path/to/file", "path_to_file")]
    [InlineData("(test)", "test")]
    [InlineData("[test]", "test")]
    [InlineData("test.name", "test_name")]
    public void ToValidIdentifier_Should_Replace_Special_Characters(string input, string expected)
    {
        // Act
        var result = CSharpIdentifierHelper.ToValidIdentifier(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "ToValidIdentifier 应该移除连续下划线")]
    [InlineData("test__name", "test_name")]
    [InlineData("test___name", "test_name")]
    [InlineData("__test", "test")]
    [InlineData("test__", "test")]
    public void ToValidIdentifier_Should_Remove_Consecutive_Underscores(string input, string expected)
    {
        // Act
        var result = CSharpIdentifierHelper.ToValidIdentifier(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "ToValidIdentifier 应该确保以字母或下划线开头")]
    [InlineData("123test", "_123test")]
    [InlineData("9name", "_9name")]
    public void ToValidIdentifier_Should_Ensure_Valid_Start(string input, string expected)
    {
        // Act
        var result = CSharpIdentifierHelper.ToValidIdentifier(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact(DisplayName = "ToValidIdentifier 应该对空输入抛出异常")]
    public void ToValidIdentifier_Should_Throw_For_Empty_Input()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CSharpIdentifierHelper.ToValidIdentifier(""));
        Assert.Throws<ArgumentException>(() => CSharpIdentifierHelper.ToValidIdentifier("   "));
    }

    [Theory(DisplayName = "ToPascalCase 应该转换为 Pascal 命名")]
    [InlineData("test", "Test")]
    [InlineData("test_name", "Test_name")]
    [InlineData("hello_world", "Hello_world")]
    public void ToPascalCase_Should_Convert_To_Pascal_Case(string input, string expected)
    {
        // Act
        var result = CSharpIdentifierHelper.ToPascalCase(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "IsValidIdentifier 应该验证标识符有效性")]
    [InlineData("ValidName", true)]
    [InlineData("_validName", true)]
    [InlineData("valid123", true)]
    [InlineData("123invalid", false)]
    [InlineData("invalid-name", false)]
    [InlineData("invalid name", false)]
    [InlineData("", false)]
    public void IsValidIdentifier_Should_Validate_Identifier(string input, bool expected)
    {
        // Act
        var result = CSharpIdentifierHelper.IsValidIdentifier(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "ToValidIdentifier 应该处理复杂的真实场景")]
    [InlineData("ArchitectureTests 的法律地位", "ArchitectureTests_的法律地位")]
    [InlineData("唯一执法形式", "唯一执法形式")]
    [InlineData("必须有测试", "必须有测试")]
    [InlineData("禁止无执法路径", "禁止无执法路径")]
    public void ToValidIdentifier_Should_Handle_Real_World_Scenarios(string input, string expected)
    {
        // Act
        var result = CSharpIdentifierHelper.ToValidIdentifier(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "ToValidIdentifier 应该对只包含被移除字符的输入返回 Unnamed")]
    [InlineData("()")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("<>")]
    [InlineData("\"'")]
    [InlineData("(){}[]<>\"'")]
    [InlineData("()()()")]
    [InlineData("[[[]]]")]
    [InlineData("???***")]
    [InlineData("____")]
    public void ToValidIdentifier_Should_Return_Unnamed_For_All_Removed_Input(string input)
    {
        // Act
        var result = CSharpIdentifierHelper.ToValidIdentifier(input);

        // Assert
        result.Should().Be("Unnamed");
    }
}
