---
title: "PR #247 GitHub Issues 创建模板"
date: 2026-02-03
purpose: "提供可直接复制粘贴的 GitHub Issues 创建内容"
related_doc: "pr247-alignment-followup-issues.md"
---

# PR #247 GitHub Issues 创建模板

> 📋 **使用说明**：本文档提供可直接复制粘贴到 GitHub Issues 的内容模板。

---

## 主议题 1：完成治理层 ADR 对齐（ADR-920, 930, 940）

### 创建议题信息

- **标题**：`完成治理层 ADR 对齐（ADR-920, 930, 940）`
- **标签**：`documentation`, `adr`, `governance`, `alignment`
- **里程碑**：ADR 对齐工作
- **受让人**：@douhuaa 或架构委员会成员

### 议题正文

```markdown
## 目标

完成 PR #247 未完成的治理层 ADR 对齐工作，包括 ADR-920、ADR-930、ADR-940。

## 背景

PR #247 已完成 ADR-900 和 ADR-910 的对齐工作，引入了 Rule/Clause 双层编号体系（`ADR-XXX_Y_Z`）。
根据 ADR-907-A 对齐执行标准，剩余治理层 ADR 需要完成对齐。

## 范围

- ADR-920：示例代码治理规范（v1.1 → v2.0）
- ADR-930：代码审查合规性（需评估 Decision 章节）
- ADR-940：ADR 关系追溯管理（v1.x → v2.0）

## 验收标准

每个 ADR 对齐后需满足：

- [ ] Front Matter 的 version 和 date 已更新
- [ ] Decision 章节使用 Rule/Clause 双层结构
- [ ] 所有规则编号使用 `ADR-XXX_<Rule>_<Clause>` 格式
- [ ] Enforcement 章节包含完整的规则编号映射表
- [ ] History 章节记录了此次对齐

## 子任务

- [ ] #[子议题1] 对齐 ADR-920（示例代码治理规范）
- [ ] #[子议题2] 评估并对齐 ADR-930（代码审查合规性）
- [ ] #[子议题3] 对齐 ADR-940（ADR 关系追溯管理）

## 参考

- [ADR-907-A：ADR 对齐执行标准](../docs/adr/governance/adr-907-a-adr-alignment-execution-standard.md)
- [PR #247](https://github.com/douhuaa/Zss.BilliardHall/pull/247)
- [详细规划文档](../docs/reports/pr247-alignment-followup-issues.md)

## 预估工时

5-8 小时（3 个 ADR）
```

---

## 子议题 1.1：对齐 ADR-920（示例代码治理规范）

### 创建议题信息

- **标题**：`对齐 ADR-920（示例代码治理规范）到 Rule/Clause 编号体系`
- **标签**：`documentation`, `adr`, `governance`, `alignment`
- **关联主议题**：在正文中引用主议题编号
- **受让人**：架构委员会成员

### 议题正文

```markdown
## 任务

将 ADR-920 从 v1.1 对齐到 v2.0，引入 Rule/Clause 双层编号体系。

**关联主议题**：#[主议题编号]

## 当前状态

- 版本：v1.1
- 预估 Rules：2
- 预估 Clauses：4
- 难度：⭐

## 对齐步骤

1. [ ] 查看当前 ADR-920 文档
2. [ ] 识别现有规则并智能分组为 Rules
3. [ ] 更新 Decision 章节为 Rule/Clause 双层结构
4. [ ] 创建 Enforcement 映射表
5. [ ] 更新 Front Matter（version: "2.0", date: 2026-02-03）
6. [ ] 更新 History 章节
7. [ ] 验证文档完整性

## 验收标准

- [ ] 使用 `ADR-920_<Rule>_<Clause>` 编号格式
- [ ] Enforcement 表格包含所有 RuleId
- [ ] History 记录对齐信息
- [ ] 无遗漏原有规则

## 依赖

- ADR-907-A 对齐执行标准
- PR #247 作为参考示例

## 预估工时

1-2 小时

## 参考

- [ADR-920 当前版本](../docs/adr/governance/ADR-920-examples-governance-constitution.md)
- [详细规划文档](../docs/reports/pr247-alignment-followup-issues.md#子议题-11对齐-adr-920示例代码治理规范)
```

---

## 子议题 1.2：评估并对齐 ADR-930（代码审查合规性）

### 创建议题信息

- **标题**：`评估并对齐 ADR-930（代码审查合规性）到 Rule/Clause 编号体系`
- **标签**：`documentation`, `adr`, `governance`, `alignment`, `needs-evaluation`
- **关联主议题**：在正文中引用主议题编号
- **受让人**：架构委员会成员

### 议题正文

```markdown
## 任务

评估 ADR-930 的 Decision 章节完整性，如有必要补充后对齐到 v2.0。

**关联主议题**：#[主议题编号]

## 当前状态

- 版本：v1.x
- 预估 Rules：?（需评估）
- 预估 Clauses：?（需评估）
- 难度：⭐⭐
- 特殊说明：Decision 章节不完整

## 对齐步骤

1. [ ] 评估 ADR-930 的 Decision 章节完整性
2. [ ] 如有必要，补充缺失的规则
3. [ ] 识别现有规则并智能分组为 Rules
4. [ ] 更新 Decision 章节为 Rule/Clause 双层结构
5. [ ] 创建 Enforcement 映射表
6. [ ] 更新 Front Matter
7. [ ] 更新 History 章节
8. [ ] 验证文档完整性

## 验收标准

- [ ] Decision 章节完整
- [ ] 使用 `ADR-930_<Rule>_<Clause>` 编号格式
- [ ] Enforcement 表格包含所有 RuleId
- [ ] History 记录对齐信息

## 依赖

- ADR-907-A 对齐执行标准
- 可能需要架构委员会讨论补充规则

## 预估工时

2-3 小时（包括评估时间）

## 参考

- [ADR-930 当前版本](../docs/adr/governance/ADR-930-code-review-compliance.md)
- [详细规划文档](../docs/reports/pr247-alignment-followup-issues.md#子议题-12评估并对齐-adr-930代码审查合规性)
```

---

## 子议题 1.3：对齐 ADR-940（ADR 关系追溯管理）

### 创建议题信息

- **标题**：`对齐 ADR-940（ADR 关系追溯管理）到 Rule/Clause 编号体系`
- **标签**：`documentation`, `adr`, `governance`, `alignment`
- **关联主议题**：在正文中引用主议题编号
- **受让人**：架构委员会成员

### 议题正文

```markdown
## 任务

将 ADR-940 从 v1.x 对齐到 v2.0，引入 Rule/Clause 双层编号体系。

**关联主议题**：#[主议题编号]

## 当前状态

- 版本：v1.x
- 预估 Rules：1
- 预估 Clauses：5
- 难度：⭐⭐

## 对齐步骤

1. [ ] 查看当前 ADR-940 文档
2. [ ] 识别现有规则并智能分组为 Rules
3. [ ] 更新 Decision 章节为 Rule/Clause 双层结构
4. [ ] 创建 Enforcement 映射表
5. [ ] 更新 Front Matter（version: "2.0"）
6. [ ] 更新 History 章节
7. [ ] 验证文档完整性

## 验收标准

- [ ] 使用 `ADR-940_<Rule>_<Clause>` 编号格式
- [ ] Enforcement 表格包含所有 RuleId
- [ ] History 记录对齐信息
- [ ] 无遗漏原有规则

## 依赖

- ADR-907-A 对齐执行标准
- PR #247 作为参考示例

## 预估工时

1-2 小时

## 参考

- [ADR-940 当前版本](../docs/adr/governance/ADR-940-adr-relationship-traceability-management.md)
- [详细规划文档](../docs/reports/pr247-alignment-followup-issues.md#子议题-13对齐-adr-940adr-关系追溯管理)
```

---

## 主议题 2：同步架构测试到 Rule/Clause 编号体系（ADR-900, 910）

### 创建议题信息

- **标题**：`同步架构测试到 Rule/Clause 编号体系（ADR-900, 910）`
- **标签**：`testing`, `architecture-tests`, `adr`, `alignment`, `critical`
- **优先级**：🔥 高（阻塞性工作）
- **里程碑**：ADR 对齐工作
- **受让人**：@douhuaa 或架构委员会成员

### 议题正文

```markdown
## 目标

根据 ADR-907-A_3_1（L1 强制规则），同步更新 ADR-900 和 ADR-910 的架构测试，
使测试命名与新的 Rule/Clause 编号体系一致。

## ⚠️ 重要性

这是 **阻塞性工作**，根据 ADR-907-A_3_1：

> 任一 ADR 提交若修改了 Rule/Clause 编号，**必须在同一 PR 中更新对应 ArchitectureTests**，否则 CI 失败。

## 背景

PR #247 已完成 ADR-900 和 ADR-910 文档的对齐，但**未同步更新架构测试**。
这违反了 ADR-907-A 的测试绑定规则，需要立即修复。

## 范围

1. ADR-900 相关测试（4 Rules, 7 Clauses）
2. ADR-910 相关测试（2 Rules, 5 Clauses）

## 测试同步要求

- ✅ Clause 编号变更 → 测试方法名必须同步变更
- ✅ Rule 编号变更 → 测试类名必须同步变更
- ✅ 测试失败信息必须引用新 RuleId
- ❌ 禁止保留旧编号测试（ADR-907-A_3_4）

## 验收标准

- [ ] 所有测试类名和方法名与新 RuleId 一致
- [ ] 测试失败信息引用新 RuleId
- [ ] 旧编号测试已删除
- [ ] 所有测试通过
- [ ] CI 通过

## 子任务

- [ ] #[子议题1] 同步 ADR-900 架构测试
- [ ] #[子议题2] 同步 ADR-910 架构测试

## 参考

- [ADR-907-A §3：测试绑定规则](../docs/adr/governance/adr-907-a-adr-alignment-execution-standard.md)
- [详细规划文档](../docs/reports/pr247-alignment-followup-issues.md#主议题-2同步架构测试到-ruleclause-编号体系adr-907-a_3_1-强制要求)

## 预估工时

3-5 小时
```

---

## 子议题 2.1：同步 ADR-900 架构测试

### 创建议题信息

- **标题**：`同步 ADR-900 架构测试到新 Rule/Clause 编号`
- **标签**：`testing`, `architecture-tests`, `adr`, `alignment`
- **关联主议题**：在正文中引用主议题编号
- **受让人**：架构委员会成员

### 议题正文

```markdown
## 任务

更新 ADR-900 相关的架构测试，使其与新的 Rule/Clause 编号体系一致。

**关联主议题**：#[主议题编号]

## 测试映射

### 旧编号 → 新编号映射

#### Rule 1：架构裁决权威性
- `ADR-900.1` → `ADR-900_1_1`（审判权唯一性）
- `ADR-900.2` → `ADR-900_1_2`（架构违规的判定原则）

#### Rule 2：执行级别与测试映射
- `ADR-900.3` → `ADR-900_2_1`（执行级别分离原则）
- `ADR-900.4` → `ADR-900_2_2`（ADR ↔ 测试 ↔ CI 的一一映射）

#### Rule 3：破例治理机制
- `ADR-900.5` → `ADR-900_3_1`（破例强制要求）
- （新增）→ `ADR-900_3_2`（CI 自动监控机制）

#### Rule 4：冲突裁决优先级
- `ADR-900.6` → `ADR-900_4_1`（裁决优先级顺序）

## 测试文件结构

```
src/tests/ArchitectureTests/ADR/
├── ADR_900_1_Architecture_Tests.cs  # Rule 1 测试
├── ADR_900_2_Architecture_Tests.cs  # Rule 2 测试
├── ADR_900_3_Architecture_Tests.cs  # Rule 3 测试
└── ADR_900_4_Architecture_Tests.cs  # Rule 4 测试
```

## 测试方法命名规范

```csharp
// Rule 1 测试示例
public class ADR_900_1_Architecture_Tests
{
    [Fact]
    public void ADR_900_1_1_ADR_正文是唯一裁决依据()
    {
        // 测试实现
        // 失败信息：违反 ADR-900_1_1：ADR 正文是唯一裁决依据
    }

    [Fact]
    public void ADR_900_1_2_架构违规判定原则必须遵守()
    {
        // 测试实现
        // 失败信息：违反 ADR-900_1_2：架构违规的判定原则
    }
}
```

## 执行步骤

1. [ ] 查找现有 ADR-900 测试文件
2. [ ] 创建新的测试类（按 Rule 分组）
3. [ ] 迁移测试方法并重命名
4. [ ] 更新测试失败信息
5. [ ] 删除旧测试类
6. [ ] 运行测试验证
7. [ ] CI 验证

## 验收标准

- [ ] 测试类名格式：`ADR_900_<Rule>_Architecture_Tests`
- [ ] 测试方法名格式：`ADR_900_<Rule>_<Clause>_<行为描述>`
- [ ] 失败信息引用新 RuleId（如 `ADR-900_1_1`）
- [ ] 所有测试通过
- [ ] 旧测试文件已删除

## 预估工时

2-3 小时

## 参考

- [详细规划文档](../docs/reports/pr247-alignment-followup-issues.md#子议题-21同步-adr-900-架构测试)
```

---

## 子议题 2.2：同步 ADR-910 架构测试

### 创建议题信息

- **标题**：`同步 ADR-910 架构测试到新 Rule/Clause 编号`
- **标签**：`testing`, `architecture-tests`, `adr`, `alignment`
- **关联主议题**：在正文中引用主议题编号
- **受让人**：架构委员会成员

### 议题正文

```markdown
## 任务

更新 ADR-910 相关的架构测试，使其与新的 Rule/Clause 编号体系一致。

**关联主议题**：#[主议题编号]

## 测试映射

### 旧编号 → 新编号映射

#### Rule 1：README 的定位与权限边界
- `ADR-910.1` → `ADR-910_1_1`（README 是使用说明不是架构裁决书）
- `ADR-910.2` → `ADR-910_1_2`（禁用裁决性语言规则）
- `ADR-910.3` → `ADR-910_1_3`（必须包含无裁决力声明）

#### Rule 2：README 与 ADR 的关系治理
- `ADR-910.4` → `ADR-910_2_1`（README 引用 ADR 的规范）
- `ADR-910.5` → `ADR-910_2_2`（README 的变更治理规则）

## 测试文件结构

```
src/tests/ArchitectureTests/ADR/
├── ADR_910_1_Architecture_Tests.cs  # Rule 1 测试
└── ADR_910_2_Architecture_Tests.cs  # Rule 2 测试
```

## 测试方法命名规范

```csharp
// Rule 1 测试示例
public class ADR_910_1_Architecture_Tests
{
    [Fact]
    public void ADR_910_1_1_README_必须是使用说明不是架构裁决书()
    {
        // 测试实现
        // 失败信息：违反 ADR-910_1_1：README 是使用说明不是架构裁决书
    }

    [Fact]
    public void ADR_910_1_2_README_禁止使用裁决性语言()
    {
        // 测试实现
        // 失败信息：违反 ADR-910_1_2：禁用裁决性语言规则
    }

    [Fact]
    public void ADR_910_1_3_README_必须包含无裁决力声明()
    {
        // 测试实现
        // 失败信息：违反 ADR-910_1_3：必须包含无裁决力声明
    }
}
```

## 执行步骤

1. [ ] 查找现有 ADR-910 测试文件
2. [ ] 创建新的测试类（按 Rule 分组）
3. [ ] 迁移测试方法并重命名
4. [ ] 更新测试失败信息
5. [ ] 删除旧测试类
6. [ ] 运行测试验证
7. [ ] CI 验证

## 验收标准

- [ ] 测试类名格式：`ADR_910_<Rule>_Architecture_Tests`
- [ ] 测试方法名格式：`ADR_910_<Rule>_<Clause>_<行为描述>`
- [ ] 失败信息引用新 RuleId（如 `ADR-910_1_1`）
- [ ] 所有测试通过
- [ ] 旧测试文件已删除

## 预估工时

1-2 小时

## 参考

- [详细规划文档](../docs/reports/pr247-alignment-followup-issues.md#子议题-22同步-adr-910-架构测试)
```

---

## 创建顺序建议

1. **先创建主议题 2**（架构测试同步）- 这是阻塞性工作
   - 创建子议题 2.1
   - 创建子议题 2.2

2. **再创建主议题 1**（治理层 ADR 对齐）
   - 创建子议题 1.1
   - 创建子议题 1.2
   - 创建子议题 1.3

3. **在主议题中补充子议题编号**
   - 主议题创建后，将子议题的实际编号填入主议题的"子任务"部分

---

## 标签说明

建议使用以下标签：

- `documentation` - 文档相关工作
- `testing` - 测试相关工作
- `adr` - ADR 相关
- `governance` - 治理层 ADR
- `alignment` - 对齐工作
- `critical` - 关键任务
- `needs-evaluation` - 需要评估

---

**维护者**：Architecture Board  
**使用指南**：复制对应章节的"议题正文"内容，粘贴到 GitHub Issues 创建页面  
**后续工作**：创建 Issues 后，更新 [pr247-alignment-followup-issues.md](./pr247-alignment-followup-issues.md) 中的 Issue 编号
