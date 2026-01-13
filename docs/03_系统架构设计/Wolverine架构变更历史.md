# Wolverine 架构变更历史

> **目的**: 记录 Wolverine 模块化架构蓝图的演进历史，包含所有版本的变更说明和强化内容
>
> **最新版本**: v1.2.0
>
> **更新日期**: 2026-01-12

---

## 版本总览

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| v1.2.0 | 2026-01-12 | 架构师反馈强化：防护栏 + 破例机制 |
| v1.1.0 | 2026-01-12 | 社区反馈强化：风险缓解 + 架构升级 |
| v1.0.0 | - | 初始版本 |

---

# v1.2.0 架构师反馈强化总结

## 变更概述

基于资深架构师 @douhuaa 的深度评审反馈，对 Wolverine 模块化架构蓝图进行了第二轮强化。本次更新聚焦于"防傻"和"防护栏"，确保规范在实际应用中不被误用。

**版本**: v1.1.0 → v1.2.0  
**日期**: 2026-01-12  
**文档大小**: 2000 行 → 2231 行 (+11.5%)  
**Commit**: d168b7d

---

## 🛡️ 强化的 8 个关键点

### 1. BuildingBlocks 第 5 条隐含规则

**问题**: "3 个模块使用"是必要条件但不充分

**强化内容**:
```
✅ 必须满足以下所有条件:
5. 抽象后修改成本真的降低（隐含条件）

⚠️ 残酷的真相: "3 个模块使用" ≠ "值得抽象"

判断标准：抽象之后，修改成本是否真的下降？
- 如果答案是否定的，哪怕 5 个模块在用，也不要抽
- 宁可重复，不要错误的抽象
```

**目的**: 防止"抽取烂设计"变成祖宗牌位

---

### 2. ErrorCodes 高级陷阱警告

**问题**: ErrorCodes 太好用，容易滑向业务语义

**强化内容**:
```
⚠️ ErrorCodes 的高级陷阱

铁律: ErrorCodes 只允许表达"失败类型"，不允许表达"业务决策原因"

✅ 好的：表达失败类型
- ErrorCodes.Tables.NotFound
- ErrorCodes.Tables.InvalidStatus

❌ 危险的：表达业务决策原因
- ErrorCodes.Tables.CannotReserveAtNight      // 业务规则！
- ErrorCodes.Members.MemberLevelTooLow        // 业务规则！

正确做法: 业务决策相关的错误码必须在模块内定义
```

**防护建议**:
- Code Review 时严格审查 ErrorCodes 新增项
- 问自己：这是"技术失败"还是"业务决策"？
- 即使字符串重复，也认了

---

### 3. Module Event 显式声明要求

**问题**: Module Event 最容易被"随便用"

**强化内容**:
```
⚠️ Module Event 的特殊风险

现实真相: Module Event 是最容易被"随便用"的事件层级

强制要求: Module Event 必须被显式声明为"对外事件"

推荐做法:

1. 文件夹命名区分:
Modules/Sessions/
├── Events/                    # 内部事件（Domain Event）
│   └── SessionStateChanged.cs
└── PublicEvents/              # 对外事件（Module Event）
    ├── SessionStarted.cs
    └── SessionEnded.cs

2. 或使用注释/Attribute 标记:
/// <summary>
/// 会话开始事件（Module Event - 对外契约）
/// </summary>
/// <remarks>
/// 消费者：
/// - Billing 模块（计费开始）
/// - Devices 模块（设备控制）
/// 修改此事件需通知所有消费方
/// </remarks>
```

**目的**: 让作者在创建时意识到——这是契约，不是内部玩具

---

### 4. Integration Event 不可修改铁律

**问题**: "严格版本管理"不够狠

**强化内容**:
```
⚠️ Integration Event 不可修改铁律

一旦发布，视为"只增不改"

现实规则：
- ❌ 不改字段含义
- ❌ 不删字段
- ✅ 只能加字段（可选）
- ⚠️ 老字段哪怕废弃也要留

正确的演进方式:
// V1 - 初始版本
public sealed record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    decimal Amount
) : IIntegrationEvent;

// V2 - 新增字段（向后兼容）
public sealed record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    decimal Amount,
    string? Currency = "CNY"  // 新增可选字段
) : IIntegrationEvent;
```

**防止反噬**:
- Kafka / RabbitMQ 历史消息
- Outbox 重放
- 跨服务版本不一致

---

### 5. Saga 心理刹车

**问题**: 团队对 Saga 态度"合理即用"

**强化内容**:
```
## 五、Saga（跨步骤业务流程）

⚠️ 警告: Saga 是重武器，不是常规武器！误用会导致"状态机地狱"

💡 心理刹车: 如果你在犹豫要不要用 Saga，答案通常是：不要
```

**目的**: 提供默认决策偏好，避免过度使用

---

### 6. Handler 认知负债说明

**问题**: 行数限制缺乏心理学解释

**强化内容**:
```
认知负债真相: Handler 超行数，本质上是"认知负债"，不是代码问题

| 行数范围 | 认知状态 |
|---------|---------|
| ≤ 40 行 | 业务语义清晰 |
| 41-60 行 | 作者已经 hold 不住完整业务语义 |
| 61-80 行 | 这个人已经在靠意志力写代码 |
| > 80 行 | 必须强制拆分 |

现实判断标准:
- Handler > 60 行 → 作者无法在脑海中维护完整业务流程
- Handler > 80 行 → 编码依靠毅力而非理解
```

**目的**: 从认知科学角度解释限制的必要性

---

### 7. 新增第十一章：何时可以打破这些规则

**问题**: 规则太严格，需要豁免机制

**新增内容**:

**可以破例的场景**:
- 小模块（< 5 个 UseCase）暂缓 Module Marker
- 内部工具模块可以更灵活
- 管理后台 CRUD 放宽到 60 行
- 原型阶段快速验证（需设定重构 deadline）

**破例的铁律**:
1. 写清楚理由（在代码注释或文档中）
2. 评估影响范围（只影响局部 vs 影响架构）
3. 设定归还债务的时间（技术债必须有还款计划）
4. 团队达成共识（不能个人私自决定）

**绝对不能破例的红线**:
- ❌ 在 BuildingBlocks 中放业务规则
- ❌ 跨服务使用 InvokeAsync
- ❌ 创建 Application/Domain/Infrastructure 分层
- ❌ 创建 Shared Service 跨模块直接调用
- ❌ Integration Event 破坏兼容性

**平衡原则**:
> 破例之后，是否让三年后的团队更难维护？

---

### 8. 版本历史更新

```
| 版本 | 变更说明 |
|------|----------|
| 1.2.0 | 架构师反馈强化：基于资深架构师深度评审，加强防护栏
        - 🛡️ BuildingBlocks 第 5 条隐含规则
        - 🛡️ ErrorCodes 高级陷阱警告
        - 🛡️ Module Event 显式声明要求
        - 🛡️ Integration Event 不可修改铁律强化
        - 💡 Saga 心理刹车
        - 💡 Handler 认知负债说明
        - 📖 新增第十一章：何时可以打破这些规则
        - 📖 破例铁律、红线清单、平衡原则 |
```

---

## 📊 影响分析

### 文档变化

| 指标 | v1.1.0 | v1.2.0 | 变化 |
|------|--------|--------|------|
| 总行数 | ~2000 | 2231 | +11.5% |
| 主要章节 | 63 | 64 | +1 |
| 警告/防护栏 | 12 | 20 | +67% |
| 破例指导 | 0 | 1章 | 新增 |

### 强化位置

- **2.3 BuildingBlocks**: +50 行（第 5 条规则 + ErrorCodes 陷阱）
- **2.4 事件分类**: +60 行（Module Event 声明 + Integration Event 铁律）
- **5.1 Saga 使用**: +5 行（心理刹车）
- **8.4 Handler 行数**: +15 行（认知负债）
- **第十一章**: +87 行（新增章节）
- **版本历史**: +10 行（v1.2.0 说明）

---

## 🎯 与原始反馈的对应

### 反馈 1: BuildingBlocks 要再狠一点

✅ **已实现**:
- 第 5 条隐含规则
- "3 个模块 ≠ 值得抽象"警告
- ErrorCodes 陷阱专节

### 反馈 2: 事件三分法要强化

✅ **已实现**:
- Module Event 显式声明要求
- Integration Event 只增不改铁律 + 演进示例

### 反馈 3: Saga 心理刹车

✅ **已实现**:
- 开篇加入"犹豫时默认不用"

### 反馈 4: Handler 行数的认知解释

✅ **已实现**:
- 60 行 = hold 不住业务语义
- 80 行 = 靠意志力写代码

### 反馈 5: 何时可以打破规则

✅ **已实现**:
- 新增第十一章
- 破例场景、铁律、红线、平衡原则

---

## ✅ 质量保证

- ✅ **Build Status**: 编译成功
- ✅ **Backward Compatibility**: 无破坏性变更
- ✅ **Documentation**: 完整且自洽
- ✅ **Feedback Integration**: 100% 反馈已整合
- ✅ **Readability**: 中英文混排，符合团队习惯

---

## 🚀 后续建议（可选）

基于评审者的三个可选建议：

1. **压缩为海报版** (10 条不可违背铁律)
   - 可作为团队墙报
   - Code Review 快速参考卡

2. **CI 级架构违规检测**
   - Handler 行数检测（已有 Roslyn Analyzer 示例）
   - BuildingBlocks 准入检测（文件数量、依赖检查）
   - ErrorCodes 业务语义检测（关键词黑名单）

3. **按本文档逐条对照体检**
   - 审计现有 Saga（是否满足 3 铁律）
   - 审计 BuildingBlocks（是否满足 3 模块规则）
   - 审计 Handler 行数（> 60 行的列表）

---

## 🙏 致谢

特别感谢 @douhuaa 的深度评审，反馈包含：
- 防护栏不够"防傻"的识别
- 现实世界滥用场景的预警
- 从认知科学角度的解释
- 平衡原则与破例机制的设计

这些反馈将蓝图从"可用"提升到"可持续"。

---

---

# v1.1.0 社区反馈强化总结

## 变更概述

本次更新基于社区反馈，针对 Wolverine 模块化架构蓝图进行了重大强化，解决了 4 个隐藏风险，并实施了 3 个架构升级建议。

**版本**: v1.0.0 → v1.1.0  
**日期**: 2026-01-12  
**文档大小**: 1100+ 行 → 2000+ 行 (+81%)  
**新增章节**: 8 个

---

## 🚨 四大隐藏风险缓解

### 风险 1: 事件边界不清晰

**问题**: 
- 事件类型混在一起，不知道是否跨模块/跨服务
- 修改事件时不知道影响范围
- "这个事件谁都在用，改不了"

**解决方案**:
- ✅ 明确三种事件层级：Domain Event、Module Event、Integration Event
- ✅ 新增 2.4 节《事件分类与边界管理》
- ✅ 创建 `BuildingBlocks/Contracts/IntegrationEvents/` 文件夹
- ✅ 提供事件升级路径和影响分析表

**关键产出**:
```
Domain Event → 模块内（可自由修改）
Module Event → 跨模块（需考虑消费者）
Integration Event → 跨服务（严格版本管理）
```

---

### 风险 2: Saga 滥用

**问题**:
- 团队对 Saga 态度是"合理即用"，导致过度使用
- "我只是想拆代码，结果引入了状态机地狱"

**解决方案**:
- ✅ 收紧 Saga 使用标准：必须满足 3 条铁律
- ✅ 更新 5.1 节《何时使用 Saga》
- ✅ 提供详细决策树和反模式示例

**Saga 使用三条铁律**:
1. ✅ 跨模块（2 个以上模块）
2. ✅ 跨时间（持续 > 1 分钟）
3. ✅ 需要补偿（不是简单回滚）

**不满足 → 使用 Handler 或 Event**

---

### 风险 3: Result<T> 错误模型失控

**问题**:
- 随意的错误消息，前端无法区分错误类型
- 日志无法聚合统计
- 重试策略失效
- 多语言支持困难

**解决方案**:
- ✅ 在 `Result<T>` 中添加 `ErrorCode` 属性
- ✅ 新增 8.2 节《Result<T> 错误模型管理》
- ✅ 创建 `ErrorCodes.cs` 提供结构化错误码
- ✅ 错误码格式：`{Area}:{Key}`

**示例**:
```csharp
// 之前
Result.Fail<Guid>("台球桌不可用");  // 无法识别

// 现在
Result.Fail<Guid>("台球桌不可用", ErrorCodes.Tables.Unavailable);
// 前端可识别、日志可聚合
```

---

### 风险 4: BuildingBlocks 污染

**问题**:
- 99% 的团队会在 BuildingBlocks 上失败
- 只要 2 个模块用到就抽取，导致过度抽象
- BuildingBlocks 变成"工具箱"而非"防腐层"

**解决方案**:
- ✅ 强化 2.3 节《BuildingBlocks 防污染铁律》
- ✅ 明确"3 个模块真实使用"规则
- ✅ 提供严格的准入标准和审核检查清单
- ✅ 添加决策流程图

**铁律**:
```
任何进入 BuildingBlocks 的代码，必须：
1. 被 3 个以上模块真实使用
2. 跨模块不可避免
3. 没有业务语义
4. 不会频繁变更

不到 3 个 → 复制、忍着、不准抽
```

---

## 🔧 三大架构升级

### 升级 1: 显式 Module Marker

**目标**: 每个模块都有明确的身份标识

**实现**:
- ✅ 创建 `IWolverineModule` 接口
- ✅ 新增 3.3 节《模块标记》
- ✅ 说明 4 个核心职责

**Module Marker 职责**:
1. 自动模块扫描
2. 权限边界管理
3. Feature Toggle 配置
4. 模块级日志追踪

**示例**:
```csharp
public sealed class TablesModule : IWolverineModule
{
    public static string ModuleName => "Tables";
}
```

---

### 升级 2: 禁止跨进程同步命令

**目标**: 明确 `InvokeAsync` 只能用于进程内

**实现**:
- ✅ 新增 6.2 节《跨进程同步命令的铁律》
- ✅ 明确 InvokeAsync 的使用边界
- ✅ 提供跨服务通信的 3 种正确方式
- ✅ 添加防护措施和检查清单

**核心原则**:
```
✅ 进程内跨模块 → InvokeAsync
❌ 跨服务 → 禁止 InvokeAsync
✅ 跨服务 → Event / Saga / HTTP
```

---

### 升级 3: Handler 行数限制

**目标**: 通过行数限制防止业务逻辑失控

**实现**:
- ✅ 更新 8.4 节《Handler 行数限制（团队规范）》
- ✅ 明确 40/60/80 行三级限制
- ✅ 提供重构策略和检查清单

**三级限制**:
```
≤ 40 行 → ✅ 通过审查
41-60 行 → ⚠️ Code Review 重点检查
61-80 行 → ❌ 禁止合并
> 80 行 → 🚨 架构问题，必须重构
```

---

## 📝 文档结构优化

### 新增章节

1. **2.3 BuildingBlocks 防污染铁律** (约 100 行)
2. **2.4 事件分类与边界管理** (约 150 行)
3. **3.3 模块标记（Module Marker）** (约 120 行)
4. **5.1 何时使用 Saga（收紧标准）** (约 150 行)
5. **6.2 跨进程同步命令的铁律** (约 120 行)
6. **8.2 Result<T> 错误模型管理** (约 180 行)
7. **8.4 Handler 行数限制（团队规范）** (约 200 行)
8. **第十一章 关键要点速查表** (约 150 行)

### 更新章节

- 2.2 关键设计原则 - 强化 BuildingBlocks 说明
- 5.1 何时使用 Saga - 完全重写，收紧标准
- 6.1 通信方式选择 - 添加跨进程限制
- 8.1 先照抄，再优化 - 添加错误码阶段
- 第十二章 版本历史 - 更新为 v1.1.0

---

## 🛠️ 代码变更

### 1. Result<T> 增强

**文件**: `src/Wolverine/BuildingBlocks/Contracts/Result.cs`

```csharp
// 新增 ErrorCode 属性
public string? ErrorCode { get; }

// 新增工厂方法
public static Result Fail(string error, string errorCode)
public static Result<T> Fail<T>(string error, string errorCode)
```

### 2. IWolverineModule 接口

**文件**: `src/Wolverine/BuildingBlocks/Contracts/IWolverineModule.cs`

```csharp
public interface IWolverineModule
{
    static abstract string ModuleName { get; }
}
```

### 3. ErrorCodes 常量类

**文件**: `src/Wolverine/BuildingBlocks/Contracts/ErrorCodes.cs`

包含 6 个模块的结构化错误码：
- Tables
- Sessions
- Billing
- Payments
- Members
- Devices

### 4. Integration Events 文件夹

**路径**: `src/Wolverine/BuildingBlocks/Contracts/IntegrationEvents/`

包含完整的 README.md，说明：
- Integration Event 的用途和规范
- 与其他事件类型的区别
- 准入标准和修改影响
- 反模式和示例

---

## 📊 影响分析

### 文档增强

| 指标 | v1.0.0 | v1.1.0 | 增长 |
|------|--------|--------|------|
| 总行数 | ~1100 | ~2000 | +81% |
| 主要章节 | 55 | 63 | +15% |
| 代码示例 | ~50 | ~80 | +60% |
| 检查清单 | 3 | 8 | +167% |

### 代码变更

| 变更类型 | 数量 |
|---------|------|
| 新增文件 | 4 |
| 修改文件 | 2 |
| 新增接口 | 1 |
| 新增常量类 | 1 |
| Result 增强 | 3 个方法 |

### 向后兼容性

✅ **完全向后兼容**:
- Result<T> 新增可选参数，不影响现有代码
- IWolverineModule 是新接口，不强制实现
- ErrorCodes 是新增的常量类
- 所有现有代码可正常编译和运行

⚠️ **推荐迁移**:
- 新代码应使用 ErrorCode
- 新模块应实现 IWolverineModule
- 跨服务事件应移到 IntegrationEvents

---

## ✅ Code Review 新增检查项

### 架构层面
- [ ] 是否有共享服务（应拒绝）
- [ ] BuildingBlocks 新增是否满足 3 模块规则
- [ ] 跨服务调用是否避免 InvokeAsync

### 事件层面
- [ ] 事件是否明确分类（Domain/Module/Integration）
- [ ] Integration Event 是否在 BuildingBlocks/Contracts
- [ ] Module Event 是否有消费者文档

### Handler 层面
- [ ] Handler 行数 ≤ 40 行（或有合理理由）
- [ ] 业务失败是否返回 ErrorCode
- [ ] 是否使用 [Transactional] 特性

### Saga 层面
- [ ] 是否满足 Saga 三条铁律
- [ ] 是否可以用 Handler 或 Event 替代
- [ ] 是否有超时和补偿逻辑

---

## 📚 参考资源

### 项目文档
- `docs/03_系统架构设计/Wolverine模块化架构蓝图.md` - 主文档
- `docs/06_开发规范/Saga使用指南.md` - Saga 详细指南
- `docs/06_开发规范/FluentValidation集成指南.md` - 验证最佳实践

### 代码资源
- `src/Wolverine/BuildingBlocks/Contracts/Result.cs` - Result 类型
- `src/Wolverine/BuildingBlocks/Contracts/ErrorCodes.cs` - 错误码常量
- `src/Wolverine/BuildingBlocks/Contracts/IWolverineModule.cs` - 模块接口
- `src/Wolverine/BuildingBlocks/Contracts/IntegrationEvents/README.md` - 集成事件说明

---

## 🎯 下一步行动

### 立即执行
1. ✅ 审查现有模块是否需要实现 IWolverineModule
2. ✅ 评估现有错误处理是否需要添加 ErrorCode
3. ✅ 检查现有 Saga 是否满足三条铁律

### 短期（1-2 周）
1. 在 CI 中添加 Handler 行数检查
2. 更新 Code Review 模板，添加新检查项
3. 为现有模块添加 Module Marker

### 中期（1 个月）
1. 将现有 Module Event 迁移到明确的文件夹结构
2. 为关键业务流程添加 ErrorCode
3. 评估并重构超过 60 行的 Handler

### 长期（持续）
1. 监控 BuildingBlocks 准入，严格执行 3 模块规则
2. 定期审查 Saga 使用，避免滥用
3. 持续优化错误码体系

---

## 🙏 致谢

本次强化基于社区反馈和最佳实践，特别感谢：
- 提出 4 大隐藏风险的架构师
- 提供 3 大升级建议的技术专家
- 所有参与 Code Review 的团队成员

---

**文档版本**: v1.2.0  
**更新日期**: 2026-01-12  
**维护者**: 架构团队  
**社区反馈**: 已整合资深架构师深度评审意见及社区反馈
