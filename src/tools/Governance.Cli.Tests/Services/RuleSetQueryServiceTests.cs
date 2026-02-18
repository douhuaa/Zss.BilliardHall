using Zss.BilliardHall.Specification.Language.RuleIdLanguage;
using Zss.BilliardHall.Specification.Services;

namespace Zss.BilliardHall.Tools.Governance.Cli.Tests.Services;

/// <summary>
/// RuleSetQueryService 测试
/// </summary>
public sealed class RuleSetQueryServiceTests
{
    private readonly RuleSetQueryService _service = new();

    [Fact]
    public void GetRuleSetStrict_WithValidAdrNumber_ShouldReturnRuleSet()
    {
        // Act
        var ruleSet = _service.GetRuleSetStrict(1);

        // Assert
        ruleSet.Should().NotBeNull();
        ruleSet.AdrNumber.Should().Be(1);
    }

    [Fact]
    public void GetRuleSetStrict_WithInvalidAdrNumber_ShouldThrow()
    {
        // Act & Assert
        var act = () => _service.GetRuleSetStrict(99999);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetRuleSet_WithInvalidAdrNumber_ShouldReturnNull()
    {
        // Act
        var ruleSet = _service.GetRuleSet(99999);

        // Assert
        ruleSet.Should().BeNull();
    }

    [Fact]
    public void FormatRuleId_ShouldReturnCorrectFormat()
    {
        // Arrange
        var ruleId = ArchitectureRuleId.Rule(907, 3);

        // Act
        var formatted = _service.FormatRuleId(ruleId);

        // Assert
        formatted.Should().Be("ADR-907_3");
    }

    [Fact]
    public void CreateSummary_ShouldIncludeAllInformation()
    {
        // Arrange
        var ruleSet = _service.GetRuleSetStrict(1);

        // Act
        var summary = _service.CreateSummary(ruleSet);

        // Assert
        summary.AdrNumber.Should().Be(1);
        summary.FormattedId.Should().Be("ADR-001");
        summary.RuleCount.Should().BeGreaterThan(0);
        summary.ClauseCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetAllRuleSets_ShouldReturnMultipleRuleSets()
    {
        // Act
        var allRuleSets = _service.GetAllRuleSets().ToList();

        // Assert
        allRuleSets.Should().NotBeEmpty();
        allRuleSets.Should().Contain(rs => rs.AdrNumber == 1);
    }
}
