#!/bin/bash

# FindRepositoryRoot 自动化重构脚本
# 使用正则表达式批量替换

echo "========================================"
echo "FindRepositoryRoot 自动化重构工具"
echo "========================================"
echo ""

TEST_DIR="src/tests/ArchitectureTests"
TARGET_NAMESPACE="Zss.BilliardHall.Tests.ArchitectureTests.Shared"
FILES_MODIFIED=0
CALLS_REPLACED=0
METHODS_REMOVED=0

# 查找所有包含 FindRepositoryRoot 的文件（排除 TestEnvironment.cs）
FILES=$(grep -rl "FindRepositoryRoot" "$TEST_DIR" --include="*.cs" | grep -v "TestEnvironment.cs")

echo "找到 $(echo "$FILES" | wc -l) 个文件需要处理"
echo ""

for FILE in $FILES; do
    RELATIVE_PATH="${FILE#./}"
    FILE_MODIFIED=0
    
    # 检查是否包含 FindRepositoryRoot() 调用
    if grep -q "FindRepositoryRoot()" "$FILE"; then
        echo "处理: $RELATIVE_PATH"
        
        # 统计调用次数
        COUNT=$(grep -o "FindRepositoryRoot()" "$FILE" | wc -l)
        echo "  找到 $COUNT 个 FindRepositoryRoot() 调用"
        
        # 替换 FindRepositoryRoot() 为 TestEnvironment.RepositoryRoot
        sed -i 's/FindRepositoryRoot()/TestEnvironment.RepositoryRoot/g' "$FILE"
        CALLS_REPLACED=$((CALLS_REPLACED + COUNT))
        FILE_MODIFIED=1
        
        # 检查是否有方法定义需要删除
        if grep -q "private static string? FindRepositoryRoot()" "$FILE"; then
            echo "  删除 FindRepositoryRoot 方法定义"
            METHODS_REMOVED=$((METHODS_REMOVED + 1))
        fi
        
        # 删除 FindRepositoryRoot 方法定义（包括整个方法体）
        # 使用 Perl 正则处理多行匹配
        perl -i -0pe 's/\n    \/\/ =+ 辅助方法 =+\n\n    private static string\? FindRepositoryRoot\(\)(\n|.)*?\n    \}/\n/gs' "$FILE"
        perl -i -0pe 's/\n    private static string\? FindRepositoryRoot\(\)(\n|.)*?\n    \}/\n/gs' "$FILE"
        
        # 添加 using 声明（如果不存在）
        if ! grep -q "using $TARGET_NAMESPACE" "$FILE"; then
            # 在最后一个 using 之后添加
            sed -i "/^using /a using $TARGET_NAMESPACE;" "$FILE" | head -1
            echo "  添加 using $TARGET_NAMESPACE"
        fi
        
        FILES_MODIFIED=$((FILES_MODIFIED + 1))
        echo "  ✓ 文件已更新"
        echo ""
    fi
done

echo "========================================"
echo "重构完成统计"
echo "========================================"
echo "修改文件数: $FILES_MODIFIED"
echo "替换调用数: $CALLS_REPLACED"
echo "删除方法数: $METHODS_REMOVED"
echo ""
echo "重构工具执行完成！"
