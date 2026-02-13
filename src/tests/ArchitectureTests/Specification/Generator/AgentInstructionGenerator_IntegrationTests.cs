namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Tests;

/// <summary>
/// AgentInstructionGenerator 集成测试
/// 测试与实际 RuleSet 的集成
/// </summary>
public sealed class AgentInstructionGenerator_IntegrationTests
{
    [Fact]
    public void GenerateInstructions_Should_Work_With_Adr907_RuleSet()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSetDef = new Adr907RuleSet();
        var ruleSet = ruleSetDef.Define();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("instructions:\n");
        result.Should().Contain("id: GEN-");
        result.Should().Contain("ADR-907 RuleSet");
    }

    [Fact]
    public void GenerateInstructions_Should_Handle_Complex_RuleSet_With_Multiple_Rules()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateComplexRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // Should contain all rules
        result.Should().Contain("id: GEN-001");
        result.Should().Contain("id: GEN-002");
        result.Should().Contain("id: GEN-003");

        // Should have proper YAML structure
        var lines = result.Split('\n');
        lines[0].Should().Be("instructions:");
    }

    [Fact]
    public void GenerateInstructions_With_Custom_Options_Should_Produce_Valid_YAML()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateSimpleRuleSet();
        var options = new InstructionGenerationOptions
        {
            AgentPrefix = "AG",
            AgentName = "Architecture Guardian",
            StartInstructionNumber = 10,
            IncludeApiExamples = true,
            IncludeConstraintChecks = true,
            IncludeTestCommands = true,
            IncludeGuidelines = true,
            IndentSpaces = 2
        };

        // Act
        var result = generator.GenerateInstructions(ruleSet, options);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("id: AG-010");
        result.Should().Contain("guidelines:");
        result.Should().Contain("commands:");
    }

    [Fact]
    public void GenerateInstructions_Should_Be_Valid_YAML()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateSimpleRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        // 验证基本 YAML 结构
        result.Should().NotBeNullOrEmpty();

        // 验证缩进一致性
        var lines = result.Split('\n');
        foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            if (line.StartsWith("  "))
            {
                // 所有缩进应该是 2 的倍数
                var leadingSpaces = line.TakeWhile(c => c == ' ').Count();
                (leadingSpaces % 2).Should().Be(0);
            }
        }
    }

    [Fact]
    public void GenerateInstructions_Should_Include_All_Execution_Types()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(999);

        ruleSet.AddRule(1, "Static Analysis Rule", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Solution);
        ruleSet.AddClause(1, 1, "Condition", "Enforcement", ClauseExecutionType.StaticAnalysis);

        ruleSet.AddRule(2, "Convention Check Rule", DecisionLevel.Must, RuleSeverity.Technical, RuleScope.Module);
        ruleSet.AddClause(2, 1, "Condition", "Enforcement", ClauseExecutionType.Convention);

        ruleSet.AddRule(3, "Runtime Check Rule", DecisionLevel.Should, RuleSeverity.Technical, RuleScope.Module);
        ruleSet.AddClause(3, 1, "Condition", "Enforcement", ClauseExecutionType.Runtime);

        ruleSet.AddRule(4, "Manual Review Rule", DecisionLevel.Should, RuleSeverity.Technical, RuleScope.Module);
        ruleSet.AddClause(4, 1, "Condition", "Enforcement", ClauseExecutionType.ManualReview);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("使用静态分析验证");
        result.Should().Contain("检查约定遵守情况");
        result.Should().Contain("运行时检查");
        result.Should().Contain("需要人工审查");
    }

    [Fact]
    public void GenerateInstructions_Should_Handle_Rules_With_Multiple_Clauses()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(950);

        ruleSet.AddRule(1, "Multi-Clause Rule", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Solution);
        ruleSet.AddClause(1, 1, "Condition 1", "Enforcement 1", ClauseExecutionType.StaticAnalysis);
        ruleSet.AddClause(1, 2, "Condition 2", "Enforcement 2", ClauseExecutionType.Convention);
        ruleSet.AddClause(1, 3, "Condition 3", "Enforcement 3", ClauseExecutionType.Runtime);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("验证 ADR-950_1 的 3 个约束条款");
        result.Should().Contain("ruleSet.GetClause(1, 1)");
        result.Should().Contain("ruleSet.GetClause(1, 2)");
        result.Should().Contain("ruleSet.GetClause(1, 3)");
    }

    [Fact]
    public void GenerateInstructions_Output_Should_Be_Deterministic()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateSimpleRuleSet();

        // Act
        var result1 = generator.GenerateInstructions(ruleSet);
        var result2 = generator.GenerateInstructions(ruleSet);

        // Assert
        result1.Should().Be(result2);
    }

    private static ArchitectureRuleSet CreateSimpleRuleSet()
    {
        var ruleSet = new ArchitectureRuleSet(910);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "简单测试规则",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "简单测试条件",
            enforcement: "简单测试执行要求",
            executionType: ClauseExecutionType.StaticAnalysis);
        return ruleSet;
    }

    private static ArchitectureRuleSet CreateComplexRuleSet()
    {
        var ruleSet = new ArchitectureRuleSet(920);

        // Rule 1: Critical with 2 clauses
        ruleSet.AddRule(1, "Critical Rule", DecisionLevel.Must, RuleSeverity.Constitutional, RuleScope.Solution);
        ruleSet.AddClause(1, 1, "Critical Condition 1", "Critical Enforcement 1", ClauseExecutionType.StaticAnalysis);
        ruleSet.AddClause(1, 2, "Critical Condition 2", "Critical Enforcement 2", ClauseExecutionType.Convention);

        // Rule 2: High with 1 clause
        ruleSet.AddRule(2, "High Priority Rule", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Module);
        ruleSet.AddClause(2, 1, "High Condition", "High Enforcement", ClauseExecutionType.StaticAnalysis);

        // Rule 3: Medium with 3 clauses
        ruleSet.AddRule(3, "Medium Priority Rule", DecisionLevel.Should, RuleSeverity.Technical, RuleScope.Module);
        ruleSet.AddClause(3, 1, "Medium Condition 1", "Medium Enforcement 1", ClauseExecutionType.Convention);
        ruleSet.AddClause(3, 2, "Medium Condition 2", "Medium Enforcement 2", ClauseExecutionType.Runtime);
        ruleSet.AddClause(3, 3, "Medium Condition 3", "Medium Enforcement 3", ClauseExecutionType.ManualReview);

        return ruleSet;
    }
}
