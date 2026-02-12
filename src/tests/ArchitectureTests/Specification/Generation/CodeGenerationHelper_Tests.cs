namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation;

/// <summary>
/// CodeGenerationHelper 单元测试
/// </summary>
public sealed class CodeGenerationHelper_Tests
{
    [Theory(DisplayName = "NormalizeNewlines 应该规范化行尾")]
    [InlineData("line1\r\nline2", "line1\nline2")]
    [InlineData("line1\rline2", "line1\nline2")]
    [InlineData("line1\nline2", "line1\nline2")]
    [InlineData("", "")]
    public void NormalizeNewlines_Should_Normalize_Line_Endings(string input, string expected)
    {
        // Act
        var result = CodeGenerationHelper.NormalizeNewlines(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Indent 应该正确添加缩进")]
    [InlineData("test", 1, "    test")]
    [InlineData("test", 2, "        test")]
    [InlineData("line1\nline2", 1, "    line1\n    line2")]
    public void Indent_Should_Add_Indentation(string text, int level, string expected)
    {
        // Act
        var result = CodeGenerationHelper.Indent(text, level);

        // Assert
        result.Should().Be(expected);
    }

    [Fact(DisplayName = "BuildCodeBlock 应该合并代码行")]
    public void BuildCodeBlock_Should_Combine_Lines()
    {
        // Act
        var result = CodeGenerationHelper.BuildCodeBlock("line1", "line2", "line3");

        // Assert
        result.Should().Be("line1\nline2\nline3");
    }

    [Fact(DisplayName = "BuildXmlDocComment 应该生成 XML 文档注释")]
    public void BuildXmlDocComment_Should_Generate_Xml_Doc_Comment()
    {
        // Act
        var result = CodeGenerationHelper.BuildXmlDocComment("Test summary");

        // Assert
        result.Should().Contain("/// <summary>");
        result.Should().Contain("/// Test summary");
        result.Should().Contain("/// </summary>");
    }

    [Fact(DisplayName = "BuildXmlDocCommentMultiLine 应该生成多行 XML 文档注释")]
    public void BuildXmlDocCommentMultiLine_Should_Generate_MultiLine_Xml_Doc_Comment()
    {
        // Arrange
        var lines = new[] { "Line 1", "Line 2", "Line 3" };

        // Act
        var result = CodeGenerationHelper.BuildXmlDocCommentMultiLine(lines);

        // Assert
        result.Should().Contain("/// <summary>");
        result.Should().Contain("/// Line 1");
        result.Should().Contain("/// Line 2");
        result.Should().Contain("/// Line 3");
        result.Should().Contain("/// </summary>");
    }

    [Theory(DisplayName = "EscapeStringLiteral 应该转义字符串字面量")]
    [InlineData("test", "test")]
    [InlineData("test\"quote", "test\\\"quote")]
    [InlineData("test\\slash", "test\\\\slash")]
    [InlineData("test\nline", "test\\nline")]
    public void EscapeStringLiteral_Should_Escape_String_Literal(string input, string expected)
    {
        // Act
        var result = CodeGenerationHelper.EscapeStringLiteral(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact(DisplayName = "BuildNamespaceDeclaration 应该生成命名空间声明")]
    public void BuildNamespaceDeclaration_Should_Generate_Namespace_Declaration()
    {
        // Act
        var result = CodeGenerationHelper.BuildNamespaceDeclaration("Test.Namespace");

        // Assert
        result.Should().Be("namespace Test.Namespace;");
    }

    [Fact(DisplayName = "BuildUsingStatements 应该生成 using 语句")]
    public void BuildUsingStatements_Should_Generate_Using_Statements()
    {
        // Act
        var result = CodeGenerationHelper.BuildUsingStatements(
            "System",
            "System.Linq",
            "Xunit");

        // Assert
        result.Should().Contain("using System;");
        result.Should().Contain("using System.Linq;");
        result.Should().Contain("using Xunit;");
    }
}
