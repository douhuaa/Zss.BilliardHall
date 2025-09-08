using Projects;
var builder = DistributedApplication.CreateBuilder(args);

// Redis
var redis = builder
    .AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);

// Mysql
var mysql = builder
    .AddMySql("mysql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var db = mysql.AddDatabase("db");

builder
    .AddProject<Zss_BilliardHall_Blazor>("zss-billiardhall-blazor")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(db, "Default")
    .WaitFor(db);

builder
    .AddProject<Zss_BilliardHall_DbMigrator>("zss-billiardhall-dbmigrator")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(db, "Default")
    .WaitFor(db);

builder
    .Build()
    .Run();
