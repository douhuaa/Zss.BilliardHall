using Zss.BilliardHall.Application;
using Zss.BilliardHall.Platform;

var builder = WebApplication.CreateBuilder(args);

PlatformBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);
ApplicationBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();

app.Run();
