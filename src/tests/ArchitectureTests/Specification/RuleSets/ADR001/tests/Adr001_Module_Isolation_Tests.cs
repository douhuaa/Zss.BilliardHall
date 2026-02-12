namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR001.tests;

/// <summary>
/// ADR-001 模块隔离架构测试
/// 使用 NetArchTest 验证模块物理隔离、依赖方向等规则
/// 
/// 测试范围：
/// - Rule 1: 模块物理隔离
/// - Rule 2: 垂直切片组织
/// - Rule 3: 模块通信机制
/// 
/// 参考：
/// - docs/adr/constitutional/ADR-001.md
/// - docs/guidelines/ARCHITECTURE-TEST-GUIDELINES.md
/// </summary>
public sealed class Adr001_Module_Isolation_Tests
{
    private readonly ArchitectureRuleSet _ruleSet;
    private const string AdrReference = "docs/adr/constitutional/ADR-001.md";

    public Adr001_Module_Isolation_Tests()
    {
        // 从 RuleSetRegistry 获取规则集（最佳实践）
        _ruleSet = RuleSetRegistry.GetStrict(1);
    }

    #region Rule 1: 模块物理隔离

    /// <summary>
    /// ADR-001_1_1: 模块按业务能力独立划分
    /// 验证：模块不相互引用，通过 NetArchTest 检查程序集依赖
    /// </summary>
    [Theory(DisplayName = "ADR-001_1_1: 模块不相互引用")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Rule1_Clause1_Modules_Should_Not_Reference_Each_Other(Assembly moduleAssembly)
    {
        // 获取规则元数据
        var clause = _ruleSet.GetClause(1, 1);
        var ruleId = clause.Id.ToString();

        // 获取当前模块名称
        var currentModuleName = moduleAssembly.GetName().Name?.Split('.').Last() ?? "Unknown";

        // 获取其他模块的命名空间（用于依赖检查）
        var otherModules = ModuleAssemblyData.ModuleNames
            .Where(m => !m.Equals(currentModuleName, StringComparison.OrdinalIgnoreCase))
            .Select(m => $"Zss.BilliardHall.Modules.{m}")
            .ToArray();

        if (otherModules.Length == 0)
        {
            // 只有一个模块，跳过测试
            return;
        }

        // 使用 NetArchTest 验证不依赖其他模块
        NetArchTestHelper.AssertNoDependencyOn(
            assembly: moduleAssembly,
            forbiddenDependencies: otherModules,
            ruleId: ruleId,
            adrReference: AdrReference);
    }

    /// <summary>
    /// ADR-001_1_3: 命名空间匹配模块边界
    /// 验证：类型命名空间与模块名称一致
    /// </summary>
    [Theory(DisplayName = "ADR-001_1_3: 命名空间匹配模块边界")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Rule1_Clause3_Namespace_Should_Match_Module_Boundary(Assembly moduleAssembly)
    {
        // 获取规则元数据
        var clause = _ruleSet.GetClause(1, 3);
        var ruleId = clause.Id.ToString();

        // 获取模块名称
        var moduleName = moduleAssembly.GetName().Name?.Split('.').Last() ?? "Unknown";
        var expectedNamespacePrefix = $"Zss.BilliardHall.Modules.{moduleName}";

        // 使用 NetArchTest 验证命名空间
        NetArchTestHelper.AssertNamespaceConvention(
            assembly: moduleAssembly,
            expectedNamespacePrefix: expectedNamespacePrefix,
            ruleId: ruleId,
            adrReference: AdrReference);
    }

    #endregion

    #region Rule 2: 垂直切片组织

    /// <summary>
    /// ADR-001_2_2: 禁止跨模块水平分层
    /// 验证：无跨模块的 Domain/Application 层依赖
    /// </summary>
    [Theory(DisplayName = "ADR-001_2_2: 禁止跨模块水平分层")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Rule2_Clause2_No_Cross_Module_Layer_Dependencies(Assembly moduleAssembly)
    {
        // 获取规则元数据
        var clause = _ruleSet.GetClause(2, 2);
        var ruleId = clause.Id.ToString();

        // 获取当前模块名称
        var currentModuleName = moduleAssembly.GetName().Name?.Split('.').Last() ?? "Unknown";

        // 构建禁止的跨模块层依赖列表
        var forbiddenLayerDependencies = ModuleAssemblyData.ModuleNames
            .Where(m => !m.Equals(currentModuleName, StringComparison.OrdinalIgnoreCase))
            .SelectMany(m => new[]
            {
                $"Zss.BilliardHall.Modules.{m}.Domain",
                $"Zss.BilliardHall.Modules.{m}.Application",
                $"Zss.BilliardHall.Modules.{m}.Infrastructure"
            })
            .ToArray();

        if (forbiddenLayerDependencies.Length == 0)
        {
            return;
        }

        // 验证 Domain 层不依赖其他模块的内部层
        var domainTypes = Types.InAssembly(moduleAssembly)
            .That()
            .ResideInNamespaceMatching($".*\\.{currentModuleName}\\.Domain.*")
            .GetTypes();

        if (domainTypes.Any())
        {
            NetArchTestHelper.AssertNoDependencyOn(
                assembly: moduleAssembly,
                forbiddenDependencies: forbiddenLayerDependencies,
                ruleId: ruleId,
                adrReference: AdrReference);
        }
    }

    #endregion

    #region Rule 3: 模块通信机制

    /// <summary>
    /// ADR-001_3_1: 模块间仅通过领域事件异步通信
    /// 验证：无直接方法调用，仅事件发布/订阅
    /// 
    /// 注：这是一个示例测试，实际实现需要更复杂的分析
    /// 当前版本仅验证模块不直接依赖其他模块的公共API类
    /// </summary>
    [Theory(DisplayName = "ADR-001_3_1: 模块间仅通过领域事件通信")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Rule3_Clause1_Modules_Communicate_Via_Events_Only(Assembly moduleAssembly)
    {
        // 获取规则元数据
        var clause = _ruleSet.GetClause(3, 1);
        var ruleId = clause.Id.ToString();

        // 获取当前模块名称
        var currentModuleName = moduleAssembly.GetName().Name?.Split('.').Last() ?? "Unknown";

        // 构建禁止直接调用的其他模块API命名空间
        var forbiddenApiDependencies = ModuleAssemblyData.ModuleNames
            .Where(m => !m.Equals(currentModuleName, StringComparison.OrdinalIgnoreCase))
            .Select(m => $"Zss.BilliardHall.Modules.{m}.Api")
            .ToArray();

        if (forbiddenApiDependencies.Length == 0)
        {
            return;
        }

        // 使用 NetArchTest 验证
        var result = Types.InAssembly(moduleAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenApiDependencies)
            .GetResult();

        if (!result.IsSuccessful)
        {
            var message = AssertionMessageBuilder.BuildFromArchTestResult(
                ruleId: ruleId,
                summary: "模块间存在直接API调用",
                failingTypeNames: result.FailingTypeNames,
                remediationSteps: new[]
                {
                    "移除对其他模块 API 的直接引用",
                    "使用领域事件进行模块间通信",
                    "考虑使用消息总线或事件总线模式"
                },
                adrReference: AdrReference);

            result.IsSuccessful.Should().BeTrue(message);
        }
    }

    /// <summary>
    /// ADR-001_3_2: 模块间查询仅通过数据契约
    /// 验证：查询使用只读 DTO，无领域对象传递
    /// 
    /// 注：这是一个高级测试，需要检查返回类型
    /// 当前版本仅验证不直接返回 Domain 层对象
    /// </summary>
    [Theory(DisplayName = "ADR-001_3_2: 查询使用数据契约")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Rule3_Clause2_Queries_Use_Data_Contracts(Assembly moduleAssembly)
    {
        // 获取规则元数据
        var clause = _ruleSet.GetClause(3, 2);
        var ruleId = clause.Id.ToString();

        // 获取当前模块名称
        var currentModuleName = moduleAssembly.GetName().Name?.Split('.').Last() ?? "Unknown";

        // 查找 Query 或 Handler 类
        var queryHandlerTypes = Types.InAssembly(moduleAssembly)
            .That()
            .HaveNameMatching(".*Query.*|.*Handler.*")
            .And()
            .AreClasses()
            .GetTypes();

        if (!queryHandlerTypes.Any())
        {
            // 没有查询处理器，跳过测试
            return;
        }

        // 验证这些类型不依赖其他模块的 Domain 层
        var forbiddenDomainDependencies = ModuleAssemblyData.ModuleNames
            .Where(m => !m.Equals(currentModuleName, StringComparison.OrdinalIgnoreCase))
            .Select(m => $"Zss.BilliardHall.Modules.{m}.Domain")
            .ToArray();

        if (forbiddenDomainDependencies.Length == 0)
        {
            return;
        }

        var result = Types.InAssembly(moduleAssembly)
            .That()
            .HaveNameMatching(".*Query.*|.*Handler.*")
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDomainDependencies)
            .GetResult();

        if (!result.IsSuccessful)
        {
            var message = AssertionMessageBuilder.BuildFromArchTestResult(
                ruleId: ruleId,
                summary: "查询处理器依赖其他模块的领域对象",
                failingTypeNames: result.FailingTypeNames,
                remediationSteps: new[]
                {
                    "使用 DTO 或数据契约而非领域对象",
                    "定义只读查询模型 (Read Model)",
                    "确保查询结果不包含领域对象引用"
                },
                adrReference: AdrReference);

            result.IsSuccessful.Should().BeTrue(message);
        }
    }

    #endregion
}
