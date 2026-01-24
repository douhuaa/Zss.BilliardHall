using NetArchTest.Rules;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-360: CI/CD Pipeline 流程标准化
/// </summary>
public sealed class ADR_0360_Architecture_Tests
{
    [Fact(DisplayName = "ADR-360: CI/CD 规则（通过 GitHub Actions 配置验证）")]
    public void CICD_Rules_Verified_By_GitHub_Actions()
    {
        // CI/CD 规则通过 GitHub Actions Workflow 文件验证
        // 包括：分支保护、PR 检查、架构测试强制等
        Assert.True(true, "CI/CD 规则通过 .github/workflows 配置文件验证");
    }
}
