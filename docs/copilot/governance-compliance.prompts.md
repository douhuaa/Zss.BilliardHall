# ADR-900 / 治理合规 Copilot 提示词

> 📋 **用途**：协助开发者理解和遵守治理层 ADR 要求，特别是破例管理、合规检查和健康度监控。

**状态**：✅ Active  
**关联 ADR**：
- [ADR-900：架构测试与 CI 治理元规则](/docs/adr/governance/ADR-900-architecture-tests.md)
- [ADR-900：ADR 新增与修订流程](/docs/adr/governance/ADR-900-architecture-tests.md)
- [ADR-930：代码审查与 ADR 合规自检流程](/docs/adr/governance/ADR-930-code-review-compliance.md)

---

## 场景 1：如何申请架构破例？

### 用户问题

&gt; "我需要临时违反 ADR-001 的模块隔离规则，应该怎么办？"

### Copilot 回答模板

根据 **ADR-900.Y 破例成本管理**，申请架构破例需要遵循以下流程：

#### 步骤 1：评估是否真的需要破例

首先确认是否有合规的替代方案：
- ✅ **优先考虑**：使用领域事件（异步通信）
- ✅ **优先考虑**：使用数据契约（只读 DTO）
- ✅ **优先考虑**：使用原始类型（Guid/string/int）

只有在确认无法通过合规方式实现时，才申请破例。

#### 步骤 2：准备破例申请

在 PR 中提供以下信息：

```markdown
## 架构破例申请

**违反 ADR**：ADR-001.X（具体规则编号）

**违规位置**：
- 文件：`src/Modules/Orders/OrderHandler.cs`
- 行号：Line 45-60

**违规描述**：
Orders 模块直接调用 Members 模块的 `IMemberRepository`

**破例原因**：
遗留代码重构中，异步事件化需要分阶段完成。当前版本需要实时验证会员状态。

**到期版本**：v2.5.0

**偿还负责人**：@developer-name

**偿还计划**：
1. v2.3.0：实现会员状态本地缓存（Read Model）
2. v2.4.0：通过领域事件保持缓存同步
3. v2.5.0：替换同步调用为缓存查询，完成偿还

**审批人**：@architect-name
```

#### 步骤 3：在代码中标注破例

在违规代码处添加注释：

```csharp
// ARCH-EXCEPTION: ADR-001.2 - 临时跨模块同步调用
// 到期版本: v2.5.0
// 负责人: @developer-name
// 参见: docs/summaries/arch-violations.md ARCH-XXX
private readonly IMemberRepository _memberRepository;
```

#### 步骤 4：更新破例记录表

在 `docs/summaries/arch-violations.md` 中添加记录：

```markdown
| ID       | ADR       | 规则      | 违规位置                  | 原因       | 到期版本 | 负责人    | 偿还计划         | 审批人    | 状态    |
|----------|-----------|-----------|--------------------------|-----------|---------|----------|-----------------|----------|---------|
| ARCH-XXX | ADR-001.2 | 模块隔离   | Orders/OrderHandler.cs   | 遗留重构   | v2.5.0  | @dev1    | 分 3 版本完成    | @arch1   | 🚧 Active |
```

#### 步骤 5：获得审批

- 提交 PR，标题包含 `[ARCH-VIOLATION]`
- 获得 Tech Lead 或架构师审批
- 合并后，CI 将监控此破例

#### 重要提醒

⚠️ **破例不是逃避，而是债务**：
- CI 会每月第一天自动扫描过期破例
- 过期未偿还 → 构建失败
- 最多延期 2 次，需重新审批
- 超过 2 次必须强制偿还

**参考**：
- [arch-violations.md](/docs/summaries/arch-violations.md) - 查看当前破例
- [ADR-900.Y](/docs/adr/governance/ADR-900-architecture-tests.md#adr-900y破例必须绑定偿还计划与到期监控)

---

## 场景 2：CI 报告过期破例，怎么办？

### 用户问题

&gt; "CI 失败了，说发现过期架构破例，我应该怎么处理？"

### Copilot 回答模板

CI 检测到破例已过期，说明到期版本已达到或超过当前版本。你有两个选择：

#### 选项 1：立即偿还（推荐）

1. **修复违规代码**，按照原偿还计划实施
2. **移除破例标注**（代码中的 `// ARCH-EXCEPTION` 注释）
3. **更新 arch-violations.md**，将破例移至"已归还破例"区：

```markdown
## 已归还破例

| ID       | ADR       | 描述           | 批准日期       | 归还日期       | 归还方式       | 归还人    |
|----------|-----------|---------------|---------------|---------------|---------------|----------|
| ARCH-XXX | ADR-001.2 | Orders 跨模块调用 | 2025-12-01    | 2026-01-26    | 实现事件化通信  | @dev1    |
```

4. **提交 PR**，说明偿还情况
5. **验证 CI 通过**

#### 选项 2：申请延期（需充分理由）

如果遇到技术障碍无法按时偿还：

1. **评估延期必要性**：
   - 是否有不可控的阻塞因素？
   - 是否已尽最大努力？
   - 延期是否会影响系统质量？

2. **准备延期申请**：

```markdown
## 破例延期申请

**破例 ID**：ARCH-XXX

**原到期版本**：v2.5.0

**新到期版本**：v2.7.0

**延期原因**：
依赖的 Members 模块领域事件重构未完成（PR #789 被阻塞）

**延期次数**：1/2（最多延期 2 次）

**新偿还计划**：
1. v2.6.0：完成 Members 模块事件重构
2. v2.7.0：完成 Orders 模块事件化，偿还破例

**审批人**：@architect-name
```

3. **更新 arch-violations.md**，添加延期记录：

```markdown
## 延期记录

| ID       | 原到期版本 | 新到期版本 | 延期原因               | 延期次数 | 审批人    | 审批日期       |
|----------|-----------|-----------|----------------------|---------|----------|---------------|
| ARCH-XXX | v2.5.0    | v2.7.0    | 依赖模块重构未完成      | 1/2     | @arch1   | 2026-01-26    |
```

4. **在活跃破例表中更新到期版本**

#### ⚠️ 重要警告

- **最多延期 2 次**：超过 2 次必须强制偿还
- **每次延期需重新审批**：不能自动续期
- **延期会增加技术债**：影响系统健康度
- **连续延期会触发架构审查**：可能需要重新评估设计

**参考**：
- [ADR-900.Y](/docs/adr/governance/ADR-900-architecture-tests.md#adr-900y破例必须绑定偿还计划与到期监控)

---

## 场景 3：如何运行治理合规性验证？

### 用户问题

&gt; "我想检查项目的治理合规性，有什么工具可以用？"

### Copilot 回答模板

项目提供了 `validate-governance-compliance.sh` 脚本，用于验证治理体系的完整性和一致性。

#### 运行验证

```bash
# 进入项目根目录
cd /path/to/Zss.BilliardHall

# 运行验证脚本
./scripts/validate-governance-compliance.sh
```

#### 验证内容

脚本会检查 7 个维度：

1. **arch-violations.md 结构完整性**
   - 检查 7 个必需章节是否存在
   - 验证强制字段（到期版本、负责人、偿还计划等）

2. **治理 CI workflows 存在性**
   - architecture-tests.yml
   - arch-violations-scanner.yml
   - adr-relationship-check.yml

3. **治理 ADR 依赖声明**
   - 验证治理 ADR 是否声明依赖 ADR-900

4. **README 无裁决力声明**（ADR-910）
   - 检查文档 README 是否包含权威边界声明

5. **治理 ADR 变更政策**
   - 检查核心治理 ADR 是否包含变更政策说明

6. **ADR-900 测试映射**
   - 验证是否存在对应的架构测试

7. **架构测试执行**
   - 运行架构测试，检查通过率

#### 输出示例

```
========================================
治理合规性验证
========================================

[1/6] 验证 arch-violations.md 结构...
✅ PASS: arch-violations.md 结构完整性
✅ PASS: arch-violations.md 强制字段

[2/6] 验证治理相关 CI workflows...
✅ PASS: 治理 CI workflows

...

========================================
验证总结
========================================

总检查项: 7
通过: 6
失败: 1

❌ 治理合规性验证失败
```

#### 修复失败项

根据输出的失败项，参考提供的修复建议：

- 缺少章节 → 补充到 arch-violations.md
- 缺少 workflow → 从模板创建
- 缺少依赖声明 → 在 ADR 的 Relationships 章节添加
- 缺少无裁决力声明 → 在 README 开头添加

#### 集成到 CI

在 `.github/workflows/governance-compliance.yml` 中：

```yaml
- name: Validate Governance Compliance
  run: ./scripts/validate-governance-compliance.sh
```

**参考**：
- [validate-governance-compliance.sh](/scripts/validate-governance-compliance.sh)
- [治理层 README](/docs/adr/governance/README.md)

---

## 场景 4：如何生成治理健康度报告？

### 用户问题

&gt; "季度结束了，我需要生成一份治理健康度报告，应该怎么做？"

### Copilot 回答模板

项目提供了治理健康度报告模板，用于季度/月度治理评估。

#### 步骤 1：复制模板

```bash
# 创建报告文件
cp docs/templates/governance-health-report-template.md \
   docs/reports/governance-health-2026-Q1.md
```

#### 步骤 2：收集数据

报告需要以下数据：

**架构测试数据**：
```bash
# 运行架构测试并保存结果
dotnet test src/tests/ArchitectureTests/ --logger "console;verbosity=detailed" \
  > architecture-test-results.txt

# 提取通过率
grep "Passed!" architecture-test-results.txt
```

**破例数据**：
```bash
# 统计活跃破例
grep "🚧 Active" docs/summaries/arch-violations.md | wc -l

# 统计已归还破例（本季度）
# 手动查看 arch-violations.md 的"已归还破例"区
```

**PR 数据**：
```bash
# 使用 GitHub CLI
gh pr list --state all --limit 100 --json number,title,createdAt,state

# 或在 GitHub UI 中筛选
# https://github.com/douhuaa/Zss.BilliardHall/pulls?q=is%3Apr+created%3A2026-01-01..2026-03-31
```

**ADR 数据**：
```bash
# 统计新增 ADR（本季度）
find docs/adr -name "ADR-*.md" -newermt "2026-01-01" ! -newermt "2026-04-01"

# 统计修订 ADR
git log --since="2026-01-01" --until="2026-04-01" --oneline -- docs/adr/
```

#### 步骤 3：填充模板

打开 `docs/reports/governance-health-2026-Q1.md`，填充以下关键章节：

1. **执行摘要**：填写关键指标和状态
2. **架构测试执行情况**：填写通过率、失败测试分析
3. **架构破例管理**：填写活跃/已归还/延期/过期破例
4. **ADR 演进情况**：填写新增/修订/废弃 ADR
5. **PR 与代码审查合规**：填写合规率、不合规分析
6. **问题与风险**：识别当前问题和风险
7. **改进建议**：提出短期/中期/长期建议

#### 步骤 4：审核和提交

```bash
# 提交报告
git add docs/reports/governance-health-2026-Q1.md
git commit -m "docs: 添加 2026 Q1 治理健康度报告"
git push

# 创建 PR 并请求审核
gh pr create --title "docs: 2026 Q1 治理健康度报告" \
  --body "季度治理健康度评估，请审核"
```

#### 未来自动化（计划）

```bash
# 自动生成报告（开发中）
./scripts/generate-governance-health-report.sh --period 2026-Q1 --output docs/reports/
```

**参考**：
- [governance-health-report-template.md](/docs/templates/governance-health-report-template.md)
- [治理层 README](/docs/adr/governance/README.md)

---

## 场景 5：README 使用了"必须"会不会违规？

### 用户问题

&gt; "我在 README 中写了'必须运行 `dotnet restore`'，会不会违反 ADR-910？"

### Copilot 回答模板

根据 **ADR-910.2 README 禁用的裁决性语言**，需要区分使用场景：

#### ❌ 违规用法

在 README 中直接使用裁决性语言定义架构约束：

```markdown
# ❌ 错误示例
模块必须使用事件通信，禁止直接引用其他模块。
```

**问题**：README 在定义架构规则，但它无权这样做。

#### ✅ 合规用法 1：引用 ADR

在引用 ADR 的上下文中使用：

```markdown
# ✅ 正确示例
根据 ADR-001，模块必须使用事件通信，禁止直接引用其他模块。
```

**原因**：明确表明规则来自 ADR，README 只是转述。

#### ✅ 合规用法 2：操作步骤

在描述操作步骤时使用：

```markdown
# ✅ 正确示例
## 构建项目

必须先运行 `dotnet restore` 恢复依赖：

\```bash
dotnet restore
\```
```

**原因**：这是操作必需步骤，不是架构约束。

#### ✅ 合规用法 3：代码示例标记

在代码示例中使用：

```markdown
# ✅ 正确示例
\```csharp
// ✅ 正确：使用事件通信
await _eventBus.PublishAsync(new OrderCreated(...));

// ❌ 禁止：直接引用其他模块
var member = _memberRepository.GetById(memberId);
\```
```

**原因**：这是示例标记，用于对比教学。

#### ✅ 合规用法 4：表格说明

在表格中引用 ADR 时使用：

```markdown
# ✅ 正确示例
| 操作 | 是否允许 | 依据 |
|------|---------|------|
| 跨模块事件 | ✅ 允许 | ADR-001 |
| 直接引用 | ❌ 禁止 | ADR-001 |
```

**原因**：表格明确引用了 ADR 依据。

#### 判断标准

问自己：
1. 我是在**定义规则**还是**解释用法**？
   - 定义规则 → 应写入 ADR
   - 解释用法 → 可写入 README

2. 我是在**做出判断**还是**引用判断**？
   - 做出判断 → 违规
   - 引用判断（明确标注 ADR）→ 合规

3. 我是在**约束架构**还是**描述步骤**？
   - 约束架构 → 违规
   - 描述步骤 → 合规

**参考**：
- [ADR-910：README 编写与维护治理规范](/docs/adr/governance/ADR-910-readme-governance-constitution.md)

---

## 常见误区

### 误区 1："CI 通过了就不需要人工审查"

❌ **错误理解**：架构测试都过了，直接合并 PR 就行。

✅ **正确理解**：
- 架构测试只能覆盖 L1（静态可执行）和部分 L2（语义半自动）
- L3（人工 Gate）和业务逻辑正确性需要人工审查
- 参见 [ADR-905：执行级别分类](/docs/adr/governance/ADR-905-enforcement-level-classification.md)

### 误区 2："破例记录在 PR 里就行了"

❌ **错误理解**：PR 描述中说明了破例，不需要更新 arch-violations.md。

✅ **正确理解**：
- arch-violations.md 是破例的**唯一权威记录**
- CI 自动扫描此文件，不读取 PR 描述
- 未记录 = 未授权违规

### 误区 3："文档可以随便写，不需要遵守规范"

❌ **错误理解**：README/Guide 只是说明文档，爱怎么写就怎么写。

✅ **正确理解**：
- README/Guide 受 ADR-910/950 约束
- 架构测试会检查裁决性语言使用
- 违规会导致 CI 失败

### 误区 4："延期可以无限次"

❌ **错误理解**：破例到期了申请延期就行，可以一直延期。

✅ **正确理解**：
- 最多延期 **2 次**
- 每次延期需重新审批
- 超过 2 次必须强制偿还或触发架构审查

---

## 相关资源

### ADR 文档

- [ADR-900：架构测试与 CI 治理元规则](/docs/adr/governance/ADR-900-architecture-tests.md)
- [ADR-900：ADR 新增与修订流程](/docs/adr/governance/ADR-900-architecture-tests.md)
- [ADR-910：README 编写与维护治理规范](/docs/adr/governance/ADR-910-readme-governance-constitution.md)
- [ADR-930：代码审查与 ADR 合规自检流程](/docs/adr/governance/ADR-930-code-review-compliance.md)

### 工具和脚本

- [arch-violations-scanner.yml](/.github/workflows/arch-violations-scanner.yml)
- [validate-governance-compliance.sh](/scripts/validate-governance-compliance.sh)
- [governance-health-report-template.md](/docs/templates/governance-health-report-template.md)

### 实施总结

- [治理层 ADR 改进实施总结](/docs/summaries/governance-adr-improvement-implementation.md)

---

**维护**：架构委员会  
**最后更新**：2026-01-26  
**状态**：✅ Active
