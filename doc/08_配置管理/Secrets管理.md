# Secrets 管理

本章节定义台球厅系统在不同环境下（本地开发 / 测试 / 生产）对敏感信息（数据库凭据、API Key、OAuth Client Secret 等）的管理策略与操作流程。

## 敏感信息分类

| 类型 | 示例 | 存储介质建议 | 轮换策略 |
|------|------|--------------|----------|
| 数据库用户名/密码 | Postgres 用户、只读账户 | Dev: User Secrets / 环境变量; Prod: Secret 管理服务 | 半年或重要人员变更 |
| 连接字符串 | DefaultConnection | 同上 | 与密码同步 |
| 外部支付密钥 | 支付平台 AppSecret | 专用 Secret 服务 (Vault) | 平台策略 |
| 第三方 API Key | 短信/邮件服务 Key | Secret 服务；必要时 KMS 加密 | 季度/被泄露时 |

## 本地开发策略

使用 .NET User Secrets 避免将密钥写入 `appsettings.Development.json` 或提交到 Git。

```powershell
# 初始化（在包含 .csproj 的目录执行）
dotnet user-secrets init

# 设置数据库连接（示例）
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=BilliardHallDb;Username=billiard_dev;Password=DevPassword123!"

# 单独存放凭据（若需要分离）
dotnet user-secrets set "Db:User" "billiard_dev"
dotnet user-secrets set "Db:Password" "DevPassword123!"
```json

> 注意：密码示例仅用于本地开发，请勿在生产重复使用。

## 环境变量命名约定

| 变量名 | 用途 | 说明 |
|--------|------|------|
| BH_DB_USER | Postgres 用户名（可选） | 不设则由 Aspire 默认或连接字符串决定 |
| BH_DB_PASSWORD | Postgres 密码（可选） | 同上 |
| BH_OTEL_EXPORTER_OTLP_ENDPOINT | OTLP Collector 端点 | 统一采集日志/Trace/指标 |

### 前端 (Nuxt / ABP Vue) 相关环境变量

| 变量名 | 用途 | 说明 | 是否敏感 |
|--------|------|------|----------|
| NUXT_AUTHORITY_URL | OIDC 授权服务器地址 | 指向后端 OpenIddict Host (HTTPS) | 否 |
| NUXT_CLIENT_ID | 前端 OIDC ClientId | 需与数据种子中 Abp_Vue 匹配 | 否 |
| NUXT_CLIENT_SECRET | OIDC ClientSecret | 若客户端机密型才需要 | 是 |
| NUXT_SCOPE | 请求的 Scope 列表 | openid profile email BilliardHall 等 | 否 |
| NUXT_REDIRECT_URI | 登录回调相对路径 | /api/auth/callback/openiddict | 否 |
| NUXT_POST_LOGOUT_REDIRECT_URI | 登出回调路径 | /api/auth/signout/callback | 否 |
| NUXT_ORIGIN | 前端站点完整基址 | <https://localhost:3000> | 否 |
| NUXT_SESSION_SECRET | 前端 Session 加密种子 | 随机生成 Base64 | 是 |
| NUXT_ABP_API_ENDPOINT | 后端 API 根地址 | <https://localhost:44388/api> | 否 |

## 生产环境策略

1. 使用云 Secret 管理服务（Azure Key Vault / AWS Secrets Manager / HashiCorp Vault）。
2. 部署编排（Kubernetes）通过 Secret -> Env 或 Volume 注入。
3. 严禁：在镜像 Dockerfile、代码仓库、公共 CI 日志中泄露密钥。
4. 建议：开启数据库最小权限账户（迁移账户 / 只读账户分离）。

### Kubernetes 环境变量示例

```yaml
env:
  - name: BH_DB_USER
    valueFrom:
      secretKeyRef:
        name: billiardhall-db
        key: user
  - name: BH_DB_PASSWORD
    valueFrom:
      secretKeyRef:
        name: billiardhall-db
        key: password
```

## 轮换与撤销

| 场景 | 动作 | 补充 |
|------|------|------|
| 人员离职 | 立即轮换其可访问的全部密钥 | 同步更新文档与权限矩阵 |
| 密钥疑似泄露 | 立刻吊销并生成新密钥 | 审计日志、回溯根因 |
| 定期巡检 | 核对老旧未轮换密钥 | 自动脚本生成报告 |

## 审计与合规

- 所有密钥写入、读取、轮换操作应记录审计日志。
- 建议对生产 Secret 访问启用 MFA / RBAC 最小权限。

## 与 AppHost 的关系

已移除 Host 配置中的硬编码密码；`appsettings.json` 改为使用环境占位：

```
"ConnectionStrings": {
  "Default": "Host=localhost;Port=5432;Database=BilliardHall;Username=${BH_DB_USER};Password=${BH_DB_PASSWORD};"
}
```

`AppHost.cs` 会读取（若存在）环境变量 `BH_DB_USER` / `BH_DB_PASSWORD` 作为参数初始值；未提供则由容器默认用户或镜像策略决定。后续可以演进为 Aspire Parameter 显式管理：

```csharp
var userParam = builder.AddParameter("postgres-user", secret: false);
var pwdParam = builder.AddParameter("postgres-password", secret: true);
var postgres = builder.AddPostgres("postgres", userName: userParam, password: pwdParam);
```

示例（本地临时设置 PowerShell）：
```powershell
$env:BH_DB_USER="billiard_dev"
$env:BH_DB_PASSWORD="DevPassword123!"
```

### 计划中的账户拆分

| 变量 | 用途 | 说明 |
|------|------|------|
| BH_DB_MIGRATION_USER | 迁移账户用户名 | 仅 DbMigrator 使用（DDL 权限） |
| BH_DB_MIGRATION_PASSWORD | 迁移账户密码 | 与上配对 |
| BH_DB_APP_USER | 应用运行账户用户名 | 最小 DML 权限，无 DDL |
| BH_DB_APP_PASSWORD | 应用运行账户密码 | 定期轮换 |

届时连接字符串中的 `${BH_DB_USER}` 占位会被 `${BH_DB_APP_USER}` 替换，迁移工具独立读取迁移账户凭据。

## 最佳实践摘要

1. 开发：User Secrets + 局部环境变量覆写。
2. 测试：CI 注入的临时 Secret（有效期短）。
3. 生产：集中 Secret 服务 + 轮换制度 + 审计日志。
4. 拆分权限（迁移账户 / 运行时账户 / 只读账户）——拆分方案已列入计划。
5. 不在源码、镜像、日志中出现明文密码。

---
版本：v1 初始化

