using Microsoft.Extensions.Hosting;
using Aspire.Hosting.ApplicationModel; // 访问 ParameterResource 以设置初始值
// AppHost: 集中描述基础设施资源与内部服务编排。
// Postgres 凭据策略（设计文档化实现）：
//  1. Aspire 默认会为 Postgres 生成随机用户/密码并通过连接字符串引用。
//  2. 若需固定开发凭据，可通过环境变量 BH_DB_USER / BH_DB_PASSWORD 或 docker 相关设置（未来可扩展）。
//  3. 生产场景请在部署编排层（如容器编排平台 / Secret 管理）覆盖，而不是硬编码在此文件。
//  4. 详细流程参见 docs/08_配置管理/Secrets管理.md。
var builder = DistributedApplication.CreateBuilder(args);

const string dbConnectionName = "Default";

// 参数资源：Postgres 用户/密码（可在 Aspire Dashboard 或环境中赋值）
// 映射策略：
//  1. 若存在环境变量 BH_DB_USER / BH_DB_PASSWORD，则作为初始值写入参数（仅在进程启动时赋值，不覆盖 Dashboard 手工修改）。
//  2. 若不存在则留空，采用镜像/默认逻辑或在 Dashboard 中补充。
var envUser = Environment.GetEnvironmentVariable("BH_DB_USER");
var envPassword = Environment.GetEnvironmentVariable("BH_DB_PASSWORD");

var postgresUserParam = string.IsNullOrWhiteSpace(envUser)
    ? builder.AddParameter("postgres-user")
    : builder.AddParameter("postgres-user", value: envUser);

var postgresPasswordParam = string.IsNullOrWhiteSpace(envPassword)
    ? builder.AddParameter("postgres-password", secret: true)
    : builder.AddParameter("postgres-password", value: envPassword, secret: true);

var postgres = builder
    .AddPostgres("postgres", userName: postgresUserParam, password: postgresPasswordParam)
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
