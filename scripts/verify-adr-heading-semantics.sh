#!/bin/bash
# 验证 ADR 标题语义约束
# 根据 ADR-946 实现

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ADR_DIR="$REPO_ROOT/docs/adr"

echo "🔍 验证 ADR 标题语义约束（ADR-946）..."
echo ""

errors=0

# 检查代码块中的 ## 关系声明 等语义块
echo "检查代码块中的语义块标题..."

while IFS= read -r adr_file; do
    filename=$(basename "$adr_file" .md)
    
    # 跳过非 ADR 文件
    [[ ! "$filename" =~ ^ADR-[0-9]+ ]] && continue
    
    # 提取 markdown 代码块内容并检查 ## 语义块
    if grep -Pzo '(?s)```markdown.*?```' "$adr_file" 2>/dev/null | grep -q "^## 关系声明\|^## 决策\|^## 执法模型"; then
        echo "❌ ADR-946.2 违规：$filename"
        echo "   代码块中使用了 ## 语义块标题"
        echo "   建议：改为英文（## Relationships Example）或降级为 ### 示例"
        echo ""
        ((errors++))
    fi
    
done < <(find "$ADR_DIR" -type f -name "ADR-*.md")

echo "================================"
echo "检查完成！"
echo ""

if [ $errors -eq 0 ]; then
    echo "✅ 所有 ADR 标题语义约束检查通过"
    exit 0
else
    echo "❌ 检查失败：发现 $errors 个标题语义违规"
    echo ""
    echo "修复建议："
    echo "1. 代码块中的模板标题改为英文或占位符"
    echo "2. 模板示例使用 ### 级别标题"
    echo "3. 确保每个语义块在正文中只出现一次"
    echo ""
    echo "参考：ADR-946 - ADR 标题级别即语义级别约束"
    exit 1
fi
