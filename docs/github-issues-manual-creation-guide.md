# Sprint 0 GitHub Issues 手动创建指南

## 问题说明

由于GitHub CLI在当前环境中没有适当的认证令牌，自动创建GitHub Issues的脚本未能成功执行。需要手动创建或在有认证权限的环境中执行。

## 解决方案

### 方案1: 手动创建GitHub Issues

按照以下详细信息在GitHub仓库中手动创建20个Sprint 0子任务Issues：

#### 🏗️ 架构设计任务组

**Issue 1: S0-001: 设计并创建 C4 Level 2 容器架构图**
- **标签**: `task`, `architecture`, `sprint0`, `completed`
- **描述**:
```
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
```

**Issue 2: S0-002: 编写架构决策记录 (ADR)**
- **标签**: `task`, `architecture`, `sprint0`, `completed`
- **描述**:
```
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
```

#### 💾 数据库设计任务组

**Issue 3: S0-003: 设计并实现数据库Schema v1**
- **标签**: `task`, `database`, `sprint0`, `completed`
- **描述**:
```
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
```

**Issue 4: S0-004: 创建种子数据**
- **标签**: `task`, `database`, `sprint0`, `completed`
- **描述**:
```
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
```

#### 📊 事件追踪系统任务组

**Issue 5: S0-005: 设计P0级别事件JSON Schema**
- **标签**: `task`, `events`, `sprint0`, `completed`
- **描述**:
```
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
```

**Issue 6: S0-006: 设计统一事件上报API契约**
- **标签**: `task`, `events`, `sprint0`, `completed`
- **描述**:
```
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
```

### 方案2: 使用GitHub CLI手动执行脚本

如果您有GitHub CLI的访问权限，可以按照以下步骤执行：

1. **设置GitHub令牌**:
```bash
export GH_TOKEN="your_github_token"
# 或者
gh auth login
```

2. **执行脚本**:
```bash
pwsh scripts/create-sprint0-subtasks.ps1
```

### 方案3: 使用GitHub API

创建一个包含所有Issues数据的JSON文件，可以通过API批量创建：

```bash
# 使用curl创建单个Issue示例
curl -X POST \
  -H "Authorization: token YOUR_TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  https://api.github.com/repos/douhuaa/Zss.BilliardHall/issues \
  -d '{
    "title": "S0-001: 设计并创建 C4 Level 2 容器架构图",
    "body": "详细描述...",
    "labels": ["task", "architecture", "sprint0", "completed"]
  }'
```

## 完整Issues清单

由于无法自动创建，以下是需要手动创建的所有20个Issues的完整信息：

1. **S0-001**: 设计并创建 C4 Level 2 容器架构图 (架构设计)
2. **S0-002**: 编写架构决策记录 (ADR) (架构设计)
3. **S0-003**: 设计并实现数据库Schema v1 (数据库设计)
4. **S0-004**: 创建种子数据 (数据库设计)
5. **S0-005**: 设计P0级别事件JSON Schema (事件追踪)
6. **S0-006**: 设计统一事件上报API契约 (事件追踪)
7. **S0-007**: 搭建Docker Compose开发环境 (开发环境)
8. **S0-008**: 设置数据库迁移工具和脚本 (开发环境) [进行中]
9. **S0-009**: 创建ASP.NET Core项目结构 (后端项目)
10. **S0-010**: 实现核心领域实体类 (后端项目)
11. **S0-011**: 配置EF Core DbContext (后端项目)
12. **S0-012**: 实现健康检查API (后端项目)
13. **S0-013**: 实现基础CRUD API (后端项目)
14. **S0-014**: 实现事件追踪API (后端项目)
15. **S0-015**: 创建项目根目录README (文档)
16. **S0-016**: 更新文档总览索引 (文档)
17. **S0-017**: 完成Backlog工作量估算 (项目管理)
18. **S0-018**: 创建Sprint 0交付总结 (项目管理)
19. **S0-019**: 代码构建验证 (质量保证)
20. **S0-020**: 环境集成测试 (质量保证) [进行中]

## 进度统计

- **已完成**: 18个任务 (90%)
- **进行中**: 2个任务 (10%)
- **总计**: 20个任务

每个Issue的完整描述和验收标准请参考 `docs/sprint0-subtasks-breakdown.md` 文档。