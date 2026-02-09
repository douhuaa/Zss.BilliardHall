#!/bin/bash

# ADR 可裁决性速查工具
#
# 此脚本从 ADR 文档中提取并展示：
# 1. 红线约束（必须遵守的硬性规则）
# 2. 建议性约束（推荐但不强制）
# 3. 需要架构测试覆盖的条款
# 4. 人工审核门控点

set -e

# 定义路径
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
ADR_PATH="$REPO_ROOT/docs/adr"
OUTPUT_FILE="${1:-}"

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BLUE='\033[0;34m'
NC='\033[0m'

# 输出函数
function log_info() { echo -e "${CYAN}ℹ️  $1${NC}"; }
function log_success() { echo -e "${GREEN}✅ $1${NC}"; }

# 提取 ADR 编号
function extract_adr_number() {
    local file="$1"
    basename "$file" | sed -n 's/^ADR[-_]\?\([0-9]\{4\}\).*/\1/p'
}

# 提取 ADR 标题
function extract_adr_title() {
    local file="$1"
    grep -m 1 "^# ADR-" "$file" | sed 's/^# ADR-[0-9]\{4\}[：:]\s*//'
}

# 生成速查手册
function generate_quick_reference() {
    cat << 'EOF'
# ADR 可裁决性速查手册

**生成时间**：
EOF
    date '+%Y-%m-%d %H:%M:%S'
    cat << 'EOF'

**用途**：为开发者和审核者提供快速参考，明确哪些约束是硬性要求，哪些需要测试覆盖。

---

## 使用说明

### 约束级别说明

- 🔴 **红线约束（MUST/MUST NOT）**：绝对禁止违反，违反即为架构违规
- 🟡 **建议约束（SHOULD/SHOULD NOT）**：强烈建议遵守，但特殊情况可申请例外
- 🔵 **可选约束（MAY）**：推荐但不强制
- ✅ **必须测试覆盖**：标记为【必须架构测试覆盖】的条款
- 🚧 **人工审核门控**：需要人工判断的场景

---

EOF
    
    # 按层级组织 ADR
    for tier in constitutional structure runtime technical governance; do
        local tier_path="$ADR_PATH/$tier"
        if [ ! -d "$tier_path" ]; then
            continue
        fi
        
        local tier_name=""
        case "$tier" in
            constitutional) tier_name="宪法层" ;;
            structure) tier_name="结构层" ;;
            runtime) tier_name="运行层" ;;
            technical) tier_name="技术层" ;;
            governance) tier_name="治理层" ;;
        esac
        
        echo "## $tier_name ($tier)"
        echo ""
        
        local has_content=false
        
        while IFS= read -r file; do
            local number=$(extract_adr_number "$file")
            local title=$(extract_adr_title "$file")
            
            # 提取约束
            local must_constraints=$(extract_must_constraints "$file")
            local should_constraints=$(extract_should_constraints "$file")
            local must_test_items=$(extract_must_test_items "$file")
            
            if [ -n "$must_constraints" ] || [ -n "$should_constraints" ] || [ -n "$must_test_items" ]; then
                echo "### ADR-$number：$title"
                echo ""
                
                # 红线约束
                if [ -n "$must_constraints" ]; then
                    echo "#### 🔴 红线约束"
                    echo ""
                    echo "$must_constraints"
                    echo ""
                fi
                
                # 建议约束
                if [ -n "$should_constraints" ]; then
                    echo "#### 🟡 建议约束"
                    echo ""
                    echo "$should_constraints"
                    echo ""
                fi
                
                # 必须测试覆盖
                if [ -n "$must_test_items" ]; then
                    echo "#### ✅ 必须测试覆盖"
                    echo ""
                    echo "$must_test_items"
                    echo ""
                fi
                
                echo "**参考**：[ADR-$number 完整文档](./$tier/ADR-$number-*.md)"
                echo ""
                echo "---"
                echo ""
                
                has_content=true
            fi
        done < <(find "$tier_path" -name "ADR-*.md" | sort)
        
        if [ "$has_content" = false ]; then
            echo "*此层级暂无可裁决约束条款*"
            echo ""
            echo "---"
            echo ""
        fi
    done
    
    # 添加快速索引
    cat << 'EOF'
## 快速索引

### 按约束类型

#### 模块隔离相关
- ADR-001：模块禁止直接引用其他模块
- ADR-003：命名空间必须遵循项目结构

#### 依赖管理相关
- ADR-002：Platform 禁止依赖 Application/Host
- ADR-004：所有依赖版本必须在 Directory.Packages.props 中管理

#### 代码组织相关
- ADR-005：Command Handler 禁止返回业务数据
- ADR-120：领域事件命名必须遵循过去式规范

### 按检测方式

#### 自动化测试覆盖
查看各 ADR 文档中标记【必须架构测试覆盖】的条款

#### 代码审查检测
需要人工审核的场景请参考各 ADR 的"人工审核门控"章节

#### CI/CD 集成
所有架构测试在 CI 中自动执行

---

## 常见违规场景

### 1. 跨模块直接引用

**错误示例**：
```csharp
using Zss.BilliardHall.Modules.Orders.Domain;
```

**正确做法**：通过领域事件、数据契约或原始类型通信

**相关 ADR**：ADR-001

### 2. Platform 依赖业务层

**错误示例**：
```xml
<ProjectReference Include="../Modules.Orders/Orders.csproj" />
```

**正确做法**：Platform 只能被依赖，不能依赖业务模块

**相关 ADR**：ADR-002

### 3. Command Handler 返回业务数据

**错误示例**：
```csharp
public OrderDto Handle(CreateOrder command) { ... }
```

**正确做法**：Command Handler 只返回 void 或 ID

**相关 ADR**：ADR-005

---

## 申请例外流程

当确实需要违反某个约束时：

1. 在 PR 标题添加 `[ARCH-VIOLATION]`
2. 填写 PR 模板中的"架构破例详情"
3. 说明违反的 ADR 和具体条款
4. 提供技术理由和归还计划
5. 获得架构委员会批准

**参考**：[ADR-900 流程规范](./governance/ADR-900-adr-process.md)

---

## 附录

### 工具链

- `validate-adr-consistency.sh` - 一致性检查
- `validate-three-way-mapping.sh` - 三位一体映射验证
- `adr-cli.sh` - ADR 管理工具

### 相关文档

- [ADR 目录](./README.md)
- [Copilot 治理体系](../copilot/README.md)
- [架构测试指南](../TESTING-GUIDE.md)

---

**维护**：此文档由 `generate-quick-reference.sh` 自动生成  
**更新频率**：每次 ADR 变更后重新生成
EOF
}

# 提取 MUST/MUST NOT 约束
function extract_must_constraints() {
    local file="$1"
    local temp_file=$(mktemp)
    
    # 移除代码块
    awk '/^```/ { in_code_block = !in_code_block; next } !in_code_block { print }' "$file" > "$temp_file"
    
    # 提取包含 MUST/禁止/必须 的行
    grep -E "必须\*\*|禁止\*\*|MUST\*\*|MUST NOT" "$temp_file" 2>/dev/null | \
        grep -v "^#" | \
        sed 's/^[*-]\s*/- /' | \
        head -n 10
    
    rm -f "$temp_file"
}

# 提取 SHOULD 约束
function extract_should_constraints() {
    local file="$1"
    local temp_file=$(mktemp)
    
    awk '/^```/ { in_code_block = !in_code_block; next } !in_code_block { print }' "$file" > "$temp_file"
    
    grep -E "应当\*\*|建议\*\*|SHOULD" "$temp_file" 2>/dev/null | \
        grep -v "^#" | \
        sed 's/^[*-]\s*/- /' | \
        head -n 5
    
    rm -f "$temp_file"
}

# 提取必须测试覆盖的条款
function extract_must_test_items() {
    local file="$1"
    local temp_file=$(mktemp)
    
    awk '/^```/ { in_code_block = !in_code_block; next } !in_code_block { print }' "$file" > "$temp_file"
    
    grep -B 1 "【必须架构测试覆盖】\|【必须测试】\|\[MUST_TEST\]" "$temp_file" 2>/dev/null | \
        grep -v "^--$" | \
        grep -v "【必须" | \
        sed 's/^[*-]\s*/- /' | \
        head -n 10
    
    rm -f "$temp_file"
}

# 主执行函数
function main() {
    log_info "生成 ADR 可裁决性速查手册..."
    
    if [ -n "$OUTPUT_FILE" ]; then
        generate_quick_reference > "$OUTPUT_FILE"
        log_success "速查手册生成完成：$OUTPUT_FILE"
        echo ""
        echo "查看手册："
        echo "  cat $OUTPUT_FILE"
    else
        generate_quick_reference
    fi
}

# 执行
main
