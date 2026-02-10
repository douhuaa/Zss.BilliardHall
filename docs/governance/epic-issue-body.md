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
- [x] 架构委员会批准决策
- [x] 创建会议纪要
- [x] 创建 Epic 文档
- [ ] 创建 GitHub Epic Issue (本 Issue)
- [ ] 创建 Milestones 和 Project
- [ ] PR #365 标记 Ready

### 阶段 1: 构建生成工具 (Milestone 1)
- [ ] 实现 ADR Decision 生成器
- [ ] 实现测试生成器
- [ ] 实现 Agent 指令生成器

### 阶段 2: Agent/Skills 集成 (Milestone 1)
- [ ] 更新 9 个 Agent
- [ ] 更新 9 个 Skills

### 阶段 3: 批量生成测试 (Milestone 2)
- [ ] 批量生成测试套件

### 阶段 4: 重新生成 ADR (Milestone 2)
- [ ] 重新生成 ADR Decision 章节

### 阶段 5: CI 集成 (Milestone 3)
- [ ] 实现一致性检查器
- [ ] CI 流程集成

### 阶段 6: 验证与优化 (Milestone 3)
- [ ] 端到端验证
- [ ] 性能优化
- [ ] 文档更新

---

**Epic 创建人**: @douhuaa  
**Epic 创建日期**: 2026-02-10  
**Epic 状态**: 📋 已批准，待实施
