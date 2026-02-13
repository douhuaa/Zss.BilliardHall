using Serilog;
using Zss.BilliardHall.Host.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

HostBootstrapper.ConfigureServices(builder);

var app = builder.Build();

HostBootstrapper.ConfigureApplication(app);
app.Run();
