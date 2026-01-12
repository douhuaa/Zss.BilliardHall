#!/bin/bash
# 运行所有测试 - 包括烟雾测试和集成测试
# Run all tests - Including smoke tests and integration tests

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=========================================="
echo "🧪 运行所有测试 (All Tests)"
echo "=========================================="
echo ""

# 运行烟雾测试
echo "步骤 1/2: 烟雾测试"
echo "Step 1/2: Smoke Tests"
echo "------------------------------------------"
bash "$SCRIPT_DIR/test-smoke.sh"

echo ""
echo ""

# 运行集成测试
echo "步骤 2/2: 集成测试"
echo "Step 2/2: Integration Tests"
echo "------------------------------------------"
bash "$SCRIPT_DIR/test-integration.sh"

echo ""
echo "=========================================="
echo "🎉 所有测试完成！"
echo "🎉 All tests completed!"
echo "=========================================="
