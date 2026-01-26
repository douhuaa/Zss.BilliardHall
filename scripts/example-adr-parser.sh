#!/bin/bash
# ADR 语义解析示例脚本
# 演示如何使用 ADR 语义解析器

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🔍 ADR 语义解析器使用示例"
echo ""

# 示例 1: 解析单个 ADR
echo "📖 示例 1: 解析单个 ADR"
echo "--------------------------------------"
dotnet run --project "$REPO_ROOT/src/tools/AdrSemanticParser/AdrParserCli/AdrParserCli.csproj" \
  -- parse "$REPO_ROOT/docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md"

echo ""
echo "--------------------------------------"
echo ""

# 示例 2: 解析并保存为 JSON
echo "📄 示例 2: 解析并保存为 JSON"
echo "--------------------------------------"
OUTPUT_DIR="/tmp/adr-parser-examples"
mkdir -p "$OUTPUT_DIR"

dotnet run --project "$REPO_ROOT/src/tools/AdrSemanticParser/AdrParserCli/AdrParserCli.csproj" \
  -- parse "$REPO_ROOT/docs/adr/governance/ADR-940-adr-relationship-traceability-management.md" \
  "$OUTPUT_DIR/adr-940.json"

echo "JSON 输出已保存到: $OUTPUT_DIR/adr-940.json"
echo ""

# 示例 3: 批量解析所有 ADR
echo "📂 示例 3: 批量解析所有 ADR"
echo "--------------------------------------"
dotnet run --project "$REPO_ROOT/src/tools/AdrSemanticParser/AdrParserCli/AdrParserCli.csproj" \
  -- batch "$REPO_ROOT/docs/adr" "$OUTPUT_DIR/all-adrs.json"

echo ""
echo "批量解析结果已保存到: $OUTPUT_DIR/all-adrs.json"

# 示例 4: 分析 ADR 关系
echo ""
echo "🔗 示例 4: 分析 ADR 关系统计"
echo "--------------------------------------"

if command -v jq &> /dev/null; then
    echo "总 ADR 数量: $(jq 'length' "$OUTPUT_DIR/all-adrs.json")"
    echo ""
    
    echo "有依赖关系的 ADR:"
    jq -r '.[] | select(.relationships.dependsOn | length > 0) | "  - \(.id): \(.relationships.dependsOn | length) 个依赖"' \
      "$OUTPUT_DIR/all-adrs.json"
    echo ""
    
    echo "被依赖最多的 ADR (Top 5):"
    jq -r '.[] | {id: .id, title: .title, count: (.relationships.dependedBy | length)} | select(.count > 0)' \
      "$OUTPUT_DIR/all-adrs.json" | \
      jq -s 'sort_by(-.count) | limit(5; .[]) | "  \(.count). \(.id) - \(.title)"'
    echo ""
    
    echo "状态分布:"
    jq -r '.[] | .status' "$OUTPUT_DIR/all-adrs.json" | \
      sort | uniq -c | \
      awk '{print "  " $2 ": " $1 " 个"}'
else
    echo "⚠️  未安装 jq，跳过统计分析"
    echo "安装 jq 后可以查看更多统计信息"
fi

echo ""
echo "✅ 示例演示完成！"
echo "📁 所有输出文件位于: $OUTPUT_DIR"
