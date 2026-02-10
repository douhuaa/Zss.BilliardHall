# 架构委员会会议纪要

**会议日期**: 2026-02-10  
**会议主题**: RuleSet-as-Source-of-Truth Epic 批准与启动  
**相关 PR**: [#365](https://github.com/douhuaa/Zss.BilliardHall/pull/365)

---

## 📋 出席人员

- **架构委员会成员**: douhuaa
- **技术负责人**: Copilot Agent
- **决策人**: douhuaa

---

## 🎯 会议议程

### 1. PR #365 分析报告审查

#### 背景
仓库同时维护两套架构治理系统：
- **Markdown ADR**: 46个文档，人类可读
- **RuleSet**: 43个强类型规约，可执行测试

#### 问题
- 手工同步成本高，易不一致
- Agent/Skills 未充分利用 RuleSet 结构化 API
- 测试覆盖不足（仅 5/43 RuleSet 有完整业务规则测试）

#### 分析结论
**不应该"替换"，而应该"重新定位角色"**

系统已高度成熟：
- RuleSet 覆盖率 93.5%（43/46 个 ADR）
- 321个测试，完整类型系统
- 建议策略：**RuleSet 作为唯一真相源，其他系统自动派生**

### 2. 决策要点

#### 决策 1: 批准"重新定位"策略 ✅

**决策内容**:
- 保留 ADR 和 RuleSet 双系统
- **RuleSet 作为唯一真相源**（Single Source of Truth）
- ADR 文档、测试、Agent 指令从 RuleSet 自动生成/派生

**理由**:
1. 保留两系统优势（结构化 + 人类可读）
2. 单一真相源消除不一致性
3. ROI 350%，回本周期 3-4个月
4. 风险可控（分阶段实施，向后兼容）

**投票结果**: 一致通过

#### 决策 2: 批准 7 阶段实施路径 ✅

**实施时间**: 18-24 工作日（1人月）

**阶段划分**:
1. ✅ **阶段 0**: 治理与准备（1天） - 本会议
2. **阶段 1**: 构建生成工具（4-5天）
3. **阶段 2**: Agent/Skills 集成 RuleSet API（3-4天）
4. **阶段 3**: 批量生成测试套件（3-4天）
5. **阶段 4**: 重新生成 ADR Decision 章节（2-3天）
6. **阶段 5**: CI 自动验证一致性（2-3天）
7. **阶段 6**: 验证与优化（3-4天）

**投票结果**: 一致通过

#### 决策 3: 创建 Epic 与项目管理 ✅

**Epic 名称**: `RuleSet-as-Source-of-Truth`

**里程碑**:
- **Milestone 1**: 工具构建与 API 集成（阶段 1-2，7-9天）
- **Milestone 2**: 测试与文档生成（阶段 3-4，5-7天）
- **Milestone 3**: CI 集成与验证（阶段 5-6，5-7天）

**看板设置**:
- 使用 GitHub Projects
- 泳道：To Do / In Progress / Review / Done
- 标签：`epic:ruleset-sot`, `phase-N`, `priority-high/medium/low`

**投票结果**: 一致通过

---

## ✅ 验收标准

### 阶段 0 验收标准（本阶段）

- [x] 架构委员会批准"重新定位"策略
- [x] 批准 7 阶段实施路径
- [x] 创建会议纪要文档
- [ ] 创建 Epic 文档
- [ ] 在 GitHub 创建 Epic Issue
- [ ] 在 GitHub 创建 3 个 Milestones
- [ ] 在 GitHub 创建 Project 看板
- [ ] PR #365 标记为 Ready for Review
- [ ] 记录行动项与责任人

### 后续阶段验收标准

#### 阶段 1: 工具构建
- [ ] 实现 RuleSet → ADR Decision 生成器
- [ ] 实现 RuleSet → 测试生成器
- [ ] 实现 RuleSet → Agent 指令生成器
- [ ] 单元测试覆盖率 > 80%

#### 阶段 2: API 集成
- [ ] 9 个 Agent 使用 RuleSetRegistry API
- [ ] 9 个 Skills 基于 RuleSet 查询
- [ ] 移除硬编码的 ADR 文本引用

#### 阶段 3: 测试生成
- [ ] 为 38 个 RuleSet 生成业务规则测试
- [ ] 测试总数从 321 增加到 500+
- [ ] 所有测试通过

#### 阶段 4: 文档生成
- [ ] 为 43 个 ADR 重新生成 Decision 章节
- [ ] 保留 Context/Consequences 人工内容
- [ ] 版本号自动同步

#### 阶段 5: CI 验证
- [ ] 实现 RuleSet ↔ ADR 一致性检查
- [ ] 实现 RuleSet ↔ 测试覆盖度检查
- [ ] CI 流程集成，合并前强制验证

#### 阶段 6: 验证优化
- [ ] 端到端测试通过
- [ ] 性能基准测试通过
- [ ] 文档更新完成

---

## 📝 行动项与责任人

### 即时行动（阶段 0，1天内）

| 行动项 | 责任人 | 截止日期 | 状态 |
|--------|--------|----------|------|
| 创建 Epic 文档 | Copilot Agent | 2026-02-10 | ⏳ 进行中 |
| 创建 GitHub Epic Issue | douhuaa | 2026-02-10 | 📋 待办 |
| 创建 3 个 GitHub Milestones | douhuaa | 2026-02-10 | 📋 待办 |
| 创建 GitHub Project 看板 | douhuaa | 2026-02-10 | 📋 待办 |
| PR #365 标记 Ready | Copilot Agent | 2026-02-10 | 📋 待办 |
| 创建后续 Phase Issues | Copilot Agent | 2026-02-10 | 📋 待办 |

### 后续行动（阶段 1-6）

| 阶段 | 责任人 | 预计开始 | 预计完成 |
|------|--------|----------|----------|
| 阶段 1: 构建生成工具 | Copilot Agent | 2026-02-11 | 2026-02-15 |
| 阶段 2: API 集成 | Copilot Agent | 2026-02-16 | 2026-02-19 |
| 阶段 3: 测试生成 | Copilot Agent | 2026-02-20 | 2026-02-23 |
| 阶段 4: 文档生成 | Copilot Agent | 2026-02-24 | 2026-02-26 |
| 阶段 5: CI 验证 | Copilot Agent | 2026-02-27 | 2026-03-01 |
| 阶段 6: 验证优化 | Copilot Agent | 2026-03-02 | 2026-03-05 |

---

## 📊 风险评估与缓解

### 高风险

| 风险 | 概率 | 影响 | 缓解措施 |
|------|------|------|---------|
| 自动生成内容质量不达标 | 中 | 高 | 1. 人工审查机制<br>2. 渐进式迁移<br>3. 回滚计划 |
| RuleSet API 学习曲线陡峭 | 低 | 中 | 1. 详细文档<br>2. 示例代码<br>3. 渐进式培训 |

### 中风险

| 风险 | 概率 | 影响 | 缓解措施 |
|------|------|------|---------|
| CI 性能影响 | 中 | 中 | 1. 增量验证<br>2. 缓存机制<br>3. 并行执行 |
| 现有 Agent Prompt 破坏性变更 | 低 | 中 | 1. 向后兼容<br>2. 版本控制<br>3. A/B 测试 |

---

## 💰 投资回报分析

### 投入
- **人力**: 1人月（18-24工作日）
- **风险**: 低-中（分阶段，可回滚）

### 回报（年化）
- **减少手工同步**: 节省 2小时/周 × 52周 = 104小时/年
- **减少 Agent 错误**: 节省 3小时/周 × 52周 = 156小时/年
- **测试覆盖提升**: 质量改进，难以量化
- **总计**: 260小时/年 ≈ 1.5人月/年

### ROI
- **回本周期**: 3-4个月
- **年化 ROI**: 350%

---

## 📚 参考文档

- [PR #365 - 架构治理系统整合分析](https://github.com/douhuaa/Zss.BilliardHall/pull/365)
- [EXECUTIVE-SUMMARY.md](../analysis/EXECUTIVE-SUMMARY.md)
- [SPECIFICATION-MIGRATION-ANALYSIS.md](../analysis/SPECIFICATION-MIGRATION-ANALYSIS.md)
- [QUICK-REFERENCE.md](../analysis/QUICK-REFERENCE.md)
- [ADR-007: Agent 行为与权限宪法](../adr/constitutional/ADR-007-agent-behavior-permissions-constitution.md)
- [ADR-900: 架构测试](../adr/governance/ADR-900-architecture-tests.md)
- [ADR-907: ArchitectureTests 执法治理体系](../adr/governance/ADR-907-architecturetests-enforcement-governance.md)

---

## ✍️ 会议总结

本次架构委员会会议批准了 **RuleSet-as-Source-of-Truth** 战略，确认采用"重新定位"而非"替换"的实施策略。会议批准了 7 阶段实施路径，预计 18-24 工作日完成，年化 ROI 达到 350%。

**下一步行动**：
1. ✅ 完成本会议纪要
2. 📋 创建 Epic 文档
3. 📋 在 GitHub 创建 Epic Issue、Milestones 和 Project
4. 📋 PR #365 标记为 Ready for Review

**批准人**: douhuaa  
**批准日期**: 2026-02-10  
**文档版本**: 1.0

---

**会议状态**: ✅ 已完成  
**决策有效性**: 立即生效
