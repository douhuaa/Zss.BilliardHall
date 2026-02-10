# 强类型规约系统与 Markdown 系统整合分析

**日期**: 2026-02-10  
**状态**: 分析完成

## 📊 执行摘要

### 核心发现

当前系统是**混合架构治理体系**，RuleSet 和 ADR **并存且互补**：

1. ✅ RuleSet 覆盖率 93.5%（43/46个ADR）
2. ✅ 测试体系完善（321个测试）
3. ⚠️ Agent/Skills 未充分利用 RuleSet
4. ⚠️ 缺少自动同步机制

### 建议策略：重新定位而非替换

```
RuleSet（唯一真相源） → 可执行规范
    ↓
    ├─→ ADR 文档（派生）
    ├─→ 测试生成（自动）
    ├─→ Agent 指令（基于API）
    └─→ Analyzer（未来）
```

## 📈 当前状态

### 1. ADR 系统：46个文档

| 层级 | 范围 | 数量 |
|------|------|------|
| Constitutional | 001-009 | 9 |
| Structure | 120-129 | 5 |
| Runtime | 201-240 | 4 |
| Technical | 301-360 | 4 |
| Governance | 900-999 | 24 |

### 2. RuleSet 系统：43个（93.5%覆盖）

**缺失**：
- ADR-009（草稿状态）
- ADR-904/906（已被907取代）

**实际覆盖率 ≈ 100%**

### 3. 测试体系：321个测试

| 类型 | 数量 | 说明 |
|------|------|------|
| Specification 基础设施 | 270+ | 完整 |
| ADR-907 执法测试 | 40+ | 完整 |
| ADR-001 业务测试 | 5 | 示例 |
| 其他 ADR | 0 | **待生成** |

**机会**：可生成 200-300 个业务规则测试

### 4. Agent 系统：9个

**问题**：主要通过读取 ADR 文档，未使用 RuleSet API

### 5. Skills 系统：9个

**机会**：
- `generate-test` 应基于 RuleSet
- `generate-adr` 应实现反向生成
- 代码生成应查询 RuleSet 确保合规

## 🎯 核心问题

### 问题1：Agent 未使用 RuleSet API

**当前**：
```yaml
Agent → 读 ADR Markdown → 理解语义 → 生成代码
```

**期望**：
```csharp
var ruleSet = RuleSetRegistry.Get(1);
var clause = ruleSet.GetClause(1, 1);
// 使用结构化数据
```

### 问题2：缺少双向同步

- RuleSet 手工编写 ✍️
- ADR 手工编写 ✍️
- 可能不一致 ⚠️

**需要**：
- RuleSet → ADR 自动生成
- CI 验证同步

### 问题3：测试未自动化

- 43个RuleSet，仅1个有完整测试
- 需要生成 200-300 个测试

## 💡 解决方案

### 7阶段实施路径

#### 阶段1：补充基础（1-2天）
- 验证 RuleSet 完整性
- 增强 RuleSetRegistry API

#### 阶段2：构建生成工具（2-3天）
- RuleSet → ADR 生成器
- RuleSet → 测试生成器
- RuleSet → Instructions 生成器

#### 阶段3：更新 Agent/Skills（3-4天）
- 修改 instructions 引用 RuleSet API
- 提供使用示例
- 更新失败信息格式

#### 阶段4：生成测试套件（3-4天）
- 为42个RuleSet生成测试
- 预计 200-300 个测试方法

#### 阶段5：重新生成文档（2-3天）
- 保留 Context/Consequences
- 从 RuleSet 生成 Decision 章节

#### 阶段6：CI/CD 集成（2天）
- RuleSet ↔ ADR 一致性检查
- 自动触发生成

#### 阶段7：验证清理（2-3天）
- 完整测试
- 性能优化

**总计**: 15-22 工作日

## 📊 成本效益

### 收益

**短期**：
- 测试覆盖 20 → 200+
- Agent 准确性提升
- 文档自动同步

**长期**：
- 单一真相源
- 支持 Analyzer/Generator
- 降低维护成本

### 风险缓解

1. **语义丢失**：保留 ADR 补充章节
2. **Agent 回归**：分阶段更新，保留回滚
3. **测试质量**：生成框架，人工实现
4. **性能影响**：懒加载，并行执行

## 🎯 推荐行动

### P0（立即）
1. ✅ 完成分析
2. 🔲 决策会议
3. 🔲 确定策略

### P1（核心）
1. 🔲 实现生成工具
2. 🔲 更新 test-generator
3. 🔲 生成核心测试

### P2（推广）
1. 🔲 更新所有 Agent
2. 🔲 完整测试套件
3. 🔲 重新生成文档

## 📚 附录

### RuleSet 清单（43个）

**宪法层（8）**：001-008  
**结构层（5）**：120-124  
**运行时（4）**：201, 210, 220, 240  
**技术层（4）**：301, 340, 350, 360  
**治理层（22）**：900-990系列

### 关键决策

**Q: 为什么重新定位而非替换？**
- ADR 包含无法结构化的信息（Context/Rationale）
- 两者互补，共同构成完整体系

**Q: 为什么 RuleSet 是唯一真相源？**
- 强类型保证一致性
- API 可查询
- 可自动生成派生产物

---

## 相关文档

- [Specification README](../../src/tests/ArchitectureTests/Specification/README.md)
- [ADR-007: Agent 行为宪法](../adr/constitutional/ADR-007-agent-behavior-permissions-constitution.md)
- [ADR-907: 测试执法体系](../adr/governance/ADR-907-architecture-tests-enforcement-governance.md)
- [Agent-Skills 映射](../../.github/AGENT-SKILLS-MAPPING.md)
