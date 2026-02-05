using NetArchTest.Rules;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-930: 代码审查与 ADR 合规自检流程
/// 参考：docs/adr/governance/ADR-930-code-review-compliance.md
///
/// 【测试覆盖映射】
/// ├─ ADR-930_1_1: PR 必须填写变更类型和影响范围 (L2) → PR_Template_Should_Include_Checklist
/// └─ Copilot 指令和 CI 集成 (L2) → Copilot_Instructions_Should_Exist, Architecture_Tests_Must_Be_In_CI
/// </summary>
public sealed class ADR_930_Architecture_Tests
{
    [Fact(DisplayName = "ADR-930_1_1: PR 模板应包含必要的自检清单")]
    public void PR_Template_Should_Include_Checklist()
    {
    }
    }
}
