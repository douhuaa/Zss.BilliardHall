# 自助台球厅系统

基于 ABP Framework 和 .NET 9 构建的自助台球厅管理系统，包含 UniApp 微信小程序客户端。

## 🚀 技术栈

### 后端
- **框架**: ABP Framework 9.0 + .NET 9
- **数据库**: Entity Framework Core + MySQL/PostgreSQL
- **架构**: DDD（领域驱动设计）分层架构
- **API**: RESTful API + Swagger文档

### 前端
- **客户端**: UniApp + Vue 3 + TypeScript
- **平台支持**: 微信小程序、H5、App

### 开发工具
- **IDE**: Visual Studio / VS Code / JetBrains Rider
- **版本控制**: Git + GitHub
- **CI/CD**: GitHub Actions

## 📁 项目结构

```
Zss.BilliardHall/
├── src/                           # 后端源代码
│   ├── Zss.BilliardHall.Domain/              # 领域层
│   ├── Zss.BilliardHall.Application.Contracts/  # 应用服务契约层
│   ├── Zss.BilliardHall.Application/         # 应用服务实现层
│   ├── Zss.BilliardHall.EntityFrameworkCore/ # 数据访问层
│   ├── Zss.BilliardHall.HttpApi/            # HTTP API层
│   ├── Zss.BilliardHall.HttpApi.Host/       # API主机
│   └── Zss.BilliardHall.DbMigrator/         # 数据库迁移工具
├── client/                        # 前端客户端
│   └── UniApp/                    # UniApp项目
│       ├── src/                   # 源代码
│       ├── manifest.json          # 应用配置
│       └── package.json           # 依赖管理
├── test/                         # 测试项目
├── doc/                          # 项目文档
└── db/                          # 数据库脚本
```

## 🛠️ 开发环境搭建

### 前置要求

- .NET 9 SDK
- Node.js 16+
- MySQL 8.0+ 或 PostgreSQL 13+
- Visual Studio 2022 或 VS Code
- HBuilderX（UniApp开发，可选）

### 安装步骤

1. **克隆项目**
   ```bash
   git clone https://github.com/douhuaa/Zss.BilliardHall.git
   cd Zss.BilliardHall
   ```

2. **安装 .NET 依赖**
   ```bash
   dotnet restore
   ```

3. **配置数据库连接**
   ```bash
   # 编辑 src/Zss.BilliardHall.HttpApi.Host/appsettings.json
   # 设置数据库连接字符串
   ```

4. **运行数据库迁移**
   ```bash
   cd src/Zss.BilliardHall.DbMigrator
   dotnet run
   ```

5. **启动后端服务**
   ```bash
   cd src/Zss.BilliardHall.HttpApi.Host
   dotnet run
   ```

6. **安装前端依赖**
   ```bash
   cd client/UniApp
   npm install
   ```

7. **启动前端开发服务器**
   ```bash
   # H5 开发
   npm run dev:h5
   
   # 微信小程序开发
   npm run dev:mp-weixin
   ```

## 📝 开发规范

本项目严格遵循 [第6章开发规范](doc/06_开发规范/README.md)：

### Git 分支规范
- **主分支**: `main` - 生产环境代码
- **开发分支**: `develop` - 开发环境集成分支
- **功能分支**: `feature/功能名称` - 功能开发分支
- **修复分支**: `hotfix/修复描述` - 生产环境紧急修复
- **发布分支**: `release/版本号` - 发布准备分支

### 提交信息规范
```bash
# 格式：<类型>(范围): 简短描述
git commit -m "功能(台球桌): 添加台球桌状态管理功能"
git commit -m "修复(支付): 修复微信支付回调异常"
git commit -m "文档: 更新API使用说明"
```

### 代码风格
- 遵循 Microsoft C# 编码规范
- 使用 EditorConfig 统一代码格式
- 所有公开方法必须添加中文注释
- 异常信息使用中文描述

## 🔧 配置说明

### 后端配置

主要配置文件位于 `src/Zss.BilliardHall.HttpApi.Host/appsettings.json`：

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=BilliardHall;Uid=root;Pwd=password;"
  },
  "Redis": {
    "Configuration": "localhost:6379"
  },
  "App": {
    "SelfUrl": "https://localhost:44300",
    "CorsOrigins": "https://localhost:3000,http://localhost:8080"
  }
}
```

### 前端配置

UniApp 配置文件 `client/UniApp/manifest.json`：
- 配置微信小程序 AppID
- 设置应用权限
- 配置打包参数

## 🚀 部署指南

### 后端部署

1. **发布应用**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Docker 部署**（推荐）
   ```bash
   docker build -t billiard-hall-api .
   docker run -p 8080:80 billiard-hall-api
   ```

3. **IIS 部署**
   - 将发布文件复制到 IIS 站点目录
   - 配置应用池使用 .NET 9

### 前端部署

1. **微信小程序发布**
   ```bash
   npm run build:mp-weixin
   # 使用微信开发者工具上传代码包
   ```

2. **H5 部署**
   ```bash
   npm run build:h5
   # 将 dist 目录部署到 Web 服务器
   ```

## 📊 功能特性

### 核心功能
- ✅ 台球桌管理（状态监控、设备心跳）
- ✅ 扫码开台/关台
- ✅ 实时计费系统
- ✅ 微信支付集成
- ✅ 会话管理
- ✅ 用户管理
- ✅ 多门店支持

### 系统特性
- 🏗️ 模块化架构设计
- 🔒 基于角色的权限控制
- 📱 多平台客户端支持
- 🌐 国际化支持（中文/英文）
- 📝 完整的操作日志
- 🔧 实时监控与告警

## 🤝 贡献指南

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/新功能名称`)
3. 提交更改 (`git commit -m '功能: 添加某个功能'`)
4. 推送到分支 (`git push origin feature/新功能名称`)
5. 创建 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情

## 📞 联系我们

- 项目主页: [GitHub](https://github.com/douhuaa/Zss.BilliardHall)
- 问题报告: [Issues](https://github.com/douhuaa/Zss.BilliardHall/issues)
- 文档: [项目文档](doc/自助台球系统项目文档.md)

---

💡 **提示**: 开发前请仔细阅读 [开发规范](doc/06_开发规范/README.md) 确保代码质量和团队协作效率。