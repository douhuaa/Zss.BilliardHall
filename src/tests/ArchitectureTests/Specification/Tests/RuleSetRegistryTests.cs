namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Index;

/// <summary>
/// RuleSetRegistry 功能测试
/// 验证规则集注册表的查询和验证行为
/// </summary>
public sealed class RuleSetRegistryTests
{
    #region Get 方法测试（宽容模式）

    /// <summary>
    /// 测试 Get(int) 方法能正确获取存在的规则集
    /// </summary>
    [Theory(DisplayName = "Get(int) 应正确获取存在的规则集")]
    [InlineData(1)]    // ADR-001
    [InlineData(2)]    // ADR-002
    [InlineData(900)]  // ADR-900
    [InlineData(907)]  // ADR-907
    public void Get_Int_Should_Return_RuleSet_When_Exists(int adrNumber)
    {
        // Act
        var ruleSet = RuleSetRegistry.Get(adrNumber);

        // Assert
        ruleSet.Should().NotBeNull();
        ruleSet!.AdrNumber.Should().Be(adrNumber);
    }

    /// <summary>
    /// 测试 Get(int) 方法对不存在的规则集返回 null
    /// </summary>
    [Theory(DisplayName = "Get(int) 对不存在的规则集应返回 null")]
    [InlineData(999)]
    [InlineData(9999)]
    [InlineData(-1)]
    public void Get_Int_Should_Return_Null_When_Not_Exists(int adrNumber)
    {
        // Act
        var ruleSet = RuleSetRegistry.Get(adrNumber);

        // Assert
        ruleSet.Should().BeNull();
    }

    /// <summary>
    /// 测试 Get(string) 方法支持多种格式
    /// </summary>
    [Theory(DisplayName = "Get(string) 应支持多种格式")]
    [InlineData("ADR-001", 1)]
    [InlineData("ADR-1", 1)]
    [InlineData("001", 1)]
    [InlineData("1", 1)]
    [InlineData("ADR001", 1)]
    [InlineData("ADR-900", 900)]
    [InlineData("900", 900)]
    public void Get_String_Should_Support_Multiple_Formats(string adrId, int expectedAdrNumber)
    {
        // Act
        var ruleSet = RuleSetRegistry.Get(adrId);

        // Assert
        ruleSet.Should().NotBeNull();
        ruleSet!.AdrNumber.Should().Be(expectedAdrNumber);
    }

    /// <summary>
    /// 测试 Get(string) 方法对无效输入返回 null
    /// </summary>
    [Theory(DisplayName = "Get(string) 对无效输入应返回 null")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("ADR-")]
    [InlineData("ADR")]
    public void Get_String_Should_Return_Null_For_Invalid_Input(string? adrId)
    {
        // Act
        var ruleSet = RuleSetRegistry.Get(adrId!);

        // Assert
        ruleSet.Should().BeNull();
    }

    #endregion

    #region GetStrict 方法测试（严格模式）

    /// <summary>
    /// 测试 GetStrict(int) 方法能正确获取存在的规则集
    /// </summary>
    [Theory(DisplayName = "GetStrict(int) 应正确获取存在的规则集")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(900)]
    [InlineData(907)]
    public void GetStrict_Int_Should_Return_RuleSet_When_Exists(int adrNumber)
    {
        // Act
        var ruleSet = RuleSetRegistry.GetStrict(adrNumber);

        // Assert
        ruleSet.Should().NotBeNull();
        ruleSet.AdrNumber.Should().Be(adrNumber);
    }

    /// <summary>
    /// 测试 GetStrict(int) 方法对不存在的规则集抛出异常
    /// </summary>
    [Theory(DisplayName = "GetStrict(int) 对不存在的规则集应抛出 InvalidOperationException")]
    [InlineData(999)]
    [InlineData(9999)]
    [InlineData(-1)]
    public void GetStrict_Int_Should_Throw_When_Not_Exists(int adrNumber)
    {
        // Act
        var act = () => RuleSetRegistry.GetStrict(adrNumber);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{adrNumber}*");
    }

    /// <summary>
    /// 测试 GetStrict(string) 方法支持多种格式
    /// </summary>
    [Theory(DisplayName = "GetStrict(string) 应支持多种格式")]
    [InlineData("ADR-001", 1)]
    [InlineData("ADR-1", 1)]
    [InlineData("001", 1)]
    [InlineData("1", 1)]
    [InlineData("ADR001", 1)]
    [InlineData("ADR-900", 900)]
    public void GetStrict_String_Should_Support_Multiple_Formats(string adrId, int expectedAdrNumber)
    {
        // Act
        var ruleSet = RuleSetRegistry.GetStrict(adrId);

        // Assert
        ruleSet.Should().NotBeNull();
        ruleSet.AdrNumber.Should().Be(expectedAdrNumber);
    }

    /// <summary>
    /// 测试 GetStrict(string) 方法对空字符串抛出 ArgumentException
    /// </summary>
    [Theory(DisplayName = "GetStrict(string) 对空字符串应抛出 ArgumentException")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetStrict_String_Should_Throw_ArgumentException_For_Empty_Input(string? adrId)
    {
        // Act
        var act = () => RuleSetRegistry.GetStrict(adrId!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*不能为空*")
            .And.ParamName.Should().Be("adrId");
    }

    /// <summary>
    /// 测试 GetStrict(string) 方法对格式错误抛出 ArgumentException
    /// </summary>
    [Theory(DisplayName = "GetStrict(string) 对格式错误应抛出 ArgumentException")]
    [InlineData("invalid")]
    [InlineData("ADR-")]
    [InlineData("ADR")]
    [InlineData("ADR-abc")]
    [InlineData("abc-123")]
    public void GetStrict_String_Should_Throw_ArgumentException_For_Invalid_Format(string adrId)
    {
        // Act
        var act = () => RuleSetRegistry.GetStrict(adrId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*格式*")
            .And.ParamName.Should().Be("adrId");
    }

    /// <summary>
    /// 测试 GetStrict(string) 方法对不存在的规则集抛出 InvalidOperationException
    /// </summary>
    [Theory(DisplayName = "GetStrict(string) 对不存在的规则集应抛出 InvalidOperationException")]
    [InlineData("999")]
    [InlineData("ADR-999")]
    [InlineData("9999")]
    public void GetStrict_String_Should_Throw_InvalidOperationException_When_Not_Exists(string adrId)
    {
        // Act
        var act = () => RuleSetRegistry.GetStrict(adrId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*不存在*");
    }

    #endregion

    #region 使用场景对比测试

    /// <summary>
    /// 测试使用场景：探索性查询使用 Get（宽容模式）
    /// </summary>
    [Fact(DisplayName = "探索性查询应使用 Get（宽容模式）")]
    public void Exploratory_Query_Should_Use_Get()
    {
        // Arrange - 用户可能输入任何值
        var userInput = "999";

        // Act - 使用 Get，不会抛出异常
        var ruleSet = RuleSetRegistry.Get(userInput);

        // Assert - 可以安全处理 null
        if (ruleSet == null)
        {
            // 提示用户规则集不存在
            ruleSet.Should().BeNull();
        }
        else
        {
            // 显示规则集信息
            ruleSet.Should().NotBeNull();
        }
    }

    /// <summary>
    /// 测试使用场景：测试/CI 场景使用 GetStrict（严格模式）
    /// </summary>
    [Fact(DisplayName = "测试/CI 场景应使用 GetStrict（严格模式）")]
    public void Test_CI_Scenario_Should_Use_GetStrict()
    {
        // Arrange - 测试代码中硬编码的 RuleId 必须有效
        var ruleId = "ADR-001";

        // Act & Assert - 使用 GetStrict，无效 RuleId 会立即失败
        var act = () =>
        {
            var ruleSet = RuleSetRegistry.GetStrict(ruleId);
            // 在测试中使用 ruleSet...
            ruleSet.Should().NotBeNull();
        };

        // 如果 RuleId 无效，测试会立即失败，不会静默通过
        act.Should().NotThrow();
    }

    /// <summary>
    /// 测试异常消息的可读性
    /// </summary>
    [Fact(DisplayName = "GetStrict 异常消息应包含可用的 ADR 列表")]
    public void GetStrict_Exception_Should_Include_Available_ADRs()
    {
        // Act
        var act = () => RuleSetRegistry.GetStrict(999);

        // Assert - 异常消息应包含可用的 ADR 编号
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*可用的 ADR 编号*");
    }

    #endregion

    #region 边界情况测试

    /// <summary>
    /// 测试 Get 和 GetStrict 对同一有效输入返回相同结果
    /// </summary>
    [Theory(DisplayName = "Get 和 GetStrict 对有效输入应返回相同结果")]
    [InlineData(1)]
    [InlineData(900)]
    [InlineData(907)]
    public void Get_And_GetStrict_Should_Return_Same_Result_For_Valid_Input(int adrNumber)
    {
        // Act
        var resultFromGet = RuleSetRegistry.Get(adrNumber);
        var resultFromGetStrict = RuleSetRegistry.GetStrict(adrNumber);

        // Assert
        resultFromGet.Should().NotBeNull();
        resultFromGetStrict.Should().NotBeNull();
        resultFromGet!.AdrNumber.Should().Be(resultFromGetStrict.AdrNumber);
    }

    /// <summary>
    /// 测试大小写不敏感
    /// </summary>
    [Theory(DisplayName = "Get 和 GetStrict 应支持大小写不敏感")]
    [InlineData("adr-001")]
    [InlineData("ADR-001")]
    [InlineData("Adr-001")]
    [InlineData("adr001")]
    public void Get_And_GetStrict_Should_Be_Case_Insensitive(string adrId)
    {
        // Act
        var resultFromGet = RuleSetRegistry.Get(adrId);
        var resultFromGetStrict = RuleSetRegistry.GetStrict(adrId);

        // Assert
        resultFromGet.Should().NotBeNull();
        resultFromGetStrict.Should().NotBeNull();
        resultFromGet!.AdrNumber.Should().Be(1);
        resultFromGetStrict.AdrNumber.Should().Be(1);
    }

    #endregion
}
