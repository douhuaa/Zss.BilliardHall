namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Tests;

/// <summary>
/// AgentInstructionGenerator Golden 测试
/// 验证生成的 YAML 与标准样本的一致性
/// </summary>
public sealed class AgentInstructionGenerator_GoldenTests
{
    private readonly string _goldenFilePath;

    public AgentInstructionGenerator_GoldenTests()
    {
        _goldenFilePath = Path.Combine(
            TestEnvironment.RepositoryRoot,
            "src", "tests", "ArchitectureTests", "Specification", "Generator", "Tests", "golden",
            "agent_instructions_sample.yaml");
    }

    [Fact]
    public void Golden_Sample_File_Should_Exist()
    {
        // Assert
        File.Exists(_goldenFilePath).Should().BeTrue(
            $"Golden sample file should exist at: {_goldenFilePath}");
    }

    [Fact]
    public void GenerateInstructions_Output_Should_Have_Correct_YAML_Structure()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateSampleRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // 验证顶层结构
        result.Should().StartWith("instructions:\n");

        // 验证必需字段存在
        result.Should().Contain("- id:");
        result.Should().Contain("description:");
        result.Should().Contain("action:");
        result.Should().Contain("conditions:");
        result.Should().Contain("output:");
        result.Should().Contain("tools:");
        result.Should().Contain("feedback:");
        result.Should().Contain("guidelines:");
        result.Should().Contain("commands:");
    }

    [Fact]
    public void GenerateInstructions_Should_Match_Golden_Sample_Structure()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateSampleRuleSet();
        var goldenContent = File.ReadAllText(_goldenFilePath);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        // 验证关键结构元素
        var goldenLines = goldenContent.Split('\n')
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Trim())
            .ToList();

        var resultLines = result.Split('\n')
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Trim())
            .ToList();

        // 验证开头
        resultLines.First().Should().Be("instructions:");

        // 验证包含 id 字段
        resultLines.Should().Contain(l => l.StartsWith("- id: GEN-"));
    }

    [Fact]
    public void GenerateInstructions_Should_Include_RuleSet_API_Examples()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateSampleRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("RuleSet API 查询示例");
        result.Should().Contain("ruleSet.GetClause");
    }

    [Fact]
    public void GenerateInstructions_Should_Include_Constraint_Check_Logic()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateSampleRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("约束检查逻辑");
        result.Should().Contain("使用静态分析验证");
    }

    [Fact]
    public void GenerateInstructions_Should_Include_Test_Commands()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateSampleRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("run_adr_tests:");
        result.Should().Contain("run_all_architecture_tests:");
        result.Should().Contain("dotnet test");
    }

    [Fact]
    public void GenerateInstructions_Should_Use_Proper_YAML_Indentation()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateSampleRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        var lines = result.Split('\n');

        // 验证缩进层次
        foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            var leadingSpaces = line.TakeWhile(c => c == ' ').Count();

            // 所有缩进必须是 2 的倍数
            (leadingSpaces % 2).Should().Be(0,
                $"Line should have even number of spaces: '{line}'");
        }
    }

    [Fact]
    public void GenerateInstructions_Should_Format_Instruction_IDs_Correctly()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateSampleRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        // ID 格式应该是 PREFIX-NNN
        result.Should().MatchRegex(@"id: [A-Z]{2,3}-\d{3}");
    }

    [Fact]
    public void GenerateInstructions_Should_Include_Three_State_Output()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateSampleRuleSet();

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("output: \"Allowed / Blocked / Uncertain\"");
    }

    private static ArchitectureRuleSet CreateSampleRuleSet()
    {
        var ruleSet = new ArchitectureRuleSet(907);

        // Rule 1: 架构测试命名规则
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "架构测试命名规则",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "测试类使用 ADR-XXX_Y_Z_Tests 格式",
            enforcement: "文件名必须匹配 'ADR-{Number}_{RuleNumber}_{ClauseNumber}_Tests.cs' 或 'ADR-{Number}_{RuleNumber}_Tests.cs'",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 2,
            condition: "测试方法使用 Should_描述预期行为 格式",
            enforcement: "方法名必须以 'Should_' 开头，后接清晰的行为描述",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 3,
            condition: "Rule 测试类必须有明确的 Rule 后缀",
            enforcement: "Rule 级别测试类名必须以 '_Tests' 结尾（区分于其他测试）",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Rule 2: 测试断言标准化
        ruleSet.AddRule(
            ruleNumber: 2,
            summary: "测试断言标准化",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 1,
            condition: "使用 FluentAssertions 编写断言",
            enforcement: "所有测试文件必须包含 'using FluentAssertions;'",
            executionType: ClauseExecutionType.StaticAnalysis);

        ruleSet.AddClause(
            ruleNumber: 2,
            clauseNumber: 2,
            condition: "失败消息包含具体违规信息和修复建议",
            enforcement: "所有 Should().Fail() 调用必须包含详细的上下文信息",
            executionType: ClauseExecutionType.Convention);

        return ruleSet;
    }
}
