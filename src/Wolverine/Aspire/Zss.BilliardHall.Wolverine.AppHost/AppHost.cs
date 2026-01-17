// 台球厅管理系统 - Aspire 编排主机
// 职责：定义和编排应用的所有资源（服务、容器、数据库等）
// 启动方式：设为启动项目，按 F5 运行，或命令行 `dotnet run`
// Dashboard：自动启动在 https://localhost:17001（可在 Resources、Logs、Traces、Metrics 查看监控）

var builder = DistributedApplication.CreateBuilder(args);

// 1. 定义 PostgreSQL 容器
// - 服务名称：postgres（可通过 http+https://postgres 访问）
// - 数据持久化：使用 Docker Volume，避免重启丢失数据
// - 容器生命周期：Persistent（持久化，不随 AppHost 退出而销毁）
// - 固定端口：5432（便于本地工具连接）
var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()                               // 数据存储到本地 Docker Volume
    .WithLifetime(ContainerLifetime.Persistent)     // 容器持久化（开发时保留数据）
    .WithPgAdmin();                                 // 添加 pgAdmin 管理工具

// 2. 定义数据库
// - 数据库名：Default（统一连接字符串键）
// - 连接字符串会自动注入到引用此数据库的服务（通过 ConnectionStrings:Default 配置键）
var db = postgres.AddDatabase("Default");

// 3. 定义应用服务：Bootstrapper
// - 服务名称：bootstrapper（可通过 http+https://bootstrapper 访问）
// - WithReference(db)：自动注入数据库连接字符串到服务的 Configuration
// - WaitFor(db)：确保数据库容器完全就绪后再启动服务（避免启动失败）
builder.AddProject<Projects.Bootstrapper>("bootstrapper")
    .WithReference(db)                              // 注入连接字符串
    .WaitFor(db);                                   // 依赖等待

// 4. 构建并运行应用
// - 启动所有定义的资源（容器、服务）
// - 启动 Aspire Dashboard（监控面板）
// - 配置服务发现（自动解析 http+https://service-name）
builder.Build().Run();
