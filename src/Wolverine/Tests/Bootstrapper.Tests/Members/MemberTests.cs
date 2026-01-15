using FluentAssertions;
using Xunit;
using Zss.BilliardHall.BuildingBlocks.Exceptions;
using Zss.BilliardHall.Modules.Members;

namespace Zss.BilliardHall.Wolverine.Bootstrapper.Tests.Members;

/// <summary>
/// Member 领域模型单元测试
/// Member domain model unit tests
/// </summary>
public class MemberTests
{
    private static Member CreateDefaultMember(
        decimal balance = 100m,
        int points = 0,
        MemberTier tier = MemberTier.Regular
    )
    {
        return Member.Register("测试会员", "13800138003", string.Empty, tier, balance, points);
    }

    [Fact]
    public void TopUp_WithPositiveAmount_ShouldIncreaseBalance()
    {
        var member = CreateDefaultMember(balance: 100m);

        member.TopUp(50m);

        member.Balance.Should().Be(150m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void TopUp_WithNonPositiveAmount_ShouldThrowDomainException(decimal amount)
    {
        var member = CreateDefaultMember(balance: 100m);

        var act = () => member.TopUp(amount);

        act.Should().Throw<DomainException>()
            .Where(ex => ex.Code == MemberErrorCodes.InvalidTopUpAmount.Code);
        member.Balance.Should().Be(100m);
    }

    [Fact]
    public void Deduct_WithSufficientBalance_ShouldDecreaseBalance()
    {
        var member = CreateDefaultMember(balance: 100m);

        member.Deduct(30m);

        member.Balance.Should().Be(70m);
    }

    [Fact]
    public void Deduct_WithInsufficientBalance_ShouldThrowDomainException()
    {
        var member = CreateDefaultMember(balance: 100m);

        var act = () => member.Deduct(130m);

        act.Should().Throw<DomainException>()
            .Where(ex => ex.Code == MemberErrorCodes.InsufficientBalance.Code);
        member.Balance.Should().Be(100m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Deduct_WithNonPositiveAmount_ShouldThrowDomainException(decimal amount)
    {
        var member = CreateDefaultMember(balance: 100m);

        var act = () => member.Deduct(amount);

        act.Should().Throw<DomainException>()
            .Where(ex => ex.Code == MemberErrorCodes.InvalidDeductAmount.Code);
        member.Balance.Should().Be(100m);
    }

    [Fact]
    public void AwardPoints_WithPositivePoints_ShouldIncreasePoints()
    {
        var member = CreateDefaultMember(points: 100);

        member.AwardPoints(50);

        member.Points.Should().Be(150);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AwardPoints_WithNonPositivePoints_ShouldThrowDomainException(int points)
    {
        var member = CreateDefaultMember(points: 100);

        var act = () => member.AwardPoints(points);

        act.Should().Throw<DomainException>()
            .Where(ex => ex.Code == MemberErrorCodes.InvalidAwardPoints.Code);
        member.Points.Should().Be(100);
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
    public void AwardPoints_ShouldAutomaticallyUpgradeTier(int awardedPoints, MemberTier expectedTier)
    {
        var member = CreateDefaultMember(points: 0, tier: MemberTier.Regular);

        member.AwardPoints(awardedPoints);

        member.Points.Should().Be(awardedPoints);
        member.Tier.Should().Be(expectedTier);
    }

    [Fact]
    public void AwardPoints_ShouldNotDowngradeTier()
    {
        var member = CreateDefaultMember(points: 5000, tier: MemberTier.Gold);

        member.AwardPoints(100);

        member.Points.Should().Be(5100);
        member.Tier.Should().Be(MemberTier.Gold);
    }

    [Fact]
    public void AwardPoints_FromRegularToPlatinum_ShouldUpgradeInOneStep()
    {
        var member = CreateDefaultMember(points: 0, tier: MemberTier.Regular);

        member.AwardPoints(10000);

        member.Points.Should().Be(10000);
        member.Tier.Should().Be(MemberTier.Platinum);
    }

    [Fact]
    public void Register_ShouldCreateMemberWithProvidedInitialValues()
    {
        var member = Member.Register(
            "测试会员",
            "13800138003",
            "test@example.com",
            tier: MemberTier.Gold,
            balance: 12.34m,
            points: 567
        );

        member.Id.Should().NotBeEmpty();
        member.Name.Should().Be("测试会员");
        member.Phone.Should().Be("13800138003");
        member.Email.Should().Be("test@example.com");
        member.Tier.Should().Be(MemberTier.Gold);
        member.Balance.Should().Be(12.34m);
        member.Points.Should().Be(567);
    }

    [Theory]
    [InlineData(999, MemberTier.Regular)]
    [InlineData(1000, MemberTier.Silver)]
    [InlineData(4999, MemberTier.Silver)]
    [InlineData(5000, MemberTier.Gold)]
    [InlineData(9999, MemberTier.Gold)]
    [InlineData(10000, MemberTier.Platinum)]
    public void AwardPoints_Boundaries_ShouldSetExpectedTier(int startingPoints, MemberTier expectedTier)
    {
        var member = CreateDefaultMember(points: 0, tier: MemberTier.Regular);

        member.AwardPoints(startingPoints);

        member.Points.Should().Be(startingPoints);
        member.Tier.Should().Be(expectedTier);
    }

    [Fact]
    public void AwardPoints_WhenAlreadyPlatinum_ShouldRemainPlatinum()
    {
        var member = CreateDefaultMember(points: 10000, tier: MemberTier.Platinum);

        member.AwardPoints(1);

        member.Tier.Should().Be(MemberTier.Platinum);
        member.Points.Should().Be(10001);
    }
}
