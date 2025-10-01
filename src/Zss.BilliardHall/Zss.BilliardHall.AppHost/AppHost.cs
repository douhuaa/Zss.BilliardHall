var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Zss_BilliardHall_DbMigrator>("zss-billiardhall-dbmigrator");

builder.AddProject<Projects.Zss_BilliardHall_HttpApi_Host>("zss-billiardhall-httpapi-host");

builder.Build().Run();
