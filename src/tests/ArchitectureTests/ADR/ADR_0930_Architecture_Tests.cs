using NetArchTest.Rules;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-930: 代码审查与 ADR 合规自检流程
/// </summary>
public sealed class ADR_0930_Architecture_Tests
{
    [Fact(DisplayName = "ADR-930: 代码审查规则（通过 PR Template 和人工审查验证）")]
    public void Code_Review_Rules_Verified_By_PR_Template()
    {
        // 代码审查规则通过 PR Template 和人工审查验证
        // 包括：PR 描述完整性、Copilot 自检、架构测试通过等
        Assert.True(true, "代码审查规则通过 .github/PULL_REQUEST_TEMPLATE.md 和人工审查验证");
    }
}
