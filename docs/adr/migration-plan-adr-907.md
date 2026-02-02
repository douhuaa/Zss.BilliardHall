# ADR-907 v2.0 整改计划

**版本**：1.0  
**日期**：2026-02-02  
**状态**：In Progress

---

## 背景

ADR-907 v2.0 引入了 Rule/Clause 双层编号体系，要求所有 ArchitectureTests 严格遵循新的命名和组织规范。本文档记录现有 ADR 的整改计划。

---

## 整改原则

1. **不向后兼容**：完全按 ADR-907 v2.0 新规范重写
2. **Rule/Clause 映射**：每个 Clause 对应一个测试方法
3. **测试命名规范**：
   - 方法名格式：`ADR_<编号>_<Rule>_<Clause>_<描述>`
   - DisplayName 格式：`ADR-<编号>_<Rule>_<Clause>: <描述>`
4. **失败消息规范**：包含 ADR 编号、Rule/Clause、违规描述、修复建议、文档引用

---

## ADR-907 测试执行结果

### 测试统计

- **总测试数**：21 个（对应 21 个 Clause）
- **通过测试**：14 个 ✅
- **失败测试**：7 个 ⚠️

### 失败测试分析

#### 1. ADR-907_1_2：具备裁决力的 ADR 必须有测试或声明 Non-Enforceable

**问题 ADR**：
- ADR-0905 (enforcement-level-classification.md) - 缺少架构测试且未声明 Non-Enforceable
- ADR-0901 (warning-constraint-semantics.md) - 缺少架构测试且未声明 Non-Enforceable

**整改方案**：
- [ ] 为 ADR-0905 创建架构测试 `ADR_0905_Architecture_Tests.cs`
- [ ] 为 ADR-0901 创建架构测试 `ADR_0901_Architecture_Tests.cs`
- [ ] 或在这些 ADR 的元数据中添加 `enforceable: false` 声明

#### 2. ADR-907_1_3：禁止仅文档约束的架构规则

**检测到的问题模式**：
- 部分 ADR 可能包含"仅文档约束"、"不接受执行"等禁止声明
- 需要人工审查所有 Final/Active ADR

**整改方案**：
- [ ] 审查所有 Final/Active ADR 文档
- [ ] 移除任何"仅文档约束"声明
- [ ] 确保所有架构规则要么有测试，要么明确声明 Non-Enforceable

#### 3. ADR-907_2_6：失败信息必须包含 ADR 引用

**问题文件**：多个测试文件的失败消息格式不符合规范

**整改方案**：
- [ ] 更新所有测试文件的失败消息，确保包含：
  - ADR 编号（格式：ADR-XXXX）
  - 违规标记（❌）
  - 修复建议
  - 文档引用

#### 4. ADR-907_2_7：测试不得包含空弱断言

**问题文件**：部分测试包含空测试或 TODO 占位

**整改方案**：
- [ ] 审查所有测试文件
- [ ] 移除空测试方法
- [ ] 完善 TODO 占位测试
- [ ] 确保每个测试都有实际的架构约束验证

#### 5. ADR-907_3_3：失败信息必须可溯源到 ADR

**问题文件**：大量测试文件的失败消息不完整

**问题 ADR**：
- ADR_0000_Architecture_Tests.cs
- ADR_0003_Architecture_Tests.cs
- ADR_0201_Architecture_Tests.cs
- ADR_0340_Architecture_Tests.cs
- ADR_0360_Architecture_Tests.cs

**整改方案**：
- [ ] 更新失败消息格式，确保包含：
  - ADR 编号和 Rule/Clause
  - 违规标记（❌）
  - 违规详情
  - 修复建议（具体步骤）
  - 文档引用（参考：docs/adr/...）

#### 6. ADR-907_3_4：禁止形式化断言

**问题文件**：
- ADR_920_Architecture_Tests.cs - 包含 `true.Should().BeTrue()`
- ADR_910_Architecture_Tests.cs - 包含 `true.Should().BeTrue()`

**整改方案**：
- [ ] 移除所有形式化断言：
  - `Assert.True(true)`
  - `Assert.False(false)`
  - `true.Should().BeTrue()`
  - `1.Should().Be(1)`
- [ ] 替换为实际的结构约束验证

#### 7. ADR-907_4_2：测试失败必须映射到 RuleId

**问题文件**：大量测试文件缺少 RuleId 映射

**整改方案**：
- [ ] 更新所有测试的 DisplayName，使用格式：
  - `ADR-<编号>_<Rule>_<Clause>: <描述>`
- [ ] 更新失败消息，使用格式：
  - `❌ ADR-<编号>_<Rule>_<Clause> 违规：...`
- [ ] 确保每个测试方法映射到唯一的 Clause

---

## 整改优先级

### P0（高优先级 - 立即修复）

1. **ADR_920 和 ADR_910 的形式化断言** - 违反 L1 规则，必须立即修复
2. **缺少测试的 ADR（ADR-0905, ADR-0901）** - 必须创建测试或声明 Non-Enforceable

### P1（中优先级 - 本周完成）

3. **更新 DisplayName 和失败消息格式** - 所有测试文件
4. **移除空弱断言和 TODO 占位** - 确保测试质量

### P2（低优先级 - 逐步完善）

5. **审查所有 ADR 文档** - 移除"仅文档约束"声明
6. **完善测试覆盖** - 确保所有 Final/Active ADR 都有测试

---

## 整改进度跟踪

### 第一阶段：ADR-907 测试重构 ✅

- [x] 创建 ADR_907_1_Architecture_Tests.cs
- [x] 创建 ADR_907_2_Architecture_Tests.cs
- [x] 创建 ADR_907_3_Architecture_Tests.cs
- [x] 创建 ADR_907_4_Architecture_Tests.cs
- [x] 删除旧的单文件测试
- [x] 验证测试构建和运行

### 第二阶段：现有测试整改（待完成）

- [ ] 修复 ADR_920 和 ADR_910 的形式化断言
- [ ] 为 ADR-0905 和 ADR-0901 创建测试
- [ ] 更新所有测试的 DisplayName 格式
- [ ] 更新所有测试的失败消息格式
- [ ] 移除空弱断言和 TODO 占位

### 第三阶段：ADR 文档审查（待完成）

- [ ] 审查所有 Final/Active ADR
- [ ] 移除"仅文档约束"声明
- [ ] 确保所有 ADR 要么有测试，要么声明 Non-Enforceable

---

## 参考资源

- [ADR-907 ArchitectureTests 执法治理体系](./adr/governance/ADR-907-architecture-tests-enforcement-governance.md)
- [ADR-907 Copilot 提示词](./copilot/adr-0907.prompts.md)
- [ADR-0000 架构测试宪法](./adr/governance/ADR-0000-architecture-tests.md)

---

## 备注

本整改计划基于 2026-02-02 的 ADR-907 测试执行结果生成。随着整改进度推进，本文档将持续更新。
