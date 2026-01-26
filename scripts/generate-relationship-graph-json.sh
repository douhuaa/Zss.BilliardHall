#!/bin/bash
# 使用 ADR 语义解析器生成关系图
# 本脚本演示如何将解析器集成到现有工具链中

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🔗 使用 ADR 语义解析器生成关系图"
echo ""

# 步骤 1: 批量解析所有 ADR
echo "📂 步骤 1: 批量解析所有 ADR..."
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

dotnet run --project "$REPO_ROOT/src/tools/AdrSemanticParser/AdrParserCli/AdrParserCli.csproj" \
  -- batch "$REPO_ROOT/docs/adr" "$TEMP_DIR/adrs.json" > /dev/null 2>&1

echo "✅ 成功解析 ADR 并保存到: $TEMP_DIR/adrs.json"
echo ""

# 步骤 2: 使用 jq 生成关系统计
if ! command -v jq &> /dev/null; then
    echo "⚠️  未安装 jq，跳过统计分析"
    echo "提示：安装 jq 可以查看详细统计"
    exit 0
fi

echo "📊 步骤 2: 生成关系统计..."
echo ""

# 统计总数
total=$(jq 'length' "$TEMP_DIR/adrs.json")
echo "总 ADR 数量: $total"
echo ""

# 依赖关系统计
echo "🔗 依赖关系分析:"
has_deps=$(jq '[.[] | select(.relationships.dependsOn | length > 0)] | length' "$TEMP_DIR/adrs.json")
echo "  - 有依赖的 ADR: $has_deps"

most_deps=$(jq -r '[.[] | {id: .id, count: (.relationships.dependsOn | length)}] | sort_by(-.count) | .[0] | "\(.id) (\(.count) 个依赖)"' "$TEMP_DIR/adrs.json")
echo "  - 依赖最多的: $most_deps"
echo ""

# 被依赖统计
echo "🎯 被依赖分析:"
has_dependents=$(jq '[.[] | select(.relationships.dependedBy | length > 0)] | length' "$TEMP_DIR/adrs.json")
echo "  - 被依赖的 ADR: $has_dependents"

echo "  - 被依赖最多的 Top 5:"
jq -r '[.[] | {id: .id, title: .title, count: (.relationships.dependedBy | length)}] | 
  sort_by(-.count) | 
  limit(5; .[]) | 
  "    \(.count). \(.id) - \(.title)"' "$TEMP_DIR/adrs.json"
echo ""

# 孤立节点检测
echo "🔍 孤立节点检测:"
isolated=$(jq '[.[] | select(
  (.relationships.dependsOn | length) == 0 and 
  (.relationships.dependedBy | length) == 0 and
  (.relationships.related | length) == 0
)] | length' "$TEMP_DIR/adrs.json")
echo "  - 孤立的 ADR: $isolated"

if [ "$isolated" -gt 0 ]; then
    echo "  - 孤立 ADR 列表:"
    jq -r '.[] | select(
      (.relationships.dependsOn | length) == 0 and 
      (.relationships.dependedBy | length) == 0 and
      (.relationships.related | length) == 0
    ) | "    - \(.id): \(.title)"' "$TEMP_DIR/adrs.json"
fi
echo ""

# 状态分布
echo "📈 状态分布:"
jq -r '.[] | .status' "$TEMP_DIR/adrs.json" | \
  sort | uniq -c | \
  awk '{print "  - " $2 ": " $1 " 个"}' | \
  sed 's/✅ /✅ /' | sed 's/❌ /❌ /'
echo ""

# 步骤 3: 生成 Mermaid 图表（简化版）
echo "📊 步骤 3: 生成 Mermaid 关系图..."
OUTPUT_FILE="$TEMP_DIR/relationship-graph.md"

cat > "$OUTPUT_FILE" << 'EOF'
# ADR 关系图（自动生成）

```mermaid
graph TB
EOF

# 生成依赖关系
jq -r '.[] | 
  select(.relationships.dependsOn | length > 0) | 
  .relationships.dependsOn[] as $dep | 
  "\(.id) --> \($dep.id)"' "$TEMP_DIR/adrs.json" | \
  sed 's/-/_/g' >> "$OUTPUT_FILE"

echo '```' >> "$OUTPUT_FILE"

echo "✅ 关系图已生成: $OUTPUT_FILE"
echo ""

# 显示图表预览
echo "📄 关系图预览（前 20 行）:"
head -20 "$OUTPUT_FILE"
echo ""

echo "✅ 完成！"
echo "💡 提示: 可以将 $OUTPUT_FILE 复制到 Markdown 查看器中查看完整图表"
