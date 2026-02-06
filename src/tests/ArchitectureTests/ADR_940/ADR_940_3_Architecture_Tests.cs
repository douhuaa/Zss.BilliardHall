namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-940_3: ADR 关系双向一致性验证（Rule）
/// 验证 ADR 文档之间的关系声明双向一致性
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-940_3_1: DependsOn 和 DependedBy 关系必须双向一致
/// - ADR-940_3_2: Supersedes 和 SupersededBy 关系必须双向一致
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-940-adr-relationship-and-traceability.md
///
/// 实现说明：
/// - 使用 AdrTestFixture 统一加载 ADR 文档，避免重复代码
/// - 使用 AdrRelationshipValidator 验证关系，避免代码重复
/// - 使用参数化测试 (Theory) 减少重复的测试方法
/// </summary>
public sealed class ADR_940_3_Architecture_Tests : IClassFixture<AdrTestFixture>
{
    private readonly IReadOnlyDictionary<string, AdrDocument> _adrs;

    public ADR_940_3_Architecture_Tests(AdrTestFixture fixture)
    {
        _adrs = fixture.AllAdrs;
    }

    /// <summary>
    /// ADR-940_3: 验证所有双向关系的一致性
    /// 使用参数化测试减少重复代码（§ADR-940_3_1, §ADR-940_3_2）
    /// </summary>
    [Theory(DisplayName = "ADR-940_3: ADR 关系必须双向一致")]
    [InlineData("DependsOn", "DependedBy")]
    [InlineData("Supersedes", "SupersededBy")]
    public void ADR_940_3_Bidirectional_Relationships_Must_Be_Consistent(
        string forwardRelation,
        string backwardRelation)
    {
        var violations = AdrRelationshipValidator.ValidateBidirectionalRelationship(
            _adrs,
            forwardRelation,
            backwardRelation,
            adr => AdrRelationshipValidator.GetRelationshipTargets(adr, forwardRelation),
            adr => AdrRelationshipValidator.GetRelationshipTargets(adr, backwardRelation)
        );

        violations.Should().BeEmpty(
            because: $"{forwardRelation} 和 {backwardRelation} 关系必须双向一致");
    }
}
