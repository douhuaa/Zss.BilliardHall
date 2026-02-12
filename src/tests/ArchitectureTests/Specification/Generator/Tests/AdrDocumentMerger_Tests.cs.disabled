namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Tests;

/// <summary>
/// AdrDocumentMerger 的单元测试
/// 测试文档合并功能，包括 Front Matter 保留、章节顺序等
/// </summary>
public sealed class AdrDocumentMerger_Tests
{
    private readonly IAdrDecisionGenerator _generator = new AdrDecisionGenerator();
    private readonly IAdrDocumentMerger _merger;

    public AdrDocumentMerger_Tests()
    {
        _merger = new AdrDocumentMerger(_generator);
    }

    [Fact]
    public void MergeDecisionSection_WithFrontMatter_PreservesFrontMatter()
    {
        // Arrange
        var existingAdr = @"---
adr: ADR-001
type: 架构决策记录
status: Final
level: 架构约束
---

## Decision（裁决）

旧的 Decision 内容

## Context（上下文）

这是上下文内容
";
        var ruleSet = CreateSimpleRuleSet();

        // Act
        var result = _merger.MergeDecisionSection(existingAdr, ruleSet);

        // Assert
        result.Should().StartWith("---\n");
        result.Should().Contain("adr: ADR-001");
        result.Should().Contain("type: 架构决策记录");
        result.Should().Contain("status: Final");
        result.Should().Contain("level: 架构约束");
        result.Should().NotContain("旧的 Decision 内容");
        result.Should().Contain("## Context（上下文）");
    }

    [Fact]
    public void MergeDecisionSection_WithoutDecision_InsertsNewDecision()
    {
        // Arrange
        var existingAdr = @"---
adr: ADR-001
---

## Focus（聚焦）

聚焦内容

## Context（上下文）

上下文内容
";
        var ruleSet = CreateSimpleRuleSet();

        // Act
        var result = _merger.MergeDecisionSection(existingAdr, ruleSet);

        // Assert
        result.Should().Contain("## Decision（裁决）");
        result.Should().Contain("## Focus（聚焦）");
        result.Should().Contain("## Context（上下文）");
        
        // 验证章节顺序：Focus -> Decision -> Context
        var focusIndex = result.IndexOf("## Focus");
        var decisionIndex = result.IndexOf("## Decision");
        var contextIndex = result.IndexOf("## Context");
        
        focusIndex.Should().BeLessThan(decisionIndex);
        decisionIndex.Should().BeLessThan(contextIndex);
    }

    [Fact]
    public void MergeDecisionSection_WithExistingDecision_ReplacesDecision()
    {
        // Arrange
        var existingAdr = @"## Decision（裁决）

### 旧规则

旧内容

## Context（上下文）

上下文内容
";
        var ruleSet = CreateSimpleRuleSet();

        // Act
        var result = _merger.MergeDecisionSection(existingAdr, ruleSet);

        // Assert
        result.Should().Contain("## Decision（裁决）");
        result.Should().NotContain("旧规则");
        result.Should().NotContain("旧内容");
        result.Should().Contain("## Context（上下文）");
    }

    [Fact]
    public void MergeDecisionSection_PreservesConsequences()
    {
        // Arrange
        var existingAdr = @"## Decision（裁决）

旧 Decision

## Consequences（影响与后果）

这是影响内容
这是后果内容

## References（参考文献）

参考文献内容
";
        var ruleSet = CreateSimpleRuleSet();

        // Act
        var result = _merger.MergeDecisionSection(existingAdr, ruleSet);

        // Assert
        result.Should().Contain("## Consequences（影响与后果）");
        result.Should().Contain("这是影响内容");
        result.Should().Contain("这是后果内容");
        result.Should().Contain("## References（参考文献）");
    }

    [Fact]
    public void MergeDecisionSection_MaintainsCorrectSectionOrder()
    {
        // Arrange
        var existingAdr = @"---
adr: ADR-001
---

## Focus（聚焦）

聚焦内容

## Glossary（术语）

术语内容

## Decision（裁决）

旧 Decision

## Context（上下文）

上下文内容

## Consequences（影响与后果）

影响内容

## References（参考文献）

参考内容
";
        var ruleSet = CreateSimpleRuleSet();

        // Act
        var result = _merger.MergeDecisionSection(existingAdr, ruleSet);

        // Assert - 验证章节顺序
        var focusIndex = result.IndexOf("## Focus");
        var glossaryIndex = result.IndexOf("## Glossary");
        var decisionIndex = result.IndexOf("## Decision");
        var contextIndex = result.IndexOf("## Context");
        var consequencesIndex = result.IndexOf("## Consequences");
        var referencesIndex = result.IndexOf("## References");

        focusIndex.Should().BeLessThan(glossaryIndex);
        glossaryIndex.Should().BeLessThan(decisionIndex);
        decisionIndex.Should().BeLessThan(contextIndex);
        contextIndex.Should().BeLessThan(consequencesIndex);
        consequencesIndex.Should().BeLessThan(referencesIndex);
    }

    [Fact]
    public void MergeDecisionSection_WithoutFrontMatter_WorksCorrectly()
    {
        // Arrange
        var existingAdr = @"## Decision（裁决）

旧 Decision

## Context（上下文）

上下文内容
";
        var ruleSet = CreateSimpleRuleSet();

        // Act
        var result = _merger.MergeDecisionSection(existingAdr, ruleSet);

        // Assert
        result.Should().NotStartWith("---");
        result.Should().Contain("## Decision（裁决）");
        result.Should().Contain("## Context（上下文）");
    }

    [Fact]
    public void MergeDecisionSection_WithCustomOptions_AppliesOptions()
    {
        // Arrange
        var existingAdr = @"## Decision（裁决）

旧 Decision

## Context（上下文）

上下文内容
";
        var ruleSet = CreateSimpleRuleSet();
        var options = new DecisionGenerationOptions
        {
            IncludeSectionHeader = true,
            IncludeWarningNote = false,
            HeaderLevelOffset = 0
        };

        // Act
        var result = _merger.MergeDecisionSection(existingAdr, ruleSet, options);

        // Assert
        result.Should().Contain("## Decision（裁决）");
        result.Should().NotContain("⚠️");
        result.Should().Contain("## Context（上下文）");
    }

    [Fact]
    public void MergeDecisionSection_WithStringDecision_WorksCorrectly()
    {
        // Arrange
        var existingAdr = @"## Decision（裁决）

旧 Decision

## Context（上下文）

上下文内容
";
        var newDecision = @"## Decision（裁决）

新的 Decision 内容
";

        // Act
        var result = _merger.MergeDecisionSection(existingAdr, newDecision);

        // Assert
        result.Should().Contain("新的 Decision 内容");
        result.Should().NotContain("旧 Decision");
        result.Should().Contain("## Context（上下文）");
    }

    [Fact]
    public void MergeDecisionSection_WithNullContent_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullContent = null;
        var ruleSet = CreateSimpleRuleSet();

        // Act & Assert
        var act = () => _merger.MergeDecisionSection(nullContent!, ruleSet);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MergeDecisionSection_WithNullRuleSet_ThrowsArgumentNullException()
    {
        // Arrange
        var existingAdr = "## Decision（裁决）\n\n旧内容";
        ArchitectureRuleSet? nullRuleSet = null;

        // Act & Assert
        var act = () => _merger.MergeDecisionSection(existingAdr, nullRuleSet!);
        act.Should().Throw<ArgumentNullException>();
    }

    private static ArchitectureRuleSet CreateSimpleRuleSet()
    {
        var ruleSet = new ArchitectureRuleSet(1);
        ruleSet.AddRule(1, "测试规则", DecisionLevel.Must, RuleSeverity.Governance, RuleScope.Test);
        ruleSet.AddClause(1, 1, "测试条件", "测试执行说明", ClauseExecutionType.StaticAnalysis);
        return ruleSet;
    }
}
