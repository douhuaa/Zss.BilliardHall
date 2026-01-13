# AppHost 说明

AppHost 通过 .NET Aspire 的 DistributedApplication 描述项目的基础设施与内部服务编排。

## 当前包含的资源

- PostgreSQL 数据库服务器（带持久化数据卷 `postgres`）
- `BilliardHallDb` 逻辑数据库（供 EF Core 使用）
- HttpApi Host 服务（始终启动）
- DbMigrator（仅在 Development 环境启动，用于迁移初始化）

## PostgreSQL 凭据与连接策略

- 默认行为：未显式指定用户/密码时，Aspire 会自动生成参数并写入依赖服务的连接字符串中。

- 参数资源（当前已启用）：`postgres-user` / `postgres-password` 作为 Aspire Parameter 资源被绑定到 Postgres 服务；可在 Aspire Dashboard 中填写或用环境变量注入；未填写则使用镜像默认策略（或随机）。

  *启动映射逻辑*：若进程启动前已设置 `BH_DB_USER` / `BH_DB_PASSWORD`，则会作为参数初始值创建（后续在 Dashboard 中可再修改，不会被覆盖）。

- 固定开发凭据（环境变量快速方式，可选）：预留 `BH_DB_USER` / `BH_DB_PASSWORD`；可在运行前导出：

     ```powershell
     $env:BH_DB_USER="billiard_dev"; $env:BH_DB_PASSWORD="DevPassword123!"
     ```

- 生产环境：请使用部署平台的 Secret/Config 机制（如 Kubernetes Secret、Docker Swarm secrets、Azure Key Vault 等）并在对应的容器编排中注入，而不要把凭据固化在源码。

- 用户机密 (User Secrets)：适合本地开发存放连接字符串覆盖值，详见 `docs/08_配置管理/Secrets管理.md`。

## 数据持久化策略

- 使用 `.WithDataVolume()` 为 Postgres 容器创建数据卷，确保容器重启后数据不丢失。
- 如需自定义卷名称或迁移生产，建议：
  - 在生产编排中显式声明命名卷或挂载持久化存储（PVC / EBS / NFS）。
  - 建立定期备份策略（逻辑备份 pg_dump + 物理备份 WAL 归档）。

## 后续可扩展方向

- 增加 `AddContainer("otel-collector")` 统一导出 traces / metrics / logs。
- 为 Postgres 增加健康检查端点和 readiness 依赖超时配置。
- 引入参数资源：`var userParam = builder.AddParameter("postgres-user", secret: false);` 并与 `AddPostgres` 绑定。

## 运行

```powershell
# 进入解决方案根目录
cd e:/Abp/Zss.BilliardHall
# 运行 AppHost（dotnet 9+ / Aspire 安装环境）
dotnet run --project src/Zss.BilliardHall/Zss.BilliardHall.AppHost/Zss.BilliardHall.AppHost.csproj
```

