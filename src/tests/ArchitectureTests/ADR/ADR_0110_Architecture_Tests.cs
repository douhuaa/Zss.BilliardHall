using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0110: 测试目录组织规范
/// 验证测试代码的目录组织符合规范
/// </summary>
public sealed class ADR_0110_Architecture_Tests
{
    [Fact(DisplayName = "ADR-0110.1: 架构测试类必须在 ArchitectureTests.ADR 命名空间")]
    public void Architecture_Test_Classes_Must_Be_In_ADR_Namespace()
    {
        var testAssembly = typeof(ADR_0110_Architecture_Tests).Assembly;
        var archTestTypes = testAssembly.GetTypes()
            .Where(t => t.Name.StartsWith("ADR_") && t.Name.Contains("Architecture_Tests"))
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var testType in archTestTypes)
        {
            Assert.True(testType.Namespace?.Contains(".ADR") == true,
                $"❌ ADR-0110 违规：架构测试类 {testType.Name} 必须在 ADR 命名空间中。\n" +
                $"当前命名空间：{testType.Namespace}\n" +
                $"正确命名空间：Zss.BilliardHall.Tests.ArchitectureTests.ADR\n" +
                $"修复建议：将测试类移动到 tests/ArchitectureTests/ADR/ 目录。");
        }
    }

    [Fact(DisplayName = "ADR-0110.2: ADR 测试类命名必须符合 ADR_xxxx_Architecture_Tests 格式")]
    public void ADR_Test_Classes_Must_Follow_Naming_Convention()
    {
        var testAssembly = typeof(ADR_0110_Architecture_Tests).Assembly;
        var adrTestTypes = testAssembly.GetTypes()
            .Where(t => t.Name.StartsWith("ADR_") && t.Name.Contains("Architecture_Tests"))
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        var invalidNames = new List<string>();
        var pattern = @"^ADR_\d{4}_Architecture_Tests$";

        foreach (var testType in adrTestTypes)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(testType.Name, pattern))
            {
                invalidNames.Add(testType.Name);
            }
        }

        Assert.True(invalidNames.Count == 0,
            $"❌ ADR-0110 违规：以下 ADR 测试类命名不符合规范：\n" +
            $"{string.Join("\n", invalidNames)}\n" +
            $"正确格式：ADR_{{编号:D4}}_Architecture_Tests\n" +
            $"示例：ADR_0001_Architecture_Tests, ADR_0110_Architecture_Tests\n" +
            $"修复建议：重命名测试类文件和类名以符合规范。");
    }

    [Fact(DisplayName = "ADR-0110.3: 测试方法 DisplayName 必须包含 ADR 编号")]
    public void Test_Methods_Must_Include_ADR_Number_In_DisplayName()
    {
        var testAssembly = typeof(ADR_0110_Architecture_Tests).Assembly;
        var adrTestTypes = testAssembly.GetTypes()
            .Where(t => t.Name.StartsWith("ADR_") && t.Name.Contains("Architecture_Tests"))
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        var violations = new List<string>();

        foreach (var testType in adrTestTypes)
        {
            var match = System.Text.RegularExpressions.Regex.Match(testType.Name, @"ADR_(\d{4})_");
            if (!match.Success) continue;
            
            var adrNumber = match.Groups[1].Value;
            var expectedPrefix = $"ADR-{adrNumber}";

            var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any() ||
                           m.GetCustomAttributes(typeof(TheoryAttribute), false).Any())
                .ToList();

            foreach (var method in testMethods)
            {
                var factAttr = method.GetCustomAttributes(typeof(FactAttribute), false).FirstOrDefault() as FactAttribute;
                var theoryAttr = method.GetCustomAttributes(typeof(TheoryAttribute), false).FirstOrDefault() as TheoryAttribute;
                
                var displayName = factAttr?.DisplayName ?? theoryAttr?.DisplayName;
                
                if (string.IsNullOrEmpty(displayName) || !displayName.StartsWith(expectedPrefix))
                {
                    violations.Add($"{testType.Name}.{method.Name}: DisplayName='{displayName}' 应以 '{expectedPrefix}' 开头");
                }
            }
        }

        Assert.True(violations.Count == 0,
            $"❌ ADR-0110 违规：以下测试方法的 DisplayName 未包含正确的 ADR 编号：\n" +
            $"{string.Join("\n", violations)}\n" +
            $"修复建议：确保 DisplayName 格式为 'ADR-{{编号}}.{{子编号}}: {{约束描述}}'。");
    }
}
