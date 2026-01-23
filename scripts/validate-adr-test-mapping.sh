#!/bin/bash

# ADR-测试映射一致性校验工具
#
# 此脚本用于验证 ADR 文档与架构测试之间的一致性，确保：
# 1. 每条 ADR 中标记为【必须架构测试覆盖】的条款都有对应的测试
# 2. 每个测试方法都正确引用了对应的 ADR 编号和条款
# 3. 测试失败消息包含正确的 ADR 引用

set -e

# 定义路径
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
ADR_PATH="$REPO_ROOT/docs/adr"
TESTS_PATH="$REPO_ROOT/src/tests/ArchitectureTests/ADR"
PROMPTS_PATH="$REPO_ROOT/docs/copilot"

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

# 输出函数
function log_success() { echo -e "${GREEN}✅ $1${NC}"; }
function log_warning() { echo -e "${YELLOW}⚠️  $1${NC}"; }
function log_error() { echo -e "${RED}❌ $1${NC}"; }
function log_info() { echo -e "${CYAN}ℹ️  $1${NC}"; }

# 统计变量
TOTAL_ADRS=0
TOTAL_REQUIREMENTS=0
REQUIREMENTS_WITH_TESTS=0
REQUIREMENTS_WITHOUT_TESTS=0
TOTAL_TESTS=0
TESTS_WITH_ADR_REF=0
TESTS_WITHOUT_ADR_REF=0
IS_VALID=true

# 查找 ADR 文件
function find_adr_files() {
    find "$ADR_PATH" -type f -name "ADR-*.md" | sort
}

# 查找测试文件
function find_test_files() {
    if [ -d "$TESTS_PATH" ]; then
        find "$TESTS_PATH" -type f -name "ADR_*.cs" | sort
    fi
}

# 提取 ADR 编号
function extract_adr_number() {
    local file="$1"
    basename "$file" | sed -n 's/^ADR-\?\([0-9]\{4\}\).*/\1/p'
}

# 提取测试文件编号
function extract_test_number() {
    local file="$1"
    basename "$file" | sed -n 's/^ADR_\([0-9]\{4\}\).*/\1/p'
}

# 提取 ADR 要求
function extract_adr_requirements() {
    local file="$1"
    local adr_number="$2"
    local count=0
    
    # 临时文件，移除代码块后的内容
    local temp_file=$(mktemp)
    
    # 移除代码块（```...```）中的内容，避免误计数示例代码
    awk '
        /^```/ { 
            in_code_block = !in_code_block
            next
        }
        !in_code_block { print }
    ' "$file" > "$temp_file"
    
    # 查找标记为【必须架构测试覆盖】的条款（排除代码块后）
    local marked=$(grep -c "【必须架构测试覆盖】\|【必须测试】\|\[MUST_TEST\]" "$temp_file" 2>/dev/null || true)
    if [ -n "$marked" ] && [ "$marked" != "0" ]; then
        count=$((count + marked))
    fi
    
    # 清理临时文件
    rm -f "$temp_file"
    
    # 查找快速参考表中的约束（简单计数表格行）
    if grep -q "##\s*快速参考" "$file" 2>/dev/null; then
        local table_lines=$(sed -n '/##.*快速参考/,/##/p' "$file" | grep -c "^|" 2>/dev/null || true)
        # 减去表头和分隔线
        if [ -n "$table_lines" ] && [ "$table_lines" -gt 2 ]; then
            count=$((count + table_lines - 2))
        fi
    fi
    
    echo "$count"
}

# 提取测试断言
function extract_test_assertions() {
    local file="$1"
    local adr_number="$2"
    
    if [ ! -f "$file" ]; then
        echo "0 0"
        return
    fi
    
    # 计数测试方法（查找 [Fact] 或 [Theory]，匹配完整的属性语法）
    local total_methods=$(grep -cE '^\s*\[(Fact|Theory)(\(|])'  "$file" 2>/dev/null || true)
    if [ -z "$total_methods" ]; then
        total_methods=0
    fi
    
    # 计数有 ADR 引用的方法（在代码或 DisplayName 中）
    local methods_with_ref=$(grep -cE "(ADR-$adr_number|ADR_$adr_number)" "$file" 2>/dev/null || true)
    if [ -z "$methods_with_ref" ]; then
        methods_with_ref=0
    fi
    
    echo "$total_methods $methods_with_ref"
}

# 主验证函数
function validate_mapping() {
    log_info "开始 ADR-测试映射验证..."
    echo ""
    
    # 获取所有 ADR 文件
    local adr_files=()
    while IFS= read -r file; do
        adr_files+=("$file")
    done < <(find_adr_files)
    
    TOTAL_ADRS=${#adr_files[@]}
    
    # 获取所有测试文件
    local test_files=()
    while IFS= read -r file; do
        test_files+=("$file")
    done < <(find_test_files)
    
    log_info "发现 $TOTAL_ADRS 个 ADR 文档"
    log_info "发现 ${#test_files[@]} 个测试文件"
    echo ""
    
    # 构建测试文件映射
    declare -A test_file_map
    for test_file in "${test_files[@]}"; do
        local test_num=$(extract_test_number "$test_file")
        if [ -n "$test_num" ]; then
            test_file_map["$test_num"]="$test_file"
        fi
    done
    
    # 验证每个 ADR
    for adr_file in "${adr_files[@]}"; do
        local adr_number=$(extract_adr_number "$adr_file")
        
        if [ -z "$adr_number" ]; then
            continue
        fi
        
        echo -e "${GRAY}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
        log_info "检查 ADR-$adr_number ($(basename "$adr_file"))"
        
        # 提取 ADR 要求
        local req_count=$(extract_adr_requirements "$adr_file" "$adr_number")
        TOTAL_REQUIREMENTS=$((TOTAL_REQUIREMENTS + req_count))
        
        if [ "$req_count" -eq 0 ]; then
            log_warning "  未发现标记为【必须架构测试覆盖】的条款"
        else
            log_info "  发现 $req_count 条必须测试的约束"
        fi
        
        # 检查是否有对应的测试文件（只有在有标记约束时才需要）
        if [ "$req_count" -gt 0 ] && [ -z "${test_file_map[$adr_number]}" ]; then
            log_error "  缺少测试文件: ADR_${adr_number}_Architecture_Tests.cs"
            IS_VALID=false
            REQUIREMENTS_WITHOUT_TESTS=$((REQUIREMENTS_WITHOUT_TESTS + req_count))
            echo ""
            continue
        fi
        
        # 如果没有标记约束，跳过测试文件检查
        if [ "$req_count" -eq 0 ]; then
            echo ""
            continue
        fi
        
        # 提取测试断言
        local test_file="${test_file_map[$adr_number]}"
        local test_stats=$(extract_test_assertions "$test_file" "$adr_number")
        local total_methods=$(echo "$test_stats" | cut -d' ' -f1)
        local methods_with_ref=$(echo "$test_stats" | cut -d' ' -f2)
        
        # 确保变量有默认值
        total_methods=${total_methods:-0}
        methods_with_ref=${methods_with_ref:-0}
        
        TOTAL_TESTS=$((TOTAL_TESTS + total_methods))
        TESTS_WITH_ADR_REF=$((TESTS_WITH_ADR_REF + methods_with_ref))
        
        local methods_without_ref=$((total_methods - methods_with_ref))
        TESTS_WITHOUT_ADR_REF=$((TESTS_WITHOUT_ADR_REF + methods_without_ref))
        
        log_info "  发现 $total_methods 个测试方法"
        
        # 检查测试方法是否都有 ADR 引用
        if [ "$methods_without_ref" -gt 0 ]; then
            log_warning "  $methods_without_ref 个测试方法可能缺少 ADR 引用"
            IS_VALID=false
        else
            log_success "  所有测试方法都包含 ADR 引用"
        fi
        
        # 简单检查：如果有要求但测试数量为 0，标记为问题
        if [ "$req_count" -gt 0 ] && [ "$total_methods" -eq 0 ]; then
            log_error "  ADR 有 $req_count 条约束需要测试，但未发现任何测试方法"
            REQUIREMENTS_WITHOUT_TESTS=$((REQUIREMENTS_WITHOUT_TESTS + req_count))
            IS_VALID=false
        else
            REQUIREMENTS_WITH_TESTS=$((REQUIREMENTS_WITH_TESTS + req_count))
        fi
        
        echo ""
    done
    
    # 输出总结
    echo -e "${GRAY}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""
    echo -e "${CYAN}📊 验证总结${NC}"
    echo -e "${GRAY}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""
    echo "ADR 文档统计："
    echo "  总 ADR 数：$TOTAL_ADRS"
    echo "  总约束条款数：$TOTAL_REQUIREMENTS"
    echo -e "  有测试覆盖：${GREEN}$REQUIREMENTS_WITH_TESTS${NC}"
    if [ "$REQUIREMENTS_WITHOUT_TESTS" -gt 0 ]; then
        echo -e "  缺少测试：${RED}$REQUIREMENTS_WITHOUT_TESTS${NC}"
    else
        echo -e "  缺少测试：${GREEN}$REQUIREMENTS_WITHOUT_TESTS${NC}"
    fi
    echo ""
    echo "测试文件统计："
    echo "  总测试方法数：$TOTAL_TESTS"
    echo -e "  有 ADR 引用：${GREEN}$TESTS_WITH_ADR_REF${NC}"
    if [ "$TESTS_WITHOUT_ADR_REF" -gt 0 ]; then
        echo -e "  缺少 ADR 引用：${RED}$TESTS_WITHOUT_ADR_REF${NC}"
    else
        echo -e "  缺少 ADR 引用：${GREEN}$TESTS_WITHOUT_ADR_REF${NC}"
    fi
    echo ""
    
    if [ "$IS_VALID" = true ]; then
        log_success "验证通过：ADR 文档与测试映射一致！"
    else
        log_error "验证失败：发现 ADR-测试映射不一致问题"
        echo ""
        echo -e "${YELLOW}请执行以下操作：${NC}"
        echo "  1. 为缺少测试的 ADR 约束编写对应的架构测试"
        echo "  2. 为缺少 ADR 引用的测试方法添加正确的 ADR 编号"
        echo "  3. 确保测试失败消息包含 ADR 引用（格式：ADR-XXXX 违规：...）"
        echo ""
        echo -e "${CYAN}参考文档：${NC}"
        echo "  - docs/adr/governance/ADR-0000-architecture-tests.md"
        echo "  - docs/copilot/README.md"
        echo ""
        return 1
    fi
    
    return 0
}

# 主执行
validate_mapping
exit $?
