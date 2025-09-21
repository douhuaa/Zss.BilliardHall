# 台球厅管理系统 (Billiard Hall Management System)

无人自助台球厅数字化管理平台，支持扫码开台、自动计费、在线支付的完整业务闭环。

## 快速开始

### 环境要求

- .NET 8+ SDK
- MySQL 8+ 或 PostgreSQL 13+
- Docker & Docker Compose（推荐）

### 本地开发启动

#### 方式1：使用 Docker Compose（推荐）

```bash
# 克隆项目
git clone https://github.com/douhuaa/Zss.BilliardHall.git
cd Zss.BilliardHall

# 启动基础服务（MySQL + Redis）
docker compose up -d

# 运行数据库迁移
./scripts/migrate.sh

# 启动后端服务
cd src/BilliardHall.Api
dotnet run
```

#### 方式2：本地安装

```bash
# 安装依赖
./scripts/install-deps.sh

# 配置数据库连接
cp appsettings.Development.json.example appsettings.Development.json
# 编辑 appsettings.Development.json 中的连接字符串

# 运行迁移并启动
dotnet ef database update
dotnet run
```

### 验证安装

访问以下端点确认服务正常：

- 健康检查: http://localhost:5000/health
- API文档: http://localhost:5000/swagger
- 管理界面: http://localhost:5000/admin

## 项目结构

```
├── docs/                  # 文档目录
│   ├── architecture/      # 架构设计
│   ├── design/           # 数据设计
│   └── analytics/        # 埋点方案
├── db/                   # 数据库脚本
│   └── schema.sql        # 数据库schema
├── events/               # 事件定义
│   └── schema/          # 事件JSON Schema
├── src/                 # 源代码（待创建）
│   ├── BilliardHall.Api/        # Web API
│   ├── BilliardHall.Domain/     # 领域模型
│   └── BilliardHall.Infrastructure/ # 基础设施
├── scripts/             # 脚本工具
└── docker-compose.yml   # 容器编排
```

## 核心功能

### V0.1 可用闭环
- ✅ 扫码开台
- ✅ 自动计费
- ✅ 在线支付
- ✅ 设备监控
- ✅ 事件追踪

### V0.2 体验增强（计划中）
- 预约功能
- 会员套餐
- 告警工单
- 运营报表

## API 接口

### 核心业务接口

```bash
# 开台
POST /api/session/start
{
  "table_code": "T001",
  "user_phone": "13800138000"
}

# 结束会话
POST /api/session/{sessionId}/end

# 支付
POST /api/payment/create
{
  "session_id": 12345,
  "channel": "wechat"
}

# 事件上报
POST /api/track
{
  "events": [...]
}
```

### 设备接口

```bash
# 设备心跳
POST /device/heartbeat
{
  "device_sn": "DEV001",
  "metrics": {...}
}

# 灯控指令
POST /device/light-control
{
  "table_id": 1,
  "action": "on"
}
```

## 开发规范

### Git工作流

- 主分支：`main` 
- 功能分支：`feature/功能名称`
- 修复分支：`hotfix/问题描述`

### 提交规范

```bash
# 功能
git commit -m "feat: 添加开台API"

# 修复
git commit -m "fix: 修复支付回调幂等问题"

# 文档
git commit -m "docs: 更新API文档"

# 重构
git commit -m "refactor: 重构计费服务"
```

### 代码风格

- C#: 遵循 Microsoft 编码规范
- SQL: 使用 snake_case 命名
- JSON: 使用 camelCase

## 测试

```bash
# 单元测试
dotnet test src/BilliardHall.Tests/

# 集成测试
dotnet test src/BilliardHall.IntegrationTests/

# API测试
./scripts/test-api.sh
```

## 部署

### 开发环境

```bash
docker-compose -f docker-compose.dev.yml up
```

### 生产环境

```bash
# 构建镜像
docker build -t billiard-hall:latest .

# 部署
kubectl apply -f k8s/
```

## 监控指标

| 指标名称 | 描述 | 阈值 |
|---------|------|------|
| sessions_active_gauge | 活跃会话数 | < 100 |
| payment_success_rate | 支付成功率 | > 95% |
| api_response_time_p95 | API响应时间95分位 | < 300ms |
| heartbeat_delay_avg | 设备心跳平均延迟 | < 5s |

## 故障排除

### 常见问题

**Q: 数据库连接失败**
A: 检查 `appsettings.json` 中的连接字符串，确保MySQL服务正在运行

**Q: 支付回调失败**
A: 检查支付网关配置和回调URL的网络连通性

**Q: 设备心跳超时**
A: 检查设备网络连接和防火墙配置

### 日志查看

```bash
# 应用日志
docker logs billiard-hall-api

# 数据库日志
docker logs billiard-hall-mysql

# 系统指标
curl http://localhost:5000/metrics
```

## 贡献指南

1. Fork 本项目
2. 创建功能分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'Add amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 提交 Pull Request

## 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 联系方式

- 项目维护者: [douhuaa](https://github.com/douhuaa)
- 问题反馈: [GitHub Issues](https://github.com/douhuaa/Zss.BilliardHall/issues)

---

**当前版本:** Sprint 0 (基线准备)  
**最后更新:** 2024-09