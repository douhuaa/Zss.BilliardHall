#!/bin/bash
# ADR-947 专用 Guard：关系声明区的结构与解析安全规则
# 根据 ADR-947 实现三大条款验证
# 依据 ADR-970.2 支持 JSON 输出
#
# 用法：
#   ./verify-adr-947-compliance.sh [--format text|json] [--output FILE]
#
# 示例：
#   ./verify-adr-947-compliance.sh
#   ./verify-adr-947-compliance.sh --format json
#   ./verify-adr-947-compliance.sh --format json --output docs/reports/adr-947-compliance.json

set -eo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ADR_DIR="$REPO_ROOT/docs/adr"

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
            echo "用法: $0 [--format text|json] [--output FILE]"
            echo ""
            echo "选项:"
            echo "  --format FORMAT    输出格式：text（默认）或 json"
            echo "  --output FILE      输出到文件（仅在 json 格式时有效）"
            echo "  --help             显示帮助信息"
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
    json_start "verify-adr-947-compliance" "1.0.0" "adr-947-validation"
fi

if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "🔍 ADR-947 合规性检查..."
    echo ""
fi

# 启用调试模式
if [ "${DEBUG:-}" = "1" ]; then
    set -x
fi

errors=0
warnings=0

# ============================================================================
# 条款 1：唯一顶级关系区原则
# ============================================================================
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "📋 检查条款 1：唯一顶级关系区原则..."
fi

while IFS= read -r adr_file; do
    adr_name=$(basename "$adr_file")
    
    # 计算 "## 关系声明" 出现次数（必须精确匹配行首）
    count=$(grep -c "^## 关系声明" "$adr_file" 2>/dev/null || true)
    
    if [ "$count" -gt 1 ]; then
        if [ "$OUTPUT_FORMAT" = "text" ]; then
            echo "❌ 违反条款 1：$adr_name"
            echo "   发现 $count 个顶级关系声明区（只允许 1 个）"
            echo "   修复：删除重复的 ## 关系声明，模板示例使用 ### 或更低级别"
            echo ""
        fi
        if [ "$OUTPUT_FORMAT" = "json" ]; then
            json_add_detail "Clause_1_Multiple_Relationship_Sections" "ADR-947" "error" \
                "发现 $count 个顶级关系声明区（只允许 1 个），修复：删除重复的 ## 关系声明" \
                "$adr_file" "" "docs/adr/governance/ADR-947-relationship-section-structure-parsing-safety.md"
        fi
        errors=$((errors + 1))
    else
        if [ "$OUTPUT_FORMAT" = "json" ]; then
            json_add_detail "Clause_1_Unique_Section_${adr_name}" "ADR-947" "info" \
                "仅有唯一顶级关系声明区" \
                "$adr_file" "" ""
        fi
    fi
    
    # 检查是否在模板/示例中使用了 ## 关系声明
    # 检测模式：代码块内出现 "## 关系声明"
    if grep -q '```.*## 关系声明' "$adr_file" 2>/dev/null; then
        if [ "$OUTPUT_FORMAT" = "text" ]; then
            echo "⚠️  警告条款 1：$adr_name"
            echo "   代码块中使用了 ## 关系声明（建议使用占位符或降级）"
            echo ""
        fi
        if [ "$OUTPUT_FORMAT" = "json" ]; then
            json_add_detail "Clause_1_Codeblock_Warning" "ADR-947" "warning" \
                "代码块中使用了 ## 关系声明（建议使用占位符或降级）" \
                "$adr_file" "" "docs/adr/governance/ADR-947-relationship-section-structure-parsing-safety.md"
        fi
        warnings=$((warnings + 1))
    fi
done < <(find "$ADR_DIR" -name "ADR-*.md" -not -name "README.md" -not -path "*/proposals/*" 2>/dev/null | sort)

# ============================================================================
# 条款 2：关系区边界即标题边界
# ============================================================================
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "📋 检查条款 2：关系区边界即标题边界..."
fi

while IFS= read -r adr_file; do
    adr_name=$(basename "$adr_file")
    
    # 简化检查：如果文件包含 ## 关系声明，检查其后是否有 ### （在下一个 ## 之前）
    if grep -q "^## 关系声明" "$adr_file" 2>/dev/null; then
        # 使用 awk 一次性完成检查
        has_subheading=$(awk '
            /^## 关系声明/ { in_section=1; next }
            /^## / && in_section { exit }
            /^###/ && in_section { print "yes"; exit }
        ' "$adr_file" 2>/dev/null)
        
        if [ "$has_subheading" = "yes" ]; then
            if [ "$OUTPUT_FORMAT" = "text" ]; then
                echo "❌ 违反条款 2：$adr_name"
                echo "   关系声明区内包含子标题（###）"
                echo "   修复：子标题应移到关系声明区外"
                echo ""
            fi
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "Clause_2_Subheadings_${adr_name}" "ADR-947" "error" \
                    "关系声明区内包含子标题（###），应移到关系声明区外" \
                    "$adr_file" "" "docs/adr/governance/ADR-947-relationship-section-structure-parsing-safety.md"
            fi
            errors=$((errors + 1))
        fi
    fi
done < <(find "$ADR_DIR" -name "ADR-*.md" -not -name "README.md" -not -path "*/proposals/*" 2>/dev/null | sort)

# ============================================================================
# 条款 3：禁止显式循环声明
# ============================================================================
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "📋 检查条款 3：禁止显式循环声明..."
fi

# 临时文件
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

DEPS_FILE="$TEMP_DIR/dependencies.txt"

# 提取所有依赖关系
while IFS= read -r adr_file; do
    adr_filename=$(basename "$adr_file" .md)
    adr_id=$(echo "$adr_filename" | grep -oE 'ADR-[0-9]+' 2>/dev/null || echo "")
    
    [ -z "$adr_id" ] && continue
    
    # 简化：直接使用 grep 提取依赖关系，不使用 awk
    if grep -q "^## 关系声明" "$adr_file" 2>/dev/null; then
        # 提取依赖关系
        grep -A 20 "^\*\*依赖（Depends On）\*\*" "$adr_file" 2>/dev/null | \
            head -20 | \
            { grep -oE 'ADR-[0-9]+' 2>/dev/null || true; } | \
            while read -r dep_id; do
                # 跳过当前ADR自己的编号
                [ "$dep_id" = "$adr_id" ] && continue
                echo "$adr_id|$dep_id" >> "$DEPS_FILE"
            done
    fi
done < <(find "$ADR_DIR" -name "ADR-*.md" -not -name "README.md" -not -path "*/proposals/*" 2>/dev/null | sort)

# 检测简单的双向依赖（A->B 且 B->A）
if [ -f "$DEPS_FILE" ] && [ -s "$DEPS_FILE" ]; then
    while IFS='|' read -r from to; do
        [ -z "$from" ] && continue
        
        # 检查是否存在反向依赖
        if grep -q "^${to}|${from}$" "$DEPS_FILE" 2>/dev/null; then
            if [ "$OUTPUT_FORMAT" = "text" ]; then
                echo "❌ 违反条款 5：检测到显式循环声明"
                echo "   $from ↔ $to (双向依赖)"
                echo "   修复：保留单向依赖，另一侧改为相关关系"
                echo ""
            fi
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "Clause_5_Circular_Dependency" "ADR-947" "error" \
                    "检测到显式循环声明：$from ↔ $to (双向依赖)，修复：保留单向依赖，另一侧改为相关关系" \
                    "" "" "docs/adr/governance/ADR-947-relationship-section-structure-parsing-safety.md"
            fi
            errors=$((errors + 1))
        fi
    done < "$DEPS_FILE"
fi

# ============================================================================
# 汇总报告
# ============================================================================
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "================================"
    echo "检查完成！"
    echo ""
fi

if [ $errors -gt 0 ]; then
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo "❌ ADR-947 合规性检查失败：发现 $errors 个错误"
        [ $warnings -gt 0 ] && echo "⚠️  发现 $warnings 个警告"
        echo ""
        echo "修复指南："
        echo "1. 条款 1：确保每个 ADR 只有一个 ## 关系声明"
        echo "2. 条款 2：关系区内仅包含列表，不含子标题或段落"
        echo "3. 条款 3：避免双向依赖，使用单向+相关关系"
        echo ""
        echo "参考：docs/adr/governance/ADR-947-relationship-section-structure-parsing-safety.md"
    else
        status=$(json_determine_status)
        if [ -n "$OUTPUT_FILE" ]; then
            json_save "$status" "$OUTPUT_FILE"
        else
            json_finalize "$status"
        fi
    fi
    exit 1
else
    if [ $warnings -gt 0 ]; then
        if [ "$OUTPUT_FORMAT" = "text" ]; then
            echo "⚠️  ADR-947 合规性检查通过（有 $warnings 个警告）"
            echo ""
            echo "建议查看警告并考虑优化"
        else
            status=$(json_determine_status)
            if [ -n "$OUTPUT_FILE" ]; then
                json_save "$status" "$OUTPUT_FILE"
            else
                json_finalize "$status"
            fi
        fi
        exit 0
    else
        if [ "$OUTPUT_FORMAT" = "text" ]; then
            echo "✅ ADR-947 合规性检查通过"
            echo "   所有 ADR 文档符合关系声明区结构与解析安全规则"
        else
            status=$(json_determine_status)
            if [ -n "$OUTPUT_FILE" ]; then
                json_save "$status" "$OUTPUT_FILE"
            else
                json_finalize "$status"
            fi
        fi
        exit 0
    fi
fi
