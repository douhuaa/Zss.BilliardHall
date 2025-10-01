# Zss.BilliardHall - 自助台球厅管理系统

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 项目简介

自助台球厅智能管理系统是一套完整的无人值守台球厅运营解决方案，包括移动端、管理后台和后端服务。

### 核心功能

- 🎱 **扫码开台**: 用户扫描二维码即可自助开启球台
- ⏱️ **实时计费**: 自动计算台时费用，实时显示当前金额
- 💳 **在线支付**: 支持微信支付、支付宝等多种支付方式
- 👥 **会员管理**: 会员充值、积分、套餐管理
- 📊 **数据统计**: 完善的营收统计和数据分析
- 🔧 **设备管理**: 球台状态监控和设备控制

## 项目结构

```
Zss.BilliardHall/
├── doc/                      # 项目文档
│   ├── 01_项目概述/          # 项目背景、范围、约束
│   ├── 02_需求规格说明/      # 功能需求、非功能需求
│   ├── 03_系统架构设计/      # 架构设计
│   ├── 04_模块设计/          # 各模块详细设计
│   ├── 05_数据库设计/        # 数据库设计
│   ├── 06_开发规范/          # 代码规范、Git规范
│   ├── 07_API文档/           # API接口文档
│   └── ...                   # 其他文档
├── frontend-uniapp/          # UniApp 移动端 (NEW!)
│   ├── src/
│   │   ├── pages/           # 页面
│   │   ├── api/             # API接口
│   │   ├── utils/           # 工具函数
│   │   └── ...
│   └── README.md            # 前端项目说明
└── src/
    └── Zss.BilliardHall/    # 后端项目 (ABP Framework)
        ├── src/
        │   ├── Zss.BilliardHall.Domain/
        │   ├── Zss.BilliardHall.Application/
        │   ├── Zss.BilliardHall.HttpApi.Host/
        │   └── ...
        └── test/            # 测试项目
```

## 技术栈

### 后端
- **框架**: ASP.NET Core 8.0 + ABP Framework
- **数据库**: PostgreSQL / MySQL
- **ORM**: Entity Framework Core
- **认证**: OpenIddict (OAuth 2.0 / OIDC)
- **容器化**: Docker + .NET Aspire

### 前端 (移动端)
- **框架**: UniApp (Vue 3)
- **构建**: Vite
- **目标平台**: 微信小程序、H5、App

### 基础设施
- **缓存**: Redis
- **消息队列**: RabbitMQ (可选)
- **日志**: Serilog
- **监控**: .NET Aspire Dashboard

## 快速开始

### 前置要求

- .NET 8.0 SDK
- Node.js 16+
- PostgreSQL 12+ 或 MySQL 8.0+
- Docker (可选，用于容器化部署)

### 后端运行

```bash
# 1. 克隆仓库
git clone https://github.com/douhuaa/Zss.BilliardHall.git
cd Zss.BilliardHall

# 2. 配置数据库连接
# 编辑 src/Zss.BilliardHall/src/Zss.BilliardHall.HttpApi.Host/appsettings.json

# 3. 运行迁移
cd src/Zss.BilliardHall
dotnet run --project src/Zss.BilliardHall.DbMigrator/Zss.BilliardHall.DbMigrator.csproj

# 4. 启动API服务 (使用 Aspire)
dotnet run --project Zss.BilliardHall.AppHost/Zss.BilliardHall.AppHost.csproj
```

详细说明请参考 [后端项目文档](src/Zss.BilliardHall/README.md)

### 前端运行

```bash
# 1. 进入前端目录
cd frontend-uniapp

# 2. 安装依赖
npm install

# 3. 开发运行 (微信小程序)
npm run dev:mp-weixin

# 4. 开发运行 (H5)
npm run dev:h5
```

详细说明请参考 [前端项目文档](frontend-uniapp/README.md)

## 文档

完整的项目文档位于 `doc/` 目录：

- [项目概述](doc/01_项目概述/README.md)
- [需求规格说明](doc/02_需求规格说明/README.md)
- [系统架构设计](doc/03_系统架构设计/README.md)
- [模块设计](doc/04_模块设计/README.md)
- [数据库设计](doc/05_数据库设计/README.md)
- [开发规范](doc/06_开发规范/README.md)
- [API文档](doc/07_API文档/README.md)

## 开发规范

项目遵循严格的开发规范，详见：

- [代码风格规范](doc/06_开发规范/代码风格.md)
- [Git分支规范](doc/06_开发规范/Git分支规范.md)
- [Code Review流程](doc/06_开发规范/CodeReview流程.md)

## 部署

### Docker 部署

```bash
# 构建镜像
docker-compose build

# 启动服务
docker-compose up -d
```

### 生产部署

详细的部署指南请参考：
- [后端部署文档](doc/10_部署指南/README.md)
- [前端部署文档](frontend-uniapp/README.md#生产构建)

## 项目进度

- ✅ 项目文档体系
- ✅ 后端基础架构 (ABP + OpenIddict)
- ✅ 数据库设计与迁移
- ✅ **UniApp 移动端 MVP** (NEW!)
- 🔄 核心业务模块开发中
- ⏳ 管理后台 (计划中)
- ⏳ 设备集成 (计划中)

## 贡献

欢迎贡献代码、提出问题和建议。

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'feat: Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

请遵循项目的 [开发规范](doc/06_开发规范/README.md) 和 [Git分支规范](doc/06_开发规范/Git分支规范.md)。

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

## 联系方式

- 项目仓库: https://github.com/douhuaa/Zss.BilliardHall
- Issue 反馈: https://github.com/douhuaa/Zss.BilliardHall/issues

## 致谢

- [ABP Framework](https://abp.io/)
- [UniApp](https://uniapp.dcloud.net.cn/)
- [Vue.js](https://vuejs.org/)

---

**注意**: 本项目目前处于开发阶段，部分功能尚未完善。
