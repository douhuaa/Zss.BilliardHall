# C4 Level 2 - Container Diagram

## 系统架构概览（Billiard Hall Management System）

```mermaid
C4Container
    title Container Diagram - Billiard Hall Management System V0.1

    Person(customer, "顾客", "使用手机扫码开台")
    Person(staff, "店员", "管理台球厅运营")
    Person(admin, "管理员", "系统配置与监控")

    System_Boundary(billiard_system, "Billiard Hall System") {
        Container(web_app, "Web应用", "ASP.NET Core", "提供用户界面和API")
        Container(api_gateway, "API网关", "ASP.NET Core", "统一入口，路由分发")
        Container(core_service, "核心服务", "ASP.NET Core", "业务逻辑：开台、计费、支付")
        Container(device_service, "设备服务", "ASP.NET Core", "设备管理与心跳监控")
        Container(track_service, "埋点服务", "ASP.NET Core", "事件追踪与分析")
        
        ContainerDb(database, "主数据库", "MySQL 8+", "存储业务数据")
        Container(event_store, "事件存储", "MySQL/JSON", "事件日志与审计")
        Container(cache, "缓存层", "Redis/Memory", "会话状态与计费缓存")
    }

    System_Ext(payment_gateway, "支付网关", "第三方支付服务")
    System_Ext(device_hardware, "硬件设备", "台球桌控制器")

    Rel(customer, web_app, "扫码开台", "HTTPS")
    Rel(staff, web_app, "管理操作", "HTTPS")
    Rel(admin, web_app, "系统配置", "HTTPS")

    Rel(web_app, api_gateway, "API调用", "HTTPS")
    Rel(api_gateway, core_service, "业务请求", "HTTP")
    Rel(api_gateway, device_service, "设备控制", "HTTP")
    Rel(api_gateway, track_service, "事件上报", "HTTP")

    Rel(core_service, database, "读写数据", "TCP")
    Rel(core_service, event_store, "记录事件", "TCP")
    Rel(core_service, cache, "缓存访问", "TCP")
    Rel(core_service, payment_gateway, "支付调用", "HTTPS")

    Rel(device_service, database, "设备状态", "TCP")
    Rel(device_service, device_hardware, "控制指令", "TCP/HTTP")
    Rel(device_hardware, device_service, "心跳上报", "HTTP")

    Rel(track_service, event_store, "事件入库", "TCP")

    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="2")
```

## 架构决策记录 (ADR)

### ADR-001: 技术栈选择

**状态:** 接受  
**日期:** 2024-09  

**背景:**  
需要选择后端技术栈以支持快速开发和部署。

**决策:**  
- 后端：ASP.NET Core 8+ (ABP框架可选)
- 数据库：MySQL 8+ (支持JSON字段)
- 缓存：Redis (开发阶段可用内存缓存)
- 事件：抽象EventBus接口，初期内存实现

**理由:**  
- .NET生态成熟，开发效率高
- MySQL稳定可靠，JSON支持满足灵活存储需求
- 架构可分层，便于后续扩展

**后果:**  
- 团队需要.NET技能
- 部署依赖.NET运行时

### ADR-002: 分层架构

**状态:** 接受  
**日期:** 2024-09  

**背景:**  
确定代码组织结构以支持可维护性。

**决策:**  
采用DDD分层架构：
- API层：控制器和DTO
- Domain层：业务逻辑和实体
- Infrastructure层：数据访问和外部服务

**理由:**  
- 职责分离清晰
- 便于单元测试
- 支持业务复杂度增长

### ADR-003: 数据存储策略

**状态:** 接受  
**日期:** 2024-09  

**背景:**  
V0.1阶段数据存储需求相对简单。

**决策:**  
- 单一MySQL实例
- JSON字段存储灵活配置
- Migration工具管理schema变更

**理由:**  
- 简化运维复杂度
- JSON字段支持配置灵活性
- Migration保证数据一致性

## 部署架构

### V0.1 简化部署

```
[Load Balancer] 
    ↓
[Web App + API Gateway] 
    ↓
[Core Service + Device Service]
    ↓
[MySQL + Redis]
```

### 后续扩展方向

- 微服务拆分（V0.2+）
- 消息队列引入（V0.2+）
- 多数据中心（V0.3+）

## 质量属性

| 属性 | V0.1目标 | 策略 |
|------|---------|------|
| 可用性 | 99% | 健康检查 + 重启 |
| 性能 | 开台<300ms | 缓存 + 索引优化 |
| 安全性 | 基础防护 | HTTPS + 输入验证 |
| 可观测性 | 基础日志 | 结构化日志 + 指标 |

## 外部依赖

| 系统 | 用途 | 协议 | 备注 |
|------|------|------|------|
| 支付网关 | 在线支付 | HTTPS/JSON | Sandbox环境 |
| 设备硬件 | 台球桌控制 | HTTP/TCP | 模拟器开发 |

---

**评审状态:** 待评审  
**创建日期:** 2024-09  
**版本:** v1.0