#!/bin/bash

# 治理合规性验证脚本
# 根据治理层 ADR 要求，验证系统治理机制的完整性和一致性
# 依据：ADR-0000, ADR-900, ADR-930, ADR-910, ADR-920
# 依据 ADR-970.2 支持 JSON 输出
#
# 用法：
#   ./validate-governance-compliance.sh [--format text|json] [--output FILE]
#
# 示例：
#   ./validate-governance-compliance.sh
#   ./validate-governance-compliance.sh --format json
#   ./validate-governance-compliance.sh --format json --output docs/reports/governance-compliance.json

set -eo pipefail

# 定义路径
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

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

# 加载 JSON 输出库（如果使用 JSON 格式）
if [ "$OUTPUT_FORMAT" = "json" ]; then
    source "$SCRIPT_DIR/lib/json-output.sh"
    json_start "validate-governance-compliance" "1.0.0" "governance-validation"
fi

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 计数器
TOTAL_CHECKS=0
PASSED_CHECKS=0
FAILED_CHECKS=0

# 条件输出标题
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}治理合规性验证${NC}"
    echo -e "${BLUE}========================================${NC}"
    echo ""
fi

# 辅助函数：记录检查结果
check_result() {
    local check_name="$1"
    local result="$2"
    local message="$3"
    local adr="${4:-}"
    local file="${5:-}"
    local fix_guide="${6:-docs/adr/governance/}"
    
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    
    if [ "$result" = "PASS" ]; then
        if [ "$OUTPUT_FORMAT" = "text" ]; then
            echo -e "${GREEN}✅ PASS${NC}: $check_name"
            [ -n "$message" ] && echo "   $message"
            echo ""
        fi
        if [ "$OUTPUT_FORMAT" = "json" ]; then
            json_add_detail "$check_name" "$adr" "info" "$message" "$file" "" "$fix_guide"
        fi
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    elif [ "$result" = "WARN" ]; then
        if [ "$OUTPUT_FORMAT" = "text" ]; then
            echo -e "${YELLOW}⚠️  WARN${NC}: $check_name"
            [ -n "$message" ] && echo "   $message"
            echo ""
        fi
        if [ "$OUTPUT_FORMAT" = "json" ]; then
            json_add_detail "$check_name" "$adr" "warning" "$message" "$file" "" "$fix_guide"
        fi
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    else
        if [ "$OUTPUT_FORMAT" = "text" ]; then
            echo -e "${RED}❌ FAIL${NC}: $check_name"
            [ -n "$message" ] && echo "   $message"
            echo ""
        fi
        if [ "$OUTPUT_FORMAT" = "json" ]; then
            json_add_detail "$check_name" "$adr" "error" "$message" "$file" "" "$fix_guide"
        fi
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
    fi
}

# 1. 验证 arch-violations.md 结构完整性（ADR-0000.Y）
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo -e "${BLUE}[1/6] 验证 arch-violations.md 结构...${NC}"
fi

VIOLATIONS_FILE="docs/summaries/arch-violations.md"

if [ ! -f "$VIOLATIONS_FILE" ]; then
    check_result "arch-violations.md 存在性" "FAIL" "文件不存在" "ADR-0000" "$VIOLATIONS_FILE" "docs/adr/governance/ADR-0000-architecture-tests.md"
else
    # 检查必需章节
    required_sections=(
        "## 核心变更"
        "## 破例记录模板"
        "## CI 自动监控机制"
        "## 当前活跃破例"
        "## 已归还破例"
        "## 延期记录"
        "## 破例审批流程"
    )
    
    missing_sections=()
    for section in "${required_sections[@]}"; do
        if ! grep -q "^$section" "$VIOLATIONS_FILE"; then
            missing_sections+=("$section")
        fi
    done
    
    if [ ${#missing_sections[@]} -eq 0 ]; then
        check_result "arch-violations.md 结构完整性" "PASS" "所有必需章节存在" "ADR-0000" "$VIOLATIONS_FILE"
    else
        check_result "arch-violations.md 结构完整性" "FAIL" "缺失章节: ${missing_sections[*]}" "ADR-0000" "$VIOLATIONS_FILE" "docs/adr/governance/ADR-0000-architecture-tests.md"
    fi
    
    # 检查强制字段（表头）
    if grep -q "| ID | ADR | 规则 | 违规位置 | 原因 | 到期版本 | 负责人 | 偿还计划 | 审批人 | 状态 |" "$VIOLATIONS_FILE"; then
        check_result "arch-violations.md 强制字段" "PASS" "包含到期版本、负责人、偿还计划等强制字段" "ADR-0000" "$VIOLATIONS_FILE"
    else
        check_result "arch-violations.md 强制字段" "FAIL" "表头不符合 ADR-0000.Y 要求" "ADR-0000" "$VIOLATIONS_FILE" "docs/adr/governance/ADR-0000-architecture-tests.md"
    fi
fi

# 2. 验证 CI workflow 存在性
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo -e "${BLUE}[2/6] 验证治理相关 CI workflows...${NC}"
fi

required_workflows=(
    ".github/workflows/architecture-tests.yml"
    ".github/workflows/arch-violations-scanner.yml"
    ".github/workflows/adr-relationship-check.yml"
)

missing_workflows=()
for workflow in "${required_workflows[@]}"; do
    if [ ! -f "$workflow" ]; then
        missing_workflows+=("$workflow")
    fi
done

if [ ${#missing_workflows[@]} -eq 0 ]; then
    check_result "治理 CI workflows" "PASS" "所有必需 workflows 存在" "ADR-0000" "" "docs/adr/governance/ADR-0000-architecture-tests.md"
else
    check_result "治理 CI workflows" "FAIL" "缺失 workflows: ${missing_workflows[*]}" "ADR-0000" "" "docs/adr/governance/ADR-0000-architecture-tests.md"
fi

# 3. 验证治理 ADR 的依赖声明（ADR-900）
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo -e "${BLUE}[3/6] 验证治理 ADR 依赖声明...${NC}"
fi

GOVERNANCE_DIR="docs/adr/governance"

# 检查所有治理 ADR（除 ADR-0000）是否声明依赖 ADR-0000
governance_adrs=$(find "$GOVERNANCE_DIR" -name "ADR-*.md" -not -name "ADR-0000-*.md" -not -name "README.md" 2>/dev/null || true)

adrs_without_adr0000_dep=()
while IFS= read -r adr; do
    [ -z "$adr" ] && continue
    # 检查是否在 Relationships 章节依赖 ADR-0000
    if ! grep -A 20 "## 关系声明\|## Relationships" "$adr" 2>/dev/null | grep -q "ADR-0000"; then
        adr_name=$(basename "$adr")
        adrs_without_adr0000_dep+=("$adr_name")
    fi
done <<< "$governance_adrs"

if [ ${#adrs_without_adr0000_dep[@]} -eq 0 ]; then
    check_result "治理 ADR 依赖 ADR-0000" "PASS" "所有治理 ADR 正确声明依赖关系" "ADR-900" "" "docs/adr/governance/ADR-900-adr-process.md"
else
    check_result "治理 ADR 依赖 ADR-0000" "FAIL" "以下 ADR 未声明依赖 ADR-0000: ${adrs_without_adr0000_dep[*]}" "ADR-900" "" "docs/adr/governance/ADR-900-adr-process.md"
fi

# 4. 验证 README 无裁决力声明（ADR-910）
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo -e "${BLUE}[4/6] 验证 README 无裁决力声明（ADR-910）...${NC}"
fi

# 查找所有 README.md 文件（排除 node_modules 等）
readmes=$(find . -name "README.md" -not -path "*/node_modules/*" -not -path "*/.git/*" -not -path "*/bin/*" -not -path "*/obj/*" 2>/dev/null || true)

readmes_without_disclaimer=()
while IFS= read -r readme; do
    [ -z "$readme" ] && continue
    # 检查是否包含无裁决力声明
    if ! grep -qi "无裁决力\|no.*authority\|仅.*说明\|disclaimer" "$readme" 2>/dev/null; then
        readmes_without_disclaimer+=("$readme")
    fi
done <<< "$readmes"

if [ ${#readmes_without_disclaimer[@]} -eq 0 ]; then
    check_result "README 无裁决力声明" "PASS" "所有 README 包含权威边界声明" "ADR-910" "" "docs/adr/governance/ADR-910-readme-governance-constitution.md"
else
    # 只报告文档目录下的 README
    doc_readmes=()
    for readme in "${readmes_without_disclaimer[@]}"; do
        if [[ "$readme" == *"/docs/"* ]] || [[ "$readme" == "./README.md" ]]; then
            doc_readmes+=("$readme")
        fi
    done
    
    if [ ${#doc_readmes[@]} -eq 0 ]; then
        check_result "README 无裁决力声明" "PASS" "文档 README 包含权威边界声明" "ADR-910" "" "docs/adr/governance/ADR-910-readme-governance-constitution.md"
    else
        check_result "README 无裁决力声明" "FAIL" "以下文档 README 缺少声明: ${doc_readmes[*]}" "ADR-910" "" "docs/adr/governance/ADR-910-readme-governance-constitution.md"
    fi
fi

# 5. 验证治理 ADR 包含变更政策章节
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo -e "${BLUE}[5/6] 验证治理 ADR 变更政策...${NC}"
fi

adrs_without_change_policy=()
while IFS= read -r adr; do
    [ -z "$adr" ] && continue
    # 检查是否包含变更政策或权限章节
    if ! grep -qi "变更政策\|change.*policy\|修订权限\|审批要求" "$adr" 2>/dev/null; then
        adr_name=$(basename "$adr")
        # ADR-905, 945, 946 等补充性 ADR 可能不需要变更政策
        if [[ ! "$adr_name" =~ ADR-9[45][0-9] ]]; then
            adrs_without_change_policy+=("$adr_name")
        fi
    fi
done <<< "$governance_adrs"

if [ ${#adrs_without_change_policy[@]} -eq 0 ]; then
    check_result "治理 ADR 变更政策" "PASS" "核心治理 ADR 包含变更政策" "ADR-900" "" "docs/adr/governance/ADR-900-adr-process.md"
else
    check_result "治理 ADR 变更政策" "WARN" "以下 ADR 可能缺少变更政策说明: ${adrs_without_change_policy[*]}" "ADR-900" "" "docs/adr/governance/ADR-900-adr-process.md"
fi

# 6. 验证治理 ADR 测试映射（ADR-0000）
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo -e "${BLUE}[6/6] 验证治理 ADR 架构测试映射...${NC}"
fi

# 检查是否存在对应的治理测试
if [ -d "src/tests/ArchitectureTests/ADR" ]; then
    # 检查 ADR-0000 相关测试
    if ls src/tests/ArchitectureTests/ADR/*0000* 1> /dev/null 2>&1; then
        check_result "ADR-0000 测试映射" "PASS" "存在 ADR-0000 相关测试" "ADR-0000" "" "docs/adr/governance/ADR-0000-architecture-tests.md"
    else
        check_result "ADR-0000 测试映射" "WARN" "未找到 ADR-0000 专门测试（可能集成在其他测试中）" "ADR-0000" "" "docs/adr/governance/ADR-0000-architecture-tests.md"
    fi
else
    check_result "架构测试目录" "FAIL" "架构测试目录不存在" "ADR-0000" "src/tests/ArchitectureTests/ADR" "docs/adr/governance/ADR-0000-architecture-tests.md"
fi

# 总结
if [ "$OUTPUT_FORMAT" = "text" ]; then
    echo ""
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}验证总结${NC}"
    echo -e "${BLUE}========================================${NC}"
    echo ""
    echo "总检查项: $TOTAL_CHECKS"
    echo -e "${GREEN}通过: $PASSED_CHECKS${NC}"
    echo -e "${RED}失败: $FAILED_CHECKS${NC}"
    echo ""
    
    if [ $FAILED_CHECKS -eq 0 ]; then
        echo -e "${GREEN}✅ 治理合规性验证通过${NC}"
    else
        echo -e "${RED}❌ 治理合规性验证失败${NC}"
        echo ""
        echo "请根据上述失败项进行修复："
        echo "1. 确保 arch-violations.md 包含所有必需字段和章节"
        echo "2. 确保所有治理 CI workflows 存在"
        echo "3. 确保治理 ADR 正确声明依赖关系"
        echo "4. 确保 README 包含无裁决力声明"
        echo "5. 确保核心治理 ADR 包含变更政策"
        echo ""
        echo "参考资源："
        echo "- ADR-0000: docs/adr/governance/ADR-0000-architecture-tests.md"
        echo "- ADR-900: docs/adr/governance/ADR-900-adr-process.md"
        echo "- ADR-910: docs/adr/governance/ADR-910-readme-governance-constitution.md"
        echo "- ADR-930: docs/adr/governance/ADR-930-code-review-compliance.md"
        echo ""
    fi
else
    # JSON 输出
    status=$(json_determine_status)
    if [ -n "$OUTPUT_FILE" ]; then
        json_save "$status" "$OUTPUT_FILE"
    else
        json_finalize "$status"
    fi
fi

# 退出码
if [ $FAILED_CHECKS -eq 0 ]; then
    exit 0
else
    exit 1
fi
