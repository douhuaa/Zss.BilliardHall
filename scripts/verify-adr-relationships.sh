#!/bin/bash
# 验证 ADR 关系声明
# 依据 ADR-970.2 支持 JSON 输出
# 根据 ADR-940.1 实现：检查所有 ADR 是否包含关系声明章节
#
# 用法：
#   ./verify-adr-relationships.sh [--format text|json] [--output FILE]
#
# 示例：
#   ./verify-adr-relationships.sh
#   ./verify-adr-relationships.sh --format json
#   ./verify-adr-relationships.sh --format json --output docs/reports/architecture-tests/adr-relationships.json

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
    json_start "verify-adr-relationships" "1.0.0" "adr-validation"
fi

if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "🔍 验证 ADR 关系声明章节..."
    echo ""
fi

errors=0
warnings=0

# 查找所有 ADR 文件（排除 README 和 proposals）
while IFS= read -r adr_file; do
    adr_name=$(basename "$adr_file")
    
    # 提取 ADR 编号
    adr_number=$(echo "$adr_name" | sed -n 's/^ADR-\([0-9]\{4\}\).*/\1/p')
    
    # 检查是否包含关系声明章节
    has_relationship_section=false
    if grep -q "^## 关系声明（Relationships）" "$adr_file" 2>/dev/null || \
       grep -q "^## 关系声明" "$adr_file" 2>/dev/null; then
        has_relationship_section=true
    fi
    
    if [ "$has_relationship_section" = false ]; then
        if [ "$OUTPUT_FORMAT" = "text" ]; then
            echo "❌ 错误：$adr_name 缺少关系声明章节"
            echo "   位置应在'决策'章节之后"
            echo "   参考：ADR-940.1"
            echo ""
        fi
        if [ "$OUTPUT_FORMAT" = "json" ]; then
            json_add_detail "Missing_Relationship_Section" "ADR-$adr_number" "error" \
                "缺少关系声明章节，位置应在'决策'章节之后" \
                "$adr_file" "" \
                "docs/adr/governance/ADR-940-adr-relationship-traceability.md"
        fi
        errors=$((errors + 1))
    else
        # 检查关系声明章节的位置（应在决策章节之后）
        decision_line=$(grep -n "^## 决策" "$adr_file" 2>/dev/null | head -1 | cut -d: -f1 || echo "0")
        relationship_line=$(grep -n "^## 关系声明" "$adr_file" 2>/dev/null | head -1 | cut -d: -f1 || echo "0")
        
        if [ "$decision_line" -gt 0 ] && [ "$relationship_line" -gt 0 ]; then
            if [ "$relationship_line" -lt "$decision_line" ]; then
                if [ "$OUTPUT_FORMAT" = "text" ]; then
                    echo "⚠️  警告：$adr_name 关系声明章节位置不正确"
                    echo "   当前行：$relationship_line，决策章节行：$decision_line"
                    echo "   关系声明章节应在决策章节之后"
                    echo ""
                fi
                if [ "$OUTPUT_FORMAT" = "json" ]; then
                    json_add_detail "Relationship_Section_Position" "ADR-$adr_number" "warning" \
                        "关系声明章节位置不正确（当前行：$relationship_line，决策章节行：$decision_line），应在决策章节之后" \
                        "$adr_file" "$relationship_line" \
                        "docs/adr/governance/ADR-940-adr-relationship-traceability.md"
                fi
                warnings=$((warnings + 1))
            fi
        fi
        
        # 检查是否包含所有必需的子章节
        subsections=(
            "依赖（Depends On）"
            "被依赖（Depended By）"
            "替代（Supersedes）"
            "被替代（Superseded By）"
            "相关（Related）"
        )
        
        for subsection in "${subsections[@]}"; do
            if ! grep -A 100 "^## 关系声明" "$adr_file" 2>/dev/null | grep -q "**$subsection**"; then
                if [ "$OUTPUT_FORMAT" = "text" ]; then
                    echo "⚠️  警告：$adr_name 缺少'$subsection'子章节"
                fi
                if [ "$OUTPUT_FORMAT" = "json" ]; then
                    json_add_detail "Missing_Subsection" "ADR-$adr_number" "warning" \
                        "缺少'$subsection'子章节" \
                        "$adr_file" "" \
                        "docs/adr/governance/ADR-940-adr-relationship-traceability.md"
                fi
                warnings=$((warnings + 1))
            fi
        done
        
        # 如果一切正常，在 JSON 模式下添加 info 记录
        if [ "$OUTPUT_FORMAT" = "json" ] && [ "$relationship_line" -ge "$decision_line" ]; then
            json_add_detail "Relationship_Section_Valid" "ADR-$adr_number" "info" \
                "关系声明章节完整且位置正确" \
                "$adr_file" "" \
                "docs/adr/governance/ADR-940-adr-relationship-traceability.md"
        fi
    fi
done < <(find "$ADR_DIR" -name "ADR-*.md" -not -name "README.md" -not -name "ADR-RELATIONSHIP-MAP.md" -not -path "*/proposals/*" 2>/dev/null | sort)

if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "================================"
    echo "验证完成！"
    echo ""
    echo "统计："
    echo "  ❌ 错误：$errors"
    echo "  ⚠️  警告：$warnings"
    echo ""
    
    if [ $errors -gt 0 ]; then
        echo "❌ 验证失败：存在 $errors 个错误"
        echo ""
        echo "修复建议："
        echo "1. 在每个 ADR 的'决策'章节后添加关系声明章节"
        echo "2. 使用以下模板："
        echo ""
        echo "## 关系声明（Relationships）"
        echo ""
        echo "**依赖（Depends On）**："
        echo "- 无"
        echo ""
        echo "**被依赖（Depended By）**："
        echo "- 无"
        echo ""
        echo "**替代（Supersedes）**："
        echo "- 无"
        echo ""
        echo "**被替代（Superseded By）**："
        echo "- 无"
        echo ""
        echo "**相关（Related）**："
        echo "- 无"
        echo ""
        echo "参考：ADR-940 - ADR 关系与溯源管理宪法"
    elif [ $warnings -gt 0 ]; then
        echo "⚠️  验证通过但存在 $warnings 个警告"
        echo "建议修复警告以提高文档质量"
    else
        echo "✅ 所有 ADR 都包含正确的关系声明章节"
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

if [ $errors -gt 0 ]; then
    exit 1
else
    exit 0
fi
