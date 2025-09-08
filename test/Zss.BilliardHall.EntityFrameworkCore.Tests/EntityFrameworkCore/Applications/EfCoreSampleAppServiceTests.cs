using Zss.BilliardHall.Samples;

using Xunit;

namespace Zss.BilliardHall.EntityFrameworkCore.Applications;

[Collection(BilliardHallTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<BilliardHallEntityFrameworkCoreTestModule>
{

}
