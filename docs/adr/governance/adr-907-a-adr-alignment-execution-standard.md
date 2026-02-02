---
adr: ADR-907-A
title: "ADR-907 对齐执行标准"
status: Final
level: Governance
version: "1.0"
deciders: "Architecture Board"
date: 2026-02-02
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---

# ADR-907-A：ADR-907 对齐执行标准

> ⚖️ **本 ADR 为 ADR-907 v2.0 的官方执行性附录，定义所有 ADR 向 Rule/Clause 双层编号体系对齐的强制规范与失败处理策略。**

---

## Focus（聚焦内容）

本标准确立以下执行规范：

1. **权威性声明**  
   - 明确本标准在 ADR 体系中的法律地位
   - 建立冲突裁决的优先级顺序
   - 定义 CI 失败判定的权威依据

2. **对齐失败策略**  
   - 定义何时允许部分对齐
   - 规定失败标记与记录方式
   - 明确合并准入条件

3. **测试绑定规则**  
   - 将测试更新从建议升级为强制流程卡点
   - 定义 Rule/Clause 变更时的测试同步规则
   - 禁止旧编号测试的兼容存在

**目标**：将对齐指南从"参考文档"升级为"可执行法律系统"，消除执行模糊性。

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| Rule | ADR 中的主要规则分类，对应测试类 | Rule |
| Clause | Rule 下的具体条款，对应测试方法 | Clause |
| RuleId | 唯一规则标识，格式为 `ADR-XXX_<Rule>_<Clause>` | RuleId |
| 对齐 | 将 ADR 文档重构为 Rule/Clause 双层编号体系 | Alignment |
| 部分对齐 | ADR 仅完成部分 Rule/Clause 转换 | Partial Alignment |
| 测试绑定 | Rule/Clause 编号变更必须同步更新架构测试 | Test Coupling |
| 对齐失败 | ADR 无法在一次提交中完成完整对齐 | Alignment Failure |

---

## Authority（权威性）

### 法律地位

> 🔒 **本标准为 ADR-907 的官方执行附录**
>
> 当任何 ADR、指南、文档与本标准在以下方面产生冲突时，**以本标准为准**：
> - Rule/Clause 编号格式
> - Decision/Enforcement 章节结构
> - 对齐完整性要求
> - 测试同步规则

### 优先级顺序

当发生冲突时，裁决优先级为：

```
ADR-907（主体） > ADR-907-A（本标准） > 各具体 ADR > 指南文档 > Prompts
```

**解释**：

- **ADR-907** 定义"什么是 ArchitectureTests 治理体系"
- **ADR-907-A（本标准）** 定义"如何执行对齐"
- **各具体 ADR** 必须符合本标准的格式要求
- **指南文档** 仅作为参考，不具备裁决力
- **Prompts** 用于 AI 辅助，不改变规则本身

### CI 失败判定依据

任何 CI 失败涉及 ADR 对齐问题时，判定依据为：

1. **首先**：检查是否违反 ADR-907 正文
2. **其次**：检查是否违反本标准（ADR-907-A）
3. **最后**：参考相关指南文档

**禁止行为**：

- ❌ 以"我理解不是这样"为由挑战本标准
- ❌ 以"原 ADR 没这么说"为由绕过对齐要求
- ❌ 以"这个 Clause 不需要拆"为由违反编号规范

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-907-A 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-907-A_<Rule>_<Clause>
> ```

---

### ADR-907-A_1：对齐标准与格式规范（Rule）

#### ADR-907-A_1_1 编号格式强制要求

所有 ADR 的 Decision 章节必须使用以下格式：

```markdown
### ADR-XXX_<Rule>：<Rule名称>（Rule）

#### ADR-XXX_<Rule>_<Clause> <Clause标题>
- 规则内容
```

**禁止格式**：
- `ADR-XXX.Y:LZ`（旧格式）
- `ADR-XXX.Y`（无执行级别）
- 任何非 `ADR-XXX_Y_Z` 的变体

#### ADR-907-A_1_2 Decision 章节结构要求

每个 ADR 的 Decision 章节必须包含：

1. **章节声明**：
   ```markdown
   > ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
   ```

2. **统一铁律**：
   ```markdown
   > 🔒 **统一铁律**：
   > 
   > ADR-XXX 中，所有可执法条款必须具备稳定 RuleId，格式为：
   > \`\`\`
   > ADR-XXX_<Rule>_<Clause>
   > \`\`\`
   ```

3. **Rule/Clause 双层结构**：至少包含 1 个 Rule，每个 Rule 至少包含 1 个 Clause

#### ADR-907-A_1_3 Enforcement 章节强制要求

每个 ADR 必须包含 Enforcement 章节，结构如下：

```markdown
## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-XXX 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-XXX_1_1** | L1 | 执法方式描述 | §ADR-XXX_1_1 |
```

**表格必须包含的列**：
- 规则编号（RuleId）
- 执行级（L1/L2/L3）
- 执法方式（如何验证）
- Decision 映射（指向 Decision 章节）

#### ADR-907-A_1_4 Front Matter 版本更新要求

对齐后的 ADR 必须更新 Front Matter：

- `version`: 主版本号 +1（如 "3.0" → "4.0"）
- `date`: 更新为对齐完成日期

#### ADR-907-A_1_5 History 章节记录要求

对齐后的 ADR 必须在 History 章节添加记录：

```markdown
| X.0 | 2026-02-XX | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
```

---

### ADR-907-A_2：对齐失败策略（Rule）

#### ADR-907-A_2_1 部分对齐允许条件

当 ADR 因以下原因无法在一次提交中完成完整对齐时，**允许部分对齐**：

1. **Decision 章节为空或严重不完整**（如 ADR-900, ADR-930）
2. **规则过于复杂，需要多次迭代才能合理分组**
3. **依赖其他 ADR 的对齐结果才能确定 Rule 结构**

**不允许部分对齐的情况**：
- ❌ 单纯因为"工作量大"
- ❌ 想先提交一部分"看看效果"
- ❌ 不确定如何分组所以先放一部分

#### ADR-907-A_2_2 部分对齐最低要求

即使是部分对齐，也必须满足：

1. **至少生成 Rule 1 / Clause 1_1**
2. **Front Matter 必须完整更新**
3. **必须包含 Enforcement 章节**（即使表格不完整）
4. **在 History 中声明 `Alignment Incomplete`**

**示例**：

```markdown
## History（版本历史）

| 版本 | 日期 | 变更说明 | 修订人 |
|------|------|----------|-------|
| 4.0 | 2026-02-03 | 对齐 ADR-907 v2.0（部分），Rule 1-2 已完成。**Alignment Incomplete**：Rule 3-5 待后续补充 | Architecture Board |
```

#### ADR-907-A_2_3 部分对齐追踪机制

部分对齐的 ADR 必须：

1. 在 PR 描述中明确列出**已完成的 Rule** 和**未完成的 Rule**
2. 创建 Follow-up Issue，编号为 `ADR-XXX 对齐完成（Phase 2）`
3. 在该 Issue 中关联原 PR 和目标完成日期

#### ADR-907-A_2_4 部分对齐合并准入

部分对齐的 PR 可以合并，当且仅当：

1. 满足 **ADR-907-A_2_2** 的最低要求
2. 已创建 Follow-up Issue
3. 架构委员会明确批准（在 PR 中留言 `Approved: Partial Alignment`）

#### ADR-907-A_2_5 对齐失败升级路径

如果 ADR 连续 2 次尝试对齐都失败（部分对齐后仍无法完成），必须：

1. 在架构委员会会议中讨论
2. 决定是否需要**重写 ADR** 而非对齐
3. 记录决策并更新 ADR 状态

---

### ADR-907-A_3：测试绑定规则（Rule）

#### ADR-907-A_3_1 测试同步强制要求

任一 ADR 提交若修改了 Rule/Clause 编号，**必须在同一 PR 中更新对应 ArchitectureTests**，否则 CI 失败。

**具体要求**：

- Clause 编号变更 → 测试方法名必须同步变更
- Rule 编号变更 → 测试类名必须同步变更
- 新增 Clause → 必须新增对应测试方法（可以是 TODO stub）
- 删除 Clause → 必须删除或标记废弃对应测试方法

#### ADR-907-A_3_2 测试命名一致性要求

测试命名必须与 RuleId 严格一致：

- **测试类名格式**：`ADR_<编号>_<Rule>_Architecture_Tests`
  - 示例：`ADR_907_2_Architecture_Tests.cs`

- **测试方法名格式**：`ADR_<编号>_<Rule>_<Clause>_<行为描述>`
  - 示例：`ADR_907_2_1_ArchitectureTests_Project_Must_Exist`

**禁止兼容性命名**：
- ❌ 保留旧编号方法名的同时添加新方法
- ❌ 使用注释标记"对应新编号 ADR-XXX_Y_Z"
- ❌ 通过方法参数区分不同 Clause

#### ADR-907-A_3_3 测试失败信息更新要求

测试的失败信息必须引用新的 RuleId：

```csharp
// ✅ 正确
.WithMessage($"违反 ADR-907_2_1：ArchitectureTests 必须集中于独立测试项目")

// ❌ 错误
.WithMessage($"违反 ADR-907.2:L1：ArchitectureTests 必须集中于独立测试项目")
```

#### ADR-907-A_3_4 旧编号测试处理规则

对齐完成后，**不允许旧编号测试继续存在**：

1. **立即删除**：旧测试类和方法
2. **禁止保留**：即使添加 `[Obsolete]` 或注释
3. **禁止兼容模式**：不得同时保留新旧两套测试

**例外**：仅当 ADR 处于 `Superseded` 状态时，允许保留旧测试但必须标记为 `[Obsolete("对应的 ADR 已废弃")]`。

#### ADR-907-A_3_5 测试覆盖率验证

每个 Clause 必须有对应的测试方法：

- 如果 Clause 暂时无法实现自动化测试，允许创建 TODO stub：
  ```csharp
  [Fact]
  public void ADR_907_4_5_Analyzer_Must_Detect_Weak_Assertions()
  {
      // TODO: 实现 Analyzer 弱断言检测逻辑
      throw new NotImplementedException("ADR-907_4_5 测试待实现");
  }
  ```

- TODO stub 必须在 30 天内实现或升级为 Follow-up Issue

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-907-A 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-907-A_1_1** | L1 | ArchitectureTests 验证编号格式合规性 | §ADR-907-A_1_1 |
| **ADR-907-A_1_2** | L1 | ArchitectureTests 验证 Decision 章节结构 | §ADR-907-A_1_2 |
| **ADR-907-A_1_3** | L1 | ArchitectureTests 验证 Enforcement 章节存在性与完整性 | §ADR-907-A_1_3 |
| **ADR-907-A_1_4** | L1 | CI 检查 Front Matter version/date 是否更新 | §ADR-907-A_1_4 |
| **ADR-907-A_1_5** | L1 | CI 检查 History 章节是否包含对齐记录 | §ADR-907-A_1_5 |
| **ADR-907-A_2_1** | L2 | 人工审查：部分对齐是否符合允许条件 | §ADR-907-A_2_1 |
| **ADR-907-A_2_2** | L1 | ArchitectureTests 验证部分对齐最低要求 | §ADR-907-A_2_2 |
| **ADR-907-A_2_3** | L1 | CI 检查部分对齐是否创建 Follow-up Issue | §ADR-907-A_2_3 |
| **ADR-907-A_2_4** | L2 | 人工审查：架构委员会批准记录 | §ADR-907-A_2_4 |
| **ADR-907-A_2_5** | L3 | 架构委员会会议裁决 | §ADR-907-A_2_5 |
| **ADR-907-A_3_1** | L1 | CI 阻断：ADR 变更 PR 必须包含测试更新 | §ADR-907-A_3_1 |
| **ADR-907-A_3_2** | L1 | ArchitectureTests 验证测试命名与 RuleId 一致性 | §ADR-907-A_3_2 |
| **ADR-907-A_3_3** | L1 | ArchitectureTests 验证测试失败信息引用新 RuleId | §ADR-907-A_3_3 |
| **ADR-907-A_3_4** | L1 | ArchitectureTests 检测旧编号测试残留 | §ADR-907-A_3_4 |
| **ADR-907-A_3_5** | L2 | CI 警告：Clause 无对应测试或存在 TODO stub 超期 | §ADR-907-A_3_5 |

### 执行级别说明

- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构委员会人工裁决

---

## 对齐标准（参考 ADR-907）

> ℹ️ **说明**：本节为执行参考，不具备裁决力。裁决依据请参考 Decision 章节。

### 编号格式转换

**旧格式**：
```markdown
### ADR-XXX.1:L1 <标题>
### ADR-XXX.2:L1 <标题>
```

**新格式**：
```markdown
### ADR-XXX_1：<Rule名称>（Rule）
#### ADR-XXX_1_1 <Clause标题>
#### ADR-XXX_1_2 <Clause标题>

### ADR-XXX_2：<Rule名称>（Rule）
#### ADR-XXX_2_1 <Clause标题>
```

### Decision 章节标准结构

```markdown
## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-XXX 中，所有可执法条款必须具备稳定 RuleId，格式为：
> \`\`\`
> ADR-XXX_<Rule>_<Clause>
> \`\`\`

---

### ADR-XXX_1：<Rule名称>（Rule）

#### ADR-XXX_1_1 <Clause标题>
- 规则内容

#### ADR-XXX_1_2 <Clause标题>
- 规则内容

---

### ADR-XXX_2：<Rule名称>（Rule）
...
```

### Enforcement 章节标准结构

```markdown
## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-XXX 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-XXX_1_1** | L1 | ArchitectureTests 自动化验证 | §ADR-XXX_1_1 |
| **ADR-XXX_1_2** | L1 | ArchitectureTests 自动化验证 | §ADR-XXX_1_2 |
| **ADR-XXX_2_1** | L2 | Roslyn Analyzer + 人工审查 | §ADR-XXX_2_1 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决
```

### Front Matter 更新

```yaml
---
adr: ADR-XXX
date: 2026-02-03  # 更新日期
version: "X.0"    # 主版本号 +1
# ... 其他字段保持不变
---
```

### History 更新

```markdown
## History（版本历史）

| 版本 | 日期 | 变更说明 | 修订人 |
|------|------|----------|-------|
| X.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
| ... | ... | ... | ... |
```

---

## 对齐步骤（每个 ADR）

> ℹ️ **说明**：本节为操作指南，不具备裁决力。

### 完整对齐流程

1. **查看原文件**
   ```bash
   view <ADR文件路径>
   ```

2. **更新 Front Matter**
   - version: 主版本号 +1
   - date: 当前日期

3. **重构 Decision 章节**
   - 识别所有规则
   - 智能分组为 Rule
   - 每个原规则转换为 Clause
   - 使用新的编号格式

4. **更新或创建 Enforcement 章节**
   - 创建 Enforcement 表格
   - 列出所有 RuleId
   - 标明执行级别

5. **更新 History 章节**
   - 添加新版本记录

6. **同步更新 ArchitectureTests**
   - 更新测试类名
   - 更新测试方法名
   - 更新失败信息

7. **提交变更**
   ```
   使用 report_progress 提交
   ```

### 智能分组策略

**原则**：
- 相关的规则分组为一个 Rule
- 每个 Rule 应该有清晰的主题
- 每个 Clause 应该是一个独立的可测试规则

**示例（ADR-0000）**：

**旧规则**：
- ADR-0000.1: 审判权唯一性
- ADR-0000.2: 架构违规的判定原则
- ADR-0000.3: 执行级别分离原则
- ADR-0000.4: ADR ↔ 测试 ↔ CI 的一一映射
- ADR-0000.5: 破例治理宪法规则
- ADR-0000.6: 冲突裁决优先级

**新分组**：
- **Rule 1：架构裁决权威性**
  - Clause 1_1: 审判权唯一性
  - Clause 1_2: 架构违规的判定原则
- **Rule 2：执行级别与测试映射**
  - Clause 2_1: 执行级别分离原则
  - Clause 2_2: ADR ↔ 测试 ↔ CI 的一一映射
- **Rule 3：破例治理机制**
  - Clause 3_1: 破例强制要求
  - Clause 3_2: CI 自动监控机制
- **Rule 4：冲突裁决优先级**
  - Clause 4_1: 裁决优先级顺序

---

## 待对齐的 ADR 清单

> ℹ️ **说明**：本节为进度追踪，不具备裁决力。

### Phase 1：治理层 ADR（高优先级）

| ADR | 状态 | 规则数量 | 预估难度 | 备注 |
|-----|------|---------|---------|------|
| ADR-0000 | ✅ 已完成 | 4 Rule, 7 Clause | ⭐⭐⭐ | - |
| ADR-900 | ⚠️ 待评估 | ? | ⭐⭐⭐ | Decision 章节为空，需补充 |
| ADR-901 | 🚧 进行中 | 2 Rule, 8 Clause | ⭐⭐ | Front Matter 已更新 |
| ADR-902 | ⏸️ 待对齐 | 2 Rule, 7 Clause | ⭐⭐ | - |
| ADR-905 | ⏸️ 待对齐 | 1 Rule, 5 Clause | ⭐⭐ | - |
| ADR-910 | ⏸️ 待对齐 | 2 Rule, 5 Clause | ⭐ | - |
| ADR-920 | ⏸️ 待对齐 | 2 Rule, 4 Clause | ⭐ | - |
| ADR-930 | ⚠️ 待评估 | ? | ⭐⭐ | Decision 章节不完整 |
| ADR-940 | ⏸️ 待对齐 | 1 Rule, 5 Clause | ⭐⭐ | - |

### Phase 2：宪法层 ADR（高优先级）

| ADR | 状态 | 预估难度 |
|-----|------|---------|
| ADR-0001 | ⏸️ 待对齐 | ⭐⭐⭐ |
| ADR-0002 | ⏸️ 待对齐 | ⭐⭐⭐ |
| ADR-0003 | ⏸️ 待对齐 | ⭐⭐⭐ |
| ADR-0004 | ⏸️ 待对齐 | ⭐⭐⭐ |
| ADR-0005 | ⏸️ 待对齐 | ⭐⭐⭐ |
| ADR-0006 | ⏸️ 待对齐 | ⭐⭐ |
| ADR-0007 | ⏸️ 待对齐 | ⭐⭐ |
| ADR-0008 | ⏸️ 待对齐 | ⭐⭐ |

### Phase 3：运行层、结构层、技术层 ADR（中优先级）

| ADR 范围 | 数量 | 状态 |
|---------|------|------|
| ADR-120 ~ ADR-124 | 5 个 | ⏸️ 待对齐 |
| ADR-201 ~ ADR-240 | 4 个 | ⏸️ 待对齐 |
| ADR-301 ~ ADR-360 | 4 个 | ⏸️ 待对齐 |

---

## 验证清单

> ℹ️ **说明**：本节为质量检查清单，不具备裁决力。

每个 ADR 对齐后需要检查：

- [ ] Front Matter 的 version 和 date 已更新
- [ ] Decision 章节使用 Rule/Clause 双层结构
- [ ] 所有规则编号使用 `ADR-XXX_<Rule>_<Clause>` 格式
- [ ] Enforcement 章节包含完整的规则编号映射表
- [ ] History 章节记录了此次对齐
- [ ] 没有遗漏任何原有的规则
- [ ] 文档结构完整，没有语法错误
- [ ] **对应的 ArchitectureTests 已同步更新**
- [ ] **旧编号测试已删除**
- [ ] **CI 通过**

---

## Non-Goals（明确不管什么）

- ADR 内容的业务正确性（只管格式对齐）
- 对齐过程中的性能优化
- 对齐工具的开发（可选）
- 历史 ADR 的追溯修改（只对齐 Final 状态的 ADR）

---

## Prohibited（禁止行为）

- 对齐时改变 ADR 的业务语义
- 绕过 Authority 章节定义的优先级
- 部分对齐后不创建 Follow-up Issue
- 保留旧编号测试与新编号测试共存
- 以"工作量大"为由拒绝测试同步
- 修改本标准以降低对齐难度

---

## Relationships（关系声明）

**Depends On**：

- [ADR-907：ArchitectureTests 执法治理体系](./ADR-907-architecture-tests-enforcement-governance.md)  
- [ADR-0000：架构测试与 CI 治理元规则](./ADR-0000-architecture-tests.md)

**Depended By**：

- 所有需要对齐的 ADR
- 所有 ArchitectureTests 更新流程

**Related**：

- [ADR-902：ADR 模板结构契约](./ADR-902-adr-template-structure-contract.md)
- [ADR-900：ADR 新增与修订流程](./ADR-900-adr-process.md)

---

## References（非裁决性参考）

- ADR-907 v2.0（2026-02-02）
- 原对齐指南文档（docs/ADR-907-ALIGNMENT-GUIDE.md）
- GitHub Issue #241 评审反馈

---

## History（版本历史）

| 版本 | 日期 | 变更说明 | 修订人 |
|------|------|----------|-------|
| 1.0 | 2026-02-02 | 初始版本：将对齐指南升级为正式 ADR，新增 Authority、Alignment Failure Policy、Test Coupling Rules 三个核心章节 | Architecture Board |
