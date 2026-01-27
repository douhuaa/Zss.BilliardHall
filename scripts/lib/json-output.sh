#!/bin/bash
# JSON 输出工具库
# 依据 ADR-970.2 实现标准化日志格式
#
# 用法：
#   source scripts/lib/json-output.sh
#   json_start "tool-name" "tool-version" "test-type"
#   json_add_detail "test-name" "ADR-XXXX" "severity" "message" "file" line "fix-guide"
#   json_finalize "success|failure|warning"

# 全局变量
declare -a JSON_DETAILS=()
JSON_TYPE=""
JSON_SOURCE=""
JSON_VERSION=""
JSON_TOTAL=0
JSON_PASSED=0
JSON_FAILED=0
JSON_WARNINGS=0

# 获取当前 ISO 8601 时间戳
function json_timestamp() {
    date -u +"%Y-%m-%dT%H:%M:%SZ"
}

# 初始化 JSON 报告
# 参数：source, version, type
function json_start() {
    JSON_SOURCE="$1"
    JSON_VERSION="${2:-1.0.0}"
    JSON_TYPE="${3:-validation}"
    JSON_DETAILS=()
    JSON_TOTAL=0
    JSON_PASSED=0
    JSON_FAILED=0
    JSON_WARNINGS=0
}

# 添加详细信息条目
# 参数：test, adr, severity, message, [file], [line], [fix_guide]
function json_add_detail() {
    local test="$1"
    local adr="$2"
    local severity="$3"
    local message="$4"
    local file="${5:-}"
    local line="${6:-}"
    local fix_guide="${7:-}"
    
    JSON_TOTAL=$((JSON_TOTAL + 1))
    
    case "$severity" in
        error)
            JSON_FAILED=$((JSON_FAILED + 1))
            ;;
        warning)
            JSON_WARNINGS=$((JSON_WARNINGS + 1))
            ;;
        info)
            JSON_PASSED=$((JSON_PASSED + 1))
            ;;
    esac
    
    # 转义 JSON 字符串
    message=$(echo "$message" | sed 's/"/\\"/g' | sed 's/\\/\\\\/g')
    test=$(echo "$test" | sed 's/"/\\"/g')
    
    # 构建 JSON 对象
    local detail="{"
    detail+="\"test\":\"$test\","
    
    if [ -n "$adr" ]; then
        detail+="\"adr\":\"$adr\","
    fi
    
    detail+="\"severity\":\"$severity\","
    detail+="\"message\":\"$message\""
    
    if [ -n "$file" ]; then
        detail+=",\"file\":\"$file\""
    fi
    
    if [ -n "$line" ]; then
        detail+=",\"line\":$line"
    fi
    
    if [ -n "$fix_guide" ]; then
        detail+=",\"fix_guide\":\"$fix_guide\""
    fi
    
    detail+="}"
    
    JSON_DETAILS+=("$detail")
}

# 完成并输出 JSON 报告
# 参数：status, [metadata_json]
function json_finalize() {
    local status="$1"
    local metadata="${2:-}"
    
    local timestamp=$(json_timestamp)
    
    # 获取 Git 元数据
    local branch=""
    local commit=""
    local author=""
    
    if git rev-parse --git-dir > /dev/null 2>&1; then
        branch=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")
        commit=$(git rev-parse --short HEAD 2>/dev/null || echo "unknown")
        author=$(git config user.name 2>/dev/null || echo "unknown")
    fi
    
    # 开始 JSON 输出
    echo "{"
    echo "  \"type\": \"$JSON_TYPE\","
    echo "  \"timestamp\": \"$timestamp\","
    echo "  \"source\": \"$JSON_SOURCE\","
    echo "  \"version\": \"$JSON_VERSION\","
    echo "  \"status\": \"$status\","
    echo "  \"summary\": {"
    echo "    \"total\": $JSON_TOTAL,"
    echo "    \"passed\": $JSON_PASSED,"
    echo "    \"failed\": $JSON_FAILED,"
    echo "    \"warnings\": $JSON_WARNINGS"
    echo "  },"
    echo "  \"details\": ["
    
    # 输出所有详细信息
    local detail_count=${#JSON_DETAILS[@]}
    for ((i=0; i<detail_count; i++)); do
        echo "    ${JSON_DETAILS[$i]}"
        if [ $i -lt $((detail_count - 1)) ]; then
            echo ","
        fi
    done
    
    echo "  ],"
    
    # 添加元数据
    if [ -n "$metadata" ]; then
        echo "  \"metadata\": $metadata"
    else
        echo "  \"metadata\": {"
        echo "    \"branch\": \"$branch\","
        echo "    \"commit\": \"$commit\","
        echo "    \"author\": \"$author\""
        echo "  }"
    fi
    
    echo "}"
}

# 保存 JSON 报告到文件
# 参数：status, output_path, [metadata_json]
function json_save() {
    local status="$1"
    local output_path="$2"
    local metadata="${3:-}"
    
    # 确保目录存在
    mkdir -p "$(dirname "$output_path")"
    
    # 输出到文件
    json_finalize "$status" "$metadata" > "$output_path"
    
    # 如果是 latest.json 链接，更新它
    local dir=$(dirname "$output_path")
    local latest="$dir/latest.json"
    if [[ "$output_path" != "$latest" ]]; then
        ln -sf "$(basename "$output_path")" "$latest" 2>/dev/null || true
    fi
}

# 辅助函数：根据失败数量确定状态
function json_determine_status() {
    if [ $JSON_FAILED -gt 0 ]; then
        echo "failure"
    elif [ $JSON_WARNINGS -gt 0 ]; then
        echo "warning"
    else
        echo "success"
    fi
}
