# 自助台球厅管理系统

## 项目概述

这是一个基于 ABP 框架和 UniApp 构建的智能化自助台球厅管理系统MVP版本。系统采用现代化的微服务架构，提供完整的台球厅运营管理解决方案。

## 技术栈

### 后端技术栈
- **框架**: ABP Framework (基于 .NET 8)
- **数据库**: PostgreSQL
- **缓存**: Redis
- **ORM**: Entity Framework Core
- **API**: RESTful API + Swagger文档

### 前端技术栈
- **移动端**: UniApp (Vue 3)
- **跨平台**: 支持微信小程序、H5、App

### 基础设施
- **容器化**: Docker + Docker Compose
- **CI/CD**: GitHub Actions
- **日志**: 结构化日志 + 分布式追踪

## 项目结构

```
Zss.BilliardHall/
├── backend/                    # 后端项目
│   ├── Zss.BilliardHall.Web/          # Web应用层
│   ├── Zss.BilliardHall.HttpApi/      # HTTP API层
│   ├── Zss.BilliardHall.Application/  # 应用服务层
│   ├── Zss.BilliardHall.Domain/       # 域模型层
│   ├── Zss.BilliardHall.EntityFrameworkCore/ # 数据访问层
│   └── Zss.BilliardHall.DbMigrator/   # 数据库迁移工具
├── frontend/                   # 前端项目
│   └── uniapp/                 # UniApp移动应用
├── test/                       # 测试项目
├── doc/                        # 项目文档
├── scripts/                    # 部署脚本
└── docker-compose.yml          # Docker编排文件
```

## 核心功能

### MVP版本功能 (V0.1)
- [x] 项目基础架构搭建
- [x] 用户扫码开台功能
- [ ] 实时计时与计费
- [ ] 微信/支付宝支付集成
- [ ] 台桌状态管理
- [ ] 基础数据统计

### 规划功能 (V0.2+)
- [ ] 预约管理系统
- [ ] 会员套餐管理
- [ ] 设备监控告警
- [ ] 多门店管理
- [ ] 营销活动管理
- [ ] 数据分析报表

## 快速开始

### 环境要求
- .NET 8.0 SDK
- Docker & Docker Compose
- Node.js 16+ (用于UniApp开发)

### 本地开发

1. **克隆项目**
```bash
git clone https://github.com/douhuaa/Zss.BilliardHall.git
cd Zss.BilliardHall
```

2. **启动基础服务**
```bash
# 启动数据库和Redis
docker-compose up database redis -d
```

3. **运行后端项目**
```bash
cd backend
dotnet restore
dotnet build
dotnet run --project Zss.BilliardHall.Web
```

4. **运行前端项目**
```bash
cd frontend/uniapp
# 使用HBuilderX打开项目，或者使用uni-app CLI
```

### Docker部署

```bash
# 构建并启动所有服务
docker-compose up --build
```

服务访问地址：
- 后端API: http://localhost:5000
- Swagger文档: http://localhost:5000/swagger

## 开发规范

项目严格遵循 [第6章开发规范](doc/06_开发规范/README.md)，包括：

- **代码风格**: 使用EditorConfig统一代码格式
- **分层约束**: 遵循DDD和Clean Architecture原则  
- **Git规范**: 采用中文提交信息，遵循GitFlow分支模型
- **代码审查**: 强制PR审查，确保代码质量

## 提交规范

```bash
# 功能开发
git commit -m "feat(用户管理): 添加用户注册功能"

# Bug修复  
git commit -m "fix(支付): 修复微信支付回调异常"

# 文档更新
git commit -m "docs(README): 更新部署说明"
```

## 部署架构

### 开发环境
- 本地Docker Compose部署
- 热重载开发模式
- 详细日志输出

### 生产环境
- Kubernetes集群部署
- 负载均衡和自动扩缩容
- 监控告警体系

## 文档导航

- [项目概述](doc/01_项目概述/README.md)
- [需求规格说明](doc/02_需求规格说明/README.md)  
- [系统架构设计](doc/03_系统架构设计/README.md)
- [数据库设计](doc/05_数据库设计/README.md)
- [开发规范](doc/06_开发规范/README.md)
- [API文档](doc/07_API文档/README.md)

## 贡献指南

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'feat(模块): 添加某个功能'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 打开 Pull Request

## 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 联系我们

- 项目维护者: 开发团队
- 技术支持: [GitHub Issues](https://github.com/douhuaa/Zss.BilliardHall/issues)
- 项目文档: [在线文档](https://github.com/douhuaa/Zss.BilliardHall/tree/main/doc)

---

🎱 让技术为台球厅经营赋能！