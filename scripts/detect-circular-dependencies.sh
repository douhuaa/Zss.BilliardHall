#!/bin/bash
# 检测 ADR 循环依赖
# 根据 ADR-940.4 实现

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ADR_DIR="$REPO_ROOT/docs/adr"

echo "🔍 检测 ADR 循环依赖..."
echo ""

# 临时文件
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

DEPENDENCIES_FILE="$TEMP_DIR/dependencies.txt"
GRAPH_FILE="$TEMP_DIR/graph.txt"

# 提取所有依赖关系
while IFS= read -r adr_file; do
    adr_id=$(basename "$adr_file" .md)
    
    if grep -q "^## 关系声明" "$adr_file"; then
        # 提取 "依赖（Depends On）" 列表
        sed -n '/## 关系声明/,/^##/p' "$adr_file" | \
            sed -n '/\*\*依赖（Depends On）\*\*/,/\*\*被依赖/p' | \
            grep -oE 'ADR-[0-9]+' | \
            while read -r dep_id; do
                echo "$adr_id $dep_id" >> "$GRAPH_FILE"
            done
    fi
done < <(find "$ADR_DIR" -name "ADR-*.md" -not -name "README.md" -not -path "*/proposals/*" | sort)

if [ ! -f "$GRAPH_FILE" ] || [ ! -s "$GRAPH_FILE" ]; then
    echo "ℹ️  未发现任何依赖关系，无需检查循环依赖"
    exit 0
fi

# 使用 DFS 检测循环依赖
detect_cycle() {
    local graph_file=$1
    local temp_dir=$2
    
    # Python 脚本进行循环检测
    python3 - <<'PYTHON_SCRIPT' "$graph_file" "$temp_dir"
import sys
from collections import defaultdict, deque

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
    cycles = detect_cycles(graph_file)
    
    if cycles:
        print(f"❌ 发现 {len(cycles)} 个循环依赖：")
        print()
        for i, cycle in enumerate(cycles, 1):
            print(f"循环 {i}:")
            print("  " + " -> ".join(cycle))
            print()
        sys.exit(1)
    else:
        print("✅ 未发现循环依赖")
        sys.exit(0)
PYTHON_SCRIPT
}

# 执行循环检测
if detect_cycle "$GRAPH_FILE" "$TEMP_DIR"; then
    echo ""
    echo "================================"
    echo "✅ 检测完成：依赖关系形成有向无环图（DAG）"
    exit 0
else
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
    exit 1
fi
