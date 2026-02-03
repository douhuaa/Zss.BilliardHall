#!/bin/bash
# 检测 ADR 循环依赖
# 根据 ADR-940.4 实现
# 依据 ADR-970.2 支持 JSON 输出
#
# 用法：
#   ./detect-circular-dependencies.sh [--format text|json] [--output FILE]
#
# 示例：
#   ./detect-circular-dependencies.sh
#   ./detect-circular-dependencies.sh --format json
#   ./detect-circular-dependencies.sh --format json --output docs/reports/architecture-tests/circular-dependencies.json

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
    json_start "detect-circular-dependencies" "1.0.0" "validation"
fi

if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo "🔍 检测 ADR 循环依赖..."
    echo ""
fi

# 临时文件
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

DEPENDENCIES_FILE="$TEMP_DIR/dependencies.txt"
GRAPH_FILE="$TEMP_DIR/graph.txt"

# 提取所有依赖关系
while IFS= read -r adr_file; do
    adr_filename=$(basename "$adr_file" .md)
    # Extract just the ADR number (e.g., ADR-001 from ADR-001-modular-monolith-...)
    adr_id=$(echo "$adr_filename" | grep -oE 'ADR-[0-9]+' || echo "")
    
    # Skip files without valid ADR numeric IDs (like ADR-RELATIONSHIP-MAP)
    [ -z "$adr_id" ] && continue
    
    if grep -qE "^## 关系声明|^## Relationships" "$adr_file"; then
        # 提取 "依赖（Depends On）" 或 "Depends On" 列表
        sed -n '/## 关系声明\|## Relationships/,/^##/p' "$adr_file" | \
            sed -n '/\*\*依赖（Depends On）\*\*\|\*\*Depends On\*\*/,/\*\*被依赖\|\*\*Depended By/p' | \
            { grep -oE 'ADR-[0-9]+' || true; } | \
            while read -r dep_id; do
                echo "$adr_id $dep_id" >> "$GRAPH_FILE"
            done
    fi
done < <(find "$ADR_DIR" -name "ADR-*.md" -not -name "README.md" -not -path "*/proposals/*" | sort)

if [ ! -f "$GRAPH_FILE" ] || [ ! -s "$GRAPH_FILE" ]; then
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo "ℹ️  未发现任何依赖关系，无需检查循环依赖"
    fi
    if [ "$OUTPUT_FORMAT" = "json" ]; then
        json_add_detail "Circular_Dependency_Check" "ADR-940.4" "info" \
            "未发现任何依赖关系，无需检查循环依赖" \
            "" "" \
            "docs/adr/governance/ADR-940-adr-relationship-management.md"
        status=$(json_determine_status)
        if [ -n "$OUTPUT_FILE" ]; then
            json_save "$status" "$OUTPUT_FILE"
        else
            json_finalize "$status"
        fi
    fi
    exit 0
fi

# 使用 DFS 检测循环依赖
detect_cycle() {
    local graph_file=$1
    local temp_dir=$2
    local output_format=$3
    
    # Python 脚本进行循环检测
    python3 - <<'PYTHON_SCRIPT' "$graph_file" "$temp_dir" "$output_format"
import sys
from collections import defaultdict, deque
import json

def detect_cycles(graph_file):
    # 构建邻接表
    graph = defaultdict(list)
    nodes = set()
    
    with open(graph_file, 'r') as f:
        for line in f:
            parts = line.strip().split()
            if len(parts) == 2:
                from_node, to_node = parts
                graph[from_node].append(to_node)
                nodes.add(from_node)
                nodes.add(to_node)
    
    # DFS 检测循环
    WHITE, GRAY, BLACK = 0, 1, 2
    color = {node: WHITE for node in nodes}
    parent = {node: None for node in nodes}
    cycles = []
    
    def dfs(node, path):
        if color[node] == BLACK:
            return
        if color[node] == GRAY:
            # 发现循环
            cycle_start = path.index(node)
            cycle = path[cycle_start:] + [node]
            cycles.append(cycle)
            return
        
        color[node] = GRAY
        path.append(node)
        
        for neighbor in graph[node]:
            dfs(neighbor, path[:])
        
        color[node] = BLACK
    
    for node in nodes:
        if color[node] == WHITE:
            dfs(node, [])
    
    return cycles

if __name__ == '__main__':
    graph_file = sys.argv[1]
    output_format = sys.argv[3] if len(sys.argv) > 3 else 'text'
    cycles = detect_cycles(graph_file)
    
    if cycles:
        if output_format == 'text':
            print(f"❌ 发现 {len(cycles)} 个循环依赖：")
            print()
            for i, cycle in enumerate(cycles, 1):
                print(f"循环 {i}:")
                print("  " + " -> ".join(cycle))
                print()
        sys.exit(1)
    else:
        if output_format == 'text':
            print("✅ 未发现循环依赖")
        sys.exit(0)
PYTHON_SCRIPT
}

# 执行循环检测
cycle_result=0
if detect_cycle "$GRAPH_FILE" "$TEMP_DIR" "$OUTPUT_FORMAT"; then
    cycle_result=0
    if [ "$OUTPUT_FORMAT" = "json" ]; then
        json_add_detail "Circular_Dependency_Check" "ADR-940.4" "info" \
            "未发现循环依赖，依赖关系形成有向无环图（DAG）" \
            "" "" \
            "docs/adr/governance/ADR-940-adr-relationship-management.md"
    fi
else
    cycle_result=1
    if [ "$OUTPUT_FORMAT" = "json" ]; then
        json_add_detail "Circular_Dependency_Check" "ADR-940.4" "error" \
            "检测到循环依赖，请参考修复建议" \
            "" "" \
            "docs/adr/governance/ADR-940-adr-relationship-management.md"
    fi
fi

if [ $cycle_result -eq 0 ]; then
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo ""
        echo "================================"
        echo "✅ 检测完成：依赖关系形成有向无环图（DAG）"
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
else
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo ""
        echo "================================"
        echo "❌ 检测失败：存在循环依赖"
        echo ""
        echo "解决建议："
        echo "1. 提取公共规则到新 ADR"
        echo "2. 重新设计依赖关系"
        echo "3. 将依赖改为相关关系"
        echo ""
        echo "参考：ADR-940.4 - 循环依赖禁止"
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
fi
