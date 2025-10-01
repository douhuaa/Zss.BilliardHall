using Microsoft.Extensions.Hosting;
var builder = DistributedApplication.CreateBuilder(args);

const string dbConnectionName = "Default";


var postgres = builder
    .AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var db = postgres.AddDatabase("BilliardHallDb");

if (builder.Environment.IsDevelopment())
{
    builder
        .AddProject<Projects.Zss_BilliardHall_DbMigrator>("dbMigrator")
        .WithReference(db, dbConnectionName)
        .WaitFor(postgres);
}

builder
    .AddProject<Projects.Zss_BilliardHall_HttpApi_Host>("httpApi-host")
    .WithReference(db, dbConnectionName)
    .WaitFor(postgres);

builder
    .Build()
    .Run();
