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
public sealed class ADR_122_Architecture_Tests
{
    [Theory(DisplayName = "ADR-122_1_1: 测试类必须以 Tests 结尾")]
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
                $"❌ ADR-122_1_1 违规: 测试类命名不符合规范\n\n" +
                $"违规类型：\n{violationNames}\n\n" +
                $"问题分析：\n" +
                $"测试类必须以 Tests 后缀结尾以保持命名一致性和可识别性\n\n" +
                $"修复建议：\n" +
                $"1. 将测试类重命名为 {{ClassName}}Tests 格式\n" +
                $"2. 示例：OrderServiceTests, MemberRepositoryTests\n" +
                $"3. 确保测试类与被测试类型的对应关系清晰\n\n" +
                $"参考：docs/adr/structure/ADR-122-test-organization-naming.md（§1.1）");
        }
    }

    [Fact(DisplayName = "ADR-122_1_2: 架构测试必须在专用项目中")]
    public void Architecture_Tests_Must_Be_In_Separate_Project()
    {
        var archTestAssembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "ArchitectureTests");

        (archTestAssembly != null).Should().BeTrue($"❌ ADR-122_1_2 违规：架构测试项目不存在\n\n" +
            $"问题分析：\n" +
            $"架构测试必须在专用的 ArchitectureTests 项目中组织，以与功能测试分离\n\n" +
            $"修复建议：\n" +
            $"1. 创建独立的 ArchitectureTests 项目\n" +
            $"2. 将所有架构测试迁移到该项目\n" +
            $"3. 使用 NetArchTest 或类似工具验证架构约束\n\n" +
            $"参考：docs/adr/structure/ADR-122-test-organization-naming.md（§1.2）");
    }

    [Theory(DisplayName = "ADR-122_1_3: 测试项目必须遵循命名约定")]
    [ClassData(typeof(TestAssemblyData))]
    public void Test_Projects_Must_Follow_Naming_Convention(Assembly testAssembly)
    {
        var assemblyName = testAssembly.GetName().Name;
        
        if (assemblyName == null)
            return;

        var isValid = assemblyName.EndsWith(".Tests") || 
                     assemblyName == "ArchitectureTests";

        isValid.Should().BeTrue($"❌ ADR-122_1_3 违规: 测试项目命名不符合规范\n\n" +
            $"违规项目：{assemblyName}\n\n" +
            $"问题分析：\n" +
            $"测试项目必须遵循统一的命名约定以保持项目结构清晰\n\n" +
            $"修复建议：\n" +
            $"1. 将项目重命名为 {{Module}}.Tests 格式\n" +
            $"2. 或使用特殊名称 ArchitectureTests 用于架构测试\n" +
            $"3. 示例：Orders.Tests, Members.Tests, ArchitectureTests\n" +
            $"4. 确保项目名称与被测试模块对应\n\n" +
            $"参考：docs/adr/structure/ADR-122-test-organization-naming.md（§1.3）");
    }
}

// 辅助类：获取所有测试程序集
public sealed class TestAssemblyData : TheoryData<Assembly>
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
