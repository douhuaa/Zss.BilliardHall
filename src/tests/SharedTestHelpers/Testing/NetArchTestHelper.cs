namespace Zss.BilliardHall.Tests.SharedTestHelpers.Testing;

/// <summary>
/// NetArchTest 辅助类
/// 提供 NetArchTest 的最佳实践封装，简化架构测试编写
/// 
/// 设计原则：
/// 1. 封装常用的 NetArchTest 模式，提供流畅 API
/// 2. 集成 RuleSetRegistry 获取规则元数据
/// 3. 使用 AssertionMessageBuilder 标准化错误消息
/// 4. 支持程序集缓存，提升测试性能
/// 
/// 最佳实践：
/// - 使用 Types.InAssembly() 而非 Types.InCurrentDomain() 以获得更好的性能
/// - 链式调用提高可读性：That().ResideInNamespace().Should().BeSealed()
/// - 始终检查 GetResult().IsSuccessful 并提供清晰的错误消息
/// - 使用 FailingTypeNames 提供违规类型的详细信息
/// </summary>
public static class NetArchTestHelper
{
    /// <summary>
    /// 程序集缓存，避免重复加载
    /// </summary>
    private static readonly Lazy<Assembly[]> _allAssemblies = new(() =>
    {
        var assemblies = new List<Assembly>();

        // 添加模块程序集
        assemblies.AddRange(ModuleAssemblyData.ModuleAssemblies);

        // 添加其他关键程序集（如需要）
        try
        {
            // Platform 程序集
            var platformAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name?.Contains("Platform") == true);
            if (platformAssembly != null)
            {
                assemblies.Add(platformAssembly);
            }

            // Application 程序集
            var appAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name?.Contains("Application") == true &&
                                    !a.GetName().Name.Contains("Test"));
            if (appAssembly != null)
            {
                assemblies.Add(appAssembly);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NetArchTestHelper] 加载程序集时出错: {ex.Message}");
        }

        return assemblies.Distinct().ToArray();
    });

    /// <summary>
    /// 获取所有被测试的程序集
    /// </summary>
    public static Assembly[] AllAssemblies => _allAssemblies.Value;

    /// <summary>
    /// 获取指定模块的程序集
    /// </summary>
    /// <param name="moduleName">模块名称（如 "Members", "Orders"）</param>
    /// <returns>模块程序集，如果未找到则返回 null</returns>
    public static Assembly? GetModuleAssembly(string moduleName)
    {
        return ModuleAssemblyData.ModuleAssemblies
            .FirstOrDefault(a => a.GetName().Name?.EndsWith(moduleName) == true);
    }

    /// <summary>
    /// 验证命名空间规则
    /// 使用 NetArchTest 验证类型的命名空间是否符合预期
    /// </summary>
    /// <param name="assembly">要验证的程序集</param>
    /// <param name="expectedNamespacePrefix">预期的命名空间前缀</param>
    /// <param name="ruleId">规则 ID（用于错误消息）</param>
    /// <param name="adrReference">ADR 文档引用</param>
    public static void AssertNamespaceConvention(
        Assembly assembly,
        string expectedNamespacePrefix,
        string ruleId,
        string adrReference)
    {
        var result = Types.InAssembly(assembly)
            .That()
            .AreNotNested()
            .And()
            .DoNotHaveName("AssemblyInfo")
            .Should()
            .ResideInNamespaceMatching($"^{Regex.Escape(expectedNamespacePrefix)}.*")
            .GetResult();

        if (!result.IsSuccessful)
        {
            var message = AssertionMessageBuilder.BuildFromArchTestResult(
                ruleId: ruleId,
                summary: "命名空间约定违规",
                failingTypeNames: result.FailingTypeNames,
                remediationSteps: new[]
                {
                    $"将违规类型移动到 {expectedNamespacePrefix} 命名空间下",
                    "确保命名空间与模块边界一致",
                    "检查是否有错误的 using 声明"
                },
                adrReference: adrReference);

            result.IsSuccessful.Should().BeTrue(message);
        }
    }

    /// <summary>
    /// 验证依赖规则
    /// 使用 NetArchTest 验证程序集之间的依赖关系
    /// </summary>
    /// <param name="assembly">要验证的程序集</param>
    /// <param name="forbiddenDependencies">禁止的依赖项列表</param>
    /// <param name="ruleId">规则 ID</param>
    /// <param name="adrReference">ADR 文档引用</param>
    public static void AssertNoDependencyOn(
        Assembly assembly,
        string[] forbiddenDependencies,
        string ruleId,
        string adrReference)
    {
        var predicateList = Types.InAssembly(assembly);

        foreach (var dependency in forbiddenDependencies)
        {
            var result = predicateList
                .ShouldNot()
                .HaveDependencyOn(dependency)
                .GetResult();

            if (!result.IsSuccessful)
            {
                var message = AssertionMessageBuilder.BuildFromArchTestResult(
                    ruleId: ruleId,
                    summary: $"禁止依赖 {dependency}",
                    failingTypeNames: result.FailingTypeNames,
                    remediationSteps: new[]
                    {
                        $"移除对 {dependency} 的直接引用",
                        "考虑使用依赖注入或领域事件进行通信",
                        "检查是否违反了分层架构原则"
                    },
                    adrReference: adrReference);

                result.IsSuccessful.Should().BeTrue(message);
            }
        }
    }

    /// <summary>
    /// 验证 sealed 类规则
    /// 确保特定命名空间下的类都是 sealed
    /// </summary>
    /// <param name="assembly">要验证的程序集</param>
    /// <param name="namespacePattern">命名空间模式（正则表达式）</param>
    /// <param name="ruleId">规则 ID</param>
    /// <param name="adrReference">ADR 文档引用</param>
    public static void AssertClassesAreSealed(
        Assembly assembly,
        string namespacePattern,
        string ruleId,
        string adrReference)
    {
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceMatching(namespacePattern)
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        if (!result.IsSuccessful)
        {
            var message = AssertionMessageBuilder.BuildFromArchTestResult(
                ruleId: ruleId,
                summary: "类未标记为 sealed",
                failingTypeNames: result.FailingTypeNames,
                remediationSteps: new[]
                {
                    "将违规类标记为 sealed",
                    "或将类设计为 abstract 以允许继承",
                    "确保遵循默认 sealed 的最佳实践"
                },
                adrReference: adrReference);

            result.IsSuccessful.Should().BeTrue(message);
        }
    }

    /// <summary>
    /// 验证测试结果并生成标准化错误消息
    /// </summary>
    /// <param name="result">NetArchTest 测试结果</param>
    /// <param name="ruleId">规则 ID</param>
    /// <param name="summary">简短描述</param>
    /// <param name="remediationSteps">修复建议</param>
    /// <param name="adrReference">ADR 文档引用</param>
    public static void AssertSuccessful(
        TestResult result,
        string ruleId,
        string summary,
        string[] remediationSteps,
        string adrReference)
    {
        if (!result.IsSuccessful)
        {
            var message = AssertionMessageBuilder.BuildFromArchTestResult(
                ruleId: ruleId,
                summary: summary,
                failingTypeNames: result.FailingTypeNames,
                remediationSteps: remediationSteps,
                adrReference: adrReference);

            result.IsSuccessful.Should().BeTrue(message);
        }
    }
}
