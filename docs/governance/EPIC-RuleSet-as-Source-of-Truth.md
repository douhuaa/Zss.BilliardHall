# Epic: RuleSet-as-Source-of-Truth

**Epic ID**: `ruleset-sot`  
**创建日期**: 2026-02-10  
**状态**: 📋 已批准，待实施  
**优先级**: 🔴 高  
**预计完成**: 2026-03-05  
**负责人**: Copilot Agent  
**审批人**: douhuaa

---

## 🎯 Epic 概述

### 目标

将 **RuleSet（强类型规约系统）** 确立为架构治理的**唯一真相源**（Single Source of Truth），实现 ADR 文档、测试、Agent 指令的自动生成与同步，消除手工维护的不一致性。

### 愿景

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

### 价值主张

1. **消除不一致**: RuleSet ↔ ADR 自动同步，无手工维护
2. **提升准确性**: Agent 使用结构化 API，替代文本理解
3. **扩大覆盖**: 从 5 个完整测试扩展到 500+ 业务规则测试
4. **提高效率**: 年化节省 260 小时，ROI 350%

---

## 📊 当前状态分析

### 系统现状

| 系统 | 规模 | 状态 | 问题 |
|------|------|------|------|
| **ADR 文档** | 46 个 | ✅ 结构化良好 | ⚠️ 手工同步 RuleSet |
| **RuleSet** | 43 个（93.5%覆盖） | ✅ 类型完整 | ⚠️ 未充分利用 |
| **测试** | 321 个 | ⚠️ 基础设施完善<br>⚠️ 业务测试仅 5 个 | ⚠️ 待批量生成 |
| **Agent** | 9 个 | ⚠️ 读 Markdown | ⚠️ 未用 RuleSet API |
| **Skills** | 9 个 | ⚠️ 硬编码规则 | ⚠️ 未查询 RuleSet |

### 核心问题

#### 问题 1: Agent 未使用 RuleSet API
**当前流程**:
```yaml
Agent → 读 ADR Markdown → 文本理解 → 生成代码
```
**影响**: 准确性依赖 NLP，易出错

**期望流程**:
```csharp
var ruleSet = RuleSetRegistry.Get(1);  // ADR-001
var clause = ruleSet.GetClause(1, 1);  // Rule 1, Clause 1
// clause.Id = "ADR-001_1_1"
// clause.Condition = "模块不相互引用"
// clause.ExecutionType = ClauseExecutionType.Convention
```
**收益**: 结构化查询，100% 准确

#### 问题 2: 缺少自动同步机制
**当前**: 手工维护 RuleSet ↔ ADR 一致性  
**风险**: RuleSet 更新后，ADR 可能过时  
**影响**: 治理体系可信度下降

**解决方案**: 自动生成 ADR Decision 章节

#### 问题 3: 测试覆盖不足
**当前**: 仅 5/43 RuleSet 有完整业务规则测试  
**机会**: 可自动生成 200-300 个测试  
**收益**: 全面验证架构约束

---

## 🗺️ 实施路线图

### 7 阶段实施计划（18-24 工作日）

#### 阶段 0: 治理与准备 ✅
**时间**: 1天  
**状态**: 进行中

- [x] 架构委员会批准决策
- [x] 创建会议纪要
- [ ] 创建 Epic 文档
- [ ] 创建 GitHub Epic Issue
- [ ] 创建 Milestones 和 Project
- [ ] PR #365 标记 Ready

**产物**:
- [x] 会议纪要: `ARCHITECTURE-BOARD-MEETING-2026-02-10.md`
- [ ] Epic 文档: `EPIC-RuleSet-as-Source-of-Truth.md`
- [ ] GitHub Issue: Epic #TBD
- [ ] GitHub Milestones: 3 个
- [ ] GitHub Project: RuleSet SoT

---

#### 阶段 1: 构建生成工具
**时间**: 4-5天  
**负责人**: Copilot Agent  
**预计**: 2026-02-11 至 2026-02-15

##### 目标
实现 3 个代码生成器：
1. **RuleSet → ADR Decision 生成器**
2. **RuleSet → 测试生成器**
3. **RuleSet → Agent 指令生成器**

##### 任务清单

###### 任务 1.1: ADR Decision 生成器
- [ ] 设计生成器接口 `IAdrDecisionGenerator`
- [ ] 实现 Markdown 格式化器
- [ ] 实现 RuleId 映射逻辑（`ADR-XXX_Y_Z`）
- [ ] 保留 Context/Consequences 原内容
- [ ] 单元测试（覆盖率 > 80%）

**示例输出**:
```markdown
## Decision

### Rule 1: 模块隔离约束

#### Clause 1.1: 模块不相互引用
- **RuleId**: `ADR-001_1_1`
- **执行类型**: Convention
- **约束**: 模块之间不得直接引用
- **测试**: `Module_ShouldNotReferenceOtherModules`
```

###### 任务 1.2: 测试生成器
- [ ] 设计生成器接口 `IArchitectureTestGenerator`
- [ ] 实现 xUnit 测试模板
- [ ] 实现 NetArchTest 断言生成
- [ ] 生成测试类/方法命名逻辑
- [ ] 单元测试（覆盖率 > 80%）

**示例输出**:
```csharp
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
```

###### 任务 1.3: Agent 指令生成器
- [ ] 设计生成器接口 `IAgentInstructionGenerator`
- [ ] 实现 YAML 格式化器
- [ ] 生成 RuleSet API 查询示例
- [ ] 生成约束检查逻辑
- [ ] 单元测试（覆盖率 > 80%）

**示例输出**:
```yaml
architecture_constraints:
  query_method: |
    var ruleSet = RuleSetRegistry.Get({adr_number});
    var clause = ruleSet.GetClause({rule}, {clause});
  validation:
    - check: clause.ExecutionType
      action: enforce_or_warn
```

##### 验收标准
- [ ] 3 个生成器接口定义完成
- [ ] 3 个生成器实现完成
- [ ] 单元测试覆盖率 > 80%
- [ ] 集成测试通过
- [ ] 代码审查通过

##### 风险与缓解
- **风险**: 生成内容质量不达标
- **缓解**: 人工审查前 10 个生成样本，调整模板

---

#### 阶段 2: Agent/Skills 集成 RuleSet API
**时间**: 3-4天  
**负责人**: Copilot Agent  
**预计**: 2026-02-16 至 2026-02-19

##### 目标
更新 9 个 Agent 和 9 个 Skills，使用 RuleSetRegistry API 替代 Markdown 解析

##### 任务清单

###### 任务 2.1: Agent 重构
- [ ] **Architecture Guardian** - 使用 RuleSet 验证变更
- [ ] **ADR Reviewer** - 查询 RuleSet 检查一致性
- [ ] **Test Generator** - 基于 RuleSet 生成测试
- [ ] **Module Boundary Checker** - 查询边界规则
- [ ] **Handler Pattern Enforcer** - 查询 Handler 约束
- [ ] **Documentation Maintainer** - 同步 RuleSet 更新
- [ ] **Expert Dotnet Engineer** - 查询技术规范
- [ ] 移除硬编码 ADR 引用（7 个 Agent）

###### 任务 2.2: Skills 重构
- [ ] **generate-test** - 基于 RuleSet 生成测试代码
- [ ] **generate-adr** - 使用生成器更新 Decision
- [ ] **generate-handler** - 查询 Handler 约束
- [ ] **generate-endpoint** - 查询 API 规范
- [ ] **run-architecture-tests** - 报告 RuleId
- [ ] **scan-cross-module-refs** - 查询边界规则
- [ ] **update-documentation** - 同步 RuleSet 变更
- [ ] 移除硬编码规则（9 个 Skills）

##### 验收标准
- [ ] 9 个 Agent 使用 RuleSetRegistry API
- [ ] 9 个 Skills 使用 RuleSetRegistry API
- [ ] 移除所有硬编码 ADR 文本引用
- [ ] 集成测试通过
- [ ] 代码审查通过

##### 风险与缓解
- **风险**: API 学习曲线陡峭
- **缓解**: 详细文档 + 示例代码库

---

#### 阶段 3: 批量生成测试套件
**时间**: 3-4天  
**负责人**: Copilot Agent  
**预计**: 2026-02-20 至 2026-02-23

##### 目标
为 38 个未完整覆盖的 RuleSet 生成业务规则测试

##### 任务清单
- [ ] 使用测试生成器批量生成（38 个 RuleSet）
- [ ] 人工审查前 10 个生成的测试
- [ ] 调整模板（如需要）
- [ ] 批量生成剩余测试
- [ ] 运行测试套件，修复失败
- [ ] 验证测试覆盖率从 5 → 43
- [ ] 测试总数从 321 → 500+

##### 预期成果
- **新增测试**: 200-300 个
- **覆盖 RuleSet**: 43/43（100%）
- **测试通过率**: 100%

##### 验收标准
- [ ] 43 个 RuleSet 均有完整业务规则测试
- [ ] 所有新生成测试通过
- [ ] 测试命名符合 `ADR-XXX_Y_Z` 规范
- [ ] 代码审查通过

---

#### 阶段 4: 重新生成 ADR Decision 章节
**时间**: 2-3天  
**负责人**: Copilot Agent  
**预计**: 2026-02-24 至 2026-02-26

##### 目标
为 43 个 ADR 重新生成 Decision 章节，保留人工编写的 Context/Consequences

##### 任务清单
- [ ] 备份现有 43 个 ADR 文档
- [ ] 使用 Decision 生成器批量生成
- [ ] 人工审查前 5 个生成结果
- [ ] 调整模板（如需要）
- [ ] 批量生成剩余 ADR Decision 章节
- [ ] 验证 RuleId 格式（`ADR-XXX_Y_Z`）
- [ ] 更新 ADR-RELATIONSHIP-MAP
- [ ] 版本号同步检查

##### 验收标准
- [ ] 43 个 ADR Decision 章节重新生成
- [ ] Context/Consequences 内容保留
- [ ] RuleId 格式正确
- [ ] 版本号与 RuleSet 同步
- [ ] 人工审查通过

---

#### 阶段 5: CI 自动验证一致性
**时间**: 2-3天  
**负责人**: Copilot Agent  
**预计**: 2026-02-27 至 2026-03-01

##### 目标
实现 CI 流程，自动验证 RuleSet ↔ ADR ↔ 测试的一致性

##### 任务清单

###### 任务 5.1: RuleSet ↔ ADR 一致性检查
- [ ] 实现检查器 `RuleSetAdrConsistencyChecker`
- [ ] 验证 RuleId 映射完整性
- [ ] 验证版本号同步
- [ ] 生成差异报告

###### 任务 5.2: RuleSet ↔ 测试覆盖度检查
- [ ] 实现检查器 `RuleSetTestCoverageChecker`
- [ ] 验证每个 RuleSet 有对应测试
- [ ] 验证测试命名符合 `ADR-XXX_Y_Z`
- [ ] 生成覆盖率报告

###### 任务 5.3: CI 集成
- [ ] 更新 `.github/workflows/architecture-tests.yml`
- [ ] 添加一致性检查步骤
- [ ] PR 合并前强制验证
- [ ] 失败时阻断合并

##### 验收标准
- [ ] 一致性检查器实现完成
- [ ] 覆盖度检查器实现完成
- [ ] CI 集成完成
- [ ] PR 合并前自动验证
- [ ] 测试通过

---

#### 阶段 6: 验证与优化
**时间**: 3-4天  
**负责人**: Copilot Agent  
**预计**: 2026-03-02 至 2026-03-05

##### 目标
端到端验证，性能优化，文档更新

##### 任务清单

###### 任务 6.1: 端到端验证
- [ ] 创建新 RuleSet，验证自动生成流程
- [ ] 更新现有 RuleSet，验证同步机制
- [ ] Agent 查询 RuleSet 功能测试
- [ ] CI 流程完整性测试

###### 任务 6.2: 性能优化
- [ ] RuleSetRegistry 加载性能测试
- [ ] 生成器性能基准测试
- [ ] CI 执行时间优化
- [ ] 缓存机制（如需要）

###### 任务 6.3: 文档更新
- [ ] 更新架构指南
- [ ] 更新 Agent/Skills README
- [ ] 编写 RuleSet API 使用手册
- [ ] 更新贡献指南

##### 验收标准
- [ ] 端到端测试通过
- [ ] 性能基准达标
- [ ] 所有文档更新完成
- [ ] 最终代码审查通过

---

## 📋 Milestones

### Milestone 1: 工具构建与 API 集成
**时间**: 7-9 天（阶段 1-2）  
**截止**: 2026-02-19

- [ ] 阶段 0: 治理与准备 ✅
- [ ] 阶段 1: 构建生成工具
- [ ] 阶段 2: Agent/Skills 集成 RuleSet API

### Milestone 2: 测试与文档生成
**时间**: 5-7 天（阶段 3-4）  
**截止**: 2026-02-26

- [ ] 阶段 3: 批量生成测试套件
- [ ] 阶段 4: 重新生成 ADR Decision 章节

### Milestone 3: CI 集成与验证
**时间**: 5-7 天（阶段 5-6）  
**截止**: 2026-03-05

- [ ] 阶段 5: CI 自动验证一致性
- [ ] 阶段 6: 验证与优化

---

## 📊 关键指标（KPIs）

### 量化指标

| 指标 | 基线 | 目标 | 测量方法 |
|------|------|------|---------|
| **RuleSet 覆盖率** | 93.5% (43/46) | 100% (46/46) | 自动扫描 |
| **测试覆盖率** | 11.6% (5/43) | 100% (43/43) | 测试扫描 |
| **测试总数** | 321 | 500+ | 测试运行报告 |
| **Agent API 使用率** | 0% (0/9) | 100% (9/9) | 代码审查 |
| **手工同步事件** | ~2 次/周 | 0 次/周 | 人工记录 |
| **一致性验证** | 手工 | CI 自动 | CI 日志 |

### 质量指标

| 指标 | 基线 | 目标 | 测量方法 |
|------|------|------|---------|
| **Agent 准确性** | 80% (估计) | 95%+ | 错误率统计 |
| **ADR 新鲜度** | 手工检查 | 自动同步 | 版本号对比 |
| **测试可维护性** | 手工编写 | 自动生成 | 生成比例 |

---

## 💰 投资回报分析

### 投入
- **开发时间**: 18-24 工作日（1人月）
- **维护成本**: 忽略不计（自动化后）
- **风险**: 低-中（分阶段，可回滚）

### 回报（年化）

#### 直接收益
1. **减少手工同步**: 2 小时/周 × 52 周 = **104 小时/年**
2. **减少 Agent 错误调试**: 3 小时/周 × 52 周 = **156 小时/年**
3. **减少测试编写**: 1 小时/周 × 52 周 = **52 小时/年**

**总计**: **312 小时/年** ≈ **1.8 人月/年**

#### 间接收益
- **质量提升**: 架构违规减少，技术债降低
- **响应速度**: Agent 生成代码更快
- **可扩展性**: 新增规则无需手工维护

### ROI
- **回本周期**: 3-4 个月
- **年化 ROI**: **350%**
- **3 年总收益**: 投入 1 人月，节省 5.4 人月

---

## 🚨 风险管理

### 高风险

| 风险 | 概率 | 影响 | 缓解措施 | 负责人 |
|------|------|------|---------|--------|
| 自动生成内容质量不达标 | 中 | 高 | 1. 人工审查前 10 个样本<br>2. 渐进式迁移<br>3. 保留回滚能力 | Copilot Agent |
| RuleSet API 学习曲线陡峭 | 低 | 中 | 1. 详细文档与示例<br>2. 培训材料<br>3. FAQ | Copilot Agent |

### 中风险

| 风险 | 概率 | 影响 | 缓解措施 | 负责人 |
|------|------|------|---------|--------|
| CI 性能影响 | 中 | 中 | 1. 增量验证<br>2. 缓存机制<br>3. 并行执行 | Copilot Agent |
| Agent Prompt 破坏性变更 | 低 | 中 | 1. 向后兼容<br>2. 版本控制<br>3. A/B 测试 | Copilot Agent |

### 低风险

| 风险 | 概率 | 影响 | 缓解措施 | 负责人 |
|------|------|------|---------|--------|
| 工具生成 Bug | 低 | 低 | 1. 单元测试 > 80%<br>2. 集成测试 | Copilot Agent |

---

## 📚 相关资源

### 文档
- [架构委员会会议纪要](ARCHITECTURE-BOARD-MEETING-2026-02-10.md)
- [PR #365 - 架构治理系统整合分析](https://github.com/douhuaa/Zss.BilliardHall/pull/365)
- [执行总结](../analysis/EXECUTIVE-SUMMARY.md)
- [完整技术分析](../analysis/SPECIFICATION-MIGRATION-ANALYSIS.md)
- [快速参考](../analysis/QUICK-REFERENCE.md)

### ADR 引用
- [ADR-007: Agent 行为与权限宪法](../adr/constitutional/ADR-007-agent-behavior-permissions-constitution.md)
- [ADR-900: 架构测试](../adr/governance/ADR-900-architecture-tests.md)
- [ADR-907: ArchitectureTests 执法治理体系](../adr/governance/ADR-907-architecturetests-enforcement-governance.md)
- [ADR-940: 文档版本同步协议](../adr/governance/ADR-940-documentation-version-sync-protocol.md)

### 代码库
- [RuleSet 实现](../../src/tests/ArchitectureTests/Specification/)
- [测试基础设施](../../src/tests/ArchitectureTests/)
- [Agent 配置](.github/agents/)
- [Skills 定义](.github/skills/)

---

## ✅ 验收清单

### Epic 完成标准

#### 技术交付
- [ ] 3 个代码生成器实现并测试通过
- [ ] 9 个 Agent 使用 RuleSet API
- [ ] 9 个 Skills 使用 RuleSet API
- [ ] 43 个 RuleSet 有完整业务规则测试
- [ ] 43 个 ADR Decision 章节自动生成
- [ ] CI 一致性验证集成

#### 质量标准
- [ ] 单元测试覆盖率 > 80%
- [ ] 所有测试通过（500+ 测试）
- [ ] 代码审查通过
- [ ] 性能基准达标

#### 文档完整
- [ ] 架构指南更新
- [ ] RuleSet API 手册
- [ ] Agent/Skills 文档更新
- [ ] 贡献指南更新

#### 治理合规
- [ ] 架构委员会最终审批
- [ ] ADR-RELATIONSHIP-MAP 更新
- [ ] 版本号同步
- [ ] 变更日志记录

---

## 📞 联系与支持

- **Epic 负责人**: Copilot Agent
- **架构委员会**: douhuaa
- **问题跟踪**: [GitHub Issues](https://github.com/douhuaa/Zss.BilliardHall/issues?q=label:epic:ruleset-sot)
- **项目看板**: [GitHub Projects - RuleSet SoT](https://github.com/douhuaa/Zss.BilliardHall/projects)

---

**Epic 状态**: 📋 已批准，待实施  
**最后更新**: 2026-02-10  
**文档版本**: 1.0
