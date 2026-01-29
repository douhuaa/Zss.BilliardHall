---
adr: ADR-906
title: "Analyzer / CI Gate 与 ADR 映射协议"
status: Superseded
level: Governance
version: "1.0"
deciders: "Architecture Board"
date: 2026-01-27
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: ADR-903-906
---

# ADR-906：Analyzer / CI Gate 与 ADR 映射协议

> 本 ADR 的全部裁决已被 [ADR-903-906](ADR-903-906.md) 吸收并强化，不再具有独立裁决力。
> ⚖️ **本 ADR 定义 CI / Analyzer 如何识别、执行、报告 ArchitectureTests 与 ADR 的映射关系，确保裁决自动化、可追溯、可阻断。**

---

## Focus（聚焦内容）

- 确保 ArchitectureTests 能被 CI / Analyzer 自动发现与执行
- 测试失败信息直接映射到具体 ADR 条目
- 支持 L1/L2 执法等级和破例机制
- 防止人为绕过或弱化架构裁决

**本 ADR 的唯一目标**：
> **让 CI / Analyzer 成为 ArchitectureTests 的“自动裁决执行者”，并保持 ADR 溯源性。**

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| CI Gate | 持续集成管道中自动执行架构测试、阻断违规变更的阶段 | CI Gate |
| Analyzer | 静态分析或运行时分析工具，用于验证架构规则 | Analyzer |
| Rule Mapping | 测试方法或类与 ADR 条目的对应关系 | Rule Mapping |
| Enforcement Level | 执行等级 L1 / L2，L1 可自动阻断，L2 半自动审核 | Enforcement Level |
| ADR 可追溯性 | 测试失败必须能定位到具体 ADR 子规则 | ADR Traceability |
| Exception Mechanism | ADR-0000 定义的破例/补救机制 | Exception Mechanism |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源。**

### ADR-906.1:L1 所有 ArchitectureTests 必须注册至 Analyzer / CI Gate

**规则**：
- 每个 ArchitectureTest 类 / 方法必须在 CI / Analyzer 执行列表中注册
- 注册必须使用规则映射表或约定命名扫描
- 禁止手动跳过注册

**示例**：
```text
Analyzer.Register("ADR-240.1", "ADR_240_1_HandlerMustNotCatchExceptionTests");
````

---

### ADR-906.2:L1 测试执行失败必须映射 ADR 子规则

**规则**：
- 所有 CI / Analyzer 失败日志必须包含 ADR 编号和规则描述    
- 自动生成裁决报告，便于追溯与破例申请
    

**示例**：
```text
[FAIL] ADR-240.1: Handlers must not catch exceptions (TestClass: ADR_240_1_HandlerMustNotCatchExceptionTests)
```

---

### ADR-906.3:L1 支持 L1 / L2 执法等级

**规则**：
- L1 测试失败直接阻断合并 / 部署    
- L2 测试失败仅记录警告，需人工 Code Review 确认    
- 执行等级必须与 ADR-905 定义一致
    

---

### ADR-906.4:L1 破例与偿还机制

**规则**：
- 允许通过 ADR-0000 定义的 Exception Mechanism 破例    
- 破例必须自动记录，包含：    
    - ADR 编号        
    - 测试类 / 方法        
    - 破例理由        
    - 到期处理时间
        

**禁止**：
- 手动绕过 CI 阻断    
- 未记录破例或过期未修复
    

---

### ADR-906.5:L2 CI / Analyzer 必须检测弱断言与空测试

**规则**：
- 自动扫描 ArchitectureTests 是否包含 ≥1 有效断言（参照 ADR-904）    
- 标记空测试、弱断言或多 ADR 混合测试
    

---

### ADR-906.6:L2 支持 ADR 生命周期同步

**规则**：
- ADR 被标记为 Superseded / Deprecated 时，CI / Analyzer 自动：    
    - 标记对应测试为 [Obsolete] 或删除
    - 提示迁移至替代 ADR
        
- 禁止保留“孤儿测试”
    

---

## Enforcement（执法模型）

|规则编号|执行级|执法方式|描述|
|---|---|---|---|
|ADR-906.1|L1|CI / Analyzer 自动注册扫描|确保每个 ArchitectureTest 被发现并执行|
|ADR-906.2|L1|CI / Analyzer 日志扫描|测试失败直接映射 ADR|
|ADR-906.3|L1|CI 阻断策略|L1 失败阻断，L2 警告|
|ADR-906.4|L1|Exception 记录与阻断|自动破例 / 补救记录|
|ADR-906.5|L2|断言数量 / 弱断言检测|自动标记无效测试|
|ADR-906.6|L2|ADR 生命周期同步|自动废弃 / 提醒迁移|

---

## Non-Goals（明确不管什么）

- 架构规则本身的设计或变更    
- 测试框架或断言库选型    
- 性能优化或测试速度
    

---

## Prohibited（禁止行为）

- 手动绕过 CI / Analyzer 执行    
- 弱断言或空测试未被检测    
- 未注册或孤儿 ArchitectureTests    
- 未记录破例 / 补救机制
    

---

## Relationships（关系声明）

**Depends On**：

- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md)
- [ADR-903：ArchitectureTests 命名与组织规范](../governance/ADR-903-architecture-tests-naming-organization.MD)
- [ADR-904：ArchitectureTests 最小断言语义规范](../governance/ADR-904-architecturetests-minimum-assertion-semantics.md)
- [ADR-905：执行级别分类](../governance/ADR-905-enforcement-level-classification.md)

**Depended By**：

- 所有 ArchitectureTests 执法流程
- CI / Analyzer 自动裁决规则

---

## References（非裁决性参考）

- GitHub Actions / Azure DevOps / Jenkins CI Gate 实践
- NetArchTest.Rules 自动化扫描
- xUnit / NUnit Architecture Test Practices

---


---

## Non-Goals（明确不管什么）


本 ADR 明确不涉及以下内容：

- 待补充


## History（版本历史）

|版本|日期|变更说明|修订人|
|---|---|---|---|
|1.0|2026-01-28|初次正式发布|Architecture Board|
