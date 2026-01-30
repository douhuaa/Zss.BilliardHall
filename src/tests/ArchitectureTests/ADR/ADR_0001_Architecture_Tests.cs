using NetArchTest.Rules;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0001: 模块化单体与垂直切片架构决策（v4.0）
/// 验证模块隔离、垂直切片、契约使用等核心架构约束
/// 
/// 约束映射（对应 ADR-0001 v4.0 快速参考和架构测试映射表）：
/// - ADR-0001.1: 模块不可相互引用 (L1) → Modules_Should_Not_Reference_Other_Modules
/// - ADR-0001.2: 项目文件/程序集禁止引用其他模块 (L1) → Module_Csproj_Should_Not_Reference_Other_Modules
/// - ADR-0001.3: 垂直切片/用例为最小单元 (L2) → Handlers_Should_Be_In_UseCases_Namespace
/// - ADR-0001.4: 禁止横向 Service 抽象 (L1) → Modules_Should_Not_Contain_Service_Classes
/// - ADR-0001.5: 只允许事件/契约/原始类型通信 (L2) → Contract_Rules_Semantic_Check
/// - ADR-0001.6: Contract 不含业务判断字段 (L2/L3) → Contract_Business_Field_Analyzer
/// - ADR-0001.7: 命名空间/目录强制隔离 (L1) → Namespace_Should_Match_Module_Boundaries
/// </summary>
public sealed class ADR_0001_Architecture_Tests
{

    #region 1. 模块隔离约束 (ADR-0001.1, 0001.2, 0001.7)

    [Theory(DisplayName = "ADR-0001.1: 模块不应相互引用")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Reference_Other_Modules(Assembly moduleAssembly)
    {
        var moduleName = moduleAssembly.GetName()
            .Name!
            .Split('.')
            .Last();
        foreach (var other in ModuleAssemblyData.ModuleNames.Where(m => m != moduleName))
        {
            var result = Types
                .InAssembly(moduleAssembly)
                .ShouldNot()
                .HaveDependencyOn($"Zss.BilliardHall.Modules.{other}")
                .GetResult();

            Assert.True(result.IsSuccessful,
            $"❌ ADR-0001.1 违规: 模块 {moduleName} 不应依赖模块 {other}。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
            $"修复建议：\n" +
            $"  1. 使用领域事件进行异步通信\n" +
            $"  2. 使用数据契约进行只读查询\n" +
            $"  3. 传递原始类型（Guid、string）而非领域对象\n" +
            $"参考：docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md");
        }
    }

    [Theory(DisplayName = "ADR-0001.2: 模块项目文件不应引用其他模块")]
    [MemberData(nameof(GetModuleProjectFiles))]
    public void Module_Csproj_Should_Not_Reference_Other_Modules(string csprojPath)
    {
        var allowedProjectNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "Zss.BilliardHall.Platform",
            "Zss.BilliardHall.BuildingBlocks",
        };

        var doc = new System.Xml.XmlDocument();
        doc.Load(csprojPath);
        var mgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
        mgr.AddNamespace("msb", doc.DocumentElement!.NamespaceURI);

        var projectName = Path.GetFileNameWithoutExtension(csprojPath);
        var references = doc.SelectNodes("//msb:ProjectReference", mgr);
        if (references == null) return;

        foreach (System.Xml.XmlNode reference in references)
        {
            var include = reference?.Attributes?["Include"]?.Value;
            if (string.IsNullOrEmpty(include)) continue;

            var refPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(csprojPath)!, include));
            var refName = Path.GetFileNameWithoutExtension(refPath);

            if (string.Equals(refName, projectName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (allowedProjectNames.Contains(refName))
                continue;

            Assert.Fail($"❌ ADR-0001.2 违规: 模块 {projectName} 不应引用其他模块或非白名单项目: {refName}。\n" + $"项目路径: {csprojPath}\n" + $"引用路径: {include}\n" + $"修复建议：将共享代码移至 Platform/BuildingBlocks，或改用消息通信。");
        }
    }

    [Theory(DisplayName = "ADR-0001.7: 命名空间应匹配模块边界")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Namespace_Should_Match_Module_Boundaries(Assembly moduleAssembly)
    {
        var moduleName = moduleAssembly.GetName()
            .Name!
            .Split('.')
            .Last();
        var expectedNamespace = $"Zss.BilliardHall.Modules.{moduleName}";

        var result = Types
            .InAssembly(moduleAssembly)
            .That()
            .AreClasses()
            .Or()
            .AreInterfaces()
            .Should()
            .ResideInNamespaceStartingWith(expectedNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, $"❌ ADR-0001.7 违规: 模块 {moduleName} 的类型必须在命名空间 {expectedNamespace} 下。\n" + $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" + $"修复建议：确保所有类型都在正确的模块命名空间下，遵循目录结构与命名空间一致性原则。");
    }

    public static IEnumerable<object[]> GetModuleProjectFiles()
    {
        var root = TestEnvironment.RepositoryRoot;
        var modulesDir = Path.Combine(root, "src", "Modules");
        if (!Directory.Exists(modulesDir)) yield break;

        var csprojs = Directory.GetFiles(modulesDir, "*.csproj", SearchOption.AllDirectories);
        foreach (var csproj in csprojs)
        {
            yield return new object[] { csproj };
        }
    }

    #endregion

    #region 2. 垂直切片架构约束 (ADR-0001.3, 0001.4)

    [Theory(DisplayName = "ADR-0001.3: Handler 应该在 UseCases 命名空间下")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Should_Be_In_UseCases_Namespace(Assembly moduleAssembly)
    {
        var handlers = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .AreClasses()
            .GetTypes();

        foreach (var handler in handlers)
        {
            var ns = handler.Namespace ?? "";
            // 接受 UseCases 或 Features（历史原因，语义相同）
            var hasUseCases = ns.Contains(".UseCases.") || ns.Contains(".Features.");

            Assert.True(hasUseCases,
            $"❌ ADR-0001.3 违规: Handler {handler.FullName} 必须在 UseCases 或 Features 命名空间下。\n" +
            $"当前命名空间: {ns}\n" +
            $"修复建议：\n" +
            $"  1. 将 Handler 移动到对应的 UseCases/<用例名>/ 或 Features/<用例名>/ 目录下\n" +
            $"  2. 采用垂直切片架构，每个用例包含 Endpoint、Command/Query、Handler\n" +
            $"  3. 避免创建横向的 Service 层\n" +
            $"参考：docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md");
        }
    }

    [Theory(DisplayName = "ADR-0001.4: 模块不应包含横向 Service 类")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Contain_Service_Classes(Assembly moduleAssembly)
    {
        var forbidden = new[] { "Repository", "Service", "Manager", "Store" };
        foreach (var word in forbidden)
        {
            var result = Types
                .InAssembly(moduleAssembly)
                .That()
                .ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
                .ShouldNot()
                .HaveNameMatching($".*{word}.*")
                .GetResult();

            Assert.True(result.IsSuccessful,
            $"❌ ADR-0001.4 违规: 模块 {moduleAssembly.GetName().Name} 禁止包含名称含 '{word}' 的类型。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
            $"修复建议：\n" +
            $"  1. 将业务逻辑放入领域模型或 Handler 中\n" +
            $"  2. 采用垂直切片架构，避免横向抽象层\n" +
            $"  3. 如需共享技术能力，移至 Platform/BuildingBlocks\n" +
            $"参考：docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md");
        }
    }

    #endregion

    #region 3. 契约使用规则约束 (ADR-0001.5, 0001.6)

    [Theory(DisplayName = "ADR-0001.5: 模块间只允许事件/契约/原始类型通信（语义检查）")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Contract_Rules_Semantic_Check(Assembly moduleAssembly)
    {
        var moduleName = moduleAssembly.GetName()
            .Name!
            .Split('.')
            .Last();

        // 检查是否有跨模块的领域对象引用（Entity/Aggregate/ValueObject）
        var domainTypes = new[] { "Entity", "Aggregate", "ValueObject", "DomainEvent" };

        foreach (var other in ModuleAssemblyData.ModuleNames.Where(m => m != moduleName))
        {
            foreach (var domainType in domainTypes)
            {
                var result = Types
                    .InAssembly(moduleAssembly)
                    .That()
                    .ResideInNamespaceStartingWith($"Zss.BilliardHall.Modules.{moduleName}")
                    .ShouldNot()
                    .HaveDependencyOn($"Zss.BilliardHall.Modules.{other}.Domain")
                    .GetResult();

                Assert.True(result.IsSuccessful,
                $"❌ ADR-0001.5 违规: 模块 {moduleName} 不应依赖其他模块 {other} 的领域对象。\n" +
                $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
                $"修复建议：\n" +
                $"  1. 使用领域事件进行异步通信\n" +
                $"  2. 使用只读契约（Contracts）传递数据\n" +
                $"  3. 传递原始类型（Guid、string、int）而非领域对象\n" +
                $"参考：docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md");
            }
        }
    }

    [Fact(DisplayName = "ADR-0001.6: Contract 不应包含业务判断字段（语义分析）")]
    public void Contract_Business_Field_Analyzer()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var contractTypes = Types
            .InAssembly(platformAssembly)
            .That()
            .ResideInNamespace("Zss.BilliardHall.Platform.Contracts")
            .And()
            .AreClasses()
            .GetTypes();

        // 业务判断相关字段名称模式
        var businessDecisionPatterns = new[] {
            "CanRefund", "CanCancel", "IsValid", "IsEnabled", "IsActive",
            "ShouldApprove", "MustVerify", "AllowEdit"
        };

        foreach (var contractType in contractTypes)
        {
            if (contractType.IsAbstract || contractType.IsInterface)
                continue;

            var properties = contractType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var hasBusinessDecisionPattern = businessDecisionPatterns.Any(pattern => prop.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase));

                Assert.False(hasBusinessDecisionPattern,
                $"❌ ADR-0001.6 违规: Contract {contractType.Name} 包含疑似业务判断字段: {prop.Name}。\n" +
                $"修复建议：\n" +
                $"  1. Contract 应仅用于数据传递，不包含业务决策字段\n" +
                $"  2. 将业务判断逻辑移至 Handler 或领域模型中\n" +
                $"  3. 使用简单的数据字段（如状态枚举）而非判断字段\n" +
                $"参考：docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md\n" +
                $"注意：此为 L2/L3 级别检查，可能需要人工确认是否真的违规。");
            }
        }
    }

    #endregion

}
