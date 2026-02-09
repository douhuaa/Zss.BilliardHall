# ADR 验证工具套件

本目录包含用于验证 ADR 一致性和质量的自动化工具。

## 工具概览

### 1. ADR 一致性检查器 (`check-adr-consistency.sh`)

**用途**：验证 ADR 文档的格式一致性

**检查项**：
- ✅ Front Matter 完整性（ADR-902 要求）
- ✅ 术语表格式（ADR-006 标准）
- ✅ 版本号格式（ADR-980 要求）
- ✅ 快速参考表（ADR-006 推荐）

**使用方法**：
```bash
./scripts/check-adr-consistency.sh
```

**输出示例**：
```
🔍 开始 ADR 一致性检查...

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
检查 1: Front Matter 完整性
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

❌ ADR-001-modular-monolith-vertical-slice-architecture.md 缺少 Front Matter
...
```

---

### 2. ADR 关系验证器 (`validate-adr-relationships.py`)

**用途**：验证 ADR 之间关系声明的双向一致性

**检查项**：
- ✅ 双向关系一致性（依赖、被依赖、替代等）
- ✅ 循环依赖检测
- ✅ 孤立关系引用（引用不存在的 ADR）

**使用方法**：
```bash
python3 ./scripts/validate-adr-relationships.py
```

**依赖**：Python 3.6+

**输出示例**：
```
🔍 开始 ADR 关系一致性验证...

扫描 ADR 文件...
  • 已解析 ADR-001
  • 已解析 ADR-002
...

✅ 成功解析 46 个 ADR

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
检查 1: 双向关系一致性
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

发现 1 个关系不一致问题：
  ❌ ADR-001 声明依赖 ADR-002，但 ADR-002 未声明被 ADR-001 依赖
```

---

### 3. 术语一致性检查器 (`check-terminology.sh`)

**用途**：验证术语定义的一致性和格式

**检查项**：
- ✅ 提取所有术语定义
- ✅ 查找重复定义的术语
- ✅ 验证术语表格式（英文对照列）

**使用方法**：
```bash
./scripts/check-terminology.sh
```

**输出示例**：
```
🔍 开始 ADR 术语一致性检查...

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
检查 1: 提取所有术语定义
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

✅ 提取了 192 个术语定义

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
检查 2: 查找重复定义的术语
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

⚠️  发现 2 个术语在多个 ADR 中定义：

术语 '模块化单体' 在以下 ADR 中定义：
  • ADR-001-modular-monolith-vertical-slice-architecture.md
  • ADR-006-terminology-numbering-constitution.md
```

---

## CI/CD 集成

### GitHub Actions 集成示例

在 `.github/workflows/adr-validation.yml` 中：

```yaml
name: ADR Validation

on:
  pull_request:
    paths:
      - 'docs/adr/**'
  schedule:
    - cron: '0 0 * * 0'  # 每周日运行

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Check ADR Consistency
        run: ./scripts/check-adr-consistency.sh
        continue-on-error: true
      
      - name: Validate Relationships
        run: python3 scripts/validate-adr-relationships.py
        continue-on-error: true
      
      - name: Check Terminology
        run: ./scripts/check-terminology.sh
        continue-on-error: true
```

---

## 当前发现的问题统计

基于最近的验证运行：

| 检查类别 | 发现问题数 | 严重程度 |
|---------|-----------|---------|
| 缺少 Front Matter | 30 | 🔴 高 |
| 术语表格式不符 | 17 | 🟡 中 |
| 关系声明不一致 | 1 | 🟠 中高 |
| 版本号格式问题 | 0 | ✅ 无 |

---

## 整改优先级

根据项目开发实践经验：

### P0 问题（7天内修复）
- ✅ 宪法层 ADR (0001-0008) 缺少 Front Matter
- ✅ 术语表缺少英文对照列

### P1 问题（14天内修复）
- ✅ 双向关系声明不一致
- ✅ 关系格式不符合 ADR-940 标准

### P2 问题（30天内修复）
- ✅ 其他层级 ADR 缺少 Front Matter
- ✅ 版本历史记录不完整

---

## 工具开发指南

### 添加新的检查项

1. 确定检查的 ADR 依据（如 ADR-902、ADR-940 等）
2. 在相应脚本中添加新的检查函数
3. 更新本 README 文档
4. 提交 PR 并标注 `adr-tooling` 标签

### 脚本编写规范

- ✅ 使用颜色输出提高可读性
- ✅ 提供清晰的错误消息和修复建议
- ✅ 支持 CI/CD 环境（返回正确的退出码）
- ✅ 包含进度指示和统计信息
- ✅ 引用相关 ADR 编号

---

## 故障排查

### 问题：脚本权限不足

**解决**：
```bash
chmod +x scripts/check-adr-consistency.sh
chmod +x scripts/check-terminology.sh
```

### 问题：Python 脚本找不到 ADR 目录

**解决**：确保在项目根目录运行脚本
```bash
cd /path/to/Zss.BilliardHall
python3 ./scripts/validate-adr-relationships.py
```

### 问题：颜色代码在某些终端不显示

**解决**：某些 CI 环境可能不支持 ANSI 颜色，这不影响功能

---

## 相关资源

- 📘 **相关文档**：[ADR-970 自动化工具日志集成标准](../docs/adr/governance/ADR-970-automation-log-integration-standard.md)
- 📘 [ADR-902: ADR 标准模板与结构契约](../docs/adr/governance/ADR-902-adr-template-structure-contract.md)
- 📘 [ADR-940: ADR 关系与溯源管理](../docs/adr/governance/ADR-940-adr-relationship-traceability-management.md)
- 📘 [ADR-980: ADR 生命周期同步](../docs/adr/governance/ADR-980-adr-lifecycle-synchronization.md)

---

**维护者**：架构委员会  
**最后更新**：2026-01-29  
**状态**：✅ Active
