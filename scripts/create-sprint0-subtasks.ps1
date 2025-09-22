#!/usr/bin/env pwsh
# create-sprint0-subtasks.ps1
# Creates detailed GitHub Issues for Sprint 0 sub-tasks
# Usage: pwsh ./scripts/create-sprint0-subtasks.ps1 [-DryRun]

param(
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$script:hasErrors = $false

# Check GitHub CLI authentication
Write-Host "🔍 Checking GitHub CLI authentication..."
$authResult = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ GitHub CLI is not authenticated!"
    Write-Host "Please run one of the following to authenticate:"
    Write-Host "  1. gh auth login"
    Write-Host "  2. export GH_TOKEN=your_github_token"
    Write-Host "  3. Set GITHUB_TOKEN environment variable"
    Write-Host ""
    Write-Host "Alternatively, use the manual creation guide: docs/github-issues-manual-creation-guide.md"
    Write-Host "Or use the JSON data: docs/github-issues-data.json"
    exit 1
}

function CreateIssue($title, $labels, $body, $assignee = $null) {
    Write-Host "Creating issue: $title"
    
    if ($DryRun) {
        Write-Host "  [DRY RUN] Would create issue with labels: $($labels -join ', ')"
        return
    }
    
    $args = @('issue', 'create', '--title', $title, '--body', $body)
    foreach ($label in $labels) {
        $args += @('--label', $label)
    }
    if ($assignee) {
        $args += @('--assignee', $assignee)
    }
    
    try {
        $result = gh @args 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Created successfully"
        } else {
            Write-Host "  ❌ Failed: $result"
            $script:hasErrors = $true
        }
    }
    catch {
        Write-Host "  ❌ Failed: $_"
        $script:hasErrors = $true
    }
}

Write-Host "🏗️ Creating Sprint 0 Sub-Tasks..."

# Architecture Design Tasks
CreateIssue "S0-001: 设计并创建 C4 Level 2 容器架构图" @("task", "architecture", "sprint0", "completed") @"
## 描述
设计系统架构图，包含 Web应用、API网关、核心服务、设备服务、埋点服务等关键组件

## 验收标准
- [x] 完成 C4 Level 2 容器图设计
- [x] 包含所有主要系统组件和外部依赖
- [x] 标明数据流和通信协议
- [x] 文档保存在 docs/architecture/c4-level2-containers.md

## 技术要求
- 使用 Mermaid C4 语法
- 包含系统边界和外部依赖
- 标注通信协议和数据流向

## 完成情况
✅ 已完成 - 包含完整的容器架构图和ADR文档

**估时:** 4h  
**优先级:** P0  
**任务组:** 架构设计
"@

CreateIssue "S0-002: 编写架构决策记录 (ADR)" @("task", "architecture", "sprint0", "completed") @"
## 描述
记录技术栈选择、架构模式、数据存储策略等关键决策

## 验收标准
- [x] ADR-001: 技术栈选择 (ASP.NET Core + MySQL + Redis)
- [x] ADR-002: 分层架构设计 (DDD)
- [x] ADR-003: 数据存储策略
- [x] 每个ADR包含背景、决策、理由、后果

## ADR 模板结构
- 状态 (接受/拒绝/废弃)
- 背景 (为什么需要这个决策)
- 决策 (具体的技术选择)
- 理由 (选择的原因)
- 后果 (决策的影响)

## 完成情况
✅ 已完成 - 3个核心ADR已编写完成

**估时:** 2h  
**优先级:** P0  
**任务组:** 架构设计
"@

# Database Design Tasks
CreateIssue "S0-003: 设计并实现数据库Schema v1" @("task", "database", "sprint0", "completed") @"
## 描述
创建V0.1范围所需的核心数据表结构

## 核心表清单
- [x] store - 门店信息
- [x] billiard_table - 球台信息  
- [x] user - 用户信息
- [x] table_session - 会话记录
- [x] billing_snapshot - 计费快照
- [x] payment_order - 支付订单
- [x] device - 设备信息
- [x] device_heartbeat - 设备心跳
- [x] event_log - 事件日志
- [x] pricing_rule - 计费规则
- [x] payment_callback_idempotent - 支付幂等控制

## 验收标准
- [x] 精简表结构至11个核心表
- [x] 包含完整的约束、索引、外键定义
- [x] 添加中文注释说明每个字段用途
- [x] 支持MySQL 8+标准

## 完成情况
✅ 已完成 - schema.sql v1已创建，包含所有核心表和约束

**估时:** 3h  
**优先级:** P0  
**任务组:** 数据库设计
"@

CreateIssue "S0-004: 创建种子数据" @("task", "database", "sprint0", "completed") @"
## 描述
为开发和测试环境准备基础数据

## 验收标准
- [x] 测试门店数据 (2个门店)
- [x] 测试球台数据 (每店3-5个球台)
- [x] 测试用户数据 (包含系统用户)
- [x] 测试设备数据 (每个球台对应设备)
- [x] 默认计费规则数据

## 数据内容
- 测试台球厅1号店、2号店 (北京、上海)
- 每店5个球台 (T001-T005)
- 测试用户和系统用户
- 对应的设备记录
- 默认计费规则 (1-2元/分钟)

## 完成情况
✅ 已完成 - db/seed-data.sql 已创建

**估时:** 1h  
**优先级:** P1  
**任务组:** 数据库设计
"@

# Event Tracking Tasks
CreateIssue "S0-005: 设计P0级别事件JSON Schema" @("task", "events", "sprint0", "completed") @"
## 描述
为核心业务事件定义标准化Schema

## P0事件清单
- [x] qr_scan.json - 扫码行为事件
- [x] session_start.json - 开台成功事件  
- [x] session_end_request.json - 结束请求事件
- [x] billing_frozen.json - 计费冻结事件
- [x] payment_create.json - 支付创建事件
- [x] payment_success.json - 支付成功事件
- [x] heartbeat_receive.json - 设备心跳事件

## 通用字段标准
- [x] event_type, event_time, user_id, store_id, platform
- [x] JSON Schema Draft 07规范
- [x] 支持版本控制和向后兼容

## 完成情况
✅ 已完成 - 7个P0事件Schema已定义完成

**估时:** 4h  
**优先级:** P0  
**任务组:** 事件追踪
"@

CreateIssue "S0-006: 设计统一事件上报API契约" @("task", "events", "sprint0", "completed") @"
## 描述
定义 /api/track 接口规范和响应格式

## 验收标准
- [x] POST /api/track 接口定义
- [x] 批量事件上报支持
- [x] 统一响应格式 (success, processed_count, failed_events)
- [x] Schema验证机制
- [x] API文档更新

## API 契约
```json
POST /api/track
{
  "events": [
    { "event_type": "qr_scan", ... }
  ]
}
```

## 完成情况
✅ 已完成 - API契约已定义，README包含完整文档

**估时:** 2h  
**优先级:** P0  
**任务组:** 事件追踪
"@

# Development Environment Tasks  
CreateIssue "S0-007: 搭建Docker Compose开发环境" @("task", "devops", "sprint0", "completed") @"
## 描述
创建包含所有依赖服务的Docker环境

## 验收标准
- [x] MySQL 8.0 数据库服务
- [x] Redis 7 缓存服务
- [x] 自动数据库初始化 (schema.sql + seed-data.sql)
- [x] 网络配置和端口映射
- [x] 健康检查配置
- [x] 可选管理工具 (phpMyAdmin, Redis Commander)

## 服务配置
- MySQL: localhost:3306 (billiard/billiard123)
- Redis: localhost:6379
- phpMyAdmin: localhost:8080 (可选)
- Redis Commander: localhost:8081 (可选)

## 启动命令
```bash
docker compose up -d
```

## 完成情况
✅ 已完成 - docker-compose.yml 已创建并验证

**估时:** 2h  
**优先级:** P0  
**任务组:** 开发环境
"@

CreateIssue "S0-008: 设置数据库迁移工具和脚本" @("task", "devops", "sprint0", "in-progress") @"
## 描述
建立数据库版本控制和迁移机制

## 验收标准
- [x] migrate.sh 脚本可执行
- [x] 支持数据库连接检查
- [x] 支持 --reset 重置选项
- [ ] EF Core Migrations 框架配置
- [ ] 可重复执行迁移

## 当前状态
🚧 部分完成
- ✅ migrate.sh 脚本框架已完成
- ⏸️ EF Core Migrations 集成待实现

## 后续工作
- 配置EF Core Migrations
- 测试迁移的可重复执行
- 集成到CI/CD流程

**估时:** 2h  
**优先级:** P1  
**任务组:** 开发环境
"@

# Backend Project Tasks
CreateIssue "S0-009: 创建ASP.NET Core项目结构" @("task", "backend", "sprint0", "completed") @"
## 描述
初始化后端项目和分层架构

## 验收标准
- [x] BilliardHall.sln 解决方案
- [x] BilliardHall.Api - Web API层
- [x] BilliardHall.Domain - 领域层
- [x] BilliardHall.Infrastructure - 基础设施层
- [x] 项目间引用关系正确
- [x] NuGet包版本统一 (EF Core 8.0.2)

## 项目结构
```
src/
├── BilliardHall.sln
├── BilliardHall.Api/          # Web API层
├── BilliardHall.Domain/       # 领域层  
└── BilliardHall.Infrastructure/ # 基础设施层
```

## 完成情况
✅ 已完成 - 项目结构已创建，编译成功

**估时:** 3h  
**优先级:** P0  
**任务组:** 后端项目
"@

CreateIssue "S0-010: 实现核心领域实体类" @("task", "backend", "sprint0", "completed") @"
## 描述
实现对应数据库表的实体类

## 验收标准
- [x] 11个核心实体类定义
- [x] 属性映射与数据库字段对应
- [x] 适当的数据类型和约束
- [x] 遵循DDD实体设计原则

## 实体清单
Store, BilliardTable, User, TableSession, BillingSnapshot, PaymentOrder, Device, DeviceHeartbeat, EventLog, PricingRule

## 完成情况
✅ 已完成 - BilliardHall.Domain/Entities.cs 已实现

**估时:** 2h  
**优先级:** P0  
**任务组:** 后端项目
"@

CreateIssue "S0-011: 配置EF Core DbContext" @("task", "backend", "sprint0", "completed") @"
## 描述
配置DbContext和实体映射

## 验收标准
- [x] BilliardHallDbContext 类实现
- [x] 所有实体的Fluent API配置
- [x] 表名、字段名、约束映射正确
- [x] MySQL连接字符串配置
- [x] DbSet属性定义

## 技术实现
- Pomelo MySQL Provider 8.0.2
- Fluent API 配置实体映射
- 索引和约束正确映射

## 完成情况
✅ 已完成 - DbContext 已实现并配置

**估时:** 3h  
**优先级:** P0  
**任务组:** 后端项目
"@

CreateIssue "S0-012: 实现健康检查API" @("task", "backend", "sprint0", "completed") @"
## 描述
实现监控数据库和缓存连接状态的API

## 验收标准
- [x] GET /health 基础健康检查
- [x] 数据库连接检查
- [x] Redis连接检查 (模拟实现)
- [x] 结构化健康检查响应
- [x] 集成ASP.NET Core HealthChecks

## API 端点
- GET /health - 综合健康检查
- GET /health/ready - 就绪状态检查

## 完成情况
✅ 已完成 - 健康检查API已实现

**估时:** 1h  
**优先级:** P0  
**任务组:** 后端项目
"@

CreateIssue "S0-013: 实现基础CRUD API" @("task", "backend", "sprint0", "completed") @"
## 描述
实现门店、球台等基础数据的查询接口

## 验收标准
- [x] GET /api/stores - 门店列表查询
- [x] GET /api/tables - 球台列表查询 (支持门店筛选)
- [x] 集成Swagger文档生成
- [x] 异步数据访问实现
- [x] 基础错误处理

## API 清单
- GET / - 服务信息
- GET /api/stores - 门店列表
- GET /api/tables?storeId=1 - 球台查询

## 完成情况
✅ 已完成 - 基础API已实现，Swagger文档可用

**估时:** 2h  
**优先级:** P1  
**任务组:** 后端项目
"@

CreateIssue "S0-014: 实现事件追踪API" @("task", "backend", "sprint0", "completed") @"
## 描述
实现统一的事件追踪接口

## 验收标准
- [x] POST /api/track 事件上报接口
- [x] 批量事件处理
- [x] 事件数据持久化到 event_log 表
- [x] JSON序列化和反序列化
- [x] 统一响应格式

## 实现特性
- 支持批量事件上报
- 自动JSON序列化事件载荷
- 统一错误处理和响应格式

## 完成情况
✅ 已完成 - 事件追踪API已实现

**估时:** 2h  
**优先级:** P0  
**任务组:** 后端项目
"@

# Documentation Tasks
CreateIssue "S0-015: 创建项目根目录README" @("task", "documentation", "sprint0", "completed") @"
## 描述
创建完整的项目介绍和快速启动指南

## 验收标准
- [x] 项目概述和功能介绍
- [x] 快速开始指南 (Docker + 本地开发)
- [x] 项目结构说明
- [x] API接口文档和示例
- [x] 开发规范 (Git工作流、提交规范、代码风格)
- [x] 测试和部署指南
- [x] 监控指标定义
- [x] 故障排除手册

## 文档章节
1. 快速开始 (Docker方式 + 本地方式)
2. 项目结构
3. 核心功能 (V0.1 + V0.2)
4. API接口
5. 开发规范
6. 测试
7. 部署
8. 监控指标
9. 故障排除

## 完成情况
✅ 已完成 - 根目录README.md已创建，内容完整

**估时:** 2h  
**优先级:** P0  
**任务组:** 文档
"@

CreateIssue "S0-016: 更新文档总览索引" @("task", "documentation", "sprint0", "completed") @"
## 描述
更新文档总览，加入新的架构和事件文档链接

## 验收标准
- [x] 更新 docs/README.md
- [x] 添加架构设计文档链接
- [x] 添加事件Schema文档链接  
- [x] 添加Backlog估点文档链接
- [x] 保持文档分类清晰

## 新增文档链接
- 架构设计: C4 L2容器图
- 事件Schema: P0事件定义
- Backlog估点: 工作量估算

## 完成情况
✅ 已完成 - 文档索引已更新

**估时:** 0.5h  
**优先级:** P1  
**任务组:** 文档
"@

# Project Management Tasks
CreateIssue "S0-017: 完成Backlog工作量估算" @("task", "management", "sprint0", "completed") @"
## 描述
详细分解并估算后续开发工作量

## 验收标准
- [x] Sprint 0 任务完成情况统计
- [x] V0.1 详细任务分解 (API开发、基础设施、测试、部署)
- [x] 工时估算和优先级排序
- [x] 风险评估和缓解措施
- [x] 里程碑检查点定义

## 估时统计
- Sprint 0: 24h (已完成15h, 62.5%)
- V0.1: 135h (17工作日)
- 总计: 144h (18工作日)

## 工作分解
- 核心API开发: 62h
- 基础设施: 26h
- 测试: 36h
- 部署运维: 11h

## 完成情况
✅ 已完成 - docs/backlog-estimation.md已创建

**估时:** 2h  
**优先级:** P0  
**任务组:** 项目管理
"@

CreateIssue "S0-018: 创建Sprint 0交付总结" @("task", "management", "sprint0", "completed") @"
## 描述
总结所有交付物和完成情况

## 验收标准
- [x] 交付物详情清单
- [x] 技术验证结果
- [x] 质量保证评估
- [x] 下一步行动计划
- [x] 团队就绪度评估

## 交付总结
- 架构设计完成 (C4图 + ADR)
- 数据库Schema v1完成
- 7个P0事件Schema定义
- ASP.NET Core项目和基础API
- Docker开发环境
- 完整文档体系

## 完成情况
✅ 已完成 - docs/sprint0-delivery-summary.md已创建

**估时:** 1h  
**优先级:** P1  
**任务组:** 项目管理
"@

# Quality Assurance Tasks
CreateIssue "S0-019: 代码构建验证" @("task", "qa", "sprint0", "completed") @"
## 描述
验证代码编译和构建成功

## 验收标准
- [x] dotnet build 编译成功
- [x] 无编译错误和警告
- [x] 包引用版本兼容
- [x] 项目间依赖正确

## 验证结果
✅ 构建成功
- 解决方案编译通过
- EF Core 8.0.2版本统一
- 项目引用关系正确

## 完成情况
✅ 已完成 - 构建验证通过

**估时:** 0.5h  
**优先级:** P0  
**任务组:** 质量保证
"@

CreateIssue "S0-020: 环境集成测试" @("task", "qa", "sprint0", "in-progress") @"
## 描述
测试Docker环境和数据库连接

## 验收标准
- [x] Docker Compose 成功启动
- [ ] 数据库连接正常
- [ ] 健康检查端点返回正常
- [ ] API文档可访问 (Swagger)
- [ ] 基础API接口响应正常

## 当前状态
🚧 部分完成
- ✅ Docker Compose配置正确
- ⏸️ 运行时集成测试待验证

## 后续工作
- 启动Docker环境测试
- 验证健康检查API
- 测试基础CRUD接口
- 验证事件追踪API

**估时:** 1h  
**优先级:** P0  
**任务组:** 质量保证
"@

Write-Host ""
Write-Host "📊 Sprint 0 任务创建完成!"
Write-Host ""
Write-Host "总计: 20个子任务"
Write-Host "✅ 已完成: 18个任务"
Write-Host "🚧 进行中: 2个任务"
Write-Host ""
Write-Host "任务分组："
Write-Host "  🏗️ 架构设计: 2个任务"
Write-Host "  💾 数据库设计: 2个任务"
Write-Host "  📊 事件追踪: 2个任务"
Write-Host "  🐳 开发环境: 2个任务"
Write-Host "  💻 后端项目: 6个任务"
Write-Host "  📖 文档: 2个任务"
Write-Host "  📊 项目管理: 2个任务"
Write-Host "  🔍 质量保证: 2个任务"
Write-Host ""

if ($DryRun) {
    Write-Host "🔍 DRY RUN模式 - 未实际创建GitHub Issues"
    Write-Host "运行时去除 -DryRun 参数以实际创建Issues"
} elseif ($script:hasErrors) {
    Write-Host ""
    Write-Host "⚠️  GitHub Issues创建过程中发生错误!"
    Write-Host "请检查上面的错误信息并解决认证问题"
    Write-Host ""
    Write-Host "备选方案:"
    Write-Host "1. 查看手动创建指南: docs/github-issues-manual-creation-guide.md"
    Write-Host "2. 使用JSON数据文件: docs/github-issues-data.json"
    Write-Host "3. 通过GitHub API创建Issues"
} else {
    Write-Host "✅ GitHub Issues创建完成!"
    Write-Host "请在GitHub仓库中查看创建的Issues"
}