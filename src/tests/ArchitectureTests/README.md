# ArchitectureTests（架构自动化测试）

## 目的

这组测试的目的是把 **ADR-0001 至 ADR-0005 的核心静态约束** 写成可执行规则，确保架构规范能够被自动化检查并在 CI 中执行：

- **Platform 层约束**：确保 Platform 不依赖 Application；Application 不依赖 Host
- **模块隔离**：防止模块之间互相引用导致耦合爆炸
- **命名空间规范**：确保所有类型命名空间以 Zss.BilliardHall 开头
- **依赖方向**：明确 Modules 只能依赖 Platform（而不是 Host / Application）
- **中央包管理**：检查 Directory.Packages.props 在仓库根目录存在

> 这些测试要做到两件事：**有杀伤力**（能抓到违规）+ **低维护**（不依赖 bin 固定路径、不依赖开发机目录）。

---

## 本地运行

在解决方案根目录运行：

```bash
# 先编译（保证模块程序集存在）
dotnet build

# 再跑架构测试
dotnet test src/tests/ArchitectureTests
```

如需指定配置（CI 使用 Release）：

```bash
dotnet build -c Release
export Configuration=Release
dotnet test src/tests/ArchitectureTests -c Release
```

## CI 集成

架构测试已集成到 GitHub Actions 工作流中（`.github/workflows/architecture-tests.yml`），在以下情况自动运行：

- Push 到 `main` 分支
- 针对 `main` 分支的 Pull Request

如果架构测试失败，CI 将阻断合并，确保架构规范得到严格执行。

## 测试清单

### 1. PlatformDependencyTests.cs
验证平台层和应用层的依赖约束：
- Platform 不依赖 Application
- Application 不依赖 Host
- Platform 不依赖 Host
- Platform 不依赖 Modules
- Application 不依赖 Modules

### 2. ModuleIsolationTests.cs
验证模块隔离规则：
- 模块不相互依赖
- 模块不包含传统分层命名空间（Application/Domain/Infrastructure 等）
- 模块不包含 Repository/Service/Manager 等语义
- 模块只依赖 Platform

### 3. NamespaceTests.cs
验证命名空间规范：
- 所有类型命名空间以 Zss.BilliardHall 开头
- Platform 类型在 Zss.BilliardHall.Platform 命名空间
- Application 类型在 Zss.BilliardHall.Application 命名空间
- Module 类型在 Zss.BilliardHall.Modules.{ModuleName} 命名空间
- Directory.Packages.props 存在于仓库根目录
- Directory.Build.props 存在于仓库根目录

## 常见问题

### 问题：测试失败提示"未找到模块程序集"

**原因**：模块尚未构建，测试无法加载 DLL。

**解决方法**：
```bash
dotnet build
dotnet test src/tests/ArchitectureTests
```

### 问题：本地通过但 CI 失败

**原因**：本地使用 Debug 配置，CI 使用 Release 配置。

**解决方法**：
```bash
dotnet build -c Release
export Configuration=Release
dotnet test src/tests/ArchitectureTests -c Release
```

### 问题：架构测试报告违规

**处理步骤**：
1. 查看测试输出，了解违规类型和位置
2. 根据修复建议调整代码结构
3. 重新运行测试验证修复

## 扩展建议

### F. 基础设施约束（Infrastructure）
- 仓库根目录必须存在 `Directory.Packages.props`（CPM 启用）
- 所有类型的命名空间必须以 `Zss.BilliardHall` 开头（RootNamespace 约定）
- 项目命名必须遵循 `Zss.BilliardHall.*` 约定

---

## CI 集成

架构测试已集成到 GitHub Actions CI 流程中（`.github/workflows/architecture-tests.yml`）：

- **触发条件**：Push 或 Pull Request 到 main 分支
- **测试环境**：Ubuntu Latest + .NET 10.0.x
- **失败阻断**：如果任何架构测试失败，CI 将返回非 0 退出码，阻止 PR 合并

CI 工作流程：
1. Checkout 代码
2. 设置 .NET SDK
3. 恢复依赖 (`dotnet restore`)
4. 构建所有项目（排除 docs）
5. 运行架构测试 (`dotnet test`)

---

后续可以增强架构测试：

1. **引入 Roslyn Analyzer**：做语义级别的静态检查
2. **添加更多规则**：如异步方法命名约定、异常处理规范等
3. **格式化失败信息**：在 PR 模板中强制 ARCH-VIOLATION 字段
4. **性能测试**：确保架构测试运行时间控制在合理范围内

---

**注意**：这是一个 MVP 实现，用以把 ADR 的静态规则尽快纳入 CI，阻断大多数常见的架构违规。
