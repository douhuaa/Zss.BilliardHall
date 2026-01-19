# ADR 与架构蓝图映射表

> **目的**: 将《Wolverine 模块化架构蓝图》中的关键决策映射到对应的 ADR，建立清晰的追溯关系。

## 映射原则

- **蓝图**：提供完整的架构视图、实践指南和代码示例
- **ADR**：记录关键决策的推理过程、权衡考虑和替代方案
- **关系**：蓝图是"What & How"，ADR 是"Why"

## 完整映射表

| 蓝图章节 | 对应 ADR | 内容说明 |
|---------|---------|---------|
| **序章：垂直切片架构理念** | [ADR-001](./ADR/ADR-001-采用垂直切片架构.md) | 为什么选择垂直切片而非传统分层 |
| **一、总体架构立场 > 1.1 核心原则 > 原则1** | [ADR-001](./ADR/ADR-001-采用垂直切片架构.md) | 100% 垂直切片的决策 |
| **一、总体架构立场 > 1.1 核心原则 > 原则4** | [ADR-002](./ADR/ADR-002-Handler即ApplicationService.md) | Handler 即 Application Service |
| **一、总体架构立场 > 1.1 核心原则 > 原则3** | [ADR-003](./ADR/ADR-003-模块间消息通信.md) | 通信方式分离原则 |
| **二、解决方案级 Blueprint > 2.3 BuildingBlocks 防污染铁律** | [ADR-004](./ADR/ADR-004-BuildingBlocks准入标准.md) | BuildingBlocks 5 条准入标准 |
| **二、解决方案级 Blueprint > 2.4 事件分类与边界管理** | [ADR-005](./ADR/ADR-005-事件分类体系.md) | Domain/Module/Integration Event 分类 |
| **六、跨模块通信 > 6.2 跨进程同步命令的铁律** | [ADR-006](./ADR/ADR-006-InvokeAsync使用限制.md) | InvokeAsync 仅限进程内 |
| **五、Saga > 5.1 何时使用 Saga** | [ADR-007](./ADR/ADR-007-Saga使用三条铁律.md) | Saga 使用的 3 条铁律 |
| **六、跨模块通信 > 级联消息** | [ADR-008](./ADR/ADR-008-级联消息优先策略.md) | 优先使用级联消息 |
| **六、跨模块通信 > 6.1 通信方式选择** | [ADR-009](./ADR/ADR-009-跨服务通信模式.md) | 跨服务通信模式 |
| **八、硬核实践建议 > 8.4 Handler 行数限制** | [ADR-010](./ADR/ADR-010-Handler行数限制.md) | Handler 40/60/80 规则 |
| **八、硬核实践建议 > 8.2 Result<T> 错误模型管理** | [ADR-011](./ADR/ADR-011-错误码规范.md) | Area:Key 错误码格式 |
| **二、解决方案级 Blueprint > 2.3 ErrorCodes 的高级陷阱** | [ADR-012](./ADR/ADR-012-ErrorCodes约束.md) | ErrorCodes 不承载业务语义 |
| **二、解决方案级 Blueprint > 2.4 Integration Event 不可修改铁律** | [ADR-013](./ADR/ADR-013-IntegrationEvent兼容性.md) | Integration Event 向后兼容 |
| **二、解决方案级 Blueprint > 2.1 Solution 结构** | [ADR-014](./ADR/ADR-014-Solution结构设计.md) | Bootstrapper/Modules/BuildingBlocks |
| **三、单个模块的"黄金结构" > 3.3 模块标记** | [ADR-015](./ADR/ADR-015-模块标记设计.md) | Module Marker 设计 |
| **三、单个模块的"黄金结构" > 3.1 模块目录组织** | [ADR-016](./ADR/ADR-016-模块黄金结构.md) | 单个模块的标准结构 |
| **十一、何时可以打破这些规则** | [ADR-017](./ADR/ADR-017-架构规则破例机制.md) | 破例机制和红线清单 |

## 使用指南

### 从蓝图到 ADR

**场景 1**：阅读蓝图时想了解"为什么"

1. 在蓝图中找到对应章节
2. 查看本映射表，找到对应 ADR
3. 阅读 ADR 了解决策背景和权衡

**示例**：
```
蓝图：2.3 BuildingBlocks 防污染铁律
  ↓
映射表：查找"BuildingBlocks"
  ↓
ADR-004：BuildingBlocks 严格准入标准
  ↓
了解：为什么是 5 条规则？为什么不是 3 条？
```

### 从 ADR 到蓝图

**场景 2**：阅读 ADR 后想了解"怎么做"

1. 在 ADR 的"参考资料"部分找到蓝图章节
2. 返回蓝图查看详细实践指南和代码示例

**示例**：
```
ADR-007：Saga 使用三条铁律
  ↓
参考资料：蓝图第五章
  ↓
蓝图：5.2 Saga 实现示例
  ↓
获取：完整的 Saga 代码示例
```

## 阅读路径建议

### 路径 1：快速上手（新人）

1. 阅读《Wolverine 快速上手指南》(15 分钟)
2. 阅读蓝图"序章：垂直切片架构理念"（10 分钟）
3. **可选**：阅读 ADR-001（理解为什么选择垂直切片）
4. 阅读蓝图"四、完整 Slice 的标准形态"（实践）

### 路径 2：深入理解（架构师）

1. 阅读蓝图全文（获取完整架构视图）
2. 针对关键决策点，查阅对应 ADR
3. 重点阅读：
   - ADR-001: 垂直切片架构
   - ADR-004: BuildingBlocks 准入标准
   - ADR-007: Saga 使用铁律
   - ADR-010: Handler 行数限制

### 路径 3：问题解决（日常开发）

**问题导向**：

- **问**：代码应该放在 BuildingBlocks 还是模块内？
  - **答**：ADR-004 + 蓝图 2.3 节

- **问**：什么时候用 Saga？
  - **答**：ADR-007 + 蓝图第五章

- **问**：Handler 太长怎么办？
  - **答**：ADR-010 + 蓝图 8.4 节

- **问**：跨服务能用 InvokeAsync 吗？
  - **答**：ADR-006 + 蓝图 6.2 节

## 维护指南

### 新增 ADR 时

1. 创建 ADR 文档（使用 ADR-000 模板）
2. 在 `ADR/README.md` 中添加索引条目
3. **在本映射表中添加对应关系**
4. 在蓝图相关章节中添加 ADR 引用链接

### 更新蓝图时

1. 如果涉及重大架构决策变更
2. 创建新的 ADR（标记旧 ADR 为"替代"）
3. 更新本映射表
4. 在蓝图中引用新 ADR

### 废弃 ADR 时

1. 标记 ADR 状态为"废弃"或"替代"
2. 在本映射表中更新为新 ADR
3. 保留旧 ADR 文档（供历史追溯）

## 相关资源

- [ADR 目录](./ADR/README.md) - 所有 ADR 的索引
- [Wolverine 模块化架构蓝图](./Wolverine模块化架构蓝图.md) - 完整架构文档
- [Wolverine 快速上手指南](./Wolverine快速上手指南.md) - 15 分钟入门

---

**维护者**: 架构团队  
**最后更新**: 2026-01-19
