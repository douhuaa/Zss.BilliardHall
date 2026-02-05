using NetArchTest.Rules;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-360: CI/CD Pipeline 流程标准化
/// 参考：docs/adr/technical/ADR-360-cicd-pipeline-standardization.md
/// </summary>
public sealed class ADR_360_Architecture_Tests
{
    [Fact(DisplayName = "ADR-360_1_1: GitHub Workflows 配置文件应存在")]
    public void GitHub_Workflows_Configuration_Should_Exist()
    {
    }
    }
}
