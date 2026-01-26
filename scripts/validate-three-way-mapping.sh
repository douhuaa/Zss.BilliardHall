#!/bin/bash

# 三位一体映射扫描增强工具
# ADR / 架构测试 / Copilot Prompts 映射一致性与 diff 检测
#
# 此脚本用于：
# 1. 验证 ADR、架构测试、Copilot Prompts 三者映射一致性
# 2. 检测变更前后的不一致并生成修正清单
# 3. 发现废弃、未映射、冗余的测试和 Prompts

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
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 输出函数
function log_success() { echo -e "${GREEN}✅ $1${NC}"; }
function log_warning() { echo -e "${YELLOW}⚠️  $1${NC}"; }
function log_error() { echo -e "${RED}❌ $1${NC}"; }
function log_info() { echo -e "${CYAN}ℹ️  $1${NC}"; }
function log_debug() { echo -e "${GRAY}🔍 $1${NC}"; }

# 统计变量
declare -A ADR_MAP
declare -A TEST_MAP
declare -A PROMPT_MAP
declare -a ORPHAN_TESTS
declare -a ORPHAN_PROMPTS
declare -a MISSING_TESTS
declare -a MISSING_PROMPTS
declare -a INCONSISTENT_MAPPINGS

IS_VALID=true

# 提取 ADR 编号（支持 1-4 位数字，自动补齐到 4 位）
function extract_adr_number() {
    local file="$1"
    local number=$(basename "$file" | sed -n 's/^ADR[-_]\?\([0-9]\+\).*/\1/p')
    if [ -n "$number" ]; then
        # 补齐到 4 位（使用 10# 前缀强制十进制，避免 0 开头被当作八进制）
        printf "%04d" "$((10#$number))"
    fi
}

# 查找所有 ADR 文件
function scan_adrs() {
    log_info "扫描 ADR 文档..."
    local count=0
    
    while IFS= read -r file; do
        local number=$(extract_adr_number "$file")
        if [ -n "$number" ]; then
            ADR_MAP["$number"]="$file"
            count=$((count + 1))
        fi
    done < <(find "$ADR_PATH" -type f -name "ADR-*.md" | sort)
    
    log_success "发现 $count 个 ADR 文档"
}

# 查找所有测试文件
function scan_tests() {
    log_info "扫描架构测试文件..."
    local count=0
    
    if [ -d "$TESTS_PATH" ]; then
        while IFS= read -r file; do
            local number=$(basename "$file" | sed -n 's/^ADR_\([0-9]\{4\}\).*/\1/p')
            if [ -n "$number" ]; then
                TEST_MAP["$number"]="$file"
                count=$((count + 1))
            fi
        done < <(find "$TESTS_PATH" -type f -name "ADR_*.cs" | sort)
    fi
    
    log_success "发现 $count 个测试文件"
}

# 查找所有 Prompt 文件
function scan_prompts() {
    log_info "扫描 Copilot Prompts 文件..."
    local count=0
    
    while IFS= read -r file; do
        local number=$(basename "$file" | sed -n 's/^adr-0*\([0-9]\+\)\.prompts\.md$/\1/p')
        if [ -n "$number" ]; then
            # 补齐4位
            number=$(printf "%04d" $number)
            PROMPT_MAP["$number"]="$file"
            count=$((count + 1))
        fi
    done < <(find "$PROMPTS_PATH" -type f -name "adr-*.prompts.md" | sort)
    
    log_success "发现 $count 个 Prompt 文件"
}

# 检查 ADR 是否需要测试覆盖
function adr_requires_test() {
    local file="$1"
    
    # 临时文件，移除代码块后的内容
    local temp_file=$(mktemp)
    
    # 移除代码块（```...```）中的内容
    awk '
        /^```/ { 
            in_code_block = !in_code_block
            next
        }
        !in_code_block { print }
    ' "$file" > "$temp_file"
    
    # 查找标记为【必须架构测试覆盖】的条款
    local marked=$(grep -c "【必须架构测试覆盖】\|【必须测试】\|\[MUST_TEST\]" "$temp_file" 2>/dev/null || true)
    
    # 清理临时文件
    rm -f "$temp_file"
    
    [ -n "$marked" ] && [ "$marked" != "0" ]
}

# 分析映射关系
function analyze_mappings() {
    log_info "分析三位一体映射关系..."
    echo ""
    
    # 检查每个 ADR
    for number in "${!ADR_MAP[@]}"; do
        local adr_file="${ADR_MAP[$number]}"
        local has_test="${TEST_MAP[$number]:-}"
        local has_prompt="${PROMPT_MAP[$number]:-}"
        local needs_test=false
        
        # 检查是否需要测试
        if adr_requires_test "$adr_file"; then
            needs_test=true
        fi
        
        # 检查测试映射
        if [ "$needs_test" = true ] && [ -z "$has_test" ]; then
            MISSING_TESTS+=("$number")
            log_warning "  ADR-$number：需要测试但缺少测试文件"
            IS_VALID=false
        elif [ "$needs_test" = false ] && [ -n "$has_test" ]; then
            log_debug "  ADR-$number：有测试但未标记为【必须测试】（可能是额外的验证）"
        fi
        
        # 检查 Prompt 映射（所有 ADR 都应有 Prompt）
        if [ -z "$has_prompt" ]; then
            MISSING_PROMPTS+=("$number")
            log_warning "  ADR-$number：缺少 Prompt 文件"
            IS_VALID=false
        fi
    done
    
    # 检查孤立的测试文件
    for number in "${!TEST_MAP[@]}"; do
        if [ -z "${ADR_MAP[$number]:-}" ]; then
            ORPHAN_TESTS+=("$number")
            log_warning "  测试文件 ADR_${number}_Architecture_Tests.cs：对应的 ADR 不存在"
            IS_VALID=false
        fi
    done
    
    # 检查孤立的 Prompt 文件
    for number in "${!PROMPT_MAP[@]}"; do
        if [ -z "${ADR_MAP[$number]:-}" ]; then
            ORPHAN_PROMPTS+=("$number")
            log_warning "  Prompt 文件 adr-$number.prompts.md：对应的 ADR 不存在"
            IS_VALID=false
        fi
    done
}

# 生成修正清单
function generate_correction_list() {
    if [ "$IS_VALID" = true ]; then
        return
    fi
    
    echo ""
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${CYAN}📋 修正清单${NC}"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""
    
    # 缺少测试的 ADR
    if [ ${#MISSING_TESTS[@]} -gt 0 ]; then
        echo -e "${YELLOW}需要添加测试文件：${NC}"
        for number in "${MISSING_TESTS[@]}"; do
            echo "  [ ] 为 ADR-$number 创建测试文件："
            echo "      src/tests/ArchitectureTests/ADR/ADR_${number}_Architecture_Tests.cs"
            echo "      参考：docs/adr/governance/ADR-0000-architecture-tests.md"
            echo ""
        done
    fi
    
    # 缺少 Prompt 的 ADR
    if [ ${#MISSING_PROMPTS[@]} -gt 0 ]; then
        echo -e "${YELLOW}需要添加 Prompt 文件：${NC}"
        for number in "${MISSING_PROMPTS[@]}"; do
            local num_no_leading=$(echo $number | sed 's/^0*//')
            echo "  [ ] 为 ADR-$number 创建 Prompt 文件："
            echo "      docs/copilot/adr-$num_no_leading.prompts.md"
            echo "      参考：docs/templates/copilot-prompts-template.md"
            echo ""
        done
    fi
    
    # 孤立的测试文件
    if [ ${#ORPHAN_TESTS[@]} -gt 0 ]; then
        echo -e "${YELLOW}需要处理的孤立测试：${NC}"
        for number in "${ORPHAN_TESTS[@]}"; do
            echo "  [ ] 测试 ADR_${number}_Architecture_Tests.cs 对应的 ADR 不存在"
            echo "      选项："
            echo "      1. 创建对应的 ADR-$number 文档"
            echo "      2. 删除或重命名此测试文件"
            echo ""
        done
    fi
    
    # 孤立的 Prompt 文件
    if [ ${#ORPHAN_PROMPTS[@]} -gt 0 ]; then
        echo -e "${YELLOW}需要处理的孤立 Prompt：${NC}"
        for number in "${ORPHAN_PROMPTS[@]}"; do
            local num_no_leading=$(echo $number | sed 's/^0*//')
            echo "  [ ] Prompt adr-$num_no_leading.prompts.md 对应的 ADR 不存在"
            echo "      选项："
            echo "      1. 创建对应的 ADR-$number 文档"
            echo "      2. 删除此 Prompt 文件"
            echo ""
        done
    fi
}

# 生成健康报告摘要
function generate_health_summary() {
    echo ""
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${CYAN}📊 三位一体健康度报告${NC}"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""
    
    local total_adrs=${#ADR_MAP[@]}
    local total_tests=${#TEST_MAP[@]}
    local total_prompts=${#PROMPT_MAP[@]}
    
    local missing_tests_count=${#MISSING_TESTS[@]}
    local missing_prompts_count=${#MISSING_PROMPTS[@]}
    local orphan_tests_count=${#ORPHAN_TESTS[@]}
    local orphan_prompts_count=${#ORPHAN_PROMPTS[@]}
    
    local coverage_tests=$((total_adrs - missing_tests_count))
    local coverage_prompts=$((total_adrs - missing_prompts_count))
    
    echo "文件统计："
    echo "  ADR 文档数：$total_adrs"
    echo "  测试文件数：$total_tests"
    echo "  Prompt 文件数：$total_prompts"
    echo ""
    
    echo "映射覆盖率："
    local test_rate=0
    if [ $total_adrs -gt 0 ]; then
        test_rate=$((coverage_tests * 100 / total_adrs))
    fi
    echo -ne "  测试覆盖：$coverage_tests/$total_adrs ($test_rate%) "
    if [ $test_rate -eq 100 ]; then
        echo -e "${GREEN}✅${NC}"
    elif [ $test_rate -ge 80 ]; then
        echo -e "${YELLOW}⚠️${NC}"
    else
        echo -e "${RED}❌${NC}"
    fi
    
    local prompt_rate=0
    if [ $total_adrs -gt 0 ]; then
        prompt_rate=$((coverage_prompts * 100 / total_adrs))
    fi
    echo -ne "  Prompt 覆盖：$coverage_prompts/$total_adrs ($prompt_rate%) "
    if [ $prompt_rate -eq 100 ]; then
        echo -e "${GREEN}✅${NC}"
    elif [ $prompt_rate -ge 80 ]; then
        echo -e "${YELLOW}⚠️${NC}"
    else
        echo -e "${RED}❌${NC}"
    fi
    echo ""
    
    echo "问题统计："
    echo "  缺少测试的 ADR：$missing_tests_count"
    echo "  缺少 Prompt 的 ADR：$missing_prompts_count"
    echo "  孤立的测试文件：$orphan_tests_count"
    echo "  孤立的 Prompt 文件：$orphan_prompts_count"
    echo ""
    
    if [ "$IS_VALID" = true ]; then
        log_success "三位一体映射一致性验证通过！"
    else
        log_error "发现映射不一致问题，请查看上方修正清单"
        echo ""
        echo -e "${CYAN}建议操作：${NC}"
        echo "  1. 根据修正清单逐项处理问题"
        echo "  2. 对于缺少的测试/Prompt，使用模板创建"
        echo "  3. 对于孤立的文件，确认是否需要保留"
        echo "  4. 修正后重新运行本脚本验证"
    fi
}

# 主执行函数
function main() {
    echo -e "${CYAN}╔═══════════════════════════════════════════════════════════╗${NC}"
    echo -e "${CYAN}║   三位一体映射扫描增强工具                                ║${NC}"
    echo -e "${CYAN}║   ADR / 架构测试 / Copilot Prompts 一致性验证             ║${NC}"
    echo -e "${CYAN}╚═══════════════════════════════════════════════════════════╝${NC}"
    echo ""
    
    # 扫描所有文件
    scan_adrs
    scan_tests
    scan_prompts
    echo ""
    
    # 分析映射关系
    analyze_mappings
    
    # 生成修正清单
    generate_correction_list
    
    # 生成健康报告
    generate_health_summary
    
    if [ "$IS_VALID" = true ]; then
        return 0
    else
        return 1
    fi
}

# 执行
main
exit $?
