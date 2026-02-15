using Zss.BilliardHall.Generators;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Tests;

/// <summary>
/// AgentInstructionGenerator 单元测试
/// </summary>
public sealed class AgentInstructionGenerator_Tests
{
    [Fact]
    public void GenerateInstructions_Should_Throw_When_RuleSet_Is_Null()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();

        // Act & Assert
        var act = () => generator.GenerateInstructions(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenerateInstructions_With_Options_Should_Throw_When_RuleSet_Is_Null()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var options = InstructionGenerationOptions.Default;

        // Act & Assert
        var act = () => generator.GenerateInstructions(null!, options);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenerateInstructions_With_Options_Should_Throw_When_Options_Is_Null()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(907);

        // Act & Assert
        var act = () => generator.GenerateInstructions(ruleSet, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenerateInstructions_Should_Return_Empty_Instructions_For_Empty_RuleSet()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(907);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("instructions:\n");
    }

    [Fact]
    public void GenerateInstructions_Should_Generate_Basic_YAML_Structure()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().NotBeNull();
        result.Should().StartWith("instructions:\n");
        result.Should().Contain("- id:");
        result.Should().Contain("description:");
        result.Should().Contain("action:");
        result.Should().Contain("conditions:");
        result.Should().Contain("output:");
        result.Should().Contain("tools:");
        result.Should().Contain("feedback:");
    }

    [Fact]
    public void GenerateInstructions_Should_Use_Correct_Instruction_ID_Format()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();
        var options = new InstructionGenerationOptions
        {
            AgentPrefix = "TG",
            StartInstructionNumber = 5
        };

        // Act
        var result = generator.GenerateInstructions(ruleSet, options);

        // Assert
        result.Should().Contain("id: TG-005");
    }

    [Fact]
    public void GenerateInstructions_Should_Include_Rule_Summary_In_Description()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("测试规则摘要");
    }

    [Fact]
    public void GenerateInstructions_Should_Generate_Correct_Output_Format()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("output: \"Allowed / Blocked / Uncertain\"");
    }

    [Fact]
    public void GenerateInstructions_Should_Include_RuleSet_API_In_Tools()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("tools:");
        result.Should().Contain("- \"RuleSet API\"");
        result.Should().Contain("- \"ArchitectureTests\"");
        result.Should().Contain("- \"ADR-907 RuleSet\"");
    }

    [Fact]
    public void GenerateInstructions_Should_Include_FailureObject_In_Feedback()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("feedback:");
        result.Should().Contain("生成 FailureObject");
    }

    [Fact]
    public void GenerateInstructions_Should_Include_Critical_Feedback_For_Critical_Rules()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Critical Rule",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Constitutional,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Test condition",
            enforcement: "Test enforcement",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("阻断 CI 管道（Constitutional 级别）");
    }

    [Fact]
    public void GenerateInstructions_Should_Include_Guidelines_When_Enabled()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();
        var options = new InstructionGenerationOptions
        {
            IncludeGuidelines = true
        };

        // Act
        var result = generator.GenerateInstructions(ruleSet, options);

        // Assert
        result.Should().Contain("guidelines:");
        result.Should().Contain("RuleSet API 查询示例");
        result.Should().Contain("约束检查逻辑");
    }

    [Fact]
    public void GenerateInstructions_Should_Not_Include_Guidelines_When_Disabled()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();
        var options = new InstructionGenerationOptions
        {
            IncludeGuidelines = false
        };

        // Act
        var result = generator.GenerateInstructions(ruleSet, options);

        // Assert
        result.Should().NotContain("guidelines:");
    }

    [Fact]
    public void GenerateInstructions_Should_Include_Commands_When_Enabled()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();
        var options = new InstructionGenerationOptions
        {
            IncludeTestCommands = true
        };

        // Act
        var result = generator.GenerateInstructions(ruleSet, options);

        // Assert
        result.Should().Contain("commands:");
        result.Should().Contain("run_adr_tests:");
        result.Should().Contain("run_all_architecture_tests:");
    }

    [Fact]
    public void GenerateInstructions_Should_Not_Include_Commands_When_Disabled()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();
        var options = new InstructionGenerationOptions
        {
            IncludeTestCommands = false
        };

        // Act
        var result = generator.GenerateInstructions(ruleSet, options);

        // Assert
        result.Should().NotContain("commands:");
    }

    [Fact]
    public void GenerateInstructions_Should_Handle_Multiple_Rules()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(907);

        // Add first rule
        ruleSet.AddRule(1, "First Rule", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Solution);
        ruleSet.AddClause(1, 1, "Condition 1", "Enforcement 1", ClauseExecutionType.StaticAnalysis);

        // Add second rule
        ruleSet.AddRule(2, "Second Rule", DecisionLevel.Should, RuleSeverity.Technical, RuleScope.Module);
        ruleSet.AddClause(2, 1, "Condition 2", "Enforcement 2", ClauseExecutionType.Convention);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("id: GEN-001");
        result.Should().Contain("id: GEN-002");
        result.Should().Contain("First Rule");
        result.Should().Contain("Second Rule");
    }

    [Fact]
    public void GenerateInstructions_Should_Escape_YAML_Special_Characters()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Rule with \"quotes\" and special chars",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Condition with \"quotes\"",
            enforcement: "Enforcement",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("\\\"");
    }

    [Fact]
    public void GenerateInstructions_Should_Use_LF_Line_Endings()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().NotContain("\r\n");
        result.Should().Contain("\n");
    }

    [Fact]
    public void GenerateInstructions_Should_Include_API_Query_Examples_For_Clauses()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("ruleSet.GetClause");
    }

    [Fact]
    public void GenerateInstructions_Should_Include_Constraint_Check_Logic()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("约束检查逻辑");
        result.Should().Contain("使用静态分析验证");
    }

    [Fact]
    public void GenerateInstructions_Should_Generate_Different_Conditions_Based_On_Scope()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();

        // Repository scope
        var repoRuleSet = new ArchitectureRuleSet(901);
        repoRuleSet.AddRule(1, "Repo Rule", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Solution);
        repoRuleSet.AddClause(1, 1, "Condition", "Enforcement", ClauseExecutionType.StaticAnalysis);

        // Module scope
        var moduleRuleSet = new ArchitectureRuleSet(902);
        moduleRuleSet.AddRule(1, "Module Rule", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Module);
        moduleRuleSet.AddClause(1, 1, "Condition", "Enforcement", ClauseExecutionType.StaticAnalysis);

        // Act
        var repoResult = generator.GenerateInstructions(repoRuleSet);
        var moduleResult = generator.GenerateInstructions(moduleRuleSet);

        // Assert
        repoResult.Should().Contain("CI pipeline");
        moduleResult.Should().Contain("Code Modified");
    }

    private static ArchitectureRuleSet CreateTestRuleSet()
    {
        var ruleSet = new ArchitectureRuleSet(907);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "测试规则摘要",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "测试条件",
            enforcement: "测试执行要求",
            executionType: ClauseExecutionType.StaticAnalysis);
        return ruleSet;
    }
}
