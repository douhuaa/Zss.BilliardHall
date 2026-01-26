#!/bin/bash
# 检查 ADR 关系双向一致性
# 根据 ADR-940.3 实现

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ADR_DIR="$REPO_ROOT/docs/adr"

echo "🔍 检查 ADR 关系双向一致性..."
echo ""

# 临时文件
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

DEPENDENCIES_FILE="$TEMP_DIR/dependencies.txt"
SUPERSEDES_FILE="$TEMP_DIR/supersedes.txt"

errors=0

# 提取所有依赖关系
while IFS= read -r adr_file; do
    adr_id=$(basename "$adr_file" .md)
    
    if grep -q "^## 关系声明" "$adr_file"; then
        # 提取 "依赖（Depends On）" 列表
        sed -n '/## 关系声明/,/^##/p' "$adr_file" | \
            sed -n '/\*\*依赖（Depends On）\*\*/,/\*\*被依赖/p' | \
            grep -oE 'ADR-[0-9]+' | \
            while read -r dep_id; do
                echo "$adr_id|DEPENDS_ON|$dep_id" >> "$DEPENDENCIES_FILE"
            done
        
        # 提取 "被依赖（Depended By）" 列表
        sed -n '/## 关系声明/,/^##/p' "$adr_file" | \
            sed -n '/\*\*被依赖（Depended By）\*\*/,/\*\*替代/p' | \
            grep -oE 'ADR-[0-9]+' | \
            while read -r dep_id; do
                echo "$adr_id|DEPENDED_BY|$dep_id" >> "$DEPENDENCIES_FILE"
            done
        
        # 提取 "替代（Supersedes）" 列表
        sed -n '/## 关系声明/,/^##/p' "$adr_file" | \
            sed -n '/\*\*替代（Supersedes）\*\*/,/\*\*被替代/p' | \
            grep -oE 'ADR-[0-9]+' | \
            while read -r sup_id; do
                echo "$adr_id|SUPERSEDES|$sup_id" >> "$SUPERSEDES_FILE"
            done
        
        # 提取 "被替代（Superseded By）" 列表
        sed -n '/## 关系声明/,/^##/p' "$adr_file" | \
            sed -n '/\*\*被替代（Superseded By）\*\*/,/\*\*相关/p' | \
            grep -oE 'ADR-[0-9]+' | \
            while read -r sup_id; do
                echo "$adr_id|SUPERSEDED_BY|$sup_id" >> "$SUPERSEDES_FILE"
            done
    fi
done < <(find "$ADR_DIR" -name "ADR-*.md" -not -name "README.md" -not -path "*/proposals/*" | sort)

# 检查依赖关系双向一致性
if [ -f "$DEPENDENCIES_FILE" ]; then
    echo "检查依赖关系双向一致性..."
    
    # A DEPENDS_ON B => B should have DEPENDED_BY A
    grep "DEPENDS_ON" "$DEPENDENCIES_FILE" | while IFS='|' read -r from rel to; do
        # 检查反向关系
        if ! grep -q "^${to}|DEPENDED_BY|${from}$" "$DEPENDENCIES_FILE"; then
            echo "❌ 依赖关系不一致："
            echo "   $from 依赖 $to"
            echo "   但 $to 未声明被 $from 依赖"
            echo "   请在 $to.md 的关系声明中添加：$from"
            echo ""
            ((errors++))
        fi
    done
    
    # B DEPENDED_BY A => A should have DEPENDS_ON B
    grep "DEPENDED_BY" "$DEPENDENCIES_FILE" | while IFS='|' read -r from rel to; do
        if ! grep -q "^${to}|DEPENDS_ON|${from}$" "$DEPENDENCIES_FILE"; then
            echo "❌ 被依赖关系不一致："
            echo "   $from 声明被 $to 依赖"
            echo "   但 $to 未声明依赖 $from"
            echo "   请在 $to.md 的关系声明中添加：$from"
            echo ""
            ((errors++))
        fi
    done
fi

# 检查替代关系双向一致性
if [ -f "$SUPERSEDES_FILE" ]; then
    echo "检查替代关系双向一致性..."
    
    # A SUPERSEDES B => B should have SUPERSEDED_BY A
    grep "SUPERSEDES" "$SUPERSEDES_FILE" | while IFS='|' read -r from rel to; do
        if ! grep -q "^${to}|SUPERSEDED_BY|${from}$" "$SUPERSEDES_FILE"; then
            echo "❌ 替代关系不一致："
            echo "   $from 替代 $to"
            echo "   但 $to 未声明被 $from 替代"
            echo "   请在 $to.md 的关系声明中添加：$from"
            echo ""
            ((errors++))
        fi
    done
    
    # B SUPERSEDED_BY A => A should have SUPERSEDES B
    grep "SUPERSEDED_BY" "$SUPERSEDES_FILE" | while IFS='|' read -r from rel to; do
        if ! grep -q "^${to}|SUPERSEDES|${from}$" "$SUPERSEDES_FILE"; then
            echo "❌ 被替代关系不一致："
            echo "   $from 声明被 $to 替代"
            echo "   但 $to 未声明替代 $from"
            echo "   请在 $to.md 的关系声明中添加：$from"
            echo ""
            ((errors++))
        fi
    done
fi

echo "================================"
echo "检查完成！"
echo ""

if [ $errors -gt 0 ]; then
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
    exit 1
else
    echo "✅ 所有关系都满足双向一致性要求"
    exit 0
fi
