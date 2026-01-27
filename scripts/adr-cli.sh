#!/bin/bash

# ADR 管理 CLI 工具
# 
# 此工具提供统一的 ADR 创建、修订和管理入口，确保：
# 1. 自动分配正确的编号
# 2. 自动填充元数据模板
# 3. 创建对应的测试和 Prompt 文件
# 4. 确保编号、目录、内容三元一致

set -e

# 定义路径
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
ADR_PATH="$REPO_ROOT/docs/adr"
TESTS_PATH="$REPO_ROOT/src/tests/ArchitectureTests/ADR"
PROMPTS_PATH="$REPO_ROOT/docs/copilot"
TEMPLATE_PATH="$REPO_ROOT/docs/templates/adr-template.md"
PROMPT_TEMPLATE_PATH="$REPO_ROOT/docs/templates/copilot-pormpts-template.md"

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BLUE='\033[0;34m'
NC='\033[0m'

# 输出函数
function log_success() { echo -e "${GREEN}✅ $1${NC}"; }
function log_warning() { echo -e "${YELLOW}⚠️  $1${NC}"; }
function log_error() { echo -e "${RED}❌ $1${NC}"; }
function log_info() { echo -e "${CYAN}ℹ️  $1${NC}"; }

# 层级定义
declare -A TIER_RANGES
TIER_RANGES["constitutional"]="0001-0099"
TIER_RANGES["structure"]="0100-0199"
TIER_RANGES["runtime"]="0200-0299"
TIER_RANGES["technical"]="0300-0399"
TIER_RANGES["governance"]="0900-0999"

# 显示使用说明
function show_usage() {
    echo -e "${CYAN}ADR 管理 CLI 工具${NC}"
    echo ""
    echo "用法："
    echo "  $0 create <tier> <title>       - 创建新 ADR"
    echo "  $0 next-number <tier>          - 查询指定层级的下一个可用编号"
    echo "  $0 list [tier]                 - 列出所有 ADR（可选指定层级）"
    echo "  $0 validate                    - 验证 ADR 一致性"
    echo ""
    echo "层级 (tier) 选项："
    echo "  constitutional  - 宪法层 (0001-0099)"
    echo "  structure       - 结构层 (0100-0199)"
    echo "  runtime         - 运行层 (0200-0299)"
    echo "  technical       - 技术层 (0300-0399)"
    echo "  governance      - 治理层 (0900-0999)"
    echo ""
    echo "示例："
    echo "  $0 create constitutional \"模块隔离约束\""
    echo "  $0 next-number structure"
    echo "  $0 list constitutional"
}

# 获取指定层级的下一个可用编号
function get_next_number() {
    local tier="$1"
    local range="${TIER_RANGES[$tier]}"
    
    if [ -z "$range" ]; then
        log_error "无效的层级：$tier"
        return 1
    fi
    
    # 解析范围
    IFS='-' read -r start end <<< "$range"
    start=$((10#$start))
    end=$((10#$end))
    
    # 获取已有编号
    local existing_numbers=()
    while IFS= read -r file; do
        local number=$(basename "$file" | sed -n 's/^ADR-\([0-9]\{4\}\).*/\1/p')
        if [ -n "$number" ]; then
            number=$((10#$number))
            if [ "$number" -ge "$start" ] && [ "$number" -le "$end" ]; then
                existing_numbers+=($number)
            fi
        fi
    done < <(find "$ADR_PATH/$tier" -type f -name "ADR-*.md" 2>/dev/null | sort)
    
    # 找到第一个可用编号
    for ((num=start; num<=end; num++)); do
        local found=false
        for existing in "${existing_numbers[@]}"; do
            if [ "$existing" -eq "$num" ]; then
                found=true
                break
            fi
        done
        if [ "$found" = false ]; then
            printf "%04d" $num
            return 0
        fi
    done
    
    log_error "层级 $tier 已满（范围：$start-$end）"
    return 1
}

# 列出 ADR
function list_adrs() {
    local tier="$1"
    
    log_info "ADR 文档列表："
    echo ""
    
    if [ -n "$tier" ]; then
        # 列出指定层级
        if [ ! -d "$ADR_PATH/$tier" ]; then
            log_error "层级目录不存在：$tier"
            return 1
        fi
        
        echo -e "${CYAN}$tier:${NC}"
        find "$ADR_PATH/$tier" -type f -name "ADR-*.md" -exec basename {} \; | sort
    else
        # 列出所有层级
        for tier in constitutional structure runtime technical governance; do
            if [ -d "$ADR_PATH/$tier" ]; then
                echo -e "${CYAN}$tier:${NC}"
                find "$ADR_PATH/$tier" -type f -name "ADR-*.md" -exec basename {} \; | sort
                echo ""
            fi
        done
    fi
}

# 创建新 ADR
function create_adr() {
    local tier="$1"
    local title="$2"
    
    if [ -z "$tier" ] || [ -z "$title" ]; then
        log_error "缺少必需参数"
        show_usage
        return 1
    fi
    
    if [ -z "${TIER_RANGES[$tier]}" ]; then
        log_error "无效的层级：$tier"
        show_usage
        return 1
    fi
    
    # 获取下一个可用编号
    log_info "为层级 $tier 分配编号..."
    local number=$(get_next_number "$tier")
    if [ $? -ne 0 ]; then
        return 1
    fi
    
    log_success "分配编号：ADR-$number"
    
    # 生成文件名（转换标题为 slug）
    local slug=$(echo "$title" | iconv -t ascii//TRANSLIT//IGNORE 2>/dev/null | sed 's/[^a-zA-Z0-9\u4e00-\u9fa5]/-/g' | sed 's/-\+/-/g' | sed 's/^-\|-$//g')
    local filename="ADR-$number-$slug.md"
    local filepath="$ADR_PATH/$tier/$filename"
    
    # 检查目录
    mkdir -p "$ADR_PATH/$tier"
    
    # 检查文件是否已存在
    if [ -f "$filepath" ]; then
        log_error "文件已存在：$filepath"
        return 1
    fi
    
    # 创建 ADR 文件
    log_info "创建 ADR 文档..."
    
    # 获取当前日期
    local date=$(date +%Y-%m-%d)
    
    # 根据层级设置级别
    local level
    case "$tier" in
        constitutional)
            level="Constitutional"
            ;;
        governance)
            level="Governance"
            ;;
        structure)
            level="Structure"
            ;;
        runtime)
            level="Runtime"
            ;;
        technical)
            level="Technical"
            ;;
        *)
            level="Governance"
            ;;
    esac
    
    # 从模板创建，填充基本信息
    if [ -f "$TEMPLATE_PATH" ]; then
        cp "$TEMPLATE_PATH" "$filepath"
        # 替换模板中的占位符
        sed -i "s/ADR-XXXX/ADR-$number/g" "$filepath"
        sed -i "s/〈裁决型标题〉/$title/g" "$filepath"
        sed -i "s/YYYY-MM-DD/$date/g" "$filepath"
        sed -i "s/level: Constitutional/level: $level/g" "$filepath"
    else
        log_warning "模板文件不存在，创建简单版本"
        cat > "$filepath" << EOF
---
adr: ADR-$number
title: "$title"
status: Draft
level: $level
deciders: "Architecture Board"
date: $date
version: "1.0"
maintainer: "Architecture Board"
reviewer: "待定"
supersedes: null
superseded_by: null
---

# ADR-$number：$title

> ⚖️ 权威声明：〈一句话说明本 ADR 的权威性和适用范围〉

---

## 聚焦内容（Focus）

本 ADR 聚焦于解决以下问题：

* 〈待填写〉

**适用范围**：

* 〈明确列出适用的模块 / 层 / 仓库〉

---

## 术语表（Glossary）

| 术语 | 定义 | 英文对照 |
|-----|-----|---------|

---

## 裁决（Decision）

> ⚠️ **本节是唯一裁决来源，其他章节不得产生新规则。**

### 规则列表

**ADR-$number.1：〈规则标题〉**
〈使用"必须 / 禁止 / 不允许 / 应当"等明确语义描述规则〉

---

## 执法模型（Enforcement）

### 测试映射

| 规则编号 | 执行级 | 测试 / 手段 |
|---------|-------|-----------|
| ADR-$number.1 | L1 | （待定） |

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：

* （待填写）

---

## 禁止行为（Prohibited）

ADR 中 **严禁**：

* （待填写）

---

## 关系声明（Relationships）

**依赖（Depends On）**：

* 无

**被依赖（Depended By）**：

* 无

**替代（Supersedes）**：

* 无

**被替代（Superseded By）**：

* 无

**相关（Related）**：

* 无

---

## 非裁决性参考（References）

* （待填写）

---

## 版本历史（History）

| 版本 | 日期 | 变更说明 | 作者 |
|-----|------|---------|------|
| 1.0 | $date | 初始版本 | 〈作者〉 |
EOF
    fi
    
    log_success "创建 ADR 文档：$filepath"
    
    # 创建对应的 Prompt 文件
    log_info "创建 Copilot Prompt 文件..."
    local prompt_file="$PROMPTS_PATH/adr-$((10#$number)).prompts.md"
    
    if [ -f "$PROMPT_TEMPLATE_PATH" ]; then
        cp "$PROMPT_TEMPLATE_PATH" "$prompt_file"
        sed -i "s/ADR-XXXX/ADR-$number/g" "$prompt_file"
        sed -i "s/〈ADR 标题〉/$title/g" "$prompt_file"
    else
        cat > "$prompt_file" << EOF
# ADR-$number Copilot Prompts

> 本文档为 GitHub Copilot 提供 ADR-$number ($title) 的场景化提示词

## ADR 简介

ADR-$number 定义了 $title 相关的架构约束。

## 关键约束

（待填写...）

## 常见场景

### 场景 1：（待定）

（待填写...）

## 相关资源

- [ADR-$number 正文](../adr/$tier/ADR-$number-$slug.md)
- [架构测试](../../src/tests/ArchitectureTests/ADR/ADR_${number}_Architecture_Tests.cs)
EOF
    fi
    
    log_success "创建 Prompt 文件：$prompt_file"
    
    # 提示创建测试文件
    log_info "下一步操作："
    echo ""
    echo "1. 编辑 ADR 文档："
    echo "   $filepath"
    echo ""
    echo "2. 如需要架构测试覆盖，创建测试文件："
    echo "   $TESTS_PATH/ADR_${number}_Architecture_Tests.cs"
    echo ""
    echo "3. 完善 Copilot Prompt 文件："
    echo "   $prompt_file"
    echo ""
    echo "4. 运行验证："
    echo "   $SCRIPT_DIR/validate-adr-consistency.sh"
    echo "   $SCRIPT_DIR/validate-three-way-mapping.sh"
}

# 主函数
function main() {
    local command="$1"
    shift
    
    case "$command" in
        create)
            create_adr "$@"
            ;;
        next-number)
            local tier="$1"
            if [ -z "$tier" ]; then
                log_error "缺少层级参数"
                show_usage
                return 1
            fi
            local number=$(get_next_number "$tier")
            if [ $? -eq 0 ]; then
                echo "下一个可用编号：$number"
            fi
            ;;
        list)
            list_adrs "$1"
            ;;
        validate)
            log_info "运行 ADR 一致性验证..."
            "$SCRIPT_DIR/validate-adr-consistency.sh"
            echo ""
            log_info "运行三位一体映射验证..."
            "$SCRIPT_DIR/validate-three-way-mapping.sh"
            ;;
        help|--help|-h|"")
            show_usage
            ;;
        *)
            log_error "未知命令：$command"
            show_usage
            return 1
            ;;
    esac
}

# 执行
main "$@"
