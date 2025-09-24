using Xunit;

namespace Zss.BilliardHall.EntityFrameworkCore;

[CollectionDefinition(BilliardHallTestConsts.CollectionDefinitionName)]
public class BilliardHallEntityFrameworkCoreCollection : ICollectionFixture<BilliardHallEntityFrameworkCoreFixture>
{

}
