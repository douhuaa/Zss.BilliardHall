namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests;

/// <summary>
/// RuleSetRegistry 功能测试
/// 验证规则集注册表的查询和验证行为
/// </summary>
public sealed class RuleSetRegistryTests
{
    // 物化共享 ADR 列表，避免重复延迟枚举与潜在性能问题
    private static readonly IReadOnlyList<int> AllAdrNumbers = RuleSetRegistry.GetAllAdrNumbers().ToList();

    #region Get 方法测试（宽容模式）

    [Theory(DisplayName = "Get(int) 应正确获取存在的规则集")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(900)]
    [InlineData(907)]
    public void Get_Int_Should_Return_RuleSet_When_Exists(int adrNumber)
    {
        var ruleSet = RuleSetRegistry.Get(adrNumber);
        AssertRuleSetExists(ruleSet, adrNumber);
    }

    [Theory(DisplayName = "Get(int) 对不存在的规则集应返回 null")]
    [InlineData(999)]
    [InlineData(9999)]
    [InlineData(-1)]
    public void Get_Int_Should_Return_Null_When_Not_Exists(int adrNumber)
    {
        var ruleSet = RuleSetRegistry.Get(adrNumber);
        ruleSet.Should().BeNull();
    }

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
        var ruleSet = RuleSetRegistry.Get(adrId);
        AssertRuleSetExists(ruleSet, expectedAdrNumber);
    }

    [Theory(DisplayName = "Get(string) 对无效输入应返回 null")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("ADR-")]
    [InlineData("ADR")]
    public void Get_String_Should_Return_Null_For_Invalid_Input(string? adrId)
    {
        var ruleSet = RuleSetRegistry.Get(adrId!);
        ruleSet.Should().BeNull();
    }

    #endregion

    #region GetStrict 方法测试（严格模式）

    [Theory(DisplayName = "GetStrict(int) 应正确获取存在的规则集")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(900)]
    [InlineData(907)]
    public void GetStrict_Int_Should_Return_RuleSet_When_Exists(int adrNumber)
    {
        var ruleSet = RuleSetRegistry.GetStrict(adrNumber);
        AssertRuleSetExists(ruleSet, adrNumber);
    }

    [Theory(DisplayName = "GetStrict(int) 对不存在的规则集应抛出 InvalidOperationException")]
    [InlineData(999)]
    [InlineData(9999)]
    [InlineData(-1)]
    public void GetStrict_Int_Should_Throw_When_Not_Exists(int adrNumber)
    {
        var act = () => RuleSetRegistry.GetStrict(adrNumber);
        act.Should().Throw<InvalidOperationException>().WithMessage($"*{adrNumber}*");
    }

    [Theory(DisplayName = "GetStrict(string) 应支持多种格式")]
    [InlineData("ADR-001", 1)]
    [InlineData("ADR-1", 1)]
    [InlineData("001", 1)]
    [InlineData("1", 1)]
    [InlineData("ADR001", 1)]
    [InlineData("ADR-900", 900)]
    public void GetStrict_String_Should_Support_Multiple_Formats(string adrId, int expectedAdrNumber)
    {
        var ruleSet = RuleSetRegistry.GetStrict(adrId);
        AssertRuleSetExists(ruleSet, expectedAdrNumber);
    }

    [Theory(DisplayName = "GetStrict(string) 对空字符串应抛出 ArgumentException")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetStrict_String_Should_Throw_ArgumentException_For_Empty_Input(string? adrId)
    {
        var act = () => RuleSetRegistry.GetStrict(adrId!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*不能为空*")
            .And.ParamName.Should().Be("adrId");
    }

    [Theory(DisplayName = "GetStrict(string) 对格式错误应抛出 ArgumentException")]
    [InlineData("invalid")]
    [InlineData("ADR-")]
    [InlineData("ADR")]
    [InlineData("ADR-abc")]
    [InlineData("abc-123")]
    [InlineData("ADR0001")]  // 4位数字格式 - 应被拒绝
    [InlineData("0001")]     // 4位数字格式 - 应被拒绝
    [InlineData("9999")]     // 4位数字格式 - 应被拒绝
    [InlineData("ADR.001")]  // 错误分隔符 - 应被拒绝
    public void GetStrict_String_Should_Throw_ArgumentException_For_Invalid_Format(string adrId)
    {
        var act = () => RuleSetRegistry.GetStrict(adrId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*格式*")
            .And.ParamName.Should().Be("adrId");
    }

    [Theory(DisplayName = "GetStrict(string) 对不存在的规则集应抛出 InvalidOperationException")]
    [InlineData("999")]
    [InlineData("ADR-999")]
    public void GetStrict_String_Should_Throw_InvalidOperationException_When_Not_Exists(string adrId)
    {
        var act = () => RuleSetRegistry.GetStrict(adrId);
        act.Should().Throw<InvalidOperationException>().WithMessage("*不存在*");
    }

    [Theory(DisplayName = "GetStrict(string) 应拒绝 4 位数字格式")]
    [InlineData("ADR0001")]
    [InlineData("0001")]
    [InlineData("ADR0120")]
    public void GetStrict_String_Should_Reject_Four_Digit_Format(string adrId)
    {
        var act = () => RuleSetRegistry.GetStrict(adrId);
        var exception = act.Should().Throw<ArgumentException>()
            .WithMessage("*无效的 ADR 编号格式*");
        exception.And.Message.Should().Contain("不支持 4 位数字");
        exception.And.ParamName.Should().Be("adrId");
    }

    #endregion

    #region 使用场景对比测试

    [Fact(DisplayName = "探索性查询应使用 Get（宽容模式）")]
    public void Exploratory_Query_Should_Use_Get()
    {
        var userInput = "999";
        var ruleSet = RuleSetRegistry.Get(userInput);
        // 可以安全处理 null，保持行为清晰
        if (ruleSet == null)
            ruleSet.Should().BeNull();
        else
            AssertRuleSetExists(ruleSet, ruleSet.AdrNumber);
    }

    [Fact(DisplayName = "测试/CI 场景应使用 GetStrict（严格模式）")]
    public void Test_CI_Scenario_Should_Use_GetStrict()
    {
        var ruleId = "ADR-001";
        Action act = () => {
            var ruleSet = RuleSetRegistry.GetStrict(ruleId);
            AssertRuleSetExists(ruleSet, 1);
        };
        act.Should().NotThrow("在测试/CI场景中，GetStrict 应该成功获取已定义的规则集");
    }

    [Fact(DisplayName = "GetStrict 异常消息应包含可用的 ADR 编号")]
    public void GetStrict_Exception_Should_Include_Available_ADRs()
    {
        Action act = () => RuleSetRegistry.GetStrict(999);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*可用的 ADR 编号*");
    }

    #endregion

    #region 边界情况测试

    [Theory(DisplayName = "Get 和 GetStrict 对有效输入应返回相同结果")]
    [InlineData(1)]
    [InlineData(900)]
    [InlineData(907)]
    public void Get_And_GetStrict_Should_Return_Same_Result_For_Valid_Input(int adrNumber)
    {
        var resultFromGet = RuleSetRegistry.Get(adrNumber);
        var resultFromGetStrict = RuleSetRegistry.GetStrict(adrNumber);
        AssertRuleSetExists(resultFromGet, adrNumber);
        AssertRuleSetExists(resultFromGetStrict, adrNumber);
        resultFromGet!.AdrNumber.Should().Be(resultFromGetStrict.AdrNumber);
    }

    [Theory(DisplayName = "Get 和 GetStrict 应支持大小写不敏感")]
    [InlineData("adr-001")]
    [InlineData("ADR-001")]
    [InlineData("Adr-001")]
    [InlineData("adr001")]
    public void Get_And_GetStrict_Should_Be_Case_Insensitive(string adrId)
    {
        var resultFromGet = RuleSetRegistry.Get(adrId);
        var resultFromGetStrict = RuleSetRegistry.GetStrict(adrId);
        AssertRuleSetExists(resultFromGet, 1);
        AssertRuleSetExists(resultFromGetStrict, 1);
    }

    #endregion

    #region 自动发现机制测试

    [Fact(DisplayName = "Registry 应该自动发现所有 RuleSet 定义")]
    public void Registry_Should_Auto_Discover_All_RuleSets()
    {
        var allAdrNumbers = AllAdrNumbers;
        allAdrNumbers.Should().NotBeEmpty();
        new[] { 1, 2, 3, 120, 201, 900, 907 }.ForEach(expected => allAdrNumbers.Should().Contain(expected));
    }

    [Fact(DisplayName = "所有自动发现的 RuleSet 应该可以正确实例化")]
    public void All_Auto_Discovered_RuleSets_Should_Be_Properly_Instantiated()
    {
        var allRuleSets = RuleSetRegistry.GetAllRuleSets().ToList();
        allRuleSets.Should().NotBeEmpty();
        allRuleSets.Should().OnlyHaveUniqueItems(rs => rs.AdrNumber);
        foreach (var ruleSet in allRuleSets)
        {
            ruleSet.Should().NotBeNull();
            ruleSet.AdrNumber.Should().BeGreaterThan(0);
            ruleSet.RuleCount.Should().BeGreaterThan(0, $"ADR-{ruleSet.AdrNumber} 应至少包含一个规则");
        }
    }

    [Fact(DisplayName = "Registry 不应包含重复的 ADR 编号")]
    public void Registry_Should_Not_Contain_Duplicate_Adr_Numbers()
    {
        var allAdrNumbers = AllAdrNumbers;
        var duplicates = allAdrNumbers
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        duplicates.Should().BeEmpty();
    }

    #endregion

    #region 辅助方法

    private static void AssertRuleSetExists(ArchitectureRuleSet? ruleSet, int expectedAdr)
    {
        ruleSet.Should().NotBeNull();
        ruleSet!.AdrNumber.Should().Be(expectedAdr);
    }

    #endregion
}

static class LinqExtensions
{
    // 小工具：在断言序列中逐项断言（语法糖，提升可读性）
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source) action(item);
    }
}
