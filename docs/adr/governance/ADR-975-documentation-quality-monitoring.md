---
adr: ADR-975
title: "文档质量指标与监控"
status: Accepted
level: Governance
deciders: "Architecture Board & Documentation Team"
date: 2026-02-04
version: "2.0"
maintainer: "Architecture Board & Documentation Team"
primary_enforcement: L1
reviewer: "待定"
supersedes: null
superseded_by: null
---

# ADR-975：文档质量指标与监控

> ⚖️ **本 ADR 是文档质量监控的唯一标准，定义质量指标、自动化检测和定期报告机制。**

## Focus（聚焦内容）

- 质量指标定义
- 自动化检测机制
- 定期报告生成
- 质量改进流程
- 仪表板可视化

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 质量指标 | 量化评估文档质量的标准 | Quality Metric |
| 准确性 | 文档内容与实际代码的一致性 | Accuracy |
| 完整性 | 文档覆盖所有必要内容的程度 | Completeness |
| 时效性 | 文档更新的及时程度 | Timeliness |
| 可查找性 | 用户能否快速找到文档 | Findability |
| 链接有效性 | 文档中链接的可用性 | Link Validity |
| 代码示例可编译性 | 代码示例能否成功编译运行 | Code Compilability |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-975 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-975_<Rule>_<Clause>
> ```

---

### ADR-975_1：质量指标定义（Rule）

#### ADR-975_1_1 文档质量必须通过以下指标量化评估
文档质量 **必须**通过以下指标量化评估。

#### ADR-975_1_2 核心指标
**核心指标**：

| 指标 | 定义 | 目标 | 测量方式 | 权重 |
|------|------|------|---------|------|
| **准确性** | 文档内容与代码一致 | > 95% | 人工审计 + 用户反馈 | 30% |
| **完整性** | 必要章节齐全 | > 90% | 自动检查模板完整度 | 20% |
| **时效性** | 更新距离变更时间 | < 7 天 | Git 提交时间分析 | 15% |
| **可查找性** | 用户能快速找到 | < 2 分钟 | 用户反馈调查 | 15% |
| **链接有效性** | 链接可用性 | 100% | 自动爬虫检查 | 10% |
| **代码可编译性** | 代码示例可运行 | 100% | 自动编译测试 | 10% |

#### ADR-975_1_3 计算公式
**计算公式**：
```
文档质量得分 = Σ(指标得分 × 权重)

示例：
准确性: 98% × 0.30 = 29.4
完整性: 95% × 0.20 = 19.0
时效性: 92% × 0.15 = 13.8
可查找性: 90% × 0.15 = 13.5
链接有效性: 100% × 0.10 = 10.0
代码可编译性: 100% × 0.10 = 10.0
------------------------------------
总分: 95.7%
```

#### ADR-975_1_4 质量等级
**质量等级**：
- ⭐⭐⭐⭐⭐ **优秀**：≥ 95%
- ⭐⭐⭐⭐ **良好**：85% - 94%
- ⭐⭐⭐ **合格**：75% - 84%
- ⭐⭐ **需改进**：60% - 74%
- ⭐ **不合格**：< 60%

#### ADR-975_1_5 核心原则
**核心原则**：
> 量化评估，明确目标，持续改进。

**判定**：
- ❌ 文档质量无法量化
- ❌ 目标模糊不清
- ✅ 清晰的指标和目标

---

### ADR-975_2：自动化检测机制（Rule）

#### ADR-975_2_1 质量检测必须自动化执行
质量检测 **必须**自动化执行。

#### ADR-975_2_2 链接有效性检测
**自动化检测项目**：

#### 1. 链接有效性检测

**工具**：`markdown-link-check`

**CI Workflow**：
```yaml
name: Documentation Quality Check

on:
  push:
    paths:
      - 'docs/**/*.md'
  pull_request:
    paths:
      - 'docs/**/*.md'
  schedule:
    - cron: '0 2 * * 1' # 每周一凌晨 2 点

jobs:
  link-check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Check Links
        uses: gaurav-nelson/github-action-markdown-link-check@v1
        with:
          config-file: '.github/markdown-link-check-config.json'
          folder-path: 'docs/'
      
      - name: Generate Report
        if: failure()
        run: |
          echo "## 🔗 链接检查失败" >> $GITHUB_STEP_SUMMARY
          echo "请修复失效的链接" >> $GITHUB_STEP_SUMMARY
```

#### ADR-975_2_3 代码示例可编译性检测
#### 2. 代码示例可编译性检测

**脚本**：`scripts/test-code-examples.sh`

```bash
#!/bin/bash
# 提取 Markdown 中的代码块并尝试编译

find docs/ -name "*.md" | while read file; do
    # 提取 C# 代码块
    sed -n '/```csharp/,/```/p' "$file" | sed '1d;$d' > /tmp/code.cs
    
    if [ -s /tmp/code.cs ]; then
        # 尝试编译
        dotnet build /tmp/code.cs 2>&1 | tee -a /tmp/compile-errors.log
    fi
done
```

**CI 集成**：
```yaml
  code-check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
      
      - name: Test Code Examples
        run: bash scripts/test-code-examples.sh
```

#### ADR-975_2_4 文档完整性检测
#### 3. 文档完整性检测

**脚本**：`scripts/check-doc-completeness.sh`

```bash
#!/bin/bash
# 检查 ADR 是否包含所有必要章节

REQUIRED_SECTIONS=(
    "## 聚焦内容"
    "## 术语表"
    "## 决策"
    "## 关系声明"
    "## 执法模型"
    "## 变更政策"
    "## 明确不管什么"
)

find docs/adr/ -name "ADR-*.md" | while read file; do
    for section in "${REQUIRED_SECTIONS[@]}"; do
        if ! grep -q "$section" "$file"; then
            echo "❌ $file 缺少章节: $section"
        fi
    done
done
```

#### ADR-975_2_5 时效性检测
#### 4. 时效性检测

**脚本**：`scripts/check-doc-staleness.sh`

```bash
#!/bin/bash
# 检查文档是否过时（超过 90 天未更新）

STALE_THRESHOLD_DAYS=90

find docs/ -name "*.md" | while read file; do
    last_modified=$(git log -1 --format=%ct "$file")
    current=$(date +%s)
    age_days=$(( (current - last_modified) / 86400 ))
    
    if [ $age_days -gt $STALE_THRESHOLD_DAYS ]; then
        echo "⚠️ $file 已 $age_days 天未更新（阈值：$STALE_THRESHOLD_DAYS 天）"
    fi
done
```

#### ADR-975_2_6 核心原则
**核心原则**：
> 自动检测，早期发现，及时修复。

**判定**：
- ❌ 依赖人工检查
- ❌ 问题发现太晚
- ✅ 自动化检测和报告

---

### ADR-975_3：定期报告生成（Rule）

#### ADR-975_3_1 必须每月生成文档质量报告
**必须**每月生成文档质量报告。

#### ADR-975_3_2 报告位置
**报告位置**：
```
docs/reports/quality/YYYY-MM.md
```

#### ADR-975_3_3 报告结构
**报告结构**：
```markdown
# 文档质量月度报告

**报告期**：YYYY-MM  
**生成时间**：YYYY-MM-DD  
**报告人**：[自动生成 / 负责人]
```

---

### ADR-975_4：质量阈值与阻断策略（Rule）

#### ADR-975_4_1 最低质量阈值
- ADR / Onboarding / Architecture 文档质量得分 **必须 ≥ 85%**
- 低于阈值的 PR **必须失败**

#### ADR-975_4_2 硬失败项
以下情况 **直接失败，不参与加权**：
- 任一链接失效
- 任一代码示例不可编译
- ADR 缺失必备章节

#### ADR-975_4_3 软失败项
- 时效性、可查找性允许 warning，但必须生成 Issue

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-975 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-975_1_1** | L1 | 文档扫描质量指标定义 | §ADR-975_1_1 |
| **ADR-975_1_2** | L1 | 文档扫描核心指标 | §ADR-975_1_2 |
| **ADR-975_1_3** | L1 | 文档扫描计算公式 | §ADR-975_1_3 |
| **ADR-975_1_4** | L1 | 文档扫描质量等级 | §ADR-975_1_4 |
| **ADR-975_1_5** | L1 | 文档扫描核心原则 | §ADR-975_1_5 |
| **ADR-975_2_1** | L1 | 文档扫描自动化检测 | §ADR-975_2_1 |
| **ADR-975_2_2** | L1 | 文档扫描链接有效性检测 | §ADR-975_2_2 |
| **ADR-975_2_3** | L1 | 文档扫描代码示例可编译性检测 | §ADR-975_2_3 |
| **ADR-975_2_4** | L1 | 文档扫描文档完整性检测 | §ADR-975_2_4 |
| **ADR-975_2_5** | L1 | 文档扫描时效性检测 | §ADR-975_2_5 |
| **ADR-975_2_6** | L1 | 文档扫描核心原则 | §ADR-975_2_6 |
| **ADR-975_3_1** | L1 | 文档扫描定期报告生成 | §ADR-975_3_1 |
| **ADR-975_3_2** | L1 | 文档扫描报告位置 | §ADR-975_3_2 |
| **ADR-975_3_3** | L1 | 文档扫描报告结构 | §ADR-975_3_3 |
| **ADR-975_4_1** | L1 | 文档扫描最低质量阈值 | §ADR-975_4_1 |
| **ADR-975_4_2** | L1 | 文档扫描硬失败项 | §ADR-975_4_2 |
| **ADR-975_4_3** | L2 | 文档扫描软失败项 | §ADR-975_4_3 |

### 执行方式


---

## Non-Goals（明确不管什么）

- 不评估文档内容的业务正确性
- 不评估写作风格、美观程度
- 不作为绩效考核直接依据


## Prohibited（禁止行为）

- 禁止绕过 CI 提交不合格文档
- 禁止手动篡改质量报告
- 禁止在主分支忽略质量失败

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md) - 基于其文档标准

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-955：文档搜索与可发现性优化](../governance/ADR-955-documentation-search-discoverability.md) - 可查找性是质量指标之一

---

## References（非裁决性参考）

### 相关 ADR
- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md)
- [ADR-955：文档搜索与可发现性优化](../governance/ADR-955-documentation-search-discoverability.md)

### 实施工具
- `markdown-link-check` - 链接检查工具
- `scripts/test-code-examples.sh` - 代码编译检查
- `scripts/check-doc-completeness.sh` - 完整性检查
- `scripts/check-doc-staleness.sh` - 时效性检查
- `scripts/generate-quality-report.sh` - 报告生成

### 背景材料
- [ADR-Documentation-Governance-Gap-Analysis.md](../proposals/ADR-Documentation-Governance-Gap-Analysis.md) - 原始提案

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 2.0 | 2026-02-04 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | 架构委员会 |
| 1.0 | 2026-01-29 | 初始版本 |
