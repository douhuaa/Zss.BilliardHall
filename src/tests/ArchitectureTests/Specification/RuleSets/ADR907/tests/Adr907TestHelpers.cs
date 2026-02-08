namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.tests;

/// <summary>
/// ADR-907 测试的公共辅助方法
/// 提取重复的测试逻辑，提高可维护性
/// </summary>
internal static class Adr907TestHelpers
{
    // 测试项目根目录
    internal static readonly string ArchTestProjectRoot = 
        Path.Combine(TestEnvironment.SourceRoot, "tests", "ArchitectureTests");
    
    // ADR 测试目录根路径
    internal static readonly string AdrTestsRoot = 
        Path.Combine(ArchTestProjectRoot, "Specification", "RuleSets");

    /// <summary>
    /// 获取类型的所有测试方法
    /// </summary>
    internal static List<MethodInfo> GetTestMethods(Type testType)
    {
        return testType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<FactAttribute>() != null ||
                       m.GetCustomAttribute<TheoryAttribute>() != null)
            .ToList();
    }

    /// <summary>
    /// 获取符合 ADR_XXX_Y_Z 格式的测试方法
    /// </summary>
    internal static List<MethodInfo> GetRuleClauseTestMethods(Type testType)
    {
        return GetTestMethods(testType)
            .Where(m => Regex.IsMatch(m.Name, @"^ADR_\d{3}_\d+_\d+_"))
            .ToList();
    }

    /// <summary>
    /// 获取所有 ADR 测试类型
    /// </summary>
    internal static List<Type> GetAdrTestTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.Namespace != null && t.Namespace.Contains("RuleSets.ADR"))
            .Where(t => t.Name.EndsWith("_Tests") || t.Name.EndsWith("Tests"))
            .ToList();
    }

    /// <summary>
    /// 验证目录存在并返回所有符合 ADR-XXX 格式的子目录
    /// </summary>
    internal static List<string> GetAdrDirectories(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            return new List<string>();
        }

        return Directory.GetDirectories(rootPath)
            .Where(d => Regex.IsMatch(Path.GetFileName(d), @"^ADR\d{3}$"))
            .ToList();
    }

    /// <summary>
    /// 断言违规列表为空，如果不为空则生成详细的错误消息
    /// </summary>
    internal static void AssertNoViolations(
        string ruleId,
        string summary,
        IEnumerable<string> violations,
        IEnumerable<string> remediationSteps,
        string adrReference = "docs/adr/ADR-907.md")
    {
        violations.Should().BeEmpty(
            AssertionMessageBuilder.BuildWithViolations(
                ruleId,
                summary,
                violations,
                remediationSteps,
                adrReference));
    }

    /// <summary>
    /// 检查方法是否为空或过小（启发式检查）
    /// </summary>
    internal static bool IsEmptyOrTooSmall(MethodInfo method, int minILSize = 20)
    {
        var body = method.GetMethodBody();
        return body == null || body.GetILAsByteArray()?.Length < minILSize;
    }

    /// <summary>
    /// 获取当前测试文件的路径
    /// </summary>
    internal static string? GetCurrentTestFilePath()
    {
        return new StackTrace(true).GetFrame(1)?.GetFileName();
    }

    /// <summary>
    /// 检查文件内容是否包含指定模式
    /// </summary>
    internal static bool FileContainsPattern(string? filePath, string pattern)
    {
        if (filePath == null || !File.Exists(filePath))
        {
            return false;
        }

        var content = File.ReadAllText(filePath);
        return content.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 检查文件内容是否匹配正则表达式
    /// </summary>
    internal static bool FileMatchesRegex(string? filePath, string pattern)
    {
        if (filePath == null || !File.Exists(filePath))
        {
            return false;
        }

        var content = File.ReadAllText(filePath);
        return Regex.IsMatch(content, pattern);
    }
}
