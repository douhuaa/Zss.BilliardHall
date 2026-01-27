#!/bin/bash
# 验证 ADR 标题语义约束
# 依据 ADR-970.2 支持 JSON 输出
# 根据 ADR-946 实现
#
# 用法：
#   ./verify-adr-heading-semantics.sh [--format text|json] [--output FILE]
#
# 示例：
#   ./verify-adr-heading-semantics.sh
#   ./verify-adr-heading-semantics.sh --format json
#   ./verify-adr-heading-semantics.sh --format json --output docs/reports/architecture-tests/adr-heading-semantics.json

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
    json_start "verify-adr-heading-semantics" "1.0.0" "adr-validation"
fi

if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "🔍 验证 ADR 标题语义约束（ADR-946）..."
    echo ""
fi

errors=0

# 检查代码块中的 ## 关系声明 等语义块
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "检查代码块中的语义块标题..."
fi

while IFS= read -r adr_file; do
    filename=$(basename "$adr_file" .md)
    
    # 跳过非 ADR 文件
    [[ ! "$filename" =~ ^ADR-[0-9]+ ]] && continue
    
    # 提取 ADR 编号
    adr_number=$(echo "$filename" | sed -n 's/^ADR-\([0-9]\{4\}\).*/\1/p')
    
    # 提取 markdown 代码块内容并检查 ## 语义块
    has_violation=false
    if grep -Pzo '(?s)```markdown.*?```' "$adr_file" 2>/dev/null | grep -q "^## 关系声明\|^## 决策\|^## 执法模型" 2>/dev/null; then
        has_violation=true
    fi
    
    if [ "$has_violation" = true ]; then
        if [ "$OUTPUT_FORMAT" = "text" ]; then
            echo "❌ ADR-946.2 违规：$filename"
            echo "   代码块中使用了 ## 语义块标题"
            echo "   建议：改为英文（## Relationships Example）或降级为 ### 示例"
            echo ""
        fi
        if [ "$OUTPUT_FORMAT" = "json" ]; then
            json_add_detail "Semantic_Block_In_Code" "ADR-$adr_number" "error" \
                "代码块中使用了 ## 语义块标题，应改为英文或降级为 ### 示例" \
                "$adr_file" "" \
                "docs/adr/governance/ADR-946-adr-heading-level-semantics.md"
        fi
        errors=$((errors + 1))
    else
        if [ "$OUTPUT_FORMAT" = "json" ]; then
            json_add_detail "Semantic_Block_Validation" "ADR-$adr_number" "info" \
                "标题语义约束检查通过" \
                "$adr_file" "" \
                "docs/adr/governance/ADR-946-adr-heading-level-semantics.md"
        fi
    fi
    
done < <(find "$ADR_DIR" -type f -name "ADR-*.md")

if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "================================"
    echo "检查完成！"
    echo ""
    
    if [ $errors -eq 0 ]; then
        echo "✅ 所有 ADR 标题语义约束检查通过"
    else
        echo "❌ 检查失败：发现 $errors 个标题语义违规"
        echo ""
        echo "修复建议："
        echo "1. 代码块中的模板标题改为英文或占位符"
        echo "2. 模板示例使用 ### 级别标题"
        echo "3. 确保每个语义块在正文中只出现一次"
        echo ""
        echo "参考：ADR-946 - ADR 标题级别即语义级别约束"
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

if [ $errors -eq 0 ]; then
    exit 0
else
    exit 1
fi
