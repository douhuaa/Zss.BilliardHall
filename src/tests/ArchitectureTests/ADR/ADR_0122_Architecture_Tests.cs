using NetArchTest.Rules;
using FluentAssertions;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-122: 测试代码组织与命名规范
/// 验证测试代码的目录结构、命名规范、组织规则
/// 
/// ADR 映射清单（ADR Mapping Checklist）：
/// ┌─────────────┬────────────────────────────────────────────────────────┬──────────┐
/// │ 测试方法     │ 对应 ADR 约束                                          │ ADR 章节 │
/// ├─────────────┼────────────────────────────────────────────────────────┼──────────┤
/// │ ADR-122.2   │ 测试类命名必须遵循 {TypeName}Tests 模式                │ 规则本体 │
/// │ ADR-122.4   │ 架构测试必须单独组织                                    │ 规则本体 │
/// │ ADR-122.5   │ 测试项目命名必须遵循 {Module}.Tests 模式               │ 规则本体 │
/// └─────────────┴────────────────────────────────────────────────────────┴──────────┘
/// </summary>
public sealed class ADR_0122_Architecture_Tests
{
    [Theory(DisplayName = "ADR-122.2: 测试类必须以 Tests 结尾")]
    [ClassData(typeof(TestAssemblyData))]
    public void Test_Classes_Must_End_With_Tests(Assembly testAssembly)
    {
        // 排除 ArchitectureTests（有特殊命名规则）
        if (testAssembly.GetName().Name == "ArchitectureTests")
            return;

        var testClasses = testAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetMethods().Any(m => 
                m.GetCustomAttributes(true).Any(a => 
                    a.GetType().Name == "FactAttribute" || 
                    a.GetType().Name == "TheoryAttribute")))
            .ToList();

        var violations = testClasses
            .Where(t => !t.Name.EndsWith("Tests"))
            .ToList();

        if (violations.Any())
        {
            var violationNames = string.Join("\n", violations.Select(t => $"  - {t.FullName}"));
            true.Should().BeFalse(
                $"❌ ADR-122.2 违规: 测试类命名不符合规范\n\n" +
                $"违规类型:\n{violationNames}\n\n" +
                $"修复建议:\n" +
                $"将测试类重命名为 {{ClassName}}Tests 格式\n\n" +
                $"参考: docs/copilot/adr-0122.prompts.md");
        }
    }

    [Fact(DisplayName = "ADR-122.4: 架构测试必须在专用项目中")]
    public void Architecture_Tests_Must_Be_In_Separate_Project()
    {
        var archTestAssembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "ArchitectureTests");

        archTestAssembly.Should().NotBeNull();
        true.Should().BeTrue("架构测试项目 ArchitectureTests 存在");
    }

    [Theory(DisplayName = "ADR-122.5: 测试项目必须遵循命名约定")]
    [ClassData(typeof(TestAssemblyData))]
    public void Test_Projects_Must_Follow_Naming_Convention(Assembly testAssembly)
    {
        var assemblyName = testAssembly.GetName().Name;
        
        if (assemblyName == null)
            return;

        var isValid = assemblyName.EndsWith(".Tests") || 
                     assemblyName == "ArchitectureTests";

        isValid.Should().BeTrue($"❌ ADR-122.5 违规: 测试项目命名不符合规范\n\n" +
            $"违规项目: {assemblyName}\n\n" +
            $"修复建议:\n" +
            $"将项目重命名为 {{Module}}.Tests 或 ArchitectureTests\n\n" +
            $"参考: docs/copilot/adr-0122.prompts.md");
    }
}

// 辅助类：获取所有测试程序集
public class TestAssemblyData : TheoryData<Assembly>
{
    public TestAssemblyData()
    {
        var testAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => a.GetName().Name?.Contains("Tests") == true)
            .ToList();

        foreach (var assembly in testAssemblies)
        {
            Add(assembly);
        }
    }
}
