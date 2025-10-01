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
```

> 注意：密码示例仅用于本地开发，请勿在生产重复使用。

## 环境变量命名约定

| 变量名 | 用途 | 说明 |
|--------|------|------|
| BH_DB_USER | Postgres 用户名（可选） | 不设则由 Aspire 默认或连接字符串决定 |
| BH_DB_PASSWORD | Postgres 密码（可选） | 同上 |
| BH_OTEL_EXPORTER_OTLP_ENDPOINT | OTLP Collector 端点 | 统一采集日志/Trace/指标 |

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

`AppHost.cs` 当前未强制绑定 `BH_DB_USER/BH_DB_PASSWORD`，保持默认自动生成策略，后续可演进为：

```csharp
var userParam = builder.AddParameter("postgres-user", secret: false);
var pwdParam = builder.AddParameter("postgres-password", secret: true);
var postgres = builder.AddPostgres("postgres", userName: userParam, password: pwdParam);
```

这样可以在 Aspire Dashboard 中直接管理参数，而无需手动导出环境变量。

## 最佳实践摘要

1. 开发：User Secrets + 局部环境变量覆写。
2. 测试：CI 注入的临时 Secret（有效期短）。
3. 生产：集中 Secret 服务 + 轮换制度 + 审计日志。
4. 尽可能拆分权限（迁移账户与运行时账户）。
5. 不在源码、镜像、日志中出现明文密码。

---
版本：v1 初始化

