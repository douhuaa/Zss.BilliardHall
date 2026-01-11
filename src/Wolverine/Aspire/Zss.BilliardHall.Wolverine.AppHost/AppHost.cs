var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("billiard-hall-db");

builder.AddProject<Projects.Bootstrapper>("bootstrapper").WithReference(db).WaitFor(db);

builder.Build().Run();
