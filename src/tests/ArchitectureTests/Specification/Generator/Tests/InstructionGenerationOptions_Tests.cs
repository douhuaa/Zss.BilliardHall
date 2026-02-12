namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Tests;

/// <summary>
/// InstructionGenerationOptions 测试
/// </summary>
public sealed class InstructionGenerationOptions_Tests
{
    [Fact]
    public void Default_Should_Return_Valid_Options()
    {
        // Act
        var options = InstructionGenerationOptions.Default;

        // Assert
        options.Should().NotBeNull();
        options.AgentPrefix.Should().Be("GEN");
        options.AgentName.Should().Be("Generated Agent");
        options.StartInstructionNumber.Should().Be(1);
        options.IncludeApiExamples.Should().BeTrue();
        options.IncludeConstraintChecks.Should().BeTrue();
        options.IncludeTestCommands.Should().BeTrue();
        options.IncludeGuidelines.Should().BeTrue();
        options.IndentSpaces.Should().Be(2);
    }

    [Fact]
    public void Validate_Should_Pass_For_Valid_Options()
    {
        // Arrange
        var options = new InstructionGenerationOptions
        {
            AgentPrefix = "TG",
            AgentName = "Test Generator",
            StartInstructionNumber = 1,
            IndentSpaces = 2
        };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null, "AgentPrefix 不能为空")]
    [InlineData("", "AgentPrefix 不能为空")]
    [InlineData("   ", "AgentPrefix 不能为空")]
    public void Validate_Should_Throw_When_AgentPrefix_Is_Invalid_Empty(string? prefix, string expectedMessage)
    {
        // Arrange
        var options = new InstructionGenerationOptions { AgentPrefix = prefix! };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage($"{expectedMessage}*");
    }

    [Theory]
    [InlineData("A", "AgentPrefix 必须是 2-3 个大写字母")]
    [InlineData("ABCD", "AgentPrefix 必须是 2-3 个大写字母")]
    [InlineData("ab", "AgentPrefix 必须是 2-3 个大写字母")]
    [InlineData("A1", "AgentPrefix 必须是 2-3 个大写字母")]
    [InlineData("1A", "AgentPrefix 必须是 2-3 个大写字母")]
    public void Validate_Should_Throw_When_AgentPrefix_Format_Is_Invalid(string prefix, string expectedMessage)
    {
        // Arrange
        var options = new InstructionGenerationOptions { AgentPrefix = prefix };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage($"{expectedMessage}*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Should_Throw_When_AgentName_Is_Empty(string? name)
    {
        // Arrange
        var options = new InstructionGenerationOptions { AgentName = name! };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("AgentName 不能为空*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_Should_Throw_When_StartInstructionNumber_Is_Invalid(int number)
    {
        // Arrange
        var options = new InstructionGenerationOptions { StartInstructionNumber = number };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("StartInstructionNumber 必须大于 0*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(9)]
    [InlineData(10)]
    public void Validate_Should_Throw_When_IndentSpaces_Is_Out_Of_Range(int spaces)
    {
        // Arrange
        var options = new InstructionGenerationOptions { IndentSpaces = spaces };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("IndentSpaces 必须在 1-8 之间*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    public void Validate_Should_Pass_When_IndentSpaces_Is_Valid(int spaces)
    {
        // Arrange
        var options = new InstructionGenerationOptions { IndentSpaces = spaces };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().NotThrow();
    }
}
