#!/bin/bash
# 检查 ADR 关系双向一致性
# 根据 ADR-940.3 实现
# 依据 ADR-970.2 支持 JSON 输出
#
# 用法：
#   ./check-relationship-consistency.sh [--format text|json] [--output FILE]
#
# 示例：
#   ./check-relationship-consistency.sh
#   ./check-relationship-consistency.sh --format json
#   ./check-relationship-consistency.sh --format json --output docs/reports/architecture-tests/relationship-consistency.json

set -eo pipefail

# 获取脚本目录，处理 BASH_SOURCE[0] 为空的情况（如在 GitHub Actions 中）
SCRIPT_PATH="${BASH_SOURCE[0]:-$0}"
SCRIPT_DIR="$(cd "$(dirname "$SCRIPT_PATH")" && pwd)"
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
    json_start "check-relationship-consistency" "1.0.0" "validation"
fi

if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "🔍 检查 ADR 关系双向一致性..."
    echo ""
fi

# Enable debug if DEBUG env var is set
if [ "${DEBUG:-}" = "1" ]; then
    set -x
fi

# 临时文件
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

DEPENDENCIES_FILE="$TEMP_DIR/dependencies.txt"
SUPERSEDES_FILE="$TEMP_DIR/supersedes.txt"

errors=0

# 提取所有依赖关系
while IFS= read -r adr_file; do
    adr_filename=$(basename "$adr_file" .md)
    # Extract just the ADR number (e.g., ADR-0001 from ADR-0001-modular-monolith-...)
    adr_id=$(echo "$adr_filename" | grep -oE 'ADR-[0-9]+' || echo "")
    
    # Skip files without valid ADR numeric IDs (like ADR-RELATIONSHIP-MAP)
    [ -z "$adr_id" ] && continue
    
    if grep -qE "^## 关系声明|^## Relationships" "$adr_file"; then
        # 提取 "依赖（Depends On）" 或 "Depends On" 列表
        sed -n '/^## 关系声明\|^## Relationships/,/^##/p' "$adr_file" | \
            sed -n '/\*\*依赖（Depends On）\*\*\|\*\*Depends On\*\*/,/\*\*被依赖\|\*\*Depended By\|\*\*替代\|\*\*Supersedes\|\*\*被替代\|\*\*Superseded By\|\*\*相关\|\*\*Related/p' | \
            grep -v '^\*\*被依赖\|\*\*Depended By\|\*\*替代\|\*\*Supersedes\|\*\*被替代\|\*\*Superseded By\|\*\*相关\|\*\*Related' | \
            { grep -oE 'ADR-[0-9]+' || true; } | \
            while read -r dep_id; do
                echo "$adr_id|DEPENDS_ON|$dep_id" >> "$DEPENDENCIES_FILE"
            done
        
        # 提取 "被依赖（Depended By）" 或 "Depended By" 列表
        sed -n '/^## 关系声明\|^## Relationships/,/^##/p' "$adr_file" | \
            sed -n '/\*\*被依赖（Depended By）\*\*\|\*\*Depended By\*\*/,/\*\*替代\|\*\*Supersedes\|\*\*被替代\|\*\*Superseded By\|\*\*相关\|\*\*Related/p' | \
            grep -v '^\*\*替代\|\*\*Supersedes\|\*\*被替代\|\*\*Superseded By\|\*\*相关\|\*\*Related' | \
            { grep -oE 'ADR-[0-9]+' || true; } | \
            while read -r dep_id; do
                echo "$adr_id|DEPENDED_BY|$dep_id" >> "$DEPENDENCIES_FILE"
            done
        
        # 提取 "替代（Supersedes）" 或 "Supersedes" 列表
        sed -n '/^## 关系声明\|^## Relationships/,/^##/p' "$adr_file" | \
            sed -n '/\*\*替代（Supersedes）\*\*\|\*\*Supersedes/,/\*\*被替代\|\*\*Superseded By\|\*\*相关\|\*\*Related/p' | \
            grep -v '^\*\*被替代\|\*\*Superseded By\|\*\*相关\|\*\*Related' | \
            { grep -oE 'ADR-[0-9]+' || true; } | \
            while read -r sup_id; do
                echo "$adr_id|SUPERSEDES|$sup_id" >> "$SUPERSEDES_FILE"
            done
        
        # 提取 "被替代（Superseded By）" 或 "Superseded By" 列表
        sed -n '/^## 关系声明\|^## Relationships/,/^##/p' "$adr_file" | \
            sed -n '/\*\*被替代（Superseded By）\*\*\|\*\*Superseded By\*\*/,/\*\*相关\|\*\*Related/p' | \
            grep -v '^\*\*相关\|\*\*Related' | \
            { grep -oE 'ADR-[0-9]+' || true; } | \
            while read -r sup_id; do
                echo "$adr_id|SUPERSEDED_BY|$sup_id" >> "$SUPERSEDES_FILE"
            done
    fi
done < <(find "$ADR_DIR" -name "ADR-*.md" -not -name "README.md" -not -path "*/proposals/*" | sort)

# 检查依赖关系双向一致性
if [ -f "$DEPENDENCIES_FILE" ] && [ -s "$DEPENDENCIES_FILE" ]; then
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo "检查依赖关系双向一致性..."
    fi
    
    ERROR_FILE="$TEMP_DIR/errors.txt"
    
    # A DEPENDS_ON B => B should have DEPENDED_BY A
    while IFS='|' read -r from rel to; do
        [ -z "$from" ] && continue
        # 检查反向关系
        if ! grep -q "^${to}|DEPENDED_BY|${from}$" "$DEPENDENCIES_FILE" 2>/dev/null; then
            if [ "$OUTPUT_FORMAT" = "text" ]; then
                echo "❌ 依赖关系不一致："
                echo "   $from 依赖 $to"
                echo "   但 $to 未声明被 $from 依赖"
                echo "   请在 $to.md 的关系声明中添加：$from"
                echo ""
            fi
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "Dependency_Consistency_Check" "ADR-940.3" "error" \
                    "$from 依赖 $to，但 $to 未声明被 $from 依赖" \
                    "docs/adr/*/*/$to.md" "" \
                    "docs/adr/governance/ADR-940-adr-relationship-management.md"
            fi
            echo "1" >> "$ERROR_FILE"
        else
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "Dependency_Consistency_Check" "ADR-940.3" "info" \
                    "$from 依赖 $to - 双向一致" \
                    "" "" \
                    "docs/adr/governance/ADR-940-adr-relationship-management.md"
            fi
        fi
    done < <({ grep "DEPENDS_ON" "$DEPENDENCIES_FILE" 2>/dev/null || true; })
    
    # B DEPENDED_BY A => A should have DEPENDS_ON B
    while IFS='|' read -r from rel to; do
        [ -z "$from" ] && continue
        if ! grep -q "^${to}|DEPENDS_ON|${from}$" "$DEPENDENCIES_FILE" 2>/dev/null; then
            if [ "$OUTPUT_FORMAT" = "text" ]; then
                echo "❌ 被依赖关系不一致："
                echo "   $from 声明被 $to 依赖"
                echo "   但 $to 未声明依赖 $from"
                echo "   请在 $to.md 的关系声明中添加：$from"
                echo ""
            fi
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "Depended_By_Consistency_Check" "ADR-940.3" "error" \
                    "$from 声明被 $to 依赖，但 $to 未声明依赖 $from" \
                    "docs/adr/*/*/$to.md" "" \
                    "docs/adr/governance/ADR-940-adr-relationship-management.md"
            fi
            echo "1" >> "$ERROR_FILE"
        else
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "Depended_By_Consistency_Check" "ADR-940.3" "info" \
                    "$from 被 $to 依赖 - 双向一致" \
                    "" "" \
                    "docs/adr/governance/ADR-940-adr-relationship-management.md"
            fi
        fi
    done < <({ grep "DEPENDED_BY" "$DEPENDENCIES_FILE" 2>/dev/null || true; })
    
    # Count errors
    if [ -f "$ERROR_FILE" ]; then
        errors=$(wc -l < "$ERROR_FILE")
    fi
fi

# 检查替代关系双向一致性
if [ -f "$SUPERSEDES_FILE" ] && [ -s "$SUPERSEDES_FILE" ]; then
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo "检查替代关系双向一致性..."
    fi
    
    # A SUPERSEDES B => B should have SUPERSEDED_BY A
    while IFS='|' read -r from rel to; do
        [ -z "$from" ] && continue
        if ! grep -q "^${to}|SUPERSEDED_BY|${from}$" "$SUPERSEDES_FILE" 2>/dev/null; then
            if [ "$OUTPUT_FORMAT" = "text" ]; then
                echo "❌ 替代关系不一致："
                echo "   $from 替代 $to"
                echo "   但 $to 未声明被 $from 替代"
                echo "   请在 $to.md 的关系声明中添加：$from"
                echo ""
            fi
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "Supersedes_Consistency_Check" "ADR-940.3" "error" \
                    "$from 替代 $to，但 $to 未声明被 $from 替代" \
                    "docs/adr/*/*/$to.md" "" \
                    "docs/adr/governance/ADR-940-adr-relationship-management.md"
            fi
            echo "1" >> "$ERROR_FILE"
        else
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "Supersedes_Consistency_Check" "ADR-940.3" "info" \
                    "$from 替代 $to - 双向一致" \
                    "" "" \
                    "docs/adr/governance/ADR-940-adr-relationship-management.md"
            fi
        fi
    done < <({ grep "SUPERSEDES" "$SUPERSEDES_FILE" 2>/dev/null || true; })
    
    # B SUPERSEDED_BY A => A should have SUPERSEDES B
    while IFS='|' read -r from rel to; do
        [ -z "$from" ] && continue
        if ! grep -q "^${to}|SUPERSEDES|${from}$" "$SUPERSEDES_FILE" 2>/dev/null; then
            if [ "$OUTPUT_FORMAT" = "text" ]; then
                echo "❌ 被替代关系不一致："
                echo "   $from 声明被 $to 替代"
                echo "   但 $to 未声明替代 $from"
                echo "   请在 $to.md 的关系声明中添加：$from"
                echo ""
            fi
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "Superseded_By_Consistency_Check" "ADR-940.3" "error" \
                    "$from 声明被 $to 替代，但 $to 未声明替代 $from" \
                    "docs/adr/*/*/$to.md" "" \
                    "docs/adr/governance/ADR-940-adr-relationship-management.md"
            fi
            echo "1" >> "$ERROR_FILE"
        else
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "Superseded_By_Consistency_Check" "ADR-940.3" "info" \
                    "$from 被 $to 替代 - 双向一致" \
                    "" "" \
                    "docs/adr/governance/ADR-940-adr-relationship-management.md"
            fi
        fi
    done < <({ grep "SUPERSEDED_BY" "$SUPERSEDES_FILE" 2>/dev/null || true; })
    
    # Count errors
    if [ -f "$ERROR_FILE" ]; then
        errors=$(wc -l < "$ERROR_FILE")
    fi
fi

if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "================================"
    echo "检查完成！"
    echo ""
fi

if [ $errors -gt 0 ]; then
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo "❌ 检查失败：发现 $errors 个双向一致性错误"
        echo ""
        echo "修复建议："
        echo "1. 如果 ADR-A 依赖 ADR-B，则："
        echo "   - ADR-A 的关系声明中：**依赖（Depends On）**: ADR-B"
        echo "   - ADR-B 的关系声明中：**被依赖（Depended By）**: ADR-A"
        echo ""
        echo "2. 如果 ADR-A 替代 ADR-B，则："
        echo "   - ADR-A 的关系声明中：**替代（Supersedes）**: ADR-B"
        echo "   - ADR-B 的关系声明中：**被替代（Superseded By）**: ADR-A"
        echo ""
        echo "参考：ADR-940.3 - 关系双向一致性验证"
    fi
    
    if [ "$OUTPUT_FORMAT" = "json" ]; then
        status=$(json_determine_status)
        if [ -n "$OUTPUT_FILE" ]; then
            json_save "$status" "$OUTPUT_FILE"
        else
            json_finalize "$status"
        fi
    fi
    exit 1
else
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo "✅ 所有关系都满足双向一致性要求"
    fi
    
    if [ "$OUTPUT_FORMAT" = "json" ]; then
        status=$(json_determine_status)
        if [ -n "$OUTPUT_FILE" ]; then
            json_save "$status" "$OUTPUT_FILE"
        else
            json_finalize "$status"
        fi
    fi
    exit 0
fi
