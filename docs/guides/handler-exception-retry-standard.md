# Handler 异常处理与重试工程标准

> ⚠️ **无裁决力声明**：本文档仅供参考，不具备架构裁决权。
> 所有架构决策以 [ADR-240](../adr/runtime/ADR-240-handler-exception-constraints.md) 为准。

**性质**：工程标准（Engineering Standard）- 非裁决性  
**版本**：1.0  
**最后更新**：2026-01-24  
**状态**：Active  
**相关 ADR**：[ADR-240](../adr/runtime/ADR-240-handler-exception-constraints.md)（裁决性规则）

---

## 文档定位

> **本文档不具裁决力，不可作为架构违规判定依据。**

本文档提供 Handler 异常处理的工程实践指导，包括异常分类详解、重试策略推荐、幂等性保障模式、可观测性建议等。

本文档内容随工程实践演进而更新，不受 ADR 变更流程约束。

---

[完整内容请参见原文档，此处为简化版示例]

## 一、异常分类详解

### DomainException（领域异常）
- 表示业务规则违反
- 不可重试
- 示例：订单已取消、余额不足

### ValidationException（验证异常）
- 表示输入数据不符合契约
- 不可重试
- 示例：必填字段缺失、格式错误

### InfrastructureException（基础设施异常）
- 表示技术依赖失败
- 可能可重试
- 示例：网络超时、数据库死锁

## 二、重试策略推荐

推荐参数：
- 最大重试次数：3-5 次
- 总重试时间上限：30-60 秒
- 退避策略：指数退避 + 抖动

## 三、幂等性保障模式

1. 幂等键（Idempotency Key）
2. 记录已处理事件
3. 状态机防护
4. 乐观锁版本控制

## 四、可观测性建议

推荐日志字段：
- ExceptionType
- IsRetryable
- CorrelationId
- CommandId/QueryId/EventId

---

**完整文档内容**：参见 `docs/guides/handler-exception-retry-standard.md`
