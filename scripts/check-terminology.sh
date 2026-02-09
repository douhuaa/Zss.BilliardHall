#!/bin/bash
# ADR 术语一致性检查脚本

set -e

RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}🔍 开始 ADR 术语一致性检查...${NC}\n"

ADR_DIR="docs/adr"
TEMP_FILE="/tmp/all-terms-$$.txt"

# 清理临时文件
cleanup() {
    rm -f "$TEMP_FILE" "${TEMP_FILE}.sorted" "${TEMP_FILE}.dupes"
}
trap cleanup EXIT

echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}检查 1: 提取所有术语定义${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

# 提取所有 ADR 中的术语表
term_count=0
for adr in $(find "$ADR_DIR" -type f -name "ADR-*.md" | sort); do
    adr_name=$(basename "$adr" .md)
    
    # 检查是否有术语表
    if grep -q "## 术语表" "$adr"; then
        # 提取术语表内容（从 ## 术语表 到下一个 ## 之间的表格行）
        in_glossary=0
        while IFS= read -r line; do
            if [[ "$line" =~ ^##[[:space:]]*术语表 ]]; then
                in_glossary=1
                continue
            elif [[ "$line" =~ ^## ]] && [ $in_glossary -eq 1 ]; then
                break
            elif [ $in_glossary -eq 1 ] && [[ "$line" =~ ^\|.*\|.*\| ]]; then
                # 跳过表头和分隔线
                if [[ ! "$line" =~ ^[[:space:]]*\|[[:space:]]*术语 ]] && [[ ! "$line" =~ ^[[:space:]]*\|[[:space:]]*:?-+:? ]]; then
                    # 提取第一列（术语）
                    term=$(echo "$line" | cut -d'|' -f2 | sed 's/^[[:space:]]*//; s/[[:space:]]*$//')
                    if [ -n "$term" ]; then
                        echo "$adr_name$term" >> "$TEMP_FILE"
                        term_count=$((term_count + 1))
                    fi
                fi
            fi
        done < "$adr"
    fi
done

if [ $term_count -eq 0 ]; then
    echo -e "${YELLOW}⚠️  未找到任何术语定义${NC}\n"
    exit 0
fi

echo -e "${GREEN}✅ 提取了 $term_count 个术语定义${NC}\n"

echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}检查 2: 查找重复定义的术语${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

# 查找在多个 ADR 中定义的术语
cut -f2 "$TEMP_FILE" | sort | uniq -d > "${TEMP_FILE}.dupes"

duplicate_count=$(wc -l < "${TEMP_FILE}.dupes")

if [ $duplicate_count -eq 0 ]; then
    echo -e "${GREEN}✅ 未发现重复定义的术语${NC}\n"
else
    echo -e "${YELLOW}⚠️  发现 $duplicate_count 个术语在多个 ADR 中定义：${NC}\n"
    
    while IFS= read -r term; do
        echo -e "${YELLOW}术语 '$term' 在以下 ADR 中定义：${NC}"
        grep "$term$" "$TEMP_FILE" | cut -f1 | while read adr; do
            echo "  • $adr"
        done
        echo ""
    done < "${TEMP_FILE}.dupes"
fi

echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}检查 3: 验证术语表格式（英文对照）${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

missing_english=0

for adr in $(find "$ADR_DIR" -type f -name "ADR-*.md" | sort); do
    adr_name=$(basename "$adr")
    
    if grep -q "## 术语表" "$adr"; then
        # 检查术语表是否有三列（包含英文对照）
        if ! grep -A 2 "## 术语表" "$adr" | grep -q "| 术语.*| 定义.*| 英文对照 |"; then
            echo -e "${YELLOW}⚠️  $adr_name 术语表缺少英文对照列${NC}"
            missing_english=$((missing_english + 1))
        fi
    fi
done

if [ $missing_english -eq 0 ]; then
    echo -e "${GREEN}✅ 所有术语表都包含英文对照${NC}\n"
else
    echo -e "${YELLOW}⚠️  发现 $missing_english 个术语表缺少英文对照${NC}\n"
fi

# 总结
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}检查总结${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

total_issues=$((duplicate_count + missing_english))

if [ $total_issues -eq 0 ]; then
    echo -e "${GREEN}✅ 术语一致性检查通过！${NC}"
    exit 0
else
    echo -e "${YELLOW}⚠️  发现 $total_issues 个术语相关问题${NC}"
    echo -e "${YELLOW}📋 建议：${NC}"
    if [ $duplicate_count -gt 0 ]; then
        echo -e "${YELLOW}  • 在 ADR-006 中建立权威术语表${NC}"
        echo -e "${YELLOW}  • 其他 ADR 应引用而非重复定义${NC}"
    fi
    if [ $missing_english -gt 0 ]; then
        echo -e "${YELLOW}  • 按 ADR-006 标准为术语表增加英文对照列${NC}"
    fi
    exit 0
fi
