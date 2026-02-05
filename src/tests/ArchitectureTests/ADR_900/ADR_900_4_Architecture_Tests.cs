namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_900;

/// <summary>
/// ADR-900_4: 冲突裁决优先级
/// 验证架构规则冲突时的裁决优先级顺序
///
/// 测试覆盖映射（严格遵循 ADR-900 v4.0 Rule/Clause 体系）：
/// - ADR-900_4_1: 裁决优先级顺序 → ADR_900_4_1_Conflict_Resolution_Priorities
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-900-architecture-tests.md
/// </summary>
public sealed class ADR_900_4_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";

    /// <summary>
    /// ADR-900_4_1: 裁决优先级顺序
    /// 验证 ADR-900 定义了冲突裁决的优先级顺序
    /// </summary>
    [Fact(DisplayName = "ADR-900_4_1: 必须定义冲突裁决优先级")]
    public void ADR_900_4_1_Conflict_Resolution_Priorities()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr900File = Path.Combine(repoRoot, AdrDocsPath, "governance", "ADR-900-architecture-tests.md");

        // 验证 ADR-900 文件存在
        File.Exists(adr900File).Should().BeTrue(
            $"❌ ADR-900_4_1 违规：ADR-900 文档不存在\n\n" +
            $"预期路径：{adr900File}\n\n" +
            $"修复建议：\n" +
            $"  确保 ADR-900 文档位于正确位置\n\n" +
            $"参考：docs/adr/governance/ADR-900-architecture-tests.md §4.1");

        var content = File.ReadAllText(adr900File);

        // 验证包含优先级顺序定义
        content.Should().Contain("优先级",
            $"❌ ADR-900_4_1 违规：ADR-900 未定义冲突裁决优先级\n\n" +
            $"修复建议：\n" +
            $"  在 ADR-900 中定义架构规则冲突的裁决优先级顺序：\n" +
            $"  1. 架构安全与数据一致性\n" +
            $"  2. 系统稳定性与演进能力\n" +
            $"  3. 生命周期与资源安全\n" +
            $"  4. 结构一致性与可维护性\n" +
            $"  5. 流程与治理便利性\n\n" +
            $"参考：docs/adr/governance/ADR-900-architecture-tests.md §4.1");

        // 验证包含具体优先级列表
        var priorityItems = new[] { "安全", "稳定性", "生命周期", "结构一致性", "流程" };
        foreach (var item in priorityItems)
        {
            content.Should().Contain(item,
                $"❌ ADR-900_4_1 违规：ADR-900 未包含 '{item}' 优先级\n\n" +
                $"修复建议：\n" +
                $"  完善优先级顺序定义\n\n" +
                $"参考：docs/adr/governance/ADR-900-architecture-tests.md §4.1");
        }

        // 验证说明了低优先级规则可以被牺牲
        content.Should().Contain("牺牲",
            $"❌ ADR-900_4_1 违规：ADR-900 未说明低优先级规则的处理方式\n\n" +
            $"修复建议：\n" +
            $"  明确低优先级规则可以被临时牺牲，但必须记录破例\n\n" +
            $"参考：docs/adr/governance/ADR-900-architecture-tests.md §4.1");
    }
}
