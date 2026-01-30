using Xunit;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-940: ADR 关系与溯源管理
/// 验证 ADR 文档之间的关系声明双向一致性
/// 
/// 这是治理测试，不是工具脚本：
/// - ❌ 不返回 bool
/// - ❌ 不打印日志  
/// - ✅ 直接 Assert，失败即裁决
/// 
/// 优化说明：
/// - 使用 AdrTestFixture 统一加载 ADR 文档，避免重复代码
/// - 使用 AdrRelationshipValidator 验证关系，避免代码重复
/// - 使用参数化测试 (Theory) 减少重复的测试方法
/// </summary>
public sealed class AdrRelationshipConsistencyTests : IClassFixture<AdrTestFixture>
{
    private readonly IReadOnlyDictionary<string, AdrDocument> _adrs;

    public AdrRelationshipConsistencyTests(AdrTestFixture fixture)
    {
        _adrs = fixture.AllAdrs;
    }

    /// <summary>
    /// ADR-940.3: 验证所有双向关系的一致性
    /// 使用参数化测试减少重复代码
    /// </summary>
    [Theory(DisplayName = "ADR-940.3: ADR 关系必须双向一致")]
    [InlineData("DependsOn", "DependedBy")]
    [InlineData("Supersedes", "SupersededBy")]
    public void Bidirectional_Relationships_Must_Be_Consistent(
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

        Assert.Empty(violations);
    }
}
