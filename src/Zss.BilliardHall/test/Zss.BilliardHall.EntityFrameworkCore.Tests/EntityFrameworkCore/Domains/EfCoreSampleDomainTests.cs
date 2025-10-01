using Zss.BilliardHall.Samples;
using Xunit;

namespace Zss.BilliardHall.EntityFrameworkCore.Domains;

[Collection(BilliardHallTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<BilliardHallEntityFrameworkCoreTestModule>
{

}
