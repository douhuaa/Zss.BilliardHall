# ADR 测试覆盖率扫描 - 快速上手指南

> 5 分钟快速了解 ADR 测试缺失问题和解决方案

---

## 🎯 问题是什么？

我们有 **45 个 ADR 文档**，但只有 **26 个架构测试**，测试覆盖率仅 **24%**。

这意味着：
- ❌ 很多架构约束无法自动验证
- ❌ 违规代码可能进入生产环境
- ❌ 依赖人工审查，效率低且容易遗漏

---

## 📊 现状一览

| 层级 | ADR 数 | 已测试 | 覆盖率 | 状态 |
|------|--------|--------|--------|------|
| Constitutional（宪法层） | 8 | 8 | 100% | ✅ 完全覆盖 |
| Governance（治理层） | 25 | 3 | 12% | ❌ 严重不足 |
| Structure（结构层） | 5 | 0 | 0% | ❌ 完全缺失 |
| Runtime（运行层） | 4 | 0 | 0% | ❌ 完全缺失 |
| Technical（技术层） | 4 | 0 | 0% | ❌ 完全缺失 |

---

## 🚨 最紧急的问题

### Runtime 层（运行层）完全缺失

4 个标注【必须架构测试覆盖】的 ADR 没有测试：

1. **ADR-201**: Handler 生命周期管理
   - 约束：Handler 必须使用 Scoped 生命周期
   
2. **ADR-210**: 事件版本化兼容性
   - 约束：事件必须向后兼容
   
3. **ADR-220**: 事件总线集成
   - 约束：必须使用 Outbox Pattern
   
4. **ADR-240**: Handler 异常约束
   - 约束：Handler 异常必须结构化

### Structure 层（结构层）关键缺失

1. **ADR-121**: Contract DTO 命名组织
   - 约束：Contract DTO 命名必须符合规范

---

## 🔧 如何检查？

### 快速检查

```bash
cd /path/to/Zss.BilliardHall
./scripts/check-adr-test-coverage.sh
```

### 输出示例

```
======================================
  ADR 测试覆盖率检查
======================================

📊 ADR 文档总数:    45
📊 架构测试总数:    26

======================================
  按层级统计
======================================

📁 runtime:
   - ADR 总数: 4
   - 已测试: 0
   - 覆盖率: 0%
   - 状态: ❌ 完全缺失

======================================
  缺失测试详细列表
======================================

📁 runtime 层（缺失 4 个）:

   ❌ ADR-201: Handler 生命周期管理
      - 标注必须测试: ✅
      - 优先级: 🔴 P0
      - 期望文件: src/tests/ArchitectureTests/ADR/ADR_0201_Architecture_Tests.cs
```

---

## 📋 解决方案

### 5 阶段修复计划（3 个月）

| 阶段 | 时间 | 内容 | 工作量 |
|-----|------|------|--------|
| Phase 1 | Week 1-2 | Runtime 层（4 个测试） | 6.5 天 |
| Phase 2 | Week 2-3 | Structure 层（5 个测试） | 5 天 |
| Phase 3 | Week 3-4 | Governance P0/P1（9 个测试） | 11 天 |
| Phase 4 | Month 2 | Technical 层（4 个测试） | 7 天 |
| Phase 5 | Month 2-3 | Governance P2/P3（13 个测试） | 15 天 |

**总工作量**: 44.5 人天  
**目标覆盖率**: ≥ 80%

---

## 🎯 立即行动

### 如果你是开发者

1. **查看详细分析**
   ```bash
   cat docs/reports/adr-test-gap-analysis-2026-01-29.md
   ```

2. **了解测试模板**
   ```bash
   cat src/tests/ArchitectureTests/ADR/ADR_0001_Architecture_Tests.cs
   ```

3. **认领任务**
   - 联系团队负责人
   - 选择一个 Phase 1 或 Phase 2 的任务

### 如果你是团队负责人

1. **审阅完整计划**
   - 📘 [详细分析报告](adr-test-gap-analysis-2026-01-29.md)（480 行）
   - 📘 [执行摘要](adr-test-gap-summary.md)（160 行）

2. **分配资源**
   - Backend Team: Phase 1, 2, 4
   - Testing Team: Phase 3, 4
   - Doc Team: Phase 3, 5

3. **设置里程碑**
   - Week 2: Phase 1 完成
   - Week 3: Phase 2 完成
   - Week 4: Phase 3 完成

---

## 📚 相关文档

### 核心文档

| 文档 | 用途 | 篇幅 |
|-----|------|------|
| [详细分析报告](adr-test-gap-analysis-2026-01-29.md) | 完整分析、修复计划、测试指南 | 480 行 |
| [执行摘要](adr-test-gap-summary.md) | 高层视角、关键发现、行动建议 | 160 行 |

### 工具脚本

| 脚本 | 用途 |
|-----|------|
| `check-adr-test-coverage.sh` | 自动检查测试覆盖率 |
| `check-adr-consistency.sh` | 检查 ADR 格式一致性 |
| `validate-adr-relationships.py` | 验证 ADR 关系一致性 |

### 相关 ADR

| ADR | 标题 |
|-----|------|
| [ADR-0000](../adr/governance/ADR-0000-architecture-tests.md) | 架构测试与 CI 治理宪法 |
| [ADR-904](../adr/governance/ADR-904-architecturetests-minimum-assertion-semantics.md) | 架构测试最小断言语义 |
| [ADR-930](../adr/governance/ADR-930-code-review-compliance.md) | 代码审查合规性 |

---

## ❓ 常见问题

### Q: 为什么宪法层 100% 覆盖，其他层缺失？

**A**: 宪法层是最早建立测试的，后续新增 ADR 时未同步补充测试。

### Q: 必须所有 ADR 都有测试吗？

**A**: 不一定。标注【必须架构测试覆盖】的必须有测试，其他可根据可自动化程度决定。

### Q: 如何知道某个 ADR 是否需要测试？

**A**: 
1. 查看 ADR 中是否有【必须架构测试覆盖】标注
2. 如果约束可自动化验证（命名、结构等），建议补充测试
3. 如果是流程类、建议类，可转为人工审查

### Q: 测试失败了怎么办？

**A**: 
1. 先检查是否真的违反了 ADR 约束
2. 如果代码违规，修复代码
3. 如果是历史遗留，记录破例并制定偿还计划
4. 不要修改测试来"让代码通过"

### Q: 工作量太大怎么办？

**A**: 
1. 分阶段实施，优先补充 P0 级（Runtime、Structure 关键 ADR）
2. 部分难以自动化的可转为人工审查
3. 允许暂时破例，但必须有偿还计划

---

## 📞 联系与反馈

**责任团队**: 架构委员会 / Testing Team  
**反馈渠道**: GitHub Issues (label: `adr-test-coverage`)  
**紧急联系**: @douhuaa

---

## ✅ 检查清单

在开始工作前，请确认：

- [ ] 已阅读执行摘要，了解整体情况
- [ ] 已运行 `check-adr-test-coverage.sh` 查看详细缺失
- [ ] 已查看测试模板，了解测试结构
- [ ] 已认领具体任务，明确负责的 ADR
- [ ] 已了解测试开发指南（详细分析报告 §4）

---

**版本**: 1.0  
**最后更新**: 2026-01-29  
**状态**: ✅ Active
