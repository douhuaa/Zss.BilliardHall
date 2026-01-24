# 技术方案层 ADR (ADR-300 ~ 399)

**编号范围**：ADR-300 ~ ADR-399  
**层级定位**：技术选型 / 具体实现 / 最佳实践

---

## 定义

技术方案层 ADR 定义具体的技术选型、工具使用、实现细节等：

- **框架选型**：如使用 Wolverine 作为消息总线
- **库的使用**：如使用 FluentValidation、MediatR 等
- **工具链**：如使用的构建工具、测试框架
- **最佳实践**：如日志记录、监控、部署策略
- **技术标准**：如 API 设计规范、数据库约定

---

## 与宪法层的关系

技术方案层是宪法层原则的具体落地：

```
宪法层（技术无关原则）
    ↓
技术方案层（技术相关实现）

例如：
- 宪法层定义"模块间异步通信"
- 技术层选择"使用 Wolverine + RabbitMQ"
```

**重要**：技术方案层 ADR 可以被替换或升级，只要不违反宪法层约束。

---

## 编号规则

| 编号段         | 用途          |
|-------------|-------------|
| ADR-300~309 | 框架与基础设施选型   |
| ADR-310~319 | 数据访问与持久化    |
| ADR-320~329 | API 设计与通信协议 |
| ADR-330~339 | 安全与认证授权     |
| ADR-340~349 | 日志、监控与可观测性  |
| ADR-350~359 | 测试策略与工具     |
| ADR-360~369 | 部署与运维       |
| ADR-370~399 | 其他技术主题      |

---

## 当前 ADR 列表

### 日志、监控与可观测性 (340~349)

| 编号 | 标题 | 状态 | 文件 |
|------|------|------|------|
| **ADR-340** | 结构化日志与监控约束 | ✅ Final | [ADR-340-structured-logging-monitoring-constraints.md](ADR-340-structured-logging-monitoring-constraints.md) |

---

## 待落地的 ADR（已规划）

> 详见 [待落地 ADR 提案跟踪清单](../PENDING-ADR-PROPOSALS.md)

### 高优先级
- **ADR-301**：集成测试环境自动化与隔离约束（📋 Proposed）
  - TestContainers 使用规范、数据库隔离策略、测试数据清理
- **ADR-360**：CI/CD Pipeline 流程标准化（📋 Proposed）
  - 分支合规校验、失败快照、自动部署触发、分支保护

### 中优先级
- **ADR-350**：日志与可观测性标签与字段标准（📋 Proposed）
  - 结构化日志字段规范、CorrelationId 传播、追踪标签标准

### 未来可能的 ADR
- **ADR-302**：使用 Entity Framework Core 作为 ORM
- **ADR-310**：数据库迁移策略（Code First vs Database First）
- **ADR-311**：读写分离与 CQRS 物理实现
- **ADR-320**：RESTful API 设计规范
- **ADR-321**：API 版本化策略
- **ADR-330**：使用 JWT 进行身份认证
- **ADR-341**：应用性能监控（APM）选型

---

## 变更频率

技术方案层 ADR 的变更频率高于宪法层：

- **宪法层**：3-5 年稳定
- **技术方案层**：1-2 年可能升级或替换

这是正常的技术演进，只要新技术方案仍然遵守宪法层约束。

---

## 参考文档

- [架构宪法层说明](../constitutional/ARCHITECTURE-CONSTITUTIONAL-LAYER.md)
- [ADR 新增与修订流程](../governance/ADR-0900-adr-process.md)
- [ADR 总览](../README.md)
