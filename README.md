# 自助台球系统 (Zss.BilliardHall)

一个完整的自助台球厅管理系统，支持开台计费、支付结算、设备监控等功能。

## 🎯 项目概述

本项目旨在为台球厅提供完整的自助化解决方案，包括：
- 用户扫码开台与计费
- 实时设备监控与状态管理  
- 支付集成与订单管理
- 数据分析与运营支持

## 📁 项目结构

```
├── doc/                    # 详细技术文档（中文）
│   ├── 01_项目概述/       # 项目背景与目标
│   ├── 02_需求规格说明/   # 功能与非功能需求
│   ├── 03_系统架构设计/   # 技术选型与架构
│   ├── 04_模块设计说明/   # 各模块详细设计
│   ├── 05_数据库设计/     # ER图与表结构
│   ├── 06_开发规范/       # 代码风格与规范
│   ├── 07_API文档/        # 接口文档
│   ├── 08_配置管理/       # 环境配置
│   ├── 09_测试方案/       # 测试策略
│   ├── 10_部署与运维/     # 部署指南
│   ├── 11_用户手册/       # 用户操作手册
│   └── 12_版本与变更管理/ # 版本管理
├── docs/                   # 快速参考文档
│   ├── implementation-roadmap.md  # 实施路线图
│   ├── analytics/         # 数据分析
│   │   └── tracking-plan.md      # 埋点方案
│   └── design/            # 设计文档
│       └── flow-and-states.md   # 业务流程与状态机
├── db/                     # 数据库相关
│   └── schema.sql         # 数据库初始化脚本
└── scripts/               # 工具脚本
    └── split-branch-into-prs.ps1  # 分支拆分工具
```

## 🚀 快速开始

### 环境要求
- .NET 6.0+
- MySQL 8.0+ 或 PostgreSQL 12+
- Redis（可选，用于缓存）
- Docker & Docker Compose（推荐）

### 本地开发

1. **克隆仓库**
   ```bash
   git clone https://github.com/douhuaa/Zss.BilliardHall.git
   cd Zss.BilliardHall
   ```

2. **数据库初始化**
   ```bash
   mysql -u root -p < db/schema.sql
   ```

3. **启动服务**
   ```bash
   # 使用 Docker Compose（推荐）
   docker-compose up -d
   
   # 或直接运行
   dotnet run --project src/Zss.BilliardHall.Web
   ```

4. **访问应用**
   - API: http://localhost:5000
   - 管理后台: http://localhost:5000/admin
   - 健康检查: http://localhost:5000/health

## 📋 开发指南

### 分支规范
- `main` - 主分支，稳定版本
- `develop` - 开发分支
- `feature/xxx` - 功能分支
- `hotfix/xxx` - 紧急修复

### 提交规范
```
type(scope): description

# 类型
feat: 新功能
fix: 修复
docs: 文档更新
style: 代码格式
refactor: 重构
test: 测试
chore: 构建/工具
```

### 代码审查
详见 [Code Review 流程](doc/06_开发规范/CodeReview流程.md)

## 🗂️ 核心功能模块

| 模块 | 功能 | 状态 |
|------|------|------|
| 开台管理 | 扫码开台、预约管理 | ✅ 已完成 |
| 计费引擎 | 实时计费、规则配置 | ✅ 已完成 |
| 支付集成 | 微信/支付宝支付 | 🔄 开发中 |
| 设备监控 | 心跳监控、设备控制 | ✅ 已完成 |
| 用户管理 | 会员管理、余额充值 | 📋 计划中 |
| 数据分析 | 营收统计、设备利用率 | 📋 计划中 |

## 📊 实施进度

当前处于 **V0.1 核心链路开发** 阶段

详细进度请查看：[实施路线图](docs/implementation-roadmap.md)

### 里程碑
- [x] Sprint 0: 基础架构搭建 (Week 1)
- [ ] V0.1: 核心链路开发 (Week 2-4)  
- [ ] V0.2: 体验增强 (Week 5-7)
- [ ] V0.3: 多门店支持 (Week 8-10)

## 🔧 技术栈

- **后端**: ASP.NET Core 9.0 + ABP Framework
- **数据库**: MySQL 8.0 + Redis + EF Core
- **前端**: Vue.js 3 + Uniapp (移动端)
- **设备通信**: HTTP/WebSocket + MQTT
- **部署**: Docker + Kubernetes
- **监控**: Prometheus + Grafana

## 📈 数据埋点

系统集成了完整的数据埋点方案，支持：
- 用户行为分析
- 业务指标监控  
- 设备状态追踪
- 支付转化分析

详见：[埋点追踪方案](docs/analytics/tracking-plan.md)

## 🤝 贡献指南

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情

## 📞 联系方式

- 项目负责人: [@douhuaa](https://github.com/douhuaa)
- 邮箱: douhuaa@example.com
- 项目主页: https://github.com/douhuaa/Zss.BilliardHall

---

⭐ 如果这个项目对你有帮助，请给个 Star 支持一下！