using Zss.BilliardHall.Books;
using Xunit;

namespace Zss.BilliardHall.EntityFrameworkCore.Applications.Books;

[Collection(BilliardHallTestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<BilliardHallEntityFrameworkCoreTestModule>
{

}