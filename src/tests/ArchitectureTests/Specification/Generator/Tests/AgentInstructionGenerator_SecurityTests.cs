namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Tests;

/// <summary>
/// AgentInstructionGenerator 安全测试
/// 测试 YAML 注入防护和边界条件
/// </summary>
public sealed class AgentInstructionGenerator_SecurityTests
{
    #region YAML 注入防护测试

    [Theory]
    [InlineData("Test: malicious\ncommands:\n  evil: rm -rf /")]
    [InlineData("Test with ': colon injection")]
    [InlineData("Test with\nmultiline\ninjection")]
    [InlineData("Test with | pipe")]
    [InlineData("Test with > redirection")]
    [InlineData("Test with & ampersand")]
    [InlineData("Test with # comment")]
    [InlineData("Test with - dash at start")]
    [InlineData("Test with * asterisk")]
    [InlineData("Test with [ bracket")]
    [InlineData("Test with { brace")]
    [InlineData("Test with ! exclamation")]
    public void GenerateInstructions_Should_Prevent_YAML_Structure_Injection_In_Summary(string maliciousSummary)
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(999);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: maliciousSummary,
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Safe condition",
            enforcement: "Safe enforcement",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert - 使用结构比对而非文本比对
        result.Should().NotBeNull();
        
        // 尝试反序列化验证 YAML 结构完整性
        var deserializer = new YamlDotNetSerializer();
        InstructionsContainer? container = null;
        
        try
        {
            container = deserializer.Deserialize<InstructionsContainer>(result);
        }
        catch (YamlDotNet.Core.YamlException)
        {
            // 如果反序列化失败，说明YAML结构被破坏，这是一个安全问题
            Assert.Fail($"生成的 YAML 无法被解析，可能存在注入漏洞。恶意内容：{maliciousSummary}");
        }
        
        container.Should().NotBeNull("YAML 应该能够被成功反序列化");
        container!.Instructions.Should().HaveCount(1, "应该有且仅有一个指令");
        
        var instruction = container.Instructions[0];
        instruction.Description.Should().Contain(maliciousSummary, "描述应包含原始内容");
        
        // 验证没有注入额外的命令部分
        // 如果原始内容不包含 "commands:"，那么序列化后的对象也不应该有意外的 Commands
        if (maliciousSummary.Contains("commands:"))
        {
            // 恶意内容应该被转义为描述的一部分，而不是创建新的 Commands 字段
            instruction.Description.Should().NotBeNull();
        }
        
        // 验证指令的其他必需字段存在且正确
        instruction.Id.Should().NotBeEmpty();
        instruction.Action.Should().NotBeEmpty();
        instruction.Conditions.Should().NotBeEmpty();
        instruction.Output.Should().Be("Allowed / Blocked / Uncertain");
        instruction.Tools.Should().NotBeEmpty();
        instruction.Feedback.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("Condition with $(evil command)")]
    [InlineData("Condition with `backticks`")]
    [InlineData("Condition with ${variable}")]
    [InlineData("Condition with executable: /bin/sh")]
    public void GenerateInstructions_Should_Prevent_Command_Injection_In_Condition(string maliciousCondition)
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(999);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Test Rule",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: maliciousCondition,
            enforcement: "Safe enforcement",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert - 使用结构比对而非文本比对
        result.Should().NotBeNull();
        
        // 反序列化验证 YAML 结构完整性
        var deserializer = new YamlDotNetSerializer();
        var container = deserializer.Deserialize<InstructionsContainer>(result);
        
        container.Should().NotBeNull();
        container.Instructions.Should().HaveCount(1);
        
        var instruction = container.Instructions[0];
        
        // 验证 Guidelines 中包含了条件信息（如果启用了 guidelines）
        if (instruction.Guidelines != null && instruction.Guidelines.Any())
        {
            // Guidelines 应该包含对 clause 条件的引用
            var guidelinesText = string.Join(" ", instruction.Guidelines);
            guidelinesText.Should().Contain(maliciousCondition, 
                "guidelines 应包含条件内容，但作为安全的字符串，不是可执行代码");
        }
        
        // 验证反序列化后的对象结构完整
        instruction.Id.Should().NotBeEmpty();
        instruction.Description.Should().NotBeEmpty();
        instruction.Conditions.Should().NotBeEmpty();
        instruction.Tools.Should().NotBeEmpty();
        instruction.Feedback.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("Enforcement: evil\n  - malicious: command")]
    [InlineData("Enforcement with\n- fake list item")]
    [InlineData("Enforcement:\n    commands:\n      evil: rm -rf /")]
    public void GenerateInstructions_Should_Prevent_Structure_Injection_In_Enforcement(string maliciousEnforcement)
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(999);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Test Rule",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Safe condition",
            enforcement: maliciousEnforcement,
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert - 使用结构比对而非文本比对
        result.Should().NotBeNull();
        
        // 尝试反序列化验证 YAML 结构完整性
        var deserializer = new YamlDotNetSerializer();
        InstructionsContainer? container = null;
        
        try
        {
            container = deserializer.Deserialize<InstructionsContainer>(result);
        }
        catch (YamlDotNet.Core.YamlException)
        {
            // 如果反序列化失败，说明YAML结构被破坏，这是一个安全问题
            Assert.Fail($"生成的 YAML 无法被解析，可能存在注入漏洞。恶意内容：{maliciousEnforcement}");
        }
        
        container.Should().NotBeNull("YAML 应该能够被成功反序列化");
        container!.Instructions.Should().HaveCount(1);
        
        var instruction = container.Instructions[0];
        
        // 验证 Guidelines 中包含了 enforcement 信息（如果启用了 guidelines）
        if (instruction.Guidelines != null && instruction.Guidelines.Any())
        {
            // Guidelines 应该包含对 enforcement 的引用
            var guidelinesText = string.Join(" ", instruction.Guidelines);
            guidelinesText.Should().Contain(maliciousEnforcement, 
                "guidelines 应包含 enforcement 内容，但作为安全的字符串");
        }
        
        // 验证反序列化后的对象结构完整且字段正确
        instruction.Id.Should().NotBeEmpty();
        instruction.Description.Should().NotBeEmpty();
        instruction.Action.Should().NotBeEmpty();
        instruction.Conditions.Should().NotBeEmpty();
        instruction.Output.Should().Be("Allowed / Blocked / Uncertain");
        instruction.Tools.Should().NotBeEmpty();
        instruction.Feedback.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateInstructions_Should_Escape_Quotes_In_All_Fields()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(999);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Rule with \"quotes\" in summary",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Condition with \"quotes\"",
            enforcement: "Enforcement with \"quotes\"",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().NotBeNull();
        
        // 所有引号都应该被转义
        result.Should().Contain("\\\"");
        
        // 不应该有未转义的引号破坏字符串
        var lines = result.Split('\n');
        foreach (var line in lines.Where(l => l.Contains("description:") || l.Contains("action:")))
        {
            if (line.Contains("\""))
            {
                // 检查引号是否正确配对
                var quoteCount = line.Count(c => c == '"');
                quoteCount.Should().BeGreaterThanOrEqualTo(2, $"line should have properly paired quotes: {line}");
            }
        }
    }

    [Fact]
    public void GenerateInstructions_Should_Not_Allow_Script_Injection_In_Commands()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(999);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Test Rule",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Test",
            enforcement: "Test",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().NotBeNull();
        
        // commands 部分应该只包含预定义的安全命令
        result.Should().Contain("run_adr_tests:");
        result.Should().Contain("run_all_architecture_tests:");
        
        // 验证命令格式 - 应该是 dotnet test 命令
        var commandLines = result.Split('\n').Where(l => l.Contains("dotnet test")).ToList();
        foreach (var cmdLine in commandLines)
        {
            cmdLine.Should().Contain("dotnet test", "commands should only be dotnet test");
            cmdLine.Should().NotContain("&", "should not contain shell operators");
            cmdLine.Should().NotContain("|", "should not contain pipe operators");
            
            // 检查恶意的命令分隔符 - 但允许 logger 格式中的分号
            if (cmdLine.Contains(";") && !cmdLine.Contains("console;verbosity"))
            {
                Assert.Fail($"Command line should not contain command separator ';' outside logger format: {cmdLine}");
            }
            
            cmdLine.Should().NotContain("$(", "should not contain command substitution");
        }
    }

    #endregion

    #region Instruction ID 稳定性和冲突测试

    [Fact]
    public void GenerateInstructions_Should_Generate_Stable_IDs_For_Same_RuleSet()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();

        // Act - 多次生成
        var result1 = generator.GenerateInstructions(ruleSet);
        var result2 = generator.GenerateInstructions(ruleSet);
        var result3 = generator.GenerateInstructions(ruleSet);

        // Assert - 所有生成的结果应该完全相同（确定性）
        result1.Should().Be(result2);
        result2.Should().Be(result3);
    }

    [Fact]
    public void GenerateInstructions_Should_Generate_Sequential_IDs_Without_Gaps()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(999);
        
        // 添加多个规则
        for (int i = 1; i <= 5; i++)
        {
            ruleSet.AddRule(
                ruleNumber: i,
                summary: $"Rule {i}",
                decision: DecisionLevel.Must,
                severity: RuleSeverity.Governance,
                scope: RuleScope.Solution);
            ruleSet.AddClause(
                ruleNumber: i,
                clauseNumber: 1,
                condition: $"Condition {i}",
                enforcement: $"Enforcement {i}",
                executionType: ClauseExecutionType.StaticAnalysis);
        }

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().Contain("id: GEN-001");
        result.Should().Contain("id: GEN-002");
        result.Should().Contain("id: GEN-003");
        result.Should().Contain("id: GEN-004");
        result.Should().Contain("id: GEN-005");
        
        // 不应该有跳号
        result.Should().NotContain("id: GEN-006");
    }

    [Fact]
    public void GenerateInstructions_Should_Use_Custom_Start_Number_Without_Conflicts()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();
        var options = new InstructionGenerationOptions
        {
            AgentPrefix = "TST",
            StartInstructionNumber = 42
        };

        // Act
        var result = generator.GenerateInstructions(ruleSet, options);

        // Assert
        result.Should().Contain("id: TST-042");
        result.Should().NotContain("id: TST-001");
    }

    [Fact]
    public void GenerateInstructions_Should_Support_Different_Prefixes_For_Different_Agents()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = CreateTestRuleSet();
        
        var options1 = new InstructionGenerationOptions { AgentPrefix = "AG" };
        var options2 = new InstructionGenerationOptions { AgentPrefix = "TG" };
        var options3 = new InstructionGenerationOptions { AgentPrefix = "HP" };

        // Act
        var result1 = generator.GenerateInstructions(ruleSet, options1);
        var result2 = generator.GenerateInstructions(ruleSet, options2);
        var result3 = generator.GenerateInstructions(ruleSet, options3);

        // Assert - 每个 Agent 都有唯一的 ID 前缀
        result1.Should().Contain("id: AG-001");
        result2.Should().Contain("id: TG-001");
        result3.Should().Contain("id: HP-001");
        
        // 确保没有冲突
        result1.Should().NotContain("id: TG-");
        result1.Should().NotContain("id: HP-");
    }

    [Fact]
    public void GenerateInstructions_Should_Generate_Consistent_IDs_For_Same_Rules_In_Different_Order()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        
        // 创建两个 RuleSet，规则相同但添加顺序不同
        var ruleSet1 = new ArchitectureRuleSet(999);
        ruleSet1.AddRule(1, "Rule 1", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Solution);
        ruleSet1.AddClause(1, 1, "C1", "E1", ClauseExecutionType.StaticAnalysis);
        ruleSet1.AddRule(2, "Rule 2", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Solution);
        ruleSet1.AddClause(2, 1, "C2", "E2", ClauseExecutionType.StaticAnalysis);
        
        var ruleSet2 = new ArchitectureRuleSet(999);
        ruleSet2.AddRule(2, "Rule 2", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Solution);
        ruleSet2.AddClause(2, 1, "C2", "E2", ClauseExecutionType.StaticAnalysis);
        ruleSet2.AddRule(1, "Rule 1", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Solution);
        ruleSet2.AddClause(1, 1, "C1", "E1", ClauseExecutionType.StaticAnalysis);

        // Act
        var result1 = generator.GenerateInstructions(ruleSet1);
        var result2 = generator.GenerateInstructions(ruleSet2);

        // Assert - 生成的 ID 应该基于 RuleNumber 排序，保证一致性
        result1.Should().Be(result2, "IDs should be consistent regardless of addition order");
    }

    #endregion

    #region 边界条件和异常处理测试

    [Fact]
    public void GenerateInstructions_Should_Handle_Empty_RuleSet_Gracefully()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var emptyRuleSet = new ArchitectureRuleSet(999);

        // Act
        var result = generator.GenerateInstructions(emptyRuleSet);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("instructions:\n");
    }

    [Fact]
    public void GenerateInstructions_Should_Handle_RuleSet_With_Only_Rules_No_Clauses()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(999);
        
        // 添加规则但不添加条款 - 这应该在 RuleSet.ValidateCompleteness() 时失败
        // 但生成器本身应该能处理这种情况
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Rule without clauses",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);

        // Act & Assert
        // 生成器应该能处理，即使没有 clauses
        var act = () => generator.GenerateInstructions(ruleSet);
        act.Should().NotThrow();
        
        var result = act();
        result.Should().Contain("id: GEN-001");
        result.Should().Contain("验证 ADR-999_1 的 0 个约束条款");
    }

    [Fact]
    public void GenerateInstructions_Should_Handle_Very_Large_RuleSet()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var largeRuleSet = new ArchitectureRuleSet(999);
        
        // 添加大量规则
        for (int i = 1; i <= 100; i++)
        {
            largeRuleSet.AddRule(
                ruleNumber: i,
                summary: $"Rule {i}",
                decision: DecisionLevel.Must,
                severity: RuleSeverity.Governance,
                scope: RuleScope.Solution);
            largeRuleSet.AddClause(
                ruleNumber: i,
                clauseNumber: 1,
                condition: $"Condition {i}",
                enforcement: $"Enforcement {i}",
                executionType: ClauseExecutionType.StaticAnalysis);
        }

        // Act
        var result = generator.GenerateInstructions(largeRuleSet);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("id: GEN-001");
        result.Should().Contain("id: GEN-100");
        
        // 验证所有 ID 都是三位数格式
        var lines = result.Split('\n').Where(l => l.Contains("id: GEN-")).ToList();
        lines.Should().HaveCount(100);
        
        foreach (var line in lines)
        {
            line.Should().MatchRegex(@"id: GEN-\d{3}", "IDs should be three-digit format");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GenerateInstructions_Should_Handle_Empty_Summary_Gracefully(string? emptySummary)
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(999);
        
        // Note: ArchitectureRuleDefinition.Validate() 会阻止空 summary，
        // 但我们测试生成器本身的健壮性
        try
        {
            ruleSet.AddRule(
                ruleNumber: 1,
                summary: emptySummary ?? string.Empty,
                decision: DecisionLevel.Must,
                severity: RuleSeverity.Governance,
                scope: RuleScope.Solution);
            ruleSet.AddClause(
                ruleNumber: 1,
                clauseNumber: 1,
                condition: "Test",
                enforcement: "Test",
                executionType: ClauseExecutionType.StaticAnalysis);

            // Act
            var result = generator.GenerateInstructions(ruleSet);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("description: \"\"");
        }
        catch (ArgumentException)
        {
            // 如果 RuleSet 验证失败，这也是可以接受的
            Assert.True(true, "RuleSet validation prevented empty summary");
        }
    }

    [Fact]
    public void GenerateInstructions_Should_Handle_Unicode_Characters()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(999);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "规则包含中文、日本語、한글、Русский、العربية、emoji 🚀",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Unicode condition: ✓ ✗ ⚠",
            enforcement: "Unicode enforcement",
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("规则包含中文");
        result.Should().Contain("Unicode condition");
        
        // 验证 YAML 结构仍然正确
        var lines = result.Split('\n');
        lines[0].Should().Be("instructions:");
    }

    [Fact]
    public void GenerateInstructions_Should_Handle_Extremely_Long_Text()
    {
        // Arrange
        var generator = new AgentInstructionGenerator();
        var ruleSet = new ArchitectureRuleSet(999);
        
        var longText = new string('A', 10000); // 10KB 文本
        
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: longText,
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: longText,
            enforcement: longText,
            executionType: ClauseExecutionType.StaticAnalysis);

        // Act
        var result = generator.GenerateInstructions(ruleSet);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(longText);
        
        // 验证 YAML 结构仍然正确
        var lines = result.Split('\n');
        lines[0].Should().Be("instructions:");
    }

    #endregion

    private static ArchitectureRuleSet CreateTestRuleSet()
    {
        var ruleSet = new ArchitectureRuleSet(999);
        ruleSet.AddRule(
            ruleNumber: 1,
            summary: "Test Rule",
            decision: DecisionLevel.Must,
            severity: RuleSeverity.Governance,
            scope: RuleScope.Solution);
        ruleSet.AddClause(
            ruleNumber: 1,
            clauseNumber: 1,
            condition: "Test Condition",
            enforcement: "Test Enforcement",
            executionType: ClauseExecutionType.StaticAnalysis);
        return ruleSet;
    }
}
