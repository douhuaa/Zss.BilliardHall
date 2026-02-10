# GitHub 项目管理创建指南

**Epic**: RuleSet-as-Source-of-Truth  
**创建日期**: 2026-02-10  
**负责人**: douhuaa（需要手动创建）

---

## 📋 需要创建的 GitHub 资源

根据架构委员会决策，需要在 GitHub 创建以下资源来管理 RuleSet-as-Source-of-Truth Epic：

### 1. Epic Issue
### 2. 3 个 Milestones
### 3. GitHub Project 看板
### 4. 子任务 Issues

---

## 🎯 1. 创建 Epic Issue

### Issue 信息

**标题**: `Epic: RuleSet-as-Source-of-Truth - 将 RuleSet 确立为架构治理唯一真相源`

**标签**:
- `epic`
- `epic:ruleset-sot`
- `architecture`
- `governance`
- `priority-high`

**Assignees**:
- douhuaa
- Copilot

**Milestone**: (创建后关联到 Milestone 1)

**描述**:
```markdown
## 🎯 Epic 概述

将 **RuleSet（强类型规约系统）** 确立为架构治理的**唯一真相源**（Single Source of Truth），实现 ADR 文档、测试、Agent 指令的自动生成与同步，消除手工维护的不一致性。

## 📊 当前问题

- ⚠️ 手工同步成本高：RuleSet ↔ ADR 需手工维护，易不一致
- ⚠️ Agent 未用 API：依赖文本理解 Markdown，准确性低
- ⚠️ 测试覆盖不足：仅 5/43 RuleSet 有完整业务规则测试

## 💡 解决方案

**不"替换"而"重新定位"**：

```
            ┌─────────────────┐
            │   RuleSet       │
            │ (唯一真相源)    │  ◄─── 所有规则定义在此
            └────────┬────────┘
                     │
 ┌───────────────────┼───────────────────┬──────────┐
 ▼                   ▼                   ▼          ▼
┌─────────┐    ┌──────────┐        ┌─────────┐  ┌──────┐
│   ADR   │    │   测试    │        │  Agent  │  │  CI  │
│  文档   │    │  生成     │        │  指令   │  │ 验证 │
└─────────┘    └──────────┘        └─────────┘  └──────┘
(自动派生)      (自动生成)          (基于 API)    (自动)
```

## 🗺️ 实施路线图

### 总工期
18-24 工作日（3 个 Milestones）

### Milestone 1: 工具构建与 API 集成（7-9天）
- 阶段 0: 治理与准备 ✅
- 阶段 1: 构建 3 个代码生成器
- 阶段 2: Agent/Skills 集成 RuleSet API

### Milestone 2: 测试与文档生成（5-7天）
- 阶段 3: 批量生成测试套件（500+ 测试）
- 阶段 4: 重新生成 ADR Decision 章节

### Milestone 3: CI 集成与验证（5-7天）
- 阶段 5: CI 自动验证一致性
- 阶段 6: 端到端验证与优化

## 💰 投资回报

- **投入**: 1 人月（18-24 工作日）
- **回报**: 年化节省 312 小时（1.8 人月/年）
- **ROI**: 350%
- **回本周期**: 3-4 个月

## 📚 相关文档

- [Epic 详细文档](../../../docs/governance/EPIC-RuleSet-as-Source-of-Truth.md)
- [里程碑规划](../../../docs/governance/MILESTONES-RuleSet-as-Source-of-Truth.md)
- [架构委员会会议纪要](../../../docs/governance/ARCHITECTURE-BOARD-MEETING-2026-02-10.md)
- [PR #365 - 架构治理系统整合分析](https://github.com/douhuaa/Zss.BilliardHall/pull/365)

## ✅ Epic 完成标准

- [ ] 3 个代码生成器实现并测试通过
- [ ] 9 个 Agent 使用 RuleSet API
- [ ] 9 个 Skills 使用 RuleSet API
- [ ] 43 个 RuleSet 有完整业务规则测试（500+ 测试）
- [ ] 43 个 ADR Decision 章节自动生成
- [ ] CI 一致性验证集成
- [ ] 所有文档更新完成
- [ ] 架构委员会最终批准

## 📞 联系

- **Epic 负责人**: Copilot Agent
- **架构委员会**: @douhuaa
- **项目看板**: [GitHub Projects - RuleSet SoT](#)
```

---

## 🏁 2. 创建 3 个 Milestones

### Milestone 1: Tools & API Integration

**标题**: `Milestone 1: Tools & API Integration`

**截止日期**: `2026-02-19`

**描述**:
```markdown
## 目标

实现 3 个核心代码生成器，并完成所有 Agent/Skills 的 RuleSet API 集成

## 范围

### 阶段 1: 构建生成工具（4-5天）
- ADR Decision 生成器
- 测试生成器  
- Agent 指令生成器

### 阶段 2: Agent/Skills 集成（3-4天）
- 9 个 Agent 使用 RuleSet API
- 9 个 Skills 使用 RuleSet API

## 交付物

- 3 个代码生成器（单元测试覆盖率 > 80%）
- 18 个更新的 Agent/Skills
- 移除所有硬编码 ADR 引用

## 验收标准

- [ ] 3 个生成器实现完成
- [ ] 单元测试覆盖率 > 80%
- [ ] 9 个 Agent 使用 RuleSetRegistry API
- [ ] 9 个 Skills 使用 RuleSetRegistry API
- [ ] 代码审查通过
```

---

### Milestone 2: Test & Documentation Generation

**标题**: `Milestone 2: Test & Documentation Generation`

**截止日期**: `2026-02-26`

**描述**:
```markdown
## 目标

为所有 RuleSet 生成业务规则测试，并重新生成 ADR Decision 章节

## 范围

### 阶段 3: 批量生成测试套件（3-4天）
- 为 38 个 RuleSet 生成业务测试
- 测试总数从 321 增加到 500+

### 阶段 4: 重新生成 ADR Decision（2-3天）
- 为 43 个 ADR 重新生成 Decision 章节
- 保留 Context/Consequences

## 交付物

- 200-300 个新测试（覆盖 43/43 RuleSet）
- 43 个更新的 ADR 文档
- 版本号同步验证

## 验收标准

- [ ] 43 个 RuleSet 测试覆盖 100%
- [ ] 所有新生成测试通过
- [ ] 43 个 ADR Decision 重新生成
- [ ] 版本号同步验证通过
```

---

### Milestone 3: CI Integration & Validation

**标题**: `Milestone 3: CI Integration & Validation`

**截止日期**: `2026-03-05`

**描述**:
```markdown
## 目标

实现 CI 自动验证，端到端测试，性能优化，完成文档更新

## 范围

### 阶段 5: CI 自动验证（2-3天）
- RuleSet ↔ ADR 一致性检查器
- RuleSet ↔ 测试覆盖度检查器
- CI 集成，PR 合并前强制验证

### 阶段 6: 验证与优化（3-4天）
- 端到端验证
- 性能优化
- 文档更新

## 交付物

- 2 个一致性检查器
- CI 流程更新
- 性能基准达标
- 完整文档更新

## 验收标准

- [ ] 一致性检查器实现完成
- [ ] CI 集成完成，PR 合并前自动验证
- [ ] 端到端测试通过
- [ ] 性能基准达标
- [ ] 文档更新完成
- [ ] 架构委员会最终批准
```

---

## 📊 3. 创建 GitHub Project 看板

### Project 信息

**名称**: `RuleSet as Source of Truth`

**描述**:
```
将 RuleSet 确立为架构治理的唯一真相源，实现 ADR 文档、测试、Agent 指令的自动生成与同步。

总工期：18-24 工作日
ROI：350%
```

### 看板结构

#### 视图 1: 按状态（Status Board）

**列（Columns）**:
1. **📋 To Do** - 待办任务
2. **🚧 In Progress** - 进行中
3. **👀 Review** - 待审查
4. **✅ Done** - 已完成

#### 视图 2: 按里程碑（Milestone View）

**分组（Groups）**:
- Milestone 1: Tools & API Integration
- Milestone 2: Test & Documentation Generation
- Milestone 3: CI Integration & Validation

#### 视图 3: 按优先级（Priority View）

**分组（Groups）**:
- 🔴 High Priority
- 🟡 Medium Priority
- 🟢 Low Priority

### 自动化规则

1. **新 Issue 自动添加到看板**
   - 触发器: Issue 带有标签 `epic:ruleset-sot`
   - 动作: 添加到 Project，状态设为 "To Do"

2. **PR 创建自动移动到 Review**
   - 触发器: PR 关联 Issue
   - 动作: 移动关联 Issue 到 "Review"

3. **PR 合并自动完成**
   - 触发器: PR 合并
   - 动作: 移动关联 Issue 到 "Done"

---

## 📝 4. 创建子任务 Issues

### Phase 0 Issue（可选，已完成）

**标题**: `[Phase 0] 治理与准备`  
**状态**: ✅ 已完成  
**Milestone**: Milestone 1

---

### Phase 1 Issue

**标题**: `[Phase 1] 构建代码生成工具`

**标签**: `epic:ruleset-sot`, `phase-1`, `priority-high`, `code-generation`

**Milestone**: Milestone 1

**Assignees**: Copilot

**描述**:
```markdown
## 目标

实现 3 个核心代码生成器：
1. ADR Decision 生成器
2. 测试生成器
3. Agent 指令生成器

## 任务清单

### 1. ADR Decision 生成器
- [ ] 设计接口 `IAdrDecisionGenerator`
- [ ] 实现 Markdown 格式化器
- [ ] 实现 RuleId 映射逻辑（`ADR-XXX_Y_Z`）
- [ ] 保留 Context/Consequences 原内容
- [ ] 单元测试（覆盖率 > 80%）

### 2. 测试生成器
- [ ] 设计接口 `IArchitectureTestGenerator`
- [ ] 实现 xUnit 测试模板
- [ ] 实现 NetArchTest 断言生成
- [ ] 生成测试命名逻辑
- [ ] 单元测试（覆盖率 > 80%）

### 3. Agent 指令生成器
- [ ] 设计接口 `IAgentInstructionGenerator`
- [ ] 实现 YAML 格式化器
- [ ] 生成 RuleSet API 查询示例
- [ ] 生成约束检查逻辑
- [ ] 单元测试（覆盖率 > 80%）

## 验收标准

- [ ] 3 个生成器接口定义完成
- [ ] 3 个生成器实现完成
- [ ] 单元测试覆盖率 > 80%
- [ ] 集成测试通过
- [ ] 代码审查通过

## 预计时间

4-5 工作日

## 相关文档

- [Epic 文档](../../../docs/governance/EPIC-RuleSet-as-Source-of-Truth.md)
- [里程碑规划](../../../docs/governance/MILESTONES-RuleSet-as-Source-of-Truth.md)
```

---

### Phase 2 Issue

**标题**: `[Phase 2] Agent/Skills 集成 RuleSet API`

**标签**: `epic:ruleset-sot`, `phase-2`, `priority-high`, `agent`, `skills`

**Milestone**: Milestone 1

**Assignees**: Copilot

**描述**:
```markdown
## 目标

更新 9 个 Agent 和 9 个 Skills，使用 RuleSetRegistry API 替代 Markdown 解析

## 任务清单

### Agent 重构（9个）
- [ ] Architecture Guardian
- [ ] ADR Reviewer
- [ ] Test Generator
- [ ] Module Boundary Checker
- [ ] Handler Pattern Enforcer
- [ ] Documentation Maintainer
- [ ] Expert Dotnet Engineer
- [ ] 移除硬编码 ADR 文本引用

### Skills 重构（9个）
- [ ] generate-test
- [ ] generate-adr
- [ ] generate-handler
- [ ] generate-endpoint
- [ ] run-architecture-tests
- [ ] scan-cross-module-refs
- [ ] update-documentation
- [ ] 移除硬编码规则

## 验收标准

- [ ] 9 个 Agent 使用 RuleSetRegistry API
- [ ] 9 个 Skills 使用 RuleSetRegistry API
- [ ] 移除所有硬编码 ADR 引用
- [ ] 集成测试通过
- [ ] 代码审查通过

## 预计时间

3-4 工作日

## 依赖

- Phase 1 完成（需要生成器接口）
```

---

### Phase 3 Issue

**标题**: `[Phase 3] 批量生成测试套件`

**标签**: `epic:ruleset-sot`, `phase-3`, `priority-high`, `testing`

**Milestone**: Milestone 2

**Assignees**: Copilot

**描述**:
```markdown
## 目标

为 38 个未完整覆盖的 RuleSet 生成业务规则测试

## 任务清单

- [ ] 使用测试生成器批量生成（38 个 RuleSet）
- [ ] 人工审查前 10 个生成的测试
- [ ] 调整模板（如需要）
- [ ] 批量生成剩余测试
- [ ] 运行测试套件，修复失败
- [ ] 验证测试覆盖率从 5 → 43
- [ ] 测试总数从 321 → 500+

## 验收标准

- [ ] 43 个 RuleSet 均有完整业务规则测试
- [ ] 所有新生成测试通过
- [ ] 测试命名符合 `ADR-XXX_Y_Z` 规范
- [ ] 代码审查通过

## 预计时间

3-4 工作日

## 依赖

- Phase 1 完成（需要测试生成器）
```

---

### Phase 4 Issue

**标题**: `[Phase 4] 重新生成 ADR Decision 章节`

**标签**: `epic:ruleset-sot`, `phase-4`, `priority-high`, `documentation`

**Milestone**: Milestone 2

**Assignees**: Copilot

**描述**:
```markdown
## 目标

为 43 个 ADR 重新生成 Decision 章节，保留人工编写的 Context/Consequences

## 任务清单

- [ ] 备份现有 43 个 ADR 文档
- [ ] 使用 Decision 生成器批量生成
- [ ] 人工审查前 5 个生成结果
- [ ] 调整模板（如需要）
- [ ] 批量生成剩余 ADR Decision 章节
- [ ] 验证 RuleId 格式（`ADR-XXX_Y_Z`）
- [ ] 更新 ADR-RELATIONSHIP-MAP
- [ ] 版本号同步检查

## 验收标准

- [ ] 43 个 ADR Decision 章节重新生成
- [ ] Context/Consequences 内容保留
- [ ] RuleId 格式正确
- [ ] 版本号与 RuleSet 同步
- [ ] 人工审查通过

## 预计时间

2-3 工作日

## 依赖

- Phase 1 完成（需要 Decision 生成器）
```

---

### Phase 5 Issue

**标题**: `[Phase 5] CI 自动验证一致性`

**标签**: `epic:ruleset-sot`, `phase-5`, `priority-medium`, `ci`, `automation`

**Milestone**: Milestone 3

**Assignees**: Copilot

**描述**:
```markdown
## 目标

实现 CI 流程，自动验证 RuleSet ↔ ADR ↔ 测试的一致性

## 任务清单

### 1. RuleSet ↔ ADR 一致性检查
- [ ] 实现检查器 `RuleSetAdrConsistencyChecker`
- [ ] 验证 RuleId 映射完整性
- [ ] 验证版本号同步
- [ ] 生成差异报告

### 2. RuleSet ↔ 测试覆盖度检查
- [ ] 实现检查器 `RuleSetTestCoverageChecker`
- [ ] 验证每个 RuleSet 有对应测试
- [ ] 验证测试命名符合规范
- [ ] 生成覆盖率报告

### 3. CI 集成
- [ ] 更新 `.github/workflows/architecture-tests.yml`
- [ ] 添加一致性检查步骤
- [ ] PR 合并前强制验证
- [ ] 失败时阻断合并

## 验收标准

- [ ] 一致性检查器实现完成
- [ ] 覆盖度检查器实现完成
- [ ] CI 集成完成
- [ ] PR 合并前自动验证
- [ ] 测试通过

## 预计时间

2-3 工作日

## 依赖

- Phase 3, 4 完成（需要生成的测试和文档）
```

---

### Phase 6 Issue

**标题**: `[Phase 6] 验证与优化`

**标签**: `epic:ruleset-sot`, `phase-6`, `priority-medium`, `validation`, `optimization`

**Milestone**: Milestone 3

**Assignees**: Copilot

**描述**:
```markdown
## 目标

端到端验证，性能优化，文档更新

## 任务清单

### 1. 端到端验证
- [ ] 创建新 RuleSet，验证自动生成流程
- [ ] 更新现有 RuleSet，验证同步机制
- [ ] Agent 查询 RuleSet 功能测试
- [ ] CI 流程完整性测试

### 2. 性能优化
- [ ] RuleSetRegistry 加载性能测试
- [ ] 生成器性能基准测试
- [ ] CI 执行时间优化
- [ ] 缓存机制（如需要）

### 3. 文档更新
- [ ] 更新架构指南
- [ ] 更新 Agent/Skills README
- [ ] 编写 RuleSet API 使用手册
- [ ] 更新贡献指南

## 验收标准

- [ ] 端到端测试通过
- [ ] 性能基准达标
- [ ] 所有文档更新完成
- [ ] 最终代码审查通过
- [ ] 架构委员会最终批准

## 预计时间

3-4 工作日

## 依赖

- Phase 5 完成（需要 CI 验证机制）
```

---

## 🏷️ 标签体系

建议创建以下标签：

### Epic 标签
- `epic` - Epic 级别任务
- `epic:ruleset-sot` - 本 Epic 专用标签

### 阶段标签
- `phase-0` - 治理与准备
- `phase-1` - 构建生成工具
- `phase-2` - API 集成
- `phase-3` - 测试生成
- `phase-4` - 文档生成
- `phase-5` - CI 集成
- `phase-6` - 验证优化

### 优先级标签
- `priority-high` - 高优先级
- `priority-medium` - 中优先级
- `priority-low` - 低优先级

### 类型标签
- `architecture` - 架构相关
- `governance` - 治理相关
- `code-generation` - 代码生成
- `agent` - Agent 相关
- `skills` - Skills 相关
- `testing` - 测试相关
- `documentation` - 文档相关
- `ci` - CI/CD 相关
- `automation` - 自动化相关
- `validation` - 验证相关
- `optimization` - 优化相关

---

## ✅ 创建步骤总结

### 第一步：创建标签（如需要）
1. 进入 Repository Settings → Labels
2. 创建上述标签体系

### 第二步：创建 Milestones
1. 进入 Issues → Milestones → New Milestone
2. 按顺序创建 3 个 Milestones
3. 设置截止日期和描述

### 第三步：创建 GitHub Project
1. 进入 Projects → New Project
2. 选择 Board 模板
3. 创建列：To Do / In Progress / Review / Done
4. 设置自动化规则

### 第四步：创建 Epic Issue
1. 进入 Issues → New Issue
2. 填写标题、描述、标签、Milestone
3. 关联到 Project

### 第五步：创建 Phase Issues
1. 按顺序创建 Phase 1-6 Issues
2. 每个 Issue 关联到相应 Milestone
3. 添加适当标签
4. 关联到 Project

### 第六步：验证
1. 检查 Project 看板显示所有 Issues
2. 检查 Milestones 关联正确
3. 检查标签和优先级

---

## 📚 参考文档

- [Epic 文档](EPIC-RuleSet-as-Source-of-Truth.md)
- [里程碑规划](MILESTONES-RuleSet-as-Source-of-Truth.md)
- [架构委员会会议纪要](ARCHITECTURE-BOARD-MEETING-2026-02-10.md)
- [PR #365](https://github.com/douhuaa/Zss.BilliardHall/pull/365)

---

**创建状态**: 📋 待手动创建  
**负责人**: douhuaa  
**最后更新**: 2026-02-10  
**文档版本**: 1.0
