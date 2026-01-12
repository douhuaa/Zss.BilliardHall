using FluentAssertions;
using Xunit;
using Zss.BilliardHall.Modules.Members;

namespace Zss.BilliardHall.Wolverine.Bootstrapper.Tests.Members;

/// <summary>
/// Member 领域模型单元测试
/// Member domain model unit tests
/// </summary>
public class MemberTests
{
    [Fact]
    public void TopUp_WithPositiveAmount_ShouldIncreaseBalance()
    {
        // Arrange
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Balance = 100m
        };

        // Act
        var result = member.TopUp(50m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        member.Balance.Should().Be(150m);
    }

    [Fact]
    public void TopUp_WithZeroAmount_ShouldReturnFailure()
    {
        // Arrange
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Balance = 100m
        };

        // Act
        var result = member.TopUp(0m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("充值金额必须大于0");
        member.Balance.Should().Be(100m); // Balance unchanged
    }

    [Fact]
    public void TopUp_WithNegativeAmount_ShouldReturnFailure()
    {
        // Arrange
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Balance = 100m
        };

        // Act
        var result = member.TopUp(-50m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("充值金额必须大于0");
        member.Balance.Should().Be(100m); // Balance unchanged
    }

    [Fact]
    public void Deduct_WithSufficientBalance_ShouldDecreaseBalance()
    {
        // Arrange
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Balance = 100m
        };

        // Act
        var result = member.Deduct(30m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        member.Balance.Should().Be(70m);
    }

    [Fact]
    public void Deduct_WithInsufficientBalance_ShouldReturnFailure()
    {
        // Arrange
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Balance = 100m
        };

        // Act
        var result = member.Deduct(150m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("余额不足");
        member.Balance.Should().Be(100m); // Balance unchanged
    }

    [Fact]
    public void Deduct_WithZeroAmount_ShouldReturnFailure()
    {
        // Arrange
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Balance = 100m
        };

        // Act
        var result = member.Deduct(0m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("扣减金额必须大于0");
        member.Balance.Should().Be(100m); // Balance unchanged
    }

    [Fact]
    public void AwardPoints_WithPositivePoints_ShouldIncreasePoints()
    {
        // Arrange
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Points = 100,
            Tier = MemberTier.Regular
        };

        // Act
        var result = member.AwardPoints(50);

        // Assert
        result.IsSuccess.Should().BeTrue();
        member.Points.Should().Be(150);
    }

    [Fact]
    public void AwardPoints_WithZeroPoints_ShouldReturnFailure()
    {
        // Arrange
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Points = 100
        };

        // Act
        var result = member.AwardPoints(0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("积分必须大于0");
        member.Points.Should().Be(100); // Points unchanged
    }

    [Theory]
    [InlineData(100, MemberTier.Regular)]
    [InlineData(500, MemberTier.Regular)]
    [InlineData(999, MemberTier.Regular)]
    [InlineData(1000, MemberTier.Silver)]
    [InlineData(2500, MemberTier.Silver)]
    [InlineData(4999, MemberTier.Silver)]
    [InlineData(5000, MemberTier.Gold)]
    [InlineData(7500, MemberTier.Gold)]
    [InlineData(9999, MemberTier.Gold)]
    [InlineData(10000, MemberTier.Platinum)]
    [InlineData(15000, MemberTier.Platinum)]
    public void AwardPoints_ShouldAutomaticallyUpgradeTier(int totalPoints, MemberTier expectedTier)
    {
        // Arrange
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Points = 0,
            Tier = MemberTier.Regular
        };

        // Act
        var result = member.AwardPoints(totalPoints);

        // Assert
        result.IsSuccess.Should().BeTrue();
        member.Points.Should().Be(totalPoints);
        member.Tier.Should().Be(expectedTier);
    }

    [Fact]
    public void AwardPoints_ShouldNotDowngradeTier()
    {
        // Arrange - member already at Gold tier
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Points = 5000,
            Tier = MemberTier.Gold
        };

        // Act - award only a few points (still in Gold range)
        var result = member.AwardPoints(100);

        // Assert - tier should remain Gold
        result.IsSuccess.Should().BeTrue();
        member.Points.Should().Be(5100);
        member.Tier.Should().Be(MemberTier.Gold);
    }

    [Fact]
    public void AwardPoints_FromRegularToPlatinum_ShouldUpgradeInOneStep()
    {
        // Arrange
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Points = 0,
            Tier = MemberTier.Regular
        };

        // Act - award enough points to jump to Platinum
        var result = member.AwardPoints(10000);

        // Assert
        result.IsSuccess.Should().BeTrue();
        member.Points.Should().Be(10000);
        member.Tier.Should().Be(MemberTier.Platinum);
    }
}
