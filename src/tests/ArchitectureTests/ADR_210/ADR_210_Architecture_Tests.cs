namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_210;

/// <summary>
/// ADR-210: 领域事件版本化与兼容性
/// 验证事件 SchemaVersion 属性、版本控制等规则
///
/// ADR 映射清单（ADR Mapping Checklist）：
/// ┌────────────────┬─────────────────────────────────────────────────────┬──────────┐
/// │ 测试方法         │ 对应 ADR 约束                                        │ RuleId   │
/// ├────────────────┼─────────────────────────────────────────────────────┼──────────┤
/// │ Events_Must_Have_SchemaVersion_Property              │ 事件必须包含 SchemaVersion 属性                    │ ADR-210_2_1 │
/// └────────────────┴─────────────────────────────────────────────────────┴──────────┘
///
/// 测试覆盖的 Rule：
/// - ADR-210_2：事件 SchemaVersion 要求（Rule）
/// </summary>
public sealed class ADR_210_Architecture_Tests
{
    [Theory(DisplayName = "ADR-210_2_1: 事件必须包含 SchemaVersion 属性")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Events_Must_Have_SchemaVersion_Property(Assembly moduleAssembly)
    {
        var eventTypes = Types
            .InAssembly(moduleAssembly)
            .That()
            .ResideInNamespace("DomainEvents")
            .Or()
            .HaveNameEndingWith("Event")
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var eventType in eventTypes)
        {
            var hasSchemaVersion = eventType.GetProperty("SchemaVersion") != null;

            hasSchemaVersion.Should().BeTrue($"❌ ADR-210_2_1 违规: 事件缺少 SchemaVersion 属性\n\n" +
                $"违规类型：{eventType.FullName}\n\n" +
                $"问题分析：\n" +
                $"领域事件必须包含 SchemaVersion 属性以支持版本化和向后兼容\n\n" +
                $"修复建议：\n" +
                $"1. 添加 SchemaVersion 属性到事件定义：\n" +
                $"   public string SchemaVersion {{ get; init; }} = \"1.0\";\n" +
                $"2. 使用语义化版本号（如 \"1.0\", \"2.0\", \"2.1\"）\n" +
                $"3. 当事件结构发生变化时，递增版本号\n" +
                $"4. 保持旧版本的兼容性处理代码\n\n" +
                $"参考：docs/adr/runtime/ADR-210-event-versioning-compatibility.md（§ADR-210_2_1）");
        }
    }
}
