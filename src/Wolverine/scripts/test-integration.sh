#!/bin/bash
# 集成测试 - 需要 Docker，使用 Testcontainers
# Integration tests - Requires Docker, uses Testcontainers

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "=========================================="
echo "🐳 运行集成测试 (Layer 2: Integration Tests)"
echo "🐳 Running Integration Tests (Layer 2: Integration Tests)"
echo "=========================================="
echo ""

cd "$PROJECT_ROOT"

# 检查 Docker
if ! command -v docker &> /dev/null; then
    echo "❌ 错误：未找到 docker 命令"
    echo "❌ Error: docker command not found"
    echo "请安装 Docker: https://www.docker.com/get-started"
    exit 1
fi

# 检查 Docker 是否运行
if ! docker info > /dev/null 2>&1; then
    echo "❌ 错误：Docker 未运行"
    echo "❌ Error: Docker is not running"
    echo "请启动 Docker Desktop 或 Docker daemon"
    echo "Please start Docker Desktop or Docker daemon"
    exit 1
fi

echo "✓ Docker 版本："
docker --version
echo ""

# 检查 .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "❌ 错误：未找到 dotnet 命令"
    echo "❌ Error: dotnet command not found"
    exit 1
fi

echo "✓ .NET SDK 版本："
dotnet --version
echo ""

# 运行 Bootstrapper 集成测试
echo "🔬 运行 Bootstrapper 集成测试（使用 Testcontainers）..."
echo "🔬 Running Bootstrapper integration tests (with Testcontainers)..."
echo "⏳ 首次运行可能需要下载 PostgreSQL 镜像..."
echo "⏳ First run may take time to download PostgreSQL image..."
echo ""

dotnet test \
    Bootstrapper.Tests/Zss.BilliardHall.Wolverine.Bootstrapper.Tests.csproj \
    --filter "Category=Integration" \
    -c Release \
    --logger "console;verbosity=normal" \
    --nologo

echo ""
echo "=========================================="
echo "✅ 所有集成测试通过！"
echo "✅ All integration tests passed!"
echo "=========================================="
