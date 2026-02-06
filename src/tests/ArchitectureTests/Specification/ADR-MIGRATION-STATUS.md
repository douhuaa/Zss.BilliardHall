# ADR 到 ArchitectureTestSpecification 迁移状态

> **迁移日期**：2026-02-06  
> **状态**：🚧 进行中  
> **完成度**：7/47 ADRs (15%)

---

## 📋 迁移概览

本文档跟踪将 ADR 文档规则迁移到 `ArchitectureTestSpecification.ArchitectureRules` 的进度。

### 迁移目标

将所有 ADR 文档中的 Rule/Clause 定义转换为强类型的 `ArchitectureRuleSet`，实现：

1. **唯一规范源**：代码即规范，消除文档与测试的不一致
2. **强类型安全**：编译时检查，防止 RuleId 引用错误
3. **易于维护**：集中管理，单点更新
4. **自动验证**：通过测试确保规则定义的正确性

---

## ✅ 已完成的 ADR (7/47)

### 宪法层 (Constitutional) - 3/8

| ADR | 标题 | Rules | Clauses | 状态 |
|-----|------|-------|---------|------|
| **ADR-001** | 模块化单体与垂直切片架构 | 3 | 7 | ✅ 已完成 |
| **ADR-002** | Platform/Application/Host 启动引导 | 2 | 4 | ✅ 已完成 |
| **ADR-003** | 命名空间规则 | 2 | 4 | ✅ 已完成 |
| ADR-004 | 中央包管理 (CPM) 规范 | - | - | ⏸️ 待迁移 |
| ADR-005 | 应用交互模型 | - | - | ⏸️ 待迁移 |
| ADR-006 | 术语与编号宪法 | - | - | ⏸️ 待迁移 |
| ADR-007 | Agent 行为与权限宪法 | - | - | ⏸️ 待迁移 |
| ADR-008 | 文档治理宪法 | - | - | ⏸️ 待迁移 |

### 治理层 (Governance) - 2/22

| ADR | 标题 | Rules | Clauses | 状态 |
|-----|------|-------|---------|------|
| **ADR-900** | 架构测试与 CI 治理元规则 | 4 | 7 | ✅ 已完成 |
| ADR-901 | 警告约束语义 | - | - | ⏸️ 待迁移 |
| ADR-902 | ADR 文档质量规范 | - | - | ⏸️ 待迁移 |
| ADR-905 | 执法级别分类 | - | - | ⏸️ 待迁移 |
| **ADR-907** | ArchitectureTests 执法治理体系 | 4 | 7 | ✅ 已完成 |
| ADR-907-A | ADR-907 对齐检查清单 | - | - | ⏸️ 待迁移 |
| ADR-910 | README 治理宪法 | - | - | ⏸️ 待迁移 |
| ADR-920 | 示例治理宪法 | - | - | ⏸️ 待迁移 |
| ADR-930 | 代码审查合规 | - | - | ⏸️ 待迁移 |
| ADR-940 | ADR 关系可追溯性管理 | - | - | ⏸️ 待迁移 |
| ADR-945 | ADR 时间线演进视图 | - | - | ⏸️ 待迁移 |
| ADR-946 | ADR 标题级别语义约束 | - | - | ⏸️ 待迁移 |
| ADR-947 | 关系章节结构解析安全 | - | - | ⏸️ 待迁移 |
| ADR-950 | 指南与 FAQ 文档治理 | - | - | ⏸️ 待迁移 |
| ADR-951 | 案例库管理 | - | - | ⏸️ 待迁移 |
| ADR-952 | 工程标准 ADR 边界 | - | - | ⏸️ 待迁移 |
| ADR-955 | 文档搜索与可发现性 | - | - | ⏸️ 待迁移 |
| ADR-960 | Onboarding 文档治理 | - | - | ⏸️ 待迁移 |
| ADR-965 | Onboarding 互动式学习路径 | - | - | ⏸️ 待迁移 |
| ADR-970 | 自动化日志集成标准 | - | - | ⏸️ 待迁移 |
| ADR-975 | 文档质量监控 | - | - | ⏸️ 待迁移 |
| ADR-980 | ADR 生命周期同步 | - | - | ⏸️ 待迁移 |
| ADR-990 | 文档演进路线图 | - | - | ⏸️ 待迁移 |

### 运行时 (Runtime) - 1/5

| ADR | 标题 | Rules | Clauses | 状态 |
|-----|------|-------|---------|------|
| **ADR-201** | Handler 生命周期管理 | 1 | 2 | ✅ 已完成 |
| ADR-210 | 事件版本兼容性 | - | - | ⏸️ 待迁移 |
| ADR-220 | 事件总线集成 | - | - | ⏸️ 待迁移 |
| ADR-240 | Handler 异常约束 | - | - | ⏸️ 待迁移 |

### 结构层 (Structure) - 1/5

| ADR | 标题 | Rules | Clauses | 状态 |
|-----|------|-------|---------|------|
| **ADR-120** | 领域事件命名规范 | 3 | 4 | ✅ 已完成 |
| ADR-121 | 契约 DTO 命名组织 | - | - | ⏸️ 待迁移 |
| ADR-122 | 测试组织命名 | - | - | ⏸️ 待迁移 |
| ADR-123 | 仓储接口分层 | - | - | ⏸️ 待迁移 |
| ADR-124 | 端点命名约束 | - | - | ⏸️ 待迁移 |

### 技术层 (Technical) - 0/4

| ADR | 标题 | Rules | Clauses | 状态 |
|-----|------|-------|---------|------|
| ADR-301 | 集成测试自动化 | - | - | ⏸️ 待迁移 |
| ADR-340 | 结构化日志监控约束 | - | - | ⏸️ 待迁移 |
| ADR-350 | 日志与可观测性标准 | - | - | ⏸️ 待迁移 |
| ADR-360 | CI/CD 流水线标准化 | - | - | ⏸️ 待迁移 |

---

## 📊 统计摘要

| 层级 | 总数 | 已完成 | 待迁移 | 完成率 |
|------|------|--------|--------|--------|
| 宪法层 (Constitutional) | 8 | 3 | 5 | 38% |
| 治理层 (Governance) | 22 | 2 | 20 | 9% |
| 运行时 (Runtime) | 5 | 1 | 4 | 20% |
| 结构层 (Structure) | 5 | 1 | 4 | 20% |
| 技术层 (Technical) | 4 | 0 | 4 | 0% |
| **总计** | **47** | **7** | **40** | **15%** |

**已定义规则统计**：
- 总 Rules: 19
- 总 Clauses: 35
- 平均每个 ADR: 2.7 Rules, 5.0 Clauses

---

## 🎯 迁移计划

### Phase 1: 核心宪法层 (优先级：最高) ✅ 50% 完成

**目标**：完成核心架构约束的规则集定义

- [x] ADR-001: 模块化单体与垂直切片架构
- [x] ADR-002: Platform/Application/Host 启动引导
- [x] ADR-003: 命名空间规则
- [ ] ADR-004: 中央包管理 (CPM) 规范
- [ ] ADR-005: 应用交互模型

### Phase 2: 治理基础 (优先级：高) ✅ 33% 完成

**目标**：建立治理和测试规范的规则集

- [x] ADR-900: 架构测试与 CI 治理元规则
- [x] ADR-907: ArchitectureTests 执法治理体系
- [ ] ADR-902: ADR 文档质量规范
- [ ] ADR-905: 执法级别分类
- [ ] ADR-910: README 治理宪法
- [ ] ADR-920: 示例治理宪法

### Phase 3: 结构与运行时 (优先级：中) ✅ 17% 完成

**目标**：完成常用的结构和运行时约束

- [x] ADR-120: 领域事件命名规范
- [x] ADR-201: Handler 生命周期管理
- [ ] ADR-121: 契约 DTO 命名组织
- [ ] ADR-122: 测试组织命名
- [ ] ADR-123: 仓储接口分层
- [ ] ADR-124: 端点命名约束
- [ ] ADR-210: 事件版本兼容性
- [ ] ADR-220: 事件总线集成
- [ ] ADR-240: Handler 异常约束

### Phase 4: 扩展治理 (优先级：低) ⏸️ 未开始

**目标**：完成剩余的治理和文档规范

- [ ] ADR-930 ~ ADR-990: 其他治理层 ADR (13 个)
- [ ] ADR-006 ~ ADR-008: 剩余宪法层 ADR (3 个)

### Phase 5: 技术层 (优先级：低) ⏸️ 未开始

**目标**：完成技术实现细节的规范

- [ ] ADR-301: 集成测试自动化
- [ ] ADR-340: 结构化日志监控约束
- [ ] ADR-350: 日志与可观测性标准
- [ ] ADR-360: CI/CD 流水线标准化

---

## 🔄 迁移流程

### 对于每个 ADR，执行以下步骤：

1. **阅读 ADR 文档**
   - 打开对应的 ADR Markdown 文件
   - 识别所有 Rule 和 Clause
   - 记录 RuleId、标题、条件、执行要求

2. **在 _ArchitectureRules.cs 中添加规则集**
   ```csharp
   public static ArchitectureRuleSet AdrXXX => LazyAdrXXX.Value;
   
   private static readonly Lazy<ArchitectureRuleSet> LazyAdrXXX = new(() =>
   {
       var ruleSet = new ArchitectureRuleSet(XXX);
       
       // 添加 Rules 和 Clauses
       // ...
       
       return ruleSet;
   });
   ```

3. **更新辅助方法**
   - 在 `GetRuleSet` 的 switch 语句中添加新 case
   - 在 `GetAllRuleSets` 中添加新的 yield return
   - 在 `GetAllAdrNumbers` 中添加新的 yield return

4. **创建测试**
   - 在 `ArchitectureRulesTests.cs` 中添加针对新 ADR 的测试
   - 验证规则数量、条款数量、规则内容

5. **验证**
   - 运行测试确保所有测试通过
   - 检查规则定义与 ADR 文档一致

6. **更新本文档**
   - 将对应 ADR 的状态从"待迁移"改为"已完成"
   - 更新统计数据

---

## 🔧 维护指南

### 当 ADR 文档更新时

1. **识别变更**
   - 检查 ADR 文档的 git diff
   - 确定是新增、修改还是删除规则

2. **同步到 ArchitectureRules**
   - 更新对应的规则集定义
   - 保持 RuleId、描述等信息一致

3. **运行测试**
   - 运行 `ArchitectureRulesTests` 验证更改
   - 运行相关的架构测试确保没有破坏

4. **提交变更**
   - 一起提交 ADR 文档和 ArchitectureRules 的变更
   - 在提交消息中说明同步的原因

### 质量检查清单

- [ ] 所有 RuleId 格式正确（ADR-{编号}_{Rule}_{Clause}）
- [ ] 规则摘要与 ADR 文档一致
- [ ] 条款条件和执行要求与 ADR 文档一致
- [ ] Severity 和 Scope 设置正确
- [ ] 所有测试通过
- [ ] 文档已更新

---

## 📚 相关文档

- [ArchitectureRules 使用指南](./ARCHITECTURE-RULES-USAGE.md)
- [ArchitectureRuleSet API](./Rules/ArchitectureRuleSet.cs)
- [ArchitectureRuleId API](./Rules/ArchitectureRuleId.cs)
- [ADR 文档目录](../../../docs/adr/)

---

## 🚀 下一步行动

### 立即行动（本周）

1. ✅ 完成 Phase 1 剩余的 ADR-004、ADR-005
2. ✅ 完成 Phase 2 的 ADR-902、ADR-905
3. ✅ 开始更新现有测试使用 ArchitectureRules

### 短期目标（本月）

1. ✅ 完成 Phase 3 所有结构和运行时 ADR
2. ✅ 至少 50% 的架构测试使用 ArchitectureRules
3. ✅ 编写自动化脚本验证 ADR 与 ArchitectureRules 的一致性

### 长期目标（本季度）

1. ✅ 完成所有 47 个 ADR 的迁移
2. ✅ 100% 的架构测试使用 ArchitectureRules
3. ✅ 实现 ADR 文档 ↔ ArchitectureRules 的双向同步工具

---

**最后更新**：2026-02-06  
**维护者**：Architecture Board
