using FluentAssertions;
using NetArchTest.Rules;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;
using static Zss.BilliardHall.Tests.ArchitectureTests.Shared.AssertionMessageBuilder;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_001;

/// <summary>
/// ADR-001_1: 模块物理隔离（Rule）
/// 验证模块按业务能力独立划分，物理隔离，禁止直接依赖
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-001_1_1: 模块按业务能力独立划分
/// - ADR-001_1_2: 项目文件禁止引用其他模块
/// - ADR-001_1_3: 命名空间匹配模块边界
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md
/// </summary>
public sealed class ADR_001_1_Architecture_Tests
{
    /// <summary>
    /// ADR-001_1_1: 模块按业务能力独立划分
    /// 验证模块不可相互引用代码、类型、资源（§ADR-001_1_1）
    /// </summary>
    [Theory(DisplayName = "ADR-001_1_1: 模块不应相互引用")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_001_1_1_Modules_Should_Not_Reference_Other_Modules(Assembly moduleAssembly)
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

            result.IsSuccessful.Should().BeTrue(
                BuildFromArchTestResult(
                    ruleId: "ADR-001_1_1",
                    summary: $"模块 {moduleName} 不应依赖模块 {other}",
                    failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
                    remediationSteps: new[]
                    {
                        "使用领域事件进行异步通信",
                        "使用数据契约进行只读查询",
                        "传递原始类型（Guid、string）而非领域对象"
                    },
                    adrReference: "docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md"));
        }
    }

    /// <summary>
    /// ADR-001_1_2: 项目文件禁止引用其他模块
    /// 验证项目文件（.csproj）不得包含对其他模块的 ProjectReference（§ADR-001_1_2）
    /// </summary>
    [Theory(DisplayName = "ADR-001_1_2: 模块项目文件不应引用其他模块")]
    [MemberData(nameof(GetModuleProjectFiles))]
    public void ADR_001_1_2_Module_Csproj_Should_Not_Reference_Other_Modules(string csprojPath)
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

            true.Should().BeFalse(
                Build(
                    ruleId: "ADR-001_1_2",
                    summary: $"模块 {projectName} 不应引用其他模块或非白名单项目: {refName}",
                    currentState: $"项目路径: {csprojPath}\n引用路径: {include}",
                    remediationSteps: new[]
                    {
                        "将共享代码移至 Platform/BuildingBlocks",
                        "使用领域事件进行模块间通信",
                        "使用消息总线传递数据而非直接依赖"
                    },
                    adrReference: "docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md"));
        }
    }

    /// <summary>
    /// ADR-001_1_3: 命名空间匹配模块边界
    /// 验证命名空间必须与模块边界一致，确保命名空间不跨模块（§ADR-001_1_3）
    /// </summary>
    [Theory(DisplayName = "ADR-001_1_3: 命名空间应匹配模块边界")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_001_1_3_Namespace_Should_Match_Module_Boundaries(Assembly moduleAssembly)
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

        result.IsSuccessful.Should().BeTrue(
            BuildFromArchTestResult(
                ruleId: "ADR-001_1_3",
                summary: $"模块 {moduleName} 的类型必须在命名空间 {expectedNamespace} 下",
                failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
                remediationSteps: new[]
                {
                    "确保所有类型都在正确的模块命名空间下",
                    "遵循目录结构与命名空间一致性原则",
                    "检查文件的物理位置是否与命名空间匹配"
                },
                adrReference: "docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md"));
    }

    // ========== 辅助方法 ==========

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
}
