#!/bin/bash
# 快速烟雾测试 - 无需 Docker，适合本地快速验证
# Quick smoke tests - No Docker required, suitable for fast local validation

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "=========================================="
echo "🚀 运行烟雾测试 (Layer 1: Unit Tests)"
echo "🚀 Running Smoke Tests (Layer 1: Unit Tests)"
echo "=========================================="
echo ""

cd "$PROJECT_ROOT"

# 检查 .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "❌ 错误：未找到 dotnet 命令"
    echo "❌ Error: dotnet command not found"
    echo "请安装 .NET SDK: https://dotnet.microsoft.com/download"
    exit 1
fi

echo "✓ .NET SDK 版本："
dotnet --version
echo ""

# 运行 ServiceDefaults 测试
echo "📦 [1/2] 运行 ServiceDefaults 单元测试..."
echo "📦 [1/2] Running ServiceDefaults unit tests..."
dotnet test \
    Aspire/Zss.BilliardHall.Wolverine.ServiceDefaults.Tests/Zss.BilliardHall.Wolverine.ServiceDefaults.Tests.csproj \
    -c Release \
    --logger "console;verbosity=normal" \
    --nologo

echo ""

# 运行 Bootstrapper 烟雾测试
echo "🔥 [2/2] 运行 Bootstrapper 烟雾测试..."
echo "🔥 [2/2] Running Bootstrapper smoke tests..."
dotnet test \
    Bootstrapper.Tests/Zss.BilliardHall.Wolverine.Bootstrapper.Tests.csproj \
    --filter "Category=Unit" \
    -c Release \
    --logger "console;verbosity=normal" \
    --nologo

echo ""
echo "=========================================="
echo "✅ 所有烟雾测试通过！"
echo "✅ All smoke tests passed!"
echo "=========================================="
