---
title: "PR #247 ADR 对齐后续工作议题规划"
date: 2026-02-03
status: Planning
maintainer: "Architecture Board"
related_pr: "https://github.com/douhuaa/Zss.BilliardHall/pull/247"
related_adr: 
  - "ADR-907-A"
  - "ADR-900"
  - "ADR-910"
---

# PR #247 ADR 对齐后续工作议题规划

> 📋 **背景**：PR #247 完成了治理层 ADR 的部分对齐工作（5/8），本文档规划剩余工作的议题结构与子议题分解。

---

## 一、已完成工作总结（PR #247）

### 已对齐的 ADR

| ADR | 版本升级 | Rules | Clauses | 状态 |
|-----|---------|-------|---------|------|
| **ADR-900** | v3.1 → v4.0 | 4 | 7 | ✅ 完成 |
| **ADR-910** | v1.1 → v2.0 | 2 | 5 | ✅ 完成 |

### 对齐成果

- ✅ 引入 Rule/Clause 双层编号体系（`ADR-XXX_Y_Z`）
- ✅ 新增 Enforcement 映射表
- ✅ 更新 Front Matter 版本号和日期
- ✅ 添加 History 版本记录

**统计**：11 Rules, 32 Clauses 已对齐，32 条 Enforcement 映射

---

## 二、待完成工作清单

### Phase 1：剩余治理层 ADR 对齐（高优先级）

根据 ADR-907-A 要求，以下治理层 ADR 需要完成对齐：

| ADR | 当前版本 | 预估难度 | 预估 Rules | 预估 Clauses | 特殊说明 |
|-----|---------|---------|-----------|-------------|---------|
| **ADR-901** | v2.0 | ⭐⭐ | 2 | 8 | 已符合规范，需验证 |
| **ADR-902** | v2.0 | ⭐⭐ | 2 | 7 | 已符合规范，需验证 |
| **ADR-905** | v3.0 | ⭐⭐ | 1 | 5 | 已符合规范，需验证 |
| **ADR-920** | v1.1 | ⭐ | 2 | 4 | 需要对齐 |
| **ADR-930** | v1.x | ⭐⭐ | ? | ? | Decision 章节不完整，需评估 |
| **ADR-940** | v1.x | ⭐⭐ | 1 | 5 | 需要对齐 |

### Phase 2：架构测试同步（强制要求）

根据 **ADR-907-A_3_1**（L1 强制规则），Rule/Clause 编号变更必须同步更新架构测试。

#### 需要同步的测试

1. **ADR-900 相关测试**
   - 测试类名：`ADR_900_*_Architecture_Tests.cs`
   - 测试方法名：`ADR_900_<Rule>_<Clause>_<行为描述>`
   - 失败信息：引用新 RuleId

2. **ADR-910 相关测试**
   - 测试类名：`ADR_910_*_Architecture_Tests.cs`
   - 测试方法名：`ADR_910_<Rule>_<Clause>_<行为描述>`
   - 失败信息：引用新 RuleId

#### 测试同步要求（ADR-907-A）

- ✅ Clause 编号变更 → 测试方法名必须同步变更
- ✅ Rule 编号变更 → 测试类名必须同步变更
- ✅ 新增 Clause → 必须新增对应测试方法（可以是 TODO stub）
- ❌ 禁止保留旧编号测试与新编号测试共存

### Phase 3：文档维护（建议）

- 更新 ADR 索引文档
- 更新架构治理系统文档
- 更新相关指南文档

---

## 三、主议题结构

### 主议题 1：完成治理层 ADR 对齐（Phase 1 剩余工作）

**议题标题**：`完成治理层 ADR 对齐（ADR-920, 930, 940）`

**议题描述**：

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

## 参考

- [ADR-907-A：ADR 对齐执行标准](../docs/adr/governance/adr-907-a-adr-alignment-execution-standard.md)
- [PR #247](https://github.com/douhuaa/Zss.BilliardHall/pull/247)
```

**子议题**：

#### 子议题 1.1：对齐 ADR-920（示例代码治理规范）

```markdown
## 任务

将 ADR-920 从 v1.1 对齐到 v2.0，引入 Rule/Clause 双层编号体系。

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
```

#### 子议题 1.2：评估并对齐 ADR-930（代码审查合规性）

```markdown
## 任务

评估 ADR-930 的 Decision 章节完整性，如有必要补充后对齐到 v2.0。

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
```

#### 子议题 1.3：对齐 ADR-940（ADR 关系追溯管理）

```markdown
## 任务

将 ADR-940 从 v1.x 对齐到 v2.0，引入 Rule/Clause 双层编号体系。

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
```

---

### 主议题 2：同步架构测试到新编号体系（ADR-907-A_3_1 强制要求）

**议题标题**：`同步架构测试到 Rule/Clause 编号体系（ADR-900, 910）`

**议题描述**：

```markdown
## 目标

根据 ADR-907-A_3_1（L1 强制规则），同步更新 ADR-900 和 ADR-910 的架构测试，
使测试命名与新的 Rule/Clause 编号体系一致。

## 背景

PR #247 已完成 ADR-900 和 ADR-910 文档的对齐，但**未同步更新架构测试**。
根据 ADR-907-A_3_1：

> 任一 ADR 提交若修改了 Rule/Clause 编号，**必须在同一 PR 中更新对应 ArchitectureTests**，否则 CI 失败。

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

## 参考

- [ADR-907-A §3：测试绑定规则](../docs/adr/governance/adr-907-a-adr-alignment-execution-standard.md#adr-907-a_3测试绑定规则rule)
```

**子议题**：

#### 子议题 2.1：同步 ADR-900 架构测试

```markdown
## 任务

更新 ADR-900 相关的架构测试，使其与新的 Rule/Clause 编号体系一致。

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
```

#### 子议题 2.2：同步 ADR-910 架构测试

```markdown
## 任务

更新 ADR-910 相关的架构测试，使其与新的 Rule/Clause 编号体系一致。

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
```

---

## 四、执行优先级

### 优先级 1（高优先级，阻塞）

根据 **ADR-907-A_3_1（L1 强制规则）**，架构测试同步是阻塞性工作：

- **主议题 2：同步架构测试到新编号体系**
  - 子议题 2.1：同步 ADR-900 架构测试
  - 子议题 2.2：同步 ADR-910 架构测试

**理由**：ADR 编号变更后未同步测试会导致 CI 失败，违反 ADR-907-A。

### 优先级 2（高优先级）

完成治理层 ADR 对齐：

- **主议题 1：完成治理层 ADR 对齐**
  - 子议题 1.1：对齐 ADR-920
  - 子议题 1.2：评估并对齐 ADR-930
  - 子议题 1.3：对齐 ADR-940

**理由**：治理层 ADR 是整个架构治理体系的基础，需要尽快完成对齐。

---

## 五、风险与注意事项

### 风险 1：部分对齐失败

根据 **ADR-907-A_2_4**，部分对齐的 PR 可以合并，但需要：

1. 满足最低要求（至少 Rule 1 / Clause 1_1）
2. 创建 Follow-up Issue
3. 架构委员会明确批准

**应对措施**：如果某个 ADR 对齐困难，按照 ADR-907-A_2 的部分对齐策略处理。

### 风险 2：测试同步复杂度

ADR-900 和 ADR-910 的测试可能分散在多个文件中，需要仔细识别和迁移。

**应对措施**：
1. 使用 `grep` 搜索旧编号引用
2. 创建测试映射表
3. 逐个验证测试功能

### 风险 3：ADR-930 评估结果不确定

ADR-930 的 Decision 章节不完整，可能需要补充规则。

**应对措施**：
1. 先评估现状
2. 如需补充，提交给架构委员会讨论
3. 可能需要额外的 Issue 来补充规则

---

## 六、后续阶段规划

### Phase 2：宪法层 ADR 对齐（8 个）

- ADR-001 ~ ADR-008
- 预估工时：16-24 小时
- 优先级：高

### Phase 3：运行层、结构层、技术层 ADR 对齐（13 个）

- ADR-120 ~ ADR-124（5 个）
- ADR-201 ~ ADR-240（4 个）
- ADR-301 ~ ADR-360（4 个）
- 预估工时：26-40 小时
- 优先级：中

---

## 七、参考文档

### 权威依据

- [ADR-907-A：ADR 对齐执行标准](../docs/adr/governance/adr-907-a-adr-alignment-execution-standard.md)
- [ADR-900：架构测试与 CI 治理元规则](../docs/adr/governance/ADR-900-architecture-tests.md)
- [ADR-910：README 治理宪法](../docs/adr/governance/ADR-910-readme-governance-constitution.md)

### 参考实施

- [PR #247：对齐治理层 ADR 到 Rule/Clause 双层编号体系](https://github.com/douhuaa/Zss.BilliardHall/pull/247)
- [ADR-907 对齐指南](../docs/ADR-907-ALIGNMENT-GUIDE.md)

---

## 八、总结

### 总工时估算

- **Phase 1（治理层 ADR）**：5-8 小时
  - ADR-920: 1-2 小时
  - ADR-930: 2-3 小时（包括评估）
  - ADR-940: 1-2 小时

- **Phase 2（架构测试同步）**：3-5 小时
  - ADR-900 测试: 2-3 小时
  - ADR-910 测试: 1-2 小时

**合计：8-13 小时**

### 关键成功因素

1. ✅ 严格遵循 ADR-907-A 对齐执行标准
2. ✅ 测试与文档同步更新（不可分离）
3. ✅ 保持 RuleId 命名的一致性
4. ✅ 完整记录 History 和 Enforcement 映射
5. ✅ CI 验证通过后再合并

---

**维护者**：Architecture Board  
**审核者**：@douhuaa  
**状态**：✅ Planning Complete  
**下一步**：创建 GitHub Issues
