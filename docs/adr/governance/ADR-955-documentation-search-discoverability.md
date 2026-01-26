# ADR-955：文档搜索与可发现性优化

> ⚖️ **本 ADR 是文档搜索与可发现性的优化标准，定义索引、标签和智能跳转机制。**

**状态**：✅ Accepted  
**版本**：1.0
**级别**：体验优化 / 治理层  
**适用范围**：所有文档搜索和导航机制  
**生效时间**：即刻

---

## 聚焦内容（Focus）

- 关键词索引生成
- 标签系统标准化
- 智能跳转机制
- 搜索优化建议
- 可发现性指标

---

## 术语表（Glossary）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 关键词索引 | 自动生成的关键词到文档的映射 | Keyword Index |
| 标签系统 | 为文档添加分类标签 | Tagging System |
| 智能跳转 | 基于关系自动生成相关链接 | Smart Navigation |
| 可发现性 | 用户能否快速找到所需文档 | Discoverability |
| 搜索时延 | 从搜索到找到目标文档的时间 | Search Latency |

---

## 决策（Decision）

### 关键词索引生成（ADR-955.1）

**规则**：

**必须**自动生成关键词索引，映射关键词到相关文档。

**索引文件位置**：
```
docs/INDEX.md
```

**索引结构**：
```markdown
# 文档关键词索引

**最后更新**：YYYY-MM-DD（自动生成）

---

## A

**ADR（Architecture Decision Record）**
- [ADR-0008：文档编写与维护宪法](adr/constitutional/ADR-0008-documentation-governance-constitution.md)
- [ADR-900：ADR 新增与修订流程](adr/governance/ADR-900-adr-process.md)
- [ADR-940：ADR 关系与溯源管理](adr/governance/ADR-940-adr-relationship-traceability-management.md)

**API**
- [ADR-0005：应用内交互模型](adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [快速上手指南](QUICK-START.md#api-endpoints)

---

## C

**CQRS**
- [ADR-0005：应用内交互模型](adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [Copilot Prompts: ADR-0005](copilot/adr-0005.prompts.md)

**Command Handler**
- [ADR-0005：应用内交互模型](adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [Handler 模式指南](guides/handler-pattern-guide.md)

---

## M

**模块隔离（Module Isolation）**
- [ADR-0001：模块化单体架构](adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [Copilot Prompts: ADR-0001](copilot/adr-0001.prompts.md)
- [模块通信案例](cases/module-communication/)

---

## 符号

**#ADR-0001**
- 直接跳转：[ADR-0001](adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
```

**生成规则**：
1. **自动提取**：从所有 `.md` 文件中提取标题、术语表
2. **分类**：按首字母分组（A-Z + 符号）
3. **排序**：每组内按关键词字母顺序
4. **链接**：相对路径链接到源文档

**生成工具**：
```bash
# scripts/generate-keyword-index.sh
# 自动扫描 docs/ 目录，生成 INDEX.md
```

**更新频率**：
- **自动触发**：每次 PR 合并后
- **CI 集成**：作为文档构建的一部分

**核心原则**：
> 自动生成，实时更新，易于查找。

**判定**：
- ❌ 无关键词索引
- ❌ 手动维护索引（易过时）
- ✅ 自动生成并保持最新

---

### 标签系统（ADR-955.2）

**规则**：

ADR 和主要文档 **必须**包含标准化标签。

**标签位置**：
在文档元数据区添加标签：

```markdown
# ADR-0001：模块化单体与垂直切片架构

**状态**：✅ Accepted
**版本**：1.0
**标签**：`architecture`, `module`, `isolation`, `vertical-slice`, `cqrs`
```

**标准标签类别**：

| 类别 | 标签示例 | 用途 |
|------|---------|------|
| **层级** | `constitutional`, `structure`, `runtime`, `technical`, `governance` | ADR 分类 |
| **主题** | `architecture`, `testing`, `documentation`, `security`, `performance` | 主题分类 |
| **概念** | `module`, `handler`, `event`, `contract`, `repository` | 技术概念 |
| **关注点** | `isolation`, `dependency`, `lifecycle`, `versioning`, `logging` | 关注点 |
| **角色** | `backend-dev`, `frontend-dev`, `qa`, `devops`, `architect` | 目标受众 |

**标签命名规则**：
- 全小写
- 单词间用连字符 `-`
- 简洁明确（≤3 个单词）
- 使用既有标签优先于创建新标签

**标签索引**：
```
docs/TAGS.md
```

结构：
```markdown
# 文档标签索引

## 按标签浏览

### architecture
- [ADR-0001：模块化单体架构](adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0002：平台、应用与主机](adr/constitutional/ADR-0002-platform-application-host-bootstrap.md)

### testing
- [ADR-0000：架构测试与 CI 治理](adr/governance/ADR-0000-architecture-tests.md)
- [ADR-122：测试组织与命名](adr/structure/ADR-122-test-organization-naming.md)

### module
- [ADR-0001：模块化单体架构](adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [模块通信案例](cases/module-communication/)

---

## 按受众浏览

### backend-dev
- [ADR-0001](...)
- [ADR-0005](...)
- [Handler 模式指南](...)

### qa
- [ADR-0000](...)
- [ADR-122](...)
- [测试编写指南](...)
```

**核心原则**：
> 标准化标签，多维度浏览。

**判定**：
- ❌ 文档无标签
- ❌ 标签混乱不统一
- ✅ 标准化标签，易于分类

---

### 智能跳转机制（ADR-955.3）

**规则**：

**必须**基于关系声明自动生成相关文档链接。

**实现方式**：

1. **ADR 底部自动生成"相关文档"章节**：

```markdown
---

## 相关文档（自动生成）

### 依赖关系
本 ADR 依赖以下文档：
- ← [ADR-0000：架构测试与 CI 治理](../governance/ADR-0000-architecture-tests.md)

以下文档依赖本 ADR：
- → [ADR-0005：应用内交互模型](../constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- → [ADR-122：测试组织与命名](../structure/ADR-122-test-organization-naming.md)

### 主题相关
基于标签 `architecture`, `module`, `isolation`，推荐：
- [ADR-0002：平台、应用与主机](../constitutional/ADR-0002-platform-application-host-bootstrap.md) - `architecture`, `module`
- [ADR-0003：命名空间规则](../constitutional/ADR-0003-namespace-rules.md) - `module`, `isolation`

### 辅助资料
- [Copilot Prompts: ADR-0001](../../copilot/adr-0001.prompts.md) - 场景化指导
- [模块通信案例](../../cases/module-communication/) - 实践示例
```

2. **生成工具**：
```bash
# scripts/generate-smart-links.sh
# 基于关系声明和标签，自动生成相关链接
```

3. **悬浮提示**（在支持的编辑器中）：
- 鼠标悬停在 ADR 引用上时显示摘要
- 使用 Markdown 提示语法

**核心原则**：
> 自动关联，减少手动查找。

**判定**：
- ❌ 阅读 ADR 时不知道查看哪些相关文档
- ❌ 手动维护相关链接
- ✅ 自动生成相关文档推荐

---

### 搜索优化建议（ADR-955.4）

**规则**：

**应该**提供搜索最佳实践建议。

**搜索技巧文档**：
```
docs/HOW-TO-SEARCH.md
```

内容：
```markdown
# 如何快速找到文档

## 搜索策略

### 1. 使用关键词索引
最快方式：查阅 [关键词索引](INDEX.md)

示例：
- 找"模块隔离"相关内容 → 查 INDEX.md 的 "M" 部分
- 找"CQRS"相关内容 → 查 INDEX.md 的 "C" 部分

### 2. 使用标签浏览
按主题浏览：查阅 [标签索引](TAGS.md)

示例：
- 找所有架构相关 ADR → 查 TAGS.md 的 `architecture` 标签
- 找所有测试相关文档 → 查 TAGS.md 的 `testing` 标签

### 3. 使用 GitHub 搜索
高级搜索技巧：
- 精确短语：`"module isolation"`
- 限定文件类型：`adr extension:md`
- 限定路径：`path:docs/adr/ handler`
- 组合：`path:docs/adr/ "CQRS" OR "Command"`

### 4. 使用 ADR 关系图
视觉化导航：查阅 [ADR 关系图](adr/ADR-RELATIONSHIP-MAP.md)

### 5. 使用 grep 命令
本地搜索：
```bash
# 搜索包含"Handler"的所有 ADR
grep -r "Handler" docs/adr/

# 搜索 ADR 编号引用
grep -r "ADR-0001" docs/
```

## 常见搜索场景

| 我想找... | 建议方式 |
|----------|---------|
| 特定 ADR 编号 | 直接访问 `docs/adr/.../ADR-XXXX-*.md` |
| 某个概念的定义 | 查 INDEX.md 关键词索引 |
| 某个主题的所有文档 | 查 TAGS.md 标签索引 |
| 与某个 ADR 相关的文档 | 查看该 ADR 的"相关文档"章节 |
| 如何实现某个模式 | 查 `docs/guides/` 或 `docs/cases/` |
| 架构测试失败如何修复 | 查对应 ADR 的 Copilot Prompts |
| 某个工具如何使用 | 查 `docs/` 根目录的指南文档 |

## 搜索时延目标

我们的目标是：从搜索到找到目标文档 **< 2 分钟**

如果超过 2 分钟，请：
1. 创建 Issue，标签 `documentation-findability`
2. 描述搜索场景和遇到的困难
3. 我们将改进索引和导航
```

**核心原则**：
> 教会搜索，降低门槛。

**判定**：
- ❌ 新人不知道如何搜索文档
- ❌ 搜索效率低下
- ✅ 有明确的搜索指南

---

### 可发现性指标（ADR-955.5）

**规则**：

**应该**定义和追踪文档可发现性指标。

**关键指标**：

| 指标 | 定义 | 目标 | 测量方式 |
|------|------|------|---------|
| 搜索时延 | 从搜索到找到目标文档的平均时间 | < 2 分钟 | 用户反馈调查 |
| 索引覆盖率 | 关键词索引覆盖的文档比例 | > 95% | 自动计算 |
| 标签完整性 | 包含标签的 ADR 比例 | 100% | 自动检查 |
| 链接有效性 | 有效链接的比例 | 100% | CI 自动检查 |
| 孤立文档率 | 无入站链接的文档比例 | < 5% | 自动分析 |

**定期报告**：
```
docs/reports/discoverability/YYYY-MM.md
```

内容：
```markdown
# 文档可发现性月度报告

**报告期**：YYYY-MM
**生成时间**：YYYY-MM-DD

## 指标汇总

| 指标 | 当前值 | 目标 | 状态 |
|------|--------|------|------|
| 搜索时延 | 1.5 分钟 | < 2 分钟 | ✅ 达标 |
| 索引覆盖率 | 98% | > 95% | ✅ 达标 |
| 标签完整性 | 95% | 100% | ⚠️ 接近 |
| 链接有效性 | 100% | 100% | ✅ 达标 |
| 孤立文档率 | 3% | < 5% | ✅ 达标 |

## 改进建议

1. 为缺少标签的 5% ADR 添加标签
2. 优化 INDEX.md 的关键词提取算法

## 用户反馈

- 正面：关键词索引很有用
- 改进：希望增加按难度级别筛选
```

**核心原则**：
> 量化评估，持续改进。

**判定**：
- ❌ 不知道文档可发现性如何
- ❌ 无数据支持改进决策
- ✅ 定期测量和报告

---

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 基于其文档组织
- [ADR-940：ADR 关系与溯源管理宪法](../governance/ADR-940-adr-relationship-traceability-management.md) - 基于其关系声明

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-975：文档质量指标与监控](../governance/ADR-975-documentation-quality-monitoring.md) - 质量监控包含可发现性

---

## 执法模型（Enforcement）

| 规则编号 | 执行级别 | 测试/手段 | 说明 |
|---------|---------|----------|------|
| ADR-955.1 | L1 | `generate-keyword-index.sh` | 自动生成索引 |
| ADR-955.2 | L2 | Code Review | 人工审查标签 |
| ADR-955.3 | L1 | `generate-smart-links.sh` | 自动生成链接 |
| ADR-955.4 | L2 | 文档存在性检查 | 确保搜索指南存在 |
| ADR-955.5 | L2 | 月度报告 | 定期生成指标报告 |

---

## 破例与归还（Exception）

### 允许破例的前提

破例 **仅在以下情况允许**：
- 技术限制导致无法自动生成索引（需手动）
- 标签系统迁移期（6 个月宽限）
- 小型项目（<50 文档）可简化

### 破例要求

每个破例 **必须**：
- 记录原因和预期解决时间
- 提供替代方案
- 架构委员会批准

---

## 变更政策（Change Policy）

### 变更规则

本 ADR 属于 **治理层体验优化规则**：
- 修改需架构委员会同意
- 需更新相关脚本和文档
- 需向团队通报变更

### 失效与替代

- 本 ADR 一旦被替代，**必须**更新所有索引和导航工具
- 不允许"隐性废弃"

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：
- 文档内容质量（由 ADR-0008 负责）
- 搜索引擎的技术实现
- 全文搜索功能（依赖 GitHub 搜索）
- 文档的可读性和写作风格
- AI 辅助搜索（可探索）

---

## 非裁决性参考（References）

### 相关 ADR
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md)
- [ADR-940：ADR 关系与溯源管理宪法](../governance/ADR-940-adr-relationship-traceability-management.md)

### 实施工具
- `scripts/generate-keyword-index.sh` - 关键词索引生成
- `scripts/generate-smart-links.sh` - 智能链接生成
- `docs/INDEX.md` - 关键词索引
- `docs/TAGS.md` - 标签索引
- `docs/HOW-TO-SEARCH.md` - 搜索指南

### 背景材料
- [ADR-Documentation-Governance-Gap-Analysis.md](../proposals/ADR-Documentation-Governance-Gap-Analysis.md) - 原始提案

---

## 版本历史（Version History）

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|----------|------|
| 1.0 | 2026-01-26 | 初版：定义文档搜索与可发现性优化标准 | GitHub Copilot |

---

**维护**：架构委员会 & 文档团队  
**审核**：待定  
**状态**：✅ Accepted
**版本**：1.0
