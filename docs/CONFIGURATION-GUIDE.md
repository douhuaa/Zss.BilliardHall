# 配置指南

> **文档版本**: 1.0  
> **更新日期**: 2026-02-14  
> **适用于**: Zss.BilliardHall MVP

本指南说明如何在不同环境中配置 Zss.BilliardHall 应用程序。

---

## 📋 目录

1. [配置概览](#配置概览)
2. [开发环境配置](#开发环境配置)
3. [生产环境配置](#生产环境配置)
4. [配置项说明](#配置项说明)
5. [常见问题](#常见问题)

---

## 配置概览

Zss.BilliardHall 使用标准的 ASP.NET Core 配置系统，支持多种配置来源：

1. **appsettings.json** - 默认配置（提交到版本控制）
2. **User Secrets** - 开发环境敏感数据（不提交到版本控制）
3. **环境变量** - 容器化部署配置
4. **Azure Key Vault** - 生产环境密钥管理

**重要原则**：
- ⚠️ **禁止**在 appsettings.json 中提交真实的密码、连接字符串或 API 密钥
- ✅ **必须**使用 User Secrets（开发）或 Key Vault（生产）存储敏感数据
- ✅ appsettings.json 仅包含占位符和非敏感配置

---

## 开发环境配置

### 方式 1：使用 User Secrets（推荐）

#### 初始化 User Secrets

```bash
# 进入 Web Host 目录
cd src/Host/Web

# 初始化 User Secrets
dotnet user-secrets init

# 设置数据库连接字符串
dotnet user-secrets set "Marten:ConnectionString" "Host=localhost;Port=5432;Database=zss_billiard_hall;Username=dev_user;Password=dev_password"
```

#### 初始化 Worker Host User Secrets

```bash
# 进入 Worker Host 目录
cd src/Host/Worker

# 初始化 User Secrets
dotnet user-secrets init

# 设置数据库连接字符串
dotnet user-secrets set "Marten:ConnectionString" "Host=localhost;Port=5432;Database=zss_billiard_hall;Username=dev_user;Password=dev_password"
```

#### 查看和管理 User Secrets

```bash
# 列出所有 secrets
dotnet user-secrets list

# 删除特定 secret
dotnet user-secrets remove "Marten:ConnectionString"

# 清除所有 secrets
dotnet user-secrets clear
```

### 方式 2：使用 Docker Compose

创建 `docker-compose.override.yml`（不提交到版本控制）：

```yaml
version: '3.8'

services:
  web:
    environment:
      - Marten__ConnectionString=Host=postgres;Port=5432;Database=zss_billiard_hall;Username=dev_user;Password=dev_password
  
  worker:
    environment:
      - Marten__ConnectionString=Host=postgres;Port=5432;Database=zss_billiard_hall;Username=dev_user;Password=dev_password
  
  postgres:
    environment:
      - POSTGRES_USER=dev_user
      - POSTGRES_PASSWORD=dev_password
      - POSTGRES_DB=zss_billiard_hall
```

**注意**：环境变量使用双下划线 `__` 表示配置层级（`Marten__ConnectionString` = `Marten:ConnectionString`）

---

## 生产环境配置

### 方式 1：Azure Key Vault（推荐）

#### 在 Program.cs 中配置

```csharp
var builder = WebApplication.CreateBuilder(args);

// 开发环境使用 User Secrets，生产环境使用 Key Vault
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
    }
}
```

#### 在 Key Vault 中创建密钥

```bash
# 使用 Azure CLI
az keyvault secret set \
  --vault-name "zss-billiard-hall-kv" \
  --name "Marten--ConnectionString" \
  --value "Host=prod-db.postgres.database.azure.com;Port=5432;Database=zss_billiard_hall;Username=prod_user;Password=***"
```

### 方式 2：Kubernetes Secrets

创建 Kubernetes Secret：

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: zss-billiard-hall-secrets
type: Opaque
stringData:
  marten-connection-string: "Host=postgres.default.svc.cluster.local;Port=5432;Database=zss_billiard_hall;Username=prod_user;Password=***"
```

在 Deployment 中引用：

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: zss-billiard-hall-web
spec:
  template:
    spec:
      containers:
      - name: web
        env:
        - name: Marten__ConnectionString
          valueFrom:
            secretKeyRef:
              name: zss-billiard-hall-secrets
              key: marten-connection-string
```

---

## 配置项说明

### Marten 数据库配置

```json
{
  "Marten": {
    "ConnectionString": "Host=localhost;Port=5432;Database=zss_billiard_hall;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

**说明**：
- 使用 Npgsql 连接字符串格式
- 必须通过 User Secrets / Key Vault / 环境变量配置
- 应用启动时会验证配置是否存在

### 模块配置

```json
{
  "Modules": {
    "Enabled": ["Members", "Orders"]
  }
}
```

**说明**：
- 控制启用哪些业务模块
- 如果不配置 `Enabled`，默认启用所有注册的模块
- 模块名称不区分大小写

### Serilog 日志配置

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### OpenTelemetry 可观测性配置

```json
{
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317",
    "ServiceName": "Zss.BilliardHall",
    "ServiceVersion": "1.0.0"
  }
}
```

### Wolverine HTTP 配置

```json
{
  "Wolverine": {
    "Http": {
      "Enabled": true
    }
  }
}
```

---

## 常见问题

### Q1: 为什么启动时报错 "Marten:ConnectionString 未配置"？

**原因**：应用启动时验证配置，发现 `Marten:ConnectionString` 为空。

**解决方案**：
1. 使用 `dotnet user-secrets set` 配置连接字符串（开发环境）
2. 或设置环境变量 `Marten__ConnectionString`（生产环境）
3. 或在 Azure Key Vault 中配置密钥（生产环境）

### Q2: User Secrets 存储在哪里？

**位置**：
- **Windows**: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- **Linux/macOS**: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

**查看方法**：
```bash
dotnet user-secrets list
```

### Q3: 如何在 CI/CD 中配置敏感数据？

**GitHub Actions 示例**：
```yaml
- name: Run Integration Tests
  env:
    Marten__ConnectionString: ${{ secrets.TEST_DB_CONNECTION_STRING }}
  run: dotnet test
```

**Azure Pipelines 示例**：
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
  env:
    Marten__ConnectionString: $(TestDbConnectionString)
```

### Q4: 如何验证配置是否生效？

**方法 1**：启动应用并查看日志
```bash
dotnet run --project src/Host/Web/Web.csproj
```

**方法 2**：使用配置快照端点（如果启用）
```bash
curl http://localhost:5000/configuration
```

### Q5: 生产环境如何轮换密钥？

**步骤**：
1. 在 Key Vault 中创建新版本的密钥
2. 应用自动使用最新版本（需配置 Key Vault 自动刷新）
3. 或重启应用以加载新配置

---

## 相关文档

- [MVP-RUNBOOK.md](./MVP-RUNBOOK.md) - 部署和运行指南
- [MODULE-DEVELOPMENT-GUIDE.md](./MODULE-DEVELOPMENT-GUIDE.md) - 模块开发指南
- [ADR-002：Platform / Application / Host 三层启动体系](./adr/constitutional/ADR-002-platform-application-host-bootstrap.md)

---

## 安全最佳实践

1. ✅ **永远不要**提交真实密码到版本控制
2. ✅ **使用强密码**和定期轮换策略
3. ✅ **限制访问权限**，只授予必要的数据库权限
4. ✅ **启用审计日志**，记录配置访问
5. ✅ **使用加密连接**（SSL/TLS）连接数据库
