using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Zss.BilliardHall.Pages;

[Collection(BilliardHallTestConsts.CollectionDefinitionName)]
public class Index_Tests : BilliardHallWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
