# GitHub Epic 创建脚本

**创建日期**: 2026-02-10  
**Epic**: RuleSet-as-Source-of-Truth  
**仓库**: douhuaa/Zss.BilliardHall

---

## 📋 执行清单

- [ ] 创建 3 个 Milestones
- [ ] 创建 Epic Issue
- [ ] 创建 GitHub Project 看板
- [ ] 创建子任务 Issues（按阶段）
- [ ] 关联所有 Issues 到 Epic
- [ ] 更新 PR #365

---

## 1️⃣ 创建 Milestones

### Milestone 1: Tools & API Integration

```bash
gh milestone create \
  --title "Tools & API Integration" \
  --description "实现 3 个核心代码生成器，并完成所有 Agent/Skills 的 RuleSet API 集成" \
  --due-date "2026-02-19"
```

**详细描述**:
```markdown
## 目标
实现 3 个核心代码生成器，并完成所有 Agent/Skills 的 RuleSet API 集成

## 范围
- 阶段 1: 构建生成工具（4-5天）
  - ADR Decision 生成器
  - 测试生成器
  - Agent 指令生成器
- 阶段 2: Agent/Skills 集成 RuleSet API（3-4天）
  - 更新 9 个 Agent
  - 更新 9 个 Skills
  - 移除硬编码引用

## 关键交付物
1. 3 个代码生成器（接口 + 实现）
2. 18 个更新的 Agent/Skills
3. 单元测试套件（覆盖率 > 80%）

## 验收标准
- ✅ 3 个生成器接口定义并实现完成
- ✅ 单元测试覆盖率 > 80%
- ✅ 集成测试通过
- ✅ 9 个 Agent 使用 RuleSetRegistry API
- ✅ 9 个 Skills 使用 RuleSetRegistry API
- ✅ 移除所有硬编码 ADR 引用
- ✅ 代码审查通过

## 时间
- 开始: 2026-02-11
- 截止: 2026-02-19
- 工期: 7-9 工作日
```

---

### Milestone 2: Test & Documentation Generation

```bash
gh milestone create \
  --title "Test & Documentation Generation" \
  --description "为所有 RuleSet 生成业务规则测试，并重新生成 ADR Decision 章节" \
  --due-date "2026-02-26"
```

**详细描述**:
```markdown
## 目标
为所有 RuleSet 生成业务规则测试，并重新生成 ADR Decision 章节

## 范围
- 阶段 3: 批量生成测试套件（3-4天）
  - 生成 38 个 RuleSet 测试
  - 测试覆盖率从 5 → 43
  - 测试总数从 321 → 500+
- 阶段 4: 重新生成 ADR Decision 章节（2-3天）
  - 重新生成 43 个 ADR Decision 章节
  - 保留 Context/Consequences
  - 版本号同步

## 关键交付物
1. 200-300 个新测试
2. 43 个更新的 ADR 文档

## 验收标准
- ✅ 43 个 RuleSet 均有完整业务规则测试
- ✅ 所有新生成测试通过
- ✅ 测试命名符合 ADR-XXX_Y_Z 规范
- ✅ 43 个 ADR Decision 章节重新生成
- ✅ Context/Consequences 内容保留
- ✅ RuleId 格式正确
- ✅ 版本号与 RuleSet 同步
- ✅ 人工审查通过

## 时间
- 开始: 2026-02-20
- 截止: 2026-02-26
- 工期: 5-7 工作日
```

---

### Milestone 3: CI Integration & Validation

```bash
gh milestone create \
  --title "CI Integration & Validation" \
  --description "实现 CI 自动验证，端到端测试，性能优化，完成文档更新" \
  --due-date "2026-03-05"
```

**详细描述**:
```markdown
## 目标
实现 CI 自动验证，端到端测试，性能优化，完成文档更新

## 范围
- 阶段 5: CI 自动验证一致性（2-3天）
  - RuleSet ↔ ADR 一致性检查器
  - RuleSet ↔ 测试覆盖度检查器
  - CI 集成
- 阶段 6: 验证与优化（3-4天）
  - 端到端验证
  - 性能优化
  - 文档更新

## 关键交付物
1. 2 个一致性检查器
2. CI 流程更新
3. 性能基准
4. 完整文档

## 验收标准
- ✅ 一致性检查器实现完成
- ✅ 覆盖度检查器实现完成
- ✅ CI 集成完成
- ✅ PR 合并前自动验证
- ✅ 端到端测试通过
- ✅ 性能基准达标
- ✅ 所有文档更新完成
- ✅ 最终代码审查通过
- ✅ 架构委员会最终批准

## 时间
- 开始: 2026-02-27
- 截止: 2026-03-05
- 工期: 5-7 工作日
```

---

## 2️⃣ 创建 Epic Issue

```bash
gh issue create \
  --title "Epic: RuleSet-as-Source-of-Truth" \
  --label "epic:ruleset-sot,priority:high,type:epic" \
  --body-file docs/governance/epic-issue-body.md
```

**Issue Body** (`docs/governance/epic-issue-body.md`):

```markdown
# Epic: RuleSet-as-Source-of-Truth

**Epic ID**: `ruleset-sot`  
**状态**: 📋 已批准，待实施  
**优先级**: 🔴 高  
**预计完成**: 2026-03-05  
**负责人**: @copilot  
**审批人**: @douhuaa

---

## 🎯 目标

将 **RuleSet（强类型规约系统）** 确立为架构治理的**唯一真相源**（Single Source of Truth），实现 ADR 文档、测试、Agent 指令的自动生成与同步，消除手工维护的不一致性。

## 📊 愿景架构

```
                    ┌─────────────────┐
                    │   RuleSet       │
                    │ (唯一真相源)    │  ◄─── 所有规则定义在此
                    └────────┬────────┘
                             │
         ┌───────────────────┼───────────────────┬──────────┐
         ▼                   ▼                   ▼          ▼
    ┌─────────┐        ┌──────────┐        ┌─────────┐  ┌──────┐
    │   ADR   │        │   测试    │        │  Agent  │  │  CI  │
    │  文档   │        │  生成     │        │  指令   │  │ 验证 │
    └─────────┘        └──────────┘        └─────────┘  └──────┘
    (自动派生)          (自动生成)          (基于 API)    (自动)
```

## 💼 价值主张

1. **消除不一致**: RuleSet ↔ ADR 自动同步，无手工维护
2. **提升准确性**: Agent 使用结构化 API，替代文本理解
3. **扩大覆盖**: 从 5 个完整测试扩展到 500+ 业务规则测试
4. **提高效率**: 年化节省 260 小时，ROI 350%

## 📈 量化指标

| 指标 | 基线 | 目标 | 提升 |
|------|------|------|------|
| RuleSet 覆盖率 | 93.5% (43/46) | 100% (46/46) | +2.5% |
| 测试覆盖率 | 11.6% (5/43) | 100% (43/43) | +88.4% |
| 测试总数 | 321 | 500+ | +55.8% |
| Agent API 使用率 | 0% (0/9) | 100% (9/9) | +100% |
| Skills API 使用率 | 0% (0/9) | 100% (9/9) | +100% |

## 🗺️ 实施路线图

### 里程碑

- **Milestone 1**: Tools & API Integration (7-9天)
  - 构建 3 个代码生成器
  - Agent/Skills 集成 RuleSet API
  - 📅 2026-02-11 → 2026-02-19

- **Milestone 2**: Test & Documentation Generation (5-7天)
  - 批量生成测试套件
  - 重新生成 ADR Decision 章节
  - 📅 2026-02-20 → 2026-02-26

- **Milestone 3**: CI Integration & Validation (5-7天)
  - CI 自动验证一致性
  - 端到端测试与性能优化
  - 📅 2026-02-27 → 2026-03-05

### 总工期
- **计划**: 18-24 工作日
- **预计完成**: 2026-03-05

## 🔗 相关资源

- 📄 [Epic 详细文档](../docs/governance/EPIC-RuleSet-as-Source-of-Truth.md)
- 📄 [里程碑规划](../docs/governance/MILESTONES-RuleSet-as-Source-of-Truth.md)
- 📄 [架构委员会会议纪要](../docs/governance/ARCHITECTURE-BOARD-MEETING-2026-02-10.md)
- 🔀 [PR #365](https://github.com/douhuaa/Zss.BilliardHall/pull/365)

## ✅ 整体验收标准

### 技术指标
- ✅ 3 个代码生成器实现完成
- ✅ 9 个 Agent 使用 RuleSet API
- ✅ 9 个 Skills 使用 RuleSet API
- ✅ 43/43 RuleSet 测试覆盖 100%
- ✅ 测试总数 > 500
- ✅ 43 个 ADR Decision 重新生成
- ✅ CI 一致性验证集成
- ✅ 性能基准达标

### 质量指标
- ✅ 单元测试覆盖率 > 80%
- ✅ 所有测试通过
- ✅ 代码审查通过
- ✅ 文档更新完成
- ✅ 架构委员会最终批准

## 🚨 风险管理

### Top 3 风险
1. **自动生成内容质量** (高风险)
   - 缓解: 人工审查前 10 个样本，调整模板
2. **生成测试失败率** (高风险)
   - 缓解: 人工审查样本，迭代优化
3. **CI 性能影响** (中风险)
   - 缓解: 增量验证，缓存机制

---

## 🔄 子任务追踪

### 阶段 0: 治理与准备 ✅
- [x] 架构委员会批准决策 (#TBD)
- [x] 创建会议纪要
- [x] 创建 Epic 文档
- [ ] 创建 GitHub Epic Issue (本 Issue)
- [ ] 创建 Milestones 和 Project
- [ ] PR #365 标记 Ready

### 阶段 1: 构建生成工具 (Milestone 1)
- [ ] 实现 ADR Decision 生成器 (#TBD)
- [ ] 实现测试生成器 (#TBD)
- [ ] 实现 Agent 指令生成器 (#TBD)

### 阶段 2: Agent/Skills 集成 (Milestone 1)
- [ ] 更新 9 个 Agent (#TBD)
- [ ] 更新 9 个 Skills (#TBD)

### 阶段 3: 批量生成测试 (Milestone 2)
- [ ] 批量生成测试套件 (#TBD)

### 阶段 4: 重新生成 ADR (Milestone 2)
- [ ] 重新生成 ADR Decision 章节 (#TBD)

### 阶段 5: CI 集成 (Milestone 3)
- [ ] 实现一致性检查器 (#TBD)
- [ ] CI 流程集成 (#TBD)

### 阶段 6: 验证与优化 (Milestone 3)
- [ ] 端到端验证 (#TBD)
- [ ] 性能优化 (#TBD)
- [ ] 文档更新 (#TBD)

---

**Epic 创建人**: @douhuaa  
**Epic 创建日期**: 2026-02-10  
**Epic 状态**: 📋 已批准，待实施
```

---

## 3️⃣ 创建 GitHub Project 看板

### 通过 GitHub Web UI 创建

1. 访问仓库页面：https://github.com/douhuaa/Zss.BilliardHall
2. 点击 "Projects" 标签
3. 点击 "New project"
4. 选择 "Board" 模板
5. 配置项目：
   - **名称**: `RuleSet-as-Source-of-Truth`
   - **描述**: `将 RuleSet 确立为架构治理的唯一真相源`
   - **Visibility**: Private（私有仓库默认）

### 看板配置

#### 列（Columns）
1. **📋 To Do** - 待开始
2. **🔨 In Progress** - 进行中
3. **👀 In Review** - 代码审查中
4. **✅ Done** - 已完成

#### 自动化规则
- Issue 创建后自动添加到 "To Do"
- Issue 状态变为 "In Progress" 时移动到对应列
- PR 创建后自动添加到 "In Review"
- Issue 关闭后自动移动到 "Done"

### 使用 GitHub CLI（推荐）

```bash
# 创建 Project
gh project create \
  --owner douhuaa \
  --title "RuleSet-as-Source-of-Truth" \
  --body "将 RuleSet 确立为架构治理的唯一真相源"

# 获取 Project ID（记下来用于后续操作）
gh project list --owner douhuaa
```

---

## 4️⃣ 创建子任务 Issues

### 阶段 0: 治理与准备

#### Issue 0.1: 创建 GitHub Epic 基础设施

```bash
gh issue create \
  --title "[Phase 0] 创建 GitHub Epic 基础设施" \
  --label "phase-0,priority:high,type:task" \
  --milestone "Tools & API Integration" \
  --body "$(cat <<'EOF'
## 任务描述
创建 Epic Issue、Milestones、GitHub Project 看板

## 检查清单
- [ ] 创建 Milestone 1: Tools & API Integration
- [ ] 创建 Milestone 2: Test & Documentation Generation
- [ ] 创建 Milestone 3: CI Integration & Validation
- [ ] 创建 Epic Issue
- [ ] 创建 GitHub Project 看板
- [ ] 配置看板列和自动化

## 验收标准
- ✅ 3 个 Milestones 创建成功
- ✅ Epic Issue 创建成功
- ✅ GitHub Project 创建成功
- ✅ 所有 Issues 关联到 Epic

## 时间估算
- 1 小时

## 关联
- Epic: #TBD
- 文档: docs/governance/GITHUB-SETUP-GUIDE.md
EOF
)"
```

#### Issue 0.2: PR #365 准备与标记

```bash
gh issue create \
  --title "[Phase 0] PR #365 准备与标记" \
  --label "phase-0,priority:high,type:task" \
  --milestone "Tools & API Integration" \
  --body "$(cat <<'EOF'
## 任务描述
更新 PR #365，添加 Epic 关联，标记为 Ready for Review

## 检查清单
- [ ] 更新 PR 描述，关联 Epic Issue
- [ ] 添加标签 `epic:ruleset-sot`
- [ ] 标记为 Ready for Review
- [ ] 通知架构委员会审查

## 验收标准
- ✅ PR #365 关联到 Epic
- ✅ PR 状态为 Ready for Review
- ✅ 所有检查通过

## 时间估算
- 30 分钟

## 关联
- Epic: #TBD
- PR: #365
EOF
)"
```

---

### 阶段 1: 构建生成工具

#### Issue 1.1: 实现 ADR Decision 生成器

```bash
gh issue create \
  --title "[Phase 1] 实现 ADR Decision 生成器" \
  --label "phase-1,priority:high,type:feature,milestone-1" \
  --milestone "Tools & API Integration" \
  --body "$(cat <<'EOF'
## 任务描述
实现 RuleSet → ADR Decision 自动生成器

## 技术规格
- 接口: `IAdrDecisionGenerator`
- 输入: RuleSet 对象
- 输出: Markdown 格式的 Decision 章节
- 保留: Context/Consequences 内容

## 检查清单
- [ ] 设计接口 `IAdrDecisionGenerator`
- [ ] 实现 Markdown 格式化器
- [ ] 实现 RuleId 映射逻辑 (`ADR-XXX_Y_Z`)
- [ ] 实现 Context/Consequences 保留机制
- [ ] 单元测试（覆盖率 > 80%）
- [ ] 集成测试
- [ ] 代码审查

## 示例输出
\`\`\`markdown
## Decision

### Rule 1: 模块隔离约束

#### Clause 1.1: 模块不相互引用
- **RuleId**: \`ADR-001_1_1\`
- **执行类型**: Convention
- **约束**: 模块之间不得直接引用
- **测试**: \`Module_ShouldNotReferenceOtherModules\`
\`\`\`

## 验收标准
- ✅ 接口定义清晰
- ✅ 生成内容格式正确
- ✅ RuleId 映射准确
- ✅ 单元测试覆盖率 > 80%
- ✅ 代码审查通过

## 时间估算
- 1.5-2 天

## 关联
- Epic: #TBD
- Milestone: Tools & API Integration
EOF
)"
```

#### Issue 1.2: 实现测试生成器

```bash
gh issue create \
  --title "[Phase 1] 实现测试生成器" \
  --label "phase-1,priority:high,type:feature,milestone-1" \
  --milestone "Tools & API Integration" \
  --body "$(cat <<'EOF'
## 任务描述
实现 RuleSet → ArchitectureTests 自动生成器

## 技术规格
- 接口: `IArchitectureTestGenerator`
- 输入: RuleSet 对象
- 输出: xUnit 测试类代码
- 框架: NetArchTest.Rules

## 检查清单
- [ ] 设计接口 `IArchitectureTestGenerator`
- [ ] 实现 xUnit 测试模板
- [ ] 实现 NetArchTest 断言生成
- [ ] 生成测试类/方法命名逻辑
- [ ] 单元测试（覆盖率 > 80%）
- [ ] 集成测试
- [ ] 代码审查

## 示例输出
\`\`\`csharp
[Fact]
public void ADR001_Rule1_Clause1_ModuleShouldNotReferenceOtherModules()
{
    var ruleSet = RuleSetRegistry.Get(1);
    var clause = ruleSet.GetClause(1, 1);
    
    var result = Types.InAssembly(typeof(RentalModule).Assembly)
        .ShouldNot()
        .HaveDependencyOn("Zss.BilliardHall.PointOfSale")
        .GetResult();
    
    Assert.True(result.IsSuccessful, 
        $"Violation of {clause.Id}: {clause.Condition}");
}
\`\`\`

## 验收标准
- ✅ 接口定义清晰
- ✅ 生成测试代码可编译
- ✅ 测试命名符合规范
- ✅ 单元测试覆盖率 > 80%
- ✅ 代码审查通过

## 时间估算
- 1.5-2 天

## 关联
- Epic: #TBD
- Milestone: Tools & API Integration
EOF
)"
```

#### Issue 1.3: 实现 Agent 指令生成器

```bash
gh issue create \
  --title "[Phase 1] 实现 Agent 指令生成器" \
  --label "phase-1,priority:medium,type:feature,milestone-1" \
  --milestone "Tools & API Integration" \
  --body "$(cat <<'EOF'
## 任务描述
实现 RuleSet → Agent Instructions 自动生成器

## 技术规格
- 接口: `IAgentInstructionGenerator`
- 输入: RuleSet 对象
- 输出: YAML 格式的 Agent 指令
- 包含: RuleSet API 查询示例

## 检查清单
- [ ] 设计接口 `IAgentInstructionGenerator`
- [ ] 实现 YAML 格式化器
- [ ] 生成 RuleSet API 查询示例
- [ ] 生成约束检查逻辑
- [ ] 单元测试（覆盖率 > 80%）
- [ ] 集成测试
- [ ] 代码审查

## 示例输出
\`\`\`yaml
architecture_constraints:
  query_method: |
    var ruleSet = RuleSetRegistry.Get({adr_number});
    var clause = ruleSet.GetClause({rule}, {clause});
  validation:
    - check: clause.ExecutionType
      action: enforce_or_warn
\`\`\`

## 验收标准
- ✅ 接口定义清晰
- ✅ 生成 YAML 格式正确
- ✅ API 查询示例可用
- ✅ 单元测试覆盖率 > 80%
- ✅ 代码审查通过

## 时间估算
- 1-1.5 天

## 关联
- Epic: #TBD
- Milestone: Tools & API Integration
EOF
)"
```

---

### 阶段 2: Agent/Skills 集成

#### Issue 2.1: 更新 9 个 Agent 使用 RuleSet API

```bash
gh issue create \
  --title "[Phase 2] 更新 9 个 Agent 使用 RuleSet API" \
  --label "phase-2,priority:high,type:refactor,milestone-1" \
  --milestone "Tools & API Integration" \
  --body "$(cat <<'EOF'
## 任务描述
重构所有 Agent，使用 RuleSetRegistry API 替代 Markdown 解析

## Agent 列表
1. Architecture Guardian
2. ADR Reviewer
3. Test Generator
4. Module Boundary Checker
5. Handler Pattern Enforcer
6. Documentation Maintainer
7. Expert Dotnet Engineer

## 检查清单
- [ ] Architecture Guardian - 使用 RuleSet 验证变更
- [ ] ADR Reviewer - 查询 RuleSet 检查一致性
- [ ] Test Generator - 基于 RuleSet 生成测试
- [ ] Module Boundary Checker - 查询边界规则
- [ ] Handler Pattern Enforcer - 查询 Handler 约束
- [ ] Documentation Maintainer - 同步 RuleSet 更新
- [ ] Expert Dotnet Engineer - 查询技术规范
- [ ] 移除所有硬编码 ADR 文本引用
- [ ] 集成测试
- [ ] 代码审查

## 重构模式
**之前**:
\`\`\`yaml
constraints:
  module_isolation: "模块不相互引用"  # 硬编码
\`\`\`

**之后**:
\`\`\`csharp
var ruleSet = RuleSetRegistry.Get(1);  // ADR-001
var clause = ruleSet.GetClause(1, 1);  // Rule 1, Clause 1
// clause.Condition: "模块不相互引用"
\`\`\`

## 验收标准
- ✅ 9 个 Agent 使用 RuleSetRegistry API
- ✅ 移除所有硬编码 ADR 引用
- ✅ 集成测试通过
- ✅ 代码审查通过

## 时间估算
- 2-2.5 天

## 关联
- Epic: #TBD
- Milestone: Tools & API Integration
- 依赖: #TBD (Issue 1.1-1.3)
EOF
)"
```

#### Issue 2.2: 更新 9 个 Skills 使用 RuleSet API

```bash
gh issue create \
  --title "[Phase 2] 更新 9 个 Skills 使用 RuleSet API" \
  --label "phase-2,priority:high,type:refactor,milestone-1" \
  --milestone "Tools & API Integration" \
  --body "$(cat <<'EOF'
## 任务描述
重构所有 Skills，使用 RuleSetRegistry API 替代硬编码规则

## Skills 列表
1. generate-test
2. generate-adr
3. generate-handler
4. generate-endpoint
5. run-architecture-tests
6. scan-cross-module-refs
7. update-documentation

## 检查清单
- [ ] generate-test - 基于 RuleSet 生成测试代码
- [ ] generate-adr - 使用生成器更新 Decision
- [ ] generate-handler - 查询 Handler 约束
- [ ] generate-endpoint - 查询 API 规范
- [ ] run-architecture-tests - 报告 RuleId
- [ ] scan-cross-module-refs - 查询边界规则
- [ ] update-documentation - 同步 RuleSet 变更
- [ ] 移除所有硬编码规则
- [ ] 集成测试
- [ ] 代码审查

## 验收标准
- ✅ 9 个 Skills 使用 RuleSetRegistry API
- ✅ 移除所有硬编码规则
- ✅ 集成测试通过
- ✅ 代码审查通过

## 时间估算
- 1.5-2 天

## 关联
- Epic: #TBD
- Milestone: Tools & API Integration
- 依赖: #TBD (Issue 1.1-1.3)
EOF
)"
```

---

### 阶段 3: 批量生成测试

#### Issue 3.1: 批量生成测试套件

```bash
gh issue create \
  --title "[Phase 3] 批量生成测试套件" \
  --label "phase-3,priority:high,type:feature,milestone-2" \
  --milestone "Test & Documentation Generation" \
  --body "$(cat <<'EOF'
## 任务描述
为 38 个未完整覆盖的 RuleSet 生成业务规则测试

## 目标
- 测试覆盖率: 5/43 → 43/43 (100%)
- 测试总数: 321 → 500+
- 新增测试: 200-300 个

## 检查清单
- [ ] 识别 38 个未完整覆盖的 RuleSet
- [ ] 使用测试生成器批量生成
- [ ] 人工审查前 10 个生成的测试
- [ ] 调整模板（如需要）
- [ ] 批量生成剩余测试
- [ ] 运行测试套件
- [ ] 修复失败测试
- [ ] 验证测试覆盖率达到 100%
- [ ] 代码审查

## RuleSet 优先级
**P0 (核心架构)**:
- ADR-001, ADR-002, ADR-003, ADR-004, ADR-005

**P1 (执法层)**:
- ADR-007, ADR-900, ADR-907

**P2 (业务规则)**:
- ADR-201, ADR-240, ADR-250

## 验收标准
- ✅ 43/43 RuleSet 均有完整业务规则测试
- ✅ 所有新生成测试通过
- ✅ 测试命名符合 ADR-XXX_Y_Z 规范
- ✅ 测试总数 > 500
- ✅ 代码审查通过

## 时间估算
- 3-4 天

## 关联
- Epic: #TBD
- Milestone: Test & Documentation Generation
- 依赖: #TBD (Issue 1.2 测试生成器)
EOF
)"
```

---

### 阶段 4: 重新生成 ADR

#### Issue 4.1: 重新生成 ADR Decision 章节

```bash
gh issue create \
  --title "[Phase 4] 重新生成 ADR Decision 章节" \
  --label "phase-4,priority:high,type:feature,milestone-2" \
  --milestone "Test & Documentation Generation" \
  --body "$(cat <<'EOF'
## 任务描述
为 43 个 ADR 重新生成 Decision 章节，保留人工编写的 Context/Consequences

## 目标
- 重新生成: 43 个 ADR Decision 章节
- 格式统一: ADR-XXX_Y_Z
- 版本同步: 与 RuleSet 版本一致

## 检查清单
- [ ] 备份现有 43 个 ADR 文档
- [ ] 使用 Decision 生成器批量生成
- [ ] 人工审查前 5 个生成结果
- [ ] 调整模板（如需要）
- [ ] 批量生成剩余 ADR Decision 章节
- [ ] 验证 RuleId 格式 (`ADR-XXX_Y_Z`)
- [ ] 验证 Context/Consequences 保留
- [ ] 更新 ADR-RELATIONSHIP-MAP
- [ ] 版本号同步检查
- [ ] 人工审查通过

## ADR 优先级
**P0 (宪法层)**:
- ADR-006, ADR-007, ADR-008

**P1 (治理层)**:
- ADR-900, ADR-901, ADR-907

**P2 (运行层)**:
- ADR-201, ADR-240, ADR-250

## 验收标准
- ✅ 43 个 ADR Decision 章节重新生成
- ✅ Context/Consequences 内容保留
- ✅ RuleId 格式正确
- ✅ 版本号与 RuleSet 同步
- ✅ ADR-RELATIONSHIP-MAP 更新
- ✅ 人工审查通过

## 时间估算
- 2-3 天

## 关联
- Epic: #TBD
- Milestone: Test & Documentation Generation
- 依赖: #TBD (Issue 1.1 Decision 生成器)
EOF
)"
```

---

### 阶段 5: CI 集成

#### Issue 5.1: 实现一致性检查器

```bash
gh issue create \
  --title "[Phase 5] 实现一致性检查器" \
  --label "phase-5,priority:high,type:feature,milestone-3" \
  --milestone "CI Integration & Validation" \
  --body "$(cat <<'EOF'
## 任务描述
实现 RuleSet ↔ ADR 和 RuleSet ↔ 测试的一致性检查器

## 检查器列表
1. **RuleSet ↔ ADR 一致性检查器**
   - 验证 RuleId 映射完整性
   - 验证版本号同步
   - 生成差异报告

2. **RuleSet ↔ 测试覆盖度检查器**
   - 验证每个 RuleSet 有对应测试
   - 验证测试命名符合规范
   - 生成覆盖率报告

## 检查清单
- [ ] 设计检查器接口
- [ ] 实现 RuleSet ↔ ADR 一致性检查器
- [ ] 实现 RuleSet ↔ 测试覆盖度检查器
- [ ] 实现差异报告生成
- [ ] 单元测试（覆盖率 > 80%）
- [ ] 集成测试
- [ ] 代码审查

## 检查规则
**RuleId 映射**:
- RuleSet.Id ↔ ADR 编号
- RuleSet.Version ↔ ADR Version

**测试映射**:
- RuleSet.Id ↔ 测试类名
- Clause.Id ↔ 测试方法名

## 验收标准
- ✅ 2 个检查器实现完成
- ✅ 差异报告清晰
- ✅ 单元测试覆盖率 > 80%
- ✅ 集成测试通过
- ✅ 代码审查通过

## 时间估算
- 1.5-2 天

## 关联
- Epic: #TBD
- Milestone: CI Integration & Validation
EOF
)"
```

#### Issue 5.2: CI 流程集成

```bash
gh issue create \
  --title "[Phase 5] CI 流程集成" \
  --label "phase-5,priority:high,type:feature,milestone-3" \
  --milestone "CI Integration & Validation" \
  --body "$(cat <<'EOF'
## 任务描述
将一致性检查集成到 CI Pipeline，PR 合并前强制验证

## 检查清单
- [ ] 更新 `.github/workflows/architecture-tests.yml`
- [ ] 添加一致性检查步骤
- [ ] 配置 PR 合并前强制验证
- [ ] 配置失败时阻断合并
- [ ] 测试 CI 流程
- [ ] 文档更新

## CI 步骤
\`\`\`yaml
- name: RuleSet Consistency Check
  run: dotnet run --project tools/RuleSetConsistencyChecker
  
- name: Test Coverage Check
  run: dotnet run --project tools/RuleSetTestCoverageChecker
  
- name: Fail on Inconsistency
  if: failure()
  run: exit 1
\`\`\`

## 验收标准
- ✅ CI 集成完成
- ✅ PR 合并前自动验证
- ✅ 失败时阻断合并
- ✅ 测试 CI 流程通过
- ✅ 文档更新完成

## 时间估算
- 1 天

## 关联
- Epic: #TBD
- Milestone: CI Integration & Validation
- 依赖: #TBD (Issue 5.1 一致性检查器)
EOF
)"
```

---

### 阶段 6: 验证与优化

#### Issue 6.1: 端到端验证

```bash
gh issue create \
  --title "[Phase 6] 端到端验证" \
  --label "phase-6,priority:high,type:test,milestone-3" \
  --milestone "CI Integration & Validation" \
  --body "$(cat <<'EOF'
## 任务描述
端到端验证整个 RuleSet-as-Source-of-Truth 系统

## 验证场景
1. **创建新 RuleSet**
   - 验证 ADR Decision 自动生成
   - 验证测试自动生成
   - 验证 Agent 指令更新

2. **更新现有 RuleSet**
   - 验证 ADR 同步机制
   - 验证测试同步机制
   - 验证 CI 验证机制

3. **Agent 查询 RuleSet**
   - 验证 Agent 使用 RuleSetRegistry API
   - 验证查询结果准确性

## 检查清单
- [ ] 创建新 RuleSet 测试
- [ ] 更新现有 RuleSet 测试
- [ ] Agent 查询功能测试
- [ ] CI 流程完整性测试
- [ ] 性能基准测试
- [ ] 边界情况测试
- [ ] 回归测试

## 验收标准
- ✅ 所有端到端测试通过
- ✅ 性能基准达标
- ✅ 边界情况覆盖
- ✅ 回归测试通过

## 时间估算
- 1.5-2 天

## 关联
- Epic: #TBD
- Milestone: CI Integration & Validation
EOF
)"
```

#### Issue 6.2: 性能优化

```bash
gh issue create \
  --title "[Phase 6] 性能优化" \
  --label "phase-6,priority:medium,type:optimization,milestone-3" \
  --milestone "CI Integration & Validation" \
  --body "$(cat <<'EOF'
## 任务描述
优化 RuleSetRegistry 加载、生成器执行、CI 流程性能

## 性能目标
- RuleSetRegistry 加载时间: < 1s
- 生成器执行时间: < 10s/RuleSet
- CI 执行时间: < 5min

## 检查清单
- [ ] RuleSetRegistry 加载性能测试
- [ ] 生成器性能基准测试
- [ ] CI 执行时间分析
- [ ] 缓存机制设计（如需要）
- [ ] 并发优化（如需要）
- [ ] 性能测试验证
- [ ] 文档更新

## 优化策略
1. **懒加载**: RuleSet 按需加载
2. **缓存**: 缓存已生成内容
3. **并发**: 批量生成并发执行
4. **增量**: CI 只验证变更部分

## 验收标准
- ✅ 加载时间 < 1s
- ✅ 生成时间 < 10s/RuleSet
- ✅ CI 执行时间 < 5min
- ✅ 性能测试通过
- ✅ 文档更新完成

## 时间估算
- 1-1.5 天

## 关联
- Epic: #TBD
- Milestone: CI Integration & Validation
EOF
)"
```

#### Issue 6.3: 文档更新

```bash
gh issue create \
  --title "[Phase 6] 文档更新" \
  --label "phase-6,priority:high,type:documentation,milestone-3" \
  --milestone "CI Integration & Validation" \
  --body "$(cat <<'EOF'
## 任务描述
更新所有相关文档，包括架构指南、Agent 手册、贡献指南

## 文档清单
1. **架构指南**
   - RuleSet-as-Source-of-Truth 架构说明
   - 系统流程图
   - 最佳实践

2. **Agent/Skills 手册**
   - RuleSetRegistry API 使用指南
   - Agent 重构指南
   - Skills 开发指南

3. **贡献指南**
   - 如何添加新 RuleSet
   - 如何更新现有 RuleSet
   - CI 流程说明

4. **API 文档**
   - RuleSetRegistry API 参考
   - 生成器 API 参考
   - 检查器 API 参考

## 检查清单
- [ ] 更新架构指南
- [ ] 更新 Agent/Skills README
- [ ] 编写 RuleSet API 使用手册
- [ ] 更新贡献指南
- [ ] 生成 API 文档
- [ ] 添加使用示例
- [ ] 文档审查

## 验收标准
- ✅ 所有文档更新完成
- ✅ 文档清晰易懂
- ✅ 示例代码可运行
- ✅ 文档审查通过

## 时间估算
- 1-1.5 天

## 关联
- Epic: #TBD
- Milestone: CI Integration & Validation
EOF
)"
```

---

## 5️⃣ 关联所有 Issues 到 Epic

### 使用脚本批量关联

创建脚本 `link-issues-to-epic.sh`:

```bash
#!/bin/bash

# Epic Issue 编号（创建后填写）
EPIC_ISSUE_NUMBER="TBD"

# 所有子任务 Issue 编号（创建后填写）
ISSUE_NUMBERS=(
  "TBD"  # Issue 0.1
  "TBD"  # Issue 0.2
  "TBD"  # Issue 1.1
  "TBD"  # Issue 1.2
  "TBD"  # Issue 1.3
  "TBD"  # Issue 2.1
  "TBD"  # Issue 2.2
  "TBD"  # Issue 3.1
  "TBD"  # Issue 4.1
  "TBD"  # Issue 5.1
  "TBD"  # Issue 5.2
  "TBD"  # Issue 6.1
  "TBD"  # Issue 6.2
  "TBD"  # Issue 6.3
)

# 批量关联
for issue in "${ISSUE_NUMBERS[@]}"; do
  gh issue comment $EPIC_ISSUE_NUMBER --body "关联子任务: #$issue"
  gh issue comment $issue --body "关联 Epic: #$EPIC_ISSUE_NUMBER"
done

echo "所有 Issues 已关联到 Epic #$EPIC_ISSUE_NUMBER"
```

---

## 6️⃣ 更新 PR #365

```bash
# 添加 Epic 关联
gh pr edit 365 \
  --add-label "epic:ruleset-sot" \
  --body "$(cat <<'EOF'
## PR 概述
将 RuleSet 确立为架构治理的唯一真相源，实现 ADR、测试、Agent 指令的自动生成与同步

## 关联 Epic
- Epic: #TBD
- Milestone 1: Tools & API Integration
- Milestone 2: Test & Documentation Generation
- Milestone 3: CI Integration & Validation

## 变更说明
本 PR 作为 RuleSet-as-Source-of-Truth Epic 的基础，包含核心分析和治理文档

## 文档
- [Epic 文档](../docs/governance/EPIC-RuleSet-as-Source-of-Truth.md)
- [里程碑规划](../docs/governance/MILESTONES-RuleSet-as-Source-of-Truth.md)
- [架构委员会会议纪要](../docs/governance/ARCHITECTURE-BOARD-MEETING-2026-02-10.md)

## 验收标准
- ✅ 架构委员会批准
- ✅ Epic 创建完成
- ✅ GitHub 基础设施就绪

## 状态
📋 Ready for Review
EOF
)"

# 标记为 Ready
gh pr ready 365
```

---

## 📊 执行进度追踪

### 创建后更新以下信息

| 资源 | 编号 | 状态 | 创建时间 |
|------|------|------|----------|
| **Milestones** | | | |
| Milestone 1 | TBD | ⏳ 待创建 | - |
| Milestone 2 | TBD | ⏳ 待创建 | - |
| Milestone 3 | TBD | ⏳ 待创建 | - |
| **Epic Issue** | TBD | ⏳ 待创建 | - |
| **GitHub Project** | TBD | ⏳ 待创建 | - |
| **子任务 Issues** | | | |
| Issue 0.1 | TBD | ⏳ 待创建 | - |
| Issue 0.2 | TBD | ⏳ 待创建 | - |
| Issue 1.1 | TBD | ⏳ 待创建 | - |
| Issue 1.2 | TBD | ⏳ 待创建 | - |
| Issue 1.3 | TBD | ⏳ 待创建 | - |
| Issue 2.1 | TBD | ⏳ 待创建 | - |
| Issue 2.2 | TBD | ⏳ 待创建 | - |
| Issue 3.1 | TBD | ⏳ 待创建 | - |
| Issue 4.1 | TBD | ⏳ 待创建 | - |
| Issue 5.1 | TBD | ⏳ 待创建 | - |
| Issue 5.2 | TBD | ⏳ 待创建 | - |
| Issue 6.1 | TBD | ⏳ 待创建 | - |
| Issue 6.2 | TBD | ⏳ 待创建 | - |
| Issue 6.3 | TBD | ⏳ 待创建 | - |

---

## 🚀 快速执行命令

### 一键执行所有命令（创建后填写编号）

```bash
# 设置仓库
export REPO="douhuaa/Zss.BilliardHall"

# 1. 创建 Milestones
# （执行上面的 3 个 milestone create 命令）

# 2. 创建 Epic Issue
# （执行上面的 Epic issue create 命令）

# 3. 创建所有子任务 Issues
# （执行上面的 14 个 issue create 命令）

# 4. 关联 Issues 到 Epic
# （执行 link-issues-to-epic.sh 脚本）

# 5. 更新 PR #365
# （执行上面的 PR update 命令）
```

---

**脚本版本**: 1.0  
**创建日期**: 2026-02-10  
**最后更新**: 2026-02-10  
**维护人**: @copilot
