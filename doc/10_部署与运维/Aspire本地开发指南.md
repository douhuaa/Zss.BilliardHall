# Aspire 本地开发指南

## 1. 概述

本指南帮助开发者在本地快速启动和调试台球厅管理系统的 Aspire 应用。

> **前置条件**: Docker Desktop、.NET 10 SDK、IDE（Visual Studio 2022 或 Rider）

---

## 2. 环境准备

### 2.1 安装依赖

#### Windows / macOS

```powershell
# 1. 安装 .NET 10 SDK
winget install Microsoft.DotNet.SDK.10

# 2. 安装 Docker Desktop
winget install Docker.DockerDesktop

# 3. 验证安装
dotnet --version  # 应显示 10.x.x
docker --version  # 应显示 Docker version 24.x+
```

#### Linux (Ubuntu/Debian)

```bash
# 1. 安装 .NET 10 SDK
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# 2. 安装 Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# 3. 验证安装
dotnet --version
docker --version
```

### 2.2 IDE 设置

#### Visual Studio 2022

1. 安装 **Visual Studio 2022 17.9+**（包含 .NET Aspire 工作负载）
2. 或通过 Visual Studio Installer 添加 ".NET Aspire" 工作负载

#### JetBrains Rider

1. 安装 **Rider 2024.1+**
2. 安装插件：Settings → Plugins → 搜索 ".NET Aspire" → Install

---

## 3. 克隆与构建

### 3.1 克隆仓库

```bash
git clone https://github.com/douhuaa/Zss.BilliardHall.git
cd Zss.BilliardHall
```

### 3.2 初始构建

```bash
# 恢复依赖
dotnet restore

# 构建整个解决方案（验证无编译错误）
dotnet build

# 可选：运行测试
dotnet test
```

---

## 4. 启动 Aspire 应用

### 4.1 方式 1: Visual Studio（推荐）

1. 打开 `Zss.BilliardHall.sln`
2. 右键 `Zss.BilliardHall.Wolverine.AppHost` 项目 → **设为启动项目**
3. 按 **F5** 或点击"启动调试"
4. 浏览器自动打开 Aspire Dashboard（默认 `https://localhost:17001`）

### 4.2 方式 2: JetBrains Rider

1. 打开 `Zss.BilliardHall.sln`
2. 运行配置下拉菜单选择 **AppHost**
3. 点击运行按钮（或 Shift+F10）
4. 浏览器自动打开 Aspire Dashboard

### 4.3 方式 3: 命令行

```bash
cd src/Wolverine/Aspire/Zss.BilliardHall.Wolverine.AppHost
dotnet run
```

**预期输出**:

```
info: Aspire.Hosting.DistributedApplication[0]
      Aspire version: 9.0.0
      Listening on https://localhost:17001
info: Aspire.Hosting.DistributedApplication[0]
      Now listening on: https://localhost:17001
```

打开浏览器访问 `https://localhost:17001`

---

## 5. Aspire Dashboard 使用

### 5.1 界面概览

Dashboard 提供统一的监控和调试界面：

| Tab | 功能 | 说明 |
|-----|------|------|
| **Resources** | 资源状态视图 | 查看所有服务、容器、数据库的运行状态 |
| **Console** | 实时日志 | 查看每个资源的控制台输出 |
| **Structured Logs** | 结构化日志 | 按级别、时间、服务筛选日志 |
| **Traces** | 分布式追踪 | 查看请求的完整调用链 |
| **Metrics** | 性能指标 | CPU、内存、HTTP 请求速率等图表 |

### 5.2 常用操作

#### 查看资源状态

1. 切换到 **Resources** Tab
2. 查看每个资源的状态：
   - ✅ **Running**: 正常运行
   - ⏸️ **Starting**: 启动中
   - ❌ **Failed**: 启动失败（点击查看错误）

#### 查看服务日志

1. **Resources** Tab → 点击服务名称（如 `bootstrapper`）
2. 切换到 **Console** 查看实时输出
3. 或切换到 **Structured Logs** 查看结构化日志

#### 追踪请求

1. 发起 HTTP 请求到 API（如 `https://localhost:7001/api/members`）
2. 切换到 **Traces** Tab
3. 找到对应的 Trace，点击查看详细时间线

#### 重启服务

1. **Resources** Tab → 找到服务
2. 点击右侧 **⋮**（更多）→ **Restart**

---

## 6. 常见任务

### 6.1 访问 Bootstrapper API

**Swagger UI**:

1. 启动 AppHost 后，查看 Dashboard → Resources → `bootstrapper`
2. 复制 **Endpoint** 地址（如 `https://localhost:7001`）
3. 浏览器访问 `https://localhost:7001/swagger`

**Curl 示例**:

```bash
# 健康检查
curl http://localhost:7001/health

# 示例 API
curl https://localhost:7001/api/members
```

### 6.2 连接 PostgreSQL

**方式 1: 通过 pgAdmin / DBeaver**

1. Dashboard → Resources → `postgres`
2. 查看 **Connection String**（点击复制）
3. 在 pgAdmin 中新建连接：
   - Host: `localhost`
   - Port: `5432`（或 Dashboard 显示的端口）
   - Database: `billiard-hall-db`
   - Username: `postgres`
   - Password: （查看 Dashboard 环境变量）

**方式 2: psql 命令行**

```bash
# 从 Dashboard 复制连接字符串
export PGPASSWORD='<password-from-dashboard>'
psql -h localhost -p 5432 -U postgres -d billiard-hall-db

# 查看表
\dt

# 退出
\q
```

### 6.3 清空数据（重新开始）

```bash
# 停止 AppHost（Ctrl+C）

# 查找 Aspire 创建的 Docker 卷
docker volume ls | grep aspire

# 删除数据卷（⚠️ 会清空所有数据）
docker volume rm <volume-name>

# 重新启动 AppHost
dotnet run
```

---

## 7. 调试技巧

### 7.1 调试单个服务

**场景**: 只想调试 Bootstrapper，不启动整个 AppHost。

**步骤**:

1. 先启动 AppHost（启动依赖容器如 PostgreSQL）
2. 停止 Bootstrapper（Dashboard → Resources → `bootstrapper` → Stop）
3. 在 IDE 中直接运行 Bootstrapper 项目（F5 调试）

**注意**: 连接字符串等配置需手动设置（从 Dashboard 复制）。

### 7.2 附加到运行中的服务

**Visual Studio**:

1. Debug → Attach to Process...
2. 搜索 `Bootstrapper.exe` 或 `dotnet.exe`（根据进程名筛选）
3. 点击 Attach

**Rider**:

1. Run → Attach to Process...
2. 选择对应的 `dotnet` 进程
3. Attach Debugger

### 7.3 查看环境变量

需要查看 Aspire 注入的连接字符串或配置：

1. Dashboard → Resources → 点击服务
2. 切换到 **Environment** Tab
3. 查看所有注入的环境变量

**示例**:

```
ConnectionStrings__billiard-hall-db=Host=localhost;Port=5432;Database=billiard-hall-db;...
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
ASPNETCORE_ENVIRONMENT=Development
```

### 7.4 启用详细日志

**临时启用**（命令行）:

```bash
export LOGGING__LOGLEVEL__DEFAULT=Debug
dotnet run
```

**永久配置**（appsettings.Development.json）:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.Extensions.ServiceDiscovery": "Trace",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 8. 故障排查

### 8.1 常见错误

#### 错误：端口已被占用

```
Error: Failed to bind to address https://127.0.0.1:17001
```

**原因**: 端口 17001 被其他 Aspire 实例占用。

**解决**:

```bash
# Windows
netstat -ano | findstr :17001
taskkill /PID <PID> /F

# Linux/macOS
lsof -ti:17001 | xargs kill -9
```

#### 错误：PostgreSQL 容器启动失败

```
Error: Container 'postgres' failed to start
```

**原因**: Docker Desktop 未运行或容器镜像拉取失败。

**解决**:

1. 启动 Docker Desktop
2. 手动拉取镜像：`docker pull postgres:16`
3. 重启 AppHost

#### 错误：服务无法连接数据库

```
Npgsql.NpgsqlException: Connection refused
```

**原因**: 服务在数据库就绪前启动。

**解决**:

确保 AppHost 中使用了 `.WaitFor(db)`：

```csharp
builder.AddProject<Projects.Bootstrapper>("bootstrapper")
    .WithReference(db)
    .WaitFor(db); // ✅ 等待数据库就绪
```

### 8.2 诊断命令

```bash
# 查看 Aspire 容器状态
docker ps -a | grep aspire

# 查看容器日志
docker logs <container-id>

# 验证网络连接
docker network inspect <network-name>

# 测试数据库连接
docker exec -it <postgres-container-id> psql -U postgres
```

### 8.3 重置环境

完全清理 Aspire 环境：

```bash
# 停止所有容器
docker stop $(docker ps -aq)

# 删除 Aspire 相关容器
docker rm $(docker ps -aq --filter "label=aspire")

# 删除 Aspire 数据卷
docker volume rm $(docker volume ls -q --filter "label=aspire")

# 删除网络
docker network prune -f
```

---

## 9. 性能优化

### 9.1 容器持久化

默认配置已启用数据卷持久化，避免每次重启丢失数据：

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()                          // ✅ 数据持久化
    .WithLifetime(ContainerLifetime.Persistent); // ✅ 容器持久化
```

### 9.2 减少启动时间

**技巧 1: 预拉取镜像**

```bash
docker pull postgres:16
docker pull redis:7
docker pull rabbitmq:3-management
```

**技巧 2: 使用本地镜像缓存**

Docker Desktop → Settings → Resources → Disk image size: 增加到 64GB+

**技巧 3: 禁用不需要的资源**

编辑 AppHost.cs，注释掉暂时不用的资源：

```csharp
// var redis = builder.AddRedis("redis"); // 暂时不用
```

### 9.3 内存限制

限制容器内存使用（避免 Docker Desktop 占用过多资源）：

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithResourceLimit(memory: 512); // 限制 512MB
```

---

## 10. 团队协作

### 10.1 共享配置

团队成员使用统一的 User Secrets（避免提交密钥）：

```bash
cd src/Wolverine/Bootstrapper
dotnet user-secrets init
dotnet user-secrets set "TestApi:Key" "dev-test-key-12345"
```

### 10.2 文档同步

更新 Aspire 配置后，通知团队：

1. 提交代码：`git commit -am "feat(aspire): 新增 Redis 容器"`
2. 更新文档：补充新资源的使用说明
3. 团队会议同步变更

### 10.3 故障互助

使用 Aspire Dashboard 截图快速沟通问题：

1. Dashboard → Resources → 截图显示错误状态
2. Dashboard → Console → 复制错误日志
3. 贴到团队群或 Issue 中

---

## 11. 下一步

掌握本地开发后，继续学习：

- [Aspire 编排架构](../03_系统架构设计/Aspire编排架构.md) - 深入理解架构设计
- [ServiceDefaults 集成指南](../06_开发规范/ServiceDefaults集成指南.md) - 服务配置详解
- [Wolverine 快速上手指南](../03_系统架构设计/Wolverine快速上手指南.md) - 开始编写业务代码

---

## 12. 常见问题 (FAQ)

### Q1: 如何更改 Dashboard 端口？

**A**: 编辑 `appsettings.json`（AppHost 项目）:

```json
{
  "Aspire": {
    "Dashboard": {
      "Url": "https://localhost:18888"
    }
  }
}
```

### Q2: 如何添加新的依赖容器（如 Redis）？

**A**: 在 AppHost.cs 中添加：

```csharp
var redis = builder.AddRedis("redis")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.Bootstrapper>("bootstrapper")
    .WithReference(db)
    .WithReference(redis) // 新增 Redis 引用
    .WaitFor(db)
    .WaitFor(redis);
```

### Q3: 如何在 CI/CD 中运行测试？

**A**: CI 环境不使用 AppHost，直接使用 Testcontainers：

```csharp
// 集成测试
var container = new PostgreSqlBuilder()
    .WithImage("postgres:16")
    .Build();

await container.StartAsync();
```

详见：[集成测试计划](../09_测试方案/集成测试计划.md)

### Q4: Aspire 支持非 .NET 服务吗？

**A**: 支持。可添加任意容器或可执行文件：

```csharp
// Node.js 前端
builder.AddNpmApp("frontend", "../frontend")
    .WithHttpEndpoint(port: 3000)
    .WithEnvironment("API_URL", "http+https://bootstrapper");

// Python 服务
builder.AddDockerfile("ml-service", "../ml-service")
    .WithHttpEndpoint(port: 8000);
```

---

## 13. 版本历史

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|---------|------|
| 1.0.0 | 2024-01-15 | 初始版本，涵盖本地开发完整流程 | 架构团队 |

---

**最后更新**: 2024-01-15  
**负责人**: 架构团队  
**审核状态**: ✅ 已审核
