# 测试脚本 / Test Scripts

本目录包含用于运行不同层级测试的 Bash 脚本。

This directory contains Bash scripts for running different layers of tests.

## 📋 可用脚本 / Available Scripts

### 1. `test-smoke.sh` - 快速烟雾测试 🚀

**用途**: 运行快速单元测试，无需 Docker，适合本地开发快速验证。

**Usage**: Run fast unit tests without Docker, suitable for quick local validation.

**包含测试**:
- ServiceDefaults.Tests (19 个测试)
- Bootstrapper.Tests - 烟雾测试 (6 个测试)

**执行时间**: < 2 秒

**运行方式**:
```bash
cd src/Wolverine/scripts
./test-smoke.sh
```

### 2. `test-integration.sh` - 集成测试 🐳

**用途**: 运行需要 Docker 的集成测试，使用 Testcontainers 启动 PostgreSQL。

**Usage**: Run integration tests that require Docker, using Testcontainers for PostgreSQL.

**包含测试**:
- Bootstrapper.Tests - 集成测试 (5 个测试)

**前置条件**:
- ✅ Docker 已安装并运行
- ✅ 首次运行会下载 PostgreSQL 镜像

**执行时间**: 5-30 秒（首次更长）

**运行方式**:
```bash
cd src/Wolverine/scripts
./test-integration.sh
```

### 3. `test-all.sh` - 所有测试 🧪

**用途**: 依次运行烟雾测试和集成测试。

**Usage**: Run smoke tests followed by integration tests.

**包含测试**:
- 所有烟雾测试 (25 个)
- 所有集成测试 (5 个)

**前置条件**: Docker 已安装并运行

**执行时间**: < 35 秒

**运行方式**:
```bash
cd src/Wolverine/scripts
./test-all.sh
```

### 4. `test-ci.sh` - CI 模拟测试 🤖

**用途**: 模拟 CI 环境的完整测试流程，包括依赖恢复、代码格式检查、构建和测试。

**Usage**: Simulate complete CI testing workflow including restore, format check, build, and tests.

**执行步骤**:
1. 恢复依赖 (`dotnet restore`)
2. 检查代码格式 (`dotnet format --verify-no-changes`)
3. 构建项目 (`dotnet build`)
4. 运行快速测试（Layer 1）

**运行方式**:
```bash
cd src/Wolverine/scripts
./test-ci.sh
```

## 🎯 使用场景 / Use Cases

### 本地开发时快速验证 / Quick validation during local development
```bash
./test-smoke.sh
```

### 提交前完整验证 / Full validation before commit
```bash
./test-all.sh
```

### 模拟 CI 环境测试 / Simulate CI environment
```bash
./test-ci.sh
```

### 验证 Marten 集成 / Verify Marten integration
```bash
./test-integration.sh
```

## 📊 测试层级 / Test Layers

```
Layer 1: 单元测试 (Unit Tests)
├── 无需外部依赖 / No external dependencies
├── 快速执行 < 2 秒 / Fast execution < 2s
└── 脚本：test-smoke.sh

Layer 2: 集成测试 (Integration Tests)
├── 需要 Docker / Requires Docker
├── Testcontainers PostgreSQL
└── 脚本：test-integration.sh

Layer 3: E2E 测试 (End-to-End Tests)
├── 需要 Aspire DCP / Requires Aspire DCP
├── 手动触发 / Manual trigger
└── 位置：../Aspire/Zss.BilliardHall.Wolverine.AppHost.Tests
```

## 🛠️ 故障排查 / Troubleshooting

### 错误：未找到 dotnet 命令 / Error: dotnet command not found
**解决**: 安装 .NET SDK 10.0+
- https://dotnet.microsoft.com/download

### 错误：Docker 未运行 / Error: Docker is not running
**解决**: 启动 Docker Desktop 或 Docker daemon
```bash
# 检查 Docker 状态
docker info
```

### 集成测试失败：容器无法启动 / Integration tests fail: Container cannot start
**可能原因**:
1. Docker 资源不足（内存/磁盘）
2. 端口冲突
3. 网络配置问题

**解决**:
```bash
# 清理 Docker 资源
docker container prune -f
docker volume prune -f

# 重启 Docker
# macOS/Windows: 重启 Docker Desktop
# Linux: sudo systemctl restart docker
```

### 脚本权限错误 / Script permission error
```bash
chmod +x *.sh
```

## 📚 相关文档 / Related Documentation

- [测试入口说明](../测试入口说明.md) - 完整测试架构说明
- [Bootstrapper.Tests README](../Bootstrapper.Tests/README.md) - 测试项目详细文档
- [AppHost.Tests README](../Aspire/Zss.BilliardHall.Wolverine.AppHost.Tests/README.md) - E2E 测试说明

## 🔄 CI/CD 集成 / CI/CD Integration

这些脚本与 GitHub Actions workflow (`.github/workflows/ci.yml`) 保持一致。

These scripts are aligned with GitHub Actions workflow (`.github/workflows/ci.yml`).

**CI 运行的测试**:
- PR 检查：`test-smoke.sh` 的逻辑（Layer 1）
- 手动触发：可选 AppHost E2E 测试（Layer 3）

---

**版本**: 1.0.0  
**更新日期**: 2026-01-11  
**维护者**: Wolverine Team
