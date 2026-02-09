---
adr: ADR-955
title: "文档搜索与可发现性优化"
status: Accepted
level: Governance
deciders: "Architecture Board"
date: 2026-02-03
version: "2.0"
maintainer: "架构委员会"
primary_enforcement: L1
reviewer: "@douhuaa"
supersedes: null
superseded_by: null
---

# ADR-955：文档搜索与可发现性优化

> ⚖️ **本 ADR 定义文档的可发现性标准、搜索优化策略及文档索引规范，以提升工程知识库的可用性。**

**状态**：✅ Accepted  

## Focus（聚焦内容）

- 文档索引策略  
- 搜索优化规则  
- 标签与元数据标准  
- 文档可发现性审计机制  

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 可发现性 | 文档在知识库中容易被检索到的能力 | Discoverability |
| 文档索引 | 对文档进行分类、标注、搜索优化的机制 | Indexing |
| 元数据 | 文档附加信息，如标签、分类、作者 | Metadata |
| 搜索优化 | 提高文档被搜索引擎或内部搜索检索到的能力 | Search Optimization |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-955 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-955_<Rule>_<Clause>
> ```

---

### ADR-955_1：文档索引策略（Rule）

#### ADR-955_1_1 元数据要求
- 所有 Guide / FAQ / Case / Standard 必须加入元数据：
  - 分类标签（#模块 #主题 #类型）  
  - 相关 ADR 链接  
  - 作者与日期  

#### ADR-955_1_2 目录结构要求
- 文档目录结构必须清晰，支持快速检索：
```
docs/
├── guides/
├── faqs/
├── cases/
└── engineering-standards/
```

#### ADR-955_1_3 核心原则
- 文档必须可以按 ADR、模块、主题进行搜索  
- 避免重复内容，提高查找效率  

**判定**：
- ❌ 文档缺少元数据或分类标签  
- ❌ 文档目录混乱，无法快速定位  
- ✅ 文档清晰索引，便于检索  

---

### ADR-955_2：搜索优化规则（Rule）

#### ADR-955_2_1 标题关键词要求
- 所有文档标题必须包含核心关键词  

#### ADR-955_2_2 摘要要求
- 文档内容首段应包含摘要，便于搜索引擎抓取  

#### ADR-955_2_3 搜索引擎功能
- 允许内部知识库搜索引擎提供：
  - 全文搜索  
  - 标签过滤  
  - ADR / Module 快速链接  

#### ADR-955_2_4 日志分析
- 定期分析搜索日志，优化常用关键词索引  

**判定**：
- ❌ 标题或摘要缺失关键字  
- ❌ 标签未按照规范使用  
- ✅ 文档可被搜索引擎和内部搜索准确检索  

---

### ADR-955_3：审计与维护机制（Rule）

#### ADR-955_3_1 审计内容
- 每季度审计所有文档的可发现性：
  - 元数据完整性  
  - 索引正确性  
  - 搜索效果可用性  

#### ADR-955_3_2 Issue 跟踪
- 对缺失或错误的文档，创建 Issue 跟踪修复  

#### ADR-955_3_3 报告存档
- 审计报告存档，形成可追踪记录  

**判定**：
- ❌ 审计未完成  
- ❌ 搜索结果不准确  
- ✅ 定期审计并修复问题  

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-955 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-955_1_1** | L1 | 自动检查文档元数据 | §ADR-955_1_1 |
| **ADR-955_1_2** | L1 | 自动检查文档目录结构 | §ADR-955_1_2 |
| **ADR-955_1_3** | L2 | 人工审查重复内容 | §ADR-955_1_3 |
| **ADR-955_2_1** | L1 | 自动检查文档标题关键词 | §ADR-955_2_1 |
| **ADR-955_2_2** | L1 | 自动检查文档摘要 | §ADR-955_2_2 |
| **ADR-955_2_3** | L1 | 自动检查搜索引擎功能 | §ADR-955_2_3 |
| **ADR-955_2_4** | L2 | 定期日志分析脚本 | §ADR-955_2_4 |
| **ADR-955_3_1** | L1 | 定期审计脚本 | §ADR-955_3_1 |
| **ADR-955_3_2** | L2 | 人工创建 Issue | §ADR-955_3_2 |
| **ADR-955_3_3** | L1 | 自动存档审计报告 | §ADR-955_3_3 |

---

## Non-Goals（明确不管什么）

- 文档内容的技术正确性  
- 文档格式样式（字体、颜色等）  
- 搜索引擎实现技术细节  

---

## Prohibited（禁止行为）

- 文档缺失元数据或标签  
- 重复文档未合并或索引  
- 不遵循目录结构，导致搜索不可用  

---

## Relationships（关系声明）

**Depends On**：
- [ADR-950：指南与 FAQ 文档治理规范](ADR-950-guide-faq-documentation-governance.md)  
- [ADR-951：案例库管理规范](ADR-951-case-repository-management.md)  
- [ADR-952：工程标准与 ADR 分离边界](ADR-952-engineering-standard-adr-boundary.md)  

**Depended By**：
- 所有内部知识库文档索引系统  

**Supersedes**：
- 无

**Superseded By**：
- 无

**Related**：
- [ADR-920：示例代码治理规范](ADR-920-examples-governance-constitution.md)  

---

## References（非裁决性参考）

### 模板
- [元数据模板](../../templates/metadata-template.md)  

### 相关文档
- [文档搜索优化指南](../../guides/document-search-optimization.md)  
- [文档审计清单](../../templates/document-audit-checklist.md)  

---

## History（版本历史）

| 版本  | 日期         | 说明   | 修订人 |
|-------|------------|------|------|
| 2.0   | 2026-02-04 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系。创建 ADR_955_1/2/3_Architecture_Tests，实现所有 3 个 Rule（文档索引策略、搜索优化规则、审计与维护机制）和 10 个 Clause 的测试用例。验证文档的可发现性标准和搜索优化策略。 | Copilot Agent |
| 1.0   | 2026-02-03 | 初始版本 | 架构委员会 |
