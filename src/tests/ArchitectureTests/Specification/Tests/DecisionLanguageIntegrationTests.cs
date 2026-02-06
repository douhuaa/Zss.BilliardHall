namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

using Zss.BilliardHall.Tests.ArchitectureTests.Specification.DecisionLanguage;

/// <summary>
/// DecisionLanguage 集成测试
/// 演示如何在实际的架构测试场景中使用 DecisionLanguage
/// </summary>
public sealed class DecisionLanguageIntegrationTests
{
    /// <summary>
    /// 示例：验证 ADR 文档中的裁决语言分类
    /// </summary>
    [Fact(DisplayName = "应该能够正确分类 ADR 文档中的裁决规则")]
    public void Should_Correctly_Classify_ADR_Decision_Rules()
    {
        // Arrange - 模拟 ADR 文档中的规则
        var adrRules = new Dictionary<string, (DecisionLevel? ExpectedLevel, bool ExpectedBlocking)>
        {
            ["必须使用 Handler 模式处理命令"] = (DecisionLevel.Must, true),
            ["禁止在 Endpoint 中编写业务逻辑"] = (DecisionLevel.MustNot, true),
            ["应该考虑使用 CQRS 分离读写"] = (DecisionLevel.Should, false),
            ["推荐使用依赖注入管理生命周期"] = (DecisionLevel.Should, false),
            ["Handler 模式的实现示例"] = (null, false), // 描述性文本，无裁决语言
        };

        // Act & Assert
        foreach (var (rule, (expectedLevel, expectedBlocking)) in adrRules)
        {
            var result = ArchitectureTestSpecification.DecisionLanguage.Parse(rule);

            result.Level.Should().Be(expectedLevel, $"规则 '{rule}' 的级别应为 {expectedLevel}");
            result.IsBlocking.Should().Be(expectedBlocking, $"规则 '{rule}' 的阻断状态应为 {expectedBlocking}");
        }
    }

    /// <summary>
    /// 示例：模拟架构测试中的三态输出场景
    /// </summary>
    [Theory(DisplayName = "应该根据裁决语言级别产生相应的测试行为")]
    [InlineData("必须实现 IHandler 接口", "BLOCKED")]
    [InlineData("禁止使用反射创建实例", "BLOCKED")]
    [InlineData("应该优先使用构造函数注入", "WARNING")]
    [InlineData("推荐使用只读属性", "WARNING")]
    [InlineData("Handler 模式示例", "ALLOWED")]
    public void Should_Produce_Appropriate_Test_Behavior_Based_On_Decision_Level(
        string rule,
        string expectedBehavior)
    {
        // Act
        var decision = ArchitectureTestSpecification.DecisionLanguage.Parse(rule);
        string actualBehavior;

        if (decision.IsBlocking)
        {
            actualBehavior = "BLOCKED";
        }
        else if (decision.IsDecision)
        {
            actualBehavior = "WARNING";
        }
        else
        {
            actualBehavior = "ALLOWED";
        }

        // Assert
        actualBehavior.Should().Be(expectedBehavior, $"规则 '{rule}' 应产生 {expectedBehavior} 行为");
    }

    /// <summary>
    /// 示例：批量验证文档中不应包含的裁决语言
    /// </summary>
    [Fact(DisplayName = "非裁决性文档不应包含阻断级别的裁决语言")]
    public void Non_Decision_Documents_Should_Not_Contain_Blocking_Decision_Language()
    {
        // Arrange - 模拟 Guide 或 FAQ 文档中的内容
        var guideContent = new[]
        {
            "如何使用 Handler 模式",
            "推荐的项目结构",
            "常见问题解答",
            "应该遵循的最佳实践",
        };

        // Act & Assert
        foreach (var content in guideContent)
        {
            var hasBlockingDecision = ArchitectureTestSpecification.DecisionLanguage
                .IsBlockingDecision(content);

            hasBlockingDecision.Should().BeFalse(
                $"Guide 文档内容 '{content}' 不应包含阻断级别的裁决语言");
        }
    }

    /// <summary>
    /// 示例：统计 ADR 文档的裁决规则分布
    /// </summary>
    [Fact(DisplayName = "应该能够统计 ADR 文档的裁决规则分布")]
    public void Should_Be_Able_To_Analyze_Decision_Rule_Distribution()
    {
        // Arrange - 模拟一个 ADR 文档的规则集
        var adrRules = new[]
        {
            "必须使用 Handler 模式",
            "必须实现 IHandler 接口",
            "禁止直接访问数据库",
            "禁止在 Endpoint 中写业务逻辑",
            "应该使用依赖注入",
            "应该考虑性能影响",
            "推荐使用异步编程",
        };

        // Act
        var mustCount = 0;
        var mustNotCount = 0;
        var shouldCount = 0;
        var noneCount = 0;

        foreach (var rule in adrRules)
        {
            var decision = ArchitectureTestSpecification.DecisionLanguage.Parse(rule);

            switch (decision.Level)
            {
                case DecisionLevel.Must:
                    mustCount++;
                    break;
                case DecisionLevel.MustNot:
                    mustNotCount++;
                    break;
                case DecisionLevel.Should:
                    shouldCount++;
                    break;
                case null:
                    noneCount++;
                    break;
            }
        }

        // Assert
        mustCount.Should().Be(2, "应有 2 条 Must 规则");
        mustNotCount.Should().Be(2, "应有 2 条 MustNot 规则");
        shouldCount.Should().Be(3, "应有 3 条 Should 规则");
        noneCount.Should().Be(0, "不应有 None 规则");

        var blockingCount = mustCount + mustNotCount;
        var totalCount = adrRules.Length;
        var blockingPercentage = (double)blockingCount / totalCount * 100;

        blockingPercentage.Should().BeApproximately(57.14, 0.01,
            "阻断级别规则应占约 57%");
    }

    /// <summary>
    /// 示例：验证裁决语言的优先级规则
    /// </summary>
    [Fact(DisplayName = "当文本包含多个裁决关键词时应匹配第一个")]
    public void When_Text_Contains_Multiple_Keywords_Should_Match_First_Rule()
    {
        // Arrange - 同时包含 Must 和 Should 关键词
        var mixedText = "必须实现接口，同时应该考虑性能";

        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(mixedText);

        // Assert - 应该匹配 Must（因为 Must 规则在前）
        result.Level.Should().Be(DecisionLevel.Must,
            "当同时包含多个关键词时，应匹配规则集中的第一个");
        result.IsBlocking.Should().BeTrue();
    }

    /// <summary>
    /// 示例：验证空文本和纯描述性文本的处理
    /// </summary>
    [Theory(DisplayName = "空文本或纯描述性文本应返回 None")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("这是一个描述性的文本")]
    [InlineData("Handler 模式的工作原理")]
    public void Empty_Or_Descriptive_Text_Should_Return_None(string? text)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(text!);

        // Assert
        result.Should().Be(DecisionResult.None);
        result.IsDecision.Should().BeFalse();
        result.Level.Should().BeNull();
    }
}
