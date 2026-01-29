#!/bin/bash
# 生成 ADR 关系图
# 根据 ADR-940 实现

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ADR_DIR="$REPO_ROOT/docs/adr"
OUTPUT_FILE="$ADR_DIR/ADR-RELATIONSHIP-MAP.md"

echo "🔍 扫描 ADR 关系声明..."

# 临时文件
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

RELATIONSHIPS_FILE="$TEMP_DIR/relationships.txt"
ADR_LIST_FILE="$TEMP_DIR/adr_list.txt"

# 查找所有 ADR 文件（排除 README 和 proposals）
find "$ADR_DIR" -name "ADR-*.md" -not -name "README.md" -not -path "*/proposals/*" | sort > "$ADR_LIST_FILE"

# 提取关系
while IFS= read -r adr_file; do
    adr_id=$(basename "$adr_file" | sed 's/\.md$//')
    
    # 提取关系声明章节
    if grep -qE "## 关系声明|## Relationships" "$adr_file"; then
        # 提取依赖关系
        sed -n '/## 关系声明\|## Relationships/,/^##/p' "$adr_file" | \
            grep -A 10 "\*\*依赖（Depends On）\*\*\|\*\*Depends On\*\*" | \
            grep -E "\[ADR-[0-9]+" | \
            sed "s/.*\[ADR-\([0-9]*\).*/DEPENDS|$adr_id|ADR-\1/" >> "$RELATIONSHIPS_FILE" || true
        
        # 提取替代关系
        sed -n '/## 关系声明\|## Relationships/,/^##/p' "$adr_file" | \
            grep -A 10 "\*\*替代（Supersedes）\*\*\|\*\*Supersedes\*\*" | \
            grep -E "\[ADR-[0-9]+" | \
            sed "s/.*\[ADR-\([0-9]*\).*/SUPERSEDES|$adr_id|ADR-\1/" >> "$RELATIONSHIPS_FILE" || true
        
        # 提取相关关系
        sed -n '/## 关系声明\|## Relationships/,/^##/p' "$adr_file" | \
            grep -A 10 "\*\*相关（Related）\*\*\|\*\*Related\*\*" | \
            grep -E "\[ADR-[0-9]+" | \
            sed "s/.*\[ADR-\([0-9]*\).*/RELATED|$adr_id|ADR-\1/" >> "$RELATIONSHIPS_FILE" || true
    fi
done < "$ADR_LIST_FILE"

# 生成关系图文档
cat > "$OUTPUT_FILE" << 'EOF'
# ADR 关系图（ADR Relationship Map）

> 🤖 **本文件由 `scripts/generate-adr-relationship-map.sh` 自动生成**  
> 📅 **生成时间**：$(date '+%Y-%m-%d %H:%M:%S')  
> 🎯 **依据**：ADR-940 - ADR 关系与溯源管理宪法

---

## 全局关系图（Global Relationship Map）

```mermaid
graph TB
EOF

# 生成 Mermaid 图表
echo "" >> "$OUTPUT_FILE"

# 定义颜色方案
echo "    %% 颜色方案" >> "$OUTPUT_FILE"
echo "    classDef constitutional fill:#90EE90" >> "$OUTPUT_FILE"
echo "    classDef governance fill:#87CEEB" >> "$OUTPUT_FILE"
echo "    classDef structure fill:#FFD700" >> "$OUTPUT_FILE"
echo "    classDef runtime fill:#FFA07A" >> "$OUTPUT_FILE"
echo "    classDef technical fill:#DDA0DD" >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"

# 添加节点和关系
if [ -f "$RELATIONSHIPS_FILE" ]; then
    # 添加依赖关系
    grep "^DEPENDS" "$RELATIONSHIPS_FILE" | while IFS='|' read -r type from to; do
        from_node=$(echo "$from" | tr '-' '_' | tr '[:lower:]' '[:upper:]')
        to_node=$(echo "$to" | tr '-' '_' | tr '[:lower:]' '[:upper:]')
        echo "    $from_node --> $to_node" >> "$OUTPUT_FILE"
    done
    
    # 添加替代关系（用虚线）
    grep "^SUPERSEDES" "$RELATIONSHIPS_FILE" | while IFS='|' read -r type from to; do
        from_node=$(echo "$from" | tr '-' '_' | tr '[:lower:]' '[:upper:]')
        to_node=$(echo "$to" | tr '-' '_' | tr '[:lower:]' '[:upper:]')
        echo "    $from_node -.替代.-> $to_node" >> "$OUTPUT_FILE"
    done
fi

echo '```' >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"

# 生成关系表格
cat >> "$OUTPUT_FILE" << 'EOF'

---

## 关系列表（Relationship List）

### 按 ADR 分组

EOF

while IFS= read -r adr_file; do
    adr_id=$(basename "$adr_file" | sed 's/\.md$//')
    adr_title=$(head -1 "$adr_file" | sed 's/^# //')
    
    # 提取状态
    status=$(grep "^\*\*状态\*\*" "$adr_file" | head -1 | sed 's/.*：//' || echo "未知")
    
    echo "#### $adr_title" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"
    echo "**状态**：$status" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"
    
    # 检查是否有关系声明
    if grep -qE "## 关系声明|## Relationships" "$adr_file"; then
        # 提取依赖
        depends=$(sed -n '/## 关系声明\|## Relationships/,/^##/p' "$adr_file" | \
                  grep -A 10 "\*\*依赖（Depends On）\*\*\|\*\*Depends On\*\*" | \
                  grep -E "\[ADR-[0-9]+" | \
                  sed 's/^[[:space:]]*//' || echo "")
        
        # 提取被依赖
        depended=$(sed -n '/## 关系声明\|## Relationships/,/^##/p' "$adr_file" | \
                   grep -A 10 "\*\*被依赖（Depended By）\*\*\|\*\*Depended By\*\*" | \
                   grep -E "\[ADR-[0-9]+" | \
                   sed 's/^[[:space:]]*//' || echo "")
        
        # 提取替代
        supersedes=$(sed -n '/## 关系声明\|## Relationships/,/^##/p' "$adr_file" | \
                     grep -A 10 "\*\*替代（Supersedes）\*\*\|\*\*Supersedes\*\*" | \
                     grep -E "\[ADR-[0-9]+" | \
                     sed 's/^[[:space:]]*//' || echo "")
        
        # 提取被替代
        superseded=$(sed -n '/## 关系声明\|## Relationships/,/^##/p' "$adr_file" | \
                     grep -A 10 "\*\*被替代（Superseded By）\*\*\|\*\*Superseded By\*\*" | \
                     grep -E "\[ADR-[0-9]+" | \
                     sed 's/^[[:space:]]*//' || echo "")
        
        # 提取相关
        related=$(sed -n '/## 关系声明\|## Relationships/,/^##/p' "$adr_file" | \
                  grep -A 10 "\*\*相关（Related）\*\*\|\*\*Related\*\*" | \
                  grep -E "\[ADR-[0-9]+" | \
                  sed 's/^[[:space:]]*//' || echo "")
        
        echo "**依赖（Depends On）**：" >> "$OUTPUT_FILE"
        if [ -n "$depends" ]; then
            echo "$depends" >> "$OUTPUT_FILE"
        else
            echo "- 无" >> "$OUTPUT_FILE"
        fi
        echo "" >> "$OUTPUT_FILE"
        
        echo "**被依赖（Depended By）**：" >> "$OUTPUT_FILE"
        if [ -n "$depended" ]; then
            echo "$depended" >> "$OUTPUT_FILE"
        else
            echo "- 无" >> "$OUTPUT_FILE"
        fi
        echo "" >> "$OUTPUT_FILE"
        
        echo "**替代（Supersedes）**：" >> "$OUTPUT_FILE"
        if [ -n "$supersedes" ]; then
            echo "$supersedes" >> "$OUTPUT_FILE"
        else
            echo "- 无" >> "$OUTPUT_FILE"
        fi
        echo "" >> "$OUTPUT_FILE"
        
        echo "**被替代（Superseded By）**：" >> "$OUTPUT_FILE"
        if [ -n "$superseded" ]; then
            echo "$superseded" >> "$OUTPUT_FILE"
        else
            echo "- 无" >> "$OUTPUT_FILE"
        fi
        echo "" >> "$OUTPUT_FILE"
        
        echo "**相关（Related）**：" >> "$OUTPUT_FILE"
        if [ -n "$related" ]; then
            echo "$related" >> "$OUTPUT_FILE"
        else
            echo "- 无" >> "$OUTPUT_FILE"
        fi
        echo "" >> "$OUTPUT_FILE"
    else
        echo "⚠️ **缺少关系声明章节** - 不符合 ADR-940 要求" >> "$OUTPUT_FILE"
        echo "" >> "$OUTPUT_FILE"
    fi
    
    echo "---" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"
done < "$ADR_LIST_FILE"

# 添加统计信息
cat >> "$OUTPUT_FILE" << EOF

## 统计信息（Statistics）

- **ADR 总数**：$(wc -l < "$ADR_LIST_FILE")
- **包含关系声明的 ADR**：$(while IFS= read -r f; do grep -qE "## 关系声明|## Relationships" "$f" && echo 1; done < "$ADR_LIST_FILE" | wc -l)
- **缺少关系声明的 ADR**：$(while IFS= read -r f; do grep -qE "## 关系声明|## Relationships" "$f" || echo 1; done < "$ADR_LIST_FILE" | wc -l)
- **依赖关系数**：$(grep -c "^DEPENDS" "$RELATIONSHIPS_FILE" 2>/dev/null || echo 0)
- **替代关系数**：$(grep -c "^SUPERSEDES" "$RELATIONSHIPS_FILE" 2>/dev/null || echo 0)
- **相关关系数**：$(grep -c "^RELATED" "$RELATIONSHIPS_FILE" 2>/dev/null || echo 0)

---

**生成时间**：$(date '+%Y-%m-%d %H:%M:%S')  
**维护**：架构委员会  
**状态**：🤖 自动生成
EOF

echo "✅ 关系图已生成：$OUTPUT_FILE"

# 显示警告信息
missing_count=$(while IFS= read -r f; do grep -qE "## 关系声明|## Relationships" "$f" || echo 1; done < "$ADR_LIST_FILE" | wc -l)
if [ "$missing_count" -gt 0 ]; then
    echo ""
    echo "⚠️  警告：$missing_count 个 ADR 缺少关系声明章节"
    echo "请运行 'scripts/verify-adr-relationships.sh' 查看详情"
fi
