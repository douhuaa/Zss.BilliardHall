#!/bin/bash

# ADR 健康报告生成器
# 依据 ADR-970.2 支持 JSON 输出
#
# 此脚本生成 ADR 治理体系的综合健康报告，包括：
# 1. ADR 文档统计和覆盖率
# 2. 测试和 Prompt 映射健康度
# 3. 编号一致性状态
# 4. 破例和技术债务统计
# 5. 近期变更趋势
#
# 用法：
#   ./generate-health-report.sh [OUTPUT_FILE] [--format text|json]
#
# 示例：
#   ./generate-health-report.sh
#   ./generate-health-report.sh docs/adr-health-report.md
#   ./generate-health-report.sh --format json
#   ./generate-health-report.sh docs/reports/architecture-tests/health-report.json --format json

set -eo pipefail

# 定义路径
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
ADR_PATH="$REPO_ROOT/docs/adr"
TESTS_PATH="$REPO_ROOT/src/tests/ArchitectureTests/ADR"
PROMPTS_PATH="$REPO_ROOT/docs/copilot"

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
        --help)
            echo "用法: $0 [OUTPUT_FILE] [--format text|json]"
            echo ""
            echo "选项:"
            echo "  OUTPUT_FILE        输出文件路径（可选）"
            echo "  --format FORMAT    输出格式：text（默认）或 json"
            echo "  --help             显示帮助信息"
            echo ""
            echo "示例:"
            echo "  $0                                    # 生成默认文本报告"
            echo "  $0 docs/my-report.md                  # 生成文本报告到指定文件"
            echo "  $0 --format json                      # 生成 JSON 报告到默认位置"
            echo "  $0 report.json --format json          # 生成 JSON 报告到指定文件"
            exit 0
            ;;
        *)
            if [ -z "$OUTPUT_FILE" ]; then
                OUTPUT_FILE="$1"
            fi
            shift
            ;;
    esac
done

# 设置默认输出文件
if [ -z "$OUTPUT_FILE" ]; then
    if [ "$OUTPUT_FORMAT" = "json" ]; then
        OUTPUT_FILE="$REPO_ROOT/docs/reports/architecture-tests/health-report.json"
    else
        OUTPUT_FILE="$REPO_ROOT/docs/adr-health-report.md"
    fi
fi

# 加载 JSON 输出库（如果使用 JSON 格式）
if [ "$OUTPUT_FORMAT" = "json" ]; then
    source "$SCRIPT_DIR/lib/json-output.sh"
    json_start "generate-health-report" "1.0.0" "health-report"
fi

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BLUE='\033[0;34m'
NC='\033[0m'

# 输出函数
function log_info() { echo -e "${CYAN}ℹ️  $1${NC}"; }

# 生成报告头
function generate_header() {
    cat << EOF
# ADR 治理体系健康报告

**生成时间**：$(date '+%Y-%m-%d %H:%M:%S')  
**仓库**：Zss.BilliardHall  
**报告版本**：1.0

---

## 执行摘要

本报告提供 ADR 治理体系的全面健康度评估，包括文档完整性、测试覆盖率、映射一致性等关键指标。

---

EOF
}

# 统计 ADR 文档
function analyze_adrs() {
    cat << EOF
## ADR 文档统计

EOF
    
    local constitutional_count=$(find "$ADR_PATH/constitutional" -name "ADR-*.md" 2>/dev/null | wc -l)
    local structure_count=$(find "$ADR_PATH/structure" -name "ADR-*.md" 2>/dev/null | wc -l)
    local runtime_count=$(find "$ADR_PATH/runtime" -name "ADR-*.md" 2>/dev/null | wc -l)
    local technical_count=$(find "$ADR_PATH/technical" -name "ADR-*.md" 2>/dev/null | wc -l)
    local governance_count=$(find "$ADR_PATH/governance" -name "ADR-*.md" 2>/dev/null | wc -l)
    local total_count=$((constitutional_count + structure_count + runtime_count + technical_count + governance_count))
    
    cat << EOF
### 按层级分布

| 层级 | 编号范围 | 文档数量 | 占比 |
|-----|---------|---------|------|
| 宪法层 (constitutional) | 0001-0099 | $constitutional_count | $(awk "BEGIN {printf \"%.1f%%\", $constitutional_count * 100.0 / $total_count}") |
| 结构层 (structure) | 0100-0199 | $structure_count | $(awk "BEGIN {printf \"%.1f%%\", $structure_count * 100.0 / $total_count}") |
| 运行层 (runtime) | 0200-0299 | $runtime_count | $(awk "BEGIN {printf \"%.1f%%\", $runtime_count * 100.0 / $total_count}") |
| 技术层 (technical) | 0300-0399 | $technical_count | $(awk "BEGIN {printf \"%.1f%%\", $technical_count * 100.0 / $total_count}") |
| 治理层 (governance) | 0900-0999 | $governance_count | $(awk "BEGIN {printf \"%.1f%%\", $governance_count * 100.0 / $total_count}") |
| **总计** | | **$total_count** | **100%** |

### 状态分布

EOF
    
    local draft_count=$(find "$ADR_PATH" -name "ADR-*.md" -exec grep -l "**状态\*\*：Draft\|**状态\*\*.*Draft" {} \; 2>/dev/null | wc -l)
    local accepted_count=$(find "$ADR_PATH" -name "ADR-*.md" -exec grep -l "**状态\*\*：Accepted\|**状态\*\*.*Accepted" {} \; 2>/dev/null | wc -l)
    local final_count=$(find "$ADR_PATH" -name "ADR-*.md" -exec grep -l "**状态\*\*：Final\|**状态\*\*.*Final" {} \; 2>/dev/null | wc -l)
    local superseded_count=$(find "$ADR_PATH" -name "ADR-*.md" -exec grep -l "**状态\*\*：Superseded\|**状态\*\*.*Superseded" {} \; 2>/dev/null | wc -l)
    
    cat << EOF
| 状态 | 数量 | 说明 |
|-----|------|------|
| Draft | $draft_count | 草案阶段 |
| Accepted | $accepted_count | 已接受 |
| Final | $final_count | 已定稿 |
| Superseded | $superseded_count | 已废弃 |

---

EOF
}

# 分析测试覆盖
function analyze_test_coverage() {
    cat << EOF
## 架构测试覆盖率

EOF
    
    local total_tests=$(find "$TESTS_PATH" -name "ADR_*.cs" 2>/dev/null | wc -l)
    local adr_with_must_test=0
    local adr_with_test_file=0
    
    # 计算需要测试的 ADR 数量
    while IFS= read -r file; do
        local temp_file=$(mktemp)
        awk '/^```/ { in_code_block = !in_code_block; next } !in_code_block { print }' "$file" > "$temp_file"
        local marked=$(grep -c "【必须架构测试覆盖】\|【必须测试】\|\[MUST_TEST\]" "$temp_file" 2>/dev/null || true)
        rm -f "$temp_file"
        
        if [ -n "$marked" ] && [ "$marked" != "0" ]; then
            adr_with_must_test=$((adr_with_must_test + 1))
            
            # 检查是否有对应的测试文件
            local number=$(basename "$file" | sed -n 's/^ADR-\([0-9]\{4\}\).*/\1/p')
            if [ -f "$TESTS_PATH/ADR_${number}_Architecture_Tests.cs" ]; then
                adr_with_test_file=$((adr_with_test_file + 1))
            fi
        fi
    done < <(find "$ADR_PATH" -name "ADR-*.md")
    
    local coverage_rate=0
    if [ $adr_with_must_test -gt 0 ]; then
        coverage_rate=$((adr_with_test_file * 100 / adr_with_must_test))
    fi
    
    cat << EOF
### 测试文件统计

| 指标 | 数量 | 覆盖率 |
|-----|------|--------|
| 标记【必须测试】的 ADR | $adr_with_must_test | - |
| 有测试文件的 ADR | $adr_with_test_file | ${coverage_rate}% |
| 总测试文件数 | $total_tests | - |

EOF
    
    if [ $coverage_rate -eq 100 ]; then
        echo "✅ **状态**：测试覆盖率达标"
    elif [ $coverage_rate -ge 80 ]; then
        echo "⚠️ **状态**：测试覆盖率需要提升"
    else
        echo "❌ **状态**：测试覆盖率不足"
    fi
    
    echo ""
    echo "---"
    echo ""
}

# 分析 Prompt 映射
function analyze_prompt_mapping() {
    cat << EOF
## Copilot Prompts 映射

EOF
    
    local total_prompts=$(find "$PROMPTS_PATH" -name "adr-*.prompts.md" 2>/dev/null | wc -l)
    local total_adrs=$(find "$ADR_PATH" -name "ADR-*.md" | wc -l)
    
    local mapping_rate=0
    if [ $total_adrs -gt 0 ]; then
        mapping_rate=$((total_prompts * 100 / total_adrs))
    fi
    
    cat << EOF
### Prompt 文件统计

| 指标 | 数量 | 映射率 |
|-----|------|--------|
| ADR 总数 | $total_adrs | - |
| Prompt 文件数 | $total_prompts | ${mapping_rate}% |

EOF
    
    if [ $mapping_rate -eq 100 ]; then
        echo "✅ **状态**：Prompt 映射完整"
    elif [ $mapping_rate -ge 80 ]; then
        echo "⚠️ **状态**：Prompt 映射基本完整"
    else
        echo "❌ **状态**：Prompt 映射不完整"
    fi
    
    echo ""
    echo "---"
    echo ""
}

# 分析编号一致性
function analyze_numbering_consistency() {
    cat << EOF
## 编号与目录一致性

EOF
    
    # 运行一致性检查脚本并捕获结果
    local result=$("$SCRIPT_DIR/validate-adr-consistency.sh" 2>&1)
    local exit_code=$?
    
    if [ $exit_code -eq 0 ]; then
        echo "✅ **状态**：所有 ADR 编号、目录、内容一致"
    else
        echo "❌ **状态**：发现编号或目录不一致问题"
        echo ""
        echo "### 问题详情"
        echo ""
        echo "\`\`\`"
        echo "$result" | grep "❌\|⚠️" || echo "请运行 scripts/validate-adr-consistency.sh 查看详情"
        echo "\`\`\`"
    fi
    
    echo ""
    echo "---"
    echo ""
}

# 生成建议
function generate_recommendations() {
    cat << EOF
## 改进建议

### 高优先级

EOF
    
    # 基于分析结果生成建议
    local has_recommendations=false
    
    # 检查测试覆盖率
    local adr_with_must_test=0
    local adr_with_test_file=0
    while IFS= read -r file; do
        local temp_file=$(mktemp)
        awk '/^```/ { in_code_block = !in_code_block; next } !in_code_block { print }' "$file" > "$temp_file"
        local marked=$(grep -c "【必须架构测试覆盖】\|【必须测试】\|\[MUST_TEST\]" "$temp_file" 2>/dev/null || true)
        rm -f "$temp_file"
        
        if [ -n "$marked" ] && [ "$marked" != "0" ]; then
            adr_with_must_test=$((adr_with_must_test + 1))
            local number=$(basename "$file" | sed -n 's/^ADR-\([0-9]\{4\}\).*/\1/p')
            if [ -f "$TESTS_PATH/ADR_${number}_Architecture_Tests.cs" ]; then
                adr_with_test_file=$((adr_with_test_file + 1))
            fi
        fi
    done < <(find "$ADR_PATH" -name "ADR-*.md")
    
    local coverage_rate=0
    if [ $adr_with_must_test -gt 0 ]; then
        coverage_rate=$((adr_with_test_file * 100 / adr_with_must_test))
    fi
    
    if [ $coverage_rate -lt 100 ]; then
        echo "1. **补充架构测试**：为标记【必须测试】但缺少测试的 ADR 添加测试文件"
        has_recommendations=true
    fi
    
    # 检查 Prompt 映射
    local total_prompts=$(find "$PROMPTS_PATH" -name "adr-*.prompts.md" 2>/dev/null | wc -l)
    local total_adrs=$(find "$ADR_PATH" -name "ADR-*.md" | wc -l)
    local mapping_rate=0
    if [ $total_adrs -gt 0 ]; then
        mapping_rate=$((total_prompts * 100 / total_adrs))
    fi
    
    if [ $mapping_rate -lt 100 ]; then
        echo "2. **完善 Copilot Prompts**：为缺少 Prompt 的 ADR 创建场景化提示词"
        has_recommendations=true
    fi
    
    if [ "$has_recommendations" = false ]; then
        echo "无高优先级改进项 ✅"
    fi
    
    cat << EOF

### 中优先级

1. **定期审查**：每月审查 Draft 状态的 ADR，推动其进入 Accepted 或 Final 状态
2. **清理孤立文件**：检查并处理孤立的测试和 Prompt 文件
3. **优化文档**：补充 ADR 示例代码和常见场景说明

### 低优先级

1. **统计分析**：添加 ADR 变更趋势分析
2. **自动化增强**：集成更多自动化检查到 CI/CD
3. **文档模板**：持续优化 ADR 和 Prompt 模板

---

EOF
}

# 生成工具使用指南
function generate_tools_guide() {
    cat << EOF
## 工具使用指南

### 日常维护工具

1. **ADR 一致性检查**
   \`\`\`bash
   ./scripts/validate-adr-consistency.sh
   \`\`\`

2. **三位一体映射验证**
   \`\`\`bash
   ./scripts/validate-three-way-mapping.sh
   \`\`\`

3. **ADR 管理 CLI**
   \`\`\`bash
   # 创建新 ADR
   ./scripts/adr-cli.sh create constitutional "标题"
   
   # 查询下一个可用编号
   ./scripts/adr-cli.sh next-number structure
   
   # 列出 ADR
   ./scripts/adr-cli.sh list
   \`\`\`

4. **生成健康报告**
   \`\`\`bash
   ./scripts/generate-health-report.sh
   \`\`\`

### 问题诊断流程

1. 运行一致性检查发现问题
2. 根据报告定位具体文件
3. 使用 ADR CLI 工具修正
4. 重新运行验证确认修复

---

EOF
}

# 生成报告尾部
function generate_footer() {
    cat << EOF
## 附录

### 相关资源

- [ADR 目录](./adr/README.md)
- [ADR 流程规范](./adr/governance/ADR-900-adr-process.md)
- [架构测试宪法](./adr/governance/ADR-0000-architecture-tests.md)
- [Copilot 治理体系](./copilot/README.md)

### 下次报告

建议每月生成一次健康报告，跟踪改进进度。

---

**报告生成**：自动化工具 v1.0  
**维护者**：架构委员会
EOF
}

# 主执行函数
function main() {
    if [ "$OUTPUT_FORMAT" = "text" ]; then
        log_info "生成 ADR 健康报告..."
        log_info "输出文件：$OUTPUT_FILE"
        
        {
            generate_header
            analyze_adrs
            analyze_test_coverage
            analyze_prompt_mapping
            analyze_numbering_consistency
            generate_recommendations
            generate_tools_guide
            generate_footer
        } > "$OUTPUT_FILE"
        
        log_info "✅ 报告生成完成：$OUTPUT_FILE"
        echo ""
        echo "查看报告："
        echo "  cat $OUTPUT_FILE"
    else
        # JSON 模式
        generate_json_report
    fi
}

# 生成 JSON 报告
function generate_json_report() {
    # 统计 ADR 文档
    local constitutional_count=$(find "$ADR_PATH/constitutional" -name "ADR-*.md" 2>/dev/null | wc -l)
    local structure_count=$(find "$ADR_PATH/structure" -name "ADR-*.md" 2>/dev/null | wc -l)
    local runtime_count=$(find "$ADR_PATH/runtime" -name "ADR-*.md" 2>/dev/null | wc -l)
    local technical_count=$(find "$ADR_PATH/technical" -name "ADR-*.md" 2>/dev/null | wc -l)
    local governance_count=$(find "$ADR_PATH/governance" -name "ADR-*.md" 2>/dev/null | wc -l)
    local total_count=$((constitutional_count + structure_count + runtime_count + technical_count + governance_count))
    
    # 添加 ADR 统计详情
    json_add_detail "ADR_Count_Constitutional" "ADR-900" "info" \
        "宪法层 ADR 数量: $constitutional_count" \
        "" "" \
        "docs/adr/governance/ADR-900-adr-process.md"
    
    json_add_detail "ADR_Count_Structure" "ADR-900" "info" \
        "结构层 ADR 数量: $structure_count" \
        "" "" \
        "docs/adr/governance/ADR-900-adr-process.md"
    
    json_add_detail "ADR_Count_Runtime" "ADR-900" "info" \
        "运行层 ADR 数量: $runtime_count" \
        "" "" \
        "docs/adr/governance/ADR-900-adr-process.md"
    
    json_add_detail "ADR_Count_Technical" "ADR-900" "info" \
        "技术层 ADR 数量: $technical_count" \
        "" "" \
        "docs/adr/governance/ADR-900-adr-process.md"
    
    json_add_detail "ADR_Count_Governance" "ADR-900" "info" \
        "治理层 ADR 数量: $governance_count" \
        "" "" \
        "docs/adr/governance/ADR-900-adr-process.md"
    
    json_add_detail "ADR_Count_Total" "ADR-900" "info" \
        "ADR 总数: $total_count" \
        "" "" \
        "docs/adr/governance/ADR-900-adr-process.md"
    
    # 统计测试覆盖率
    local total_tests=$(find "$TESTS_PATH" -name "ADR_*.cs" 2>/dev/null | wc -l)
    local adr_with_must_test=0
    local adr_with_test_file=0
    
    while IFS= read -r file; do
        local temp_file=$(mktemp)
        awk '/^```/ { in_code_block = !in_code_block; next } !in_code_block { print }' "$file" > "$temp_file"
        local marked=$(grep -c "【必须架构测试覆盖】\|【必须测试】\|\[MUST_TEST\]" "$temp_file" 2>/dev/null || true)
        rm -f "$temp_file"
        
        if [ -n "$marked" ] && [ "$marked" != "0" ]; then
            adr_with_must_test=$((adr_with_must_test + 1))
            local number=$(basename "$file" | sed -n 's/^ADR-\([0-9]\{4\}\).*/\1/p')
            if [ -f "$TESTS_PATH/ADR_${number}_Architecture_Tests.cs" ]; then
                adr_with_test_file=$((adr_with_test_file + 1))
            fi
        fi
    done < <(find "$ADR_PATH" -name "ADR-*.md")
    
    local coverage_rate=0
    if [ $adr_with_must_test -gt 0 ]; then
        coverage_rate=$((adr_with_test_file * 100 / adr_with_must_test))
    fi
    
    local coverage_severity="info"
    if [ $coverage_rate -lt 80 ]; then
        coverage_severity="warning"
    fi
    if [ $coverage_rate -lt 60 ]; then
        coverage_severity="error"
    fi
    
    json_add_detail "Test_Coverage" "ADR-0000" "$coverage_severity" \
        "测试覆盖率: ${coverage_rate}% ($adr_with_test_file/$adr_with_must_test)" \
        "" "" \
        "docs/adr/governance/ADR-0000-architecture-tests.md"
    
    # 统计 Prompt 映射
    local total_prompts=$(find "$PROMPTS_PATH" -name "adr-*.prompts.md" 2>/dev/null | wc -l)
    local mapping_rate=0
    if [ $total_count -gt 0 ]; then
        mapping_rate=$((total_prompts * 100 / total_count))
    fi
    
    local mapping_severity="info"
    if [ $mapping_rate -lt 80 ]; then
        mapping_severity="warning"
    fi
    
    json_add_detail "Prompt_Mapping" "ADR-900" "$mapping_severity" \
        "Prompt 映射率: ${mapping_rate}% ($total_prompts/$total_count)" \
        "" "" \
        "docs/adr/governance/ADR-900-adr-process.md"
    
    # 检查编号一致性
    if "$SCRIPT_DIR/validate-adr-consistency.sh" > /dev/null 2>&1; then
        json_add_detail "Numbering_Consistency" "ADR-930" "info" \
            "编号与目录一致性: 通过" \
            "" "" \
            "docs/adr/governance/ADR-930-adr-numbering-convention.md"
    else
        json_add_detail "Numbering_Consistency" "ADR-930" "error" \
            "编号与目录一致性: 失败" \
            "" "" \
            "docs/adr/governance/ADR-930-adr-numbering-convention.md"
    fi
    
    # 输出 JSON
    status=$(json_determine_status)
    json_save "$status" "$OUTPUT_FILE"
}

# 执行
main
