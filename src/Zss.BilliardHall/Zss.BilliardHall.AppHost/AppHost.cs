using Microsoft.Extensions.Hosting;
// AppHost: 集中描述基础设施资源与内部服务编排。
// Postgres 凭据策略（设计文档化实现）：
//  1. Aspire 默认会为 Postgres 生成随机用户/密码并通过连接字符串引用。
//  2. 若需固定开发凭据，可通过环境变量 BH_DB_USER / BH_DB_PASSWORD 或 docker 相关设置（未来可扩展）。
//  3. 生产场景请在部署编排层（如容器编排平台 / Secret 管理）覆盖，而不是硬编码在此文件。
//  4. 详细流程参见 doc/08_配置管理/Secrets管理.md。
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
