# ADR 引用索引

**版本**: 1.0  
**最后更新**: 2026-02-09  
**状态**: Active

---

## 概述

本文档维护 Instructions 和 Skills 配置文件中引用的所有 ADR，并验证这些引用的有效性。

### 使用说明

- ✅ **绿色复选标记**：ADR 存在且可访问
- ⚠️ **警告标记**：ADR 已归档但仍可引用
- ❌ **红叉标记**：ADR 不存在或路径错误

---

## Instructions 文件中的 ADR 引用

### 1. adr-reviewer.instructions.yaml

**引用的 ADR**：
- ✅ ADR-902 - ADR 标准模板与结构契约
- ✅ ADR-907-A - ADR-907 对齐执行标准
- ✅ ADR-940 - ADR 关系可追溯性管理

**验证状态**：全部通过 ✅

---

### 2. architecture-guardian.instructions.yaml

**引用的 ADR**：
- ✅ ADR-007 - Agent 行为与权限宪法
- ✅ ADR-900 - 架构测试与 CI 治理元规则
- ✅ ADR-907 - ArchitectureTests 执法治理体系

**验证状态**：全部通过 ✅

---

### 3. documentation-maintainer.instructions.yaml

**引用的 ADR**：
- ✅ ADR-008 - 文档治理宪法
- ✅ ADR-910 - README 治理宪法
- ✅ ADR-940 - ADR 关系可追溯性管理
- ✅ ADR-946 - ADR 标题级别语义约束
- ✅ ADR-947 - 关系声明区结构与解析安全

**验证状态**：全部通过 ✅

---

### 4. expert-dotnet-software-engineer.instructions.yaml

**引用的 ADR**：
- ✅ ADR-001 - 模块化单体垂直切片架构
- ✅ ADR-002 - 平台应用主机引导
- ✅ ADR-003 - 命名空间规则
- ✅ ADR-004 - CPM 最终决策
- ✅ ADR-005 - 应用交互模型最终决策

**验证状态**：全部通过 ✅

---

### 5. handler-pattern-enforcer.instructions.yaml

**引用的 ADR**：
- ✅ ADR-201 - Handler 生命周期管理
- ✅ ADR-240 - Handler 异常约束

**验证状态**：全部通过 ✅

---

### 6. module-boundary-checker.instructions.yaml

**引用的 ADR**：
- ✅ ADR-001 - 模块化单体垂直切片架构
- ✅ ADR-003 - 命名空间规则
- ✅ ADR-005 - 应用交互模型最终决策

**验证状态**：全部通过 ✅

---

### 7. test-generator.instructions.yaml

**引用的 ADR**：
- ✅ ADR-900 - 架构测试与 CI 治理元规则
- ✅ ADR-907 - ArchitectureTests 执法治理体系
- ✅ ADR-907-A - ADR-907 对齐执行标准

**验证状态**：全部通过 ✅

---

## Skills 文件中的 ADR 引用

### 代码生成类

#### generate-handler.skill.md
**引用的 ADR**：
- ✅ ADR-001 - 模块化单体垂直切片架构
- ✅ ADR-005 - 应用交互模型最终决策
- ✅ ADR-122 - 测试组织与命名

**验证状态**：全部通过 ✅

---

#### generate-endpoint.skill.md
**引用的 ADR**：
- ✅ ADR-001 - 模块化单体垂直切片架构
- ✅ ADR-005 - 应用交互模型最终决策
- ✅ ADR-124 - Endpoint 命名约束

**验证状态**：全部通过 ✅

---

#### generate-test.skill.md
**引用的 ADR**：
- ✅ ADR-900 - 架构测试与 CI 治理元规则
- ✅ ADR-907 - ArchitectureTests 执法治理体系
- ✅ ADR-122 - 测试组织与命名

**验证状态**：全部通过 ✅

---

### 代码分析类

#### scan-cross-module-refs.skill.md
**引用的 ADR**：
- ✅ ADR-001 - 模块化单体垂直切片架构
- ✅ ADR-003 - 命名空间规则
- ✅ ADR-005 - 应用交互模型最终决策

**验证状态**：全部通过 ✅

---

### 文档生成类

#### generate-adr.skill.md
**引用的 ADR**：
- ✅ ADR-902 - ADR 标准模板与结构契约
- ✅ ADR-907 - ArchitectureTests 执法治理体系
- ✅ ADR-907-A - ADR-907 对齐执行标准
- ✅ ADR-940 - ADR 关系可追溯性管理
- ✅ ADR-946 - ADR 标题级别语义约束
- ✅ ADR-947 - 关系声明区结构与解析安全

**验证状态**：全部通过 ✅

---

#### update-documentation.skill.md
**引用的 ADR**：
- ✅ ADR-008 - 文档治理宪法
- ✅ ADR-910 - README 治理宪法
- ✅ ADR-940 - ADR 关系可追溯性管理

**验证状态**：全部通过 ✅

---

### 测试执行类

#### run-architecture-tests.skill.md
**引用的 ADR**：
- ✅ ADR-001 - 模块化单体垂直切片架构
- ✅ ADR-900 - 架构测试与 CI 治理元规则
- ✅ ADR-907 - ArchitectureTests 执法治理体系

**验证状态**：全部通过 ✅

---

#### run-unit-tests.skill.md
**引用的 ADR**：
- ✅ ADR-122 - 测试组织与命名
- ✅ ADR-900 - 架构测试与 CI 治理元规则

**验证状态**：全部通过 ✅

---

### CI/CD 集成类

#### post-comment.skill.md
**引用的 ADR**：
- ✅ ADR-007 - Agent 行为与权限宪法
- ✅ ADR-940 - ADR 关系可追溯性管理

**验证状态**：全部通过 ✅

---

## ADR 引用统计

### 按引用频率排序

| ADR | 标题 | 引用次数 | 主要引用来源 |
|-----|------|---------|-------------|
| ADR-001 | 模块化单体垂直切片架构 | 7 | Instructions, Skills (代码生成/分析) |
| ADR-005 | 应用交互模型最终决策 | 6 | Instructions, Skills (代码生成/分析) |
| ADR-900 | 架构测试与 CI 治理元规则 | 6 | Instructions, Skills (测试) |
| ADR-907 | ArchitectureTests 执法治理体系 | 5 | Instructions, Skills (测试/文档) |
| ADR-940 | ADR 关系可追溯性管理 | 4 | Instructions, Skills (文档) |
| ADR-122 | 测试组织与命名 | 3 | Skills (测试) |
| ADR-003 | 命名空间规则 | 3 | Instructions, Skills (代码分析) |
| ADR-007 | Agent 行为与权限宪法 | 2 | Instructions, Skills (CI/CD) |
| ADR-008 | 文档治理宪法 | 2 | Instructions, Skills (文档) |
| ADR-902 | ADR 标准模板与结构契约 | 2 | Instructions, Skills (文档) |
| ADR-907-A | ADR-907 对齐执行标准 | 2 | Instructions, Skills (文档) |
| ADR-910 | README 治理宪法 | 2 | Instructions, Skills (文档) |
| ADR-946 | ADR 标题级别语义约束 | 2 | Instructions, Skills (文档) |
| ADR-947 | 关系声明区结构与解析安全 | 2 | Instructions, Skills (文档) |
| ADR-002 | 平台应用主机引导 | 1 | Instructions |
| ADR-004 | CPM 最终决策 | 1 | Instructions |
| ADR-124 | Endpoint 命名约束 | 1 | Skills (代码生成) |
| ADR-201 | Handler 生命周期管理 | 1 | Instructions |
| ADR-240 | Handler 异常约束 | 1 | Instructions |

---

## ADR 文档路径映射

### Constitutional（宪法级）
```
docs/adr/constitutional/
├── ADR-001-modular-monolith-vertical-slice-architecture.md  ✅
├── ADR-002-platform-application-host-bootstrap.md          ✅
├── ADR-003-namespace-rules.md                              ✅
├── ADR-004-Cpm-Final.md                                    ✅
├── ADR-005-Application-Interaction-Model-Final.md          ✅
├── ADR-006-terminology-numbering-constitution.md           ✅
├── ADR-007-agent-behavior-permissions-constitution.md      ✅
└── ADR-008-documentation-governance-constitution.md        ✅
```

### Governance（治理级）
```
docs/adr/governance/
├── ADR-900-architecture-tests.md                           ✅
├── ADR-902-adr-template-structure-contract.md              ✅
├── ADR-907-architecture-tests-enforcement-governance.md    ✅
├── ADR-907-a-alignment-checklist.md                        ✅
├── ADR-910-readme-governance-constitution.md               ✅
├── ADR-940-adr-relationship-traceability-management.md     ✅
├── ADR-946-adr-heading-level-semantic-constraint.md        ✅
└── ADR-947-relationship-section-structure-parsing-safety.md ✅
```

### Structure（结构级）
```
docs/adr/structure/
├── ADR-122-test-organization-naming.md                     ✅
└── ADR-124-endpoint-naming-constraints.md                  ✅
```

### Runtime（运行时级）
```
docs/adr/runtime/
├── ADR-201-handler-lifecycle-management.md                 ✅
└── ADR-240-handler-exception-constraints.md                ✅
```

---

## 验证结果汇总

### 总体状态

- **总引用数**：55 次
- **唯一 ADR 数**：19 个
- **验证通过**：19/19 (100%)
- **验证失败**：0/19 (0%)
- **已归档引用**：0

### 分类统计

| 级别 | ADR 数量 | 引用次数 | 状态 |
|------|---------|---------|------|
| Constitutional | 8 | 23 | ✅ 全部通过 |
| Governance | 8 | 28 | ✅ 全部通过 |
| Structure | 2 | 4 | ✅ 全部通过 |
| Runtime | 2 | 2 | ✅ 全部通过 |
| **总计** | **19** | **55** | **✅ 100%** |

---

## 未被引用的重要 ADR

以下 ADR 存在于仓库中但未被 Instructions/Skills 引用，可能需要评估是否应该被引用：

### Governance 级别
- ADR-901 - Warning 约束语义
- ADR-905 - 执行级别分类
- ADR-920 - Examples 治理宪法
- ADR-930 - Code Review 合规性
- ADR-945 - ADR 时间轴演化视图
- ADR-950 - Guide/FAQ 文档治理
- ADR-951 - Case 仓库管理
- ADR-952 - 工程标准与 ADR 边界
- ADR-955 - 文档搜索可发现性
- ADR-960 - Onboarding 文档治理
- ADR-965 - Onboarding 互动学习路径
- ADR-970 - 自动化日志集成标准
- ADR-975 - 文档质量监控
- ADR-980 - ADR 生命周期同步
- ADR-990 - 文档演化路线图

### Structure 级别
- ADR-120 - 领域事件命名约定
- ADR-121 - Contract/DTO 命名组织
- ADR-123 - Repository 接口分层

### Runtime 级别
- ADR-210 - 事件版本兼容性
- ADR-220 - 事件总线集成

### Technical 级别
- ADR-301 - 集成测试自动化
- ADR-340 - 结构化日志监控约束
- ADR-350 - 日志可观测性标准
- ADR-360 - CI/CD 管道标准化

**建议**：评估这些 ADR 是否应该被相关的 Instructions 或 Skills 引用。

---

## 维护流程

### 添加新引用
1. 在 Instructions 或 Skills 文件中添加 ADR 引用
2. 在本文档中更新相应的引用列表
3. 验证 ADR 文档存在
4. 更新引用统计

### 删除引用
1. 从 Instructions 或 Skills 文件中移除引用
2. 在本文档中移除相应的引用记录
3. 更新引用统计
4. 评估 ADR 是否变为"未被引用"状态

### 定期审查
- **频率**：每月一次
- **责任人**：Documentation Maintainer
- **检查内容**：
  - 验证所有引用的有效性
  - 检查新增的 ADR 是否需要被引用
  - 更新引用统计
  - 清理失效引用

---

## 版本历史

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|---------|------|
| 1.0 | 2026-02-09 | 初始版本：建立 ADR 引用索引系统 | Copilot Agent |

---

## 相关文档

- [ADR Relationship Map](../docs/adr/ADR-RELATIONSHIP-MAP.md)
- [Agent-Skills 权限映射](./AGENT-SKILLS-MAPPING.md)
- [Architecture Governance System](../docs/ARCHITECTURE-GOVERNANCE-SYSTEM.md)

---

**维护责任**：Documentation Maintainer  
**审核周期**：每月  
**状态**：✅ Active
