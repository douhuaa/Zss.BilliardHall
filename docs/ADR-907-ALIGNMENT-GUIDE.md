# ADR-907 v2.0 对齐指南

> ⚠️ **已废弃 - 请参阅 [ADR-907-A：ADR-907 对齐执行标准](./adr/governance/adr-907-a-adr-alignment-execution-standard.md)**
> 
> 本文档已升级为正式 ADR（ADR-907-A），具备执行力和法律地位。
> 
> - **权威文档**：[ADR-907-A](./adr/governance/adr-907-a-adr-alignment-execution-standard.md)
> - **本文档状态**：仅作为历史参考，不具备裁决力
> - **升级日期**：2026-02-02

---

> **任务背景**: 根据 ADR-907 v2.0（2026-02-02）的要求，将所有 ADR 文档对齐到 Rule/Clause 双层编号体系。

## 已完成的工作

### ✅ Phase 1 - 部分完成
- [x] **ADR-900**：架构测试与 CI 治理元规则
  - 已对齐到 4 个 Rule，7 个 Clause
  - 清理了重复的 Decision 章节
  - 版本号更新到 4.0
  - 已提交：commit 39dae50

- [ ] **ADR-901**：语义元规则（进行中）
  - Front Matter 已更新
  - Decision 章节对齐方案已准备
  - 需要应用变更并完成

## 对齐标准（参考 ADR-907）

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

---
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

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| X.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 |
| ... | ... | ... |
```

## 待对齐的 ADR 清单

### Phase 1：治理层 ADR（高优先级）

| ADR | 状态 | 规则数量 | 预估难度 | 备注 |
|-----|------|---------|---------|------|
| ADR-900 | ✅ 已完成 | 4 Rule, 7 Clause | ⭐⭐⭐ | - |
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
| ADR-001 | ⏸️ 待对齐 | ⭐⭐⭐ |
| ADR-002 | ⏸️ 待对齐 | ⭐⭐⭐ |
| ADR-003 | ⏸️ 待对齐 | ⭐⭐⭐ |
| ADR-004 | ⏸️ 待对齐 | ⭐⭐⭐ |
| ADR-005 | ⏸️ 待对齐 | ⭐⭐⭐ |
| ADR-006 | ⏸️ 待对齐 | ⭐⭐ |
| ADR-007 | ⏸️ 待对齐 | ⭐⭐ |
| ADR-008 | ⏸️ 待对齐 | ⭐⭐ |

### Phase 3：运行层、结构层、技术层 ADR（中优先级）

| ADR 范围 | 数量 | 状态 |
|---------|------|------|
| ADR-120 ~ ADR-124 | 5 个 | ⏸️ 待对齐 |
| ADR-201 ~ ADR-240 | 4 个 | ⏸️ 待对齐 |
| ADR-301 ~ ADR-360 | 4 个 | ⏸️ 待对齐 |

## 对齐步骤（每个 ADR）

1. **查看原文件**
   ```bash
   view <ADR文件路径>
   ```

2. **更新 Front Matter**
   - version: 主版本号 +1
   - date: 2026-02-03

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

6. **提交变更**
   ```
   使用 report_progress 提交
   ```

## 智能分组策略

### 原则
- 相关的规则分组为一个 Rule
- 每个 Rule 应该有清晰的主题
- 每个 Clause 应该是一个独立的可测试规则

### 示例（ADR-900）

**旧规则**：
- ADR-900.1: 审判权唯一性
- ADR-900.2: 架构违规的判定原则
- ADR-900.3: 执行级别分离原则
- ADR-900.4: ADR ↔ 测试 ↔ CI 的一一映射
- ADR-900.5: 破例治理宪法规则
- ADR-900.6: 冲突裁决优先级

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

## 注意事项

1. **保持内容完整性**：只改变编号格式和结构，不改变规则内容
2. **一致性**：所有 ADR 使用相同的格式和风格
3. **可追溯性**：确保新旧编号的对应关系清晰
4. **测试对齐**：ADR 对齐后，对应的架构测试也需要更新
5. **文档交叉引用**：其他文档中引用旧编号的地方需要更新

## 验证清单

每个 ADR 对齐后需要检查：

- [ ] Front Matter 的 version 和 date 已更新
- [ ] Decision 章节使用 Rule/Clause 双层结构
- [ ] 所有规则编号使用 `ADR-XXX_<Rule>_<Clause>` 格式
- [ ] Enforcement 章节包含完整的规则编号映射表
- [ ] History 章节记录了此次对齐
- [ ] 没有遗漏任何原有的规则
- [ ] 文档结构完整，没有语法错误

## 后续工作

1. **完成所有 ADR 对齐**（Phase 1-3）
2. **更新架构测试**：将测试类和方法名更新为新格式
3. **更新 Copilot Prompts**：更新所有引用旧编号的地方
4. **更新文档交叉引用**：更新其他文档中的 ADR 引用
5. **CI 验证**：运行架构测试确保一切正常

## 工具支持

可以考虑编写脚本来辅助对齐工作：

```bash
# 示例：批量更新编号格式
# 将 ADR-XXX.Y:LZ 转换为 ADR-XXX_Y_Z
find docs/adr -name "*.md" -exec sed -i 's/ADR-\([0-9]\+\)\.\([0-9]\+\):L\([0-9]\+\)/ADR-\1_\2_\3/g' {} \;
```

**注意**：实际使用前需要仔细测试和验证。

---

**维护者**: Architecture Board  
**最后更新**: 2026-02-03  
**状态**: 进行中
