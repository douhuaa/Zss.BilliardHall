# 快速开始指南 (Quick Start Guide)

本指南帮助您快速运行 Zss.BilliardHall 项目。

## 📋 前置条件

- [x] .NET 9 SDK 已安装
- [ ] PostgreSQL 12+ 已安装并运行
- [ ] 已配置数据库连接字符串

## 🚀 快速启动步骤

### 1. 克隆项目（如果尚未克隆）

```bash
git clone https://github.com/douhuaa/Zss.BilliardHall.git
cd Zss.BilliardHall
```

### 2. 配置数据库连接

编辑 `src/Zss.BilliardHall.HttpApi.Host/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=ZssBilliardHall;Username=postgres;Password=your_password"
  }
}
```

### 3. 创建数据库（首次运行）

```bash
# 安装 EF Core 工具（如果未安装）
dotnet tool install -g dotnet-ef

# 创建并应用数据库迁移
cd src/Zss.BilliardHall.EntityFrameworkCore
dotnet ef migrations add InitialCreate
dotnet ef database update
cd ../..
```

### 4. 构建项目

```bash
dotnet build
```

### 5. 运行测试（可选）

```bash
dotnet test
```

### 6. 启动应用程序

#### 方式 A: 分别启动 API 和 Blazor

**终端 1 - 启动 API:**
```bash
cd src/Zss.BilliardHall.HttpApi.Host
dotnet run
```

API 将在以下地址启动：
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000
- Swagger UI: https://localhost:5001/swagger

**终端 2 - 启动 Blazor:**
```bash
cd src/Zss.BilliardHall.Blazor
dotnet run
```

Blazor 应用将在以下地址启动：
- HTTPS: https://localhost:5002
- HTTP: http://localhost:5003

#### 方式 B: 使用 Docker Compose（待实现）

```bash
docker-compose up -d
```

## 📍 访问应用

| 服务 | 地址 | 说明 |
|------|------|------|
| Blazor UI | https://localhost:5002 | 前端应用 |
| API | https://localhost:5001 | Web API |
| Swagger | https://localhost:5001/swagger | API 文档 |

## 🛠️ 开发工具推荐

- **IDE**: 
  - Visual Studio 2022 (Windows/Mac)
  - JetBrains Rider
  - Visual Studio Code
- **数据库工具**: 
  - pgAdmin (PostgreSQL)
  - DBeaver
- **API 测试**: 
  - Postman
  - Swagger UI (内置)

## 📚 下一步

1. 阅读 [项目搭建指南](doc/项目搭建指南.md) 了解项目架构
2. 查看 [技术选型](doc/03_系统架构设计/技术选型.md) 了解技术栈
3. 参考 [README.md](README.md) 获取完整文档

## ❓ 常见问题

### Q: 端口被占用怎么办？

修改 `Properties/launchSettings.json` 中的端口配置。

### Q: 数据库连接失败？

1. 确认 PostgreSQL 服务是否运行: `pg_isready`
2. 检查连接字符串中的用户名、密码和数据库名
3. 确保数据库已创建或运行 `dotnet ef database update`

### Q: 编译错误？

1. 确认 .NET 9 SDK 已安装: `dotnet --version`
2. 清理并重新构建: `dotnet clean && dotnet build`
3. 删除所有 `bin` 和 `obj` 目录后重新构建

## 🆘 获取帮助

如遇到问题，请：
1. 查看项目文档
2. 检查 GitHub Issues
3. 联系项目维护者

---

**祝您使用愉快！Happy Coding! 🎉**
