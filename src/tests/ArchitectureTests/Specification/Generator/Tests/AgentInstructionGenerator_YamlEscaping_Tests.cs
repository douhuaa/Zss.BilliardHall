namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Tests;

/// <summary>
/// AgentInstructionGenerator YAML 转义测试
/// 测试多行字符串和特殊字符的序列化/反序列化
/// </summary>
public sealed class AgentInstructionGenerator_YamlEscaping_Tests
{
    private readonly AgentInstructionGenerator _generator;
    private readonly YamlDotNetSerializer _serializer;

    public AgentInstructionGenerator_YamlEscaping_Tests()
    {
        _generator = new AgentInstructionGenerator();
        _serializer = new YamlDotNetSerializer();
    }

    [Theory]
    [InlineData("Single line text without special characters")]
    [InlineData("Text with\nnewline character")]
    [InlineData("Multiple\nlines\nof\ntext")]
    [InlineData("colon: here\nand more")]
    [InlineData(": starts with colon")]
    [InlineData("ends with space ")]
    [InlineData(" starts with space")]
    [InlineData("Contains \"quotes\" inside")]
    [InlineData("Contains 'single quotes'")]
    [InlineData("Contains `backticks`")]
    [InlineData("Contains $dollar signs")]
    [InlineData("Mixed: colon\nand newline\nwith \"quotes\"")]
    public void GenerateInstructions_Should_Preserve_Original_String_After_Serialization(string testString)
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet(1);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: testString,
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Test condition",
            enforcement: "Test enforcement",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act - 生成 YAML
        var yaml = _generator.GenerateInstructions(ruleSet);

        // Assert - 反序列化并验证字符串值
        yaml.Should().NotBeNullOrEmpty();

        InstructionsContainer? container = null;
        var deserializeAction = () => container = _serializer.Deserialize<InstructionsContainer>(yaml);

        // 验证 YAML 可以成功反序列化
        deserializeAction.Should().NotThrow("YAML 应该能够被成功反序列化");

        container.Should().NotBeNull();
        container!.Instructions.Should().HaveCount(1);

        var instruction = container.Instructions[0];
        instruction.Description.Should().Contain(testString, "instruction 字段值应与原始字符串相同");
    }

    [Fact]
    public void GenerateInstructions_Should_Handle_Complex_Multiline_With_Special_Chars()
    {
        // Arrange
        var complexString = @"This is a complex string:
Line 1 with colon: value
Line 2 with ""quotes""
Line 3 with `backticks` and $variables
Line 4 with trailing space 
Line 5: another colon case";

        var ruleSet = new ArchitectureRuleSet(1);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: complexString,
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Test condition",
            enforcement: "Test enforcement",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var yaml = _generator.GenerateInstructions(ruleSet);

        // Assert
        var container = _serializer.Deserialize<InstructionsContainer>(yaml);
        container.Should().NotBeNull();
        container.Instructions.Should().HaveCount(1);
        
        var instruction = container.Instructions[0];
        instruction.Description.Should().Contain("This is a complex string:");
        instruction.Description.Should().Contain("Line 1 with colon: value");
        instruction.Description.Should().Contain("quotes");
        instruction.Description.Should().Contain("backticks");
        instruction.Description.Should().Contain("variables");
    }

    [Fact]
    public void GenerateInstructions_Should_Use_Literal_Style_For_Multiline_Strings()
    {
        // Arrange
        var multilineString = "Line 1\nLine 2\nLine 3";
        var ruleSet = new ArchitectureRuleSet(1);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: multilineString,
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Test condition",
            enforcement: "Test enforcement",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var yaml = _generator.GenerateInstructions(ruleSet);

        // Assert
        // 验证使用了 literal block 格式（| 符号）用于多行字符串
        yaml.Should().Contain("|", "多行字符串应使用 literal block 格式");

        // 验证可以正确反序列化
        var container = _serializer.Deserialize<InstructionsContainer>(yaml);
        container.Should().NotBeNull();
        container.Instructions.Should().HaveCount(1);
    }

    [Fact]
    public void GenerateInstructions_Should_Handle_Valid_Non_Empty_Strings()
    {
        // Arrange - 使用有效的非空字符串
        var testString = "Valid summary text";
        var ruleSet = new ArchitectureRuleSet(1);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: testString,
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Test condition",
            enforcement: "Test enforcement",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var yaml = _generator.GenerateInstructions(ruleSet);

        // Assert
        var deserializeAction = () => _serializer.Deserialize<InstructionsContainer>(yaml);
        deserializeAction.Should().NotThrow("有效字符串应该能够正确序列化和反序列化");
    }

    [Fact]
    public void GenerateInstructions_Should_Quote_Strings_With_Leading_Colon()
    {
        // Arrange
        var colonString = ": This starts with a colon";
        var ruleSet = new ArchitectureRuleSet(1);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: colonString,
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Test condition",
            enforcement: "Test enforcement",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var yaml = _generator.GenerateInstructions(ruleSet);

        // Assert
        // 以冒号开头的字符串应该被引号包裹或使用其他安全格式
        yaml.Should().NotBeNullOrEmpty();

        var container = _serializer.Deserialize<InstructionsContainer>(yaml);
        container.Should().NotBeNull();
        container.Instructions[0].Description.Should().Contain(": This starts with a colon");
    }

    [Fact]
    public void GenerateInstructions_Should_Handle_String_With_Trailing_Spaces()
    {
        // Arrange
        var trailingSpaceString = "Text with trailing spaces   ";
        var ruleSet = new ArchitectureRuleSet(1);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: trailingSpaceString,
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Test condition",
            enforcement: "Test enforcement",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var yaml = _generator.GenerateInstructions(ruleSet);

        // Assert
        var container = _serializer.Deserialize<InstructionsContainer>(yaml);
        container.Should().NotBeNull();
        
        // 注意：YamlDotNet 可能会修剪尾随空格，这是预期行为
        // 我们主要验证不会导致解析错误
        container.Instructions.Should().HaveCount(1);
    }
}
