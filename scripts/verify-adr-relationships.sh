#!/bin/bash
# 验证 ADR 关系声明
# 根据 ADR-940.1 实现：检查所有 ADR 是否包含关系声明章节

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ADR_DIR="$REPO_ROOT/docs/adr"

echo "🔍 验证 ADR 关系声明章节..."
echo ""

errors=0
warnings=0

# 查找所有 ADR 文件（排除 README 和 proposals）
while IFS= read -r adr_file; do
    adr_name=$(basename "$adr_file")
    
    # 检查是否包含关系声明章节
    if ! grep -q "^## 关系声明（Relationships）" "$adr_file" && \
       ! grep -q "^## 关系声明" "$adr_file"; then
        echo "❌ 错误：$adr_name 缺少关系声明章节"
        echo "   位置应在'决策'章节之后"
        echo "   参考：ADR-940.1"
        echo ""
        ((errors++))
    else
        # 检查关系声明章节的位置（应在决策章节之后）
        decision_line=$(grep -n "^## 决策" "$adr_file" | head -1 | cut -d: -f1 || echo "0")
        relationship_line=$(grep -n "^## 关系声明" "$adr_file" | head -1 | cut -d: -f1 || echo "0")
        
        if [ "$decision_line" -gt 0 ] && [ "$relationship_line" -gt 0 ]; then
            if [ "$relationship_line" -lt "$decision_line" ]; then
                echo "⚠️  警告：$adr_name 关系声明章节位置不正确"
                echo "   当前行：$relationship_line，决策章节行：$decision_line"
                echo "   关系声明章节应在决策章节之后"
                echo ""
                ((warnings++))
            fi
        fi
        
        # 检查是否包含所有必需的子章节
        if ! grep -A 100 "^## 关系声明" "$adr_file" | grep -q "**依赖（Depends On）**"; then
            echo "⚠️  警告：$adr_name 缺少'依赖（Depends On）'子章节"
            ((warnings++))
        fi
        
        if ! grep -A 100 "^## 关系声明" "$adr_file" | grep -q "**被依赖（Depended By）**"; then
            echo "⚠️  警告：$adr_name 缺少'被依赖（Depended By）'子章节"
            ((warnings++))
        fi
        
        if ! grep -A 100 "^## 关系声明" "$adr_file" | grep -q "**替代（Supersedes）**"; then
            echo "⚠️  警告：$adr_name 缺少'替代（Supersedes）'子章节"
            ((warnings++))
        fi
        
        if ! grep -A 100 "^## 关系声明" "$adr_file" | grep -q "**被替代（Superseded By）**"; then
            echo "⚠️  警告：$adr_name 缺少'被替代（Superseded By）'子章节"
            ((warnings++))
        fi
        
        if ! grep -A 100 "^## 关系声明" "$adr_file" | grep -q "**相关（Related）**"; then
            echo "⚠️  警告：$adr_name 缺少'相关（Related）'子章节"
            ((warnings++))
        fi
    fi
done < <(find "$ADR_DIR" -name "ADR-*.md" -not -name "README.md" -not -name "ADR-RELATIONSHIP-MAP.md" -not -path "*/proposals/*" | sort)

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
    exit 1
elif [ $warnings -gt 0 ]; then
    echo "⚠️  验证通过但存在 $warnings 个警告"
    echo "建议修复警告以提高文档质量"
    exit 0
else
    echo "✅ 所有 ADR 都包含正确的关系声明章节"
    exit 0
fi
