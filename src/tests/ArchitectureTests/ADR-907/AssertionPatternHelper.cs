using System.Text.RegularExpressions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_907;

/// <summary>
/// ADR-907 断言模式检测辅助类
/// 提供统一的断言模式定义和检测方法，支持所有常用的 FluentAssertions API
/// </summary>
internal static class AssertionPatternHelper
{
    /// <summary>
    /// 获取所有支持的断言方法正则模式
    /// 用于 ADR-907_3_1 测试：统计测试类中的有效断言数量
    /// </summary>
    public static string[] GetAssertionPatterns() => new[]
    {
        // xUnit Assert 方法
        @"Assert\.True\s*\(",
        @"Assert\.False\s*\(",
        @"Assert\.Equal\s*\(",
        @"Assert\.NotEqual\s*\(",
        @"Assert\.Matches\s*\(",
        @"Assert\.Fail\s*\(",
        
        // FluentAssertions 基础断言
        @"\.Should\(\)\.BeTrue\s*\(",
        @"\.Should\(\)\.BeFalse\s*\(",
        @"\.Should\(\)\.Be\s*\(",
        @"\.Should\(\)\.NotBe\s*\(",
        
        // FluentAssertions 集合断言
        @"\.Should\(\)\.BeEmpty\s*\(",
        @"\.Should\(\)\.NotBeEmpty\s*\(",
        @"\.Should\(\)\.Contain\s*\(",
        @"\.Should\(\)\.NotContain\s*\(",
        
        // FluentAssertions null 检查
        @"\.Should\(\)\.BeNull\s*\(",
        @"\.Should\(\)\.NotBeNull\s*\(",
        @"\.Should\(\)\.NotBeNullOrEmpty\s*\(",
        @"\.Should\(\)\.NotBeNullOrWhiteSpace\s*\(",
        
        // FluentAssertions 字符串断言
        @"\.Should\(\)\.StartWith\s*\(",
        @"\.Should\(\)\.EndWith\s*\(",
        @"\.Should\(\)\.Match\s*\(",
        @"\.Should\(\)\.MatchRegex\s*\(",
        
        // FluentAssertions 数值比较
        @"\.Should\(\)\.BeGreaterThan\s*\(",
        @"\.Should\(\)\.BeLessThan\s*\(",
        @"\.Should\(\)\.BeGreaterThanOrEqualTo\s*\(",
        @"\.Should\(\)\.BeLessThanOrEqualTo\s*\(",
        
        // FluentAssertions 类型检查
        @"\.Should\(\)\.BeOfType\s*\(",
        @"\.Should\(\)\.BeAssignableTo\s*\(",
    };

    /// <summary>
    /// 获取用于提取断言消息的正则表达式模式
    /// 用于 ADR-907_2_6 和 ADR-907_3_3 测试：验证失败信息包含 ADR 引用
    /// </summary>
    /// <returns>正则表达式模式字符串</returns>
    /// <remarks>
    /// 正则表达式捕获组说明：
    /// - Group 1: 完整的断言方法调用（Should().{Method} 或 Assert.{Method}）
    /// - Group 2: FluentAssertions 方法名（BeTrue、BeEmpty 等）
    /// - Group 3: xUnit 方法名（True、False、Equal 等）
    /// - Group 4: 断言参数部分（包含消息字符串）- 由 AssertionArgsGroupIndex 常量指定
    /// </remarks>
    public static string GetAssertionMessagePattern()
    {
        // 构建完整的断言方法列表（与 GetAssertionPatterns 保持一致）
        // 注意：需要手动维护与 GetAssertionPatterns() 的同步
        const string fluentMethods = "BeTrue|BeFalse|BeEmpty|NotBeEmpty|Contain|NotContain|" +
                                    "BeNull|NotBeNull|NotBeNullOrEmpty|NotBeNullOrWhiteSpace|" +
                                    "StartWith|EndWith|Match|MatchRegex|Be|NotBe|" +
                                    "BeGreaterThan|BeLessThan|BeGreaterThanOrEqualTo|BeLessThanOrEqualTo|" +
                                    "BeOfType|BeAssignableTo";
        const string xunitMethods = "True|False|Equal|NotEqual|Matches";
        
        // 匹配格式：
        // - Should().{Method}("message") 或 Should().{Method}($"message")
        // - Assert.{Method}(condition, "message") 或 Assert.{Method}(condition, $"message")
        // - 支持多行字符串连接：$"part1" + $"part2"
        // 注意：Assert.Fail 被排除，因为它的签名不同（直接接受消息，无条件参数）
        return $@"(Should\(\)\.({fluentMethods})|Assert\.({xunitMethods}))\s*\(([^)]*\$?""[^""]+""(?:\s*\+\s*\$?""[^""]+"")*)\s*\)";
    }

    /// <summary>
    /// 断言参数捕获组索引
    /// 在 GetAssertionMessagePattern() 返回的正则表达式中，参数部分位于捕获组 4
    /// </summary>
    public const int AssertionArgsGroupIndex = 4;

    /// <summary>
    /// 从断言匹配结果中提取完整的失败消息
    /// </summary>
    /// <param name="assertMatch">断言的正则匹配结果</param>
    /// <returns>完整的失败消息字符串</returns>
    public static string ExtractFullMessage(Match assertMatch)
    {
        // 提取断言参数部分（可能包含多个字符串连接）
        var assertArgs = assertMatch.Groups[AssertionArgsGroupIndex].Value;
        
        // 提取所有字符串字面量（支持 $"..." 和 "..."）
        var stringLiterals = Regex.Matches(assertArgs, @"\$?""([^""]+)""");
        return string.Join("", stringLiterals.Cast<Match>().Select(m => m.Groups[1].Value));
    }
}
