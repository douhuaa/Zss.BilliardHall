#!/bin/bash

# ADR 治理体系全面验证脚本
#
# 此脚本运行所有验证工具并生成综合状态报告

set -e

# 定义路径
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BLUE='\033[0;34m'
NC='\033[0m'

# 输出函数
function log_success() { echo -e "${GREEN}✅ $1${NC}"; }
function log_error() { echo -e "${RED}❌ $1${NC}"; }
function log_warning() { echo -e "${YELLOW}⚠️  $1${NC}"; }
function log_info() { echo -e "${CYAN}ℹ️  $1${NC}"; }
function log_header() { echo -e "${BLUE}$1${NC}"; }

# 统计变量
TOTAL_CHECKS=0
PASSED_CHECKS=0
FAILED_CHECKS=0

# 运行单个检查
function run_check() {
    local name="$1"
    local command="$2"
    
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    
    log_info "运行：$name"
    
    if eval "$command" > /dev/null 2>&1; then
        log_success "$name - 通过"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
        return 0
    else
        log_error "$name - 失败"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
        return 1
    fi
}

# 主函数
function main() {
    clear
    
    log_header "╔═══════════════════════════════════════════════════════════╗"
    log_header "║        ADR 治理体系全面验证                               ║"
    log_header "╚═══════════════════════════════════════════════════════════╝"
    echo ""
    
    log_info "开始验证..."
    echo ""
    
    # 1. ADR 一致性检查
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    log_header "1. ADR 编号/目录/内容一致性检查"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    if "$SCRIPT_DIR/validate-adr-consistency.sh"; then
        log_success "ADR 一致性检查通过"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    else
        log_error "ADR 一致性检查失败"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
    fi
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    echo ""
    
    # 2. 三位一体映射验证
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    log_header "2. ADR/测试/Prompt 三位一体映射验证"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    if "$SCRIPT_DIR/validate-three-way-mapping.sh"; then
        log_success "三位一体映射验证通过"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    else
        log_error "三位一体映射验证失败"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
    fi
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    echo ""
    
    # 3. ADR 关系管理检查（ADR-940）
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    log_header "3. ADR 关系管理检查（ADR-940）"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    if "$SCRIPT_DIR/verify-adr-relationships.sh"; then
        log_success "ADR 关系声明检查通过"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    else
        log_error "ADR 关系声明检查失败"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
    fi
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    echo ""
    
    if "$SCRIPT_DIR/check-relationship-consistency.sh"; then
        log_success "关系双向一致性检查通过"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    else
        log_error "关系双向一致性检查失败"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
    fi
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    echo ""
    
    if "$SCRIPT_DIR/detect-circular-dependencies.sh"; then
        log_success "循环依赖检测通过"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    else
        log_error "循环依赖检测失败"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
    fi
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    echo ""
    
    # 4. 版本同步检查（ADR-980）
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    log_header "4. 版本同步检查（ADR-980）"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    if "$SCRIPT_DIR/validate-adr-version-sync.sh"; then
        log_success "版本同步检查通过"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    else
        log_error "版本同步检查失败"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
    fi
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    echo ""
    
    # 5. 工具可用性检查
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    log_header "5. 工具可用性检查"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    
    run_check "ADR CLI 工具" "test -x $SCRIPT_DIR/adr-cli.sh"
    run_check "健康报告生成器" "test -x $SCRIPT_DIR/generate-health-report.sh"
    run_check "速查手册生成器" "test -x $SCRIPT_DIR/generate-quick-reference.sh"
    run_check "一致性检查器" "test -x $SCRIPT_DIR/validate-adr-consistency.sh"
    run_check "映射验证器" "test -x $SCRIPT_DIR/validate-three-way-mapping.sh"
    run_check "关系验证器（ADR-940）" "test -x $SCRIPT_DIR/verify-adr-relationships.sh"
    run_check "关系一致性检查器（ADR-940）" "test -x $SCRIPT_DIR/check-relationship-consistency.sh"
    run_check "循环依赖检测器（ADR-940）" "test -x $SCRIPT_DIR/detect-circular-dependencies.sh"
    run_check "关系图生成器（ADR-940）" "test -x $SCRIPT_DIR/generate-adr-relationship-map.sh"
    run_check "版本同步验证器（ADR-980）" "test -x $SCRIPT_DIR/validate-adr-version-sync.sh"
    echo ""
    
    # 6. 文档完整性检查
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    log_header "6. 文档完整性检查"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    
    run_check "工具 README" "test -f $SCRIPT_DIR/README.md"
    run_check "工具使用指南" "test -f $REPO_ROOT/docs/ADR-TOOLING-GUIDE.md"
    run_check "实施总结文档" "test -f $REPO_ROOT/docs/summaries/adr-automation-implementation.md"
    run_check "ADR 模板" "test -f $REPO_ROOT/docs/templates/adr-template.md"
    run_check "Prompt 模板" "test -f $REPO_ROOT/docs/templates/copilot-pormpts-template.md"
    run_check "FAQs 目录（ADR-950）" "test -d $REPO_ROOT/docs/faqs"
    run_check "Cases 目录（ADR-950）" "test -d $REPO_ROOT/docs/cases"
    run_check "Guides 目录（ADR-950）" "test -d $REPO_ROOT/docs/guides"
    run_check "ADR 关系图（ADR-940）" "test -f $REPO_ROOT/docs/adr/ADR-RELATIONSHIP-MAP.md"
    echo ""
    
    # 7. CI 集成检查
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    log_header "7. CI/CD 集成检查"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    
    run_check "GitHub Actions 工作流" "test -f $REPO_ROOT/.github/workflows/architecture-tests.yml"
    run_check "ADR 关系检查工作流（ADR-940）" "test -f $REPO_ROOT/.github/workflows/adr-relationship-check.yml"
    run_check "ADR 版本同步工作流（ADR-980）" "test -f $REPO_ROOT/.github/workflows/adr-version-sync.yml"
    run_check "PR 模板" "test -f $REPO_ROOT/.github/PULL_REQUEST_TEMPLATE.md"
    run_check "CODEOWNERS（ADR-980）" "test -f $REPO_ROOT/CODEOWNERS"
    
    # 检查 CI 工作流中是否包含新工具
    if grep -q "validate-adr-consistency.sh" "$REPO_ROOT/.github/workflows/architecture-tests.yml"; then
        log_success "CI 工作流已集成一致性检查"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    else
        log_warning "CI 工作流未集成一致性检查"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
    fi
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    
    if grep -q "validate-three-way-mapping.sh" "$REPO_ROOT/.github/workflows/architecture-tests.yml"; then
        log_success "CI 工作流已集成三位一体映射验证"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    else
        log_warning "CI 工作流未集成三位一体映射验证"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
    fi
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    echo ""
    
    # 输出总结
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    log_header "验证总结"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""
    echo "总检查项数：$TOTAL_CHECKS"
    echo -ne "通过：${GREEN}$PASSED_CHECKS${NC}"
    echo ""
    echo -ne "失败：${RED}$FAILED_CHECKS${NC}"
    echo ""
    echo ""
    
    local pass_rate=0
    if [ $TOTAL_CHECKS -gt 0 ]; then
        pass_rate=$((PASSED_CHECKS * 100 / TOTAL_CHECKS))
    fi
    
    echo "通过率：$pass_rate%"
    echo ""
    
    if [ $FAILED_CHECKS -eq 0 ]; then
        log_success "所有检查通过！ADR 治理体系状态良好 🎉"
        echo ""
        echo "建议下一步："
        echo "  1. 定期运行此脚本（每周一次）"
        echo "  2. 每月生成健康报告"
        echo "  3. 根据报告持续改进"
    elif [ $pass_rate -ge 80 ]; then
        log_warning "大部分检查通过，但仍有 $FAILED_CHECKS 项需要改进"
        echo ""
        echo "建议："
        echo "  1. 查看上方失败的检查项"
        echo "  2. 根据工具输出修复问题"
        echo "  3. 重新运行验证"
    else
        log_error "检查通过率较低，需要立即处理"
        echo ""
        echo "紧急措施："
        echo "  1. 查看详细的验证报告"
        echo "  2. 参考工具使用指南修复"
        echo "  3. 咨询架构委员会"
    fi
    
    echo ""
    echo -e "${CYAN}更多信息：${NC}"
    echo "  - 工具概览：scripts/README.md"
    echo "  - 使用指南：docs/ADR-TOOLING-GUIDE.md"
    echo "  - 实施总结：docs/summaries/adr-automation-implementation.md"
    echo ""
    
    if [ $FAILED_CHECKS -gt 0 ]; then
        return 1
    fi
    
    return 0
}

# 执行
main
exit $?
