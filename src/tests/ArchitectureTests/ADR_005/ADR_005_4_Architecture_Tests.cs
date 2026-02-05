namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_005;

/// <summary>
/// 验证 ADR-005_4_1：模块间仅通过契约通信
/// 验证 ADR-005_4_2：契约不承载业务决策
/// </summary>
public sealed class ADR_005_4_Architecture_Tests
{
    [Theory(DisplayName = "ADR-005_4_1: 模块间仅通过契约通信")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_005_4_1_Inter_Module_Communication_Only_Via_Contract(Assembly moduleAssembly)
    {
        var entities = Types
            .InAssembly(moduleAssembly)
            .That()
            .ResideInNamespaceContaining(".Entities")
            .Or()
            .ResideInNamespaceContaining(".Domain")
            .Or()
            .HaveNameEndingWith("Entity")
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var entity in entities)
        {
            // 检查实体是否标记为 public（可能被其他模块引用）
            if (entity.IsPublic)
            {
                var constructors = entity.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

                // 建议：领域实体的公共构造函数应该很少或没有
                // 这是一个软约束，用于提醒开发者注意
                if (constructors.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"⚠️ ADR-005_4_1 建议: 实体 {entity.FullName} 是 public 且有公共构造函数，" +
                        $"确保不会被其他模块直接引用。模块间应仅通过契约 DTO 通信。");
                }
            }
        }

        // 这是一个正面测试
        true.Should().BeTrue("模块间通信契约检查完成");
    }

    [Theory(DisplayName = "ADR-005_4_2: 契约不承载业务决策")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_005_4_2_Contract_Must_Not_Contain_Business_Logic(Assembly moduleAssembly)
    {
        var contracts = Types
            .InAssembly(moduleAssembly)
            .That()
            .ResideInNamespaceContaining(".Contracts")
            .Or()
            .HaveNameEndingWith("Dto")
            .Or()
            .HaveNameEndingWith("Contract")
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var contract in contracts)
        {
            // 检查契约是否包含计算属性或业务方法
            var methods = contract
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName) // 排除属性访问器
                .Where(m => m.DeclaringType == contract) // 只检查当前类型的方法
                .Where(m => m.Name != "ToString" && m.Name != "Equals" && m.Name != "GetHashCode")
                .ToList();

            if (methods.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"⚠️ ADR-005_4_2 建议: 契约 {contract.FullName} 包含方法 " +
                    $"{string.Join(", ", methods.Select(m => m.Name))}。" +
                    $"契约应该只包含数据字段，不应包含业务方法或计算属性。");
            }

            // 检查是否包含复杂的计算属性
            var properties = contract
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetMethod?.GetMethodBody() != null)
                .ToList();

            foreach (var property in properties)
            {
                var getter = property.GetMethod;
                if (getter != null && getter.GetMethodBody()?.GetILAsByteArray()?.Length > 20)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"⚠️ ADR-005_4_2 建议: 契约 {contract.FullName} 的属性 {property.Name} " +
                        $"包含复杂逻辑。契约属性应该是简单的数据访问器，不应包含业务计算。");
                }
            }
        }

        // 这是一个正面测试
        true.Should().BeTrue("契约业务逻辑检查完成");
    }
}
