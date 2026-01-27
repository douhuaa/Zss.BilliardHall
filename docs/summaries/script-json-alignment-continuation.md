# 脚本 JSON 输出对齐实施总结（续）

> **依据**：[ADR-970：自动化工具日志集成标准](../adr/governance/ADR-970-automation-log-integration-standard.md)  
> **日期**：2026-01-27  
> **状态**：持续进行中 🔄

---

## 概述

本文档记录了从 commit 3237cbc 继续的脚本 JSON 对齐工作。基于已有的 JSON 输出基础设施，继续将更多验证脚本对齐到 ADR-970 标准。

---

## 本次 PR 新增的脚本对齐

### 1. validate-adr-test-mapping.sh ✅

**功能**：验证 ADR 文档与架构测试之间的一致性

**更新内容**：
- 添加 --format 和 --output 参数支持
- 集成 json-output.sh 库
- 为所有检查添加 JSON 详情条目
- 保持向后兼容的文本输出模式

**验证**：
```bash
# 文本模式
./scripts/validate-adr-test-mapping.sh

# JSON 模式
./scripts/validate-adr-test-mapping.sh --format json | jq '.summary'
# 输出：{ "total": 9, "passed": 9, "failed": 0, "warnings": 0 }
```

---

### 2. verify-adr-heading-semantics.sh ✅

**功能**：验证 ADR 标题语义约束（ADR-946）

**更新内容**：
- 添加 JSON 输出支持
- 修复 set -euo pipefail 导致的早退问题
- 检查代码块中的语义块标题使用

**关键技术点**：
- 使用 `set -eo pipefail` 而非 `set -euo pipefail` 避免未定义变量问题
- 使用 `has_violation` 标志而非直接依赖命令退出码
- 使用 `2>/dev/null` 抑制 grep 错误输出

**验证**：
```bash
./scripts/verify-adr-heading-semantics.sh --format json | jq '.summary'
# 输出：{ "total": 41, "passed": 40, "failed": 1, "warnings": 0 }
```

---

### 3. verify-adr-relationships.sh ✅

**功能**：验证 ADR 关系声明章节（ADR-940.1）

**更新内容**：
- 添加 JSON 输出支持
- 验证关系声明章节存在性
- 验证关系声明章节位置
- 验证所有必需的子章节（依赖、被依赖、替代、被替代、相关）

**验证**：
```bash
./scripts/verify-adr-relationships.sh --format json | jq '.summary'
# 输出：{ "total": 41, "passed": 41, "failed": 0, "warnings": 0 }
```

---

## 标准实施模式

所有脚本遵循统一的对齐模式：

### 1. 文件头和参数解析

```bash
#!/bin/bash
# [脚本功能描述]
# 依据 ADR-970.2 支持 JSON 输出
#
# 用法：
#   ./script-name.sh [--format text|json] [--output FILE]

set -eo pipefail  # 注意：不使用 -u，避免未定义变量导致早退

# 路径定义
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# 输出格式和路径
OUTPUT_FORMAT="text"
OUTPUT_FILE=""

# 解析参数
while [[ $# -gt 0 ]]; do
    case $1 in
        --format)
            OUTPUT_FORMAT="$2"
            shift 2
            ;;
        --output)
            OUTPUT_FILE="$2"
            shift 2
            ;;
        --help)
            # 显示帮助信息
            exit 0
            ;;
        *)
            echo "未知选项: $1"
            exit 1
            ;;
    esac
done

# 加载 JSON 输出库（如果使用 JSON 格式）
if [ "$OUTPUT_FORMAT" = "json" ]; then
    source "$SCRIPT_DIR/lib/json-output.sh"
    json_start "script-name" "1.0.0" "validation-type"
fi
```

### 2. 条件输出

```bash
# 条件输出函数
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "开始验证..."
fi
```

### 3. 主验证逻辑

```bash
# 主验证逻辑
errors=0
while IFS= read -r file; do
    # 避免使用 set -e 导致循环提前退出
    has_error=false
    if some_check "$file" 2>/dev/null; then
        has_error=true
    fi
    
    if [ "$has_error" = true ]; then
        if [ "$OUTPUT_FORMAT" = "text" ]; then
            echo "❌ 错误：..."
        fi
        if [ "$OUTPUT_FORMAT" = "json" ]; then
            json_add_detail "Test_Name" "ADR-XXXX" "error" \
                "错误消息" \
                "$file" "" \
                "docs/adr/path/to/adr.md"
        fi
        errors=$((errors + 1))
    else
        if [ "$OUTPUT_FORMAT" = "json" ]; then
            json_add_detail "Test_Name" "ADR-XXXX" "info" \
                "检查通过" \
                "$file" "" \
                "docs/adr/path/to/adr.md"
        fi
    fi
done < <(find_files)
```

### 4. 输出总结和退出码

```bash
# 输出总结
if [ "$OUTPUT_FORMAT" = "text" ]; then
    if [ $errors -eq 0 ]; then
        echo "✅ 验证通过"
    else
        echo "❌ 验证失败：$errors 个错误"
    fi
else
    # JSON 输出
    status=$(json_determine_status)
    if [ -n "$OUTPUT_FILE" ]; then
        json_save "$status" "$OUTPUT_FILE"
    else
        json_finalize "$status"
    fi
fi

# 退出码
if [ $errors -eq 0 ]; then
    exit 0
else
    exit 1
fi
```

---

## 关键技术要点

### 1. 避免提前退出

**问题**：使用 `set -euo pipefail` 时，未定义变量或命令失败会导致脚本立即退出，无法完成 JSON 输出。

**解决方案**：
- 使用 `set -eo pipefail` 而非 `set -euo pipefail`
- 在循环中使用标志变量而非直接依赖退出码
- 使用 `2>/dev/null` 抑制命令错误输出
- 使用 `|| true` 或条件判断避免命令失败导致脚本退出

### 2. JSON 详情记录原则

每个检查都应该添加 JSON 详情：
- 成功：使用 `severity="info"`
- 警告：使用 `severity="warning"`
- 错误：使用 `severity="error"`

始终提供：
- ADR 编号（如适用）
- 修复指南链接
- 文件路径和行号（如适用）

### 3. 向后兼容

- 默认保持文本输出（不指定 --format 时）
- 仅在明确指定 --format json 时使用 JSON
- 所有现有调用无需修改

---

## 进度统计

**已对齐脚本**：5/13 (38%)
- validate-adr-consistency.sh ✅ (前期)
- validate-three-way-mapping.sh ✅ (前期)
- validate-adr-test-mapping.sh ✅ (本PR)
- verify-adr-heading-semantics.sh ✅ (本PR)
- verify-adr-relationships.sh ✅ (本PR)

**待对齐脚本**：8/13 (62%)

优先级 P0：
- validate-governance-compliance.sh

优先级 P1：
- verify-adr-947-compliance.sh
- validate-adr-version-sync.sh

优先级 P2：
- check-relationship-consistency.sh
- detect-circular-dependencies.sh
- generate-health-report.sh
- verify-all.sh
- adr-cli.sh

---

## 测试结果

所有新对齐的脚本都经过测试：

```bash
# validate-adr-test-mapping.sh
./scripts/validate-adr-test-mapping.sh
# ✅ 文本模式通过

./scripts/validate-adr-test-mapping.sh --format json | jq '.summary'
# ✅ JSON 模式通过，输出有效 JSON

# verify-adr-heading-semantics.sh
./scripts/verify-adr-heading-semantics.sh
# ✅ 文本模式通过

./scripts/verify-adr-heading-semantics.sh --format json | jq '.summary'
# ✅ JSON 模式通过

# verify-adr-relationships.sh
./scripts/verify-adr-relationships.sh
# ✅ 文本模式通过

./scripts/verify-adr-relationships.sh --format json | jq '.summary'
# ✅ JSON 模式通过
```

---

## 后续工作

### 短期（本周内）
1. 对齐 validate-governance-compliance.sh
2. 对齐 validate-adr-version-sync.sh
3. 对齐 verify-adr-947-compliance.sh

### 中期（1-2 周）
1. 完成所有 P2 脚本对齐
2. 更新相关文档
3. 在 CI/CD 中试点 JSON 输出

### 长期（1-2 月）
1. 实现 CI Workflow 自动化
2. 开发报告可视化工具
3. 建立趋势监控

---

## 参考资源

- [原实施总结](./script-json-alignment-implementation.md) - 前期工作总结
- [ADR-970：自动化工具日志集成标准](../adr/governance/ADR-970-automation-log-integration-standard.md)
- [scripts/lib/json-output.sh](../../scripts/lib/json-output.sh) - JSON 输出库
- [scripts/README.md](../../scripts/README.md) - 工具使用指南

---

**维护**：架构委员会  
**更新日期**：2026-01-27  
**状态**：✅ 进行中（5/13 已完成，+3 本PR）
