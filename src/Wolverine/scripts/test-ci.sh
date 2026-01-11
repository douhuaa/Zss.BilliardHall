#!/bin/bash
# CI 测试脚本 - 模拟 CI 环境的测试运行
# CI test script - Simulates test execution in CI environment

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "=========================================="
echo "🤖 模拟 CI 测试环境"
echo "🤖 Simulating CI Test Environment"
echo "=========================================="
echo ""

cd "$PROJECT_ROOT"

# 检查 .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "❌ 错误：未找到 dotnet 命令"
    echo "❌ Error: dotnet command not found"
    exit 1
fi

echo "环境信息 / Environment Info:"
echo "  .NET SDK: $(dotnet --version)"
echo "  OS: $(uname -s)"
echo ""

# 恢复依赖
echo "📦 [1/4] 恢复依赖..."
echo "📦 [1/4] Restoring dependencies..."
dotnet restore Zss.BilliardHall.Wolverine.slnx --nologo
echo ""

# 检查代码格式
echo "📝 [2/4] 检查代码格式..."
echo "📝 [2/4] Checking code formatting..."
FORMAT_OUTPUT=$(dotnet format Zss.BilliardHall.Wolverine.slnx --verify-no-changes --verbosity quiet --nologo 2>&1)
FORMAT_EXIT_CODE=$?
if [ $FORMAT_EXIT_CODE -eq 0 ]; then
    echo "✓ 代码格式正确"
    echo "✓ Code formatting is correct"
else
    echo "⚠️  警告：代码格式检查失败（非阻塞）"
    echo "⚠️  Warning: Code formatting check failed (non-blocking)"
    if [ -n "$FORMAT_OUTPUT" ]; then
        echo "格式问题详情 / Formatting issues:"
        echo "$FORMAT_OUTPUT"
    fi
fi
echo ""

# 构建
echo "🔨 [3/4] 构建项目..."
echo "🔨 [3/4] Building projects..."
dotnet build Zss.BilliardHall.Wolverine.slnx -c Release --no-restore --nologo
echo ""

# 运行快速测试（Layer 1）
echo "🧪 [4/4] 运行快速测试（仅 Layer 1）..."
echo "🧪 [4/4] Running fast tests (Layer 1 only)..."
echo ""

# ServiceDefaults 测试
echo "  → ServiceDefaults.Tests"
dotnet test \
    Aspire/Zss.BilliardHall.Wolverine.ServiceDefaults.Tests/Zss.BilliardHall.Wolverine.ServiceDefaults.Tests.csproj \
    -c Release \
    --no-build \
    --logger "console;verbosity=minimal" \
    --nologo

# Bootstrapper 烟雾测试
echo "  → Bootstrapper.Tests (Smoke)"
dotnet test \
    Bootstrapper.Tests/Zss.BilliardHall.Wolverine.Bootstrapper.Tests.csproj \
    --filter "Category=Unit" \
    -c Release \
    --no-build \
    --logger "console;verbosity=minimal" \
    --nologo

echo ""
echo "=========================================="
echo "✅ CI 测试通过！"
echo "✅ CI tests passed!"
echo ""
echo "注意：AppHost E2E 测试（Layer 3）在 CI 中默认跳过"
echo "Note: AppHost E2E tests (Layer 3) are skipped by default in CI"
echo "=========================================="
