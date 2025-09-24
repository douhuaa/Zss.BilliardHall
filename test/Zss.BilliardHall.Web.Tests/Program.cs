using Microsoft.AspNetCore.Builder;
using Zss.BilliardHall;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
await builder.RunAbpModuleAsync<BilliardHallWebTestModule>();

public partial class Program
{
}
