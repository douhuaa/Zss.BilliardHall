#!/bin/bash

# ADR 测试覆盖率检查脚本
# 用途：扫描 ADR 文档与对应架构测试的映射关系，识别缺失的测试
# 依据：ADR-0000（架构测试与 CI 治理宪法）

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "======================================"
echo "  ADR 测试覆盖率检查"
echo "======================================"
echo ""
echo "📁 项目路径: $PROJECT_ROOT"
echo "⏰ 检查时间: $(date '+%Y-%m-%d %H:%M:%S')"
echo ""

# 切换到项目根目录
cd "$PROJECT_ROOT"

# ============================================
# 函数定义
# ============================================

# 提取 ADR 编号（支持 ADR-0001 和 ADR-900 格式）
extract_adr_number() {
    local filename="$1"
    echo "$filename" | sed -E 's/.*ADR-0*([0-9]+).*/\1/'
}

# 检查 ADR 是否标注"必须架构测试覆盖"
check_must_have_test() {
    local adr_file="$1"
    if grep -q "必须架构测试覆盖\|【必须架构测试覆盖】" "$adr_file" 2>/dev/null; then
        echo "✅"
    else
        echo "❌"
    fi
}

# 获取 ADR 标题
get_adr_title() {
    local adr_file="$1"
    grep -m1 "^# ADR-" "$adr_file" 2>/dev/null | sed 's/^# ADR-[0-9]*：\?\s*//' || echo "未知标题"
}

# ============================================
# 数据收集
# ============================================

echo "🔍 正在扫描 ADR 文档..."

# 收集所有 ADR 文档（排除特殊文件）
ADR_FILES=$(find docs/adr -type f -name "ADR-*.md" \
    | grep -v "RELATIONSHIP-MAP" \
    | grep -v "903-906" \
    | grep -E "(constitutional|governance|structure|runtime|technical)" \
    | sort)

TOTAL_ADRS=$(echo "$ADR_FILES" | wc -l | tr -d ' ')

echo "🔍 正在扫描架构测试..."

# 收集所有架构测试文件
TEST_FILES=$(find src/tests/ArchitectureTests/ADR -type f -name "ADR_*_Architecture_Tests.cs" 2>/dev/null | sort || echo "")

TOTAL_TESTS=$(echo "$TEST_FILES" | wc -l | tr -d ' ')

echo ""
echo "======================================"
echo "  统计概览"
echo "======================================"
echo ""
echo "📊 ADR 文档总数:    $TOTAL_ADRS"
echo "📊 架构测试总数:    $TOTAL_TESTS"
echo ""

# ============================================
# 按层级统计
# ============================================

echo "======================================"
echo "  按层级统计"
echo "======================================"
echo ""

for category in constitutional governance structure runtime technical; do
    category_adrs=$(find "docs/adr/$category" -name "ADR-*.md" 2>/dev/null | wc -l | tr -d ' ')
    
    # 计算该层级有测试的 ADR 数量
    category_tested=0
    for adr_file in $(find "docs/adr/$category" -name "ADR-*.md" 2>/dev/null); do
        adr_number=$(extract_adr_number "$adr_file")
        # 标准化为4位数字格式
        padded_number=$(printf "%04d" "$adr_number")
        test_file="src/tests/ArchitectureTests/ADR/ADR_${padded_number}_Architecture_Tests.cs"
        
        if [ -f "$test_file" ]; then
            ((category_tested++))
        fi
    done
    
    if [ "$category_adrs" -gt 0 ]; then
        coverage=$((category_tested * 100 / category_adrs))
        echo "📁 $category:"
        echo "   - ADR 总数: $category_adrs"
        echo "   - 已测试: $category_tested"
        echo "   - 覆盖率: ${coverage}%"
        
        if [ "$coverage" -eq 100 ]; then
            echo "   - 状态: ✅ 完全覆盖"
        elif [ "$coverage" -ge 80 ]; then
            echo "   - 状态: ⚠️ 基本覆盖"
        elif [ "$coverage" -ge 50 ]; then
            echo "   - 状态: ⚠️ 部分覆盖"
        else
            echo "   - 状态: ❌ 严重不足"
        fi
        echo ""
    fi
done

# ============================================
# 缺失测试详细列表
# ============================================

echo "======================================"
echo "  缺失测试详细列表"
echo "======================================"
echo ""

missing_count=0
must_test_missing=0

# 按层级分组显示
for category in constitutional governance structure runtime technical; do
    category_missing=0
    category_output=""
    
    for adr_file in $(find "docs/adr/$category" -name "ADR-*.md" 2>/dev/null | sort); do
        adr_number=$(extract_adr_number "$adr_file")
        padded_number=$(printf "%04d" "$adr_number")
        test_file="src/tests/ArchitectureTests/ADR/ADR_${padded_number}_Architecture_Tests.cs"
        
        if [ ! -f "$test_file" ]; then
            ((missing_count++))
            ((category_missing++))
            
            must_test=$(check_must_have_test "$adr_file")
            adr_title=$(get_adr_title "$adr_file")
            
            if [ "$must_test" = "✅" ]; then
                ((must_test_missing++))
                priority="🔴 P0"
            else
                priority="🟡 P2"
            fi
            
            category_output="${category_output}   ❌ ADR-${adr_number}: $adr_title\n"
            category_output="${category_output}      - 标注必须测试: $must_test\n"
            category_output="${category_output}      - 优先级: $priority\n"
            category_output="${category_output}      - 期望文件: $test_file\n"
            category_output="${category_output}\n"
        fi
    done
    
    if [ "$category_missing" -gt 0 ]; then
        echo "📁 $category 层（缺失 $category_missing 个）:"
        echo ""
        echo -e "$category_output"
    fi
done

# ============================================
# 总结报告
# ============================================

echo "======================================"
echo "  总结报告"
echo "======================================"
echo ""

overall_coverage=$((($TOTAL_ADRS - missing_count) * 100 / TOTAL_ADRS))

echo "📊 覆盖率统计:"
echo "   - ADR 文档总数: $TOTAL_ADRS"
echo "   - 已有测试: $(($TOTAL_ADRS - missing_count))"
echo "   - 缺失测试: $missing_count"
echo "   - 整体覆盖率: ${overall_coverage}%"
echo ""

echo "🔴 关键指标:"
echo "   - 标注【必须架构测试覆盖】但缺失: $must_test_missing"
echo ""

# ============================================
# 判定结果
# ============================================

echo "======================================"
echo "  判定结果"
echo "======================================"
echo ""

if [ "$missing_count" -eq 0 ]; then
    echo "✅ 恭喜！所有 ADR 都有对应的架构测试"
    echo ""
    exit 0
elif [ "$must_test_missing" -gt 0 ]; then
    echo "❌ 严重问题：发现 $must_test_missing 个标注【必须架构测试覆盖】的 ADR 缺少测试"
    echo ""
    echo "⚠️  建议："
    echo "   1. 立即为标注【必须架构测试覆盖】的 ADR 补充测试"
    echo "   2. 参考修复计划: docs/reports/adr-test-gap-analysis-2026-01-29.md"
    echo "   3. 使用测试模板: src/tests/ArchitectureTests/ADR/ADR_0001_Architecture_Tests.cs"
    echo ""
    exit 1
elif [ "$overall_coverage" -lt 80 ]; then
    echo "⚠️  警告：ADR 测试覆盖率 ${overall_coverage}% < 80%"
    echo ""
    echo "📋 建议："
    echo "   1. 按照分阶段计划补充测试"
    echo "   2. 优先补充 Runtime 和 Structure 层测试"
    echo "   3. 参考: docs/reports/adr-test-gap-analysis-2026-01-29.md"
    echo ""
    exit 0
else
    echo "✅ ADR 测试覆盖率达标（${overall_coverage}% ≥ 80%）"
    echo ""
    echo "📋 建议："
    echo "   - 继续补充剩余 $missing_count 个测试"
    echo "   - 目标：100% 覆盖"
    echo ""
    exit 0
fi
