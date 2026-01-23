# ADR 模板迁移对比

> 本文档说明现有 ADR 格式与终极模板的对应关系，以及如何迁移。

---

## 一、格式对比

### 现有格式（v1）

```markdown
# ADR-XXXX：标题

**状态**：✅ 已采纳
**级别**：架构约束
**适用范围**：...
**生效时间**：...

## 聚焦内容（Focus）
...

## 术语表（Glossary）
...

## 决策（Decision）
...

## 与其他 ADR 关系
...

## 快速参考表
...
```

### 终极模板（v2）

```markdown
# ADR-XXXX：标题

**状态**：Draft | Accepted | Final | Superseded
**级别**：宪法层 | 决策层
**影响范围**：...
**生效时间**：...

## 1. 规则本体（Rule）
...

## 2. 执法模型（Enforcement）
...

## 3. 破例与归还（Exception）
...

## 4. 变更政策（Change Policy）
...

## 5. 明确不管什么（Non-Goals）
...

## 6. 非裁决性参考（References）
...
```

---

## 二、映射关系

| 现有格式 | 终极模板 | 迁移说明 |
|---------|---------|---------|
| 聚焦内容（Focus） | 5. Non-Goals | 反向映射：Focus 说"管什么"，Non-Goals 说"不管什么" |
| 术语表（Glossary） | 6. References | 移至 References，标注"非裁决性" |
| 决策（Decision） | 1. Rule | **核心映射**：提取硬性规则，去除示例和解释 |
| 决策（Decision）中的【必须架构测试覆盖】 | 2. Enforcement | 映射到测试表 |
| （无对应） | 3. Exception | **新增**：明确破例机制 |
| （无对应） | 4. Change Policy | **新增**：明确变更流程 |
| 与其他 ADR 关系 | 6. References | 移至 References |
| 快速参考表 | 6. References | 移至 References，或删除（教学内容） |

---

## 三、迁移步骤

### 步骤 1：提取硬性规则

**从"决策"段落中提取纯规则**：

**原文（v1）**：

```markdown
## 决策

### 模块定义与隔离

- 按业务能力聚合单独模块（如 Members/Orders/Payments）
- 每模块 = 独立程序集 = 清晰边界目录&命名空间
- 模块外部仅暴露契约和事件，内部实现完全不可被引用

**判例规范：**
- ❌ 禁止模块引用其他模块代码、资源及类型
- ❌ 禁止公共"跨模块继承"或"共用领域类型"
- ✅ 允许通过契约传递原始/DTO，事件发布订阅协作
```

**迁移后（v2）**：

```markdown
## 1. Rule

Modules **MUST NOT**:
- Reference other modules directly
- Share domain objects across module boundaries
- Use cross-module inheritance
- Share common domain types

Modules **MUST** communicate using only:
- Domain Events (async)
- Data Contracts (read-only DTOs)
- Primitive types (Guid, string, int)
```

**迁移原则**：

1. **去除描述性内容**："按业务能力聚合" → 删除（这是 Why，不是 Rule）
2. **去除示例**："如 Members/Orders/Payments" → 删除
3. **转换为 MUST/MUST NOT**："禁止" → "MUST NOT"
4. **去除符号装饰**："❌ 禁止" → "MUST NOT"

---

### 步骤 2：构建执法映射

**从【必须架构测试覆盖】标记中提取**：

**原文（v1）**：

```markdown
## 决策

【必须架构测试覆盖】模块不得互相引用
【必须架构测试覆盖】Platform 不得依赖 Application/Host
```

**迁移后（v2）**：

```markdown
## 2. Enforcement

### 2.1 执行级别

| Level | 名称      | 执法方式   | 后果    |
| ----- | ------- | -------- | ----- |
| L1    | 静态可执行 | 自动化测试 | CI 阻断 |

### 2.2 测试映射

| Rule 编号    | 执行级 | 测试 / 手段                              |
| ---------- | --- | ------------------------------------ |
| ADR-0001.1 | L1  | `Modules_Should_Not_Reference_Each_Other` |
| ADR-0001.2 | L1  | `Platform_Should_Not_Depend_On_Application` |
```

---

### 步骤 3：明确破例机制

**原文（v1）**：

（通常没有明确的破例机制）

**迁移后（v2）**：

```markdown
## 3. Exception

### 3.1 允许破例的前提

破例 **仅在以下情况允许**：

* 第三方库的技术限制
* 迁移期遗留代码（必须在 6 个月内归还）
* 性能关键路径的特殊优化（需架构委员会审批）

### 3.2 破例要求

每个破例 **必须**：

* 记录在 `docs/summaries/ARCH-VIOLATIONS.md`
* 指明 ADR-0001 + Rule 编号
* 指定失效日期（不超过 6 个月）
* 给出归还计划

**未记录的破例 = 未授权架构违规。**
```

---

### 步骤 4：定义变更政策

**原文（v1）**：

```markdown
**状态**：✅ 已采纳（Final，不可随意修改）
```

**迁移后（v2）**：

```markdown
## 4. Change Policy

### 4.1 变更规则

* **宪法层 ADR**（ADR-0001~0005）

  * 修改 = 架构修宪
  * 需要架构委员会 100% 同意
  * 需要 2 周公示期
  * 需要全量回归测试

### 4.2 失效与替代

* Superseded ADR **必须**：
  - 状态标记为 "Superseded by ADR-YYYY"
  - 指向替代 ADR
  - 保留在仓库中（不删除）
```

---

### 步骤 5：标注 Non-Goals

**原文（v1）**：

```markdown
## 聚焦内容（Focus）

- 按业务能力/上下文划分独立模块
- 强制垂直切片与最小用例组织
- 模块隔离：禁止跨模块直接依赖与共享领域对象
```

**迁移后（v2）**：

```markdown
## 5. Non-Goals

本 ADR **不负责**：

* 代码风格（如缩进、命名约定）→ .editorconfig
* 命名美学（如是否使用 I 前缀）→ 团队约定
* 教学示例 → docs/copilot/adr-0001.prompts.md
* 实现细节（如具体使用哪个 ORM）→ ADR-300+ 技术层
* 模块内部组织（如是否使用 DDD）→ 模块自治
```

---

### 步骤 6：整理 References

**原文（v1）**：

```markdown
## 术语表（Glossary）
...

## 与其他 ADR 关系
...

## 快速参考表
...
```

**迁移后（v2）**：

```markdown
## 6. References

> **仅供理解，不具裁决力。**

### 术语表

| 术语 | 定义 |
|------|------|
| 模块化单体 | ... |
| 垂直切片 | ... |

### 相关 ADR

- ADR-0002：Platform/Application/Host 边界
- ADR-0005：应用交互模型

### 辅导材料

- docs/copilot/adr-0001.prompts.md
- docs/guides/module-design.md

### 历史讨论

- GitHub Issue #123：模块隔离讨论
- RFC-001：垂直切片架构提案
```

---

## 四、迁移清单

### ✅ 迁移前检查

- [ ] 阅读原 ADR，理解核心规则
- [ ] 识别哪些是规则，哪些是说明
- [ ] 识别哪些是示例，哪些是执法机制
- [ ] 确认架构测试映射关系

### ✅ 迁移中操作

- [ ] 提取硬性规则到 Rule 段落
- [ ] 转换为 MUST/MUST NOT 语气
- [ ] 去除示例代码和描述性内容
- [ ] 构建执法映射表
- [ ] 明确破例机制
- [ ] 定义变更政策
- [ ] 标注 Non-Goals
- [ ] 整理 References

### ✅ 迁移后验证

- [ ] Rule 段落 ≤ 1 页
- [ ] 没有"建议/推荐/尽量"
- [ ] 没有示例代码
- [ ] 没有长背景故事
- [ ] Enforcement 映射到具体测试
- [ ] Exception 机制完整
- [ ] Change Policy 明确
- [ ] Non-Goals 标注清晰
- [ ] References 标注"非裁决性"

---

## 五、迁移示例

### 示例 1：ADR-0001（模块化单体）

**原文片段**：

```markdown
## 决策

### 模块定义与隔离

- 按业务能力聚合单独模块（如 Members/Orders/Payments）
- 每模块 = 独立程序集 = 清晰边界目录&命名空间
- 模块外部仅暴露契约和事件，内部实现完全不可被引用

**判例规范：**
- ❌ 禁止模块引用其他模块代码、资源及类型
- ❌ 禁止公共"跨模块继承"或"共用领域类型"
- ✅ 允许通过契约传递原始/DTO，事件发布订阅协作

### 垂直切片落地标准

- 以"用例"为最小组织单元，目录建议 `UseCases/xxx/`
- 每用例切片须涵盖 Endpoint/API → Command/Query → Handler → 业务逻辑 → 存储
- ❌ 禁止将业务逻辑抽象到横向 Service 层
- ❌ 禁止横向领域/基础设施依赖上移

**推荐结构：**
```
/Modules/Orders/
  ├── UseCases/
  │    ├── CreateOrder/
  │    │    ├── CreateOrderEndpoint.cs
  │    │    ├── CreateOrderCommand.cs
  │    │    └── CreateOrderHandler.cs
```
```

**迁移后（精简）**：

```markdown
## 1. Rule

### R1.1 模块隔离

Modules **MUST NOT**:
- Reference other modules directly
- Share domain objects across module boundaries
- Use cross-module inheritance
- Share common domain types

### R1.2 跨模块通信

Cross-module communication **MUST** use only:
- Domain Events (async, publish-subscribe)
- Data Contracts (read-only DTOs)
- Primitive types (Guid, string, int)

### R1.3 垂直切片组织

Each use case **MUST**:
- Be organized in a `UseCases/{UseCaseName}/` directory
- Contain all layers: Endpoint → Command/Query → Handler → Domain Logic

Business logic **MUST NOT**:
- Be abstracted into horizontal Service layers
- Be placed in shared infrastructure components

## 2. Enforcement

### 2.1 执行级别

| Level | 名称      | 执法方式   | 后果    |
| ----- | ------- | -------- | ----- |
| L1    | 静态可执行 | 自动化测试 | CI 阻断 |

### 2.2 测试映射

| Rule 编号 | 执行级 | 测试 / 手段                              |
| ------- | --- | ------------------------------------ |
| R1.1    | L1  | `Modules_Should_Not_Reference_Each_Other` |
| R1.2    | L1  | `Modules_Should_Only_Use_Allowed_Communication` |
| R1.3    | L1  | `UseCases_Should_Follow_Vertical_Slice_Pattern` |

## 3. Exception

### 3.1 允许破例的前提

- 迁移期遗留代码（必须在 6 个月内归还）
- 第三方库的技术限制（需架构委员会审批）

### 3.2 破例要求

每个破例 **必须**：
- 记录在 `docs/summaries/ARCH-VIOLATIONS.md`
- 指明 ADR-0001 + Rule 编号（如 R1.1）
- 指定失效日期（不超过 6 个月）
- 给出归还计划

## 4. Change Policy

### 4.1 变更规则

* **宪法层 ADR**（ADR-0001~0005）
  * 修改 = 架构修宪
  * 需要架构委员会 100% 同意
  * 需要 2 周公示期
  * 需要全量回归测试

### 4.2 失效与替代

* Superseded ADR **必须**指向替代 ADR
* 不允许"隐性废弃"

## 5. Non-Goals

本 ADR **不负责**：

* 模块内部组织（DDD 聚合设计等）→ 模块自治
* 代码风格和命名约定 → .editorconfig
* 具体技术选型（ORM、日志框架）→ ADR-300+ 技术层
* 教学示例和最佳实践 → docs/copilot/adr-0001.prompts.md

## 6. References

> **仅供理解，不具裁决力。**

### 术语表

| 术语 | 定义 |
|------|------|
| 模块化单体 | 单进程，按业务能力独立模块，物理分离，逻辑松耦合 |
| 垂直切片 | 以单个业务用例为最小单元，横穿端到端实现 |
| 契约 | 只读、单向、版本化的数据 DTO |

### 相关 ADR

- ADR-0002：Platform/Application/Host 边界
- ADR-0005：应用交互模型

### 辅导材料

- docs/copilot/adr-0001.prompts.md（示例代码和常见问题）
- docs/guides/module-design.md（设计指南）

### 推荐结构示例

```
/Modules/Orders/
  ├── UseCases/
  │    ├── CreateOrder/
  │    │    ├── CreateOrderEndpoint.cs
  │    │    ├── CreateOrderCommand.cs
  │    │    └── CreateOrderHandler.cs
```

（注：此为参考，非强制）
```

**对比分析**：

| 方面 | 原文 | 迁移后 |
|------|------|--------|
| 规则陈述 | 混杂描述和规则 | 纯规则，MUST/MUST NOT |
| 示例代码 | 包含在决策中 | 移至 References |
| 术语表 | 独立段落 | 移至 References |
| 执法机制 | 隐含在【必须架构测试覆盖】 | 明确的执法映射表 |
| 破例机制 | 无明确说明 | 完整的破例和归还流程 |
| 变更政策 | 仅状态标记 | 完整的变更和废弃流程 |
| Non-Goals | 无 | 明确不负责的范围 |

---

## 六、批量迁移策略

### 策略 1：渐进式迁移（推荐）

**适用场景**：现有 ADR 较多，团队需要时间适应

**步骤**：

1. **阶段 1**：新 ADR 使用终极模板
2. **阶段 2**：宪法层 ADR 优先迁移（ADR-0001~0005）
3. **阶段 3**：治理层 ADR 迁移（ADR-0000, 900-999）
4. **阶段 4**：其他层级按需迁移

**优点**：

- 风险低，影响范围可控
- 团队有时间学习新模板
- 可以根据反馈调整

**缺点**：

- 迁移周期长
- 存在两种格式并存期

---

### 策略 2：一次性迁移

**适用场景**：ADR 数量少（< 10 个），团队准备充分

**步骤**：

1. **准备期**（1 周）：学习新模板，制定迁移计划
2. **迁移期**（1-2 周）：批量迁移所有 ADR
3. **验证期**（1 周）：验证迁移质量，修复问题

**优点**：

- 快速统一格式
- 一次性解决

**缺点**：

- 风险高，影响面大
- 需要大量时间投入

---

### 策略 3：混合策略（本项目推荐）

**适用场景**：本项目（7 个 ADR，已有良好基础）

**步骤**：

1. **立即**：新建终极模板和使用指南（已完成）
2. **本周**：迁移宪法层 ADR（ADR-0001~0005）
3. **本周**：迁移治理层 ADR（ADR-0000, ADR-0900）
4. **持续**：新 ADR 严格使用终极模板

**理由**：

- 现有 ADR 数量适中（7 个）
- 现有格式已经较好，迁移成本不高
- 可以快速统一格式，避免长期并存

---

## 七、迁移后的质量保证

### 质量检查清单

每个迁移后的 ADR 必须通过以下检查：

- [ ] **Rule 段落**
  - [ ] ≤ 1 页
  - [ ] 全部使用 MUST/MUST NOT
  - [ ] 没有示例代码
  - [ ] 没有"建议/推荐/尽量"
  - [ ] 可判真伪

- [ ] **Enforcement 段落**
  - [ ] 标注了执行级别
  - [ ] 映射到具体测试
  - [ ] 测试存在且通过

- [ ] **Exception 段落**
  - [ ] 明确破例前提
  - [ ] 明确破例要求
  - [ ] 引用 ARCH-VIOLATIONS.md

- [ ] **Change Policy 段落**
  - [ ] 明确变更流程
  - [ ] 明确废弃机制

- [ ] **Non-Goals 段落**
  - [ ] 列出不负责的事项
  - [ ] 指向替代文档

- [ ] **References 段落**
  - [ ] 标注"非裁决性"
  - [ ] 术语表（如需要）
  - [ ] 相关 ADR
  - [ ] 辅导材料

---

### 自动化验证

可以考虑编写脚本验证：

```bash
# 检查 ADR 是否包含必需段落
./scripts/validate-adr-format.sh ADR-0001-xxx.md

# 检查 Rule 段落是否 ≤ 1 页
./scripts/check-rule-length.sh ADR-0001-xxx.md

# 检查是否有禁用词（建议/推荐/尽量）
./scripts/check-forbidden-words.sh ADR-0001-xxx.md

# 检查测试映射是否存在
./scripts/validate-test-mapping.sh ADR-0001-xxx.md
```

---

## 八、常见问题

### Q1：必须立即迁移所有 ADR 吗？

**A**：不必。可以采用渐进式策略，优先迁移宪法层和治理层，其他按需迁移。

---

### Q2：迁移后原文档如何处理？

**A**：保留原文档作为历史记录，或在同一文件中更新。建议在 Git 历史中保留原版本。

---

### Q3：迁移后如何确保一致性？

**A**：

1. 使用统一的模板文件
2. 通过 Code Review 检查
3. 编写自动化验证脚本
4. 定期审计 ADR 质量

---

### Q4：Copilot prompts 需要同步更新吗？

**A**：是的。Copilot prompts 应引用新格式的 ADR，特别是 Rule 和 Enforcement 段落。

---

### Q5：架构测试需要修改吗？

**A**：架构测试本身不需要修改，但测试映射表需要更新到 ADR 的 Enforcement 段落。

---

## 九、总结

### 迁移的核心原则

1. **Rule = 裁决源**：提取纯规则，去除一切解释
2. **Enforcement = 执法**：规则必须可执行
3. **Exception = 债务**：破例必须记录和归还
4. **Change Policy = 修宪**：ADR 不是随意修改的文档
5. **Non-Goals = 边界**：防止 ADR 膨胀
6. **References = 辅助**：术语、示例、历史都在这里

### 迁移的最终目标

> **让 ADR 成为系统的法律条文，而不是架构师的解释说明。**

### 现在可以开始

你现在可以：

1. ✅ 使用终极模板编写新 ADR
2. ✅ 按迁移清单迁移现有 ADR
3. ✅ 拒绝不符合模板的 ADR

**记住**：凡是不适合放进这个模板的内容，都不配叫 ADR。
