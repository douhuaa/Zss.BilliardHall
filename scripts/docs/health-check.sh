#!/bin/bash
# 文档健康度检查脚本
# 用途：定期检查文档链接有效性、结构完整性和内容时效性

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo "======================================"
echo "文档健康度检查"
echo "======================================"
echo ""

# 计数器
passed_checks=0
failed_checks=0
warnings=0

# 辅助函数
check_pass() {
    echo -e "${GREEN}✓${NC} $1"
    ((passed_checks++))
}

check_fail() {
    echo -e "${RED}✗${NC} $1"
    ((failed_checks++))
}

check_warn() {
    echo -e "${YELLOW}⚠${NC} $1"
    ((warnings++))
}

# 1. 检查核心文档是否存在
echo "1. 核心文档存在性检查"
echo "-----------------------------------"

core_docs=(
    "docs/index.md"
    "docs/README.md"
    "docs/QUICK-START.md"
    "docs/TESTING-GUIDE.md"
    "docs/DOCUMENTATION-MAINTENANCE.md"
)

for doc in "${core_docs[@]}"; do
    if [ -f "$doc" ]; then
        check_pass "核心文档存在: $doc"
    else
        check_fail "核心文档缺失: $doc"
    fi
done

echo ""
echo "======================================"
echo "健康度报告"
echo "======================================"
echo "通过: $passed_checks | 失败: $failed_checks | 警告: $warnings"

if [ $failed_checks -eq 0 ]; then
    echo -e "${GREEN}健康度: 良好${NC}"
    exit 0
else
    echo -e "${RED}健康度: 需要改进${NC}"
    exit 1
fi
