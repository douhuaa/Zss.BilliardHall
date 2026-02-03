#!/bin/bash

# ADR 编号/目录/内容三元一致性验证工具
# 依据 ADR-970.2 支持 JSON 输出
#
# 此脚本用于验证 ADR 文档的编号、目录和内容的一致性，确保：
# 1. ADR 编号与文件名一致
# 2. ADR 编号与所在目录对应的编号范围一致
# 3. ADR 元数据（状态、级别等）完整且格式正确
# 4. 前导零使用规范（4位编号格式）
# 5. 目录结构符合层级定义
#
# 用法：
#   ./validate-adr-consistency.sh [--format text|json] [--output FILE]
#
# 示例：
#   ./validate-adr-consistency.sh
#   ./validate-adr-consistency.sh --format json
#   ./validate-adr-consistency.sh --format json --output docs/reports/architecture-tests/adr-consistency.json

set -e

# 定义路径
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
ADR_PATH="$REPO_ROOT/docs/adr"

# 输出格式和路径
OUTPUT_FORMAT="text"
OUTPUT_FILE=""

# 解析参数
while [[ $# -gt 0 ]]; do
    case $1 in
        --format)
            OUTPUT_FORMAT="$2"
            shift 2
            ;;
        --output)
            OUTPUT_FILE="$2"
            shift 2
            ;;
        --help)
            echo "用法: $0 [--format text|json] [--output FILE]"
            echo ""
            echo "选项:"
            echo "  --format FORMAT    输出格式：text（默认）或 json"
            echo "  --output FILE      输出到文件（仅在 json 格式时有效）"
            echo "  --help             显示帮助信息"
            exit 0
            ;;
        *)
            echo "未知选项: $1"
            exit 1
            ;;
    esac
done

# 如果是 JSON 模式，加载 JSON 输出库
if [ "$OUTPUT_FORMAT" = "json" ]; then
    source "$SCRIPT_DIR/lib/json-output.sh"
fi

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

# 输出函数 - 根据输出格式选择
function log_success() { 
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo -e "${GREEN}✅ $1${NC}"
    fi
}
function log_warning() { 
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo -e "${YELLOW}⚠️  $1${NC}"
    fi
}
function log_error() { 
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo -e "${RED}❌ $1${NC}"
    fi
}
function log_info() { 
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo -e "${CYAN}ℹ️  $1${NC}"
    fi
}
function log_plain() {
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo "$1"
    fi
}

# 统计变量
TOTAL_ADRS=0
VALID_ADRS=0
INVALID_ADRS=0
IS_VALID=true

# ADR 层级编号范围定义（基于 ADR-006）
declare -A TIER_RANGES
TIER_RANGES["constitutional"]="0001-0099"
TIER_RANGES["structure"]="0100-0199"
TIER_RANGES["runtime"]="0200-0299"
TIER_RANGES["technical"]="0300-0399"
TIER_RANGES["governance"]="0000,0900-0999"

# 解析编号范围
function is_in_range() {
    local number=$1
    local range=$2
    
    # 移除前导零进行数值比较
    number=$((10#$number))
    
    # 处理特殊情况（如 governance 的 0000 和 900-999）
    if [[ "$range" == *","* ]]; then
        IFS=',' read -ra RANGES <<< "$range"
        for r in "${RANGES[@]}"; do
            if [[ "$r" == *"-"* ]]; then
                IFS='-' read -r start end <<< "$r"
                start=$((10#$start))
                end=$((10#$end))
                if [ "$number" -ge "$start" ] && [ "$number" -le "$end" ]; then
                    return 0
                fi
            else
                # 单个数字
                if [ "$number" -eq "$((10#$r))" ]; then
                    return 0
                fi
            fi
        done
        return 1
    else
        # 标准范围
        IFS='-' read -r start end <<< "$range"
        start=$((10#$start))
        end=$((10#$end))
        [ "$number" -ge "$start" ] && [ "$number" -le "$end" ]
    fi
}

# 查找 ADR 文件
function find_adr_files() {
    find "$ADR_PATH" -type f -name "ADR-*.md" -o -name "ADR_*.md" | sort
}

# 提取 ADR 编号
function extract_adr_number() {
    local file="$1"
    basename "$file" | sed -n 's/^ADR[-_]\?\([0-9]\{4\}\).*/\1/p'
}

# 提取目录层级
function extract_tier() {
    local file="$1"
    local dir=$(dirname "$file")
    basename "$dir"
}

# 验证 ADR 元数据
function validate_metadata() {
    local file="$1"
    local errors=()
    
    # 检查必需的元数据字段
    if ! grep -q "^\*\*状态\*\*：" "$file" && ! grep -q "^**状态**:" "$file"; then
        errors+=("缺少状态字段")
    fi
    
    if ! grep -q "^\*\*级别\*\*：" "$file" && ! grep -q "^**级别**:" "$file"; then
        errors+=("缺少级别字段")
    fi
    
    # 检查编号格式（应为4位）
    local number=$(extract_adr_number "$file")
    if [ ${#number} -ne 4 ]; then
        errors+=("编号格式错误（应为4位，如 0001）")
    fi
    
    if [ ${#errors[@]} -eq 0 ]; then
        echo "OK"
    else
        echo "${errors[*]}"
    fi
}

# 检查跳号
function check_number_gaps() {
    local -a numbers=()
    
    while IFS= read -r file; do
        local number=$(extract_adr_number "$file")
        if [ -n "$number" ]; then
            numbers+=($((10#$number)))
        fi
    done < <(find_adr_files)
    
    # 排序
    IFS=$'\n' sorted=($(sort -n <<<"${numbers[*]}"))
    unset IFS
    
    local gaps=()
    for ((i=0; i<${#sorted[@]}-1; i++)); do
        local current=${sorted[$i]}
        local next=${sorted[$i+1]}
        local diff=$((next - current))
        
        # 跳过跨层级的检查（如从 0099 到 0100）
        if [ $diff -gt 1 ] && [ $((current / 100)) -eq $((next / 100)) ]; then
            gaps+=("$current 到 $next (跳过 $((diff-1)) 个编号)")
        fi
    done
    
    if [ ${#gaps[@]} -eq 0 ]; then
        log_success "编号连续性检查通过"
        if [ "$OUTPUT_FORMAT" = "json" ]; then
            json_add_detail "ADR_Number_Continuity" "" "info" "编号连续性检查通过"
        fi
    else
        log_warning "发现编号跳号："
        for gap in "${gaps[@]}"; do
            if [ "$OUTPUT_FORMAT" = "text" ]; then
                echo "    $gap"
            fi
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "ADR_Number_Continuity" "" "warning" "发现编号跳号: $gap"
            fi
        done
    fi
}

# 主验证函数
function validate_consistency() {
    log_info "开始 ADR 三元一致性验证..."
    log_plain ""
    
    # 如果是 JSON 模式，初始化 JSON 输出
    if [ "$OUTPUT_FORMAT" = "json" ]; then
        json_start "validate-adr-consistency" "1.0.0" "adr-validation"
    fi
    
    # 获取所有 ADR 文件
    local adr_files=()
    while IFS= read -r file; do
        adr_files+=("$file")
    done < <(find_adr_files)
    
    TOTAL_ADRS=${#adr_files[@]}
    log_info "发现 $TOTAL_ADRS 个 ADR 文档"
    log_plain ""
    
    # 验证每个 ADR
    for adr_file in "${adr_files[@]}"; do
        local adr_number=$(extract_adr_number "$adr_file")
        local tier=$(extract_tier "$adr_file")
        local filename=$(basename "$adr_file")
        
        if [ -z "$adr_number" ]; then
            continue
        fi
        
        if [ "$OUTPUT_FORMAT" = "text" ]; then
            echo -e "${GRAY}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
        fi
        log_info "检查 ADR-$adr_number ($filename)"
        
        local has_error=false
        local adr_id="ADR-$adr_number"
        
        # 1. 检查编号格式（4位）
        if [ ${#adr_number} -ne 4 ]; then
            log_error "  编号格式错误：应为4位数字（如 0001），当前为 $adr_number"
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "ADR_Numbering_Format" "$adr_id" "error" \
                    "编号格式错误：应为4位数字（如 0001），当前为 $adr_number" \
                    "$adr_file" "" "docs/adr/constitutional/ADR-006-terminology-numbering-constitution.md"
            fi
            has_error=true
        else
            log_success "  编号格式正确：$adr_number"
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "ADR_Numbering_Format" "$adr_id" "info" \
                    "编号格式正确：$adr_number" "$adr_file"
            fi
        fi
        
        # 2. 检查目录与编号范围一致性
        if [ -n "${TIER_RANGES[$tier]}" ]; then
            if is_in_range "$adr_number" "${TIER_RANGES[$tier]}"; then
                log_success "  目录位置正确：$tier (范围: ${TIER_RANGES[$tier]})"
                if [ "$OUTPUT_FORMAT" = "json" ]; then
                    json_add_detail "ADR_Tier_Consistency" "$adr_id" "info" \
                        "目录位置正确：$tier (范围: ${TIER_RANGES[$tier]})" "$adr_file"
                fi
            else
                log_error "  目录位置错误：ADR-$adr_number 不在 $tier 的编号范围 (${TIER_RANGES[$tier]}) 内"
                if [ "$OUTPUT_FORMAT" = "json" ]; then
                    json_add_detail "ADR_Tier_Consistency" "$adr_id" "error" \
                        "目录位置错误：ADR-$adr_number 不在 $tier 的编号范围 (${TIER_RANGES[$tier]}) 内" \
                        "$adr_file" "" "docs/adr/constitutional/ADR-006-terminology-numbering-constitution.md"
                fi
                has_error=true
            fi
        else
            log_warning "  未知目录层级：$tier"
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "ADR_Tier_Consistency" "$adr_id" "warning" \
                    "未知目录层级：$tier" "$adr_file"
            fi
        fi
        
        # 3. 检查元数据
        local metadata_result=$(validate_metadata "$adr_file")
        if [ "$metadata_result" == "OK" ]; then
            log_success "  元数据完整"
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "ADR_Metadata_Completeness" "$adr_id" "info" \
                    "元数据完整" "$adr_file"
            fi
        else
            log_error "  元数据问题：$metadata_result"
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "ADR_Metadata_Completeness" "$adr_id" "error" \
                    "元数据问题：$metadata_result" "$adr_file" "" \
                    "docs/adr/governance/ADR-900-adr-process.md"
            fi
            has_error=true
        fi
        
        # 4. 检查文件命名规范
        if [[ "$filename" =~ ^ADR-[0-9]{4}-.+\.md$ ]]; then
            log_success "  文件命名符合规范"
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "ADR_File_Naming" "$adr_id" "info" \
                    "文件命名符合规范" "$adr_file"
            fi
        else
            log_warning "  文件命名可能不符合规范：期望格式为 ADR-XXXX-description.md"
            if [ "$OUTPUT_FORMAT" = "json" ]; then
                json_add_detail "ADR_File_Naming" "$adr_id" "warning" \
                    "文件命名可能不符合规范：期望格式为 ADR-XXXX-description.md" "$adr_file"
            fi
        fi
        
        if [ "$has_error" = true ]; then
            INVALID_ADRS=$((INVALID_ADRS + 1))
            IS_VALID=false
        else
            VALID_ADRS=$((VALID_ADRS + 1))
        fi
        
        log_plain ""
    done
    
    # 检查编号跳号
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo -e "${GRAY}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    fi
    check_number_gaps
    log_plain ""
    
    # 输出总结
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        echo -e "${GRAY}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
        echo ""
        echo -e "${CYAN}📊 验证总结${NC}"
        echo -e "${GRAY}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
        echo ""
        echo "ADR 文档统计："
        echo "  总 ADR 数：$TOTAL_ADRS"
        echo -e "  有效 ADR：${GREEN}$VALID_ADRS${NC}"
        if [ "$INVALID_ADRS" -gt 0 ]; then
            echo -e "  无效 ADR：${RED}$INVALID_ADRS${NC}"
        else
            echo -e "  无效 ADR：${GREEN}$INVALID_ADRS${NC}"
        fi
        echo ""
    fi
    
    # 确定最终状态
    local final_status
    if [ "$IS_VALID" = true ]; then
        log_success "验证通过：所有 ADR 文档编号、目录、内容一致！"
        final_status="success"
    else
        log_error "验证失败：发现 ADR 一致性问题"
        if [ "$OUTPUT_FORMAT" = "text" ]; then
            echo ""
            echo -e "${YELLOW}请执行以下操作：${NC}"
            echo "  1. 修正编号格式错误（确保为4位数字）"
            echo "  2. 将 ADR 移动到正确的目录层级"
            echo "  3. 补充缺失的元数据字段"
            echo "  4. 确保文件命名符合规范"
            echo ""
            echo -e "${CYAN}参考文档：${NC}"
            echo "  - docs/adr/constitutional/ADR-006-terminology-numbering-constitution.md"
            echo "  - docs/adr/governance/ADR-900-adr-process.md"
            echo ""
        fi
        final_status="failure"
    fi
    
    # 如果是 JSON 模式，输出或保存 JSON
    if [ "$OUTPUT_FORMAT" = "json" ]; then
        if [ -n "$OUTPUT_FILE" ]; then
            json_save "$final_status" "$OUTPUT_FILE"
        else
            json_finalize "$final_status"
        fi
    fi
    
    [ "$IS_VALID" = true ] && return 0 || return 1
}

# 主执行
validate_consistency
exit $?
