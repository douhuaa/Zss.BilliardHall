# ADR 对齐执行计划与系统同步总览

> **制定日期**：2026-02-03  
> **审批日期**：2026-02-03  
> **版本**：1.1  
> **状态**：✅ 已批准  
> **基于标准**：ADR-907 v2.0, ADR-907-A v1.1

---

## 📋 执行摘要

本文档是 ADR 体系全面对齐到 Rule/Clause 双层编号体系的总体执行计划，包含：
- ADR 内容对齐
- Agent Instructions 同步
- 测试执法模型更新
- 文档维护与验证

### 当前状态

| 维度 | 已完成 | 待完成 | 完成率 |
|------|--------|--------|--------|
| **ADR 对齐** | 26/48 个 | 22 个 | 54% |
| **测试对齐** | 4/29 个测试文件 | 25 个 | 14% |
| **Instructions 更新** | copilot-instructions.md | 7 个 agent 配置 | 10% |
| **文档同步** | 部分 | 交叉引用、链接 | 30% |

### 关键目标

- ✅ **100%** ADR 使用 Rule/Clause 双层编号
- ✅ **100%** 测试使用新 RuleId 格式
- ✅ **100%** Instructions 引用正确 ADR
- ✅ **0 个**格式违规
- ✅ **0 个**测试失败

---

## 🎯 三态判定结果

**总体判定**：✅ **Allowed - 已获批准执行**

**理由**：
1. ✅ 治理框架完备（ADR-907/907-A）
2. ✅ Architecture Board 已批准所有 5 个关键决策点（2026-02-03）
3. ⚠️ 测试格式违规（25 个旧格式测试）- 阶段 1 将修复
4. ✅ Agent 体系已就绪

**批准的关键决策**：
- ✅ 不允许保留旧测试，必须完全迁移
- ✅ 每批对齐 5 个 ADR
- ✅ 对齐期间新 ADR 必须使用新格式
- ✅ ADR-930 先补充完整再对齐
- ✅ 使用 Feature Branch `feature/adr-alignment`

**执行条件**：
- 所有关键决策已获 Architecture Board 批准
- 可立即开始阶段 1 执行

---

## 📊 详细分析报告

### 1. ADR 审查报告（adr-reviewer）

**已完成审查**：48 个 ADR 文档

**对齐状态**：

| 层级 | 总数 | 已对齐 | 需对齐 | 对齐率 |
|------|------|--------|--------|--------|
| 治理层（Governance） | 24 | 15 | 9 | 63% |
| 宪法层（Constitutional） | 8 | 8 | 0 | 100% |
| 结构层（Structure） | 6 | 1 | 5 | 17% |
| 运行层（Runtime） | 5 | 1 | 4 | 20% |
| 技术层（Technical） | 6 | 2 | 4 | 33% |

**关键发现**：
- ✅ 宪法层 ADR 内容质量高，仅需格式对齐
- ⚠️ 治理层 9 个 ADR 待对齐（高优先级）
- ⚠️ 部分 ADR 缺少统一铁律声明
- ⚠️ 部分 ADR 缺少完整 Enforcement 章节

**详细清单**：参见 [ADR Reviewer 完整报告](#adr-reviewer-report)

---

### 2. 测试对齐验证（test-generator）

**测试文件扫描结果**：

```
已对齐（新格式）：4 个
- ADR_900_Architecture_Tests.cs
- ADR_901_Architecture_Tests.cs
- ADR_902_Architecture_Tests.cs
- ADR-907/ 目录（4 个 Rule 测试类）

待对齐（旧格式）：25 个
- ADR_001 ~ 008（宪法层，8 个）
- ADR_120 ~ 124（结构层，5 个）
- ADR_201, 210, 220, 240（运行层，4 个）
- ADR_301, 340, 350, 360（技术层，4 个）
- ADR_910, 920, 930（治理层，3 个）
```

**违规情况**（基于 ADR-907_2_4, 2_5）：

| 违规类型 | 数量 | 严重性 |
|---------|------|--------|
| 测试类命名格式不符 | 25 个 | ⚠️ High |
| 失败消息引用旧格式 | 估计 100+ | ⚠️ Medium |
| 测试方法命名不一致 | 估计 200+ | ⚠️ Medium |

**FailureObject**：
```json
{
  "ruleId": "ADR-907_2_4",
  "severity": "Error",
  "message": "测试类命名必须使用 ADR_<编号>_<Rule>_Architecture_Tests 格式",
  "violatingFiles": ["ADR_001_Architecture_Tests.cs", "... 25 个文件"],
  "fix": {
    "strategy": "批量重构",
    "estimatedEffort": "2-3 天"
  }
}
```

**详细分析**：参见 [Test Generator 完整报告](#test-generator-report)

---

### 3. Instructions 同步分析（documentation-maintainer）

**已完成**：
- ✅ copilot-instructions.md 全面更新
  - 添加 ADR-007, 900, 907, 907-A 引用
  - 添加权威性声明
  - 完善三态输出规则（详细表格）
  - 添加 FailureObject 标准格式
  - 添加 RuleId 格式规范
  - 更新反馈闭环流程

**待完成**：
- [ ] 创建 `.github/agents/` 详细配置文件（7 个）
- [ ] 更新 YAML instructions 元数据（7 个）
- [ ] 同步文档中的示例和引用

**建议架构**：
```
.github/
├── copilot-instructions.md         # ✅ 已更新
├── instructions/                    # ⏸️ 保持简洁 YAML
│   ├── adr-reviewer.instructions.yaml
│   ├── architecture-guardian.instructions.yaml
│   └── ...
└── agents/                          # ❓ 需创建
    ├── adr-reviewer.agent.md        # 详细配置手册
    ├── architecture-guardian.agent.md
    └── ...
```

**详细分析**：参见 [Documentation Maintainer 完整报告](#doc-maintainer-report)

---

### 4. 执行策略协调（architecture-guardian）

**执行路线图**（5 个阶段）：

#### 阶段 1：紧急修复（P0）
- **时间**：2-3 天
- **任务**：拆分 25 个旧测试为 Rule 测试类
- **验证**：所有测试通过，命名规范验证通过

#### 阶段 2：治理层对齐（P1）
- **时间**：3-4 周
- **任务**：对齐 9 个治理层 ADR
- **批次**：分 2 批，每批 5 个（已获 Architecture Board 批准）

#### 阶段 3：宪法层对齐（P2）
- **时间**：4-5 周
- **任务**：对齐 8 个宪法层 ADR（格式调整）
- **批次**：分 2 批，每批 5 个或更少，按依赖关系排序

#### 阶段 4：其他层对齐（P3-P4）
- **时间**：3-4 周
- **任务**：对齐剩余 13 个 ADR
- **批次**：分 3 批，每批不超过 5 个，按层级分组

#### 阶段 5：验证与收尾
- **时间**：1 周
- **任务**：全量测试、文档同步、CI 验证

**Agent 职责分工**：

| Agent | 主要职责 | 负责 ADR 类型 |
|-------|---------|-------------|
| architecture-guardian | 总协调、三态判定 | 所有 |
| adr-reviewer | ADR 格式验证 | 所有 |
| test-generator | 测试生成/拆分 | 所有有测试的 ADR |
| documentation-maintainer | 文档链接、索引 | 文档类 ADR |
| module-boundary-checker | 模块边界检查 | ADR-001, 003 |
| handler-pattern-enforcer | Handler 模式检查 | ADR-005, 240 |
| expert-dotnet-software-engineer | .NET 技术咨询 | 技术类 ADR |

**详细路线图**：参见 [Architecture Guardian 完整报告](#arch-guardian-report)

---

## ⚠️ 风险评估与缓解

### 高风险项（P0）

#### 风险 1：批量重命名导致测试失败
- **概率**：Medium | **影响**：High
- **缓解措施**：
  - ✅ 使用 Feature Branch
  - ✅ 每次拆分后立即运行测试
  - ✅ 设置 Git Tag 回滚点

#### 风险 2：ADR 依赖关系导致连锁失败
- **概率**：Medium | **影响**：High
- **缓解措施**：
  - ✅ 使用 ADR-RELATIONSHIP-MAP.md 分析依赖
  - ✅ 优先对齐被依赖的 ADR
  - ✅ 对齐前检查依赖是否已对齐

### 中风险项（P1-P2）

#### 风险 3：对齐期间新 ADR 提交冲突
- **概率**：High | **影响**：Medium
- **缓解措施**：
  - ✅ 新 ADR 必须直接使用新格式
  - ✅ 使用 Git 分支隔离
  - ✅ Architecture Board 审批所有新 ADR

#### 风险 4：测试覆盖率下降
- **概率**：Low | **影响**：High
- **缓解措施**：
  - ✅ 每次拆分后运行覆盖率报告
  - ✅ 禁止删除有效断言

---

## ❓ 需要人工确认的决策点

以下 5 个决策点需要 **Architecture Board 审批**：

| # | 决策点 | ADR 覆盖 | Architecture Board 决策 | 优先级 |
|---|--------|---------|----------------------|--------|
| 1 | 是否允许保留旧测试作为兼容层？ | ADR-907-A 未明确 | ✅ 已批准：不允许，必须完全迁移 | P0 |
| 2 | 每批对齐的 ADR 数量上限？ | ADR-907-A 未明确 | ✅ 已批准：5 个 | P1 |
| 3 | 对齐期间新 ADR 如何处理？ | ADR-907-A 未明确 | ✅ 已批准：必须直接使用新格式 | P1 |
| 4 | ADR-930 如何处理？（Decision 章节不完整） | - | ✅ 已批准：先补充完整再对齐 | P2 |
| 5 | 是否需要 Feature Branch？ | ADR-900 未明确 | ✅ 已批准：使用 feature/adr-alignment | P0 |

**决策后行动**：
- 将决策结果记录到 ADR-907-A
- 更新本执行计划（已完成）
- 通知所有相关 Agent

---

## 📋 检查清单

### 阶段 1 完成检查清单
- [ ] 所有 25 个旧测试已拆分为 Rule 测试类
- [ ] 测试目录结构符合 `ADR-XXXX/` 格式
- [ ] 测试类命名符合 `ADR_XXX_<Rule>_Architecture_Tests`
- [ ] 测试方法命名符合 `ADR_XXX_<Rule>_<Clause>_<Description>`
- [ ] 所有测试通过（100% 通过率）
- [ ] 测试覆盖率不低于基线
- [ ] CI 验证通过

### 阶段 2-4 每批完成检查清单
- [ ] 批次内所有 ADR 已更新 Rule/Clause 结构
- [ ] 批次内所有 ADR 已创建对应架构测试
- [ ] 所有测试通过（100% 通过率）
- [ ] 对齐清单已更新状态
- [ ] 相关 Prompts 已同步更新
- [ ] 文档链接已验证
- [ ] Git Tag 已创建

### 阶段 5 最终检查清单
- [ ] 所有 48 个 ADR 已对齐（100%）
- [ ] 所有架构测试通过（100% 通过率）
- [ ] 对齐清单显示 0 个"待对齐"
- [ ] RuleId 唯一性验证通过
- [ ] 命名规范验证通过
- [ ] CI 集成验证通过
- [ ] 文档同步验证通过
- [ ] Architecture Board 最终审批

---

## 🚀 立即行动项

### ✅ 已批准决策（2026-02-03）

所有 5 个关键决策点已获 Architecture Board 批准：
1. ✅ 不允许保留旧测试作为兼容层，必须完全迁移
2. ✅ 每批对齐 ADR 数量上限：5 个
3. ✅ 对齐期间新 ADR 必须直接使用新格式
4. ✅ ADR-930 先补充完整 Decision 章节再对齐
5. ✅ 使用 Feature Branch `feature/adr-alignment`

### 可立即执行（无阻塞）

| # | 任务 | 负责人 | 预计时间 | 状态 |
|---|------|--------|---------|------|
| 1 | 创建 Feature Branch `feature/adr-alignment` | Human | 5 分钟 | 🟢 Ready |
| 2 | 调用 test-generator 生成测试拆分清单 | Guardian | 1 小时 | 🟢 Ready |
| 3 | 配置 CI 验证脚本 | Human + Guardian | 2 小时 | 🟢 Ready |
| 4 | 创建 GitHub Project Board 跟踪进度 | Human | 30 分钟 | 🟢 Ready |
| 5 | 开始阶段 1 执行（测试拆分） | test-generator + Human | 2-3 天 | 🟢 Ready |

### 需记录到 ADR-907-A 的决策

| 决策项 | 决策结果 | 优先级 |
|-------|---------|--------|
| 旧测试保留策略 | 不允许，必须完全迁移 | P0 |
| 批次对齐数量 | 5 个 ADR/批次 | P1 |
| 新 ADR 提交政策 | 必须直接使用新格式 | P1 |
| ADR-930 处理方式 | 先补充完整再对齐 | P2 |
| Feature Branch 策略 | 使用 feature/adr-alignment | P0 |

---

## 📚 附录：完整报告链接

### <a name="adr-reviewer-report"></a>A. ADR Reviewer 完整报告

详见 adr-reviewer agent 生成的完整审查报告，包括：
- 48 个 ADR 的详细对齐状态
- 每个 ADR 的问题清单和修复建议
- 按优先级排序的对齐计划
- 结构完整性检查结果

**关键数据**：
- 26 个已对齐，22 个需对齐
- 治理层：15/24 已对齐
- 宪法层：8/8 已对齐（格式调整中）
- 其他层：3/17 已对齐

---

### <a name="test-generator-report"></a>B. Test Generator 完整报告

详见 test-generator agent 生成的测试对齐验证报告，包括：
- 29 个测试文件的扫描结果
- 旧 RuleId 格式识别
- 测试绑定规则违规情况
- 详细的测试更新计划

**关键数据**：
- 4 个已对齐，25 个待对齐
- 估计 100+ 失败消息需要更新
- 估计 200+ 测试方法需要重命名

---

### <a name="doc-maintainer-report"></a>C. Documentation Maintainer 完整报告

详见 documentation-maintainer agent 生成的 instructions 同步分析报告，包括：
- `.github/instructions/` 文件分析
- copilot-instructions.md 更新建议
- 两层架构建议（YAML + Markdown）

**关键更新**：
- ✅ copilot-instructions.md 已全面更新
- ❓ 建议创建 `.github/agents/` 详细配置

---

### <a name="arch-guardian-report"></a>D. Architecture Guardian 完整报告

详见 architecture-guardian agent 生成的协调报告，包括：
- 5 阶段执行路线图
- Agent 协调与职责分工
- 风险评估与缓解措施
- 完整的检查清单

**关键策略**：
- 总工期：10-12 周
- 批次对齐：5-8 个 ADR/批
- 每批验证：100% 测试通过

---

## ✅ 签署与审批

### 执行计划制定者

- **Architecture Guardian**：2026-02-03
- **adr-reviewer**：2026-02-03
- **test-generator**：2026-02-03
- **documentation-maintainer**：2026-02-03

### 审批记录

- [x] **Architecture Board**（@douhuaa）：已批准 - 日期：2026-02-03
  - 决策 1：同意 - 不允许保留旧测试作为兼容层
  - 决策 2：批准 - 每批对齐 5 个 ADR
  - 决策 3：同意 - 对齐期间新 ADR 必须使用新格式
  - 决策 4：批准 - ADR-930 先补充完整再对齐
  - 决策 5：同意 - 使用 Feature Branch
  - 决策后行动：记录到 ADR-907-A，更新本执行计划（已完成）

### 审批状态

✅ **已批准** - 所有关键决策已获 Architecture Board 批准，可立即开始执行

---

## 📞 联系方式

**问题反馈**：
- Architecture Board：[架构委员会邮箱]
- Guardian 协调人：[待指定]

**进度跟踪**：
- GitHub Project Board：[待创建]
- 对齐清单：`docs/adr/governance/adr-907-a-alignment-checklist.md`

---

**文档版本**：1.1  
**制定日期**：2026-02-03  
**审批日期**：2026-02-03  
**最后更新**：2026-02-03  
**执行状态**：✅ 已批准，Ready to Execute  
**下次审查**：阶段 1 完成后
