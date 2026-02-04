---
adr: ADR-951
title: "案例库管理规范"
status: Accepted
level: Governance
deciders: "Tech Lead"
date: 2026-01-26
version: "1.0"
maintainer: "Tech Lead"
primary_enforcement: L1
reviewer: "@douhuaa"
supersedes: null
superseded_by: null
---

# ADR-951：案例库管理规范

> ⚖️ **本 ADR 定义案例库的组织、审核和维护标准。**

**状态**：✅ Accepted

## Focus（聚焦内容）

- 案例库结构组织
- 案例文档标准
- 案例审核标准
- 案例维护机制

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 案例 | 记录实际问题和解决方案的文档 | Case Study |
| 案例库 | 所有案例的集合和组织结构 | Case Repository |
| 最佳实践 | 经过验证的优秀解决方案 | Best Practice |
| 反模式 | 应该避免的错误做法 | Anti-Pattern |

---

## Decision（裁决）

> 🔒 **统一铁律**：  
> 所有案例库规则必须映射到 Clause，并可通过 ArchitectureTests 或文档扫描自动验证。

---

### ADR-951_1：案例库结构组织（Rule）

#### ADR-951_1_1 目录结构规范
- 案例库目录：`docs/cases/`
- 分类目录：模块通信、测试、重构、常见错误
- 每个分类包含 `README.md` 索引
- 案例文件命名：小写 + 连字符
- 判定：
  - ❌ 案例未分类或分类不清
  - ✅ 案例按主题组织并有索引

---

### ADR-951_2：案例文档标准（Rule）

#### ADR-951_2_1 必需章节
1. 元数据：适用场景、相关 ADR、难度、作者、日期、标签
2. 背景：问题描述
3. 解决方案
4. 代码示例：可运行、包含注释
5. 常见陷阱
6. 相关案例

#### ADR-951_2_2 案例级别
| 级别 | 定义 | 审核要求 |
|------|------|----------|
| Core | 核心场景，高频使用，架构关键 | 强审核（2 位 reviewer + 完整清单） |
| Reference | 参考场景，边缘问题 | 弱审核（1 位 reviewer + 基础清单） |

#### ADR-951_2_3 代码示例要求
- 必须可编译/运行
- 包含必要 using/import
- 注释说明关键点
- 正确 (✅) / 错误 (❌) 做法标注

---

### ADR-951_3：案例审核标准（Rule）

#### ADR-951_3_1 Core 案例审核流程
- ADR 合规性审核
- 代码质量审核
- 内容完整性审核
- 实用性审核
- 至少 2 位 reviewer 批准
- 必须通过完整审核清单

#### ADR-951_3_2 Reference 案例审核流程
- ADR 合规性审核（基础）
- 代码可运行性审核
- 至少 1 位 reviewer 批准
- 通过基础审核清单

---

### ADR-951_4：案例维护机制（Rule）

#### ADR-951_4_1 年度审核
- 审核内容：
  - 代码是否符合 ADR
  - 是否有更优方案
  - 是否需要标记过时
  - 引用 ADR 是否有效

#### ADR-951_4_2 过时案例处理
- 标题前添加 `[已过时]`
- 添加过时原因与日期
- 链接推荐新案例
- 不删除，保留历史

#### ADR-951_4_3 更新机制
- 发现问题创建 Issue
- 每季度回顾 Issues
- 更新后递增版本号

---

## Enforcement（执法模型）

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|-----------|--------|---------|---------------|
| ADR-951_1_1 | L1 | 自动扫描案例库目录和分类结构 | §ADR-951_1_1 |
| ADR-951_2_1 | L1 | 检测案例文档包含必需章节 | §ADR-951_2_1 |
| ADR-951_2_2 | L1 | 核查案例级别标记是否符合审核要求 | §ADR-951_2_2 |
| ADR-951_2_3 | L1 | 检查代码示例是否可运行、注释完整 | §ADR-951_2_3 |
| ADR-951_3_1 | L1 | Core 案例必须通过完整审核清单 | §ADR-951_3_1 |
| ADR-951_3_2 | L1 | Reference 案例必须通过基础审核清单 | §ADR-951_3_2 |
| ADR-951_4_1 | L1 | 年度案例审核执行 | §ADR-951_4_1 |
| ADR-951_4_2 | L1 | 过时案例标记和处理 | §ADR-951_4_2 |
| ADR-951_4_3 | L1 | Issue 跟踪和季度回顾更新 | §ADR-951_4_3 |

---

## Non-Goals（明确不管什么）

- 案例内容质量审美
- 文档文本风格与排版
- 单元/集成测试
- 案例数量或覆盖率

---

## Prohibited（禁止行为）

- 案例未分类或缺少索引
- 缺少必需章节
- 代码示例不可运行
- 未标记 Core/Reference 级别
- Core 案例使用弱审核
- 过时案例未标记或未更新

---

## Relationships（关系声明）

**Depends On**：
- [ADR-950：指南与 FAQ 文档治理规范](ADR-950-guide-faq-documentation-governance.md)
- [ADR-920：示例代码治理规范](../governance/ADR-920-examples-governance-constitution.md)

**Depended By**：
- 所有案例引用与管理流程

**Supersedes / Superseded By**：无

**Related**：
- [ADR-920：示例代码治理规范](ADR-920-examples-governance-constitution.md)

---

## References（非裁决性参考）

### 模板
- [Case 模板](../../templates/case-template.md)

### 相关文档
- [案例库索引](../../cases/README.md)
- [案例审核清单](../../templates/case-review-checklist.md)

---

## History（版本历史）

| 版本  | 日期         | 说明   | 修订人 |
|-------|------------|------|------|
| 2.0   | 2026-02-04 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系。创建 ADR_951_1/2/3/4_Architecture_Tests，实现所有 4 个 Rule（案例库结构组织、案例文档标准、案例审核标准、案例维护机制）和 9 个 Clause 的测试用例。验证案例库的组织、审核和维护规范。 | Copilot Agent |
| 1.0   | 2026-01-29 | 初始版本 | Tech Lead |
