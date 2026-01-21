# CI/CD 与架构测试集成指南

## 概述

架构测试是构建流程的一部分，必须通过才能合并代码。这确保架构约束在工程层面得到强制执行。

**完整的三层自动化防御体系和执行指南请参阅：[架构自动化验证系统](architecture-automation-verification.md)**

## CI 流程

### 标准构建流程

```yaml
# GitHub Actions 示例
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Run Architecture Tests
      run: dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj --configuration Release --no-build
      env:
        Configuration: Release
    
    - name: Run Unit Tests
      run: dotnet test --filter "FullyQualifiedName!~ArchitectureTests" --configuration Release --no-build
```

### 关键点

1. **架构测试优先运行**
   - 在其他测试之前运行架构测试
   - 如果架构测试失败，立即终止构建

2. **配置环境变量**
   ```bash
   Configuration=Release
   ```
   确保加载正确的程序集

3. **失败即阻断**
   - 架构测试失败 = 构建失败
   - PR 无法合并

## 本地开发流程

### 提交前检查

开发者应该在提交代码前运行架构测试：

```bash
# 1. 构建项目
dotnet build

# 2. 运行架构测试
dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj

# 3. 如果测试通过，再运行其他测试
dotnet test
```

### 配置 Git Hooks（推荐）

创建 `.git/hooks/pre-push` 文件：

```bash
#!/bin/sh

echo "Running architecture tests before push..."

dotnet build
if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj
if [ $? -ne 0 ]; then
    echo "❌ Architecture tests failed. Push aborted."
    echo "Please fix architecture violations before pushing."
    exit 1
fi

echo "✅ Architecture tests passed"
exit 0
```

使文件可执行：
```bash
chmod +x .git/hooks/pre-push
```

## 处理架构违规

### 违规类型

架构测试可能因以下原因失败：

1. **模块隔离违规**
   - 模块之间直接相互引用
   - 模块包含传统分层命名空间

2. **契约使用违规**
   - Command Handler 依赖 IQuery 接口
   - Platform 依赖业务契约

3. **垂直切片违规**
   - 创建了横向 Service
   - 使用了 Shared/Common 文件夹
   - Handler 之间直接调用

4. **Platform 层违规**
   - Platform 包含业务逻辑
   - Platform 依赖业务模块

### 修复步骤

1. **阅读测试失败信息**
   ```
   模块 Orders 不应依赖模块 Members。
   修复建议：将共享逻辑移至 Platform/BuildingBlocks，
   或改为消息通信（Publish/Invoke）。
   ```

2. **理解违规原因**
   - 为什么这条规则存在？
   - 参考 [ADR-0001](/docs/adr/ADR-0001-modular-monolith-vertical-slice-architecture.md)

3. **应用修复方案**
   - 按照测试建议进行修改
   - 参考 [架构指南](/docs/architecture-guide.md)

4. **验证修复**
   ```bash
   dotnet build
   dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj
   ```

### 豁免机制（谨慎使用）

在极特殊情况下，如果违规无法立即修复：

1. **创建 ADR** 记录豁免原因
   - 为什么违规？
   - 为什么不能立即修复？
   - 计划何时修复？

2. **PR 中标注**
   ```markdown
   ## ⚠️ ARCH-VIOLATION
   
   本 PR 包含架构违规豁免：
   - 违规项：Orders 模块临时依赖 Members 模块
   - ADR: ADR-0002-temporary-coupling-exemption.md
   - 偿还计划：将在 Q2 重构时修复
   - 责任人：@developer
   ```

3. **设置提醒**
   - 在项目看板创建技术债任务
   - 设置到期日期

## 监控与报告

### 架构健康度指标

定期监控以下指标：

1. **架构测试通过率**
   - 目标：100%
   - 当前豁免数量

2. **模块耦合度**
   - 跨模块依赖数量（应为 0）
   - 领域事件数量（正常增长）

3. **垂直切片遵守度**
   - Service 类数量（应为 0）
   - 切片平均大小

### 生成架构报告

```bash
# 运行架构测试并生成详细报告
dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj \
    --logger "trx;LogFileName=architecture-test-results.trx" \
    --logger "html;LogFileName=architecture-test-results.html"
```

## 持续改进

### 定期评审

建议每季度进行架构评审：

1. **评审豁免项**
   - 是否已偿还？
   - 是否需要延期？

2. **评审架构测试**
   - 是否需要新的测试？
   - 是否有测试过于严格？

3. **更新 ADR**
   - 记录架构演进
   - 更新指南文档

### 团队培训

1. **新人入职**
   - 阅读架构指南
   - 理解 ADR
   - 运行架构测试

2. **定期分享**
   - 架构违规案例分析
   - 最佳实践分享

## 故障排除

### 常见问题

**Q: 架构测试在本地通过，但 CI 失败？**

A: 检查以下几点：
- CI 环境的配置（Debug vs Release）
- 模块程序集是否正确构建
- 环境变量设置

**Q: 如何临时禁用某个架构测试？**

A: 不建议禁用。如果必须：
1. 在测试方法上添加 `[Fact(Skip = "Reason")]`
2. 创建 ADR 记录原因
3. 设置恢复时间

**Q: 架构测试运行很慢？**

A: 优化建议：
- 并行运行测试（xUnit 默认支持）
- 缓存构建产物
- 只在 PR 时运行完整测试套件

## 参考

- [Architecture Tests README](/src/tests/ArchitectureTests/README.md)
- [ADR-0001](/docs/adr/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [Architecture Guide](/docs/architecture-guide.md)
