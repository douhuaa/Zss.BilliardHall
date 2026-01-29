#!/bin/bash
# ADR 一致性检查脚本
# 检查 Front Matter、术语表格式、版本号等

set -e

RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🔍 开始 ADR 一致性检查...${NC}\n"

ISSUES_FOUND=0
ADR_DIR="docs/adr"

# 检查 1：Front Matter 完整性
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}检查 1: Front Matter 完整性${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

check_front_matter() {
    local missing_count=0
    local checked_count=0
    
    for adr in $(find "$ADR_DIR" -type f -name "ADR-*.md" | sort); do
        checked_count=$((checked_count + 1))
        adr_name=$(basename "$adr")
        
        # 检查是否有 Front Matter (以 --- 开头)
        if ! head -1 "$adr" | grep -q "^---$"; then
            echo -e "${RED}❌ $adr_name 缺少 Front Matter${NC}"
            ISSUES_FOUND=$((ISSUES_FOUND + 1))
            missing_count=$((missing_count + 1))
        fi
    done
    
    if [ $missing_count -eq 0 ]; then
        echo -e "${GREEN}✅ 所有 ADR 都包含 Front Matter${NC}"
    else
        echo -e "${YELLOW}⚠️  发现 $missing_count 个 ADR 缺少 Front Matter（共检查 $checked_count 个）${NC}"
    fi
    echo ""
}

# 检查 2：术语表格式
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}检查 2: 术语表格式（ADR-0006 标准）${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

check_glossary_format() {
    local invalid_count=0
    local has_glossary_count=0
    
    for adr in $(find "$ADR_DIR" -type f -name "ADR-*.md" | sort); do
        adr_name=$(basename "$adr")
        
        # 检查是否有术语表章节（支持带括号的标题，如 ## 术语表（Glossary））
        if grep -q "## 术语表" "$adr"; then
            has_glossary_count=$((has_glossary_count + 1))
            
            # 检查是否有标准三列格式：术语 | 定义 | 英文对照
            # 使用更宽松的匹配，支持中文标点和空格，并查找整个术语表章节（最多100行）
            if ! grep -A 100 "## 术语表" "$adr" | grep -q "英文对照"; then
                echo -e "${YELLOW}⚠️  $adr_name 术语表格式不符合 ADR-0006（缺少英文对照列）${NC}"
                ISSUES_FOUND=$((ISSUES_FOUND + 1))
                invalid_count=$((invalid_count + 1))
            fi
        fi
    done
    
    if [ $invalid_count -eq 0 ] && [ $has_glossary_count -gt 0 ]; then
        echo -e "${GREEN}✅ 所有术语表格式符合标准${NC}"
    elif [ $invalid_count -gt 0 ]; then
        echo -e "${YELLOW}⚠️  发现 $invalid_count 个术语表格式不符合 ADR-0006（共 $has_glossary_count 个有术语表的 ADR）${NC}"
    else
        echo -e "${BLUE}ℹ️  未发现包含术语表的 ADR${NC}"
    fi
    echo ""
}

# 检查 3：版本号格式
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}检查 3: 版本号格式（ADR-980 标准）${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

check_version_format() {
    local invalid_count=0
    
    for adr in $(find "$ADR_DIR" -type f -name "ADR-*.md" | sort); do
        adr_name=$(basename "$adr")
        
        # 查找版本号（在 Front Matter 或正文中）
        if grep -q "version:" "$adr"; then
            version=$(grep "version:" "$adr" | head -1 | sed 's/.*version: *//' | tr -d '"' | tr -d "'")
            
            # 检查是否包含 'v' 前缀
            if echo "$version" | grep -q "^v"; then
                echo -e "${RED}❌ $adr_name 版本号包含 'v' 前缀: $version（应为 ${version#v}）${NC}"
                ISSUES_FOUND=$((ISSUES_FOUND + 1))
                invalid_count=$((invalid_count + 1))
            fi
            
            # 检查是否为有效格式 X.Y 或 X.Y.Z
            if ! echo "$version" | grep -qE "^[0-9]+\.[0-9]+(\.[0-9]+)?$"; then
                echo -e "${YELLOW}⚠️  $adr_name 版本号格式可能不规范: $version${NC}"
                invalid_count=$((invalid_count + 1))
            fi
        elif grep -qE "\*\*版本\*\*" "$adr"; then
            version=$(grep -E "\*\*版本\*\*" "$adr" | head -1 | sed 's/.*：//' | sed 's/ .*//')
            
            if echo "$version" | grep -q "^v"; then
                echo -e "${RED}❌ $adr_name 版本号包含 'v' 前缀: $version（应为 ${version#v}）${NC}"
                ISSUES_FOUND=$((ISSUES_FOUND + 1))
                invalid_count=$((invalid_count + 1))
            fi
        fi
    done
    
    if [ $invalid_count -eq 0 ]; then
        echo -e "${GREEN}✅ 所有版本号格式正确${NC}"
    else
        echo -e "${YELLOW}⚠️  发现 $invalid_count 个版本号格式问题${NC}"
    fi
    echo ""
}

# 检查 4：快速参考表（ADR-0006 要求）
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}检查 4: 快速参考表（ADR-0006 推荐）${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

check_quick_reference() {
    local missing_count=0
    local constitutional_count=0
    
    # 只检查宪法层 ADR（0001-0099）
    for adr in $(find "$ADR_DIR/constitutional" -type f -name "ADR-*.md" 2>/dev/null | sort); do
        constitutional_count=$((constitutional_count + 1))
        adr_name=$(basename "$adr")
        
        # 检查是否有"快速参考"或"Quick Reference"章节
        if ! grep -qiE "##.*快速参考|##.*Quick Reference" "$adr"; then
            echo -e "${YELLOW}⚠️  $adr_name 缺少快速参考表章节${NC}"
            missing_count=$((missing_count + 1))
        fi
    done
    
    if [ $constitutional_count -eq 0 ]; then
        echo -e "${BLUE}ℹ️  未找到宪法层 ADR 目录${NC}"
    elif [ $missing_count -eq 0 ]; then
        echo -e "${GREEN}✅ 所有宪法层 ADR 都包含快速参考表${NC}"
    else
        echo -e "${YELLOW}⚠️  发现 $missing_count 个宪法层 ADR 缺少快速参考表（共 $constitutional_count 个）${NC}"
    fi
    echo ""
}

# 执行所有检查
check_front_matter
check_glossary_format
check_version_format
check_quick_reference

# 输出总结
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}检查总结${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

if [ $ISSUES_FOUND -eq 0 ]; then
    echo -e "${GREEN}✅ ADR 一致性检查通过！未发现严重问题。${NC}"
    exit 0
else
    echo -e "${RED}❌ ADR 一致性检查发现 $ISSUES_FOUND 个问题需要修复。${NC}"
    echo -e "${YELLOW}📋 请参阅 docs/reports/adr-synchronization-analysis-2026-01-29.md 了解详细整改建议。${NC}"
    exit 1
fi
