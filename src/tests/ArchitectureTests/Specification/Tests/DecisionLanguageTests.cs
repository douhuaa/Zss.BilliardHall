namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

using Zss.BilliardHall.Tests.ArchitectureTests.Specification.DecisionLanguage;

/// <summary>
/// DecisionLanguage 功能测试
/// 验证裁决语言模型的解析和判定行为
/// </summary>
public sealed class DecisionLanguageTests
{
    /// <summary>
    /// 测试 Parse 方法能正确识别 Must 级别的裁决语言
    /// </summary>
    [Theory(DisplayName = "Parse 应正确识别 Must 级别裁决语言")]
    [InlineData("必须遵循模块边界")]
    [InlineData("Handler 强制使用统一接口")]
    [InlineData("需要实现 IHandler 接口")]
    [InlineData("这是一个必须要做的事情")]
    public void Parse_Should_Recognize_Must_Level_Decisions(string sentence)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        // Assert
        result.Should().NotBeNull();
        result.Level.Should().Be(DecisionLevel.Must);
        result.IsBlocking.Should().BeTrue();
        result.IsDecision.Should().BeTrue();
    }

    /// <summary>
    /// 测试 Parse 方法能正确识别 MustNot 级别的裁决语言
    /// </summary>
    [Theory(DisplayName = "Parse 应正确识别 MustNot 级别裁决语言")]
    [InlineData("禁止跨模块直接调用")]
    [InlineData("Endpoint 不得包含业务逻辑")]
    [InlineData("不允许使用反射创建对象")]
    [InlineData("禁止在这里做这件事")]
    public void Parse_Should_Recognize_MustNot_Level_Decisions(string sentence)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        // Assert
        result.Should().NotBeNull();
        result.Level.Should().Be(DecisionLevel.MustNot);
        result.IsBlocking.Should().BeTrue();
        result.IsDecision.Should().BeTrue();
    }

    /// <summary>
    /// 测试 Parse 方法能正确识别 Should 级别的裁决语言
    /// </summary>
    [Theory(DisplayName = "Parse 应正确识别 Should 级别裁决语言")]
    [InlineData("应该优先使用 Handler 模式")]
    [InlineData("建议使用依赖注入")]
    [InlineData("推荐遵循 CQRS 原则")]
    [InlineData("这里应该考虑性能")]
    public void Parse_Should_Recognize_Should_Level_Decisions(string sentence)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        // Assert
        result.Should().NotBeNull();
        result.Level.Should().Be(DecisionLevel.Should);
        result.IsBlocking.Should().BeFalse();
        result.IsDecision.Should().BeTrue();
    }

    /// <summary>
    /// 测试 Parse 方法对不包含裁决语言的文本返回 None
    /// </summary>
    [Theory(DisplayName = "Parse 对不包含裁决语言的文本应返回 None")]
    [InlineData("这是一个描述性文本")]
    [InlineData("Handler 模式的示例")]
    [InlineData("如何使用依赖注入")]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_Should_Return_None_For_Non_Decision_Text(string sentence)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        // Assert
        result.Should().Be(DecisionResult.None);
        result.IsDecision.Should().BeFalse();
        result.IsBlocking.Should().BeFalse();
        result.Level.Should().BeNull(); // None 的 Level 为 null
    }

    /// <summary>
    /// 测试 Parse 方法对 null 或空字符串的处理
    /// </summary>
    [Theory(DisplayName = "Parse 应正确处理 null 或空字符串")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_Should_Handle_Null_Or_Empty_Gracefully(string? sentence)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence!);

        // Assert
        result.Should().Be(DecisionResult.None);
    }

    /// <summary>
    /// 测试 IsBlockingDecision 方法
    /// </summary>
    [Theory(DisplayName = "IsBlockingDecision 应正确判断阻断级别")]
    [InlineData("必须遵循规则", true)]
    [InlineData("禁止这样做", true)]
    [InlineData("应该考虑", false)]
    [InlineData("这是描述", false)]
    public void IsBlockingDecision_Should_Correctly_Identify_Blocking_Decisions(
        string sentence,
        bool expectedIsBlocking)
    {
        // Act
        var isBlocking = ArchitectureTestSpecification.DecisionLanguage.IsBlockingDecision(sentence);

        // Assert
        isBlocking.Should().Be(expectedIsBlocking);
    }

    /// <summary>
    /// 测试 HasDecisionLanguage 方法
    /// </summary>
    [Theory(DisplayName = "HasDecisionLanguage 应正确判断是否包含裁决语言")]
    [InlineData("必须遵循规则", true)]
    [InlineData("禁止这样做", true)]
    [InlineData("应该考虑", true)]
    [InlineData("建议使用", true)]
    [InlineData("这是描述", false)]
    [InlineData("示例代码", false)]
    public void HasDecisionLanguage_Should_Correctly_Identify_Decision_Language(
        string sentence,
        bool expectedHasDecision)
    {
        // Act
        var hasDecision = ArchitectureTestSpecification.DecisionLanguage.HasDecisionLanguage(sentence);

        // Assert
        hasDecision.Should().Be(expectedHasDecision);
    }

    /// <summary>
    /// 测试规则优先级：当文本包含多个关键词时，应匹配第一个
    /// </summary>
    [Fact(DisplayName = "Parse 应按规则顺序优先匹配")]
    public void Parse_Should_Match_First_Rule_When_Multiple_Keywords_Present()
    {
        // Arrange - 同时包含 "必须" 和 "应该"
        var sentence = "必须遵循规范，应该考虑性能";

        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        // Assert - 应该匹配 Must（因为 Must 规则在前）
        result.Level.Should().Be(DecisionLevel.Must);
        result.IsBlocking.Should().BeTrue();
    }

    /// <summary>
    /// 测试 DecisionResult 的相等性比较
    /// </summary>
    [Fact(DisplayName = "DecisionResult 应支持值相等性比较")]
    public void DecisionResult_Should_Support_Value_Equality()
    {
        // Arrange
        var result1 = new DecisionResult(DecisionLevel.Must, IsBlocking: true);
        var result2 = new DecisionResult(DecisionLevel.Must, IsBlocking: true);
        var result3 = new DecisionResult(DecisionLevel.Should, IsBlocking: false);

        // Assert
        result1.Should().Be(result2);
        result1.Should().NotBe(result3);
        result1.Should().NotBe(DecisionResult.None);
    }

    /// <summary>
    /// 测试 DecisionRule 的配置正确性
    /// </summary>
    [Fact(DisplayName = "DecisionLanguage.Rules 应包含所有三个级别的规则")]
    public void DecisionLanguage_Rules_Should_Contain_All_Three_Levels()
    {
        // Arrange
        var rules = ArchitectureTestSpecification.DecisionLanguage.Rules;

        // Assert
        rules.Should().HaveCount(3);
        rules.Should().Contain(r => r.Level == DecisionLevel.Must && r.IsBlocking);
        rules.Should().Contain(r => r.Level == DecisionLevel.MustNot && r.IsBlocking);
        rules.Should().Contain(r => r.Level == DecisionLevel.Should && !r.IsBlocking);
    }

    /// <summary>
    /// 测试 Must 规则包含正确的关键词
    /// </summary>
    [Fact(DisplayName = "Must 规则应包含正确的关键词")]
    public void Must_Rule_Should_Contain_Correct_Keywords()
    {
        // Arrange
        var mustRule = ArchitectureTestSpecification.DecisionLanguage.Rules
            .First(r => r.Level == DecisionLevel.Must);

        // Assert
        mustRule.Keywords.Should().Contain("必须");
        mustRule.Keywords.Should().Contain("强制");
        mustRule.Keywords.Should().Contain("需要");
        mustRule.IsBlocking.Should().BeTrue();
    }

    /// <summary>
    /// 测试 MustNot 规则包含正确的关键词
    /// </summary>
    [Fact(DisplayName = "MustNot 规则应包含正确的关键词")]
    public void MustNot_Rule_Should_Contain_Correct_Keywords()
    {
        // Arrange
        var mustNotRule = ArchitectureTestSpecification.DecisionLanguage.Rules
            .First(r => r.Level == DecisionLevel.MustNot);

        // Assert
        mustNotRule.Keywords.Should().Contain("禁止");
        mustNotRule.Keywords.Should().Contain("不得");
        mustNotRule.Keywords.Should().Contain("不允许");
        mustNotRule.IsBlocking.Should().BeTrue();
    }

    /// <summary>
    /// 测试 Should 规则包含正确的关键词
    /// </summary>
    [Fact(DisplayName = "Should 规则应包含正确的关键词")]
    public void Should_Rule_Should_Contain_Correct_Keywords()
    {
        // Arrange
        var shouldRule = ArchitectureTestSpecification.DecisionLanguage.Rules
            .First(r => r.Level == DecisionLevel.Should);

        // Assert
        shouldRule.Keywords.Should().Contain("应该");
        shouldRule.Keywords.Should().Contain("建议");
        shouldRule.Keywords.Should().Contain("推荐");
        shouldRule.IsBlocking.Should().BeFalse();
    }

    /// <summary>
    /// 测试否定上下文的识别 - "不应该"、"应该避免" 等不应被识别为裁决语言
    /// </summary>
    [Theory(DisplayName = "Parse 应正确识别并排除否定上下文")]
    [InlineData("不应该使用反射")]
    [InlineData("应该避免使用反射")]
    [InlineData("应该不要直接访问数据库")]
    [InlineData("不必须实现该接口")]
    [InlineData("不需要额外配置")]
    public void Parse_Should_Exclude_Negative_Context(string sentence)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        // Assert - 否定上下文不应识别为裁决语言
        result.Should().Be(DecisionResult.None,
            $"否定上下文 '{sentence}' 不应被识别为裁决语言");
    }

    /// <summary>
    /// 测试词边界识别 - "需要性" 中的 "需要" 不应被匹配
    /// </summary>
    [Theory(DisplayName = "Parse 应正确处理词边界，避免复合词误判")]
    [InlineData("需要性分析")]
    [InlineData("可需要性")]
    [InlineData("禁止者")]
    public void Parse_Should_Respect_Word_Boundaries(string sentence)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        // Assert - 复合词中的关键词不应被匹配
        result.Should().Be(DecisionResult.None,
            $"复合词 '{sentence}' 中的关键词不应被匹配");
    }

    /// <summary>
    /// 测试正向裁决语言在否定上下文测试后仍能正确识别
    /// </summary>
    [Theory(DisplayName = "Parse 应正确识别正向裁决语言（非否定上下文）")]
    [InlineData("应该使用依赖注入", DecisionLevel.Should)]
    [InlineData("必须实现接口", DecisionLevel.Must)]
    [InlineData("需要遵循规范", DecisionLevel.Must)]
    [InlineData("禁止直接调用", DecisionLevel.MustNot)]
    public void Parse_Should_Still_Recognize_Positive_Decisions(
        string sentence,
        DecisionLevel expectedLevel)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        // Assert
        result.Level.Should().Be(expectedLevel,
            $"正向裁决语言 '{sentence}' 应被正确识别");
        result.IsDecision.Should().BeTrue();
    }

    /// <summary>
    /// 测试复杂场景：同一句子中同时包含正向和否定上下文
    /// </summary>
    [Fact(DisplayName = "Parse 应处理同一句子中的多个关键词出现")]
    public void Parse_Should_Handle_Multiple_Keyword_Occurrences()
    {
        // Arrange - "应该" 出现两次，第一次是否定上下文，第二次是正向
        var sentence = "不应该使用反射，但应该使用依赖注入";

        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        // Assert - 应该识别出第二个"应该"
        result.Level.Should().Be(DecisionLevel.Should,
            "应该识别句子中非否定上下文的关键词");
        result.IsDecision.Should().BeTrue();
    }

    /// <summary>
    /// 测试边界情况：关键词在句子开头
    /// </summary>
    [Theory(DisplayName = "Parse 应正确处理关键词在句子开头的情况")]
    [InlineData("应该优先考虑性能", DecisionLevel.Should)]
    [InlineData("必须遵循规范", DecisionLevel.Must)]
    [InlineData("禁止直接访问", DecisionLevel.MustNot)]
    public void Parse_Should_Handle_Keywords_At_Start(
        string sentence,
        DecisionLevel expectedLevel)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        // Assert
        result.Level.Should().Be(expectedLevel);
    }

    /// <summary>
    /// 测试边界情况：关键词在句子末尾
    /// </summary>
    [Theory(DisplayName = "Parse 应正确处理关键词在句子末尾的情况")]
    [InlineData("使用依赖注入是必须", DecisionLevel.Must)]
    [InlineData("这是禁止", DecisionLevel.MustNot)]
    public void Parse_Should_Handle_Keywords_At_End(
        string sentence,
        DecisionLevel expectedLevel)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        // Assert
        result.Level.Should().Be(expectedLevel);
    }

    /// <summary>
    /// 测试"应该避免"这类典型的误判场景
    /// </summary>
    [Theory(DisplayName = "Parse 应正确处理'应该避免'等典型误判场景")]
    [InlineData("应该避免使用全局变量")]
    [InlineData("建议避免过度使用继承")]
    [InlineData("推荐避免循环依赖")]
    public void Parse_Should_Handle_ShouldAvoid_Pattern(string sentence)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        // Assert - "应该避免" 应该被识别为否定上下文
        result.Should().Be(DecisionResult.None,
            $"'{sentence}' 中的'应该避免'应被识别为否定上下文");
    }

    #region 新API测试（DecisionParseResult & DecisionExecutionResult）

    /// <summary>
    /// 测试新的 ParseToDecision 方法能正确识别 Must 级别
    /// </summary>
    [Theory(DisplayName = "ParseToDecision 应正确识别 Must 级别裁决语言")]
    [InlineData("必须遵循模块边界")]
    [InlineData("Handler 强制使用统一接口")]
    [InlineData("需要实现 IHandler 接口")]
    public void ParseToDecision_Should_Recognize_Must_Level(string sentence)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.ParseToDecision(sentence);

        // Assert
        result.Should().NotBeNull();
        result.Level.Should().Be(DecisionLevel.Must);
        result.HasDecision.Should().BeTrue();
    }

    /// <summary>
    /// 测试新的 ParseToDecision 方法能正确识别 MustNot 级别
    /// </summary>
    [Theory(DisplayName = "ParseToDecision 应正确识别 MustNot 级别裁决语言")]
    [InlineData("禁止跨模块直接调用")]
    [InlineData("Endpoint 不得包含业务逻辑")]
    [InlineData("不允许使用反射创建对象")]
    public void ParseToDecision_Should_Recognize_MustNot_Level(string sentence)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.ParseToDecision(sentence);

        // Assert
        result.Should().NotBeNull();
        result.Level.Should().Be(DecisionLevel.MustNot);
        result.HasDecision.Should().BeTrue();
    }

    /// <summary>
    /// 测试新的 ParseToDecision 方法能正确识别 Should 级别
    /// </summary>
    [Theory(DisplayName = "ParseToDecision 应正确识别 Should 级别裁决语言")]
    [InlineData("应该优先使用 Handler 模式")]
    [InlineData("建议使用依赖注入")]
    [InlineData("推荐遵循 CQRS 原则")]
    public void ParseToDecision_Should_Recognize_Should_Level(string sentence)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.ParseToDecision(sentence);

        // Assert
        result.Should().NotBeNull();
        result.Level.Should().Be(DecisionLevel.Should);
        result.HasDecision.Should().BeTrue();
    }

    /// <summary>
    /// 测试 ParseToDecision 对不包含裁决语言的文本返回 None
    /// </summary>
    [Theory(DisplayName = "ParseToDecision 对不包含裁决语言的文本应返回 None")]
    [InlineData("这是一个描述性文本")]
    [InlineData("Handler 模式的示例")]
    [InlineData("")]
    public void ParseToDecision_Should_Return_None_For_Non_Decision_Text(string sentence)
    {
        // Act
        var result = ArchitectureTestSpecification.DecisionLanguage.ParseToDecision(sentence);

        // Assert
        result.Should().Be(DecisionParseResult.None);
        result.HasDecision.Should().BeFalse();
        result.Level.Should().BeNull();
    }

    /// <summary>
    /// 测试 ToExecutionResult 方法将解析结果正确转换为执行结果
    /// </summary>
    [Theory(DisplayName = "ToExecutionResult 应正确转换解析结果为执行结果")]
    [InlineData(DecisionLevel.Must, true)]
    [InlineData(DecisionLevel.MustNot, true)]
    [InlineData(DecisionLevel.Should, false)]
    public void ToExecutionResult_Should_Convert_ParseResult_Correctly(
        DecisionLevel level,
        bool expectedIsBlocking)
    {
        // Arrange
        var parseResult = new DecisionParseResult(level);

        // Act
        var executionResult = ArchitectureTestSpecification.DecisionLanguage.ToExecutionResult(parseResult);

        // Assert
        executionResult.Should().NotBeNull();
        executionResult!.Level.Should().Be(level);
        executionResult.IsBlocking.Should().Be(expectedIsBlocking);
    }

    /// <summary>
    /// 测试 ToExecutionResult 对 None 解析结果返回 null
    /// </summary>
    [Fact(DisplayName = "ToExecutionResult 对 None 解析结果应返回 null")]
    public void ToExecutionResult_Should_Return_Null_For_None()
    {
        // Act
        var executionResult = ArchitectureTestSpecification.DecisionLanguage.ToExecutionResult(DecisionParseResult.None);

        // Assert
        executionResult.Should().BeNull();
    }

    /// <summary>
    /// 测试完整工作流：解析 → 转换为执行结果
    /// </summary>
    [Fact(DisplayName = "完整工作流：ParseToDecision → ToExecutionResult 应正确工作")]
    public void Full_Workflow_ParseToDecision_ToExecutionResult_Should_Work()
    {
        // Arrange
        var sentence = "必须遵循模块边界";

        // Act
        var parseResult = ArchitectureTestSpecification.DecisionLanguage.ParseToDecision(sentence);
        var executionResult = ArchitectureTestSpecification.DecisionLanguage.ToExecutionResult(parseResult);

        // Assert
        parseResult.Level.Should().Be(DecisionLevel.Must);
        parseResult.HasDecision.Should().BeTrue();

        executionResult.Should().NotBeNull();
        executionResult!.Level.Should().Be(DecisionLevel.Must);
        executionResult.IsBlocking.Should().BeTrue();
    }

    /// <summary>
    /// 测试 DecisionParseResult 的相等性比较
    /// </summary>
    [Fact(DisplayName = "DecisionParseResult 应支持值相等性比较")]
    public void DecisionParseResult_Should_Support_Value_Equality()
    {
        // Arrange
        var result1 = new DecisionParseResult(DecisionLevel.Must);
        var result2 = new DecisionParseResult(DecisionLevel.Must);
        var result3 = new DecisionParseResult(DecisionLevel.Should);

        // Assert
        result1.Should().Be(result2);
        result1.Should().NotBe(result3);
        result1.Should().NotBe(DecisionParseResult.None);
    }

    /// <summary>
    /// 测试 DecisionExecutionResult 的相等性比较
    /// </summary>
    [Fact(DisplayName = "DecisionExecutionResult 应支持值相等性比较")]
    public void DecisionExecutionResult_Should_Support_Value_Equality()
    {
        // Arrange
        var result1 = new DecisionExecutionResult(DecisionLevel.Must, IsBlocking: true);
        var result2 = new DecisionExecutionResult(DecisionLevel.Must, IsBlocking: true);
        var result3 = new DecisionExecutionResult(DecisionLevel.Should, IsBlocking: false);

        // Assert
        result1.Should().Be(result2);
        result1.Should().NotBe(result3);
    }

    #endregion
}
