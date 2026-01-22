using System.Reflection;
using System.Text.RegularExpressions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR 架构测试辅助工具集
/// 提供常用的测试辅助方法，统一错误消息格式
/// </summary>
public static class AdrTestHelpers
{
    /// <summary>
    /// 格式化 ADR 违规错误消息
    /// </summary>
    /// <param name="adrNumber">ADR 编号（如 "0110"）</param>
    /// <param name="violationDescription">违规描述</param>
    /// <param name="violations">违规项列表</param>
    /// <param name="correctFormat">正确格式说明</param>
    /// <param name="examples">示例（可选）</param>
    /// <param name="fixSuggestion">修复建议</param>
    /// <returns>格式化的错误消息</returns>
    public static string FormatViolationMessage(
        string adrNumber,
        string violationDescription,
        IEnumerable<string> violations,
        string? correctFormat = null,
        string? examples = null,
        string? fixSuggestion = null)
    {
        var message = $"❌ ADR-{adrNumber} 违规：{violationDescription}\n";
        
        if (violations.Any())
        {
            message += $"{string.Join("\n", violations)}\n";
        }
        
        if (!string.IsNullOrEmpty(correctFormat))
        {
            message += $"正确格式：{correctFormat}\n";
        }
        
        if (!string.IsNullOrEmpty(examples))
        {
            message += $"示例：{examples}\n";
        }
        
        if (!string.IsNullOrEmpty(fixSuggestion))
        {
            message += $"修复建议：{fixSuggestion}";
        }
        
        return message;
    }

    /// <summary>
    /// 断言没有违规项
    /// </summary>
    /// <param name="violations">违规项列表</param>
    /// <param name="errorMessage">错误消息</param>
    public static void AssertNoViolations(IEnumerable<string> violations, string errorMessage)
    {
        var violationList = violations.ToList();
        Assert.True(violationList.Count == 0, errorMessage);
    }

    /// <summary>
    /// 断言没有违规项（带自动格式化）
    /// </summary>
    public static void AssertNoViolations(
        string adrNumber,
        string violationDescription,
        IEnumerable<string> violations,
        string? correctFormat = null,
        string? examples = null,
        string? fixSuggestion = null)
    {
        var violationList = violations.ToList();
        if (violationList.Count > 0)
        {
            var message = FormatViolationMessage(
                adrNumber,
                violationDescription,
                violationList,
                correctFormat,
                examples,
                fixSuggestion);
            Assert.Fail(message);
        }
    }

    /// <summary>
    /// 获取程序集中指定命名空间的类型
    /// </summary>
    /// <param name="assembly">程序集</param>
    /// <param name="namespacePattern">命名空间模式（支持通配符）</param>
    /// <param name="includeAbstract">是否包含抽象类</param>
    /// <returns>类型列表</returns>
    public static IEnumerable<Type> GetTypesInNamespace(
        Assembly assembly,
        string namespacePattern,
        bool includeAbstract = false)
    {
        return assembly.GetTypes()
            .Where(t => t.Namespace?.Contains(namespacePattern) == true)
            .Where(t => t.IsClass)
            .Where(t => includeAbstract || !t.IsAbstract);
    }

    /// <summary>
    /// 验证类型名称符合指定后缀
    /// </summary>
    /// <param name="types">类型列表</param>
    /// <param name="requiredSuffix">必需的后缀</param>
    /// <param name="excludePattern">排除的模式（可选）</param>
    /// <returns>不符合规范的类型全名列表</returns>
    public static IEnumerable<string> ValidateTypeSuffix(
        IEnumerable<Type> types,
        string requiredSuffix,
        string? excludePattern = null)
    {
        var violations = new List<string>();
        
        foreach (var type in types)
        {
            // 如果有排除模式，跳过匹配的类型
            if (!string.IsNullOrEmpty(excludePattern) && type.Name.Contains(excludePattern))
            {
                continue;
            }
            
            if (!type.Name.EndsWith(requiredSuffix))
            {
                violations.Add(type.FullName ?? type.Name);
            }
        }
        
        return violations;
    }

    /// <summary>
    /// 验证类型名称符合正则表达式模式
    /// </summary>
    /// <param name="types">类型列表</param>
    /// <param name="pattern">正则表达式模式</param>
    /// <returns>不符合规范的类型信息列表</returns>
    public static IEnumerable<string> ValidateTypeNamePattern(
        IEnumerable<Type> types,
        string pattern)
    {
        var regex = new Regex(pattern);
        var violations = new List<string>();
        
        foreach (var type in types)
        {
            if (!regex.IsMatch(type.Name))
            {
                violations.Add($"{type.FullName} (当前名称: {type.Name})");
            }
        }
        
        return violations;
    }

    /// <summary>
    /// 获取测试方法的 DisplayName 属性值
    /// </summary>
    /// <param name="method">方法信息</param>
    /// <returns>DisplayName，如果没有则返回 null</returns>
    public static string? GetTestDisplayName(MethodInfo method)
    {
        var factAttr = method.GetCustomAttributes(typeof(FactAttribute), false)
            .FirstOrDefault() as FactAttribute;
        
        var theoryAttr = method.GetCustomAttributes(typeof(TheoryAttribute), false)
            .FirstOrDefault() as TheoryAttribute;
        
        return factAttr?.DisplayName ?? theoryAttr?.DisplayName;
    }

    /// <summary>
    /// 验证 ADR 测试类命名符合规范
    /// </summary>
    /// <param name="testAssembly">测试程序集</param>
    /// <returns>不符合规范的类名列表</returns>
    public static IEnumerable<string> ValidateAdrTestClassNames(Assembly testAssembly)
    {
        var adrTestTypes = testAssembly.GetTypes()
            .Where(t => t.Name.StartsWith("ADR_") && t.Name.Contains("Architecture_Tests"))
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        var invalidNames = new List<string>();
        var pattern = @"^ADR_\d{4}_Architecture_Tests$";

        foreach (var testType in adrTestTypes)
        {
            if (!Regex.IsMatch(testType.Name, pattern))
            {
                invalidNames.Add(testType.Name);
            }
        }

        return invalidNames;
    }

    /// <summary>
    /// 验证测试方法的 DisplayName 包含正确的 ADR 编号
    /// </summary>
    /// <param name="testType">测试类类型</param>
    /// <param name="expectedAdrNumber">期望的 ADR 编号（如 "0110"）</param>
    /// <returns>违规描述列表</returns>
    public static IEnumerable<string> ValidateTestDisplayNames(Type testType, string expectedAdrNumber)
    {
        var violations = new List<string>();
        var expectedPrefix = $"ADR-{expectedAdrNumber}";

        var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any() ||
                       m.GetCustomAttributes(typeof(TheoryAttribute), false).Any())
            .ToList();

        foreach (var method in testMethods)
        {
            var displayName = GetTestDisplayName(method);
            
            if (string.IsNullOrEmpty(displayName) || !displayName.StartsWith(expectedPrefix))
            {
                violations.Add($"{testType.Name}.{method.Name}: DisplayName='{displayName}' 应以 '{expectedPrefix}' 开头");
            }
        }

        return violations;
    }

    /// <summary>
    /// 从测试类名提取 ADR 编号
    /// </summary>
    /// <param name="testTypeName">测试类名（如 "ADR_0110_Architecture_Tests"）</param>
    /// <returns>ADR 编号（如 "0110"），如果无法提取则返回 null</returns>
    public static string? ExtractAdrNumber(string testTypeName)
    {
        var match = Regex.Match(testTypeName, @"ADR_(\d{4})_");
        return match.Success ? match.Groups[1].Value : null;
    }
}
