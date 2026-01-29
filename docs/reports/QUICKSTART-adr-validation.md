# ADR 验证工具快速上手指南

**目标读者**：架构师、文档维护者、开发者  
**预计阅读时间**：5 分钟  
**最后更新**：2026-01-29

---

## 🎯 为什么需要这些工具？

近期对 ADR 进行了大量更新，包括：
- ADR-902：定义了标准 Front Matter 和文档结构
- ADR-0006：定义了标准术语表格式（三列：术语、定义、英文对照）
- ADR-940：定义了关系声明的双向一致性要求
- ADR-980：定义了版本号格式规范

**问题**：旧的 ADR 没有同步这些新规则，导致 42 处不一致。

**解决方案**：自动化验证工具 + 详细整改计划。

---

## 🚀 5 分钟上手

### 1. 检查你的环境

```bash
# 确保在项目根目录
cd /path/to/Zss.BilliardHall

# 确保脚本有执行权限
chmod +x scripts/check-adr-consistency.sh
chmod +x scripts/check-terminology.sh

# 确保有 Python 3
python3 --version  # 应显示 Python 3.6+
```

### 2. 运行第一个检查

```bash
# 检查 ADR 一致性（最常用）
./scripts/check-adr-consistency.sh
```

**预期输出**：
```
🔍 开始 ADR 一致性检查...

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
检查 1: Front Matter 完整性
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

❌ ADR-0001-modular-monolith-vertical-slice-architecture.md 缺少 Front Matter
...
```

### 3. 理解输出

工具使用颜色标记问题严重程度：

- 🔴 **红色（❌）**：严重问题，必须修复
- 🟡 **黄色（⚠️）**：警告，建议修复
- 🟢 **绿色（✅）**：检查通过

### 4. 运行完整验证

```bash
# 运行三个核心检查
./scripts/check-adr-consistency.sh
python3 ./scripts/validate-adr-relationships.py
./scripts/check-terminology.sh
```

---

## 📋 工具详解

### 工具 1: ADR 一致性检查器

**文件**：`scripts/check-adr-consistency.sh`  
**语言**：Bash  
**用途**：验证 ADR 格式一致性

#### 检查项

| 检查项 | 依据 ADR | 严重程度 |
|-------|---------|---------|
| Front Matter 完整性 | ADR-902 | 🔴 高 |
| 术语表格式（英文对照） | ADR-0006 | 🟡 中 |
| 版本号格式（无 v 前缀） | ADR-980 | 🟡 中 |
| 快速参考表（宪法层） | ADR-0006 | 🟢 低 |

#### 使用示例

```bash
# 基础检查
./scripts/check-adr-consistency.sh

# 只检查特定目录
find docs/adr/constitutional -name "*.md" | while read file; do
    echo "检查 $file"
    # 手动检查逻辑
done
```

#### 常见问题修复

**问题 1：缺少 Front Matter**

❌ **错误示例**：
```markdown
# ADR-0001: 模块化单体与垂直切片架构

**版本**：4.0
**状态**：Final
```

✅ **正确示例**：
```markdown
---
adr: ADR-0001
title: "模块化单体与垂直切片架构"
status: Final
level: Constitutional
version: "4.0"
---

# ADR-0001: 模块化单体与垂直切片架构
```

**问题 2：术语表缺少英文对照**

❌ **错误示例**：
```markdown
## 术语表

| 术语 | 定义 |
|-----|------|
| 模块化单体 | 单进程部署的架构 |
```

✅ **正确示例**：
```markdown
## 术语表

| 术语 | 定义 | 英文对照 |
|-----|------|---------|
| 模块化单体 | 单进程部署的架构 | Modular Monolith |
```

---

### 工具 2: ADR 关系验证器

**文件**：`scripts/validate-adr-relationships.py`  
**语言**：Python 3  
**用途**：验证 ADR 关系声明的双向一致性

#### 检查项

| 检查项 | 依据 ADR | 严重程度 |
|-------|---------|---------|
| 双向关系一致性 | ADR-940 | 🔴 高 |
| 循环依赖检测 | ADR-940 | 🟡 中 |
| 孤立关系引用 | ADR-940 | 🟡 中 |

#### 使用示例

```bash
# 基础检查
python3 ./scripts/validate-adr-relationships.py

# 查看详细统计
python3 ./scripts/validate-adr-relationships.py | grep "统计信息" -A 10
```

#### 常见问题修复

**问题：双向关系不一致**

❌ **错误示例**：

在 ADR-0001 中：
```markdown
## ADR 关系

**依赖**：ADR-0002
```

但在 ADR-0002 中：
```markdown
## ADR 关系

（没有声明被 ADR-0001 依赖）
```

✅ **正确示例**：

在 ADR-0001 中：
```markdown
## ADR 关系

**依赖**：ADR-0002（模块隔离基于三层架构）
```

在 ADR-0002 中：
```markdown
## ADR 关系

**被依赖**：ADR-0001（模块隔离基于三层架构）
```

---

### 工具 3: 术语一致性检查器

**文件**：`scripts/check-terminology.sh`  
**语言**：Bash  
**用途**：验证术语定义的一致性

#### 检查项

| 检查项 | 依据 ADR | 严重程度 |
|-------|---------|---------|
| 重复定义检测 | ADR-0006 | 🟡 中 |
| 英文对照完整性 | ADR-0006 | 🟡 中 |
| 格式统一性 | ADR-0006 | 🟢 低 |

#### 使用示例

```bash
# 基础检查
./scripts/check-terminology.sh

# 查看所有提取的术语
cat /tmp/all-terms-*.txt
```

#### 常见问题修复

**问题：同一术语在多个 ADR 中定义**

❌ **不推荐**：

在 ADR-0001 中定义"模块化单体"  
在 ADR-0006 中再次定义"模块化单体"

✅ **推荐**：

在 ADR-0006（术语宪法）中建立权威定义：
```markdown
## 术语表

| 术语 | 定义 | 英文对照 |
|-----|------|---------|
| 模块化单体 | 单进程部署，按业务能力划分独立模块 | Modular Monolith |
```

在 ADR-0001 中引用：
```markdown
## 术语说明

本 ADR 中使用的术语定义参见 [ADR-0006 术语表](../constitutional/ADR-0006-terminology-numbering-constitution.md#术语表)。
```

---

## 🔄 典型工作流程

### 场景 1：修改现有 ADR

```bash
# 1. 修改前检查当前状态
./scripts/check-adr-consistency.sh

# 2. 编辑 ADR
vim docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md

# 3. 修改后验证
./scripts/check-adr-consistency.sh
python3 ./scripts/validate-adr-relationships.py

# 4. 如果通过，提交
git add docs/adr/constitutional/ADR-0001-*.md
git commit -m "fix: Add Front Matter to ADR-0001"
```

### 场景 2：创建新 ADR

```bash
# 1. 使用标准模板创建
cp docs/templates/adr-template.md docs/adr/governance/ADR-XXX-new-rule.md

# 2. 编辑内容
vim docs/adr/governance/ADR-XXX-new-rule.md

# 3. 验证格式
./scripts/check-adr-consistency.sh | grep "ADR-XXX"

# 4. 验证关系声明
python3 ./scripts/validate-adr-relationships.py

# 5. 提交
git add docs/adr/governance/ADR-XXX-new-rule.md
git commit -m "docs: Add ADR-XXX"
```

### 场景 3：批量修复问题

```bash
# 1. 生成问题清单
./scripts/check-adr-consistency.sh > /tmp/issues.txt

# 2. 按类别修复（如批量添加 Front Matter）
# 编写批量修复脚本或使用 sed/awk

# 3. 持续验证
while true; do
    ./scripts/check-adr-consistency.sh
    read -p "继续修复？(y/n) " answer
    [[ "$answer" != "y" ]] && break
done

# 4. 最终验证
./scripts/check-adr-consistency.sh && \
python3 ./scripts/validate-adr-relationships.py && \
./scripts/check-terminology.sh
```

---

## 📊 理解验证结果

### 退出码

| 退出码 | 含义 | 下一步行动 |
|-------|------|----------|
| 0 | ✅ 检查通过 | 可以提交代码 |
| 1 | ❌ 发现问题 | 必须修复后才能提交 |

### 问题优先级

根据工具输出的颜色和标记：

| 标记 | 优先级 | 修复时间要求 |
|-----|-------|-------------|
| 🔴 ❌ | P0 | 立即修复 |
| 🟡 ⚠️ | P1-P2 | 1-2周内修复 |
| 🔵 ℹ️ | P3-P4 | 1-2月内修复 |
| 🟢 ✅ | 无问题 | 保持现状 |

---

## 🆘 故障排查

### 问题 1：脚本没有执行权限

**症状**：
```bash
bash: ./scripts/check-adr-consistency.sh: Permission denied
```

**解决**：
```bash
chmod +x scripts/*.sh
```

### 问题 2：Python 脚本找不到模块

**症状**：
```bash
ModuleNotFoundError: No module named 'xxx'
```

**解决**：
```bash
# 本项目脚本不需要额外依赖，只需 Python 3 标准库
python3 --version  # 确保 >= 3.6
```

### 问题 3：工具输出没有颜色

**症状**：输出包含 `\033[0;31m` 等字符

**原因**：某些终端不支持 ANSI 颜色

**解决**：这不影响功能，可以忽略，或使用支持颜色的终端

### 问题 4：找不到 ADR 目录

**症状**：
```bash
❌ 错误：找不到 ADR 目录 docs/adr
```

**解决**：
```bash
# 确保在项目根目录运行
cd /path/to/Zss.BilliardHall
pwd  # 应显示项目根路径
ls docs/adr  # 应显示 ADR 文件
```

---

## 📚 延伸阅读

### 核心文档
- 📘 [ADR 同步性详细分析报告](adr-synchronization-analysis-2026-01-29.md) - 42 处问题详解
- 📘 [整改路线图](adr-synchronization-roadmap.md) - Gantt 图和时间表
- 📘 [工作总结](adr-synchronization-summary.md) - Phase 1-2 成果

### 相关 ADR
- 📘 [ADR-902: ADR 标准模板](../adr/governance/ADR-902-adr-template-structure-contract.md)
- 📘 [ADR-940: ADR 关系管理](../adr/governance/ADR-940-adr-relationship-traceability-management.md)
- 📘 [ADR-980: ADR 生命周期同步](../adr/governance/ADR-980-adr-lifecycle-synchronization.md)
- 📘 [ADR-0006: 术语与编号宪法](../adr/constitutional/ADR-0006-terminology-numbering-constitution.md)

### 工具文档
- 📘 [验证工具完整文档](../scripts/README-adr-validation-tools.md)

---

## 🤝 获取帮助

### 常见问题
1. **工具报告的问题是否都必须修复？**
   - P0/P1 问题：必须修复
   - P2/P3/P4 问题：建议修复，有时间表

2. **修复后如何验证？**
   - 重新运行相应的验证工具
   - 查看退出码是否为 0

3. **可以跳过某些检查吗？**
   - 不建议跳过，所有检查都基于 ADR 要求
   - 如有特殊情况，需在 PR 中说明原因

### 联系方式
- **技术问题**：在 PR 中评论或创建 Issue
- **流程问题**：联系架构委员会
- **紧急问题**：@douhuaa

---

**版本**：1.0  
**维护者**：架构委员会  
**最后更新**：2026-01-29  
**反馈渠道**：GitHub Issues (label: `adr-validation-tools`)
