namespace Zss.BilliardHall.Modules.Members.Events;

public record MemberTierUpgraded(Guid MemberId, MemberTier PreviousTier, MemberTier CurrentTier);