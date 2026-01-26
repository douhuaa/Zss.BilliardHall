# ADR-0000：架构测试与 CI 治理宪法

> **唯一架构执法元规则**：本文件定义架构合法性评判的唯一基准。所有架构测试、CI 校验、Prompt 映射、破例治理均以本 ADR 正文为裁定源。

**状态**：✅ Final（不可随意修改）  
**版本**：2.0  
**级别**：架构治理 / 宪法层  
**生效时间**：即刻  
**适用范围**：全体代码仓库及所有 ADR（ADR-0001 ~ 0005）

---

## 聚焦内容（Focus）

- ADR-测试一一映射与唯一性
- 自动化校验与 CI 阻断机制
- 架构约束的分级测试与溯源跟踪
- 破例治理与到期归还
- Prompts/流程/文档合规自检闭环

---

## 术语表（Glossary）

| 术语       | 定义                   |
|----------|----------------------|
| 架构测试     | 可自动执行的结构约束型测试        |
| ADR-测试映射 | ADR 【必须架构测试覆盖】→ 测试用例 |
| CI 阻断    | 测试失败即阻断 PR / 发布      |
| 破例       | 已批准的临时性违规（需归还）       |

---

## 核心决策（Decision）

- 所有【必须架构测试覆盖】的 ADR 条款，须有自动化测试（静态/语义/人工 Gate）
- 测试类、方法名、失败消息必须显式标注 ADR 编号
- 架构测试失败 = 架构违规，CI 阻断，无例外
- 所有 Prompts、流程、辅助材料以 ADR-0000 条款为准，发现冲突应修订辅导材料
- 测试组织须按 ADR 编号、内容、类型归档
- 三级执行级（L1静态、L2语义半自动、L3人工Gate），覆盖范围与映射表公开

### ADR-0000.X：规则冲突时的优先级裁决

当 ADR 规则发生冲突时，**必须**按以下优先级从高到低裁决：

1. **架构安全与数据一致性**（如 ADR-220.2 Outbox Pattern）
2. **跨模块稳定性与演进性**（如 ADR-210.x 事件版本化）
3. **生命周期与资源管理**（如 ADR-201.x Handler 生命周期）
4. **结构规范与可维护性**（如 ADR-122.x 测试组织）
5. **流程与治理规则**（如 ADR-930.x 代码审查）

**裁决原则**：
- ✅ 优先保障系统正确性和稳定性
- ✅ 优先保障长期演进能力
- ✅ 优先保障资源安全和性能
- ❌ 规范性要求在必要时可妥协（需记录）

**冲突示例**：
- ADR-220.2（事务性发布）vs ADR-201.4（资源释放）→ 优先 220.2
- ADR-210.3（版本保留）vs ADR-122.1（代码清理）→ 优先 210.3

### ADR-0000.Y：破例必须绑定偿还计划与到期监控

所有架构破例**必须**绑定明确的偿还计划和到期监控机制。

**破例强制要求**：
- ✅ 到期版本号（如 v2.5.0）
- ✅ 偿还负责人
- ✅ 偿还计划和时间表
- ✅ CI 自动扫描过期破例并失败构建

**实施机制**：
```markdown
## arch-violations.md 格式

| ADR | 规则 | 到期版本 | 负责人 | 偿还计划 | 状态 |
|-----|------|---------|--------|---------|------|
| ADR-201.1 | Handler Scoped | v2.5.0 | @dev | 迁移至 Scoped | 🚧 |
```

**CI 强制检查**：
- 每月第一次构建扫描 arch-violations.md
- 发现过期破例 → 构建失败
- 强制团队偿还或延期（需重新审批）

---

## 测试映射与执行分级

### 1. ADR-测试映射

| ADR 编号   | 测试类                            | 关键测试用例                 | 必须测试 |
|----------|--------------------------------|------------------------|------|
| ADR-0001 | ADR_0001_Architecture_Tests.cs | 模块隔离、契约合规、垂直切片         | ✅    |
| ADR-0002 | ADR_0002_Architecture_Tests.cs | 层级依赖、Host 装配边界         | ✅    |
| ADR-0003 | ADR_0003_Architecture_Tests.cs | 命名空间映射、防御性规则           | ✅    |
| ADR-0004 | ADR_0004_Architecture_Tests.cs | CPM、层级依赖、版本唯一性         | ✅    |
| ADR-0005 | ADR_0005_Architecture_Tests.cs | Handler、CQRS、模块通信、状态约束 | ✅    |

> 新增ADR/变更，必须同步补测。遗漏即视为流程错误。

### 2. 执行分级（Enforcement Level）

- **Level 1 静态可执行**：NetArchTest 强制，CI 阻断
- **Level 2 语义半自动**：Roslyn Analyzer 等启发式，需人工二次确认
- **Level 3 人工 Gate**：不可程控的约束，长期破例流程审计

具体标准参见 [ADR-905-enforcement-level-classification.md](/docs/adr/governance/ADR-905-enforcement-level-classification.md)

---

## 测试组织与自治原则

- 所有架构测试目录、类、用例均以 `ADR-XXXX` 前缀命名
- 测试失败消息格式：`ADR-XXXX 违规：{原因} 修复建议：{建议}`
- 禁止架构测试跳过（需显式记录在ARCH-VIOLATIONS，季度审计）
- 登录、变更、废弃测试用例，均需同步更新ADR映射表及Prompts

---

## 破例治理与归还

- 破例需在 PR 标题和描述中声明，填写《破例表单》
- 本地和CI均需预警破例到期
- 所有破例记录归档于 [`ARCH-VIOLATIONS.md`](/docs/summaries/arch-violations.md)
- 连续破例三次未归还，自动触发架构审查

---

## 自动校验与CI策略

- 所有 PR、主分支合并前自动执行所有架构测试
- 失败即阻断，无例外
- 映射脚本/shell同步比对 ADR→Test→Prompt → 显示不一致列表，强制责任人修正

---

## 企业级行动建议

- 建议设定每季度架构健康度报告，统计测试失效、破例量、CI 通过率
- Copilot Prompts 与测试、ADR 同步迭代维护
- 新增/变更ADR时，严格全链更新自检（ADR文档、测试代码、Prompts文件、映射脚本）
- Onboarding 必须培训ADR-0000机制

---

## 关系声明（Relationships）

**依赖（Depends On）**：
- 无（本 ADR 为元规则，不依赖其他 ADR）

**被依赖（Depended By）**：
- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) - 其测试执行基于本 ADR
- [ADR-0002：平台、应用与主机启动器架构](../constitutional/ADR-0002-platform-application-host-bootstrap.md) - 其测试执行基于本 ADR
- [ADR-0003：命名空间与项目结构规范](../constitutional/ADR-0003-namespace-rules.md) - 其测试执行基于本 ADR
- [ADR-0004：中央包管理与层级依赖规则](../constitutional/ADR-0004-Cpm-Final.md) - 其测试执行基于本 ADR
- [ADR-0005：应用内交互模型与执行边界](../constitutional/ADR-0005-Application-Interaction-Model-Final.md) - 其测试执行基于本 ADR
- [ADR-905：执行级别分类](./ADR-905-enforcement-level-classification.md) - 执行级别基于本 ADR
- [ADR-970：自动化工具日志集成标准](./ADR-970-automation-log-integration-standard.md) - 测试报告标准基于本 ADR
- [ADR-980：ADR 生命周期一体化同步机制](./ADR-980-adr-lifecycle-synchronization.md) - CI 检测机制基于本 ADR
- [ADR-360：CI/CD Pipeline 流程标准化](../technical/ADR-360-cicd-pipeline-standardization.md)
- [ADR-301：集成测试环境自动化与隔离约束](../technical/ADR-301-integration-test-automation.md)
- [ADR-0006：术语与编号宪法](../constitutional/ADR-0006-terminology-numbering-constitution.md)
- [ADR-0007：Agent 行为与权限宪法](../constitutional/ADR-0007-agent-behavior-permissions-constitution.md)
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md)
- [ADR-920：示例代码治理宪法](../governance/ADR-920-examples-governance-constitution.md)
- [ADR-900：ADR 新增与修订流程](../governance/ADR-900-adr-process.md)
- [ADR-930：代码审查与 ADR 合规自检流程](../governance/ADR-930-code-review-compliance.md)
- [ADR-910：README 编写与维护宪法](../governance/ADR-910-readme-governance-constitution.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0006：术语与编号宪法](../constitutional/ADR-0006-terminology-numbering-constitution.md) - ADR 编号规范
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - ADR 文档治理

---

## 版本历史

| 版本  | 日期         | 变更说明              |
|-----|------------|-------------------|
| 2.0 | 2026-01-23 | 聚焦自动化与治理闭环，细化执行分级 |
| 1.0 | 2026-01-20 | 初版                |

---

## 附件与参考

- [ADR-905-enforcement-level-classification.md](/docs/adr/governance/ADR-905-enforcement-level-classification.md)
- [`ARCH-VIOLATIONS.md`](/docs/summaries/arch-violations.md)
