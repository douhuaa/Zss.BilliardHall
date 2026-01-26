#!/bin/bash
# 验证 ADR/测试/Prompt 版本同步
# 根据 ADR-980.1 和 ADR-980.2 实现

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🔍 验证 ADR/测试/Prompt 版本同步..."
echo ""

errors=0
warnings=0

# 临时文件
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

VERSION_FILE="$TEMP_DIR/versions.txt"

# 提取 ADR 版本号
extract_adr_version() {
    local file=$1
    # 提取 **版本**：X.Y 格式
    grep "^\*\*版本\*\*" "$file" | head -1 | sed 's/.*：//' | sed 's/[[:space:]]*$//' || echo ""
}

# 提取测试版本号
extract_test_version() {
    local file=$1
    # 提取 // Version: X.Y 格式
    grep "^// Version:" "$file" | head -1 | sed 's/.*: //' | sed 's/[[:space:]]*$//' || echo ""
}

# 提取 Prompt 版本号
extract_prompt_version() {
    local file=$1
    # 提取 **版本**：X.Y 格式
    grep "^\*\*版本\*\*" "$file" | head -1 | sed 's/.*：//' | sed 's/[[:space:]]*$//' || echo ""
}

# 检查所有 ADR
while IFS= read -r adr_file; do
    adr_filename=$(basename "$adr_file" .md)
    adr_id=$(echo "$adr_filename" | grep -oE 'ADR-[0-9]+' || echo "")
    
    # Skip files without valid ADR numeric IDs (like ADR-RELATIONSHIP-MAP)
    [ -z "$adr_id" ] && continue
    
    adr_number=$(echo "$adr_id" | grep -oE '[0-9]+' | head -1)
    
    # 提取 ADR 版本
    adr_version=$(extract_adr_version "$adr_file")
    
    if [ -z "$adr_version" ]; then
        echo "⚠️  警告：$adr_id 缺少版本号"
        echo "   请在 ADR 元数据中添加：**版本**：X.Y"
        echo ""
        warnings=$((warnings + 1))
        continue
    fi
    
    # 查找对应的架构测试
    test_file="$REPO_ROOT/src/tests/ArchitectureTests/ADR/ADR_${adr_number}_Architecture_Tests.cs"
    test_version=""
    
    if [ -f "$test_file" ]; then
        test_version=$(extract_test_version "$test_file")
        
        if [ -z "$test_version" ]; then
            echo "⚠️  警告：$adr_id 对应的测试文件缺少版本号"
            echo "   文件：$(basename "$test_file")"
            echo "   请在测试类注释中添加：// Version: X.Y"
            echo ""
            warnings=$((warnings + 1))
        elif [ "$adr_version" != "$test_version" ]; then
            echo "❌ 错误：$adr_id 版本不一致"
            echo "   ADR 版本：$adr_version"
            echo "   测试版本：$test_version"
            echo "   请同步版本号"
            echo ""
            errors=$((errors + 1))
        fi
    fi
    
    # 查找对应的 Copilot Prompt
    prompt_file="$REPO_ROOT/docs/copilot/adr-${adr_number}.prompts.md"
    prompt_version=""
    
    if [ -f "$prompt_file" ]; then
        prompt_version=$(extract_prompt_version "$prompt_file")
        
        if [ -z "$prompt_version" ]; then
            echo "⚠️  警告：$adr_id 对应的 Prompt 文件缺少版本号"
            echo "   文件：$(basename "$prompt_file")"
            echo "   请在 Prompt 元数据中添加：**版本**：X.Y"
            echo ""
            warnings=$((warnings + 1))
        elif [ "$adr_version" != "$prompt_version" ]; then
            echo "❌ 错误：$adr_id 版本不一致"
            echo "   ADR 版本：$adr_version"
            echo "   Prompt 版本：$prompt_version"
            echo "   请同步版本号"
            echo ""
            errors=$((errors + 1))
        fi
    fi
    
    # 记录版本信息
    echo "$adr_id|$adr_version|$test_version|$prompt_version" >> "$VERSION_FILE"
    
done < <(find "$REPO_ROOT/docs/adr" -name "ADR-*.md" -not -name "README.md" -not -path "*/proposals/*" | sort)

echo "================================"
echo "验证完成！"
echo ""
echo "统计："
echo "  ❌ 错误：$errors"
echo "  ⚠️  警告：$warnings"
echo ""

if [ $errors -gt 0 ]; then
    echo "❌ 验证失败：存在 $errors 个版本不一致错误"
    echo ""
    echo "修复建议："
    echo ""
    echo "1. ADR 正文版本号格式："
    echo "   **版本**：X.Y"
    echo ""
    echo "2. 架构测试版本号格式："
    echo "   // Version: X.Y"
    echo "   // ADR: ADR-XXXX"
    echo "   public class ADR_XXXX_Architecture_Tests"
    echo ""
    echo "3. Copilot Prompt 版本号格式："
    echo "   **版本**：X.Y"
    echo "   **对应 ADR**：ADR-XXXX-title"
    echo ""
    echo "4. 版本号变更规则："
    echo "   +0.1：小版本（澄清、示例）"
    echo "   +1.0：大版本（新增/修改/删除规则）"
    echo ""
    echo "参考：ADR-980 - ADR 生命周期一体化同步机制"
    exit 1
elif [ $warnings -gt 0 ]; then
    echo "⚠️  验证通过但存在 $warnings 个警告"
    echo "建议添加版本号以支持版本追踪"
    exit 0
else
    echo "✅ 所有 ADR/测试/Prompt 版本同步一致"
    
    # 显示版本摘要
    if [ -f "$VERSION_FILE" ]; then
        echo ""
        echo "版本摘要（前 10 个）："
        echo "ADR | ADR版本 | 测试版本 | Prompt版本"
        echo "--- | ------- | ------- | ----------"
        head -10 "$VERSION_FILE" | while IFS='|' read -r adr adr_v test_v prompt_v; do
            test_v=${test_v:-"N/A"}
            prompt_v=${prompt_v:-"N/A"}
            echo "$adr | $adr_v | $test_v | $prompt_v"
        done
    fi
    
    exit 0
fi
