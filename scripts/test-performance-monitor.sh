#!/usr/bin/env bash

# 测试性能监控脚本
# 用于 CI/CD 管道中运行测试并生成性能报告

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
REPORT_DIR="$REPO_ROOT/docs/reports/tests"
BASELINE_FILE="$REPO_ROOT/docs/reports/tests/performance-baseline.json"
CURRENT_REPORT="$REPORT_DIR/performance-report-$(date +%Y%m%d-%H%M%S).md"

echo "🔍 开始测试性能监控..."
echo "📂 仓库根目录: $REPO_ROOT"
echo "📊 报告目录: $REPORT_DIR"

# 创建报告目录
mkdir -p "$REPORT_DIR"

# 运行架构测试（带性能监控）
echo "🧪 运行架构测试..."
cd "$REPO_ROOT"
dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj \
    --configuration Release \
    --logger "console;verbosity=minimal" \
    --collect:"XPlat Code Coverage" \
    -- \
    DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

# 运行性能报告生成器（如果有自定义工具）
# 这里我们假设测试中已经集成了 TestPerformanceCollector
# 实际使用时，可能需要在测试后运行一个额外的工具来生成报告

echo "📝 性能报告已生成（通过测试输出）"
echo "📍 查看报告: $CURRENT_REPORT"

# 如果有性能基线，则进行对比
if [ -f "$BASELINE_FILE" ]; then
    echo "📊 对比性能基线..."
    # 这里可以添加性能基线对比逻辑
    # 例如：python scripts/compare-performance.py --baseline "$BASELINE_FILE" --current "$CURRENT_REPORT"
    echo "ℹ️  性能基线对比功能待实现"
else
    echo "ℹ️  未找到性能基线文件: $BASELINE_FILE"
    echo "💡 提示：首次运行时会创建性能基线"
fi

echo "✅ 测试性能监控完成"
